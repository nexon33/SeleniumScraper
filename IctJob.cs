using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Text.Json;
using System.Security.Policy;

namespace SeleniumScraper
{
    public class IctJobListing
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
        private string _company;

        public string Company
        {
            get { return _company; }
            set { _company = value; }
        }
        private string[] _locations;

        public string[] Locations
        {
            get { return _locations; }
            set { _locations = value; }
        }
        private string[] _keywords;

        public string[] Keywords
        {
            get { return _keywords; }
            set { _keywords = value; }
        }



        public IctJobListing( string url, string title, string company, string[] locations, string[] keywords)
        {
            _url = url;
            _title = title;
            _company = company;
            _locations = locations;
            _keywords = keywords;
        }
        public override string ToString()
        {
            string keywords = string.Empty;
            foreach (var keyw in Keywords)
            {
                keywords += keyw.ToString() + ",";
            }
            string locations = string.Empty;
            foreach (var keyw in Locations)
            { 
                locations += keyw.ToString() + ",";
            }
            return $"------\ntitle: {Title}\nurl: {Url}\ncompany: {Company}\nlocation(s): {locations}\nkeyword(s): {keywords.TrimEnd(',')}\n------";
        }

    }
    public static class IctJob
    {
        public static void RunIctJobListingCrawler(IWebDriver driver)
        {
            Console.WriteLine("Search query: ");
            var searchquery = HttpUtility.HtmlEncode(Console.ReadLine());
            Console.WriteLine("Starting to scrape requested data...");
            IctJobListing[] joblistings = null;
            try
            {
                joblistings = ScrapeIctJobListingTop5Search(searchquery, driver);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured! error: " + ex.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Program.Main();
            }
            Console.WriteLine("Done with scraping...");
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
            
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Json File (.json)|*.json";
            if (save.ShowDialog() == DialogResult.OK)
            {
                extension = Path.GetExtension(save.FileName);
                string data = string.Empty;
                if (extension == ".json")
                {
                    data = JsonSerializer.Serialize(joblistings);
                }
                using (StreamWriter writer = new StreamWriter(save.OpenFile()))
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        writer.Write(data[i]);
                    }
                }
            }

            Console.WriteLine("Export as " + extension);
        }

        private static IctJobListing[] ScrapeIctJobListingTop5Search(string search, IWebDriver driver)
        {
            
            driver.Navigate().GoToUrl("https://www.ictjob.be/nl/it-vacatures-zoeken?keywords="+ search + "&keywords_options=OR&SortOrder=DESC&SortField=DATE&From=0&To=19");
            List<IctJobListing> joblisturls = new List<IctJobListing>();
            foreach (var item in driver.FindElements(By.XPath("//*[@id=\"search-result-body\"]/div/ul/li")))
            {
                if (joblisturls.Count < 5)
                {
                    var classes = item.GetAttribute("class");
                    if (!classes.Contains("create-job-alert-search-item"))
                    {
                        string title=null, url = null, company = null, location = null, keywords = null;
                        foreach (var item2 in item.FindElements(By.TagName("a")))
                        {
                            
                            if (item2.GetAttribute("itemprop") == "title")
                            {
                                title = item2.GetAttribute("innerText");
                                url = item2.GetAttribute("href");
                                
                                break;
                            }
                        }
                        foreach (var span in item.FindElements(By.TagName("span")))
                        {
                            if(span.GetAttribute("class") == "job-company")
                            {
                                company = span.GetAttribute("innerText");
                            }
                            else if (span.GetAttribute("itemprop")=="addressLocality")
                            {
                                location = span.GetAttribute("innerText");
                            }
                            else if (span.GetAttribute("class")=="job-keywords")
                            {
                                keywords = span.GetAttribute("innerText");
                            }
                        }
                        var joblisting = new IctJobListing(url, title, company,location.Split(','), keywords.Split(','));
                        Console.WriteLine(joblisting);
                        joblisturls.Add(joblisting);
                    }
                }
                else
                {
                    break;
                }
            }
            return joblisturls.ToArray();
        }
    }
}
