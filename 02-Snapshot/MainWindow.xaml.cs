using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Meta.Vlc.Wpf;

namespace _02_Snapshot
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

        private string _url;
        private string _libVlcPath;
        private string[] _vlcOptions;
        #endregion

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            btnGo.Click += (s, args) =>
            {
                btnGo.Content = "Loading...";
                btnGo.IsEnabled = false;
                Connect(txtUrl.Text, "admin", "your_password");
                btnGo.IsEnabled = true;
                btnGo.Content = "View";
            };

            btnSnap.Click += (s, args) =>
            {
                btnSnap.IsEnabled = false;
                var bytes = ImageSourceToBytes(new PngBitmapEncoder(), DisplayImage.Source);
                if (bytes.Length > 0)
                {
                    var stream = new MemoryStream(bytes);
                    var bm = new Bitmap(stream);

                    DebugImage(bm);
                }
                btnSnap.IsEnabled = true;
            };
        }

        public void Connect(string url, string username, string password, string libVlcPath = DEFAULT_LIBVLC_PATH, string[] vlcOptions = null)
        {
            if (Player != null && Player.IsLoaded)
            {
                Player.Stop();
            }

            string urlPrefix = "rtsp://" + username + ":" + password + "@";
            _url = url.Replace("http://", urlPrefix).Replace("https://", urlPrefix);
            _url += VIDEO_RELATIVE_URL;
            _libVlcPath = libVlcPath ?? DEFAULT_LIBVLC_PATH;
            _vlcOptions = vlcOptions ?? DEFAULT_VLC_OPTIONS;


            Player = new VlcPlayer(DisplayImage.Dispatcher)
            {
                LibVlcPath = _libVlcPath,
                VlcOption = _vlcOptions
            };
            Player.Initialize(@"..\..\libvlc", new string[] { "-I", "dummy", "--ignore-config", "--no-video-title" });
            Player.VideoSourceChanged += (sender, args) =>
            {
                DisplayImage.Dispatcher.BeginInvoke(new Action(() =>
                {
                    DisplayImage.Source = args.NewVideoSource;
                }));
            };
            Player.LoadMedia(new Uri(_url));
            Player.Play();
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
