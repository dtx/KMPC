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
using Kinect.Toolbox.Gestures;
using Kinect.Toolbox.Record;
using Microsoft.Kinect;
using System.IO;
namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GestureSuite _gestureSuite;
        SkeletonReplay _testReplay;
        readonly BarycenterHelper _barycenterHelper = new BarycenterHelper();
        KnownGestures[] _currentSet;
        int _currentIndex = 0;
        /// <summary>
        /// Initializes the testing suite
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            _gestureSuite = new GestureSuite();
            _gestureSuite.OnGestureDetected += ConfirmTest;
        }

        /// <summary>
        /// Copies the tests to the test array
        /// </summary>
        /// <param name="source"></param>
        private void CopyArrayToTest(KnownGestures [] source)
        {
            _currentSet = new KnownGestures[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                _currentSet[i] = source[i];
            }
        }

        /// <summary>
        /// Sets the expected list to the array
        /// </summary>
        /// <param name="expected"></param>
        private void SetExpectedList(KnownGestures[] expected)
        {
            _expectedGestures.Items.Clear();
            for (int i = 0; i < expected.Length; i++)
            {
                _expectedGestures.Items.Add(expected[i].ToString());
            }
        }

        /// <summary>
        /// Clears the list of detected gestures if there are any
        /// </summary>
        private void ClearDetectedList()
        {
            _detectedGestures.Items.Clear();
        }

        /// <summary>
        /// Runs a test with basic swipes and clockwise
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunTest1(object sender, RoutedEventArgs e)
        {
            ClearDetectedList();
            KnownGestures[] testSet1 = new KnownGestures[] { KnownGestures.SwipeDown, KnownGestures.SwipeUp, KnownGestures.SwipeLeft, 
                KnownGestures.SwipeRight, KnownGestures.Clockwise};
        
            CopyArrayToTest(testSet1);
            SetExpectedList(testSet1);

            string testFile = System.IO.Path.Combine(Environment.CurrentDirectory, @"data\test1.replay");
            Stream testReplayStreeam = File.OpenRead(testFile);
            _testReplay = new SkeletonReplay(testReplayStreeam);
            _testReplay.SkeletonFrameReady += replay_SkeletonFrameReady;
            _currentIndex = 0;
            _testReplay.Start();
        }


        /// <summary>
        /// Runs a test with hands opening and Hands closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunTest2(object sender, RoutedEventArgs e)
        {
            ClearDetectedList();
            KnownGestures[] testSet2 = new KnownGestures[] { KnownGestures.HandsOpened, KnownGestures.HandsClosed};

            CopyArrayToTest(testSet2);
            SetExpectedList(testSet2);

            string testFile = System.IO.Path.Combine(Environment.CurrentDirectory, @"data\test2.replay");
            Stream testReplayStreeam = File.OpenRead(testFile);
            _testReplay = new SkeletonReplay(testReplayStreeam);
            _testReplay.SkeletonFrameReady += replay_SkeletonFrameReady;
            _currentIndex = 0;
            _testReplay.Start();
        }


        /// <summary>
        /// Callback function to analyze replay frames
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void replay_SkeletonFrameReady(object sender, ReplaySkeletonFrameReadyEventArgs e)
        {
            ProcessFrame(e.SkeletonFrame);
        }

        /// <summary>
        /// Checks the test array to see if gesture is right
        /// </summary>
        /// <param name="gesture"></param>
        private void ConfirmTest(string gesture)
        {
            if (_currentSet == null)
                return;
            int pos = _detectedGestures.Items.Add(gesture);
            _detectedGestures.SelectedIndex = pos;
            if (gesture == _currentSet[_currentIndex].ToString())
            {
                if (_currentIndex == _currentSet.Length - 1)
                {
                    System.Windows.MessageBox.Show("Test Successful!");
                    _currentSet = null;
                    _currentIndex = 0;
                    _testReplay.Stop();
                }
                else
                {
                    _currentIndex++;
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Test failed! Expected: " +
                    _currentSet[_currentIndex] + " but got: " + gesture);
                _currentSet = null;
                _currentIndex = 0;
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
                _gestureSuite.Add(skeleton, null);
                _barycenterHelper.Add(skeleton.Position.ToVector3(), skeleton.TrackingId);
                if (!_barycenterHelper.IsStable(skeleton.TrackingId))
                    return;
            }

        }
    }
}
