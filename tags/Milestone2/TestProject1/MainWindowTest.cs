using KMPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

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
        ///</summary>
        [TestMethod()]
        [DeploymentItem("KMPC.exe")]
        public void CheckOpenedPlayerTest()
        {
            MainWindow_Accessor target = new MainWindow_Accessor(); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.CheckOpenedPlayer();
            Assert.AreEqual(1, actual);

            Process.Start("C:\\Program Files (x86)\\Windows Media Player\\wmplayer.exe");
            System.Threading.Thread.Sleep(1000);
            actual = target.CheckOpenedPlayer();
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
