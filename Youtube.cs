using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System.Web;
using System.Text.Json;
using System.Windows.Forms;

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

                //first crawl the top 5 urls
                string[] results = ScrapeYoutubeTop5Search(searchquery, driver);
                Thread.Sleep(1000);
                //scrape the data from the urls for each video
                foreach (var yturl in results)
                {
                    var vid = ScrapeYoutubeUrl(yturl, driver);
                    Console.WriteLine(vid);
                    //add to data
                    vidslist.Add(vid);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured! error: " + ex.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Program.Main();
            }
            Console.WriteLine("Done with scraping...");
            //export menu
            while (true)
            {
                Console.WriteLine("Do you want to export the data?: (Y)es/(N)o");
                var selection = Console.ReadLine();
                if (selection.ToLower() == "n" || selection.ToLower() == "no")
                {
                    //go back to main menu if no
                    Program.Main();
                }
                else if (selection.ToLower() == "yes" || selection.ToLower() == "y")
                {
                    //continue if yes
                    break;
                }
            }
            string extension = string.Empty;
            //show savefiledialog to choose format and where to save
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Csv File (.csv)|*.csv|Json File (.json)|*.json";
            if (save.ShowDialog() == DialogResult.OK)
            {
                extension = Path.GetExtension(save.FileName);
                string data = string.Empty;
                if (extension == ".csv")
                {
                    //export as csv (not escaped)
                    data = "Uploader;Url;Views;Title\n";
                    foreach (var vid in vidslist)
                    {
                        data += $"{vid.Uploader};{vid.Url};{vid.Views};{vid.Title}\n";
                    }
                }
                else if (extension == ".json")
                {
                    //export as json
                    data = JsonSerializer.Serialize(vidslist);
                }

                //do the actual export of data to file.
                using (StreamWriter writer = new StreamWriter(save.OpenFile()))
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        writer.Write(data[i]);
                    }
                }
            }

        }
        private static int ParseViewCount(string text)
        {
            //this function gets calculates the views as shown on youtube (1,2 mln for example equals to 1200000)
            int views;
            if (text.EndsWith("mln"))
            {
                views = (int)(decimal.Parse(text.Split(" ")[0]) * (decimal)1000000);
            }
            else if (text.EndsWith("K"))
            {
                views = (int)(decimal.Parse(text.Replace("K", "")) * (decimal)1000);
            }
            else
            {
                views = int.Parse(text);
            }
            return views;
        }
        private static YoutubeVid ScrapeYoutubeUrl(string yturl, IWebDriver driver)
        {
            //function to scrape youtube video data from youtube video url
            string uploader;
            string title;
            int views;

            driver.Navigate().GoToUrl(yturl);
            Thread.Sleep(200);
            //supports youtube short videos
            if (yturl.Contains(".youtube.com/shorts"))
            {
                //navigate to sort get data
                uploader = driver.FindElement(By.XPath("//*[@id=\"text-container\"]/yt-formatted-string/a")).GetAttribute("innerText");
                title = driver.FindElement(By.XPath("//*[@id=\"overlay\"]/ytd-reel-player-header-renderer/h2/yt-formatted-string/span[1]")).GetAttribute("innerText");
                string viewsraw = driver.FindElement(By.XPath("//*[@id=\"like-button\"]/yt-button-shape/label/div/span")).GetAttribute("innerText");
                //parse views amount
                views = ParseViewCount(viewsraw);
            }
            else
            {
                //navigate to video and get date
                driver.FindElement(By.Id("description-inline-expander")).Click();
                Thread.Sleep(100);
                //get views
                string viewsraw = driver.FindElement(By.XPath("//*[@id=\"info\"]/span[1]")).GetAttribute("innerText").Split(' ')[0].Replace(".", "");
                //parse views amount
                views = ParseViewCount(viewsraw);
                uploader = driver.FindElement(By.XPath("//*[@id=\"channel-name\"]/div/div/yt-formatted-string/a")).GetAttribute("innerText");
                title = driver.FindElement(By.XPath("//*[@id=\"title\"]/h1/yt-formatted-string")).GetAttribute("innerText");
            }
            var ytvid = new YoutubeVid(views, title, uploader, yturl);
            return ytvid;
        }

        private static bool FirstRun = true;
        private static string[] ScrapeYoutubeTop5Search(string search, IWebDriver driver)
        {
            // if first run dismiss cookies
            driver.Navigate().GoToUrl("https://www.youtube.com/results?search_query=" + search);
            if (FirstRun)
            {
                driver.FindElement(By.XPath("//*[@id=\"content\"]/div[2]/div[6]/div[1]/ytd-button-renderer[2]/yt-button-shape/button/yt-touch-feedback-shape/div/div[2]")).Click();
                FirstRun = false;
            }
            //set maxreload, this will be used if something goes wrong with the scraper for whatever reason.
            int maxreload = 0;
        reloadcount:
            maxreload++;
            if (maxreload > 10)
            {
                throw new Exception("Stuck in reload loop.");
            }
            List<IWebElement> ytvids = new List<IWebElement>();
            //scrape video's
            _ = Program.FindElement(By.Id("video-title"), 10);
            Thread.Sleep(500);
            ytvids.AddRange(driver.FindElements(By.Id("video-title")).Take(20));
            if (ytvids.Count < 10)
            {
                //if there need to be more videos then retry
                goto reloadcount;
            }
            //get 5 results
            string[] resulturls = new string[5];
            int count = 0;
            try
            {
                //do this loop to scroll through the videos so they load and the data can be scraped.
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
                //if fail then retry
                goto reloadcount;
            }

            return resulturls;
        }
    }
}
