using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;

namespace RealtyWeb.Controllers
{
    public class ImageController : Controller
    {
        //public ActionResult GetImage(string imageName)
        //{
        //    string path = ControllerContext.HttpContext.Server.MapPath(imageName);
        //    byte[] image = System.IO.File.ReadAllBytes(path);
        //    string contentType = "image/jpeg";

        //    return this.Image(image, contentType);


        //}

        public ActionResult GetImage(string imageName)
        {
            var path = ControllerContext.HttpContext.Server.MapPath(imageName);
            var extension =  imageName.Substring(imageName.LastIndexOf('.')).ToLower();
            string contentType = "";

            if (extension == ".png")
            {
                contentType = "png";
            }
            else if (extension == ".bmp")
            {
                contentType = "bmp";
            }
            else if (extension == ".gif")
            {
                contentType = "gif";
            }
            else
            {
                contentType = "jpeg";
            }
            return base.File(path, contentType);
        }

        //public HttpResponseMessage GetImage(string imageName)
        //{
        //    string path = ControllerContext.HttpContext.Server.MapPath(imageName);
        //    byte[] image = System.IO.File.ReadAllBytes(path);

        //    //return this.Image(image, contentType);

        //    //Image img = GetImage(imageName, width, height);
        //    //MemoryStream ms = new MemoryStream();
        //    //img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        //    HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
        //    result.Content = new ByteArrayContent(image);
        //    result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        //    return result;
        //}
    }

    public class ImageResult : ActionResult
    {
        public ImageResult(Stream imageStream, string contentType)
        {
            if (imageStream == null)
                throw new ArgumentNullException("imageStream");
            if (contentType == null)
                throw new ArgumentNullException("contentType");

            this.ImageStream = imageStream;
            this.ContentType = contentType;
        }

        public Stream ImageStream { get; private set; }
        public string ContentType { get; private set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            HttpResponseBase response = context.HttpContext.Response;

            response.ContentType = this.ContentType;

            byte[] buffer = new byte[4096];
            while (true)
            {
                int read = this.ImageStream.Read(buffer, 0, buffer.Length);
                if (read == 0)
                    break;

                response.OutputStream.Write(buffer, 0, read);
            }

            response.End();
        }
    }

    public static class ControllerExtensions
    {
        public static ImageResult Image(this Controller controller, Stream imageStream, string contentType)
        {
            return new ImageResult(imageStream, contentType);
        }

        public static ImageResult Image(this Controller controller, byte[] imageBytes, string contentType)
        {
            return new ImageResult(new MemoryStream(imageBytes), contentType);
        }
    }
}