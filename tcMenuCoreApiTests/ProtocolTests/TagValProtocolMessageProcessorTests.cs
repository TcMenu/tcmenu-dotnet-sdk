using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.Serialisation;

namespace TcMenu.CoreSdkTests.ProtocolTests
{
    [TestClass]
    public class TagValProtocolMessageProcessorTests
    {
        public const string TEST_UUID = "1c5d62aa-806b-4bf4-8118-fe305a868bd0";

        private DefaultProtocolCommandConverter protocolConverter;
        private TagValProtocolMessageProcessors processors;
        private MemoryStream stream;

        [TestInitialize]
        public void BeforeTesting()
        {
            protocolConverter = new DefaultProtocolCommandConverter();
            processors = new TagValProtocolMessageProcessors();
            processors.RegisterConverters(protocolConverter);
            stream = new MemoryStream(1024);
        }

        [TestMethod]
        public void TestHeartbeatMessageConversion()
        {
            var command = new HeartbeatCommand(HeartbeatMode.START, 10);
            SendAndAssertMsg(command, ProtocolId.TAG_VAL_PROTOCOL, HeartbeatCommand.HEARTBEAT_CMD_ID, "HI=10|HM=0|HR=1|");
            var newCommand = protocolConverter.ConvertMessageToCommand(stream) as HeartbeatCommand;
            Assert.AreEqual(10, newCommand?.Interval);
            Assert.AreEqual(0, newCommand?.Timestamp);
        }

        [TestMethod]
        public void TestNewJoinerMessageConversion()
        {
            var command = new NewJoinerCommand("superClient", TEST_UUID, 100, ApiPlatform.DNET_API, "987654321");
            SendAndAssertMsg(command, ProtocolId.TAG_VAL_PROTOCOL, NewJoinerCommand.NEW_JOINER_CMD_ID, $"NM=superClient|UU={TEST_UUID}|US=987654321|VE=100|PF=3|");
            var newCommand = protocolConverter.ConvertMessageToCommand(stream) as NewJoinerCommand;
            Assert.AreEqual("superClient", newCommand?.Name);
            Assert.AreEqual(TEST_UUID, newCommand?.Uuid);
            Assert.AreEqual((ushort)100, newCommand?.ApiVersion);
            Assert.AreEqual(ApiPlatform.DNET_API, newCommand?.ApiPlatform);
            Assert.AreEqual("987654321", newCommand?.SerialNumber);
        }

        [TestMethod]
        public void TestBootstrapMessageConversion()
        {
            var command = new BootstrapCommand(BootstrapType.START);
            SendAndAssertMsg(command, ProtocolId.TAG_VAL_PROTOCOL, BootstrapCommand.BOOTSTRAP_CMD_ID, "BT=START|");
            var newCommand = protocolConverter.ConvertMessageToCommand(stream) as BootstrapCommand;
            Assert.AreEqual(BootstrapType.START, newCommand?.BootType);
        }

        [TestMethod]
        public void TestPairingMessageConversion()
        {
            var command = new PairingCommand(TEST_UUID, "Name");
            SendAndAssertMsg(command, ProtocolId.TAG_VAL_PROTOCOL, PairingCommand.PAIRING_CMD_ID, "NM=Name|UU=" + TEST_UUID + "|");
            var newCommand = protocolConverter.ConvertMessageToCommand(stream) as PairingCommand;
            Assert.AreEqual("Name", newCommand.Name);
            Assert.AreEqual(TEST_UUID, newCommand.Uuid);
        }

        [TestMethod]
        public void TestAcknowledgementConversion()
        {
            var correlation = new CorrelationId("02F51ED3");
            var command = new AcknowledgementCommand(correlation, AckStatus.ID_NOT_FOUND);
            SendAndAssertMsg(command, ProtocolId.TAG_VAL_PROTOCOL, AcknowledgementCommand.ACKNOWLEDGEMENT_CMD_ID, "IC=02F51ED3|ST=1|");
            var newCommand = protocolConverter.ConvertMessageToCommand(stream) as AcknowledgementCommand;
            Assert.AreEqual(0x02f51ed3, newCommand.Correlation.GetUnderlyingCorrelation());
            Assert.AreEqual(AckStatus.ID_NOT_FOUND, newCommand.Status);
        }

