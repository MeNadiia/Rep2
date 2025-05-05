using NUnit.Framework;
using System;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using System.Drawing;
using System.Net.NetworkInformation;
using AventStack.ExtentReports.Reporter.Model;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using AventStack.ExtentReports.Model;


namespace Reporting
{
    public class Class1
    {
        ChromeDriver driver;
        private bool screenshotForPass = true;

        private ExtentReports extent;
        private ExtentTest test;
        static private string reportDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Report");
        static private string passDirectory = $@"{reportDirectory}\Pass\";
        static private string failDirectory = $@"{reportDirectory}\Fail\";
        static private string infoDirectory = $@"{reportDirectory}\Info\";
        static private string reportFilePath = $@"{reportDirectory}\Spark.html";
        //public static string reportFilePath;

        [SetUp]
        public void InitializeReport()
        {

            extent = new ExtentReports();
            Directory.CreateDirectory(reportDirectory);
            // Create directories if they do not exist
            Directory.CreateDirectory(passDirectory);
            Directory.CreateDirectory(failDirectory);
            Directory.CreateDirectory(infoDirectory);

            //string reportFilePath = Path.Combine(reportDirectory, "Spark.html");
            var spark = new ExtentSparkReporter(reportFilePath);
            spark.Config.Theme = AventStack.ExtentReports.Reporter.Config.Theme.Standard;  //AventStack.ExtentReports.Reporter.Configuration.Theme.;

            extent.AttachReporter(spark);
            extent.AddSystemInfo("Document Title", "My Test Report");
            extent.AddSystemInfo("Host Name", Environment.MachineName);
            extent.AddTestRunnerLogs("Environment: QA");


            //File.WriteAllText(@"C:\Reports\report-path.txt", reportFilePath);

            //Intialise the browser
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
        }

        [TearDown]
        public void FinalizeReport()
        {
            // Flush the report to generate the Spark.html file
            extent.Flush();

            // Inject JavaScript for filtering into the Spark.html file
            InjectJavaScriptForFiltering(reportFilePath);
        }

