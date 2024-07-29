namespace Bend_PSA.Utils
{
    public static class Logs
    {
        private static readonly object _lock = new();

        public static void Log(string message)
        {
            lock (_lock)
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

                string logFormat = DateTime.Now.ToString("yyyy_MM_dd -- HH_mm_ss") + " ==> ";

                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }

                try
                {
                    using (StreamWriter writer = File.AppendText(logPath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt"))
                    {
                        writer.WriteLine(logFormat + message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
