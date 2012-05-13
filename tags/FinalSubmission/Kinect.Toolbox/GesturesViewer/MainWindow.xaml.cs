using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Kinect.Toolbox;
using Kinect.Toolbox.Record;
using System.IO;
using Microsoft.Kinect;
using Microsoft.Win32;
using Kinect.Toolbox.Voice;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;
using KMPC;
using Kinect.Toolbox.Gestures;
namespace GesturesViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        KinectSensor _kinectSensor;
        GestureSuite _gestureSuite;
        readonly ColorStreamManager colorManager = new ColorStreamManager();
        readonly DepthStreamManager depthManager = new DepthStreamManager();
        AudioStreamManager audioManager;
        SkeletonDisplayManager skeletonDisplayManager;
        readonly BarycenterHelper barycenterHelper = new BarycenterHelper();
        bool displayDepth;

        private string testFile = System.IO.Path.Combine(Environment.CurrentDirectory, @"data\test1.replay");
        private KnownGestures[] testSet1 = new KnownGestures[] { KnownGestures.SwipeDown, KnownGestures.SwipeUp, KnownGestures.SwipeLeft, 
            KnownGestures.SwipeRight, KnownGestures.Clockwise};
        string letterT_KBPath;
        
        SkeletonRecorder recorder;
        SkeletonReplay replay;

        BindableNUICamera nuiCamera;

        private Skeleton[] skeletons;

        VoiceCommander voiceCommander;

        public MainWindow()
        {
            InitializeComponent();
        }

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            letterT_KBPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"data\t_KB.save");

            try
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

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            CheckOpenedPlayer();

            if (ControlFunction.playerHandle == IntPtr.Zero)
            {
                System.Windows.Forms.MessageBox.Show("Media Player is not running.");
                //Exit();
            }
        }

        private void Initialize()
        {
            if (_kinectSensor == null)
                return;

            audioManager = new AudioStreamManager(_kinectSensor.AudioSource);
            audioBeamAngle.DataContext = audioManager;

            _kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            _kinectSensor.ColorFrameReady += kinectRuntime_ColorFrameReady;

            _kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            _kinectSensor.DepthFrameReady += kinectSensor_DepthFrameReady;

            _kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters
                                                   {
                                                 Smoothing = 0.5f,
                                                 Correction = 0.5f,
                                                 Prediction = 0.5f,
                                                 JitterRadius = 0.05f,
                                                 MaxDeviationRadius = 0.04f
                                             });
            _kinectSensor.SkeletonFrameReady += kinectRuntime_SkeletonFrameReady;
            _gestureSuite = new GestureSuite();
            _gestureSuite.OnGestureDetected += OnGestureDetected;
            _gestureSuite.OnGestureDetected += UpdateGestureList;
            skeletonDisplayManager = new SkeletonDisplayManager(_kinectSensor, kinectCanvas);

            _kinectSensor.Start();

            LoadLetterTPostureDetector();

            nuiCamera = new BindableNUICamera(_kinectSensor);

            elevationSlider.DataContext = nuiCamera;

            voiceCommander = new VoiceCommander("record", "stop");
            voiceCommander.OrderDetected += voiceCommander_OrderDetected;

            StartVoiceCommander();

            kinectDisplay.DataContext = colorManager;
        }

        void kinectSensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            if (!displayDepth)
                return;

            using (var frame = e.OpenDepthImageFrame())
            {
                if (frame == null)
                    return;

                depthManager.Update(frame);
            }
        }

        void kinectRuntime_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            if (displayDepth)
                return;

            using (var frame = e.OpenColorImageFrame())
            {
                if (frame == null)
                    return;

                colorManager.Update(frame);
            }
        }

        void kinectRuntime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame == null)
                    return;

                if (recorder != null)
                    recorder.Record(frame);

                Tools.GetSkeletons(frame, ref skeletons);

                if (skeletons.All(s => s.TrackingState == SkeletonTrackingState.NotTracked))
                    return;

                ProcessFrame(frame);
            }
        }

        void ProcessFrame(ReplaySkeletonFrame frame)
        {
            Dictionary<int, string> stabilities = new Dictionary<int, string>();
            foreach (var skeleton in frame.Skeletons)
            {
                if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                    continue;

                barycenterHelper.Add(skeleton.Position.ToVector3(), skeleton.TrackingId);
                if (!barycenterHelper.IsStable(skeleton.TrackingId))
                    return;
                _gestureSuite.Add(skeleton, _kinectSensor);
            }

            skeletonDisplayManager.Draw(frame);

            stabilitiesList.ItemsSource = stabilities;

          
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Clean();
        }

        private void Clean()
        {
            if (_gestureSuite != null)
            {
                _gestureSuite.OnGestureDetected -= OnGestureDetected;
                _gestureSuite.OnGestureDetected -= UpdateGestureList;
            }

            if (audioManager != null)
            {
                audioManager.Dispose();
                audioManager = null;
            }

            
            ClosePostureDetector();

            if (voiceCommander != null)
            {
                voiceCommander.OrderDetected -= voiceCommander_OrderDetected;
                voiceCommander.Dispose();
                voiceCommander = null;
            }

            if (recorder != null)
            {
                recorder.Stop();
                recorder = null;
            }

            if (_kinectSensor != null)
            {
                _kinectSensor.ColorFrameReady -= kinectRuntime_ColorFrameReady;
                _kinectSensor.SkeletonFrameReady -= kinectRuntime_SkeletonFrameReady;
                _kinectSensor.ColorFrameReady -= kinectRuntime_ColorFrameReady;
                _kinectSensor.Stop();
                _kinectSensor = null;
            }
        }

        private void replayButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog { Title = "Select filename", Filter = "Replay files|*.replay" };

            if (openFileDialog.ShowDialog() == true)
            {
                if (replay != null)
                {
                    replay.SkeletonFrameReady -= replay_SkeletonFrameReady;
                    replay.Stop();
                }
                Stream recordStream = File.OpenRead(openFileDialog.FileName);

                replay = new SkeletonReplay(recordStream);

                replay.SkeletonFrameReady += replay_SkeletonFrameReady;

                replay.Start();
            }
        }

        void replay_SkeletonFrameReady(object sender, ReplaySkeletonFrameReadyEventArgs e)
        {
            ProcessFrame(e.SkeletonFrame);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            displayDepth = !displayDepth;

            if (displayDepth)
            {
                viewButton.Content = "View Color";
                kinectDisplay.DataContext = depthManager;
            }
            else
            {
                viewButton.Content = "View Depth";
                kinectDisplay.DataContext = colorManager;
            }
        }
        
        bool isTesting = false;
        int currentTest = 1;
        int inIndex = 0;

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            if (replay != null)
            {
                replay.SkeletonFrameReady -= replay_SkeletonFrameReady;
                replay.Stop();
            }
            isTesting = true;
            inIndex = 0;
            Stream recordStream = File.OpenRead(testFile);
            replay = new SkeletonReplay(recordStream);
            replay.SkeletonFrameReady += replay_SkeletonFrameReady;
            replay.Start();

        }
        void testingGestures(String gestureDetected)
        {
            if (!isTesting) return;
            if (inIndex >= testSet1.Length)
            {
                System.Windows.MessageBox.Show("Test Successful!");
                return;
            }
            else
            {
                if (testSet1[inIndex].ToString() != gestureDetected)
                {
                    System.Windows.MessageBox.Show("Test failed! Expected: " + testSet1[inIndex] + " but got: " + gestureDetected);
                    isTesting = false;
                    inIndex = 0;
                }
                else
                {
                    inIndex++;
                }
            }
        }
        [DllImport("user32", EntryPoint = "EnumWindows")]
        public static extern int EnumWindows(CallBack x, int y);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd,
           StringBuilder lpClassName,
           int nMaxCount
        );

        public static bool Report(int hwnd, int lParam)
        {
            if (IsWindowVisible((IntPtr)hwnd))
            {
                StringBuilder buffer = new StringBuilder(128);
                GetClassName((IntPtr)hwnd, buffer, buffer.Capacity);
                //Console.WriteLine(buffer);

                XmlReader reader = XmlReader.Create("data.xml");
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "class_name")
                    {
                        if (buffer.ToString().Equals(reader.ReadElementContentAsString()))
                        {
                            Console.WriteLine("Player Found!");
                            ControlFunction.playerHandle = (IntPtr)hwnd;
                            //playerHandle = (IntPtr)hwnd;
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
                }
                reader.Close();
            }
            return true;
        }
        private int CheckOpenedPlayer()
        {
            CallBack myCallBack = new CallBack(Report);
            return EnumWindows(myCallBack, 0);
        }
        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName,
            string lpWindowName);

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            String revCircleKBPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"data\leftOpeningHands.save");
            using (Stream revRecordStream = File.Open(revCircleKBPath, FileMode.OpenOrCreate))
            {
                //twoHandedDetector.LeftOpeningDetector.SaveState(revRecordStream);
            }
        }

        private void elevationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

       
    }
}
