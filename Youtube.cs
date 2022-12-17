﻿using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System.Web;
using System.Text.Json;

namespace SeleniumScraper
{
    public class YoutubeVid
    {
        private int _views;

        public int Views
        {
            get { return _views; }
            set { _views = value; }
        }
        private string _title;

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }
        private string _uploader;

        public string Uploader
        {
            get { return _uploader; }
            set { _uploader = value; }
        }
        private string _url;

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        public YoutubeVid( int views, string title, string uploader, string url)
        {
            _views = views;
            _title = title;
            _uploader = uploader;
            _url = url;
        }
        public override string ToString()
        {
            return $"------\nUrl: {Url}\nTitle: {Title}\nUploader: {Uploader}\nViews: {Views}\n------";
        }

    }
    public static class Youtube
    {
        public static void RunYoutubeCrawler(IWebDriver driver)
        {
            Console.WriteLine("Search query: ");
            var searchquery = HttpUtility.HtmlEncode(Console.ReadLine());
            List<YoutubeVid> vidslist = new List<YoutubeVid>();
            Console.WriteLine("Starting to scrape requested data...");
            try
            {
                string[] results = ScrapeYoutubeTop5Search(searchquery, driver);
                Thread.Sleep(1000);
                foreach (var yturl in results)
                {
                    var vid = ScrapeYoutubeUrl(yturl, driver);
                    Console.WriteLine(vid);
                    vidslist.Add(vid);
                }
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
            save.Filter = "Csv File (.csv)|*.csv|Json File (.json)|*.json";
            if (save.ShowDialog() == DialogResult.OK)
            {
                extension = Path.GetExtension(save.FileName);
                MessageBox.Show(extension);
                string data = string.Empty;
                if (extension == ".csv")
                {
                    data = "Uploader;Url;Views;Title\n";
                    foreach (var vid in vidslist)
                    {
                        data += $"{vid.Uploader};{vid.Url};{vid.Views};{vid.Title}\n";
                    }
                }
                else if (extension == ".json")
                {
                    data = JsonSerializer.Serialize(vidslist);
                }
                using (StreamWriter writer = new StreamWriter(save.OpenFile()))
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        writer.Write(data[i]);
                    }
                }
                MessageBox.Show("works");
            }

            Console.WriteLine("Export as " + extension);
        }
        private static YoutubeVid ScrapeYoutubeUrl(string yturl, IWebDriver driver)
        {
            string uploader;
            string title;
            int views;

            driver.Navigate().GoToUrl(yturl);
            Thread.Sleep(200);
            if (yturl.Contains(".youtube.com/shorts"))
            {
                uploader = driver.FindElement(By.XPath("//*[@id=\"text-container\"]/yt-formatted-string/a")).GetAttribute("innerText");
                title = driver.FindElement(By.XPath("//*[@id=\"overlay\"]/ytd-reel-player-header-renderer/h2/yt-formatted-string/span[1]")).GetAttribute("innerText");
                string viewsraw = driver.FindElement(By.XPath("//*[@id=\"like-button\"]/yt-button-shape/label/div/span")).GetAttribute("innerText");
                if (viewsraw.EndsWith("mln"))
                {
                    views = (int)(decimal.Parse(viewsraw.Split(" ")[0]) * (decimal)1000000);
                }
                else if (viewsraw.EndsWith("K"))
                {
                    views = (int)(decimal.Parse(viewsraw.Replace("K", "")) * (decimal)1000);
                }
                else
                {
                    views = int.Parse(viewsraw);
                }
            }
            else
            {
                driver.FindElement(By.Id("description-inline-expander")).Click();
                Thread.Sleep(100);
                //get views
                string viewsraw = driver.FindElement(By.XPath("//*[@id=\"info\"]/span[1]")).GetAttribute("innerText").Split(' ')[0].Replace(".", "");
                if (viewsraw.EndsWith("mln"))
                {
                    views = (int)(decimal.Parse(viewsraw.Split(" ")[0]) * (decimal)1000000);
                }
                else if (viewsraw.EndsWith("K"))
                {
                    views = (int)(decimal.Parse(viewsraw.Replace("K", "")) * (decimal)1000);
                }
                else
                {
                    views = int.Parse(viewsraw);
                }
                uploader = driver.FindElement(By.XPath("//*[@id=\"channel-name\"]/div/div/yt-formatted-string/a")).GetAttribute("innerText");
                title = driver.FindElement(By.XPath("//*[@id=\"title\"]/h1/yt-formatted-string")).GetAttribute("innerText");
            }
            var ytvid = new YoutubeVid(views, title, uploader, yturl);
            return ytvid;
        }

        private static bool FirstRun = true;
        private static string[] ScrapeYoutubeTop5Search(string search, IWebDriver driver)
        {

            driver.Navigate().GoToUrl("https://www.youtube.com/results?search_query=" + search);
            if (FirstRun)
            {
                driver.FindElement(By.XPath("//*[@id=\"content\"]/div[2]/div[6]/div[1]/ytd-button-renderer[2]/yt-button-shape/button/yt-touch-feedback-shape/div/div[2]")).Click();
                FirstRun = false;
            }
            int maxreload = 0;
        reloadcount:
            maxreload++;
            if (maxreload > 10)
            {
                throw new Exception("Stuck in reload loop.");
            }
            List<IWebElement> ytvids = new List<IWebElement>();
            var dsf = Program.FindElement(By.Id("video-title"), 10);
            Thread.Sleep(500);
            ytvids.AddRange(driver.FindElements(By.Id("video-title")).Take(10));
            if (ytvids.Count < 6)
            {
                goto reloadcount;
            }
            string[] resulturls = new string[5];
            int count = 0;
            try
            {
                foreach (var ytvid in ytvids)
                {
                    Actions actions = new Actions(driver);
                    actions.MoveToElement(ytvid);
                    actions.Perform();
                    Thread.Sleep(100);
                    if (count != 5 && !string.IsNullOrWhiteSpace(ytvid.GetAttribute("href")))
                    {
                        resulturls[count] = ytvid.GetAttribute("href");
                        count++;
                    }
                }
            }
            catch (Exception)
            {
                goto reloadcount;
            }

            return resulturls;
        }
    }
}