using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodReadsScraper.Models;
using HtmlAgilityPack;

namespace GoodReadsScraper.Services
{
    public class ExtractorService
    {
        private HtmlDocument _htmlDocument;

        public ExtractorService(HtmlDocument htmlDocument)
        {
            _htmlDocument = htmlDocument;
        }
        public Book GetBookDetails()
        {
            var bookDetails = new Book();

            // Extract the title
            var titleNode = _htmlDocument.DocumentNode.SelectSingleNode("//h1[contains(@class, 'H1Title')]");
            bookDetails.Title = titleNode?.InnerText.Trim();

            // Extract the author
            // In some cases, authors are under "ContributorLinksList" with "ContributorLink__name" class
            var authorNode = _htmlDocument.DocumentNode.SelectSingleNode("//div[@class='ContributorLinksList']//a[@class='ContributorLink']//span[@data-testid='name']");
            bookDetails.Author = authorNode?.InnerText.Trim();

            // Extract the image URL
            var imageNode = _htmlDocument.DocumentNode.SelectSingleNode("//img[contains(@class, 'ResponsiveImage')]");
            bookDetails.Image = imageNode?.GetAttributeValue("src", string.Empty);

            // Extract the book URL
            var urlNode = _htmlDocument.DocumentNode.SelectSingleNode("//a[contains(@class, 'BookCard__interactive')]");
            bookDetails.BookUrl = urlNode?.GetAttributeValue("href", string.Empty);

            return bookDetails;
        }

        public List<Review> GetReviews()
        {
            var reviews = new List<Review>();

            // Select the nodes containing reviews
            var reviewNodes = _htmlDocument.DocumentNode.SelectNodes("//article[contains(@class, 'ReviewCard')]");

            if (reviewNodes != null)
            {
                foreach (var reviewNode in reviewNodes)
                {
                    var review = new Review();

                    // Extract the user name
                    var userNode = reviewNode.SelectSingleNode(".//div[@data-testid='name']");
                    review.User = userNode?.InnerText.Trim();

                    // Extract the review text
                    var reviewTextNode = reviewNode.SelectSingleNode(".//section[@class='ReviewCard__content']//span[@class='Formatted']");
                    review.Text = reviewTextNode?.InnerText.Trim();

                    // Extract the star rating
                    var starNode = reviewNode.SelectSingleNode(".//span[contains(@class, 'RatingStars__small')]/@aria-label");
                    var starText = starNode?.GetAttributeValue("aria-label", string.Empty);
                    review.Stars = ExtractStarsFromAriaLabel(starText);

                    reviews.Add(review);
                }
            }

            return reviews;
        }

        private int ExtractStarsFromAriaLabel(string ariaLabel)
        {
            if (string.IsNullOrEmpty(ariaLabel))
                return 0;

            // Example: aria-label="Rating 5 out of 5"
            var parts = ariaLabel.Split(' ');
            if (parts.Length >= 2 && int.TryParse(parts[1], out var stars))
            {
                return stars;
            }

            return 0; // No valid stars found
        }
    }
}
