using System;
using System.IO;
using OpenQA.Selenium;

namespace test_etesthub.Helpers
{
    public static class ScreenshotHelper
    {
        // Hàm chụp màn hình và trả về đường dẫn file ảnh
        public static string TakeScreenshot(IWebDriver driver, string testCaseName)
        {
            string screenshotFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");
            if (!Directory.Exists(screenshotFolder))
            {
                Directory.CreateDirectory(screenshotFolder);
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"{testCaseName}_{timestamp}.png";
            string filePath = Path.Combine(screenshotFolder, fileName);

            ITakesScreenshot ts = (ITakesScreenshot)driver;
            Screenshot screenshot = ts.GetScreenshot();

            screenshot.SaveAsFile(filePath);

            return filePath;
        }
    }
}