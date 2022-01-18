using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToeServer
{
    public static class ImageHelper
    {
        public static string GetImagePath(byte[] buffer)
        {
            
            ImageConverter ic = new ImageConverter();
            Image img = (Image)ic.ConvertFrom(buffer);
            Bitmap bitmap1 = new Bitmap(img);
            if (!Directory.Exists("Images"))
            {
                Directory.CreateDirectory("Images");

            }
            var id = DateTime.Now.Millisecond;
            bitmap1.Save($@"Images\image{id}.png");
            var imagepath = $@"Images\image{id}.png";
            return imagepath;
        }
        public static byte[] GetBytesOfImage(string path)
        {
            var image = new Bitmap(path);
            ImageConverter imageconverter = new ImageConverter();
            var imagebytes = ((byte[])imageconverter.ConvertTo(image, typeof(byte[])));
            return imagebytes;
        }

    }
}
