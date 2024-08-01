using Bend_PSA.Context;
using Bend_PSA.Services;
using Bend_PSA.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OfficeOpenXml;
using Stiffiner_Inspection.Hubs;

namespace Bend_PSA
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddSignalR();

            //license epplus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"), sqlServerOptionsAction =>
                {
                    sqlServerOptionsAction.EnableRetryOnFailure();
                })
            );

            builder.Services.AddScoped<DataService>();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Data", Version = "v1" });
            });

            var app = builder.Build();

            //CONNECT PLC
            Global.plc.ConnectPLC();

            using (var scope = app.Services.CreateScope())
            {
                var dataService = scope.ServiceProvider.GetRequiredService<DataService>();

                //THREAD DATA SYNTHESIS SEND TO PLC
                Thread threadDataSynthesis = new(async () => await dataService.DataSynthesis())
                {
                    Name = "THREAD_SYNTHESIS_DATA_SEND_TO_PLC",
                    IsBackground = true
                };
                threadDataSynthesis.Start();

                //THREAD AUTO DELETE IMAGE DOWNLOAD
                Thread thAutoDeleteImgDownload = new(async () => await dataService.AutoDeleteImageDownload())
                {
                    Name = "THREAD_AUTO_DELETE_IMAGE_DOWNLOAD",
                    IsBackground = true
                };
                thAutoDeleteImgDownload.Start();

                //THREAD AUTO DELETE DATA OLDER THAN 3 MONTH
                Thread thAutoDeleteDataOlder3Month = new(async () => await dataService.AutoDeleteDataOlderThan3Month())
                {
                    Name = "THREAD_AUTO_DELETE_DATA_OLDER_3_MONTH",
                    IsBackground = true
                };
                thAutoDeleteDataOlder3Month.Start();

                //THREAD CHECK STATUS VISION IS BUSY OR NOT
                Thread thCheckStatusVisionBusy = new(async () => await dataService.CheckStatusVisionBusy())
                {
                    Name = "THREAD_CHECK_STATUS_VISION_BUSY",
                    IsBackground = true
                };
                thCheckStatusVisionBusy.Start();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "data");
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.MapHub<HomeHub>("/homeHub");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
