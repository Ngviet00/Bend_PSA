using Bend_PSA.Services;
using Bend_PSA.Utils;
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

        public void ChangeModel(string selectedModel)
        {
            Global.CurrentModel = selectedModel.Trim();
            Files.WriteFileToTxt(Files.GetFilePathSetting(), new Dictionary<string, string>
            {
                { "CurrentModel", selectedModel },
            });
        }
    }
}
