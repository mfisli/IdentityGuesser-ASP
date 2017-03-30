using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Vision;
using System.Web.Configuration;
using System.Diagnostics;
using Microsoft.ProjectOxford.Vision.Contract;

namespace IdentityGuesser.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Upload(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var age = string.Empty;
                var gender = string.Empty;
                var caption = string.Empty;
                List<string> tags = new List<string>();
                // Make sure the user selected an image file
                if (!file.ContentType.StartsWith("image"))
                {
                    TempData["Message"] = "Only image files may be uploaded";
                    return RedirectToAction("Index");
                }

                string pic = System.IO.Path.GetFileName(file.FileName);
                string path = System.IO.Path.Combine(
                                   Server.MapPath("~/Images"), pic);
                // file is uploaded
                file.SaveAs(path);
                ViewBag.FileName = path;
                Debug.WriteLine("path: " + path);
                // Create Project Oxford Computer Vision API Service client
                var SubscriptionKey = WebConfigurationManager.AppSettings["SubscriptionKey"];
                VisionServiceClient VisionServiceClient = new VisionServiceClient(SubscriptionKey);
                Debug.WriteLine("VisionServiceClient is created");

                using (Stream imageFileStream = System.IO.File.OpenRead(path))
                {
                    // Analyze the image for features
                    Debug.WriteLine("Calling VisionServiceClient.AnalyzeImageAsync()...");
                    //VisualFeature.Description, VisualFeature.Faces
                    VisualFeature[] visualFeatures = new VisualFeature[] 
                    {
                        VisualFeature.Faces, VisualFeature.Description
                    };
                    AnalysisResult analysisResult = await VisionServiceClient.AnalyzeImageAsync(imageFileStream, visualFeatures);
                    age = analysisResult.Faces[0].Age.ToString();
                    gender = analysisResult.Faces[0].Gender;
                    caption = analysisResult.Description.Captions[0].Text;
                    for (int i = 0; i < analysisResult.Description.Tags.Length; i++)
                    {
                        tags.Add(analysisResult.Description.Tags[i]);
                    }
                }
                
                string tempTags = string.Empty;
                foreach(string tag in tags)
                {
                    tempTags += tag + " ";
                }
                ViewBag.Caption = "Age: " + age + " Gender: " + gender + " Caption: " + caption + " Tags: " + tempTags;


            }
            return View("Index");
        }
    }
}