        [TestMethod]
        public void TestValueChangeConversionForValue()
        {
            var command = new MenuChangeCommand(33, CorrelationId.EMPTY_CORRELATION, ChangeType.ABSOLUTE, 100.ToString());
            SendAndAssertMsg(command, ProtocolId.TAG_VAL_PROTOCOL, MenuChangeCommand.CHANGE_CMD_ID, "IC=00000000|ID=33|TC=1|VC=100|");
            var newCommand = protocolConverter.ConvertMessageToCommand(stream) as MenuChangeCommand;
            Assert.AreEqual(33, newCommand?.MenuId);
            Assert.AreEqual(CorrelationId.EMPTY_CORRELATION, newCommand?.Correlation);
            Assert.AreEqual(ChangeType.ABSOLUTE, newCommand?.ChangeType);
            Assert.AreEqual("100", newCommand?.Value);
        }

        [TestMethod]
        public void TestValueChangeConversionForList()
        {
            var listOfValues = new List<string>() { "item1", "item2", "item3", "item4" };
            var command = new MenuChangeCommand(33, CorrelationId.EMPTY_CORRELATION, ChangeType.CHANGE_LIST, listOfValues);
            SendAndAssertMsg(command, ProtocolId.TAG_VAL_PROTOCOL, MenuChangeCommand.CHANGE_CMD_ID, "IC=00000000|ID=33|TC=2|cA=item1|cB=item2|cC=item3|cD=item4|");
            var newCommand = protocolConverter.ConvertMessageToCommand(stream) as MenuChangeCommand;
            Assert.AreEqual(33, newCommand?.MenuId);
            Assert.AreEqual(CorrelationId.EMPTY_CORRELATION, newCommand?.Correlation);
            Assert.AreEqual(ChangeType.CHANGE_LIST, newCommand?.ChangeType);
            CollectionAssert.AreEquivalent(listOfValues, newCommand?.ListValues);
        }

        [TestMethod]
        public void TestDialogChangeConversion()
        {
            var command = new DialogCommand(DialogMode.SHOW, "header", "message", MenuButtonType.ACCEPT, MenuButtonType.CANCEL, CorrelationId.EMPTY_CORRELATION);
            SendAndAssertMsg(command, ProtocolId.TAG_VAL_PROTOCOL, DialogCommand.DIALOG_CMD_ID, "MO=S|IC=00000000|HF=header|BU=message|B1=1|B2=2|");
            var newCommand = protocolConverter.ConvertMessageToCommand(stream) as DialogCommand;
            Assert.AreEqual(DialogMode.SHOW, command.Mode);
            Assert.AreEqual("header", command.Header);
            Assert.AreEqual("message", command.Message);
            Assert.AreEqual(MenuButtonType.ACCEPT, command.Button1);
            Assert.AreEqual(MenuButtonType.CANCEL, command.Button2);
        }

        [TestMethod]
        public void TestDialogChangeConversionForAction()
        {
            var command = new DialogCommand(DialogMode.ACTION, null, null, MenuButtonType.ACCEPT, MenuButtonType.CANCEL, CorrelationId.EMPTY_CORRELATION);
            SendAndAssertMsg(command, ProtocolId.TAG_VAL_PROTOCOL, DialogCommand.DIALOG_CMD_ID, "MO=A|IC=00000000|HF=|BU=|B1=1|B2=2|");
            var newCommand = protocolConverter.ConvertMessageToCommand(stream) as DialogCommand;
            Assert.AreEqual(DialogMode.ACTION, command.Mode);
            Assert.AreEqual(null, command.Header);
            Assert.AreEqual(null, command.Message);
            Assert.AreEqual(MenuButtonType.ACCEPT, command.Button1);
            Assert.AreEqual(MenuButtonType.CANCEL, command.Button2);
        }

        [TestMethod]
        public void TestActionBootMessageConversion()
        {
            ActionMenuItem item = new ActionMenuItemBuilder()
                .WithId(123).WithEepromLocation(-1).WithName("act").WithReadOnly(false)
                .Build();
            var command = new ActionBootstrapCommand(33, item, false);
            SendAndAssertMsg(command, ProtocolId.TAG_VAL_PROTOCOL, ActionBootstrapCommand.ACTION_BOOT_CMD, "PI=33|ID=123|IE=-1|RO=0|VI=1|NM=act|VC=0|");
            var newCommand = protocolConverter.ConvertMessageToCommand(stream) as ActionBootstrapCommand;
            Assert.AreEqual(33, newCommand?.SubMenuId);
            Assert.AreEqual(123, newCommand?.Item.Id);
            Assert.AreEqual("act", newCommand?.Item.Name);
        }

