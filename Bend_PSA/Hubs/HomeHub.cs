using Bend_PSA.Services;
using Microsoft.AspNetCore.SignalR;

namespace Stiffiner_Inspection.Hubs
{
    public class HomeHub : Hub
    {
        private readonly DataService _dataService;

        public HomeHub(DataService dataService)
        {
            _dataService = dataService;
        }
    }
}
