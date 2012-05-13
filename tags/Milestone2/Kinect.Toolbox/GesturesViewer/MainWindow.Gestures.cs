using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Kinect.Toolbox;
using Microsoft.Kinect;

namespace GesturesViewer
{
    partial class MainWindow
    {
        void LoadCircleGestureDetector()
        {
            using (Stream recordStream = File.Open(circleKBPath, FileMode.OpenOrCreate))
            {
                circleGestureRecognizer = new TemplatedGestureDetector("Circle", recordStream);
                circleGestureRecognizer.TraceTo(gesturesCanvas, Colors.Red);
                circleGestureRecognizer.OnGestureDetected += OnGestureDetected;
                circleGestureRecognizer.OnGestureDetected += testingGestures;
            }
            templates.ItemsSource = circleGestureRecognizer.LearningMachine.Paths;
        }

        private void recordCircle_Click(object sender, RoutedEventArgs e)
        {
            if (circleGestureRecognizer.IsRecordingPath)
            {
                circleGestureRecognizer.EndRecordTemplate();
                recordCircle.Content = "Record new Circle";
                return;
            }

            circleGestureRecognizer.StartRecordTemplate();
            recordCircle.Content = "Stop Recording";
        }

        void OnGestureDetected(string gesture)
        {
            int pos = detectedGestures.Items.Add(string.Format("{0} : {1}", gesture, DateTime.Now));

            detectedGestures.SelectedIndex = pos;
        }

        void CloseGestureDetector()
        {
            if (circleGestureRecognizer == null)
                return;

            using (Stream recordStream = File.Create(circleKBPath))
            {
                circleGestureRecognizer.SaveState(recordStream);
            }
            circleGestureRecognizer.OnGestureDetected -= OnGestureDetected;
        }

    }
}
