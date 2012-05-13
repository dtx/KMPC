using KMPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Windows.Media;
using WMPLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows;

namespace TestProject1
{


    /// <summary>
    ///This is a test class for MainWindowTest and is intended
    ///to contain all MainWindowTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MainWindowTest
    {


        private TestContext testContextInstance;

        /*
         * Run windows media player for testing, set to run in minimized mode 
         */
        private Process setupProc()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"C:\Program Files (x86)\Windows Media Player\wmplayer.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            //startInfo.Arguments = "\"D:\\Visual Studio 2010\\KMPC\\testvid.mp4\"";
            return Process.Start(startInfo);
        }


        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for CheckOpenedPlayer
        /// Setup your test environment before running, or else it will fail
        /// 1) add a test video as D:\Visual Studio 2010\testvid.mp4\, or
        ///     or change the file path to whatever you like that WMP runs (see Process setupProc();)
        /// 2) Go to Test > Edit Test Settings > Local > Deployment > Enable Deployment & Add File
        ///     a) add your TestProject1/bin/Debug/data.xml
        ///         i) This needs to be created first, run KMPC.exe from TestProject1/bin/Debug and
        ///             click "Button" -> Save
        ///     b) Apply & Close
        /// 3) Should be good to go.
        ///</summary>
        [TestMethod()]
        [DeploymentItem("KMPC.exe")]
        public void CheckOpenedPlayerTest()
        {
            MainWindow_Accessor target = new MainWindow_Accessor();
            int playerRunning = 0, playerNotRunning = 1;
            int actual;

            // Run CheckOpenPlayer() without any running WMP instances
            actual = target.CheckOpenedPlayer();
            Assert.AreEqual(playerNotRunning, actual);

            //Runs an instance of WMP to check if CheckOpenedPlayer can detect it
            Process wmp = setupProc();
            System.Threading.Thread.Sleep(1000);
            actual = target.CheckOpenedPlayer();
            Assert.AreEqual(playerRunning, actual);
            ControlFunction.Execute(Parameter.play);
            System.Threading.Thread.Sleep(1000);
            Console.Out.WriteLine("process ID: " + wmp.Id);
            wmp.Kill();
        }
    }
}

