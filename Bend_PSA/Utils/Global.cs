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

        public static int RunMode = (int)ERunMode.NORMAL;
        public static int CurrentRoll = 1;
        public static int CurrentIndexPLC = 1;

        public static short CONNECT_1 = Constants.INACTIVE;
        public static short CONNECT_2 = Constants.INACTIVE;

        public static short CAM_1 = Constants.INACTIVE;
        public static short CAM_2 = Constants.INACTIVE;

        public static short DEEP_LEARNING_1 = Constants.INACTIVE;
        public static short DEEP_LEARNING_2 = Constants.INACTIVE;

        public static System.Timers.Timer? timerClientConnect1 = null;
        public static System.Timers.Timer? timerClientConnect2 = null;

        public static System.Timers.Timer? timerClientCam1 = null;
        public static System.Timers.Timer? timerClientCam2 = null;

        public static System.Timers.Timer? timerClientDeepLearning1 = null;
        public static System.Timers.Timer? timerClientDeepLearning2 = null;

        public static string StringModels = string.Empty;

        public static string CurrentModel = string.Empty;
        public static List<string> ListModels = [];

        public static short Client1PostModel = Constants.INACTIVE;
        public static short Client2PostModel = Constants.INACTIVE;

        public static string PathDownloadImage = @"D:\publish_image";

        public static string PATH_EXPORT_EXCEL = @"D:\Export_Excel";

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

        public static List<string> GetListModelsAppearTwoTime(string models)
        {
            return models.Split(',').GroupBy(x => x).Where(g => g.Count() == 2).Select(g => g.Key).ToList();
        }
    }
}
