using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Common
{
    public partial class CommonBase
    {
        public static string ImgToBase64String(string Imagefilename)
        {
            try
            {
                FileInfo file = new FileInfo(Imagefilename);
                var stream = file.OpenRead();
                byte[] buffer = new byte[file.Length];
                stream.Read(buffer, 0, Convert.ToInt32(file.Length));
                string base64String = Convert.ToBase64String(buffer);
                return base64String;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 截取一张图片的指定部分
        /// </summary>
        /// <param name="bitmapPathAndName">原始图片路径名称</param>
        /// <param name="width">截取图片的宽度</param>
        /// <param name="height">截取图片的高度</param>
        /// <param name="offsetX">开始截取图片的X坐标</param>
        /// <param name="offsetY">开始截取图片的Y坐标</param>
        /// <returns></returns>
        public static Bitmap GetPartOfImageRec(Bitmap sourceBitmap, int width, int height, int offsetX, int offsetY)
        {
            //Bitmap sourceBitmap = new Bitmap(bitmapPathAndName);
            Bitmap resultBitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resultBitmap))
            {
                Rectangle resultRectangle = new Rectangle(0, 0, width, height);
                Rectangle sourceRectangle = new Rectangle(0 + offsetX, 0 + offsetY, width, height);
                g.DrawImage(sourceBitmap, resultRectangle, sourceRectangle, GraphicsUnit.Pixel);
            }
            return resultBitmap;
        }
    }

}
