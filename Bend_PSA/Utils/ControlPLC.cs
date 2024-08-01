using ActUtlType64Lib;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Bend_PSA.Utils
{
    public class ControlPLC : INotifyPropertyChanged
    {
        private static readonly object _lock = new();
        private ActUtlType64 _plc = new ActUtlType64();
        private const int plcStation = 1;
        private const int timeSleep = 100;

        private const string REGISTER_PLC_WRITE = "D";

        public const string REGISTER_PLC_READ_STATUS = "D20";
        private const string REGISTER_PLC_VISION_BUSY= "M420";

        public event PropertyChangedEventHandler? PropertyChanged;

        private int _statusPLC;

        public int StatusPLC
        {
            get { return _statusPLC; }

            set
            {
                if (_statusPLC != value)
                {
                    _statusPLC = value;
                    Notify();
                }
            }
        }

        public void Notify([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ControlPLC()
        {
            _plc.ActLogicalStationNumber = plcStation;
        }

        public void ConnectPLC()
        {
            if (_plc.Open() == 0 || _plc.Open() == 25202689)
            {
                Thread thReadStatusPLC = new Thread(async () => await ReadStatusPLC());
                thReadStatusPLC.Name = "THREAD_READ_STATUS_PLC";
                thReadStatusPLC.IsBackground = true;
                thReadStatusPLC.Start();
            }
            else
            {
                Logs.Log($"Error cant connect to PLC station number {plcStation}");
            }
        }

        private async Task ReadStatusPLC()
        {
            while (true)
            {
                _plc.ReadDeviceBlock(REGISTER_PLC_READ_STATUS, 1, out int valueReaded);
                StatusPLC = valueReaded;
                await Task.Delay(timeSleep);
            }
        }

        public int ReadDeviceBlock(string address)
        {
            _plc.ReadDeviceBlock(address, 1, out int valueReaded);

            return valueReaded;
        }

        public bool SetBitDevice2(string address, short signal)
        {
            try
            {
                _plc.SetDevice2(address, signal);
                return true;
            }
            catch (Exception ex)
            {
                Logs.Log($"Error can not SetBitDevice2 in ControlPLC, error: {ex.Message}");
                return false;
            }
        }

        public bool WriteToRegister(int index, int data)
        {
            try
            {
                _plc.WriteDeviceBlock($"{REGISTER_PLC_WRITE}{index}", 1, data);
                return true;
            }
            catch (Exception ex)
            {
                Logs.Log($"Error can not WriteToRegister in ControlPLC, error: {ex.Message}");
                return false;
            }
        }

        public void VisionBusy(bool result)
        {
            try
            {
                _plc.SetDevice2(REGISTER_PLC_VISION_BUSY, result ? (short)1 : (short)0);
            }
            catch (Exception ex)
            {
                Logs.Log($"Error can not set VisionBusy in ControlPLC, error: {ex.Message}");
            }
        }
    }
}
