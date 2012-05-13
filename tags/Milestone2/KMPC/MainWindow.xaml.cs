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
        static IntPtr playerHandle;

        [DllImport("user32", EntryPoint = "EnumWindows")]
        public static extern int EnumWindows(CallBack x, int y);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd,
           StringBuilder lpClassName,
           int nMaxCount
        );

        static private string play;
        static private string stop;
        static private string full;
        static private string mute;
        static private string fwd;
        static private string bwd;
        static private string vup;
        static private string vdown;

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
                           Console.WriteLine("Player Found!");
                           playerHandle = (IntPtr)hwnd;
                           reader.ReadToFollowing("play");
                           play = reader.ReadElementContentAsString();
                           reader.ReadToFollowing("stop");
                           stop = reader.ReadElementContentAsString();
                           reader.ReadToFollowing("full");
                           full = reader.ReadElementContentAsString();
                           reader.ReadToFollowing("fwd");
                           fwd = reader.ReadElementContentAsString();
                           reader.ReadToFollowing("bwd");
                           bwd = reader.ReadElementContentAsString();
                           reader.ReadToFollowing("mute");
                           mute = reader.ReadElementContentAsString();
                           reader.ReadToFollowing("vup");
                           vup = reader.ReadElementContentAsString();
                           reader.ReadToFollowing("vdown");
                           vdown = reader.ReadElementContentAsString();
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

            if (playerHandle == IntPtr.Zero)
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

        // Activate an application window.
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Play();
        }

        private string KeyPressInterpreter(string control_str)
        {
            bool contains_modifier = false;
            char[] delimiterChars = {'+'};
            StringBuilder sb = new StringBuilder();
            
            if (control_str != null)
            {
                if (control_str.Contains("Shift"))
                {
                    sb.Append("+");
                    contains_modifier = true;
                }
                if (control_str.Contains("Control"))
                {
                    sb.Append("^");
                    contains_modifier = true;
                }
                if (control_str.Contains("Alt"))
                {
                    sb.Append("%");
                    contains_modifier = true;
                }
                if (contains_modifier == true)
                {
                    sb.Append("(");
                }

                string[] words = control_str.Split(delimiterChars);

                foreach (string str in words)
                {
                    string s = str.Trim();
                    if (s.Length == 1)
                    {
                        sb.Append(s.ToLower());
                    }
                    else if (s.Equals("Left") || s.Equals("Right") || s.Equals("Up") || s.Equals("Down") || (s.Substring(0, 1).Equals("F")))
                    {
                        sb.Append("{"+s.ToUpper()+"}");
                    }
                    else if (s.Contains("Space"))
                    {
                        sb.Append(" ");
                    }
                    else if (s.Equals("Return"))
                    {
                        sb.Append("{ENTER}");
                    }
                }
                if (contains_modifier == true)
                {
                    sb.Append(")");
                }

                return sb.ToString();
            }

            return null;
        }

        private void Play()
        {
            SetForegroundWindow(playerHandle);
            SendKeys.SendWait(KeyPressInterpreter(play));
        }

        private void Stop()
        {
            SetForegroundWindow(playerHandle);
            SendKeys.SendWait(KeyPressInterpreter(stop));
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            Full_Screen();
        }

        private void Full_Screen()
        {
            SetForegroundWindow(playerHandle);
            SendKeys.SendWait(KeyPressInterpreter(full));
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            Mute();
        }

        private void Mute()
        {
            SetForegroundWindow(playerHandle);
            SendKeys.SendWait(KeyPressInterpreter(mute));
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            Volume_Up();
        }

        private void Volume_Up()
        {
            SetForegroundWindow(playerHandle);
            SendKeys.SendWait(KeyPressInterpreter(vup));
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            Volume_Down();
        }

        private void Volume_Down()
        {
            SetForegroundWindow(playerHandle);
            SendKeys.SendWait(KeyPressInterpreter(vdown));
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            Form1 f1 = new Form1();
            f1.FormClosed += new FormClosedEventHandler(f1_FormClosed);
            f1.Show();
        }

        void f1_FormClosed(object sender, FormClosedEventArgs e)
        {
            CheckOpenedPlayer();
            if (playerHandle == IntPtr.Zero)
            {
                System.Windows.Forms.MessageBox.Show("Media Player is not running.");
            }
        }

        private void button9_Click(object sender, RoutedEventArgs e)
        {
            Forward();
        }

        private void Forward()
        {
            SetForegroundWindow(playerHandle);
            SendKeys.SendWait(KeyPressInterpreter(fwd));
        }

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            Backward();
        }

        private void Backward()
        {
            SetForegroundWindow(playerHandle);
            SendKeys.SendWait(KeyPressInterpreter(bwd));
        }
    }
}
