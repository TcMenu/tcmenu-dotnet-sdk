using System;
using System.Collections.Generic;
using System.Text;

namespace TcMenu.CoreSdk.MenuItems
{
    /// <summary>
    /// The base class for all menu items, has the most basic operations available on it that are needed by 
    /// pretty much all menu items.
    /// </summary>
    public abstract class MenuItem
    {
        public string Name { get; }
        public string VariableName { get;  }
        public int Id { get; }
        public int EepromAddress { get; }
        public string FunctionName { get; }
        public bool ReadOnly { get; }
        public bool LocalOnly { get; }
        public bool Visible { get; }
        public bool StaticInRAM { get; }

        protected MenuItem(string name, string variableName, int id, int eepromAddress, string functionName,
                        bool readOnly, bool localOnly, bool visible, bool staticInRAM)
        {
            Name = name;
            VariableName = variableName;
            Id = id;
            EepromAddress = eepromAddress;
            FunctionName = functionName;
            ReadOnly = readOnly;
            LocalOnly = localOnly;
            Visible = visible;
            StaticInRAM = staticInRAM;
        }

        /// <summary>
        /// Indicates if the item has children or not
        /// </summary>
        /// <returns></returns>
        public virtual bool HasChildren()
        {
            return false;
        }

        public abstract void Accept(MenuItemVisitor visitor);

        public override string ToString()
        {
            return $"{Name} Id({Id})";
        }

        public override bool Equals(object obj)
        {
            var item = obj as MenuItem;
            return item != null &&
                   Name == item.Name &&
                   Id == item.Id &&
                   EepromAddress == item.EepromAddress &&
                   VariableName== item.VariableName &&
                   FunctionName == item.FunctionName &&
                   ReadOnly == item.ReadOnly &&
                   Visible == item.Visible &&
                   LocalOnly == item.LocalOnly &&
                   StaticInRAM == item.StaticInRAM;
        }

        public override int GetHashCode()
        {
            var hashCode = 1014301000;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EepromAddress.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FunctionName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(VariableName);
            hashCode = hashCode * -1521134295 + ReadOnly.GetHashCode();
            hashCode = hashCode * -1521134295 + Visible.GetHashCode();
            hashCode = hashCode * -1521134295 + LocalOnly.GetHashCode();
            hashCode = hashCode * -1521134295 + StaticInRAM.GetHashCode();
            return hashCode;
        }
    }

    public interface IAnyMenuBuilder
    {
    }

    public abstract class MenuItemBuilder<T, B> : IAnyMenuBuilder where B: MenuItem
    {
        protected string Name;
        protected string VariableName = string.Empty;
        protected int Id;
        protected int EepromAddress = -1;
        protected string FunctionName = string.Empty;
        protected bool ReadOnly;
        protected bool LocalOnly;
        protected bool Visible = true;
        protected bool StaticInRam = false;

        /// <summary>
        /// Override in the leaf to return the appropriate class - better than casting.
        /// </summary>
        /// <returns></returns>
        protected abstract T GetThis();

        public abstract B Build();

        /// <summary>
        /// Set the name for a given menu item
        /// </summary>
        /// <param name="name">the name to be set</param>
        /// <returns>itself for chaining</returns>
        public T WithName(string name)
        {
            Name = name;
            return GetThis();
        }

        /// <summary>
        /// Set the visibility flag for this item
        /// </summary>
        /// <param name="vis">the visibility flag, default true</param>
        /// <returns></returns>
        public T WithVisible(bool vis)
        {
            Visible = vis;
            return GetThis();
        }

        /// <summary>
        /// Sets the ID parameter of a menu item
        /// </summary>
        /// <param name="id">The new ID</param>
        /// <returns>Itself for chaining</returns>
        public T WithId(int id)
        {
            Id = id;
            return GetThis();
        }

        /// <summary>
        /// Sets the readonly flag of the item
        /// </summary>
        /// <param name="read">true if read only</param>
        /// <returns>itself</returns>
        public T WithReadOnly(bool read)
        {
            ReadOnly = read;
            return GetThis();
        }

        /// <summary>
        /// Sets the local only status of the item
        /// </summary>
        /// <param name="local">true if local, false if remote allowed</param>
        /// <returns>itself</returns>
        public T WithLocalOnly(bool local)
        {
            LocalOnly = local;
            return GetThis();
        }

        /// <summary>
        /// Sets the eeprom location associated with the item
        /// </summary>
        /// <param name="location">the eeprom position</param>
        /// <returns>itself</returns>
        public T WithEepromLocation(int location)
        {
            EepromAddress = location;
            return GetThis();
        }

        /// <summary>
        /// Sets the function name to a new value
        /// </summary>
        /// <param name="name">the function name</param>
        /// <returns>itself</returns>
        public T WithFunctionName(string name)
        {
            FunctionName = name;
            return GetThis();
        }

        /// <summary>
        /// Sets the variable name for the menuitem, that will be used during menu generation. It will be the part
        /// at the end of variable name EG: a variable name of Xyz would become menuXyz and minfoXyz
        /// </summary>
        /// <param name="name">the variable name extension</param>
        /// <returns>itself</returns>
        public T WithVariableName(string name)
        {
            VariableName = name;
            return GetThis();
        }

        /// <summary>
        /// Used mainly in the designer but copied for completeness, signifies that the static data for the item is
        /// stored in RAM instead of FLASH.
        /// </summary>
        /// <param name="staticInRam"></param>
        /// <returns>true when static in RAM, otherwise false</returns>
        public T WithStaticInRam(bool staticInRam)
        {
            StaticInRam = staticInRam;
            return GetThis();
        }

        /// <summary>
        /// Copies all the fields from the supplied menu item into the builder.
        /// This will always be a deep copy.
        /// </summary>
        /// <param name="menuItem">the item to copy values from</param>
        /// <returns>itself</returns>
        public virtual T WithExisting(B menuItem)
        {
            Id = menuItem.Id;
            Name = menuItem.Name;
            VariableName = menuItem.VariableName;
            FunctionName = menuItem.FunctionName;
            EepromAddress = menuItem.EepromAddress;
            ReadOnly = menuItem.ReadOnly;
            LocalOnly = menuItem.LocalOnly;
            Visible = menuItem.Visible;
            StaticInRam = menuItem.StaticInRAM;
            return GetThis();
        }
    }
}
