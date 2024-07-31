using Bend_PSA.Models;
using Bend_PSA.Services;
using Bend_PSA.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Hubs;
using System.Diagnostics;

namespace Bend_PSA.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataService _dataService;
        private IHubContext<HomeHub> _homeHub;

        public HomeController(DataService dataService, IHubContext<HomeHub> homeHub)
        {
            _dataService = dataService;
            _homeHub = homeHub;
        }

        public IActionResult Index()
        {
            GetDataHomePage();

            ControlPLC.Instance.PropertyChanged += PLCPropertyChanged;

            return View();
        }

        private async void PLCPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "StatusPLC":
                    await _homeHub.Clients.All.SendAsync("StatusPLC", ((ControlPLC?)sender)?.StatusPLC);
                    break;
            }
        }

        private void GetDataHomePage()
        {
            Dictionary<string, string> currentData = Files.ReadValueFileTxt(Files.GetFilePathSetting(), ["TotalOK", "TotalNG", "TotalEmpty", "Timeline", "CurrentModel", "ListModels"]);

            Global.TotalOK = int.Parse(currentData["TotalOK"]);
            Global.TotalNG = int.Parse(currentData["TotalNG"]);
            Global.TotalEmpty = int.Parse(currentData["TotalEmpty"]);
            Global.TimeLine = currentData["Timeline"];
            Global.CurrentModel = currentData["CurrentModel"];
            Global.ListModels = currentData["ListModels"].Split(new[] { ", " }, StringSplitOptions.None).ToList();

            ViewBag.Total = Global.TotalOK + Global.TotalNG + Global.TotalEmpty;
            ViewBag.TotalOK = Global.TotalOK;
            ViewBag.TotalNG = Global.TotalNG;
            ViewBag.TotalEmpty = Global.TotalEmpty;
            ViewBag.CurrentModel = Global.CurrentModel;
            ViewBag.ListModels = Global.ListModels;

            ViewBag.PercentChartOK = ViewBag.Total == 0 ? 0 : Math.Round((double)Global.TotalOK / (double)ViewBag.Total * Constants.PERCENT, 2);
            ViewBag.PercentChartNG = ViewBag.Total == 0 ? 0 : Math.Round((double)Global.TotalNG / (double)ViewBag.Total * Constants.PERCENT, 2);
            ViewBag.PercentChartEmpty = ViewBag.Total == 0 ? 0 : Math.Round((double)Global.TotalEmpty / (double)ViewBag.Total * Constants.PERCENT, 2);
            
            ViewBag.currentStatusPLC = ControlPLC.Instance.ReadDeviceBlock(ControlPLC.REGISTER_PLC_READ_STATUS);
        }

        public IActionResult ClearData()
        {
            Global.TimeLine = DateTime.Now.ToString("yyMMddHHmmss");
            Files.WriteFileToTxt(Files.GetFilePathSetting(), new Dictionary<string, string>
            {
                { "TotalOK", "0" },
                { "TotalNG", "0" },
                { "TotalEmpty", "0" },
                { "Timeline", Global.TimeLine },
            });
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
