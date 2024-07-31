namespace Bend_PSA.Models.Responses
{
    public class NameTimer(string name, System.Timers.Timer timer)
    {
        public string Name { get; set; } = name;
        public System.Timers.Timer Timer { get; set; } = timer;
    }
}
