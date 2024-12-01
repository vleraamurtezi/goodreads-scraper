using CsvHelper;
using GoodReadsScraper.Models;
using GoodReadsScraper.Services;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Globalization;

namespace GoodReadsScraper
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Prepare();
        }

        static void Scrape()
        {
            var baseUrl = "https://www.goodreads.com";
            var categories = new DirectoryInfo("URLs").GetFiles().ToList();
            var scraperService = new ScraperService();

            foreach (var category in categories)
            {
                var alreadyCrawled = new DirectoryInfo($"{category.FullName.Replace(".txt", "")}").GetFiles().ToList()
                    .Count;
                if (alreadyCrawled == 100)
                    continue;
                var allUrls = File.ReadAllLines(category.FullName).Skip(alreadyCrawled);
                var counter = alreadyCrawled;
                Console.WriteLine(category.Name.Replace(".txt", "").ToUpper() + ":");
                foreach (var slug in allUrls)
                {

                    var url = $"{baseUrl}{slug}/reviews";

                    var htmlDocument = scraperService.GetPageHtml(url);
                    if (htmlDocument == null)
                    {
                        htmlDocument = scraperService.GetPageHtml(url);
                        if (htmlDocument == null)
                            continue;
                    }

                    // Pass the HtmlDocument to ExtractService for data extraction
                    var extractorService = new ExtractorService(htmlDocument);
                    var book = extractorService.GetBookDetails();
                    var reviews = extractorService.GetReviews();

                    var bookReviews = new BookReviews()
                    {
                        Book = book,
                        Reviews = reviews,
                    };

                    File.WriteAllText($"{category.FullName.Replace(".txt", "")}/{counter}.txt",
                        JsonConvert.SerializeObject(bookReviews));
                    counter++;
                    // Output the extracted data
                    Console.WriteLine($"{counter}. {book.Title} by {book.Author} with {reviews.Count} reviews!");
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }

                Console.WriteLine("\n");
            }
        }

        static void Prepare()
        {
            var records = new List<Output>();
            var categories = new DirectoryInfo("URLs").GetDirectories().ToList();
            foreach (var category in categories)
            {
                var files = new DirectoryInfo(category.FullName).GetFiles();
                foreach (var file in files)
                {
                    var content = File.ReadAllText(file.FullName);
                    var data = JsonConvert.DeserializeObject<BookReviews>(content);
                    foreach (var dataReview in data.Reviews)
                    {
                        var output = new Output
                        {
                            Reviwer = dataReview.User,
                            Genre = category.Name,
                            Book = data.Book.Title,
                            Author = data.Book.Author,
                            Review = dataReview.Text,
                            Starts = dataReview.Stars
                        };
                        records.Add(output);
                    }

                }
            }

            using (var writer = new StreamWriter("output.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(records);
            }

        }
    }
}