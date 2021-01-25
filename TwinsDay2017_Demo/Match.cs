using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Neurotec.Biometrics;

namespace TwinsDay2017_Demo
{
    public class Match
    {
        public string ID { get; set; }
        public int Score { get; set; }
        public string Filename { get; set; }
        public string LargeFile { get; set; }
        public int Rank { get; set; }
        public double FAR { get; set; }
        public double Probability { get; set; }
        public BitmapImage Image { get; set; }

        public Match(string filename, string large, string id, int score, double far, double prob)
        {
            this.Filename = filename;
            this.LargeFile = large;
            this.ID = id;
            this.Score = score;
            this.FAR = far;
            this.Probability = prob;
        }

        public void GetImage()
        {
            Image = new BitmapImage();
            Image.BeginInit();
            Image.CacheOption = BitmapCacheOption.OnLoad; 

            // Images are sideways, so we have to set the height to the width and width to the height, then rotate by 90 deg
            //Image.DecodePixelHeight = 320; // 160 or 256
            //Image.DecodePixelWidth = 480; // 240 or 384
            Image.UriSource = new Uri(this.Filename);
            //Image.Rotation = Rotation.Rotate90;
            Image.EndInit();

            // Make image unmodifiable to save on performance
            if(Image.CanFreeze)
                Image.Freeze();
        }
    }
}
