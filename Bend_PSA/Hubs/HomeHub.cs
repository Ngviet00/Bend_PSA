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
            try
            {
                Global.CurrentModel = selectedModel.Trim();
                Files.WriteFileToTxt(Files.GetFilePathSetting(), new Dictionary<string, string>
                {
                    { "CurrentModel", selectedModel },
                });
            }
            catch (Exception ex)
            {
                Logs.Log($"Error can not change model, error: {ex.Message}");
            }
        }

        public void ReloadModels()
        {
            try
            {
                Global.Client1PostModel = Constants.ACTIVE;
                Global.Client2PostModel = Constants.ACTIVE;

                Global.StringModels = string.Empty;
                Global.CurrentModel = string.Empty;
                Global.ListModels.Clear();

                Files.WriteFileToTxt(Files.GetFilePathSetting(), new Dictionary<string, string>
                {
                    { "CurrentModel", string.Empty },
                    { "ListModels", string.Empty },
                });
            }
            catch (Exception ex)
            {
                Logs.Log($"Error can not reload model, error: {ex.Message}");
            }
        }

        public async Task<string> ExportData(string fromDate, string toDate)
        {
            try
            {
                return await _dataService.ExportData(fromDate, toDate);
            }
            catch (Exception ex)
            {
                Logs.Log($"Error can not export data, error: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
