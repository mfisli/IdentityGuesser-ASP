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
                // Make sure Images Dir exists on local and on deploy server! 
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
                var gender = string.Empty;
                var caption = string.Empty;
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



                    //payload 
                    AnalysisResult analysisResult = await VisionServiceClient.AnalyzeImageAsync(imageFileStream, visualFeatures);
                    var age = analysisResult.Faces[0].Age.ToString();
                    gender = analysisResult.Faces[0].Gender;
                    caption = analysisResult.Description.Captions[0].Text;
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
                using (Stream imageFileStream = System.IO.File.OpenRead(path))
                {

                    var emotionServiceClient = new EmotionServiceClient("1829328c70be4f8aa8cb16da23ba10b2");
                    var emotionResult = await emotionServiceClient.RecognizeAsync(imageFileStream);
                    // find most confident emotion dectection
                    Dictionary<string, double> emoResultDictionary = new Dictionary<string, double>();
                    if (emotionResult.Length != 0)
                    {
                        foreach (var item in emotionResult)
                        {
                            Debug.WriteLine("____________________ Fear: " + item.Scores.Fear);
                            emoResultDictionary.Add("Fear", item.Scores.Fear);
                            Debug.WriteLine("____________________ Contempt: " + item.Scores.Contempt);
                            emoResultDictionary.Add("Contempt", item.Scores.Contempt);
                            Debug.WriteLine("____________________ Disgust: " + item.Scores.Disgust);
                            emoResultDictionary.Add("Disgust", item.Scores.Disgust);
                            Debug.WriteLine("____________________ Anger: " + item.Scores.Anger);
                            emoResultDictionary.Add("Anger", item.Scores.Anger);
                            Debug.WriteLine("____________________ Happiness: " + item.Scores.Happiness);
                            emoResultDictionary.Add("Happiness", item.Scores.Happiness);
                            Debug.WriteLine("____________________ Surprise: " + item.Scores.Surprise);
                            emoResultDictionary.Add("Surprise", item.Scores.Surprise);
                            Debug.WriteLine("____________________ Neutral: " + item.Scores.Neutral);
                            emoResultDictionary.Add("Neutral", item.Scores.Neutral);
                        }
                    }
                    var confidentEmotion = emoResultDictionary.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                    Debug.WriteLine("_________ Most Confident: " + confidentEmotion + " at " + emoResultDictionary.Values.Max());
                    List<SearchResult> result = BingWebSearcher.Search(confidentEmotion + " " + caption);
                    //result.Name: the name of the search result
                    //result.Link: the link of the search result
                    //ViewBag.Message = result[0].Link;
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