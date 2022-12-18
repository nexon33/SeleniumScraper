# SeleniumScraper Documentation

This is a practice project written in .NET 6

#features

The selenium scraper supports scaping the top 5 videos found on your youtube searchterm. It is also possible to get the 5 most recent joblistings on ictjob.be
The chrome selenium instance is configured to run headless and without sound and code is added to make sure the selenium instance stops running when exiting the application. (Does not work when pressing stop debug)
For the youtube scraper you can export the videos as .json or .csv and the scraper also supports youtube short videos. 
The views count of the youtube videos gets converted to an integer for easy use.
The ictjob results also seperates the keyword(s) and location(s) if applicable. 
The amazon scraper does basically the same thing as the other scrapers, I did not convert the price to decimal because there are multiple currencies on amazon.