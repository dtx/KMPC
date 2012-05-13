using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Xml;
using Kinect.Toolbox;
using Kinect.Toolbox.Gestures;
using Kinect.Toolbox.Record;
using Microsoft.Kinect;

namespace KMPC
{
    public delegate bool CallBack(int hwnd, int lParam);

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Sensor and gesture detection variables
        private KinectSensor _kinectSensor;
        private Skeleton[] _skeletons;
        private GestureSuite _gestureDetector;
        //helps stabilize skeleton
        readonly BarycenterHelper barycenterHelper = new BarycenterHelper();
        
        private int _currentIndex = 0;
        [DllImport("user32", EntryPoint = "EnumWindows")]
        public static extern int EnumWindows(CallBack x, int y);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd,
           StringBuilder lpClassName,
           int nMaxCount
        );

        // Get a handle to an application window.
        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName,
            string lpWindowName);

        /// <summary>
        /// Callback function. This function will be called for every open window found.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="lParam"></param>
        public static bool Report(int hwnd, int lParam)
        {
            if (IsWindowVisible((IntPtr)hwnd))
            {
                StringBuilder buffer = new StringBuilder(128);
                GetClassName((IntPtr)hwnd, buffer, buffer.Capacity);

                try
                {
                    XmlReader reader = XmlReader.Create("data.xml");

                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "class_name" && buffer.ToString().Equals(reader.ReadElementContentAsString()))
                        {
                            ControlFunction.playerHandle = (IntPtr)hwnd;
                            reader.ReadToFollowing("play");
                            Parameter.play = reader.ReadElementContentAsString();
                            reader.ReadToFollowing("stop");
                            Parameter.stop = reader.ReadElementContentAsString();
                            reader.ReadToFollowing("full");
                            Parameter.full = reader.ReadElementContentAsString();
                            reader.ReadToFollowing("fwd");
                            Parameter.fwd = reader.ReadElementContentAsString();
                            reader.ReadToFollowing("bwd");
                            Parameter.bwd = reader.ReadElementContentAsString();
                            reader.ReadToFollowing("mute");
                            Parameter.mute = reader.ReadElementContentAsString();
                            reader.ReadToFollowing("vup");
                            Parameter.vup = reader.ReadElementContentAsString();
                            reader.ReadToFollowing("vdown");
                            Parameter.vdown = reader.ReadElementContentAsString();
                            reader.Close();
                            return false;
                        }
                    }
                    reader.Close();
                }
                catch (FileNotFoundException)
                {
                    return true;
                }
            }
            return true;
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //listen to any status change for Kinects
            KinectSensor.KinectSensors.StatusChanged += Kinects_StatusChanged;

            //loop through all the Kinects attached to this PC, and start the first that is connected without an error.
            foreach (KinectSensor kinect in KinectSensor.KinectSensors)
            {
                if (kinect.Status == KinectStatus.Connected)
                {
                    _kinectSensor = kinect;
                    break;
                }
            }

            if (KinectSensor.KinectSensors.Count == 0)
                System.Windows.MessageBox.Show("No Kinect found");
            else
                Initialize();


            CheckOpenedPlayer();

            if (ControlFunction.playerHandle == IntPtr.Zero)
            {
                System.Windows.Forms.MessageBox.Show("Media Player is not running.");
            }
        }

        /// <summary>
        /// Initialize kinect sensor stuff here. Called when a kinect is
        /// detected
        /// </summary>
        private void Initialize()
        {
            if (_kinectSensor == null)
                return;
            //initailize kinectSensor streams
            _kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            _kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            _kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters
            {
                Smoothing = 0.5f,
                Correction = 0.5f,
                Prediction = 0.5f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.04f
            });
            //add delegate for skeleton detection
            _kinectSensor.SkeletonFrameReady += kinectRuntime_SkeletonFrameReady;
            _gestureDetector = new GestureSuite();
            _gestureDetector.OnGestureDetected += ParseGesture;
            _gestureDetector.OnGestureDetected += UpdateGestureList;
            _kinectSensor.Start();
        }

        /// <summary>
        /// Called when a new frame is ready to be analyzed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void kinectRuntime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame == null)
                    return;

                
                Tools.GetSkeletons(frame, ref _skeletons);

                if (_skeletons.All(s => s.TrackingState == SkeletonTrackingState.NotTracked))
                    return;
                
                ProcessFrame(frame);
            }
        }

        /// <summary>
        /// Determines if the frame is stable, if so process it
        /// </summary>
        /// <param name="frame"></param>
        void ProcessFrame(ReplaySkeletonFrame frame)
        {
            Dictionary<int, string> stabilities = new Dictionary<int, string>();
            foreach (var skeleton in frame.Skeletons)
            {
                if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                    continue;
                _gestureDetector.Add(skeleton, _kinectSensor);
                barycenterHelper.Add(skeleton.Position.ToVector3(), skeleton.TrackingId);
                if (!barycenterHelper.IsStable(skeleton.TrackingId))
                    return;
            }
        
        }

        /// <summary>
        /// Called when kinect is disconnected
        /// </summary>
        private void Clean()
        {
        }

        /// <summary>
        /// Check if there is a recorded media player which is open now 
        /// </summary>
        private int CheckOpenedPlayer()
        {
            CallBack myCallBack = new CallBack(Report);
            return EnumWindows(myCallBack, 0);
        }

        /// <summary>
        /// Detect kinect state changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Kinects_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (_kinectSensor == null)
                    {
                        _kinectSensor = e.Sensor;
                        Initialize();
                    }
                    break;
                case KinectStatus.Disconnected:
                    if (_kinectSensor == e.Sensor)
                    {
                        Clean();
                        System.Windows.MessageBox.Show("Kinect was disconnected");
                    }
                    break;
                case KinectStatus.NotReady:
                    break;
                case KinectStatus.NotPowered:
                    if (_kinectSensor == e.Sensor)
                    {
                        Clean();
                        System.Windows.MessageBox.Show("Kinect is no more powered");
                    }
                    break;
                default:
                    System.Windows.MessageBox.Show("Unhandled Status: " + e.Status);
                    break;
            }
        }

        /// <summary>
        /// Stops the kinect sensing
        /// </summary>
        void StopKinect()
        {
            if (_kinectSensor != null)
            {
                _kinectSensor.Stop();
                _kinectSensor.AudioSource.Stop();
                Clean();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopKinect();
        }

        /// <summary>
        /// Open a form to add/edit shortcut information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addPlayerShortcuts_Click(object sender, RoutedEventArgs e)
        {
            PlayerShortcutForm psForm = new PlayerShortcutForm();
            psForm.FormClosed += new FormClosedEventHandler(psForm_FormClosed);
            psForm.Show();
        }

        /// <summary>
        /// Check for opened media player after shortcut information form is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void psForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            CheckOpenedPlayer();
            if (ControlFunction.playerHandle == IntPtr.Zero)
            {
                System.Windows.Forms.MessageBox.Show("Media Player is not running.");
            }
        }

        /// <summary>
        /// Converts the gestures into media player controls
        /// </summary>
        /// <param name="gesture"></param>
        void ParseGesture(string gesture)
        {            
            if (gesture == KnownGestures.SwipeRight.ToString())
            {
                ControlFunction.Execute(Parameter.play);
            }
            else if (gesture == KnownGestures.SwipeLeft.ToString())
            {
                ControlFunction.Execute(Parameter.mute);
            }
            else if (gesture == KnownGestures.SwipeUp.ToString())
            {
                ControlFunction.Execute(Parameter.vup);
            }
            else if (gesture == KnownGestures.SwipeDown.ToString())
            {
                ControlFunction.Execute(Parameter.vdown);
            }
            else if (gesture == KnownGestures.Clockwise.ToString())
            {
                ControlFunction.Execute(Parameter.fwd);
            }
            else if (gesture == KnownGestures.CounterClockwise.ToString())
            {
                ControlFunction.Execute(Parameter.bwd);
            }
            else if (gesture == KnownGestures.HandsOpened.ToString())
            {
                ControlFunction.Execute(Parameter.full);
            }
            else if (gesture == KnownGestures.HandsClosed.ToString())
            {
                ControlFunction.Execute(Parameter.stop);
            }
        }
        /// <summary>
        /// Updates the gesture list in the GUI
        /// </summary>
        /// <param name="gesture"></param>
        void UpdateGestureList(string gesture)
        {
            int pos = gestureList.Items.Add(string.Format("{0} : {1}", gesture, DateTime.Now));
            gestureList.SelectedIndex = pos;
        }
    }
}
