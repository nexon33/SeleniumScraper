using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
namespace SeleniumScraper
{
    internal static class Program
    {
        private static IWebDriver driver = null;
        private static void ConsoleExit(object sender, EventArgs e)
        {
            driver.Quit();
        }

        [STAThread]
        public static void Main()
        {
            if (driver == null)
            {
                AppDomain.CurrentDomain.ProcessExit += new EventHandler(ConsoleExit);
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