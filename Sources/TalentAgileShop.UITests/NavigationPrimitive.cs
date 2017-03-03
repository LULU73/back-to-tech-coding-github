using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.PhantomJS;

namespace TalentAgileShop.UITests
{

    internal sealed class NavigationPrimitives
    {
        private readonly TestContext _context;

        public string SiteUrl { get; }

        public IWebDriver WebDriver { get; private set; }

        private string GetTestProperty(string propertyName)
        {
            var result = _context?.Properties[propertyName] as string;
            return result;
        }


        public void TakeScreenshotIfCurrentTestFailed()
        {
            if (_context.CurrentTestOutcome != UnitTestOutcome.Failed)
            {
                return;
            }
            try
            {
                string fileNameBase =
                    $"error_{_context.TestName}_{DateTime.Now:yyyyMMdd_HHmmss}";

                if (!Directory.Exists(_context.ResultsDirectory))
                    Directory.CreateDirectory(_context.ResultsDirectory);

                var pageSource = WebDriver.PageSource;
                var sourceFilePath = Path.Combine(_context.ResultsDirectory, fileNameBase + "_source.html");
                File.WriteAllText(sourceFilePath, pageSource, Encoding.UTF8);
                _context.AddResultFile(sourceFilePath);
                Console.WriteLine("Page source: {0}", new Uri(sourceFilePath));

                var takesScreenshot = WebDriver as ITakesScreenshot;

                if (takesScreenshot == null)
                { return;}

                var screenshot = takesScreenshot.GetScreenshot();

                var screenshotFilePath = Path.Combine(_context.ResultsDirectory, fileNameBase + "_screenshot.png");

                screenshot.SaveAsFile(screenshotFilePath, ScreenshotImageFormat.Png);
                _context.AddResultFile(screenshotFilePath);
                Console.WriteLine("Screenshot: {0}", new Uri(screenshotFilePath));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while taking screenshot: {0}", ex);
            }
        }

