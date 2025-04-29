using System.Collections.Generic;
using System.IO;
using System.Text;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Serialisation;

namespace TcMenu.CoreSdk.Protocol
{
    public class TagValProtocolMessageProcessors
    {
        public const byte END_OF_FIELD = (byte)'|';
        public const byte KEY_VAL_SEPARATOR = (byte)'=';

        public void RegisterConverters(DefaultProtocolCommandConverter converter)
        {
            converter.RegisterFromMsgToCmdConverter(HeartbeatCommand.HEARTBEAT_CMD_ID, ProtocolId.TAG_VAL_PROTOCOL, ConvertMsgToHeartbeat);
            converter.RegisterFromMsgToCmdConverter(NewJoinerCommand.NEW_JOINER_CMD_ID, ProtocolId.TAG_VAL_PROTOCOL, ConvertMsgToNewJoiner);
            converter.RegisterFromMsgToCmdConverter(BootstrapCommand.BOOTSTRAP_CMD_ID, ProtocolId.TAG_VAL_PROTOCOL, ConvertMsgToBootstrap);
            converter.RegisterFromMsgToCmdConverter(PairingCommand.PAIRING_CMD_ID, ProtocolId.TAG_VAL_PROTOCOL, ConvertMsgToPairing);
            converter.RegisterFromMsgToCmdConverter(AcknowledgementCommand.ACKNOWLEDGEMENT_CMD_ID, ProtocolId.TAG_VAL_PROTOCOL, ConvertMsgToAcknowledgement);
            converter.RegisterFromMsgToCmdConverter(AnalogBootstrapCommand.ANALOG_BOOT_CMD, ProtocolId.TAG_VAL_PROTOCOL, ConvertMsgToAnalogBoot);
            converter.RegisterFromMsgToCmdConverter(FloatBootstrapCommand.FLOAT_BOOT_CMD, ProtocolId.TAG_VAL_PROTOCOL, ConvertMsgToFloatBoot);
            converter.RegisterFromMsgToCmdConverter(LargeNumberBootstrapCommand.DECIMAL_BOOT_CMD, ProtocolId.TAG_VAL_PROTOCOL, ConvertMsgToLargeNumBoot);
            converter.RegisterFromMsgToCmdConverter(EnumBootstrapCommand.ENUM_BOOT_CMD, ProtocolId.TAG_VAL_PROTOCOL, ConvertMsgToEnumBoot);
            converter.RegisterFromMsgToCmdConverter(SubMenuBootstrapCommand.SUBMENU_BOOT_CMD, ProtocolId.TAG_VAL_PROTOCOL, ConvertMsgToSubMenuBoot);
            converter.RegisterFromMsgToCmdConverter(ActionBootstrapCommand.ACTION_BOOT_CMD, ProtocolId.TAG_VAL_PROTOCOL, ConvertMsgToActionBoot);
            converter.RegisterFromMsgToCmdConverter(BooleanBootstrapCommand.BOOLEAN_BOOT_CMD, ProtocolId.TAG_VAL_PROTOCOL, ConvertMsgToBooleanBoot);
            converter.RegisterFromMsgToCmdConverter(TextBootstrapCommand.TEXT_BOOT_CMD, ProtocolId.TAG_VAL_PROTOCOL, ConvertMsgToTextBoot);
            converter.RegisterFromMsgToCmdConverter(Rgb32BootstrapCommand.RGB32_BOOT_CMD, ProtocolId.TAG_VAL_PROTOCOL, ConvertMsgToRgb32Boot);
            converter.RegisterFromMsgToCmdConverter(ScrollChoiceBootstrapCommand.SCROLL_BOOT_CMD, ProtocolId.TAG_VAL_PROTOCOL, ConvertMsgToScrollChoiceBoot);
            converter.RegisterFromMsgToCmdConverter(RuntimeListBootstrapCommand.LIST_BOOT_CMD, ProtocolId.TAG_VAL_PROTOCOL, ConvertMsgToListBoot);
            converter.RegisterFromMsgToCmdConverter(MenuChangeCommand.CHANGE_CMD_ID, ProtocolId.TAG_VAL_PROTOCOL, ConvertMsgToChangeCommand);
            converter.RegisterFromMsgToCmdConverter(DialogCommand.DIALOG_CMD_ID, ProtocolId.TAG_VAL_PROTOCOL, ConvertMsgToDialogCommand);
            //converter.RegisterFromMsgToCmdConverter(, ProtocolId.TAG_VAL_PROTOCOL, ConvertMsgTo);

            converter.RegisterToMessageConverter(HeartbeatCommand.HEARTBEAT_CMD_ID, ProtocolId.TAG_VAL_PROTOCOL, ConvertHeartbeatToMsg);
            converter.RegisterToMessageConverter(NewJoinerCommand.NEW_JOINER_CMD_ID, ProtocolId.TAG_VAL_PROTOCOL, ConvertNewJoinerToMsg);
            converter.RegisterToMessageConverter(BootstrapCommand.BOOTSTRAP_CMD_ID, ProtocolId.TAG_VAL_PROTOCOL, ConvertBootstrapToMsg);
            converter.RegisterToMessageConverter(PairingCommand.PAIRING_CMD_ID, ProtocolId.TAG_VAL_PROTOCOL, ConvertPairingToMsg);
            converter.RegisterToMessageConverter(AcknowledgementCommand.ACKNOWLEDGEMENT_CMD_ID, ProtocolId.TAG_VAL_PROTOCOL, ConvertAcknowldgementToMsg);
            converter.RegisterToMessageConverter(AnalogBootstrapCommand.ANALOG_BOOT_CMD, ProtocolId.TAG_VAL_PROTOCOL, ConvertAnalogBootToMsg);
            converter.RegisterToMessageConverter(FloatBootstrapCommand.FLOAT_BOOT_CMD, ProtocolId.TAG_VAL_PROTOCOL, ConvertFloatMenuBootToMsg);
            converter.RegisterToMessageConverter(LargeNumberBootstrapCommand.DECIMAL_BOOT_CMD, ProtocolId.TAG_VAL_PROTOCOL, ConvertLargeNumberMenuBootToMsg);
            converter.RegisterToMessageConverter(Rgb32BootstrapCommand.RGB32_BOOT_CMD, ProtocolId.TAG_VAL_PROTOCOL, ConvertRgb32MenuBootToMsg);
            converter.RegisterToMessageConverter(ScrollChoiceBootstrapCommand.SCROLL_BOOT_CMD, ProtocolId.TAG_VAL_PROTOCOL, ConvertScrollBootToMsg);
            converter.RegisterToMessageConverter(EnumBootstrapCommand.ENUM_BOOT_CMD, ProtocolId.TAG_VAL_PROTOCOL, ConvertEnumBootToMsg);
            converter.RegisterToMessageConverter(SubMenuBootstrapCommand.SUBMENU_BOOT_CMD, ProtocolId.TAG_VAL_PROTOCOL, ConvertSubMenuBootToMsg);
            converter.RegisterToMessageConverter(ActionBootstrapCommand.ACTION_BOOT_CMD, ProtocolId.TAG_VAL_PROTOCOL, ConvertActionMenuBootToMsg);
            converter.RegisterToMessageConverter(BooleanBootstrapCommand.BOOLEAN_BOOT_CMD, ProtocolId.TAG_VAL_PROTOCOL, ConvertBooleanMenuBootToMsg);
            converter.RegisterToMessageConverter(TextBootstrapCommand.TEXT_BOOT_CMD, ProtocolId.TAG_VAL_PROTOCOL, ConvertTextMenuBootToMsg);
            converter.RegisterToMessageConverter(RuntimeListBootstrapCommand.LIST_BOOT_CMD, ProtocolId.TAG_VAL_PROTOCOL, ConvertListMenuBootToMsg);
            converter.RegisterToMessageConverter(MenuChangeCommand.CHANGE_CMD_ID, ProtocolId.TAG_VAL_PROTOCOL, ConvertChangeCmdToMsg);
            converter.RegisterToMessageConverter(DialogCommand.DIALOG_CMD_ID, ProtocolId.TAG_VAL_PROTOCOL, ConvertDialogCommandToMsg);
            //converter.RegisterToMessageConverter(, ProtocolId.TAG_VAL_PROTOCOL, Convert);
        }

