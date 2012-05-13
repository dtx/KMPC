using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Kinect.Toolbox;
using Microsoft.Kinect;
using KMPC;
using Kinect.Toolbox.Gestures;
namespace GesturesViewer
{
    partial class MainWindow
    {
        private void recordCircle_Click(object sender, RoutedEventArgs e)
        {
            if (circleDetector.CounterClockwiseDetector.IsRecordingPath)
            {
                circleDetector.CounterClockwiseDetector.EndRecordTemplate();
                recordCircle.Content = "Record new Circle";
                return;
            }

            circleDetector.CounterClockwiseDetector.StartRecordTemplate();
            recordCircle.Content = "Stop Recording";
        }
        DateTime prevGesture = DateTime.MinValue;
        void OnGestureDetected(string gesture)
        {
            if ((DateTime.Now - prevGesture).TotalMilliseconds > 1000) prevGesture = DateTime.Now;
            else return;
            int pos = detectedGestures.Items.Add(string.Format("{0} : {1}", gesture, DateTime.Now));
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
            detectedGestures.SelectedIndex = pos;
        }


    }
}
