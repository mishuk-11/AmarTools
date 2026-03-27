using System;
using System.Collections.Generic;
using System.IO; 
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace AmrTools.Controllers
{
    public class ToolsController : Controller
    
        [HttpGet]
        public IActionResult YouTubeThumbnail()
        {
            return View("~/Views/Tools/YouTubeThumbnail.cshtml");
        }

        [HttpPost]
        public IActionResult YouTubeThumbnail(string videoUrl)
        {
            if (string.IsNullOrEmpty(videoUrl))
            {
                ViewBag.Error = "Please enter a valid YouTube URL!";
                return View("~/Views/Tools/YouTubeThumbnail.cshtml"); 
            }

            var youtubeMatch = Regex.Match(videoUrl, @"(?:youtube\.com\/(?:[^\/]+\/.+\/|(?:v|e(?:mbed)?)\/|.*[?&]v=)|youtu\.be\/)([^""&?\/\s]{11})");

            if (youtubeMatch.Success)
            {
                string videoId = youtubeMatch.Groups[1].Value;
                ViewBag.ThumbnailUrl = $"https://img.youtube.com/vi/{videoId}/maxresdefault.jpg";
                ViewBag.VideoId = videoId;
            }
            else
            {
                ViewBag.Error = "Invalid YouTube URL format!";
            }

            return View("~/Views/Tools/YouTubeThumbnail.cshtml"); /
        }

        [HttpGet]
        public IActionResult ImageResizer()
        {
            var plan = HttpContext.Session.GetString("UserPlan");
            if (plan != "Pro")
            {
                return RedirectToAction("Pricing", "Home");
            }
            return View("~/Views/Tools/ImageResizer.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> ImageResizer(IFormFile imageFile, int width, int height)
        {
            var userPlan = HttpContext.Session.GetString("UserPlan");
            if (userPlan != "Pro")
            {
                return RedirectToAction("Pricing", "Home");
            }
            
            if (imageFile == null || imageFile.Length == 0)
            {
                ViewBag.Error = "Please upload an image first!";
                return View("~/Views/Tools/ImageResizer.cshtml");
            }

            try
            {
                using var image = await Image.LoadAsync(imageFile.OpenReadStream());
                image.Mutate(x => x.Resize(width, height));

                using var ms = new MemoryStream();
                await image.SaveAsPngAsync(ms);
                byte[] imageBytes = ms.ToArray();

                ViewBag.ResizedImage = $"data:image/png;base64,{Convert.ToBase64String(imageBytes)}";
                ViewBag.Success = "Image resized successfully!";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error processing image: " + ex.Message;
            }

            return View("~/Views/Tools/ImageResizer.cshtml");
        }
       
        [HttpGet]
        public IActionResult Keywords() => View("~/Views/Tools/Keywords.cshtml");

        [HttpPost]
        public async Task<IActionResult> Keywords(string seedKeyword)
        {
            if (string.IsNullOrEmpty(seedKeyword))
            {
                ViewBag.Error = "Please enter a topic or keyword!";
                return View("~/Views/Tools/Keywords.cshtml");
            }

            try
            {
                string url = $"http://suggestqueries.google.com/complete/search?output=toolbar&hl=en&q={Uri.EscapeDataString(seedKeyword)}";

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetStringAsync(url);
                    var doc = XDocument.Parse(response);

                    var keywords = doc.Descendants("suggestion")
                                      .Select(s => s.Attribute("data")?.Value)
                                      .Where(v => v != null)
                                      .ToList();

                    ViewBag.Keywords = keywords;
                    ViewBag.Seed = seedKeyword;
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Could not fetch keywords. Please try again later.";
            }

            return View("~/Views/Tools/Keywords.cshtml");
        }


    }
}