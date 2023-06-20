using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml.Linq;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteCore;
using Font = Microsoft.Maui.Font;
using MenuItem = TcMenu.CoreSdk.MenuItems.MenuItem;

namespace TcMenuCoreMaui.Controls
{

    public interface IEditorComponent
    {
        View ViewItem { get; }
        int Id { get; }

        void OnItemUpdated(AnyMenuState newValue);

        void OnCorrelation(CorrelationId correlationId, AckStatus status);

        void Tick();
    }

    public delegate void MenuActionConsumer(MenuItem item);

    public interface IMenuEditorFactory
    {
        IEditorComponent CreateUpDown(MenuItem item, ComponentSettings settings);
        IEditorComponent CreateBooleanButton(MenuItem item, ComponentSettings settings);
        IEditorComponent CreateButtonWithAction(MenuItem item, string text, ComponentSettings settings, MenuActionConsumer actionConsumer);
        IEditorComponent CreateText<TVal>(MenuItem enumItem, ComponentSettings settings);
        IEditorComponent CreateList(MenuItem item, ComponentSettings settings, RuntimeListStringAdapter adapter);
        IEditorComponent CreateDate(MenuItem item, ComponentSettings settings);
        IEditorComponent CreateTime(MenuItem item, ComponentSettings settings);
        IEditorComponent CreateHorizontalSlider(MenuItem item, ComponentSettings settings);
        IEditorComponent CreateRgbColor(MenuItem item, ComponentSettings settings);
    }

    public class MauiMenuEditorFactory : IMenuEditorFactory
    {
        private readonly IRemoteController _controller;

        public MauiMenuEditorFactory(IRemoteController controller)
        {
            _controller = controller;
        }

        public IEditorComponent CreateUpDown(MenuItem item, ComponentSettings settings)
        {
            if (item is ScrollChoiceMenuItem)
            {
                return new ScrollUpDownEditorComponent(item, _controller, settings);
            }
            else if (item is AnalogMenuItem or EnumMenuItem)
            {
                return new IntegerUpDownEditorComponent(item, _controller, settings);
            }
            else throw new ArgumentException("Item cannot render as up/down " + item);
        }

        public IEditorComponent CreateBooleanButton(MenuItem item, ComponentSettings settings)
        {
            return new MauiBoolEditorComponent(_controller, settings, item);
        }

        public IEditorComponent CreateButtonWithAction(MenuItem item, string text, ComponentSettings settings,
            MenuActionConsumer actionConsumer)
        {
            return new MauiBoolEditorComponent(_controller, settings, item, actionConsumer);
        }

        public IEditorComponent CreateText<TVal>(MenuItem enumItem, ComponentSettings settings)
        {
            return new MauiTextEditorComponent<TVal>(_controller, settings, enumItem);
        }

        public IEditorComponent CreateList(MenuItem item, ComponentSettings settings, RuntimeListStringAdapter adapter)
        {
            return new ListEditorComponent(_controller, settings, item);
        }

        public IEditorComponent CreateDate(MenuItem item, ComponentSettings settings)
        {
            return new DateFieldEditorComponent(_controller, settings, item);
        }

        public IEditorComponent CreateTime(MenuItem item, ComponentSettings settings)
        {
            return new TimeFieldEditorComponent(_controller, settings, item);
        }

        public IEditorComponent CreateHorizontalSlider(MenuItem item, ComponentSettings settings)
        {
            return new IntegerUpDownEditorComponent(item, _controller, settings);
        }

        public IEditorComponent CreateRgbColor(MenuItem item, ComponentSettings settings)
        {
            return new RgbColorEditorComponent(item, _controller, settings);
        }
    }


}