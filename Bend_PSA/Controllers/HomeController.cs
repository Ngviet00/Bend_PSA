using Bend_PSA.Models;
using Bend_PSA.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Bend_PSA.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataService _dataService;
        public HomeController(DataService dataService)
        {
            _dataService = dataService;
        }

        public IActionResult Index()
        {
            //READ FILR AND SET VALUE TO GLOBAL
            return View();
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
