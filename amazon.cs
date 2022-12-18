using OpenQA.Selenium;
using System.Web;
using System.Text.Json;
using System.Reflection.Metadata;

namespace SeleniumScraper
{
    public class AmazonListing
    {
        private string _url;

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }
        private string _title;

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }
        private string _rating;

        public string Rating
        {
            get { return _rating; }
            set { _rating = value; }
        }
        private string _price;

        public string Price
        {
            get { return _price; }
            set { _price = value; }
        }
        private int _totalratings;

        public int TotalRatings
        {
            get { return _totalratings; }
            set { _totalratings = value; }
        }



        public AmazonListing( string url, string title, string rating, string price, int totalratings)
        {
            _url = url;
            _title = title;
            _rating = rating;
            _price = price;
            _totalratings = totalratings;
        }
        public override string ToString()
        {
            return $"------\ntitle: {Title}\nurl: {Url}\nPrice: {Price}\nRating: {Rating}\nTotalRatings: {TotalRatings}\n------";
        }

    }
    public static class Amazon
    {
        private static AmazonListing ScrapeAmazonUrl(string amazonurl, IWebDriver driver)
        {
            string price;
            string title;
            string totalratings, rating;
            driver.Navigate().GoToUrl(amazonurl);
            title = Program.FindElement(By.Id("productTitle"), 10).Text;
            totalratings = driver.FindElement(By.Id("acrCustomerReviewText")).Text.Replace("ratings", "");
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
            rating = driver.FindElement(By.XPath("//*[@id=\"acrPopover\"]/span[1]/a/i[1]/span")).GetAttribute("innerText");
            //the price can be on multiple places so this is how I get it
            try
            {
                price =driver.FindElement(By.Id("price")).GetAttribute("innerText");
            }
            catch (Exception)
            {
                try
                {
                    price = driver.FindElement(By.XPath("//*[@id=\"corePriceDisplay_desktop_feature_div\"]/div[1]/span/span[1]")).GetAttribute("innerText");
                }
                catch (Exception)
                {
                    try
                    {
                        price = driver.FindElement(By.XPath("//*[@id=\"corePrice_feature_div\"]/div/span/span[1]")).GetAttribute("innerText");
                    }
                    catch (Exception)
                    {
                        try
                        {
                            price = driver.FindElement(By.Id("price_inside_buybox")).GetAttribute("innerText");
                        }
                        catch (Exception)
                        {
                            price = "not found";
                        }
                    }
                }
            }
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            var amazonlisting = new AmazonListing(amazonurl, title, rating.Split(" ")[0],price, int.Parse(totalratings.Replace(",", "")));
            return amazonlisting;
        }
        public static void RunAmazonScraper(IWebDriver driver)
        {
            Console.WriteLine("Search query: ");
            var searchquery = HttpUtility.HtmlEncode(Console.ReadLine());
            Console.WriteLine("Starting to scrape requested data...");
            string[] amazonlistingurls = null;
            try
            {
                amazonlistingurls = ScrapeAmazonTop5(searchquery, driver);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured! error: " + ex.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Program.Main();
            }
            List<AmazonListing> amazonlistings = new List<AmazonListing>();
            foreach (var amazonurl  in amazonlistingurls)
            {
                var listing = ScrapeAmazonUrl(amazonurl, driver);
                amazonlistings.Add(listing);
                Console.WriteLine(listing);
            }
            Console.WriteLine("Done with scraping...");
            //export menu
            while (true)
            {
                Console.WriteLine("Do you want to export the data?: (Y)es/(N)o");
                var selection = Console.ReadLine();
                if (selection.ToLower() == "n" || selection.ToLower() == "no")
                {
                    Program.Main();
                }
                else if (selection.ToLower() == "yes" || selection.ToLower() == "y")
                {
                    break;
                }
            }
            string extension = string.Empty;
            //only supports json, location and keywords in json array
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Json File (.json)|*.json";
            if (save.ShowDialog() == DialogResult.OK)
            {
                extension = Path.GetExtension(save.FileName);
                string data = string.Empty;
                if (extension == ".json")
                {
                    data = JsonSerializer.Serialize(amazonlistings);
                }
                using (StreamWriter writer = new StreamWriter(save.OpenFile()))
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        writer.Write(data[i]);
                    }
                }
            }
        }

        private static string[] ScrapeAmazonTop5(string search, IWebDriver driver)
        {
            //search
            driver.Navigate().GoToUrl("https://www.amazon.com/s?k=" + search);
            List<string> amazonlistingurls = new List<string>();
            //loop through job elements
            foreach (var item in driver.FindElements(By.XPath(".//div[contains(@data-component-type,\"s-search-result\")]")))
            {
                if (amazonlistingurls.Count < 5)
                {
                    //File.WriteAllText("html.html", item.GetAttribute("innerHTML"));
                    string url = item.FindElement(By.TagName("a")).GetAttribute("href");
                    amazonlistingurls.Add(url);
                }
                else
                {
                    //if scraped enough then break and return
                    break;
                }
            }
            return amazonlistingurls.ToArray();
        }
    }
}
