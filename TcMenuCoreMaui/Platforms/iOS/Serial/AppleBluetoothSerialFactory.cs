using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreBluetooth;
using CoreFoundation;
using Foundation;
using ObjCRuntime;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteCore;
using TcMenuCoreMaui.BaseSerial;


namespace TcMenuCoreMaui.iOS.SerialImpl
{
    public class AppleBluetoothSerialFactory : ISerialPortFactory
    {
        public static readonly  CBUUID ServiceId = CBUUID.FromString("0256d4a7-90e2-41c7-8031-ad5f2b42fc84");
        private readonly CBCentralManager _btManager;

        private volatile Action<SerialPortInformation> _portDelegate;
        
        public Action<SerialPortInformation> PortDelegate => _portDelegate;

        private Dictionary<string, CBPeripheral> _peripheralsById = new Dictionary<string, CBPeripheral>();
        private volatile bool _isBtEnabled;

        public AppleBluetoothSerialFactory()
        {
            _btManager = new CBCentralManager(new TcBluetoothDelegate(this), DispatchQueue.MainQueue);
        }

        public bool StartScanningPorts(SerialPortType type, Action<SerialPortInformation> portDelegate)
        {
            if (!_isBtEnabled) return false;
            
            _portDelegate = portDelegate;
            _btManager.ScanForPeripherals(ServiceId);
            return true;
        }

        public void StopScanningPorts()
        {
            if(_btManager.IsScanning) _btManager.StopScan();
        }

        public IRemoteConnector CreateSerialConnector(LocalIdentification localId, SerialPortInformation info, int baud,
            IProtocolCommandConverter converter, ProtocolId protocol, SystemClock clock, bool pairing)
        {
            throw new System.NotImplementedException();
        }

        public string MakeIdForPeripheral(CBPeripheral peripheral)
        {
            lock (_peripheralsById)
            {
                _peripheralsById.Add(peripheral.Name, peripheral);
            }

            return peripheral.Name;
        }

        public void SetPoweredUp(bool b)
        {
            _isBtEnabled = b;
        }
    }

    public class TcBluetoothDelegate : CBCentralManagerDelegate
    {
        private readonly AppleBluetoothSerialFactory _serialFactory;

        public TcBluetoothDelegate(AppleBluetoothSerialFactory serialFactory)
        {
            _serialFactory = serialFactory;
        }

        public override void UpdatedState(CBCentralManager central)
        {
            _serialFactory.SetPoweredUp(central.State == CBManagerState.PoweredOn);
        }

        public override void DiscoveredPeripheral(CBCentralManager central, CBPeripheral peripheral, NSDictionary advertisementData,
            NSNumber rssi)
        {
            _serialFactory.PortDelegate?.Invoke(new SerialPortInformation(
                peripheral.Name,
                SerialPortType.BLE_BLUETOOTH,
                _serialFactory.MakeIdForPeripheral(peripheral),
                (double)rssi));
        }
    }
    
}