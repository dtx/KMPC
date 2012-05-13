using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Kinect.Toolbox;
using Microsoft.Kinect;
using KMPC;
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
            if (gesture == "SwipeToRight")
            {
                ControlFunction.Execute(Parameter.fwd);
            }
            else if (gesture == "SwipeToLeft")
            {
                ControlFunction.Execute(Parameter.bwd);
            }
            else if (gesture == "SwipeUp")
            {
                ControlFunction.Execute(Parameter.vup);
            }
            else if (gesture == "SwipeDown")
            {
                ControlFunction.Execute(Parameter.vdown);
            }
            else if (gesture == "Circle")
            {
                ControlFunction.Execute(Parameter.play);
            }
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
