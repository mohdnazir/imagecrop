using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JCropDemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Crop(string myimg, int x, int y, int height, int width)
        {
            string basestring = myimg.Substring(0, myimg.LastIndexOf("base64,") + 7);
            byte[] image = Convert.FromBase64String(myimg.Replace(basestring, ""));
            string imgpath = Path.GetTempFileName();
            System.IO.File.WriteAllBytes(imgpath, image);

            Rectangle cropRect = new Rectangle(new Point(x, y), new Size(width, height));
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            using (Bitmap src = Image.FromFile(imgpath) as Bitmap)
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
            }

            System.IO.File.Delete(imgpath);
            MemoryStream ms = new MemoryStream();
            target.Save(ms, GetImageFormat(basestring));
            byte[] byteImage = ms.ToArray();
            var SigBase64 = Convert.ToBase64String(byteImage); //Get Base64
            return Json(basestring + SigBase64, JsonRequestBehavior.AllowGet);
        }
        ImageFormat GetImageFormat(string base64ref)
        {
            if (base64ref.Contains("/png"))
                return ImageFormat.Png;
            if (base64ref.Contains("/gif"))
                return ImageFormat.Gif;
            if (base64ref.Contains("/jpg"))
                return ImageFormat.Jpeg;

            return ImageFormat.Jpeg;
        }

    }
}