        [TestMethod]
        public void TestSubMenuBootMessageConversion()
        {
            SubMenuItem item = new SubMenuItemBuilder()
                .WithId(222).WithEepromLocation(-1).WithName("sub").WithReadOnly(false)
                .Build();
            var command = new SubMenuBootstrapCommand(33, item, false);
            SendAndAssertMsg(command, ProtocolId.TAG_VAL_PROTOCOL, SubMenuBootstrapCommand.SUBMENU_BOOT_CMD, "PI=33|ID=222|IE=-1|RO=0|VI=1|NM=sub|VC=0|");
            var newCommand = protocolConverter.ConvertMessageToCommand(stream) as SubMenuBootstrapCommand;
            Assert.AreEqual(33, newCommand?.SubMenuId);
            Assert.AreEqual(222, newCommand?.Item.Id);
            Assert.AreEqual("sub", newCommand?.Item.Name);
        }

        [TestMethod]
        public void TestBooleanMenuBootMessageConversion()
        {
            BooleanMenuItem item = new BooleanMenuItemBuilder()
                .WithId(111).WithEepromLocation(222).WithName("bool").WithReadOnly(true).WithNaming(BooleanNaming.ON_OFF).WithVisible(false)
                .Build();
            var command = new BooleanBootstrapCommand(0, item, false);
            SendAndAssertMsg(command, ProtocolId.TAG_VAL_PROTOCOL, BooleanBootstrapCommand.BOOLEAN_BOOT_CMD, "PI=0|ID=111|IE=222|RO=1|VI=0|NM=bool|VC=0|BN=1|");
            var newCommand = protocolConverter.ConvertMessageToCommand(stream) as BooleanBootstrapCommand;
            Assert.AreEqual(0, newCommand?.SubMenuId);
            Assert.AreEqual(111, newCommand?.Item.Id);
            Assert.AreEqual("bool", newCommand?.Item.Name);
            Assert.AreEqual(BooleanNaming.ON_OFF, newCommand?.Item.Naming);
        }

        [TestMethod]
        public void TestFloatMenuBootMessageConversion()
        {
            FloatMenuItem item = new FloatMenuItemBuilder()
                .WithId(111).WithEepromLocation(222).WithName("float").WithReadOnly(true).WithDecimalPlaces(4)
                .Build();
            var command = new FloatBootstrapCommand(0, item, 5.5F);
            SendAndAssertMsg(command, ProtocolId.TAG_VAL_PROTOCOL, FloatBootstrapCommand.FLOAT_BOOT_CMD, "PI=0|ID=111|IE=222|RO=1|VI=1|NM=float|VC=5.5|FD=4|");
            var newCommand = protocolConverter.ConvertMessageToCommand(stream) as FloatBootstrapCommand;
            Assert.AreEqual(0, newCommand?.SubMenuId);
            Assert.AreEqual(111, newCommand?.Item.Id);
            Assert.AreEqual("float", newCommand?.Item.Name);

            var state = newCommand.NewMenuState(null);
            Assert.AreEqual(5.5, (double)state.Value, 0.0001);
            Assert.IsFalse(state.Changed);
            Assert.IsFalse(state.Active);
            Assert.AreEqual(newCommand.Item, state.Item);
        }

        [TestMethod]
        public void TestLargeNumMenuBootMessageConversion()
        {
            LargeNumberMenuItem item = new LargeNumberMenuItemBuilder()
                .WithId(111).WithEepromLocation(222).WithName("large").WithReadOnly(true).WithDecimalPlaces(4).WithTotalDigits(10).WithAllowNegative(false)
                .WithVisible(false).Build();
            var command = new LargeNumberBootstrapCommand(0, item, 10.4001M);
            SendAndAssertMsg(command, ProtocolId.TAG_VAL_PROTOCOL, LargeNumberBootstrapCommand.DECIMAL_BOOT_CMD, "PI=0|ID=111|IE=222|RO=1|VI=0|NM=large|VC=10.4001|FD=4|ML=10|NA=0|");
            var newCommand = protocolConverter.ConvertMessageToCommand(stream) as LargeNumberBootstrapCommand;
            Assert.IsNotNull(newCommand);
            Assert.AreEqual(0, newCommand.SubMenuId);
            Assert.AreEqual(111, newCommand.Item.Id);
            Assert.AreEqual("large", newCommand.Item.Name);
            Assert.IsFalse(newCommand.Item.AllowNegative);

            var state = newCommand.NewMenuState(null);
            Assert.AreEqual(10.4001M, state.Value);
            Assert.IsFalse(state.Changed);
            Assert.IsFalse(state.Active);
            Assert.AreEqual(newCommand.Item, state.Item);
        }
        
