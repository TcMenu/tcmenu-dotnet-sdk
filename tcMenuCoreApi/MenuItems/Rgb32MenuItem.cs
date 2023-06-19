using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.Protocol;

namespace TcMenu.CoreSdk.MenuItems
{
    public class Rgb32MenuItem : MenuItem
    {
        public bool IncludeAlphaChannel { get; }

        public Rgb32MenuItem(string name, string varName, int id, int eepromAddress, string functionName, bool readOnly, bool localOnly,
            bool visible, bool includeAlpha, bool staticInRam) 
            : base(name, varName, id, eepromAddress, functionName, readOnly, localOnly, visible, staticInRam)
        {
            IncludeAlphaChannel = includeAlpha;
        }

        public override void Accept(MenuItemVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool Equals(object obj)
        {
            return obj is Rgb32MenuItem item &&
                   base.Equals(obj) &&
                   IncludeAlphaChannel == item.IncludeAlphaChannel;
        }

        public override int GetHashCode()
        {
            var hashCode = 1269443129;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + IncludeAlphaChannel.GetHashCode();
            return hashCode;
        }
    }

    public class Rgb32MenuItemBuilder : MenuItemBuilder<Rgb32MenuItemBuilder, Rgb32MenuItem>
    {
        private bool _includeAlpha = true;

        public Rgb32MenuItemBuilder WithIncludeAlphaChannel(bool alphaNeeded)
        {
            _includeAlpha = alphaNeeded;
            return this;
        }

        public override Rgb32MenuItemBuilder WithExisting(Rgb32MenuItem menuItem)
        {
            _includeAlpha = menuItem.IncludeAlphaChannel;
            return base.WithExisting(menuItem);
        }

        public override Rgb32MenuItem Build()
        {
            return new Rgb32MenuItem(Name, VariableName, Id, EepromAddress, FunctionName, ReadOnly, LocalOnly, Visible,
                _includeAlpha, StaticInRam);
        }

        protected override Rgb32MenuItemBuilder GetThis()
        {
            return this;
        }
    }
}