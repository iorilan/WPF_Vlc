using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using xZune.Vlc.Wpf;

namespace GetStream
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string CAMERA_URL = "http://192.168.10.181:554/streaming/channels/401";
        private const string CAMERA_URL2 = "http://192.168.10.181:554/streaming/channels/501";
        private const string CAMERA_URL3 = "http://192.168.10.181:554/streaming/channels/801";
        //private const string CAMERA_URL4 = "http://192.168.10.181:554/streaming/channels/502";
        private const string USERNAME = "admin";
        private const string PASSWORD = "your_password";

        private IList<FoscamHDVideo> _videos = new List<FoscamHDVideo>();
        public ThreadSeparatedImage image1 = new ThreadSeparatedImage();
        public ThreadSeparatedImage image2 = new ThreadSeparatedImage();
        public ThreadSeparatedImage image3 = new ThreadSeparatedImage();
        public MainWindow()
        {
            InitializeComponent();

            //  var fullPath = Path.Combine(@"C:\Tmp\SnappedPhoto\", DateTime.Now.Ticks + ".png");
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var video = new FoscamHDVideo(CAMERA_URL, USERNAME, PASSWORD,image1);
            _videos.Add(video);
            video.Player.SetValue(Grid.RowProperty, 0);
            video.Player.SetValue(Grid.ColumnProperty, 0);
            LayoutRoot.Children.Add(video.Player);

            var video2 = new FoscamHDVideo(CAMERA_URL2, USERNAME, PASSWORD,image2);
            _videos.Add(video2);
            video2.Player.SetValue(Grid.RowProperty, 0);
            video2.Player.SetValue(Grid.ColumnProperty, 1);
            LayoutRoot.Children.Add(video2.Player);

            var video3 = new FoscamHDVideo(CAMERA_URL3, USERNAME, PASSWORD,image3);
            _videos.Add(video3);
            video3.Player.SetValue(Grid.RowProperty, 1);
            video3.Player.SetValue(Grid.ColumnProperty, 0);
            LayoutRoot.Children.Add(video3.Player);

            video.PlayVideo();
            video2.PlayVideo();
            video3.PlayVideo();
        }


        private void SaveImage(Image image, string location)
        {
            if(image == null) { return;}

            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                (int)image.Width,
                (int)image.Height,
                100, 100, PixelFormats.Default);
            renderTargetBitmap.Render(image);
            JpegBitmapEncoder jpegBitmapEncoder = new JpegBitmapEncoder();
            jpegBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            using (FileStream fileStream = new FileStream(location, FileMode.Create))
            {
                jpegBitmapEncoder.Save(fileStream);
                fileStream.Flush();
                fileStream.Close();
            }
        }




        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_videos.Count > 0)
            {
                foreach (var video in _videos)
                {
                    video.StopVideo();
                }
            }
        }
    }
}