        [TestMethod]
        public void TestRgb32MenuBootMessageConversion()
        {
            var item = new Rgb32MenuItemBuilder()
                .WithId(111).WithEepromLocation(222).WithName("rgb").WithReadOnly(true).WithIncludeAlphaChannel(true)
                .WithVisible(false).Build();
            var command = new Rgb32BootstrapCommand(0, item, new PortableColor("#aabbcc"));
            SendAndAssertMsg(command, ProtocolId.TAG_VAL_PROTOCOL, Rgb32BootstrapCommand.RGB32_BOOT_CMD, "PI=0|ID=111|IE=222|RO=1|VI=0|NM=rgb|VC=#AABBCCFF|RA=1|");
            var newCommand = protocolConverter.ConvertMessageToCommand(stream) as Rgb32BootstrapCommand;
            Assert.IsNotNull(newCommand);
            Assert.AreEqual(0, newCommand.SubMenuId);
            Assert.AreEqual(111, newCommand.Item.Id);
            Assert.AreEqual("rgb", newCommand.Item.Name);
            Assert.IsTrue(newCommand.Item.IncludeAlphaChannel);

            var state = newCommand.NewMenuState(null);
            Assert.AreEqual(new PortableColor("#aabbccff"), state.Value);
            Assert.IsFalse(state.Changed);
            Assert.IsFalse(state.Active);
            Assert.AreEqual(newCommand.Item, state.Item);
        }

                
        [TestMethod]
        public void TestScrollMenuBootMessageConversion()
        {
            var item = new ScrollChoiceMenuItemBuilder()
                .WithId(111).WithEepromLocation(222).WithName("scroll").WithReadOnly(true).WithItemWidth(20)
                .WithChoiceMode(ScrollChoiceMode.ARRAY_IN_RAM).WithEepromOffset(22).WithNumEntries(30)
                .WithVisible(false).Build();
            var command = new ScrollChoiceBootstrapCommand(0, item, new CurrentScrollPosition("2-ABC"));
            SendAndAssertMsg(command, ProtocolId.TAG_VAL_PROTOCOL, ScrollChoiceBootstrapCommand.SCROLL_BOOT_CMD, "PI=0|ID=111|IE=222|RO=1|VI=0|NM=scroll|VC=2-ABC|WI=20|NC=30|EM=1|BU=|");
            var newCommand = protocolConverter.ConvertMessageToCommand(stream) as ScrollChoiceBootstrapCommand;
            Assert.IsNotNull(newCommand);
            Assert.AreEqual(0, newCommand.SubMenuId);
            Assert.AreEqual(111, newCommand.Item.Id);
            Assert.AreEqual("scroll", newCommand.Item.Name);
            Assert.AreEqual(20, newCommand.Item.ItemWidth);
            Assert.AreEqual(30, newCommand.Item.NumEntries);
            Assert.AreEqual(ScrollChoiceMode.ARRAY_IN_RAM, newCommand.Item.ChoiceMode);

            var state = newCommand.NewMenuState(null);
            Assert.AreEqual(new CurrentScrollPosition("2-ABC"), state.Value);
            Assert.IsFalse(state.Changed);
            Assert.IsFalse(state.Active);
            Assert.AreEqual(newCommand.Item, state.Item);
        }

        [TestMethod]
        public void TestTextMenuBootMessageConversion()
        {
            EditableTextMenuItem item = new EditableTextMenuItemBuilder()
                .WithId(111).WithEepromLocation(222).WithName("editable").WithReadOnly(false)
                .WithTextLength(10).WithEditType(EditItemType.IP_ADDRESS)
                .Build();
            var command = new TextBootstrapCommand(0, item, "Text");
            SendAndAssertMsg(command, ProtocolId.TAG_VAL_PROTOCOL, TextBootstrapCommand.TEXT_BOOT_CMD, "PI=0|ID=111|IE=222|RO=0|VI=1|NM=editable|VC=Text|ML=10|EM=1|");
            var newCommand = protocolConverter.ConvertMessageToCommand(stream) as TextBootstrapCommand;
            Assert.AreEqual(0, newCommand?.SubMenuId);
            Assert.AreEqual(111, newCommand?.Item.Id);
            Assert.AreEqual("editable", newCommand?.Item.Name);

            var state = newCommand.NewMenuState(null);
            Assert.AreEqual("Text", state.Value);
            Assert.IsFalse(state.Changed);
            Assert.IsFalse(state.Active);
            Assert.AreEqual(newCommand.Item, state.Item);
        }

