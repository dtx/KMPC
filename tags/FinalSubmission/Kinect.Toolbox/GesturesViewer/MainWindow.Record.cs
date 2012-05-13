using System.IO;
using System.Windows;
using Kinect.Toolbox.Record;
using Microsoft.Win32;

namespace GesturesViewer
{
    partial class MainWindow
    {
        private void recordOption_Click(object sender, RoutedEventArgs e)
        {
            if (recorder != null)
            {
                StopRecord();
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog { Title = "Select filename", Filter = "Replay files|*.replay" };

            if (saveFileDialog.ShowDialog() == true)
            {
                DirectRecord(saveFileDialog.FileName);
            }
        }

        void DirectRecord(string targetFileName)
        {
            recorder = new SkeletonRecorder();
            Stream recordStream = File.Create(targetFileName);

            recorder.Start(recordStream);
            recordOption.Content = "Stop Recording";
        }

        void StopRecord()
        {
            if (recorder != null)
            {
                recorder.Stop();
                recorder = null;
                recordOption.Content = "Record";
                return;
            }
        }
    }
}
