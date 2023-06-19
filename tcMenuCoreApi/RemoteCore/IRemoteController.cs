using System.Threading.Tasks;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Protocol;

namespace TcMenu.CoreSdk.RemoteCore
{
    public delegate void AcknolwedgementsReceivedHandler(CorrelationId correlation, AckStatus status);
    public delegate void MenuChangedEventHandler(MenuItem changed, bool valueOnly);
    public delegate void DialogUpdatedEventHandler(CorrelationId cor, DialogMode mode,  string hdr, string msg, MenuButtonType b1, MenuButtonType b2);

    public interface IRemoteController
    {
        event AcknolwedgementsReceivedHandler AcknowledgementsReceived;
        event MenuChangedEventHandler MenuChangedEvent;
        event DialogUpdatedEventHandler DialogUpdatedEvent;

        IRemoteConnector Connector { get; }
        MenuTree ManagedMenu { get; }
        void Start();
        void Stop(bool waitForCompleteStop = false);

        Task<CorrelationId> SendDeltaChange(MenuItem item, int difference);
        Task<CorrelationId> SendAbsoluteChange(MenuItem item, object newValue);
        Task<CorrelationId> SendDialogAction(MenuButtonType buttonType);
    }
}
