using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

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
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                // Make sure the user selected an image file
                if (!file.ContentType.StartsWith("image"))
                {
                    TempData["Message"] = "Only image files may be uploaded";
                    return RedirectToAction("Index");
                }
                ViewBag.FileName = file.FileName;
                string pic = System.IO.Path.GetFileName(file.FileName);
                string path = System.IO.Path.Combine(
                                   Server.MapPath("~/Images"), pic); 
                // file is uploaded
                file.SaveAs(path);
            }
            return View("Index");
        }
    }

}