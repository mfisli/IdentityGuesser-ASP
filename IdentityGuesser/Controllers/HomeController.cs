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
using Microsoft.ProjectOxford.Emotion;
using System.Web.Configuration;
using System.Diagnostics;
using Microsoft.ProjectOxford.Vision.Contract;
using Microsoft.ProjectOxford.Emotion.Contract;
using IdentityGuesser.Models;
using System.Data.Entity;
using System.Net.Http;

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
            ViewBag.Message = "This is a bot that uses Microsoft Cognitive Services. You can upload a photo and the bot will guess the age, gender, and produce a caption of that it sees.";

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Upload(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {                
                // Make sure the user selected an image file
                // TODO: exception thrown if image > 4MB
                if (!file.ContentType.StartsWith("image"))
                {
                    TempData["Message"] = "Only image files may be uploaded";
                    return RedirectToAction("Index");
                }
                //if (file.ContentLength > 102400)
                //{
                //    TempData["Message"] = "Filesize of image is too large. Maximum file size permitted is " + 4 + "KB";
                //    return RedirectToAction("Index");
                //}
                // Make sure Images Dir exists on local and on deploy server! 
                System.IO.DirectoryInfo di = new DirectoryInfo(Server.MapPath("~/Images"));
                // Delete all old images
                var deleteCount = 0;
                foreach (FileInfo image in di.GetFiles())
                {
                    image.Delete();
                    deleteCount++;
                }
                string pic = System.IO.Path.GetFileName(file.FileName);
                string path = System.IO.Path.Combine(Server.MapPath("~/Images"), pic);
                // holds payloads from api calls
                var gender = string.Empty;
                var caption = string.Empty;
                var age = string.Empty;
                file.SaveAs(path);
                Debug.WriteLine("path: " + path);
                //.............................................................
                // Computer Vision service API
                var SubscriptionKey = WebConfigurationManager.AppSettings["SubscriptionKey"];
                VisionServiceClient VisionServiceClient = new VisionServiceClient(SubscriptionKey);
                using (Stream imageFileStream = System.IO.File.OpenRead(path))
                {
                    VisualFeature[] visualFeatures = new VisualFeature[]
                    {
                        VisualFeature.Faces, VisualFeature.Description
                    };
                    // vision payload 
                    AnalysisResult analysisResult = await VisionServiceClient.AnalyzeImageAsync(imageFileStream, visualFeatures);
                    if (analysisResult.Faces.Length != 0)
                    {
                        age = analysisResult.Faces[0].Age.ToString();
                        gender = analysisResult.Faces[0].Gender;
                        
                    }
                    else
                    {
                        age = "unknown";
                        gender = "unknown";
                    }
                    // Assume: there is always a caption
                    caption = analysisResult.Description.Captions[0].Text;
                    //List<string> tags = new List<string>();
                    //ViewBag.Tags = "Tags : ";
                    //for (int i = 0; i < analysisResult.Description.Tags.Length; i++)
                    //{
                    //    ViewBag.Tags += analysisResult.Description.Tags[i] + " ";
                    //}
                    ViewBag.ImagePath = "..\\Images\\" + pic;
                    ViewBag.Age = age;
                    ViewBag.Gender = gender;
                    ViewBag.Caption = caption;
                }
                //.............................................................
                // Emotion Service API
                var emotionServiceClient = new EmotionServiceClient("1829328c70be4f8aa8cb16da23ba10b2"); // should be in appsettings
                using (Stream imageFileStream = System.IO.File.OpenRead(path))
                {
                    // emotion payload
                    var emotionResult = await emotionServiceClient.RecognizeAsync(imageFileStream);
                    Dictionary<string, double> emoResultDictionary = new Dictionary<string, double>();
                    if (emotionResult.Length != 0)
                    {
                        foreach (var item in emotionResult)
                        {
                            emoResultDictionary.Add("Fear", item.Scores.Fear);
                            emoResultDictionary.Add("Contempt", item.Scores.Contempt);
                            emoResultDictionary.Add("Disgust", item.Scores.Disgust);
                            emoResultDictionary.Add("Anger", item.Scores.Anger);
                            emoResultDictionary.Add("Happiness", item.Scores.Happiness);
                            emoResultDictionary.Add("Surprise", item.Scores.Surprise);
                            emoResultDictionary.Add("Neutral", item.Scores.Neutral);
                        }
                    }
                    // get key with highest value 
                    var confidentEmotion = string.Empty;
                    if (emoResultDictionary.Count != 0)
                    {
                        confidentEmotion = emoResultDictionary.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                        ViewBag.Emotion = confidentEmotion;
                    }
                    else
                    {
                        ViewBag.Emotion = "No Emotion dectected";
                    }
                    //.............................................................
                    // bing image search API
                    // limit results to 5 for similar images based on emotion and caption 
                    List<SearchResult> result = BingWebSearcher.Search(confidentEmotion + " " + caption);
                    ViewBag.Images = new String[5];
                    for(var i = 0; i < 5 && i < result.Count; i++)
                    {
                        ViewBag.Images[i] = result[i].Link;
                    }
                }
            }
            return View("Index");
        }
    }
}