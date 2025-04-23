using NUnit.Framework;
using System;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using System.Drawing;
using System.Net.NetworkInformation;


namespace Reporting
{
    public class Class1
    {
        private bool screenshotForPass = true;

        private ExtentReports extent;
        private ExtentTest test;
        static private string reportDirectory = @"C:\Reports\";
        static private string passDirectory = $@"{reportDirectory}Pass\";
        static private string failDirectory = $@"{reportDirectory}Fail\";
        static private string infoDirectory = $@"{reportDirectory}Info\";

        [SetUp]
        public void InitializeReport()
        {
            extent = new ExtentReports();
            Directory.CreateDirectory(reportDirectory);
            var spark = new ExtentSparkReporter($"{reportDirectory}Spark.html");
            extent.AttachReporter(spark);

            // Create directories if they do not exist
            Directory.CreateDirectory(passDirectory);
            Directory.CreateDirectory(failDirectory);
            Directory.CreateDirectory(infoDirectory);
        }

        [TearDown]
        public void FinalizeReport()
        {
            extent.Flush();
        }

        public void CreateTest(string testName)
        {
            test = extent.CreateTest(testName);
        }

        public void LogStep(Status status, string message, bool bScreenshot = true)
        {
            if (status == Status.Pass || status == Status.Info)
                bScreenshot = screenshotForPass; // Use the class variable for Pass and Info status
            if (bScreenshot)
            {
                string screenshotPath = CaptureScreenshot(status);
                if (screenshotPath == null)
                    test.Log(status, message);
                else
                    test.Log(status, message, MediaEntityBuilder.CreateScreenCaptureFromPath(screenshotPath).Build());
            }
            else
            {
                test.Log(status, message);
            }
        }

        private string CaptureScreenshot(Status status)
        {
            string directory = passDirectory;
            switch (status)
            {
                case Status.Pass:
                    directory = passDirectory;
                    break;
                case Status.Fail:
                case Status.Error:
                case Status.Warning:
                    directory = failDirectory;
                    break;
                case Status.Info:
                    directory = infoDirectory;
                    break;
                default:
                    throw new Exception("Invalid status");
            }
            string fileName = $"{Guid.NewGuid()}.png";
            string filePath = Path.Combine(directory, fileName);

            using (Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
                    bitmap.Save(filePath, ImageFormat.Png);
                }
            }

            return filePath;
        }

        [Test]
        public void TestMethod1()
        {
            InitializeReport();
            CreateTest("MyFirstTest");
            LogStep(Status.Pass, "This is a logging event for MyFirstTest, and it passed!");
            CreateTest("MySecondTest");
            LogStep(Status.Fail, "This is failed!");
            LogStep(Status.Pass, "This is pass!");
            CreateTest("MyTest3");
            LogStep(Status.Info, "This is info!");
            CreateTest("MyTest4");
            LogStep(Status.Warning, "This is warning!", false);
            CreateTest("MyTest5");
            LogStep(Status.Error, "This is error!");
            CreateTest("MyTest6");
            LogStep(Status.Info, "This is pass!");
            //LogStep(Status.Warning, "This is warning!");
            LogStep(Status.Pass, "This is skip!");
        }
    }
}