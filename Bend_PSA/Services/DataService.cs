using Bend_PSA.Context;
using Bend_PSA.Models.Entities;
using Bend_PSA.Models.Requests;
using Bend_PSA.Models.Responses;
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
            try
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
            catch (Exception ex)
            {
                Logs.Log($"Error can not save to database, error: {ex.Message}");
            }
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
            try
            {
                while (true)
                {
                    if (Global.CONNECT_1 == Constants.ACTIVE && Global.CONNECT_2 == Constants.ACTIVE
                        && Global.CAM_1 == Constants.ACTIVE && Global.CAM_2 == Constants.ACTIVE
                        && Global.DEEP_LEARNING_1 == Constants.ACTIVE && Global.DEEP_LEARNING_2 == Constants.ACTIVE
                        && !string.IsNullOrWhiteSpace(Global.CurrentModel))
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
            catch (Exception ex)
            {
                Logs.Log($"Error can not save to database, error: {ex.Message}");
            }
        }

        public async Task<string> ExportData(string fromDate, string toDate)
        {
            try
            {
                var sql = $@"
                    WITH DataResults AS (
                        SELECT 
                            CAST(d.time AS DATE) AS date_select, 
                            d.model,
                            CASE 
                                WHEN d.model = 'Stiffener_954_PSA_Side' OR d.model = 'Stiffener_953_PSA_Side' OR d.model = 'Stiffener_963_964_PSA_Side'
                                THEN 'PSA'  
                                ELSE 'SUS' 
                            END AS type_model, 
                            SUM(CASE WHEN d.result_area = 1 AND d.result_line = 1 THEN 1 ELSE 0 END) AS ok,
                            SUM(CASE WHEN d.result_area = 2 OR d.result_line = 2 THEN 1 ELSE 0 END) AS ng
                        FROM data d
                        WHERE CAST(d.time AS DATE) >= '{fromDate}' 
                          AND CAST(d.time AS DATE) <= '{toDate}' 
                          AND d.model <> ''
                        GROUP BY d.model, CAST(d.time AS DATE)
                    ),
                    ErrorCounts AS (
                        SELECT 
                            CAST(d.time AS DATE) AS date_select,
                            d.model,
                            SUM(CASE WHEN e.type_error = 1 THEN 1 ELSE 0 END) AS error_particle,
                            SUM(CASE WHEN e.type_error = 2 THEN 1 ELSE 0 END) AS error_ng_tape_position,
                            SUM(CASE WHEN e.type_error = 3 THEN 1 ELSE 0 END) AS error_deform,
                            SUM(CASE WHEN e.type_error = 4 THEN 1 ELSE 0 END) AS error_scratch,
                            SUM(CASE WHEN e.type_error = 5 THEN 1 ELSE 0 END) AS error_dirty
                        FROM data d
                        LEFT JOIN errors e ON d.id = e.data_id
                        WHERE CAST(d.time AS DATE) >= '{fromDate}' 
                          AND CAST(d.time AS DATE) <= '{toDate}'
                          AND d.model <> '' 
                        GROUP BY d.model, CAST(d.time AS DATE)
                    )
                    SELECT 
                        d.date_select,
                        d.model,
                        d.type_model,
                        d.ok,
                        d.ng,
                        COALESCE(e.error_particle, 0) AS error_particle,
                        COALESCE(e.error_ng_tape_position, 0) AS error_ng_tape_position,
                        COALESCE(e.error_deform, 0) AS error_deform,
                        COALESCE(e.error_scratch, 0) AS error_scratch,
                        COALESCE(e.error_dirty, 0) AS error_dirty
                    FROM DataResults d
                    LEFT JOIN ErrorCounts e ON d.model = e.model AND d.date_select = e.date_select
                    ORDER BY d.date_select asc;";

                var results = new List<ExportDataResponse>();

                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();

                    using var command = connection.CreateCommand();
                    command.CommandText = sql;

                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var dateSelect = reader.GetDateTime(reader.GetOrdinal("date_select")).ToString("dd/MM/yyyy");

                        var result = new ExportDataResponse
                        {
                            DateSelect = dateSelect,
                            Model = reader.GetString(reader.GetOrdinal("model")),
                            TypeModel = reader.GetString(reader.GetOrdinal("type_model")),
                            Ok = reader.GetInt32(reader.GetOrdinal("ok")),
                            Ng = reader.GetInt32(reader.GetOrdinal("ng")),
                            ErrorParticle = reader.GetInt32(reader.GetOrdinal("error_particle")),
                            ErrorNgTapePosition = reader.GetInt32(reader.GetOrdinal("error_ng_tape_position")),
                            ErrorDeform = reader.GetInt32(reader.GetOrdinal("error_deform")),
                            ErrorScratch = reader.GetInt32(reader.GetOrdinal("error_scratch")),
                            ErrorDirty = reader.GetInt32(reader.GetOrdinal("error_dirty"))
                        };

                        results.Add(result);
                    }
                }

                if (results.Count > 0)
                {
                    if (!Directory.Exists(Global.PATH_EXPORT_EXCEL))
                    {
                        Directory.CreateDirectory(Global.PATH_EXPORT_EXCEL);
                    }

                    string fileName = string.Empty;

                    if (fromDate == toDate)
                    {
                        fileName = $"{fromDate}.xlsx";
                    }
                    else
                    {
                        fileName = $"{fromDate}_{toDate}.xlsx";
                    }

                    string filePath = Path.Combine(Global.PATH_EXPORT_EXCEL, fileName);
                    filePath = Files.GetUniqueFilePath(filePath);

                    foreach (var item in results)
                    {
                        Files.ExportExcel(item, filePath);
                    }

                    return "success";
                }
                else
                {
                    return "Not data";
                }
            }
            catch (Exception ex)
            {
                Logs.Log($"Error can not export data, error: {ex.Message}");
                return "Not data";
            }
        }
    }
}
