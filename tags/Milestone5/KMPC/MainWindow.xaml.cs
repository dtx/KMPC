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
using System.IO;

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

        // Get a handle to an application window.
        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName,
            string lpWindowName);

        /// <summary>
        /// Callback function. This function will be called for every open window found.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="lParam"></param>
        public static bool Report(int hwnd, int lParam)
        {
            if (IsWindowVisible((IntPtr)hwnd))
            {
                StringBuilder buffer = new StringBuilder(128);
                GetClassName((IntPtr)hwnd, buffer, buffer.Capacity);

                try
                {
                    XmlReader reader = XmlReader.Create("data.xml");

                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "class_name" && buffer.ToString().Equals(reader.ReadElementContentAsString()))
                        {
                            ControlFunction.playerHandle = (IntPtr)hwnd;
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
                    reader.Close();
                }
                catch (FileNotFoundException)
                {
                    return true;
                }
            }
            return true;
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kinectSensorChooser1.KinectSensorChanged += kinectSensorChooser_KinectSensorChanged;

            CheckOpenedPlayer();

            if (ControlFunction.playerHandle == IntPtr.Zero)
            {
                System.Windows.Forms.MessageBox.Show("Media Player is not running.");
            }
        }

        /// <summary>
        /// Check if there is a recorded media player which is open now 
        /// </summary>
        private int CheckOpenedPlayer()
        {
            CallBack myCallBack = new CallBack(Report);
            return EnumWindows(myCallBack, 0);
        }

        /// <summary>
        /// Detect kinect state changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void kinectSensorChooser_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
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

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            ControlFunction.Execute(Parameter.play);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            ControlFunction.Execute(Parameter.stop);
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            ControlFunction.Execute(Parameter.full);
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            ControlFunction.Execute(Parameter.mute);
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            ControlFunction.Execute(Parameter.vup);
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            ControlFunction.Execute(Parameter.vdown);
        }

        /// <summary>
        /// Open a form to add/edit shortcut information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addPlayerShortcuts_Click(object sender, RoutedEventArgs e)
        {
            PlayerShortcutForm psForm = new PlayerShortcutForm();
            psForm.FormClosed += new FormClosedEventHandler(psForm_FormClosed);
            psForm.Show();
        }

        /// <summary>
        /// Check for opened media player after shortcut information form is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void psForm_FormClosed(object sender, FormClosedEventArgs e)
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

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            ControlFunction.Execute(Parameter.bwd);
        }
    }
}