        private void InjectJavaScriptForFiltering(string filePath)
        {
            // JavaScript snippet for filtering steps by status
            string script = @"
<script>
    // Function to toggle steps by status
    function toggleStepsByStatus(status, button) {
        const rows = document.querySelectorAll('.event-row'); // Select step rows
        const isActive = button.classList.contains('active'); // Check if the button is active

        if (isActive) {
            // If the button is active, deactivate it and hide the steps
            button.classList.remove('active');
            button.style.backgroundColor = 'transparent'; // Set button color to transparent
            button.style.color = 'black'; // Set font color to black
            rows.forEach(row => {
                const statusBadge = row.querySelector('.badge'); // Find the badge element
                if (statusBadge && statusBadge.classList.contains(status + '-bg')) {
                    row.style.display = 'none'; // Hide the row
                }
            });
        } else {
            // If the button is not active, activate it and show the steps
            button.classList.add('active');
            button.style.backgroundColor = getStatusColor(status); // Highlight the button with the status color
            button.style.color = 'white'; // Set font color to white
            rows.forEach(row => {
                const statusBadge = row.querySelector('.badge'); // Find the badge element
                if (statusBadge && statusBadge.classList.contains(status + '-bg')) {
                    row.style.display = ''; // Show the row
                }
            });
        }
    }

    // Function to clear all filters and show all steps
    function clearAllFilters() {
        const rows = document.querySelectorAll('.event-row'); // Select step rows
        const buttons = document.querySelectorAll('.filter-button'); // Select all filter buttons

        // Show all steps
        rows.forEach(row => {
            row.style.display = ''; // Show all rows
        });

        // Highlight all buttons with their respective colors
        buttons.forEach(button => {
            const status = button.getAttribute('data-status'); // Get the status from the button
            button.classList.add('active');
            button.style.backgroundColor = getStatusColor(status); // Highlight the button with the status color
            button.style.color = 'white'; // Set font color to white
        });
    }

    // Function to get the color for a specific status
    function getStatusColor(status) {
        const statusColors = {
            pass: '#28a745', // Green
            fail: '#dc3545', // Red
            warning: '#ffc107', // Yellow
            info: '#17a2b8', // Blue
            skip: '#6c757d', // Gray
            error: '#dc3545', // Red (same as fail)
        };
        return statusColors[status] || 'transparent'; // Default to transparent if status is not found
    }

    // Add filter buttons to the page
    document.addEventListener('DOMContentLoaded', () => {
        const filterContainer = document.createElement('div');
        filterContainer.style.margin = '10px 0';
        filterContainer.style.textAlign = 'right'; // Align buttons to the right

        // Common button styles
        const buttonStyles = {
            marginRight: '5px',
            padding: '6px 12px', // Explicit button size from earlier implementation
            border: '1px solid #ccc',
            borderRadius: '4px',
            cursor: 'pointer',
            fontSize: '14px', // Match earlier font size
            color: 'white', // Default text color
            backgroundColor: 'transparent', // Default background color (transparent)
        };

        // Create filter buttons
        const statuses = ['pass', 'fail', 'warning', 'info', 'skip', 'error'];
        statuses.forEach(status => {
            const button = document.createElement('button');
            button.textContent = status.charAt(0).toUpperCase() + status.slice(1); // Capitalize the first letter
            Object.assign(button.style, buttonStyles); // Apply common styles
            button.classList.add('filter-button', 'active'); // Add a class for styling and mark as active
            button.setAttribute('data-status', status); // Store the status in a data attribute
            button.style.backgroundColor = getStatusColor(status); // Set the initial color based on the status
            button.onclick = () => toggleStepsByStatus(status, button);
            filterContainer.appendChild(button);
        });

        // Add a ""Clear"" button
        const clearButton = document.createElement('button');
        clearButton.textContent = 'Clear';
        Object.assign(clearButton.style, buttonStyles); // Apply common styles
        clearButton.style.marginLeft = '10px'; // Add extra margin for the ""Clear"" button
        clearButton.style.backgroundColor = 'transparent'; // Set ""Clear"" button color to transparent
        clearButton.style.color = 'black'; // Set font color to black for ""Clear"" button
        clearButton.onclick = clearAllFilters;
        filterContainer.appendChild(clearButton);

        // Add the filter container to the page
        const mainContent = document.querySelector('.main-content'); // Place buttons in the main content section
        if (mainContent) {
            mainContent.insertBefore(filterContainer, mainContent.firstChild);
        }
    });
</script>
";

            // Append the JavaScript snippet to the Spark.html file
            if (File.Exists(filePath))
            {
                string htmlContent = File.ReadAllText(filePath);

                // Insert the script before the closing </body> tag
                if (htmlContent.Contains("</body>"))
                {
                    htmlContent = htmlContent.Replace("</body>", script + "\n</body>");
                    File.WriteAllText(filePath, htmlContent);
                }
            }
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
                Media screenshotPath = CaptureScreenshot();
                if (screenshotPath == null)
                    test.Log(status, message);
                else
                {
                    //test.Log(status, message, MediaEntityBuilder.CreateScreenCaptureFromPath(screenshotPath).Build());
                    test.Log(status, message, screenshotPath);
                }
            }
            else
            {
                test.Log(status, message);
            }
        }

        public Media CaptureScreenshot()
        {
            ITakesScreenshot ts = (ITakesScreenshot)driver;
            var screenshot = ts.GetScreenshot().AsBase64EncodedString;
            string screenShotName = $"{Guid.NewGuid()}.png";

            return MediaEntityBuilder.CreateScreenCaptureFromBase64String(screenshot, screenShotName)
                .Build();
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

            // Generate a relative file path for the screenshot
            string fileName = $"{Guid.NewGuid()}.png";
            string relativeFilePath = Path.Combine(directory.Replace(reportDirectory, "."), fileName);
            string absoluteFilePath = Path.Combine(directory, fileName);

            // Capture and save the screenshot
            using (Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
                    bitmap.Save(absoluteFilePath, ImageFormat.Png);
                }
            }

            return relativeFilePath; // Return the relative path for the report
        }

        [Test]
        public void TestMethod1()
        {
            //InitializeReport();
            CreateTest("MyFirstTest");
            LogStep(Status.Pass, "This is a logging event for MyFirstTest, and it passed!");
            LogStep(Status.Pass, "This is pass!");
            LogStep(Status.Info, "This is info!");
            LogStep(Status.Info, "This is info!");
            LogStep(Status.Pass, "This is pass!");
            LogStep(Status.Fail, "This is failed!");
            CreateTest("MySecondTest");
            LogStep(Status.Fail, "This is failed!");
            LogStep(Status.Pass, "This is pass!");
            CreateTest("MyTest3");
            LogStep(Status.Info, "This is info!");
            CreateTest("MyTest4");
            LogStep(Status.Warning, "This is warning!", false);
            CreateTest("MyTest5");
            LogStep(Status.Info, "This is info!");
            LogStep(Status.Pass, "This is pass!");
            //LogStep(Status.Warning, "This is warning!");
            LogStep(Status.Pass, "This is skip!");
        }
    }
}