        private IWebDriver CreateWebDriver()
        {
            var driverName = GetTestProperty("webDriver");


            if (string.Compare(driverName, "chrome", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                var options = new ChromeOptions();
                options.AddArgument("-incognito ");
                options.AddArgument("--start-maximized");
                var location = GetTestProperty("chromeDriverLocation");
                var webDriver = new ChromeDriver(location, options);
                return webDriver;
            }

            if (string.Compare(driverName, "phantomJs", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                var driver = new PhantomJSDriver();

                driver.Manage().Window.Size = new Size(1200, 1000);
                return driver;
            }

            if (string.IsNullOrEmpty(driverName))
            {
                throw new InvalidOperationException("Driver name missing: do you have selected a test setting file?");
            }
            throw new InvalidOperationException($"Invalid driver name: {driverName}");
        }




        public NavigationPrimitives(TestContext context)
        {
            _context = context;
           
            SiteUrl = GetTestProperty("siteUrl");
        }


        public void InitializeBrowser()
        {
            if (WebDriver == null)
            {
                DisposeBrowser();
            }

            WebDriver = CreateWebDriver();
        }

        public void DisposeBrowser()
        {
            if (WebDriver == null)
            {
                return;
            }

            WebDriver.Close();
            WebDriver.Dispose();
            WebDriver = null;
        }

   

        public NavigationPrimitives WhenINavigateToTheHomepage()
        {


            WebDriver.Navigate().GoToUrl(SiteUrl);
            return this;
        }

        public NavigationPrimitives WhenINavigateToTheCatalogPage()
        {


            WebDriver.Navigate().GoToUrl(SiteUrl + "/catalog");
            return this;
        }
        public NavigationPrimitives WhenINavigateToTheCartPage()
        {


            WebDriver.Navigate().GoToUrl(SiteUrl + "/cart");
            return this;
        }


        public NavigationPrimitives WhenINavigateToThisProductPage(string id)
        {


            var url = $"{SiteUrl}/products/{id}";

            WebDriver.Navigate().GoToUrl(url);
            return this;
        }



        public NavigationPrimitives WhenIClickOnCatalog()
        {
            var element = WebDriver.FindElement(By.Id("catalogMenuLink"));

            Check.That(element.TagName).IsEqualTo("a");

            element.Click();

            return this;
        }



        public NavigationPrimitives WhenIClickOnAddToCartButton()
        {
            var element = WebDriver.FindElement(By.Id("addToCartButton"));

            Check.That(element.TagName).IsEqualTo("a");

            element.Click();
            return this;
        }

        public NavigationPrimitives WhenISwitchToThumbnailView()
        {
            var element = WebDriver.FindElement(By.Id("goToThumbnailView"));

            Check.That(element.TagName).IsEqualTo("a");

            element.Click();
            return this;
        }


        public NavigationPrimitives WhenISwitchToListView()
        {
            var element = WebDriver.FindElement(By.Id("goToListView"));

            Check.That(element.TagName).IsEqualTo("a");

            element.Click();
            return this;
        }

        public NavigationPrimitives WhenISelectThisDiscountCode(string discountCode)
        {
            var discountCodeTextBox = WebDriver.FindElement(By.Id("discountCode"));

            var discountCodeButton = WebDriver.FindElement(By.Id("changeCodeBtn"));
            discountCodeTextBox.Clear();
            discountCodeTextBox.SendKeys(discountCode);
            discountCodeButton.Click();
            return this;
        }



        public NavigationPrimitives ThenIShouldSeeTheWelcomeText()
        {
            var element = WebDriver.FindElement(By.Id("startShopping"));

            Check.That(element).IsNotNull();
            Check.That(element.TagName).IsEqualTo("a");
            return this;
        }


        public NavigationPrimitives ThenIShouldSeeTheProductList()
        {
            Check.ThatCode(() =>
            {
                WebDriver.FindElement(By.Id("catalogList"));
            }).DoesNotThrow();
            return this;

        }


        public NavigationPrimitives ThenIShouldSeeTheCategoryFilters()
        {
            Check.ThatCode(() =>
            {
                WebDriver.FindElement(By.Id("categories"));
            }).DoesNotThrow();
            return this;

        }


        public NavigationPrimitives ThenIShouldSeeThisLog(int logCount, string log)
        {
            var allLogs = WebDriver
                .FindElements(By.ClassName("alert-success"))
                .ToList();

            var validLogs = allLogs.Where(element => element.Text.Contains(log));

            Check.That(validLogs.Count()).IsEqualTo(logCount);
            return this;
        }


        public NavigationPrimitives ThenICanSwitchToThumbnailView()
        {
            Check.ThatCode(() =>
            {
                WebDriver.FindElement(By.Id("goToThumbnailView"));
            }).DoesNotThrow();
            return this;
        }



        public NavigationPrimitives ThenICanSwitchToListView()
        {
            Check.ThatCode(() =>
            {
                WebDriver.FindElement(By.Id("goToListView"));
            }).DoesNotThrow();
            return this;
        }

        private string FormatCost(string cost)
        {
            return cost.Replace(",", ".");
        }


        public NavigationPrimitives ThenTheProductCostIs(decimal expectedProductCost)
        {
            var productCostElement = WebDriver.FindElement(By.Id("productCost"));

            var cost = decimal.Parse(FormatCost(productCostElement.Text), NumberStyles.AllowDecimalPoint,CultureInfo.CurrentUICulture.NumberFormat);

            Check.That(cost).IsEqualTo(expectedProductCost);
            return this;
        }


        public NavigationPrimitives ThenTheDeliveryCostIs(decimal expectedDeliveryCost)
        {
            var productCostElement = WebDriver.FindElement(By.Id("deliveryCost"));

            var cost = decimal.Parse(FormatCost(productCostElement.Text), NumberStyles.AllowDecimalPoint);

            Check.That(cost).IsEqualTo(expectedDeliveryCost);
            return this;
        }
    }
}