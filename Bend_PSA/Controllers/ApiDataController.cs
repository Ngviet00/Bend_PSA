using Bend_PSA.Models.Requests;
using Bend_PSA.Services;
using Bend_PSA.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Dynamic;

namespace Bend_PSA.Controllers.Api
{
    [Route("api/data/")]
    [ApiController]
    public class ApiDataController : Controller
    {
        private readonly DataService _dataService;

        public ApiDataController(DataService dataService) 
        {
            _dataService = dataService;
        }

        [HttpPost("save-data")]
        public IActionResult SaveData(DataRequest dataRequest)
        {
            try
            {
                if (dataRequest.ClientId == (int)EClient.CLIENT_1)
                {
                    Global.dataClient1.Enqueue(dataRequest);
                }
                else
                {
                    Global.dataClient2.Enqueue(dataRequest);
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
        public IActionResult GetData(int clientId)
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
                        break;

                    case 2:
                        break;
                }

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
    }
}
