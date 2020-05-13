using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
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
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            ViewBag.SurfaceData = new SurfaceData
                                  {
                                      Expression = "-2*x^2+(-2*y^2)+(-3*sin(x)*cos(y))",
                                      XStart = -100,
                                      XEnd = 100,
                                      YEnd = 20,
                                      YStart = -20
                                  };

            return View();
        }

        public IActionResult CalculateSurface(SurfaceData surfaceData)
        {
            if (surfaceData.XStart >= surfaceData.XEnd)
            {
                ModelState.AddModelError("XStart", "XEnd must be greater than XStart");
                ModelState.AddModelError("XEnd", "XEnd must be greater than XStart");
            }

            if (surfaceData.YStart >= surfaceData.YEnd)
            {
                ModelState.AddModelError("YStart", "YEnd must be greater than XStart");
                ModelState.AddModelError("YEnd", "YEnd must be greater than XStart");
            }


            ViewBag.SimpsonResult = 0;
            ViewBag.SurfaceData = surfaceData;

            DataStorage.SurfaceData.Expression = surfaceData.Expression;
            DataStorage.SurfaceData.YStart = surfaceData.YStart;
            DataStorage.SurfaceData.YEnd = surfaceData.YEnd;
            DataStorage.SurfaceData.XStart = surfaceData.XStart;
            DataStorage.SurfaceData.XEnd = surfaceData.XEnd;

            if (!ModelState.IsValid)
            {
                return View("Index", surfaceData);
            }

            return View("Index", surfaceData);
        }

        public IActionResult GetChartData()
        {
            var excelWriter = new ExcelWriter();

            var surfaceData = DataStorage.SurfaceData;

            var expression = surfaceData.Expression;

            try
            {
                Surface surface;

                surface = new Surface(expression);

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

                excelWriter.WriteThreadsDataToExcel(integrationInfo.ThreadsTime);

                var json = integrationInfo.ThreadsTime.Select(item => new {ThreadsNumber = item.Key, Time = item.Value.TotalSeconds})
                                          .ToGoogleDataTable()
                                          .NewColumn(new Column(ColumnType.String, "Threads Count"), x => x.ThreadsNumber)
                                          .NewColumn(new Column(ColumnType.Number, "Time"), x => x.Time)
                                          .Build()
                                          .GetJson();
                return Content(json);
            }
            catch(Exception exception)
            {
                return Error();
            }
        }

        public IActionResult GetSimpsonResult()
        {
            return Json(JsonSerializer.Serialize(Math.Round(DataStorage.SurfaceData.SimpsonResult, 5)));
        }

        public async Task<IActionResult> GetPythonResult()
        {
            var jsonPythonData = JsonSerializer.Serialize(DataStorage.SurfaceData);

            using (var writer = new StreamWriter("transferData.json"))
            {
                writer.Write(jsonPythonData);
            }

            var start = new ProcessStartInfo();
            start.FileName = "D:\\Python\\Python37_64\\python.exe";
            start.Arguments = Path.Combine(_hostEnvironment.ContentRootPath, "PythonScript", "PythonSurface.py");
            start.RedirectStandardOutput = true;
            start.UseShellExecute = false;

            var process = Process.Start(start);

            var pipeServer = new NamedPipeServerStream("pythonPipe", PipeDirection.InOut, 10, PipeTransmissionMode.Message);

            pipeServer.WaitForConnection();

            var encoder = new UnicodeEncoding();
            var outBuffer = encoder.GetBytes(jsonPythonData);
            pipeServer.Write(outBuffer);
            pipeServer.Close();

            var line = await process.StandardOutput.ReadLineAsync();

            process.WaitForExit();

            if (line == null)
            {
                return Error();
            }

            return Json(JsonSerializer.Serialize(Math.Round(double.Parse(line, CultureInfo.InvariantCulture), 4)));
        }

        public IActionResult GetGraphImage()
        {
            return PartialView("PartialViews/GraphPartialView");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}