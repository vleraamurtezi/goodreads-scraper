using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace GoodReadsScraper.Services
{
    public class ScraperService : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public ScraperService()
        {
            var chromeOptions = new ChromeOptions();
            //chromeOptions.AddArgument("--headless"); // Run Chrome in headless mode (without opening a browser window)
            chromeOptions.AddArgument("--no-sandbox");
            chromeOptions.AddArgument("--disable-dev-shm-usage");
            chromeOptions.AddArgument("log-level=3"); // Suppress console logs
            chromeOptions.AddArgument("--silent"); // Additional option to silence output
            chromeOptions.AddArgument("--disable-logging"); // Disable logging

            // Redirect WebDriver logs to nowhere
            var service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true; // Suppress initial diagnostics
            service.HideCommandPromptWindow = true; // Hide the command prompt window

            // Temporarily suppress console output
            var originalConsoleOut = Console.Out;
            var originalConsoleError = Console.Error;

            try
            {
                using (var nullOutput = new StreamWriter(Stream.Null))
                {
                    Console.SetOut(nullOutput);
                    Console.SetError(nullOutput);

                    // Initialize the driver while suppressing output
                    _driver = new ChromeDriver(service, chromeOptions);
                    _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));

                }
            }
            finally
            {
                // Restore console output
                Console.SetOut(originalConsoleOut);
                Console.SetError(originalConsoleError);
            }
        }

        public HtmlDocument GetPageHtml(string url)
        {
            try
            {
                // Navigate to the page
                _driver.Navigate().GoToUrl(url);

                // Click the Filters button
                ClickFiltersButton();

                Thread.Sleep(TimeSpan.FromSeconds(3));

                // Select the English language filter
                SelectEnglishLanguageFilter();

                // Wait for the reviews to load
                //WaitForElementToBeClickable(By.XPath("//div[contains(@class, 'ReviewCard')]"), TimeSpan.FromSeconds(30));

                // Scroll down to load more reviews, if necessary (for pagination or lazy-loading)
                ScrollToBottom();

                // Extract page source (HTML content)
                var pageSource = _driver.PageSource;

                // Load the HTML into HtmlAgilityPack's HtmlDocument
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(pageSource);

                return htmlDocument;
            }
            catch
            {
                return null;
            }
            
        }

        private void ClickFiltersButton()
        {
            // Wait for the Filters button to be clickable and click it
            var filtersButton = WaitForElementToBeClickable(By.XPath("//button[contains(., 'Filters')]"), TimeSpan.FromSeconds(10));
            filtersButton.Click();
        }

        private void SelectEnglishLanguageFilter()
        {
            // Wait for the label that has for="en" to be clickable and click it
            var englishRadioLabel = WaitForElementToBeClickable(By.XPath("//label[@for='en']"), TimeSpan.FromSeconds(10));
            englishRadioLabel.Click();

            var applyButton = WaitForElementToBeClickable(By.XPath("//span[contains(text(), 'Apply')]"), TimeSpan.FromSeconds(10));
            applyButton.Click();
        }

        private void ScrollToBottom()
        {
            var jsExecutor = (IJavaScriptExecutor)_driver;
            var currentHeight = (long)0;
            var newHeight = (long)0;

            do
            {
                currentHeight = newHeight;
                jsExecutor.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
                System.Threading.Thread.Sleep(1000);
                newHeight = (long)jsExecutor.ExecuteScript("return document.body.scrollHeight;");
            } while (newHeight > currentHeight);
        }

        private IWebElement WaitForElementToBeClickable(By by, TimeSpan timeout)
        {
            try
            {
                return _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(by));
            }
            catch (WebDriverTimeoutException)
            {
                throw new TimeoutException($"Element located by {by} was not clickable within the timeout period.");
            }
        }

        public void Dispose()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
    }
}
