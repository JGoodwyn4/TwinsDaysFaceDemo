using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Windows.Shapes;
//using System.Threading;

namespace TwinsDay2017_Demo
{
    /// <summary>
    /// Interaction logic for ImageDisplay.xaml
    /// </summary>
    public partial class ImageDisplay : Window
    {
        public ImageDisplay(string filename, string RID)
        {
            BitmapImage display = new BitmapImage();
            //RenderOptions.SetBitmapScalingMode(display, BitmapScalingMode.LowQuality);
            display.BeginInit();
            //display.DecodePixelHeight = 1920;
            //display.DecodePixelWidth = 1280;
            display.UriSource = new Uri(filename);
            display.CacheOption = BitmapCacheOption.OnLoad;
            display.EndInit();
            
            if(display.CanFreeze)
                display.Freeze();

            InitializeComponent();

            this.Title = string.Format("Result Image ({0})", RID);
            ShowImage.Source = display;
        }
    }
}
