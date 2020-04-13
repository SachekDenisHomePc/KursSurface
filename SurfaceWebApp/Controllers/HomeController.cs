using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Google.DataTable.Net.Wrapper;
using Google.DataTable.Net.Wrapper.Extension;
using KursSurface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SurfaceWebApp.Models;

namespace SurfaceWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _hostEnvironment;

        public HomeController(ILogger<HomeController> logger,
            IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CalculateSurface(SurfaceData surfaceData)
        {
            ViewBag.SimpsonResult = 0;
            DataStorage.SurfaceData.Expression = surfaceData.Expression;
            DataStorage.SurfaceData.YStart = surfaceData.YStart;
            DataStorage.SurfaceData.YEnd = surfaceData.YEnd;
            DataStorage.SurfaceData.XStart = surfaceData.XStart;
            DataStorage.SurfaceData.XEnd = surfaceData.XEnd;


            var viewModel = new SurfaceViewModel();
            //viewModel.ThreadsTimeJson = json;

            return View("Index", viewModel);
        }

        public IActionResult GetChartData()
        {
            var surfaceData = DataStorage.SurfaceData;

            string expression = surfaceData.Expression;

            var surface = new Surface(expression);

            var integrationArgumentsInfo = new DoubleIntegrationInfo
            {
                XStart = surfaceData.XStart,
                XEnd = surfaceData.XEnd,
                YStart = surfaceData.YStart,
                YEnd = surfaceData.YEnd
            };

            var numericalIntegration = new NumericalIntegration();

            var integrationInfo = numericalIntegration.CalculateBySimpsonMethod(integrationArgumentsInfo, surface.CalculateSurfaceFunction);

            DataStorage.SurfaceData.SimpsonResult = integrationInfo.IntegrationResult;

            var json = integrationInfo.ThreadsTime.Select(item => new { ThreadsNumber = item.Key, Time = item.Value.TotalSeconds }).ToGoogleDataTable()
                .NewColumn(new Column(ColumnType.String, "Threads Count"), x => x.ThreadsNumber)
                .NewColumn(new Column(ColumnType.Number, "Time"), x => x.Time)
                .Build()
                .GetJson();

            return Content(json);
        }

        public IActionResult GetSimpsonResult()
        {
            return Json(JsonSerializer.Serialize(DataStorage.SurfaceData.SimpsonResult));
        }

        public IActionResult GetPythonResult()
        {
            var jsonPythonData = JsonSerializer.Serialize(DataStorage.SurfaceData);

            using (var streamWriter = new StreamWriter("PythonScript\\pythonData.json"))
            {
                streamWriter.Write(jsonPythonData);
            }

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "D:\\Python\\Python37_64\\python.exe";
            start.Arguments = Path.Combine(_hostEnvironment.ContentRootPath, "PythonScript", "PythonSurface.py");
            start.RedirectStandardOutput = true;
            start.UseShellExecute = false;

            Process process = Process.Start(start);

            process.WaitForExit();

            return Json(null);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
