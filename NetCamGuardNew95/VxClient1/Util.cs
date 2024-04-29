using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace VxGuardClient.Extensions
{
    public class Util
    {
        public static Image Base64ToImage(string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                Image image = Image.FromStream(ms, true);
                return image;
            }
        }
        public static string ImageToBase64(Image image, System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to base 64 string
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }
        public static Image ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.Default;
                graphics.InterpolationMode = InterpolationMode.Default;
                graphics.SmoothingMode = SmoothingMode.Default;
                graphics.PixelOffsetMode = PixelOffsetMode.Default;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        public static string compressBase64Image(string src)
        {
            try
            {
                var img = Base64ToImage(src);
                var newImg = ResizeImage(img, (int)(img.Width * 0.5), (int)(img.Height * 0.5));
                var newImgBase64 = ImageToBase64(newImg, ImageFormat.Jpeg);
                return newImgBase64;
            }
            catch
            {
                return src;
            }
        }

        public static Image compressBase64ImageToImage(string src)
        {
            var img = Base64ToImage(src);
            return img;

            //try
            //{
            //    var newImg = ResizeImage(img, (int)(img.Width * 0.5), (int)(img.Height * 0.5));
            //    return newImg;
            //}
            //catch
            //{
            //    return img;
            //}
        }
        public static string GetDateFolderName()
        {
            string date = string.Format("{0:yyyyMMdd}", DateTime.Now);
            return date;
        }

        public static bool Base64Save(string base64String, string pathFilename)
        {
            if (string.IsNullOrEmpty(base64String))
                return false;

            if (System.IO.File.Exists(pathFilename))
            {
                System.IO.File.Delete(pathFilename);
            }

            byte[] imageBytes = Convert.FromBase64String(base64String);

            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                try
                {
                    Image image = Image.FromStream(ms);
                    image.Save(pathFilename, ImageFormat.Jpeg);
                    image.Dispose();
                    ms.FlushAsync();
                    ms.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n[FUNC::Base64Save] [EXCEPTION] [{ex.Message}] pathFilename = {pathFilename}\n");
                    return false;
                }
            }
        }
    }
}
