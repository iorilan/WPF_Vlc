using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Meta.Vlc.Wpf;

namespace _03_MultipleSnap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        #region --- Constants ---

        private const string VIDEO_RELATIVE_URL = "/videoMain";
        private const string DEFAULT_LIBVLC_PATH = "../../LibVlc";
        private readonly string[] DEFAULT_VLC_OPTIONS = new string[]
        {
            "-I dummy",
            "--ignore-config",
            "--no-video-title",
            "--file-logging",
            "--logfile=log.txt",
            "--verbose=2",
            "--no-sub-autodetect-file",
            //"--rtsp-tcp", //needed to pass RTSP through a VPN
            //"--rtsp-frame-buffer-size=500000", //needed to avoid Live555 error when using --rtsp-tcp (RTCPInstance error: Hit limit when reading incoming packet over TCP. Increase "maxRTCPPacketSize")
            "--network-caching=500" //caching value for network resources in msec (needed for low frame lag - if broken frames need to increase it)
        };

        /* Other maybe useful parameters from https://wiki.videolan.org/VLC_command-line_help

          --rtsp-kasenna, --no-rtsp-kasenna
                                     Kasenna RTSP dialect(default disabled)
              Kasenna servers use an old and nonstandard dialect of RTSP.With this
              parameter VLC will try this dialect, but then it cannot connect to
              normal RTSP servers. (default disabled)
          --rtsp-wmserver, --no-rtsp-wmserver
                                     WMServer RTSP dialect(default disabled)
              WMServer uses a nonstandard dialect of RTSP.Selecting this parameter
              will tell VLC to assume some options contrary to RFC 2326 guidelines.
              (default disabled)
        */

        #endregion

        #region --- Fields ---

        private string _url1;
        private string _url2;
        private string _url3;
        private string _libVlcPath;
        private string[] _vlcOptions;
        #endregion


        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _libVlcPath = DEFAULT_LIBVLC_PATH;
            _vlcOptions = DEFAULT_VLC_OPTIONS;

            _url1 = "http://192.168.10.181:554/streaming/channels/401";
            _url2 = "http://192.168.10.181:554/streaming/channels/501";
            _url3 = "http://192.168.10.181:554/streaming/channels/801";
            InitVideoSource(ref _url1, "admin","your_password");
            InitVideoSource(ref _url2, "admin", "your_password");
            InitVideoSource(ref _url3, "admin", "your_password");

            InitPlayer(Player1, DisplayImage1,_url1);
            InitPlayer(Player2, DisplayImage2, _url2);
            InitPlayer(Player3, DisplayImage3, _url3);

            btnSnap1.Click += (s, args) =>
            {
                btnSnap1.IsEnabled = false;
                var bytes = ImageSourceToBytes(new PngBitmapEncoder(), DisplayImage1.Source);
                if (bytes.Length > 0)
                {
                    var stream = new MemoryStream(bytes);
                    var bm = new Bitmap(stream);

                    DebugImage(bm);
                }
                btnSnap1.IsEnabled = true;
            };
            btnSnap2.Click += (s, args) =>
            {
                btnSnap2.IsEnabled = false;
                var bytes = ImageSourceToBytes(new PngBitmapEncoder(), DisplayImage2.Source);
                if (bytes.Length > 0)
                {
                    var stream = new MemoryStream(bytes);
                    var bm = new Bitmap(stream);

                    DebugImage(bm);
                }
                btnSnap2.IsEnabled = true;
            };
            btnSnap3.Click += (s, args) =>
            {
                btnSnap3.IsEnabled = false;
                var bytes = ImageSourceToBytes(new PngBitmapEncoder(), DisplayImage3.Source);
                if (bytes.Length > 0)
                {
                    var stream = new MemoryStream(bytes);
                    var bm = new Bitmap(stream);

                    DebugImage(bm);
                }
                btnSnap3.IsEnabled = true;
            };
        }

        public void InitVideoSource(ref string url, string username, string password)
        {
            string urlPrefix = "rtsp://" + username + ":" + password + "@";
            var u = url.Replace("http://", urlPrefix).Replace("https://", urlPrefix);
            u += VIDEO_RELATIVE_URL;

            url = u;
        }

        public void InitPlayer(VlcPlayer player, System.Windows.Controls.Image image, string url)
        {
            if (player != null && player.IsLoaded)
            {
                player.Stop();
            }

            player = new VlcPlayer(image.Dispatcher)
            {
                LibVlcPath = _libVlcPath,
                VlcOption = _vlcOptions
            };
            player.Initialize(@"..\..\libvlc", new string[] { "-I", "dummy", "--ignore-config", "--no-video-title" });
            player.VideoSourceChanged += (sender, args) =>
            {
                image.Dispatcher.BeginInvoke(new Action(() =>
                {
                    image.Source = args.NewVideoSource;
                }));
            };
            player.LoadMedia(new Uri(url));
            player.Play();
        }

        public static void DebugImage(Bitmap bmp)
        {
            var img = new Image<Bgr, byte>(bmp);
            ImageViewer.Show(img);
        }
        public byte[] ImageSourceToBytes(BitmapEncoder encoder, ImageSource imageSource)
        {
            byte[] bytes = null;
            var bitmapSource = imageSource as BitmapSource;

            if (bitmapSource != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }

            return bytes;
        }

    }
}
