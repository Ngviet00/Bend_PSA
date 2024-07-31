using Bend_PSA.Context;
using Bend_PSA.Models.Entities;
using Bend_PSA.Models.Requests;
using Bend_PSA.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Stiffiner_Inspection.Hubs;

namespace Bend_PSA.Services
{
    public class DataService
    {
        private readonly ApplicationDbContext _context;
        private IHubContext<HomeHub> _homeHub;
        private readonly IServiceScopeFactory _scopeFactory;

        public DataService(ApplicationDbContext context, IHubContext<HomeHub> homeHub, IServiceScopeFactory scopeFactory)
        {
            _context = context;
            _homeHub = homeHub;
            _scopeFactory = scopeFactory;
        }

        public async Task DataSynthesis()
        {
            while (true)
            {
                if (!Global.dataClient1.IsEmpty && !Global.dataClient2.IsEmpty)
                {
                    if (Global.dataClient1.TryDequeue(out var dataRequest1) && Global.dataClient2.TryDequeue(out var dataRequest2))
                    {
                        var result = GetResultItem(dataRequest1, dataRequest2);
                        await SendDataToPLC(result);
                        await SaveToDB(dataRequest1, dataRequest2);
                        await ShowTotalQtyToScreen(result);
                        await _homeHub.Clients.All.SendAsync("SendDataToClient", dataRequest1, dataRequest2, Global.CurrentModel, Global.CurrentRoll);
                    }
                }
                await Task.Delay(200);
            }
        }

        private async Task ShowTotalQtyToScreen(int result)
        {
            switch (result)
            {
                case 1:
                    Global.TotalOK += 1;
                    break;

                case 2:
                    Global.TotalNG += 1;
                    break;

                case 3:
                    Global.TotalEmpty += 1;
                    break;
            }

            Files.WriteFileToTxt(Files.GetFilePathSetting(), new Dictionary<string, string>
            {
                { "TotalOK", Global.TotalOK.ToString() },
                { "TotalNG", Global.TotalNG.ToString() },
                { "TotalEmpty", Global.TotalEmpty.ToString() },
            });

            await _homeHub.Clients.All.SendAsync("ShowTotalQtyToScreen", 
                Global.TotalOK + Global.TotalNG + Global.TotalEmpty, 
                Global.TotalOK, Global.TotalNG, Global.TotalEmpty);
        }

        public async Task SaveToDB(DataRequest data1, DataRequest data2)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            List<Image> images = [];
            List<Error> errors = [];

            var result = new Data
            {
                Model = Global.CurrentModel,
                Roll = Global.CurrentRoll,
                Result1 = data1.Result,
                Result2 = data2.Result,
                TimeLine = Global.TimeLine
            };

            await context.Data.AddAsync(result);
            await context.SaveChangesAsync();

            if (result.Id == Guid.Empty)
            {
                throw new Exception("Failed to generate DataId.");
            }

            var finalResult = GetResultItem(data1, data2);

            if (finalResult == (int)EDataStatus.NG)
            {
                foreach (var item in data1.Errors)
                {
                    errors.Add(new Error
                    {
                        DataId = result.Id,
                        TypeError = item.ErrorCode
                    });
                }

                foreach (var item in data2.Errors)
                {
                    errors.Add(new Error
                    {
                        DataId = result.Id,
                        TypeError = item.ErrorCode
                    });
                }

                foreach (var item in data1.Images)
                {
                    images.Add(new Image
                    {
                        DataId = result.Id,
                        PathUrl = item.PathImage
                    });
                }

                foreach (var item in data2.Images)
                {
                    images.Add(new Image
                    {
                        DataId = result.Id,
                        PathUrl = item.PathImage
                    });
                }
            }

            await context.Error.AddRangeAsync(errors);
            await context.Image.AddRangeAsync(images);

            await context.SaveChangesAsync();
        }

        public async Task SendDataToPLC(int data)
        {
            if (!ControlPLC.Instance.WriteToRegister(Global.CurrentIndexPLC, data))
            {
                await _homeHub.Clients.All.SendAsync("SendDataPLCError", "Error can not send data to PLC!");
            }
        }

        public int GetResultItem(DataRequest data1, DataRequest data2)
        {
            if (data1.Result == (int)EDataStatus.OK && data2.Result == (int)EDataStatus.OK)
            {
                return (int)EDataStatus.OK;
            }

            if (data1.Result == (int)EDataStatus.NG || data2.Result == (int)EDataStatus.NG)
            {
                return (int)EDataStatus.NG;
            }
            else
            {
                return (int)EDataStatus.EMPTY;
            }
        }

        public async Task AutoDeleteDataOlderThan3Month()
        {
            try
            {
                while (true)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    await context.Database.ExecuteSqlRawAsync("DELETE FROM data WHERE created_at < DATEADD(MONTH, -3, GETDATE())");
                    await Task.Delay(TimeSpan.FromHours(5));
                }
            }
            catch (Exception ex)
            {
                Logs.Log($"Error can not AutoDeleteDataOlderThan3Month in DataService, error: {ex.Message}");
            }
        }

        public async Task AutoDeleteImageDownload()
        {
            try
            {
                while (true)
                {
                    DeleteImageDownload(Global.PathDownloadImage);
                    await Task.Delay(TimeSpan.FromHours(3));
                }
            }
            catch (Exception ex)
            {
                Logs.Log($"Error can not AutoDeleteImageDownload in DataService, error: {ex.Message}");
            }
        }

        private static void DeleteImageDownload(string basePath)
        {
            if (!Directory.Exists(basePath))
            {
                Logs.Log("Not exists path image download folder to delete!");
                return;
            }

            int batchSize = 1000;

            var fileBatch = Directory.EnumerateFiles(basePath).Take(batchSize);

            while (fileBatch.Any())
            {
                foreach (var file in fileBatch)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        Logs.Log($"Error deleting file {file}, error: {ex.Message}");
                    }
                }

                fileBatch = Directory.EnumerateFiles(basePath).Skip(batchSize).Take(batchSize);
            }

            var directories = Directory.GetDirectories(basePath);

            foreach (var directory in directories)
            {
                DeleteImageDownload(directory);
            }
        }

        public async Task CheckStatusVisionBusy()
        {
            while (true)
            {
                if (Global.CONNECT_1 == Constants.ACTIVE && Global.CONNECT_2 == Constants.ACTIVE &&
                    Global.CAM_1 == Constants.ACTIVE && Global.CAM_2 == Constants.ACTIVE &&
                    Global.DEEP_LEARNING_1 == Constants.ACTIVE && Global.DEEP_LEARNING_2 == Constants.ACTIVE)
                {
                    await _homeHub.Clients.All.SendAsync("VisionBusy", true);
                    ControlPLC.Instance.VisionBusy(false);
                }
                else
                {
                    await _homeHub.Clients.All.SendAsync("VisionBusy", false);
                    ControlPLC.Instance.VisionBusy(true);
                }
                await Task.Delay(200);
            }
        }
    }
}
