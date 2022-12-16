using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web;
using System.Windows;
namespace SeleniumScraper
{
    internal static class Program
    {
        private static IWebDriver driver = null;

        static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                driver.Quit();
            }
            return false;
        }
        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
                                               // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);


        /// <summary>Defines the entry point of the application.</summary>
        /// 
        [STAThread]
        public static void Main()
        {
            if (driver == null)
            {
                handler = new ConsoleEventDelegate(ConsoleEventCallback);
                SetConsoleCtrlHandler(handler, true);
                ChromeOptions options = new ChromeOptions();
                var chromeDriverService = ChromeDriverService.CreateDefaultService();
                chromeDriverService.HideCommandPromptWindow = true;
                chromeDriverService.SuppressInitialDiagnosticInformation = true;
                options.AddArgument("--disable-logging");
                options.AddArgument("log-level=3");
                //options.AddArgument("headless");
                options.AddArgument("--mute-audio");
                driver = new ChromeDriver(chromeDriverService, options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            }
            Console.WriteLine("---------------\nWelcome to the menu!\n---------------\n");
            while (true)
            {
                Console.WriteLine("Choose an option:");
                Console.WriteLine("(1) Youtube Search Top 5");
                Console.WriteLine("(2) IctJob Search 5 most recent");
                Console.WriteLine("(3) Exit Scraper");
                var result = Console.ReadLine();
                if (result == "1")
                {
                    Youtube.RunYoutubeCrawler(driver);
                }
                else if (result == "2")
                {
                    IctJob.RunIctJobListingCrawler(driver);
                }
                else if (result == "3")
                {
                    Environment.Exit(0);
                }

            }

        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            driver.Quit();
        }
        
        public static IWebElement FindElement(By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv => drv.FindElement(by));
            }
            return driver.FindElement(by);
        }
    }
}