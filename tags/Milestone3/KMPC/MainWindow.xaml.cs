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
using Microsoft.Kinect;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;

namespace KMPC
{
    public delegate bool CallBack(int hwnd, int lParam);

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

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
                       if(buffer.ToString().Equals(reader.ReadElementContentAsString()))
                       {
                           //Console.WriteLine("Player Found!");
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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kinectSensorChooser1.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser1_KinectSensorChanged);
            
            CheckOpenedPlayer();

            if (ControlFunction.playerHandle == IntPtr.Zero)
            {
                System.Windows.Forms.MessageBox.Show("Media Player is not running.");
                //Exit();
            }
        }

        private int CheckOpenedPlayer()
        {
            CallBack myCallBack = new CallBack(Report);
            return EnumWindows(myCallBack, 0);
        }

        void kinectSensorChooser1_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            KinectSensor oldSensor = (KinectSensor)e.OldValue;
            StopKinect(oldSensor);

            KinectSensor newSensor = (KinectSensor)e.NewValue;

            newSensor.ColorStream.Enable();
            newSensor.DepthStream.Enable();
            newSensor.SkeletonStream.Enable();
            //newSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(_sensor_AllFramesReady);
            try
            {
                newSensor.Start();
            }
            catch (System.IO.IOException)
            {
                kinectSensorChooser1.AppConflictOccurred();
            }
        }

        void _sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            //using(ColorImageFrame colorFrame = e.OpenColorImageFrame())
            //{
            //    if (colorFrame == null)
            //    {
            //        return;
            //    }

            //    byte[] pixels = new byte[colorFrame.PixelDataLength];
            //    colorFrame.CopyPixelDataTo(pixels);

            //    int stride = colorFrame.Width*4;

            //    image1.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height,
            //        96, 96,PixelFormats.Bgr32, null, pixels, stride);

            //}
        }

        void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                sensor.AudioSource.Stop();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopKinect(kinectSensorChooser1.Kinect);
        }

        // Get a handle to an application window.
        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName,
            string lpWindowName);

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            ControlFunction.Execute(Parameter.play);
        }

        //private void Stop()
        //{
        //    stop = stop.Trim();
        //    if (!stop.Equals(""))
        //    {
        //        SetForegroundWindow(playerHandle);
        //        SendKeys.SendWait(KeyPressInterpreter(stop));
        //    }
        //}

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            ControlFunction.Execute(Parameter.stop);
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            ControlFunction.Execute(Parameter.full);
        }

        //private void Full_Screen()
        //{
        //    full = full.Trim();
        //    if (!full.Equals(""))
        //    {
        //        SetForegroundWindow(playerHandle);
        //        SendKeys.SendWait(KeyPressInterpreter(full));
        //    }
        //}

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            ControlFunction.Execute(Parameter.mute);
        }

        //private void Mute()
        //{
        //    mute = mute.Trim();
        //    if (!mute.Equals(""))
        //    {
        //        SetForegroundWindow(playerHandle);
        //        SendKeys.SendWait(KeyPressInterpreter(mute));
        //    }
        //}

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            ControlFunction.Execute(Parameter.vup);
        }

        //private void Volume_Up()
        //{
        //    vup = vup.Trim();
        //    if (!vup.Equals(""))
        //    {
        //        SetForegroundWindow(playerHandle);
        //        SendKeys.SendWait(KeyPressInterpreter(vup));
        //    }
        //}

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            ControlFunction.Execute(Parameter.vdown);
        }

        //private void Volume_Down()
        //{
        //    vdown = vdown.Trim();
        //    if (!vdown.Equals(""))
        //    {
        //        SetForegroundWindow(playerHandle);
        //        SendKeys.SendWait(KeyPressInterpreter(vdown));
        //    }
        //}

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            Form1 f1 = new Form1();
            f1.FormClosed += new FormClosedEventHandler(f1_FormClosed);
            f1.Show();
        }

        void f1_FormClosed(object sender, FormClosedEventArgs e)
        {
            CheckOpenedPlayer();
            if (ControlFunction.playerHandle == IntPtr.Zero)
            {
                System.Windows.Forms.MessageBox.Show("Media Player is not running.");
            }
        }

        private void button9_Click(object sender, RoutedEventArgs e)
        {
            ControlFunction.Execute(Parameter.fwd);
        }

        //private void Forward()
        //{
        //    fwd = fwd.Trim();
        //    if (!fwd.Equals(""))
        //    {
        //        SetForegroundWindow(playerHandle);
        //        SendKeys.SendWait(KeyPressInterpreter(fwd));
        //    }
        //}

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            ControlFunction.Execute(Parameter.bwd);
        }

        //private void Backward()
        //{
        //    bwd = bwd.Trim();
        //    if (!bwd.Equals(""))
        //    {
        //        SetForegroundWindow(playerHandle);
        //        SendKeys.SendWait(KeyPressInterpreter(bwd));
        //    }
        //}
    }
}
