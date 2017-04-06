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
using IdentityGuesser.Models;
using System.Data.Entity;

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
            ViewBag.Message = "This is a bot that uses Microsoft Cognitive Services. You can upload a photo and the bot will guess the age, gender, and a caption of that it sees.";

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Upload(HttpPostedFileBase file)
        {
            //PoemModel poemModel = new PoemModel();
            //PoemContext db = new PoemContext();
            if (file != null && file.ContentLength > 0)
            {
                
                // Make sure the user selected an image file
                if (!file.ContentType.StartsWith("image"))
                {
                    TempData["Message"] = "Only image files may be uploaded";
                    return RedirectToAction("Index");
                }

                System.IO.DirectoryInfo di = new DirectoryInfo(Server.MapPath("~/Images"));

                var deleteCount = 0;
                foreach (FileInfo image in di.GetFiles())
                {
                    image.Delete();
                    deleteCount++;
                }
                Debug.WriteLine("Images deleted: " + deleteCount);

                string pic = System.IO.Path.GetFileName(file.FileName);
                string path = System.IO.Path.Combine(
                                   Server.MapPath("~/Images"), pic);
                // file is uploaded
                file.SaveAs(path);
                //ViewBag.FileName = path;
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
                    var age = analysisResult.Faces[0].Age.ToString();
                    var gender = analysisResult.Faces[0].Gender;
                    var caption = analysisResult.Description.Captions[0].Text;
                    List<string> tags = new List<string>();
                    ViewBag.Tags = "Tags : "; 
                    for (int i = 0; i < analysisResult.Description.Tags.Length; i++)
                    {
                        ViewBag.Tags += analysisResult.Description.Tags[i] + " ";
                    }
                    ViewBag.ImagePath = "..\\Images\\" + pic;
                    ViewBag.Age = "Age: " + age;
                    ViewBag.Gender = "Gender : " + gender;
                    ViewBag.Caption = "Caption : " + caption;                
                }

            }
            return View("Index");
        }
    }
}