        [TestMethod]
        public void TestListMenuBootMessageConversion()
        {
            RuntimeListMenuItem item = new RuntimeListMenuItemBuilder()
                .WithId(111).WithEepromLocation(222).WithName("runlist").WithReadOnly(false)
                .WithInitialRows(4)
                .Build();
            var command = new RuntimeListBootstrapCommand(0, item, new List<string>() { "item1", "item2" });
            SendAndAssertMsg(command, ProtocolId.TAG_VAL_PROTOCOL, RuntimeListBootstrapCommand.LIST_BOOT_CMD, "PI=0|ID=111|IE=222|RO=0|VI=1|NM=runlist|NC=4|cA=0|cB=1|CA=item1|CB=item2|");
            var newCommand = protocolConverter.ConvertMessageToCommand(stream) as RuntimeListBootstrapCommand;
            Assert.AreEqual(0, newCommand?.SubMenuId);
            Assert.AreEqual(111, newCommand?.Item.Id);
            Assert.AreEqual("runlist", newCommand?.Item.Name);

            var state = newCommand.NewMenuState(null);
            CollectionAssert.AreEquivalent(new List<string> { "0\titem1", "1\titem2" }, state.Value);
            Assert.IsFalse(state.Changed);
            Assert.IsFalse(state.Active);
            Assert.AreEqual(newCommand.Item, state.Item);
        }

        [TestMethod]
        public void TestAnalogBootMessageConversion()
        {
            AnalogMenuItem analogItem = new AnalogMenuItemBuilder()
                .WithId(123).WithEepromLocation(-1).WithName("analog").WithReadOnly(false)
                .WithUnitName("dB").WithOffset(-180).WithMaxValue(255).WithDivisor(2)
                .Build();
            var command = new AnalogBootstrapCommand(10, analogItem, 42);
            SendAndAssertMsg(command, ProtocolId.TAG_VAL_PROTOCOL, AnalogBootstrapCommand.ANALOG_BOOT_CMD, "PI=10|ID=123|IE=-1|RO=0|VI=1|NM=analog|AU=dB|AM=255|AD=2|AO=-180|VC=42|");
            var newCommand = protocolConverter.ConvertMessageToCommand(stream) as AnalogBootstrapCommand;
            Assert.AreEqual(10, newCommand?.SubMenuId);
            Assert.AreEqual(42, newCommand?.CurrentValue);
            Assert.AreEqual(123, newCommand?.Item.Id);
            Assert.AreEqual(-1, newCommand?.Item.EepromAddress);
            Assert.AreEqual("analog", newCommand?.Item.Name);
            Assert.AreEqual("dB", newCommand?.Item.UnitName);
            Assert.AreEqual(-180, newCommand?.Item.Offset);
            Assert.AreEqual(255, newCommand?.Item.MaximumValue);
            Assert.AreEqual(2, newCommand?.Item.Divisor);

            var state = newCommand.NewMenuState(null);
            Assert.AreEqual(42, state.Value);
            Assert.IsFalse(state.Changed);
            Assert.IsFalse(state.Active);
            Assert.AreEqual(newCommand.Item, state.Item);

            state = newCommand.NewMenuState(new MenuState<int>(state.Item, false, true, 33));
            Assert.AreEqual(42, state.Value);
            Assert.IsTrue(state.Changed);
            Assert.IsTrue(state.Active);
        }

