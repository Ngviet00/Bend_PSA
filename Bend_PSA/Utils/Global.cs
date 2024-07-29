using Bend_PSA.Models.Requests;
using System.Collections.Concurrent;
using System.Globalization;

namespace Bend_PSA.Utils
{
    public static class Global
    {
        public static ConcurrentQueue<DataRequest> dataClient1 = new();
        public static ConcurrentQueue<DataRequest> dataClient2 = new();

        public static int TotalOK = 0;
        public static int TotalNG = 0;
        public static int TotalEmpty = 0;

        public static int RunMode = 2;
        public static int CurrentRoll = 1;
        public static int CurrentIndexPLC = 1;

        public static string CurrentModel = string.Empty;

        public static string PathDownloadImage = @"D:\publish_image";

        public static string TimeLine = DateTime.Now.ToString("yyMMddHHmmss");

        public static string FormatNumber(int number)
        {
            NumberFormatInfo customNumberFormat = new()
            {
                NumberDecimalSeparator = ",",
                NumberGroupSeparator = "."
            };

            return number.ToString("#,##0", customNumberFormat);
        }
    }
}
