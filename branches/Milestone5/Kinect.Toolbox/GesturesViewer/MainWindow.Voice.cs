using System;
using System.IO;

namespace GesturesViewer
{
    partial class MainWindow
    {
        void StartVoiceCommander()
        {
            voiceCommander.Start(kinectSensor);
        }

        void voiceCommander_OrderDetected(string order)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (audioControl.IsChecked == false)
                    return;

                switch (order)
                {
                    case "record":
                        //DirectRecord(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "kinectRecord" + Guid.NewGuid() + ".replay"));
                        recordCircle_Click(null, null);
                        break;
                    case "stop":
                        recordCircle_Click(null, null);
                        break;
                }
            }));
        }
    }
}
