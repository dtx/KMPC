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
    ///This is a test class for ControlFunctionTest and is intended
    ///to contain all ControlFunctionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ControlFunctionTest
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
        ///A test for KeyPressInterpreter
        ///</summary>



        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public void ExecuteTest()
        {
            bool actual;

            // null input returns false
            actual = ControlFunction.Execute("");
            Assert.AreEqual(false, actual);

            // sensible input returns true (non-null and not white space)
            actual = ControlFunction.Execute("^p");
            Assert.AreEqual(true, actual);

            // non-null but space only input returns false
            actual = ControlFunction.Execute("  ");
            Assert.AreEqual(false, actual);
            
        }

        /// <summary>
        ///A test for KeyPressInterpreter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("KMPC.exe")]
        public void KeyPressInterpreterTest()
        {
            string control_str = string.Empty; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;

            control_str = "Shift";
            expected = "+()";
            actual = ControlFunction_Accessor.KeyPressInterpreter(control_str);
            Assert.AreEqual(expected, actual);

            control_str = "Control";
            expected = "^()";
            actual = ControlFunction_Accessor.KeyPressInterpreter(control_str);
            Assert.AreEqual(expected, actual);

            control_str = "Alt";
            expected = "%()";
            actual = ControlFunction_Accessor.KeyPressInterpreter(control_str);
            Assert.AreEqual(expected, actual);

            control_str = "Left";
            expected = "{LEFT}";
            actual = ControlFunction_Accessor.KeyPressInterpreter(control_str);
            Assert.AreEqual(expected, actual);

            control_str = "Right";
            expected = "{RIGHT}";
            actual = ControlFunction_Accessor.KeyPressInterpreter(control_str);
            Assert.AreEqual(expected, actual);

            control_str = "Up";
            expected = "{UP}";
            actual = ControlFunction_Accessor.KeyPressInterpreter(control_str);
            Assert.AreEqual(expected, actual);

            control_str = "Down";
            expected = "{DOWN}";
            actual = ControlFunction_Accessor.KeyPressInterpreter(control_str);
            Assert.AreEqual(expected, actual);

            control_str = "Space";
            expected = " ";
            actual = ControlFunction_Accessor.KeyPressInterpreter(control_str);
            Assert.AreEqual(expected, actual);

            control_str = "Return";
            expected = "{ENTER}";
            actual = ControlFunction_Accessor.KeyPressInterpreter(control_str);
            Assert.AreEqual(expected, actual);

            control_str = "Shift+Control+Alt+Space+Return+Up+Down+Right+Left";
            expected = "+^%( {ENTER}{UP}{DOWN}{RIGHT}{LEFT})";
            actual = ControlFunction_Accessor.KeyPressInterpreter(control_str);
            Assert.AreEqual(expected, actual);
        }
    }
}