        protected void ConvertHeartbeatToMsg(MenuCommand msgIn, Stream stream)
        {
            if (msgIn is HeartbeatCommand cmd)
            {
                WriteField(stream, FieldKeyPairs.FIELD_HB_INTERVAL, cmd.Interval);
                WriteField(stream, FieldKeyPairs.FIELD_HB_MILLISEC, cmd.Timestamp);
                WriteField(stream, FieldKeyPairs.FIELD_HB_MODE, (int)cmd.Mode);
            }
        }

        protected MenuCommand ConvertMsgToHeartbeat(Stream stream)
        {
            TagValTextParser textParser = new TagValTextParser(stream);
            return new HeartbeatCommand(
                (HeartbeatMode)textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_HB_MODE),
                textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_HB_INTERVAL),
                textParser.GetValueForKeyAsIntWithDefault(FieldKeyPairs.FIELD_HB_MILLISEC, 0)
            );
        }

        protected void ConvertAcknowldgementToMsg(MenuCommand msgIn, Stream stream)
        {
            if (msgIn is AcknowledgementCommand cmd)
            {
                WriteField(stream, FieldKeyPairs.FIELD_CORRELATION, cmd.Correlation);
                WriteField(stream, FieldKeyPairs.FIELD_ACK_STATUS, (int)cmd.Status);
            }
        }

        protected MenuCommand ConvertMsgToAcknowledgement(Stream stream)
        {
            TagValTextParser textParser = new TagValTextParser(stream);
            CorrelationId cor = new CorrelationId(textParser.GetValueForKeyWithDefault(FieldKeyPairs.FIELD_CORRELATION, "0"));
            AckStatus sts = (AckStatus)textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_ACK_STATUS);
            return new AcknowledgementCommand(cor, sts);
        }

        protected void ConvertNewJoinerToMsg(MenuCommand msgIn, Stream stream)
        {
            if(msgIn is NewJoinerCommand cmd)
            {
                WriteField(stream, FieldKeyPairs.FIELD_MSG_NAME, cmd.Name);
                WriteField(stream, FieldKeyPairs.FIELD_UUID, cmd.Uuid);
                WriteField(stream, FieldKeyPairs.FIELD_SERIAL_NO, cmd.SerialNumber);
                WriteField(stream, FieldKeyPairs.FIELD_VERSION, cmd.ApiVersion);
                WriteField(stream, FieldKeyPairs.FIELD_PLATFORM, (byte)cmd.ApiPlatform);
            }
        }

        protected MenuCommand ConvertMsgToNewJoiner(Stream stream)
        {
            TagValTextParser textParser = new TagValTextParser(stream);
            return new NewJoinerCommand(
                textParser.GetValueForKey(FieldKeyPairs.FIELD_MSG_NAME),
                textParser.GetValueForKey(FieldKeyPairs.FIELD_UUID),
                (ushort)textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_VERSION),
                (ApiPlatform)textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_PLATFORM),
                textParser.GetValueForKeyWithDefault(FieldKeyPairs.FIELD_SERIAL_NO, "0")
            );
        }

        protected void ConvertBootstrapToMsg(MenuCommand msgIn, Stream stream)
        {
            if(msgIn is BootstrapCommand cmd)
            {
                WriteField(stream, FieldKeyPairs.FIELD_BOOT_TYPE, cmd.BootType == BootstrapType.START ? "START" : "END");
            }
        }

        protected MenuCommand ConvertMsgToBootstrap(Stream stream)
        {
            TagValTextParser textParser = new TagValTextParser(stream);
            var bootType = textParser.GetValueForKey(FieldKeyPairs.FIELD_BOOT_TYPE);
            return new BootstrapCommand((bootType.Equals("START")) ? BootstrapType.START : BootstrapType.END);
        }

        protected void ConvertPairingToMsg(MenuCommand msgIn, Stream stream)
        {
            if (msgIn is PairingCommand cmd)
            {
                WriteField(stream, FieldKeyPairs.FIELD_MSG_NAME, cmd.Name);
                WriteField(stream, FieldKeyPairs.FIELD_UUID, cmd.Uuid);
            }
        }

        protected MenuCommand ConvertMsgToPairing(Stream stream)
        {
            TagValTextParser textParser = new TagValTextParser(stream);
            return new PairingCommand(
                textParser.GetValueForKey(FieldKeyPairs.FIELD_UUID),
                textParser.GetValueForKey(FieldKeyPairs.FIELD_MSG_NAME)
            );
        }

        protected void ConvertAnalogBootToMsg(MenuCommand msgIn, Stream stream)
        {
            if (msgIn is AnalogBootstrapCommand cmd)
            {
                WriteBootCoreFields(stream, cmd);
                WriteField(stream, FieldKeyPairs.FIELD_ANALOG_UNIT, cmd.Item.UnitName);
                WriteField(stream, FieldKeyPairs.FIELD_ANALOG_MAX, cmd.Item.MaximumValue);
                WriteField(stream, FieldKeyPairs.FIELD_ANALOG_DIV, cmd.Item.Divisor);
                WriteField(stream, FieldKeyPairs.FIELD_ANALOG_OFF, cmd.Item.Offset);
                WriteField(stream, FieldKeyPairs.FIELD_CURRENT_VAL, cmd.CurrentValue);
            }
        }

        protected MenuCommand ConvertMsgToAnalogBoot(Stream stream)
        {
            TagValTextParser textParser = new TagValTextParser(stream);

            AnalogMenuItemBuilder itemBuilder = new AnalogMenuItemBuilder()
                .WithDivisor(textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_ANALOG_DIV))
                .WithMaxValue(textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_ANALOG_MAX))
                .WithOffset(textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_ANALOG_OFF))
                .WithUnitName(textParser.GetValueForKey(FieldKeyPairs.FIELD_ANALOG_UNIT))
                .WithStep(textParser.GetValueForKeyAsIntWithDefault(FieldKeyPairs.FIELD_ANALOG_STEP, 1));

            int parent = ReadBootItemBasicsAndSub(textParser, itemBuilder);
            int currentValue = textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_CURRENT_VAL);

            return new AnalogBootstrapCommand(parent, itemBuilder.Build(), currentValue);
        }

        protected void ConvertEnumBootToMsg(MenuCommand msgIn, Stream stream)
        {
            if (msgIn is EnumBootstrapCommand cmd)
            {
                WriteBootCoreFields(stream, cmd);
                WriteField(stream, FieldKeyPairs.FIELD_NO_CHOICES, cmd.Item.EnumEntries.Count - 1);
                OutputAllFieldsOfList(stream, cmd.Item.EnumEntries, 'C');
                WriteField(stream, FieldKeyPairs.FIELD_CURRENT_VAL, cmd.CurrentValue);
            }
        }

        protected MenuCommand ConvertMsgToEnumBoot(Stream stream)
        {
            TagValTextParser textParser = new TagValTextParser(stream);

            EnumMenuItemBuilder itemBuilder = new EnumMenuItemBuilder();

            itemBuilder.WithEntries(textParser.GetAllKeysAsString('C'));

            int parent = ReadBootItemBasicsAndSub(textParser, itemBuilder);
            int currentValue = textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_CURRENT_VAL);

            return new EnumBootstrapCommand(parent, itemBuilder.Build(), currentValue);
        }

        protected MenuCommand ConvertMsgToListBoot(Stream stream)
        {
            TagValTextParser textParser = new TagValTextParser(stream);

            RuntimeListMenuItemBuilder itemBuilder = new RuntimeListMenuItemBuilder();
            itemBuilder.WithInitialRows(textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_NO_CHOICES));

            int parent = ReadBootItemBasicsAndSub(textParser, itemBuilder);
            List<string> currentValue = textParser.GetAllKeysAsString('c', 'C');

            return new RuntimeListBootstrapCommand(parent, itemBuilder.Build(), currentValue);
        }

        protected void ConvertListMenuBootToMsg(MenuCommand msgIn, Stream stream)
        {
            if (msgIn is RuntimeListBootstrapCommand cmd)
            {
                WriteBootCoreFields(stream, cmd);
                WriteField(stream, FieldKeyPairs.FIELD_NO_CHOICES, cmd.Item.InitialRows);

                for (int i = 0; i < cmd.CurrentValue.Count; i++)
                {
                    WriteField(stream, MenuCommand.MakeCmdPair('c', (char)('A' + i)), i);
                }

                OutputAllFieldsOfList(stream, cmd.CurrentValue, 'C');
            }
        }

        protected MenuCommand ConvertMsgToSubMenuBoot(Stream stream)
        {
            TagValTextParser textParser = new TagValTextParser(stream);
            SubMenuItemBuilder itemBuilder = new SubMenuItemBuilder();
            int parent = ReadBootItemBasicsAndSub(textParser, itemBuilder);
            return new SubMenuBootstrapCommand(parent, itemBuilder.Build(), false);
        }

        protected void ConvertSubMenuBootToMsg(MenuCommand msgIn, Stream stream)
        {
            if (msgIn is SubMenuBootstrapCommand cmd)
            {
                WriteBootCoreFields(stream, cmd);
                WriteField(stream, FieldKeyPairs.FIELD_CURRENT_VAL, 0);
            }
        }

        protected MenuCommand ConvertMsgToActionBoot(Stream stream)
        {
            TagValTextParser textParser = new TagValTextParser(stream);
            ActionMenuItemBuilder itemBuilder = new ActionMenuItemBuilder();
            int parent = ReadBootItemBasicsAndSub(textParser, itemBuilder);
            return new ActionBootstrapCommand(parent, itemBuilder.Build(), false);
        }

        protected void ConvertActionMenuBootToMsg(MenuCommand msgIn, Stream stream)
        {
            if (msgIn is ActionBootstrapCommand cmd)
            {
                WriteBootCoreFields(stream, cmd);
                WriteField(stream, FieldKeyPairs.FIELD_CURRENT_VAL, 0);
            }
        }

        protected void ConvertBooleanMenuBootToMsg(MenuCommand msgIn, Stream stream)
        {
            if (msgIn is BooleanBootstrapCommand cmd)
            {
                WriteBootCoreFields(stream, cmd);
                WriteField(stream, FieldKeyPairs.FIELD_CURRENT_VAL, cmd.CurrentValue ? 1 : 0);
                WriteField(stream, FieldKeyPairs.FIELD_BOOL_NAMING, (int)cmd.Item.Naming);
            }
        }

        protected MenuCommand ConvertMsgToBooleanBoot(Stream stream)
        {
            TagValTextParser textParser = new TagValTextParser(stream);
            BooleanMenuItemBuilder itemBuilder = new BooleanMenuItemBuilder();
            itemBuilder.WithNaming((BooleanNaming)textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_BOOL_NAMING));
            int parent = ReadBootItemBasicsAndSub(textParser, itemBuilder);
            bool val = textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_CURRENT_VAL) != 0;
            return new BooleanBootstrapCommand(parent, itemBuilder.Build(), val);
        }

        protected void ConvertFloatMenuBootToMsg(MenuCommand msgIn, Stream stream)
        {
            if (msgIn is FloatBootstrapCommand cmd)
            {
                WriteBootCoreFields(stream, cmd);
                WriteField(stream, FieldKeyPairs.FIELD_CURRENT_VAL, cmd.CurrentValue);
                WriteField(stream, FieldKeyPairs.FIELD_FLOAT_DP, cmd.Item.DecimalPlaces);
            }
        }

        protected void ConvertLargeNumberMenuBootToMsg(MenuCommand msgIn, Stream stream)
        {
            if (msgIn is LargeNumberBootstrapCommand cmd)
            {
                WriteBootCoreFields(stream, cmd);
                WriteField(stream, FieldKeyPairs.FIELD_CURRENT_VAL, cmd.CurrentValue);
                WriteField(stream, FieldKeyPairs.FIELD_FLOAT_DP, cmd.Item.DecimalPlaces);
                WriteField(stream, FieldKeyPairs.FIELD_MAX_LEN, cmd.Item.TotalDigits);
                WriteField(stream, FieldKeyPairs.FIELD_ALLOW_NEG, cmd.Item.AllowNegative ? 1 : 0);
            }
        }
        
        protected void ConvertRgb32MenuBootToMsg(MenuCommand msgIn, Stream stream)
        {
            if (msgIn is Rgb32BootstrapCommand cmd)
            {
                WriteBootCoreFields(stream, cmd);
                WriteField(stream, FieldKeyPairs.FIELD_CURRENT_VAL, cmd.CurrentValue);
                WriteField(stream, FieldKeyPairs.FIELD_ALPHA, cmd.Item.IncludeAlphaChannel ? 1 : 0);
            }
        }

        protected void ConvertScrollBootToMsg(MenuCommand msgIn, Stream stream)
        {
            if (msgIn is ScrollChoiceBootstrapCommand cmd)
            {
                WriteBootCoreFields(stream, cmd);
                WriteField(stream, FieldKeyPairs.FIELD_CURRENT_VAL, cmd.CurrentValue.ToString());
                WriteField(stream, FieldKeyPairs.FIELD_WIDTH, cmd.Item.ItemWidth);
                WriteField(stream, FieldKeyPairs.FIELD_NO_CHOICES, cmd.Item.NumEntries);
                WriteField(stream, FieldKeyPairs.FIELD_EDIT_MODE, (int)cmd.Item.ChoiceMode);
                WriteField(stream, FieldKeyPairs.FIELD_BUFFER, cmd.Item.RamVariable ?? "");
            }
        }

        protected MenuCommand ConvertMsgToTextBoot(Stream stream)
        {
            TagValTextParser textParser = new TagValTextParser(stream);
            EditableTextMenuItemBuilder itemBuilder = new EditableTextMenuItemBuilder();
            itemBuilder.WithEditType((EditItemType)textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_EDIT_MODE));
            itemBuilder.WithTextLength(textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_MAX_LEN));

            int parent = ReadBootItemBasicsAndSub(textParser, itemBuilder);

            string val = textParser.GetValueForKey(FieldKeyPairs.FIELD_CURRENT_VAL);
            return new TextBootstrapCommand(parent, itemBuilder.Build(), val);
        }

        protected MenuCommand ConvertMsgToRgb32Boot(Stream stream)
        {
            var textParser = new TagValTextParser(stream);
            var itemBuilder = new Rgb32MenuItemBuilder();
            itemBuilder.WithIncludeAlphaChannel(textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_ALPHA) == 1);

            int parent = ReadBootItemBasicsAndSub(textParser, itemBuilder);
            var val = textParser.GetValueForKey(FieldKeyPairs.FIELD_CURRENT_VAL);
            return new Rgb32BootstrapCommand(parent, itemBuilder.Build(), new PortableColor(val));
        }

        protected MenuCommand ConvertMsgToScrollChoiceBoot(Stream stream)
        {
            var textParser = new TagValTextParser(stream);
            var itemBuilder = new ScrollChoiceMenuItemBuilder();
            itemBuilder.WithNumEntries(textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_NO_CHOICES));
            itemBuilder.WithItemWidth(textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_WIDTH));
            itemBuilder.WithChoiceMode((ScrollChoiceMode)textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_EDIT_MODE));
            itemBuilder.WithRamVariable(textParser.GetValueForKeyWithDefault(FieldKeyPairs.FIELD_BUFFER, ""));
            int parent = ReadBootItemBasicsAndSub(textParser, itemBuilder);
            var val = textParser.GetValueForKey(FieldKeyPairs.FIELD_CURRENT_VAL);
            return new ScrollChoiceBootstrapCommand(parent, itemBuilder.Build(), new CurrentScrollPosition(val));
        }

        protected void ConvertTextMenuBootToMsg(MenuCommand msgIn, Stream stream)
        {
            if (msgIn is TextBootstrapCommand cmd)
            {
                WriteBootCoreFields(stream, cmd);
                WriteField(stream, FieldKeyPairs.FIELD_CURRENT_VAL, cmd.CurrentValue);
                WriteField(stream, FieldKeyPairs.FIELD_MAX_LEN, cmd.Item.TextLength);
                WriteField(stream, FieldKeyPairs.FIELD_EDIT_MODE, (int)cmd.Item.EditType);
            }
        }

        protected MenuCommand ConvertMsgToFloatBoot(Stream stream)
        {
            TagValTextParser textParser = new TagValTextParser(stream);
            FloatMenuItemBuilder itemBuilder = new FloatMenuItemBuilder();
            itemBuilder.WithDecimalPlaces(textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_FLOAT_DP));
            int parent = ReadBootItemBasicsAndSub(textParser, itemBuilder);
            float val = float.Parse(textParser.GetValueForKey(FieldKeyPairs.FIELD_CURRENT_VAL));
            return new FloatBootstrapCommand(parent, itemBuilder.Build(), val);
        }

        protected MenuCommand ConvertMsgToLargeNumBoot(Stream stream)
        {
            TagValTextParser textParser = new TagValTextParser(stream);
            LargeNumberMenuItemBuilder largeNumBuilder = new LargeNumberMenuItemBuilder();
            largeNumBuilder.WithTotalDigits(textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_MAX_LEN));
            largeNumBuilder.WithDecimalPlaces(textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_FLOAT_DP));
            largeNumBuilder.WithAllowNegative(
                textParser.GetValueForKeyAsIntWithDefault(FieldKeyPairs.FIELD_ALLOW_NEG, 1) != 0);
            int parent = ReadBootItemBasicsAndSub(textParser, largeNumBuilder);
            var val = LargeNumberBootstrapCommand.StrToDecimalSafe(textParser.GetValueForKey(FieldKeyPairs.FIELD_CURRENT_VAL));
            return new LargeNumberBootstrapCommand(parent, largeNumBuilder.Build(), val);
        }

        protected void ConvertChangeCmdToMsg(MenuCommand msgIn, Stream stream)
        {
            if (msgIn is MenuChangeCommand cmd)
            {
                WriteField(stream, FieldKeyPairs.FIELD_CORRELATION, cmd.Correlation);
                WriteField(stream, FieldKeyPairs.FIELD_ID, cmd.MenuId);
                WriteField(stream, FieldKeyPairs.FIELD_CHANGE_TYPE, (int)cmd.ChangeType);
                if (cmd.ChangeType == ChangeType.CHANGE_LIST)
                {
                    OutputAllFieldsOfList(stream, cmd.ListValues, 'c');
                }
                else
                {
                    WriteField(stream, FieldKeyPairs.FIELD_CURRENT_VAL, cmd.Value);
                }
            }
        }

        protected MenuCommand ConvertMsgToChangeCommand(Stream stream)
        {
            TagValTextParser textParser = new TagValTextParser(stream);
            ChangeType type = (ChangeType)textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_CHANGE_TYPE);
            var id = textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_ID);
            var corId = CorrelationFromMsg(textParser);
            switch(type)
            {
                case ChangeType.CHANGE_LIST:
                    return new MenuChangeCommand(id, corId, type, textParser.GetAllKeysAsString('c'));
                case ChangeType.ABSOLUTE:
                case ChangeType.DELTA:
                default:
                    return new MenuChangeCommand(id, corId, type, textParser.GetValueForKey(FieldKeyPairs.FIELD_CURRENT_VAL));
            }
        }

        protected MenuCommand ConvertMsgToDialogCommand(Stream stream)
        {
            TagValTextParser textParser = new TagValTextParser(stream);
            var mode = textParser.GetValueForKey(FieldKeyPairs.FIELD_MODE)[0];
            return new DialogCommand(
                (DialogMode)mode,
                textParser.GetValueForKeyWithDefault(FieldKeyPairs.FIELD_HEADER, ""),
                textParser.GetValueForKeyWithDefault(FieldKeyPairs.FIELD_BUFFER, ""),
                (MenuButtonType)textParser.GetValueForKeyAsIntWithDefault(FieldKeyPairs.FIELD_BUTTON1, 0),
                (MenuButtonType)textParser.GetValueForKeyAsIntWithDefault(FieldKeyPairs.FIELD_BUTTON2, 0),
                CorrelationFromMsg(textParser)
            );
        }

        protected void ConvertDialogCommandToMsg(MenuCommand msgIn, Stream stream)
        {
            if (msgIn is DialogCommand cmd)
            {
                WriteField(stream, FieldKeyPairs.FIELD_MODE, (char)cmd.Mode);
                WriteField(stream, FieldKeyPairs.FIELD_CORRELATION, cmd.Correlation);
                WriteField(stream, FieldKeyPairs.FIELD_HEADER, cmd.Header ?? "");
                WriteField(stream, FieldKeyPairs.FIELD_BUFFER, cmd.Message ?? "");
                WriteField(stream, FieldKeyPairs.FIELD_BUTTON1, (int)cmd.Button1);
                WriteField(stream, FieldKeyPairs.FIELD_BUTTON2, (int)cmd.Button2);
            }
        }

        private CorrelationId CorrelationFromMsg(TagValTextParser parser)
        {
            var correlation = parser.GetValueForKeyWithDefault(FieldKeyPairs.FIELD_CORRELATION, "0");
            return correlation != null ? new CorrelationId(correlation) : CorrelationId.EMPTY_CORRELATION;
        }

        private void OutputAllFieldsOfList(Stream stream, List<string> data, char cmdKey)
        {
            int count = data?.Count ?? 0;
            for (int i = 0; i < count; i++)
            {
                ushort key = MenuCommand.MakeCmdPair(cmdKey, (char)('A' + i));
                WriteField(stream, key, data[i]);
            }
        }

        private int ReadBootItemBasicsAndSub<T,B>(TagValTextParser textParser, MenuItemBuilder<T, B> itemBuilder) where T: MenuItemBuilder<T,B> where B : MenuItem
        {
            itemBuilder.WithEepromLocation(textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_EEPROM))
                .WithId(textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_ID))
                .WithReadOnly(textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_READONLY) != 0)
                .WithVisible(textParser.GetValueForKeyAsIntWithDefault(FieldKeyPairs.FIELD_VISIBLE, 1) != 0)
                .WithName(textParser.GetValueForKey(FieldKeyPairs.FIELD_MSG_NAME));

            return textParser.GetValueForKeyAsInt(FieldKeyPairs.FIELD_PARENT);
        }

        private void WriteBootCoreFields<T, V>(Stream stream, BootstrapMenuCommand<T, V> boot) where T : MenuItem
        {
            WriteField(stream, FieldKeyPairs.FIELD_PARENT, boot.SubMenuId);
            WriteField(stream, FieldKeyPairs.FIELD_ID, boot.Item.Id);
            WriteField(stream, FieldKeyPairs.FIELD_EEPROM, boot.Item.EepromAddress);
            WriteField(stream, FieldKeyPairs.FIELD_READONLY, boot.Item.ReadOnly ? 1 : 0);
            WriteField(stream, FieldKeyPairs.FIELD_VISIBLE, boot.Item.Visible ? 1 : 0);
            WriteField(stream, FieldKeyPairs.FIELD_MSG_NAME, boot.Item.Name);
        }

        private void WriteField(Stream stream, ushort field, object data)
        {

            stream.WriteByte((byte)(field >> 8));
            stream.WriteByte((byte)(field & 0xff));
            stream.WriteByte(KEY_VAL_SEPARATOR);

            if (data is string v) {
                if (v.IndexOf('|') != -1)
                {
                    v = v.Replace("|", "\\|");
                }
                if (v.IndexOf('=') != -1)
                {
                    v = v.Replace("=", "\\=");
                }
                var val = Encoding.UTF8.GetBytes(v);
                stream.Write(val, 0, val.Length);
            }
            else
            {
                var val = Encoding.UTF8.GetBytes(data.ToString());
                stream.Write(val, 0, val.Length);

            }

            stream.WriteByte(END_OF_FIELD);
        }
    }
}