        [TestMethod]
        public void TestEnumBootMessageConversion()
        {
            EnumMenuItem enumItem = new EnumMenuItemBuilder()
                .WithId(312).WithEepromLocation(222).WithName("enum").WithReadOnly(true)
                .WithNewEntry("Entry1").WithNewEntry("Entry2")
                .Build();
            var command = new EnumBootstrapCommand(0, enumItem, 2);
            SendAndAssertMsg(command, ProtocolId.TAG_VAL_PROTOCOL, EnumBootstrapCommand.ENUM_BOOT_CMD, "PI=0|ID=312|IE=222|RO=1|VI=1|NM=enum|NC=1|CA=Entry1|CB=Entry2|VC=2|");
            var newCommand = protocolConverter.ConvertMessageToCommand(stream) as EnumBootstrapCommand;
            Assert.AreEqual(0, newCommand?.SubMenuId);
            Assert.AreEqual(2, newCommand?.CurrentValue);
            Assert.AreEqual(312, newCommand?.Item.Id);
            Assert.AreEqual(222, newCommand?.Item.EepromAddress);
            Assert.AreEqual("enum", newCommand?.Item.Name);
            CollectionAssert.AreEquivalent(new List<string>() { "Entry1", "Entry2" }, newCommand?.Item.EnumEntries);

            var state = newCommand.NewMenuState(null);
            Assert.AreEqual(2, state.Value);
            Assert.IsFalse(state.Changed);
            Assert.IsFalse(state.Active);
            Assert.AreEqual(newCommand.Item, state.Item);
        }

        [TestMethod]
        [ExpectedException(exceptionType:typeof(TcProtocolException))]
        public void TestUnexpectedProtocolOnOutput()
        {
            HeartbeatCommand command = new HeartbeatCommand(HeartbeatMode.NORMAL, 10);
            protocolConverter.ConvertCommandToMessage(command, ProtocolId.NO_PROTOCOL, stream);
        }

        [TestMethod]
        [ExpectedException(exceptionType: typeof(TcProtocolException))]
        public void TestUnexpectedInputMsg()
        {
            // simulate a dodgy message..
            stream.WriteByte(0x01);
            stream.WriteByte(0x00);      // bad protocol
            stream.WriteByte((byte)'~'); // and invalid msg type
            stream.WriteByte((byte)'`');
            stream.Seek(0, SeekOrigin.Begin);
            protocolConverter.ConvertMessageToCommand(stream);
        }

        private void SendAndAssertMsg(MenuCommand command, ProtocolId expectedProtocol, ushort expectedCmd, string expected)
        {
            protocolConverter.ConvertCommandToMessage(command, ProtocolId.TAG_VAL_PROTOCOL, stream);

            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(DefaultProtocolCommandConverter.START_OF_MESSAGE, (byte)stream.ReadByte());
            Assert.AreEqual((byte)expectedProtocol, (byte)stream.ReadByte());
            Assert.AreEqual((byte)(expectedCmd >> 8), (byte)stream.ReadByte());
            Assert.AreEqual((byte)(expectedCmd & 0xff), (byte)stream.ReadByte());

            bool foundEnd = false;
            var sb = new StringBuilder();
            while(!foundEnd)
            {
                char ch = (char)stream.ReadByte();
                if (ch == 0x02) foundEnd = true;
                else if (ch == -1) Assert.Fail("Found EOF when not expected");
                else sb.Append(ch);
            }
            Assert.IsTrue(foundEnd);

            var str = sb.ToString();
            Assert.AreEqual(expected, str);

            stream.Seek(0, SeekOrigin.Begin);
        }

        [TestMethod]
        public void TestContainsFullMessage()
        {
            byte[] dataGood = { 0x01, 0x48, 0x02 };
            byte[] dataGood2 = { 0x29, 0x84, 0x01, 0x48, 0x02, 0x10, 0x01, 0x38, 0x02 };
            byte[] dataMissingStart = { 0x48, 0x02 };
            byte[] dataMissingEnd = { 0x01, 0x48 };

            Assert.AreEqual(2, protocolConverter.PositionOfEOMInBuffer(dataGood, dataGood.Length, ProtocolId.TAG_VAL_PROTOCOL));
            Assert.AreEqual(4, protocolConverter.PositionOfEOMInBuffer(dataGood2, dataGood2.Length, ProtocolId.TAG_VAL_PROTOCOL));
            Assert.AreEqual(-1, protocolConverter.PositionOfEOMInBuffer(dataMissingStart, dataMissingStart.Length, ProtocolId.TAG_VAL_PROTOCOL));
            Assert.AreEqual(-1, protocolConverter.PositionOfEOMInBuffer(dataMissingEnd, dataMissingEnd.Length, ProtocolId.TAG_VAL_PROTOCOL));
        }
    }
}
