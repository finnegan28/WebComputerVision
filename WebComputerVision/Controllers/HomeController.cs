using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using WebComputerVision.Models;

namespace WebComputerVision.Controllers
{
    public class HomeController : Controller
    {

        private const string apiKey = "API KEY HERE";
        private string BytesToSrcString(byte[] bytes) => "data:image/jpg;base64," + Convert.ToBase64String(bytes);

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Vision()
        {
            ViewData["Message"] = "Picture analysis";
            return View();
        }

        private string FileToImgSrcString(IFormFile file)
        {
            byte[] fileBytes;
            using (var stream = file.OpenReadStream())
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }
            }
            return BytesToSrcString(fileBytes);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vision(IFormFile file)
        {
            ViewData["originalImage"] = FileToImgSrcString(file);
            string result = null;
            using (var httpClient = new HttpClient())
            {

                string baseUri = "https://southcentralus.api.cognitive.microsoft.com/vision/v1.0/ocr";

                httpClient.BaseAddress = new Uri(baseUri);
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

                HttpContent content = new StreamContent(file.OpenReadStream());
                content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/octet-stream");

                var response = await httpClient.PostAsync(baseUri, content);

                string jsonResponse = await response.Content.ReadAsStringAsync();

                var jresult = JObject.Parse(jsonResponse);

                foreach (var region in jresult["regions"])
                {
                    foreach (var line in region["lines"])
                    {
                        foreach (var word in line["words"])
                        {
                            result = result + " " + word["text"].ToString();
                        }
                    }
                }
            }

            ViewData["result"] = result;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
