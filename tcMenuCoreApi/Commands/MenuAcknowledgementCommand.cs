using System;
using System.Collections.Generic;
using System.Text;
using TcMenu.CoreSdk.Protocol;

namespace TcMenu.CoreSdk.Commands
{
    public class MenuAcknowledgementCommand
    {
        public CorrelationId Correlation { get; }
        public AckStatus Status { get; }

        public MenuAcknowledgementCommand(CorrelationId correlation, AckStatus status)
        {
            Correlation = correlation;
            Status = status;
        }

        public override bool Equals(object obj)
        {
            var command = obj as MenuAcknowledgementCommand;
            return command != null &&
                   EqualityComparer<CorrelationId>.Default.Equals(Correlation, command.Correlation) &&
                   Status == command.Status;
        }

        public override int GetHashCode()
        {
            var hashCode = -765415658;
            hashCode = hashCode * -1521134295 + EqualityComparer<CorrelationId>.Default.GetHashCode(Correlation);
            hashCode = hashCode * -1521134295 + Status.GetHashCode();
            return hashCode;
        }
    }
}
