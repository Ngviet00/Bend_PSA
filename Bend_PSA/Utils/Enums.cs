namespace Bend_PSA.Utils
{
    public enum EStatusPLC
    {
        START = 1,
        STOP = 2,
        EMG = 3,
        DISCONNECTED = 4
    }

    public enum ERunMode
    {
        MASTER = 1,
        NORMAL = 2,
        BYPASS = 3
    }

    public enum EDataStatus
    {
        OK = 1,
        NG = 2,
        EMPTY = 3
    }

    public enum EClient
    {
        CLIENT_1 = 1,
        CLIENT_2 = 2,
    }
}
