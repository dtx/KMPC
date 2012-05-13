using System.IO;
using System.Windows;
using Kinect.Toolbox;

namespace GesturesViewer
{
    partial class MainWindow
    {
        void LoadLetterTPostureDetector()
        {
           
        }

        void ClosePostureDetector()
        {
                   }

        void templatePostureDetector_PostureDetected(string posture)
        {
            MessageBox.Show("Give me a......." + posture);
        }

        private void recordT_Click(object sender, RoutedEventArgs e)
        {
           }
    }
}
