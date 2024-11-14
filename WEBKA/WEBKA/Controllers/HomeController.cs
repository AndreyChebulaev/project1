using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WEBKA.Models;

namespace WEBKA.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private IWebHostEnvironment hostEnv;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            hostEnv = env;

        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> Upload(IFormFile file)
        {
            string fileDic = "Files";
            string filePath = Path.Combine(hostEnv.WebRootPath, fileDic);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            string fileName = file.FileName;
            filePath = Path.Combine(filePath, fileName);
            using (FileStream fs = System.IO.File.OpenWrite(filePath))
            {
                await file.CopyToAsync(fs);

            }
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
