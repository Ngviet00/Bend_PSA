using Bend_PSA.Models.Requests;
using Bend_PSA.Services;
using Bend_PSA.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Hubs;
using System.Dynamic;

namespace Bend_PSA.Controllers.Api
{
    [Route("api/data/")]
    [ApiController]
    public class ApiDataController : Controller
    {
        private readonly DataService _dataService;
        private IHubContext<HomeHub> _homeHub;

        public ApiDataController(DataService dataService, IHubContext<HomeHub> homeHub) 
        {
            _dataService = dataService;
            _homeHub = homeHub;
        }

        [HttpPost("save-data")]
        public IActionResult SaveData(DataRequest dataRequest)
        {
            try
            {
                switch (dataRequest.ClientId)
                {
                    case 1:
                        Global.dataClient1.Enqueue(dataRequest);
                        break;

                    case 2:
                        Global.dataClient2.Enqueue(dataRequest);
                        break;
                }

                return Ok(new
                {
                    status = 200,
                    message = "success!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = 500,
                    message = ex.Message,
                });
            }
        }

        [HttpGet("get-data")]
        public async Task<IActionResult> GetData(int clientId)
        {
            try
            {
                dynamic data = new ExpandoObject();
                data.status = 200;
                data.message = "success";
                data.current_model = Global.CurrentModel;
                data.mode_run = Global.RunMode;

                switch (clientId)
                {
                    case 1:
                        Global.CONNECT_1 = Constants.ACTIVE;
                        break;
                    case 2:
                        Global.CONNECT_2 = Constants.ACTIVE;
                        break;
                }
                
                TimerManager.RemoveTimer($"ClientConnect{clientId}");
                TimerManager.AddTimer($"ClientConnect{clientId}", 5000, async (sender, e) => await TimerElapsed(_homeHub, clientId, "ClientConnect"));
                
                await _homeHub.Clients.All.SendAsync("ClientConnect", clientId, true);

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = 500,
                    message = ex.Message,
                });
            }
        }

        [HttpGet("get-cam")]
        public async Task<IActionResult> GetCam(int clientId)
        {
            try
            {
                switch (clientId)
                {
                    case 1:
                        Global.CAM_1 = Constants.ACTIVE;
                        break;
                    case 2:
                        Global.CAM_2 = Constants.ACTIVE;
                        break;
                }

                TimerManager.RemoveTimer($"ClientCam{clientId}");
                TimerManager.AddTimer($"ClientCam{clientId}", 5000, async (sender, e) => await TimerElapsed(_homeHub, clientId, "ClientCam"));

                await _homeHub.Clients.All.SendAsync("ClientCam", clientId, true);

                return Ok(new
                {
                    status = 200,
                    message = "success!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = 500,
                    message = ex.Message,
                });
            }
        }

        [HttpGet("get-deep-learning")]
        public async Task<IActionResult> GetDeepLearning(int clientId)
        {
            try
            {
                switch (clientId)
                {
                    case 1:
                        Global.DEEP_LEARNING_1 = Constants.ACTIVE;
                        break;
                    case 2:
                        Global.DEEP_LEARNING_2 = Constants.ACTIVE;
                        break;
                }

                TimerManager.RemoveTimer($"ClientDeepLearning{clientId}");
                TimerManager.AddTimer($"ClientDeepLearning{clientId}", 5000, async (sender, e) => await TimerElapsed(_homeHub, clientId, "ClientDeepLearning"));

                await _homeHub.Clients.All.SendAsync("ClientDeepLearning", clientId, true);

                return Ok(new
                {
                    status = 200,
                    message = "success!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = 500,
                    message = ex.Message,
                });
            }
        }

        private static async Task TimerElapsed(IHubContext<HomeHub> homeHub, int clientId, string nameActionHomeHub)
        {
            await homeHub.Clients.All.SendAsync(nameActionHomeHub, clientId, false);
            TimerManager.RemoveTimer($"{nameActionHomeHub}{clientId}");

            switch (nameActionHomeHub + clientId)
            {
                case "ClientConnect1":
                    Global.CONNECT_1 = Constants.INACTIVE;
                    break;

                case "ClientConnect2":
                    Global.CONNECT_2 = Constants.INACTIVE;
                    break;

                case "ClientCam1":
                    Global.CAM_1 = Constants.INACTIVE;
                    break;

                case "ClientCam2":
                    Global.CAM_2= Constants.INACTIVE;
                    break;

                case "ClientDeepLearning1":
                    Global.DEEP_LEARNING_1 = Constants.INACTIVE;
                    break;

                case "ClientDeepLearning2":
                    Global.DEEP_LEARNING_2 = Constants.INACTIVE;
                    break;
            }
        }
    }
}
