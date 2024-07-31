using Bend_PSA.Models.Responses;
using System.Timers;

namespace Bend_PSA.Utils
{
    public static class TimerManager
    {
        private static readonly List<NameTimer> namedTimers = [];

        public static void AddTimer(string name, double interval, ElapsedEventHandler onElapsed)
        {
            var timer = new System.Timers.Timer(interval);
            timer.Elapsed += onElapsed;
            timer.Start();

            var namedTimer = new NameTimer(name, timer);
            namedTimers.Add(namedTimer);
        }

        public static void RemoveTimer(string name)
        {
            var namedTimer = namedTimers.Find(nt => nt.Name == name);

            if (namedTimer != null)
            {
                namedTimer.Timer.Stop();
                namedTimer.Timer.Dispose();
                namedTimers.Remove(namedTimer);
            }
        }

        public static void ListTimers()
        {
            foreach (var namedTimer in namedTimers)
            {
                Console.WriteLine($"Timer Name: {namedTimer.Name}, Interval: {namedTimer.Timer.Interval}");
            }
        }
    }
}
