using KMPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml;

namespace TestProject1
{
    
    
    /// <summary>
    ///This is a test class for Form1Test and is intended
    ///to contain all Form1Test Unit Tests
    ///</summary>
    [TestClass()]
    public class Form1Test
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
        ///A test for OnButtonOKClicked
        ///</summary>
        [TestMethod()]
        [DeploymentItem("KMPC.exe")]
        public void OnButtonOKClickedTest()
        {
            Form1_Accessor target = new Form1_Accessor(); // TODO: Initialize to an appropriate value
            object sender = null; // TODO: Initialize to an appropriate value
            EventArgs e = null; // TODO: Initialize to an appropriate value


            // empty the data.xml if it exists
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load("data.xml");
            }
            catch (Exception)
            {
                doc.LoadXml("<item></item>");
            }
            XmlTextWriter writer = new XmlTextWriter("data.xml", null);
            writer.Formatting = Formatting.Indented;
            doc.WriteTo(writer);
            writer.Close();

            target.textBox_play.Text = "Control + P";
            target.textBox_stop.Text = "Control + S";
            target.textBox_full.Text = "Alt + Enter";
            target.textBox_mute.Text = "F7";
            target.textBox_fwd.Text = "Shift + Control + F";
            target.textBox_bwd.Text = "Shift + Control + B";
            target.textBox_vup.Text = "F9";
            target.textBox_vdown.Text = "F8";
            target.textBox_class_name.Text = "WMPlayerApp";


            target.OnButtonOKClicked(sender, e);

            //check the elements in the xml
            doc.Load("data.xml");
            String test;
            //the test shows NULL, need to parse in next time
            XmlNode root = doc.FirstChild;

            if (root.HasChildNodes)
            {
                XmlNode child = root.FirstChild;
                if (child.HasChildNodes)
                {
                    for (int i = 0; i < child.ChildNodes.Count; i++)
                    {
                        if(i == 0)
                            Assert.AreEqual(child.ChildNodes[i].InnerText, "WMPlayerApp");
                        if (i == 1)
                            Assert.AreEqual(child.ChildNodes[i].InnerText, "Control + P");
                        if (i == 2)
                            Assert.AreEqual(child.ChildNodes[i].InnerText, "Control + S");
                        if (i == 3)
                            Assert.AreEqual(child.ChildNodes[i].InnerText, "Alt + Enter");
                        if (i == 4)
                            Assert.AreEqual(child.ChildNodes[i].InnerText, "Shift + Control + F");
                        if (i == 5)
                            Assert.AreEqual(child.ChildNodes[i].InnerText, "Shift + Control + B");
                        if (i == 6)
                            Assert.AreEqual(child.ChildNodes[i].InnerText, "F7");
                        if (i == 7)
                            Assert.AreEqual(child.ChildNodes[i].InnerText, "F9");
                        if (i == 8)
                            Assert.AreEqual(child.ChildNodes[i].InnerText, "F8");
                    }

                }
            }
            /*test = doc["player"]["class_name"].InnerText;
            Assert.AreEqual(doc["player"]["class_name"].InnerText, "WMPlayerApp");
            */
            //Assert.AreEqual(1, 1);
        }
    }
}
