using ActUtlType64Lib;
using System.ComponentModel;

namespace Bend_PSA.Utils
{
    public class ControlPLC : INotifyPropertyChanged
    {
        public static ControlPLC? _instance;
        private static readonly object _lock = new();
        private readonly ActUtlType64 _plc = new();
        private const int plcStation = 1;
        private const int timeSleep = 100;
        private const string REGISTER_PLC_WRITE = "D";

        public event PropertyChangedEventHandler? PropertyChanged;

        public ControlPLC()
        {
            _plc.ActLogicalStationNumber = plcStation;
        }

        public static ControlPLC Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ControlPLC();
                    }
                    return _instance;
                }
            }
        }

        public void ConnectPLC()
        {
            if (_plc.Open() == 0)
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

        private static async Task ReadStatusPLC()
        {
            while (true)
            {
                //read all data
                //assign variable
                //notify
                //Console.WriteLine("11");
                await Task.Delay(timeSleep);
            }
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
    }
}
