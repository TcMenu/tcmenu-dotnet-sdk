using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace TcMenu.CoreSdk.MenuItems
{
    public enum MoveType { MOVE_UP, MOVE_DOWN }

    /// <summary>
    /// Menu tree holds all the menu items for a specific remote connection or session. It holds a hierarchy of
    /// items, where some items of type submenu can hold other items.
    /// </summary>
    public class MenuTree
    {
        /// <summary>
        /// The root item used when requiring access to the top level
        /// </summary>
        public static readonly SubMenuItem ROOT = new SubMenuItem("ROOT", "ROOT", 0, -1, null, false, false, false, true, false);
        private readonly Dictionary<SubMenuItem, List<MenuItem>> _menusBySubMenu = new Dictionary<SubMenuItem, List<MenuItem>>();
        private readonly ConcurrentDictionary<int, AnyMenuState> _stateById = new ConcurrentDictionary<int, AnyMenuState>();

        public MenuTree()
        {
            _menusBySubMenu.Add(ROOT, new List<MenuItem>());
        }

        /// <summary>
        /// Add a new menu item to a sub menu, for the top level menu use ROOT.
        /// </summary>
        /// <param name="parent">the parent where to add or ROOT</param>
        /// <param name="item">the item to add</param>
        public void AddMenuItem(SubMenuItem parent, MenuItem item)
        {
            SubMenuItem subMenu = parent ?? ROOT;

            lock(_menusBySubMenu) {
                List<MenuItem> subMenuChildren = _menusBySubMenu.ComputeIfAbsent(subMenu, () => new List<MenuItem>());
                subMenuChildren.Add(item);

                if (item.HasChildren())
                {
                    _menusBySubMenu.Add((SubMenuItem)item, new List<MenuItem>());
                }
            }
        }

        /// <summary>
        /// This will either add or replace an existing item depending on if the menu ID already exists.
        /// </summary>
        /// <param name="parentId">the ID of the parent</param>
        /// <param name="item">the item to add / update</param>
        public void AddOrUpdateItem(int parentId, MenuItem item)
        {
            lock(_menusBySubMenu) {
                if (GetMenuById(item.Id) != null)
                { 
                    ReplaceMenuById(GetSubMenuById(parentId), item);
                }
                else
                {
                    AddMenuItem(GetSubMenuById(parentId), item);
                }
            }
        }

        /// <summary>
        /// Remove an item from the tree using only the item. The parent will be looked up
        /// </summary>
        /// <param name="toRemove"></param>
        public void RemoveMenuItem(MenuItem toRemove)
        {
            lock(_menusBySubMenu) {
                RemoveMenuItem(FindParent(toRemove), toRemove);
            }
        }

        /// <summary>
        /// Remove the selected entry from the tree by it's parent and item
        /// </summary>
        /// <param name="parent">the parent of the item</param>
        /// <param name="item">the item</param>
        public void RemoveMenuItem(SubMenuItem parent, MenuItem item)
        {
            SubMenuItem subMenu = (parent != null) ? parent : ROOT;

            lock(_menusBySubMenu) {
                if (!_menusBySubMenu.ContainsKey(parent)) throw new InvalidOperationException("Menu element not found");
                List<MenuItem> subMenuChildren = _menusBySubMenu[subMenu];

                subMenuChildren.Remove(item);
                if (item is SubMenuItem sm)
                {
                    _menusBySubMenu.Remove(sm);
                }
            }
            _stateById.TryRemove(item.Id, out AnyMenuState oldVal);
        }

        /// <summary>
        /// Move an item up or down in the list of items within a submenu
        /// </summary>
        /// <param name="parent">the parent that contains this list of items</param>
        /// <param name="item">the item to be moved</param>
        /// <param name="moveType">up or down</param>
        public void MoveItem(SubMenuItem parent, MenuItem newItem, MoveType moveType)
        {
            lock(_menusBySubMenu) {
                List<MenuItem> items = _menusBySubMenu[parent];
                int idx = items.IndexOf(newItem);
                if (idx < 0) return;

                items.RemoveAt(idx);

                idx = (moveType == MoveType.MOVE_UP) ? --idx : ++idx;
                if (idx < 0) idx = 0;

                if (idx >= items.Count)
                {
                    items.Add(newItem);
                }
                else
                {
                    items.Insert(idx, newItem);
                }
            }
        }

        /// <summary>
        /// Replace the menu item with the one provided, the parent will be looked up.
        /// </summary>
        /// <param name="toReplace">the item to be replaced by ID</param>
        public void ReplaceMenuById(MenuItem toReplace)
        {
            lock(_menusBySubMenu) {
                ReplaceMenuById(FindParent(toReplace), toReplace);
            }
        }

        /// <summary>
        /// Replace the menu item that has a given parent with the one provided. This is an infrequent
        /// operation and not optimised.
        /// </summary>
        /// <param name="subMenu">the sub menu</param>
        /// <param name="toReplace">the item to replace</param>
        public void ReplaceMenuById(SubMenuItem subMenu, MenuItem toReplace)
        {
            lock(_menusBySubMenu) {
                List<MenuItem> list = _menusBySubMenu[subMenu];
                int idx = -1;
                for (int i = 0; i < list.Count; ++i)
                {
                    if (list[i].Id == toReplace.Id)
                    {
                        idx = i;
                    }
                }

                if (idx != -1)
                {
                    MenuItem oldItem = list[idx];
                    list[idx] = toReplace;
                    if (toReplace is SubMenuItem sub)
                    {
                        var oldSub = oldItem as SubMenuItem;
                        List<MenuItem> items = _menusBySubMenu[oldSub];
                        _menusBySubMenu.Remove(oldSub);
                        _menusBySubMenu[sub] = items;
                    }
                }
            }
        }

        /// <summary>
        /// gets a submenu by it's ID. Returns either a submenuitem or null.
        /// </summary>
        /// <param name="parentId">The ID of the submenu</param>
        /// <returns></returns>
        public SubMenuItem GetSubMenuById(int id)
        {
            lock (_menusBySubMenu) {
                var filtered = from sm in _menusBySubMenu
                               where sm.Key.Id == id
                               select sm.Key;
                return filtered.FirstOrDefault();
            }
        }

        /// <summary>
        /// Get a list of all menu items for a given submenu
        /// </summary>
        /// <param name="item">the sub menu to obtain or ROOT</param>
        /// <returns>all the item in that sub menu</returns>
        public ReadOnlyCollection<MenuItem> GetMenuItems(MenuItem item)
        {
            lock(_menusBySubMenu) {
                if (item is SubMenuItem sub)
                {
                    var menuItems = _menusBySubMenu[sub];
                    // we must make a deep copy before returning this, as otherwise it is unsafe to rely on it
                    // not changing.
                    return new ReadOnlyCollection<MenuItem>(new List<MenuItem>(menuItems));
                }
            }
            return null;
        }

        /// <summary>
        /// Returns all the submenus that are currently stored
        /// </summary>
        /// <returns>All sub menus</returns>
        public HashSet<SubMenuItem> GetAllSubMenus()
        {
            lock(_menusBySubMenu) {
                return new HashSet<SubMenuItem>(_menusBySubMenu.Keys);
            }
        }

        /// <summary>
        /// Get every single menu item that's stored in this tree
        /// </summary>
        /// <returns>every single item</returns>
        public HashSet<MenuItem> GetAllMenuItems()
        {
            var toReturn = new HashSet<MenuItem>();
            lock(_menusBySubMenu)
            {
                foreach (SubMenuItem sub in _menusBySubMenu.Keys)
                {
                    toReturn.Add(sub);
                    toReturn.UnionWith(GetMenuItems(sub));
                }
            }
            return toReturn;
        }

        /// <summary>
        /// Gets the menu item with the specified ID, finding the submenu if needed. In most cases the linkage between
        /// ID and item will be cached and therefore fast
        /// </summary>
        /// <param name="id">the id to be looked up</param>
        /// <returns>a menu item (or null)</returns>
        public MenuItem GetMenuById(int id)
        {
            if(_stateById.ContainsKey(id))
            {
                return _stateById[id].Item;
            }

            var possibleSubItem = GetSubMenuById(id);
            if (possibleSubItem != null) return possibleSubItem;

            return (from sm in GetAllMenuItems()
                    where (sm.Id == id)
                    select sm).FirstOrDefault();
        }


        /// <summary>
        /// Store a new state for a menu item
        /// </summary>
        /// <param name="item">the item for which the state should be stored</param>
        /// <param name="state">the state to store</param>
        public void ChangeItemState(MenuItem item, AnyMenuState state)
        {
            _stateById[item.Id] = state;
        }

        /// <summary>
        /// Gets the state associated with a given menu item
        /// </summary>
        /// <param name="menuItem">the item for which the state is needed</param>
        /// <returns>the state</returns>
        public AnyMenuState GetState(MenuItem menuItem)
        {
            _stateById.TryGetValue(menuItem.Id, out var state);
            return state ?? null;
        }

        /// <summary>
        /// Finds the parent item of the one passed in
        /// </summary>
        /// <param name="toFind">item for which to find parent</param>
        /// <returns>the parent item</returns>
        public SubMenuItem FindParent(MenuItem toFind)
        {
            lock(_menusBySubMenu) {
                SubMenuItem parent = MenuTree.ROOT;
                foreach(SubMenuItem subItem in _menusBySubMenu.Keys)
                {
                    foreach(MenuItem menuItem in _menusBySubMenu[subItem])
                    {
                        if (menuItem.Id == toFind.Id)
                        {
                            parent = subItem;
                        }
                    }
                }
                return parent;
            }
        }
    }

    public static class DictionaryHelper
    {
        /// <summary>
        /// Computes a value if one is not already in the map. Do not use on concurrent maps.
        /// </summary>
        /// <typeparam name="K">Key</typeparam>
        /// <typeparam name="V">Value</typeparam>
        /// <param name="dict">The dictionary</param>
        /// <param name="k">the key</param>
        /// <param name="toCompute">a function to compute a value</param>
        /// <returns></returns>
        public static V ComputeIfAbsent<K, V>(this Dictionary<K, V> dict, K k, Func<V> toCompute)
        {
            if (!dict.ContainsKey(k)) dict[k] = toCompute.Invoke();

            return dict[k];
        }
    }
}
