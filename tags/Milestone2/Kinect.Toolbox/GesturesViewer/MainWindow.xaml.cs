using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Kinect.Toolbox;
using Kinect.Toolbox.Record;
using System.IO;
using Microsoft.Kinect;
using Microsoft.Win32;
using Kinect.Toolbox.Voice;

namespace GesturesViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        KinectSensor kinectSensor;

        SwipeGestureDetector swipeGestureRecognizer;
        TemplatedGestureDetector circleGestureRecognizer;
        readonly ColorStreamManager colorManager = new ColorStreamManager();
        readonly DepthStreamManager depthManager = new DepthStreamManager();
        AudioStreamManager audioManager;
        SkeletonDisplayManager skeletonDisplayManager;
        readonly BarycenterHelper barycenterHelper = new BarycenterHelper();
        readonly AlgorithmicPostureDetector algorithmicPostureRecognizer = new AlgorithmicPostureDetector();
        TemplatedPostureDetector templatePostureDetector;
        private bool recordNextFrameForPosture;
        bool displayDepth;

        string circleKBPath;
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
                    if (kinectSensor == null)
                    {
                        kinectSensor = e.Sensor;
                        Initialize();
                    }
                    break;
                case KinectStatus.Disconnected:
                    if (kinectSensor == e.Sensor)
                    {
                        Clean();
                        MessageBox.Show("Kinect was disconnected");
                    }
                    break;
                case KinectStatus.NotReady:
                    break;
                case KinectStatus.NotPowered:
                    if (kinectSensor == e.Sensor)
                    {
                        Clean();
                        MessageBox.Show("Kinect is no more powered");
                    }
                    break;
                default:
                    MessageBox.Show("Unhandled Status: " + e.Status);
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            circleKBPath = Path.Combine(Environment.CurrentDirectory, @"data\circleKB.save");
            letterT_KBPath = Path.Combine(Environment.CurrentDirectory, @"data\t_KB.save");

            try
            {
                //listen to any status change for Kinects
                KinectSensor.KinectSensors.StatusChanged += Kinects_StatusChanged;

                //loop through all the Kinects attached to this PC, and start the first that is connected without an error.
                foreach (KinectSensor kinect in KinectSensor.KinectSensors)
                {
                    if (kinect.Status == KinectStatus.Connected)
                    {
                        kinectSensor = kinect;
                        break;
                    }
                }

                if (KinectSensor.KinectSensors.Count == 0)
                    MessageBox.Show("No Kinect found");
                else
                    Initialize();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Initialize()
        {
            if (kinectSensor == null)
                return;

            audioManager = new AudioStreamManager(kinectSensor.AudioSource);
            audioBeamAngle.DataContext = audioManager;

            kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            kinectSensor.ColorFrameReady += kinectRuntime_ColorFrameReady;

            kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            kinectSensor.DepthFrameReady += kinectSensor_DepthFrameReady;

            kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters
                                                   {
                                                 Smoothing = 0.5f,
                                                 Correction = 0.5f,
                                                 Prediction = 0.5f,
                                                 JitterRadius = 0.05f,
                                                 MaxDeviationRadius = 0.04f
                                             });
            kinectSensor.SkeletonFrameReady += kinectRuntime_SkeletonFrameReady;

            swipeGestureRecognizer = new SwipeGestureDetector();
            swipeGestureRecognizer.OnGestureDetected += OnGestureDetected;
            swipeGestureRecognizer.OnGestureDetected += testingGestures;

            skeletonDisplayManager = new SkeletonDisplayManager(kinectSensor, kinectCanvas);

            kinectSensor.Start();

            LoadCircleGestureDetector();
            LoadLetterTPostureDetector();

            nuiCamera = new BindableNUICamera(kinectSensor);

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

                foreach (Joint joint in skeleton.Joints)
                {
                    if (joint.TrackingState != JointTrackingState.Tracked)
                        continue;

                    if (joint.JointType == JointType.HandRight)
                    {
                        swipeGestureRecognizer.Add(joint.Position, kinectSensor);
                        circleGestureRecognizer.Add(joint.Position, kinectSensor);
                    }
                }

                algorithmicPostureRecognizer.TrackPostures(skeleton);
                templatePostureDetector.TrackPostures(skeleton);

                if (recordNextFrameForPosture)
                {
                    templatePostureDetector.AddTemplate(skeleton);
                    recordNextFrameForPosture = false;
                }
            }

            skeletonDisplayManager.Draw(frame);

            stabilitiesList.ItemsSource = stabilities;

            currentPosture.Text = "Current posture: " + algorithmicPostureRecognizer.CurrentPosture;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Clean();
        }

        private void Clean()
        {
            if (swipeGestureRecognizer != null)
            {
                swipeGestureRecognizer.OnGestureDetected -= OnGestureDetected;
            }

            if (audioManager != null)
            {
                audioManager.Dispose();
                audioManager = null;
            }

            CloseGestureDetector();

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

            if (kinectSensor != null)
            {
                kinectSensor.ColorFrameReady -= kinectRuntime_ColorFrameReady;
                kinectSensor.SkeletonFrameReady -= kinectRuntime_SkeletonFrameReady;
                kinectSensor.ColorFrameReady -= kinectRuntime_ColorFrameReady;
                kinectSensor.Stop();
                kinectSensor = null;
            }
        }

        private void replayButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Title = "Select filename", Filter = "Replay files|*.replay" };

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
        string testFile = Path.Combine(Environment.CurrentDirectory, @"data\test1.replay");
        public string[] testSet1 = new string[] { "SwipeDown", "SwipeUp", "SwipeToLeft", "SwipeToRight1", "Circle" };
        bool isTesting = false;
        int currentTest = 1;
        int inIndex = 0;

        private void button1_Click(object sender, RoutedEventArgs e)
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
                MessageBox.Show("Test Successful!");
                return;
            }
            else
            {
                if (testSet1[inIndex] != gestureDetected)
                {
                    MessageBox.Show("Test failed! Expected: " + testSet1[inIndex] + " but got: " + gestureDetected);
                    isTesting = false;
                    inIndex = 0;
                }
                else
                {
                    inIndex++;
                }
            }
        }
    }
}
