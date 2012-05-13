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
            using (Stream revRecordStream = File.Open(revCircleKBPath, FileMode.OpenOrCreate))
            {
                revCircleGestureRecognizer = new TemplatedGestureDetector("RevCircle", revRecordStream);
                revCircleGestureRecognizer.TraceTo(gesturesCanvas, Colors.Red);
                revCircleGestureRecognizer.OnGestureDetected += OnGestureDetected;
                revCircleGestureRecognizer.OnGestureDetected += testingGestures;
            }
            using (Stream recordStream = File.Open(circleKBPath, FileMode.OpenOrCreate))
            {
                circleGestureRecognizer = new TemplatedGestureDetector("Circle", recordStream);
                circleGestureRecognizer.TraceTo(gesturesCanvas, Colors.Red);
                circleGestureRecognizer.OnGestureDetected += OnGestureDetected;
                circleGestureRecognizer.OnGestureDetected += testingGestures;
            }
            templates.ItemsSource = revCircleGestureRecognizer.LearningMachine.Paths;
        }

        private void recordCircle_Click(object sender, RoutedEventArgs e)
        {
            if (revCircleGestureRecognizer.IsRecordingPath)
            {
                revCircleGestureRecognizer.EndRecordTemplate();
                recordCircle.Content = "Record new Circle";
                return;
            }

            revCircleGestureRecognizer.StartRecordTemplate();
            recordCircle.Content = "Stop Recording";
        }
        DateTime prevGesture = DateTime.MinValue;
        void OnGestureDetected(string gesture)
        {
            if ((DateTime.Now - prevGesture).TotalMilliseconds > 500) prevGesture = DateTime.Now;
            else return;
            int pos = detectedGestures.Items.Add(string.Format("{0} : {1}", gesture, DateTime.Now));
            if (gesture == "SwipeToRight")
            {
                ControlFunction.Execute(Parameter.play);
            }
            else if (gesture == "SwipeToLeft")
            {
                ControlFunction.Execute(Parameter.mute);
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
                ControlFunction.Execute(Parameter.fwd);
            }
            else if (gesture == "RevCircle")
            {
                ControlFunction.Execute(Parameter.bwd);
            }
            else if (gesture == "HandsOpened")
            {
                ControlFunction.Execute(Parameter.full);
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
