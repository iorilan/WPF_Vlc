using System;
using System.Windows.Threading;
using Meta.Vlc.Wpf;

namespace _02_Snapshot
{
    public class FoscamHDVideo
    {

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
        private VlcPlayer _player;

        public VlcPlayer Player
        {
            get { return _player; }
        }

        #endregion

        #region --- Initialization ---

        public FoscamHDVideo(string url, string username, string password,ThreadSeparatedImage image, string libVlcPath = DEFAULT_LIBVLC_PATH, string[] vlcOptions = null)
        {
            string urlPrefix = "rtsp://" + username + ":" + password + "@";
            _url = url.Replace("http://", urlPrefix).Replace("https://", urlPrefix);
            _url += VIDEO_RELATIVE_URL;
            _libVlcPath = libVlcPath ?? DEFAULT_LIBVLC_PATH;
            _vlcOptions = vlcOptions ?? DEFAULT_VLC_OPTIONS;


            _player = new VlcPlayer(image.Dispatcher)
            {
                LibVlcPath = _libVlcPath,
                VlcOption = _vlcOptions
            };
            _player.VideoSourceChanged += (sender, args) =>
            {
                image.Dispatcher.BeginInvoke(DispatcherPriority.Normal,new Action(() =>
                {
                    image.Source = args.NewVideoSource;
                }));
            };

        }

        #endregion

        #region --- Properties ---

        //public UIElement VideoDisplay()
        //{
        //    if (_player == null)
        //    {
        //        _player = new VlcPlayer()
        //        {
        //            LibVlcPath = _libVlcPath,
        //            VlcOption = _vlcOptions
        //        };
        //    }
        //    return _player;
        //}

        #endregion

        #region --- Methods ---


        public void PlayVideo()
        {
            _player.LoadMedia(new Uri(_url));
            _player.Play();
        }

        public void StopVideo()
        {
            if (_player != null)
            {
                _player.Stop();
            }
        }



        #endregion

    }
}
