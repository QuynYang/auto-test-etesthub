using System;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using test_etesthub.Pages;
using test_etesthub.Helpers;

namespace test_etesthub.Tests
{
    [TestFixture]
    public class LoginLogicTests
    {
        private IWebDriver _driver;
        private LoginPage _loginPage;
        private ExcelReportManager _excelManager;
        private string _currentTestCaseId = "";

        private readonly string _loginUrl = "https://e-testhub-frontend.onrender.com/Home/Login";

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            string absolutePath = @"C:\Users\vungo\OneDrive\Tài liệu\2025\BDCLPM\LT\baocao.xlsx";
            _excelManager = new ExcelReportManager(absolutePath);
            _excelManager.OpenReport();
        }

        [SetUp]
        public void Setup()
        {
            _driver = new ChromeDriver();
            _driver.Manage().Window.Maximize();
            _loginPage = new LoginPage(_driver);
            _loginPage.GoToLoginPage(_loginUrl);
        }

        [Test]
        public void S1_9_DangNhap_EmailHopLe_MatKhauDung()
        {
            _currentTestCaseId = "S.1.9";

            string[] testData = _excelManager.GetTestDataArray(_currentTestCaseId);

            string role = testData.Length > 0 ? testData[0] : "";
            string email = testData.Length > 1 ? testData[1] : "";
            string password = testData.Length > 2 ? testData[2] : "";

            TestContext.WriteLine($"[DATA] Hệ thống lấy được -> Role: {role} | Email: {email} | Pass: {password}");

            string roleLower = role.ToLower();
            if (roleLower.Contains("sinh viên"))
            {
                _loginPage.RoleStudentBtn.Click();
            }
            else if (roleLower.Contains("giáo viên"))
            {
                _loginPage.RoleTeacherBtn.Click();
            }
            else
            {
                _loginPage.RoleAdminBtn.Click();
            }

            _loginPage.EmailInput.SendKeys(email);
            _loginPage.PasswordInput.SendKeys(password);
            _loginPage.ClickDangNhap();

            OpenQA.Selenium.Support.UI.WebDriverWait wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            try
            {
                wait.Until(d => !d.Url.Contains("Login"));
            }
            catch {  }

            string currentUrl = _driver.Url;
            Assert.That(currentUrl.Contains("Login"), Is.False, "Lỗi: Đăng nhập thành công nhưng vẫn kẹt ở trang Login!");
        }

        [Test]
        public void S1_10_DangNhap_EmailHopLe_MatKhauSai()
        {
            _currentTestCaseId = "S.1.10";

            string[] testData = _excelManager.GetTestDataArray(_currentTestCaseId);

            string role = testData.Length > 0 ? testData[0] : "";
            string email = testData.Length > 1 ? testData[1] : "";
            string password = testData.Length > 2 ? testData[2] : "";

            TestContext.WriteLine($"[DATA] Hệ thống lấy được -> Role: {role} | Email: {email} | Pass: {password}");

            string roleLower = role.ToLower();
            if (roleLower.Contains("sinh viên")) { _loginPage.RoleStudentBtn.Click(); }
            else if (roleLower.Contains("giáo viên")) { _loginPage.RoleTeacherBtn.Click(); }
            else { _loginPage.RoleAdminBtn.Click(); }

            _loginPage.EmailInput.SendKeys(email);
            _loginPage.PasswordInput.SendKeys(password);
            _loginPage.ClickDangNhap();

            OpenQA.Selenium.Support.UI.WebDriverWait wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            try
            {
                wait.Until(d => _loginPage.MainErrorMessage.Displayed);
            }
            catch { }

            Assert.That(_loginPage.MainErrorMessage.Displayed, Is.True, "Lỗi: Không hiển thị thông báo sai mật khẩu.");
        }

        [Test]
        public void S1_11_DangNhap_EmailSaiDinhDang()
        {
            _currentTestCaseId = "S.1.11";

            string[] testData = _excelManager.GetTestDataArray(_currentTestCaseId);
            string role = testData.Length > 0 ? testData[0] : "Sinh viên";
            string email = testData.Length > 1 ? testData[1] : "abc@"; // Đảm bảo excel có chữ abc@
            string password = testData.Length > 2 ? testData[2] : "123456";

            TestContext.WriteLine($"[DATA] Lấy được -> Role: {role} | Email: {email} | Pass: {password}");

            string roleLower = role.ToLower();
            if (roleLower.Contains("sinh viên")) { _loginPage.RoleStudentBtn.Click(); }
            else if (roleLower.Contains("giáo viên")) { _loginPage.RoleTeacherBtn.Click(); }
            else { _loginPage.RoleAdminBtn.Click(); }

            _loginPage.EmailInput.SendKeys(email);
            _loginPage.PasswordInput.SendKeys(password);

            _loginPage.ClickDangNhap();

            System.Threading.Thread.Sleep(500);

            string emailValidationMsg = _loginPage.EmailInput.GetAttribute("validationMessage");
            string emailErrorText = _loginPage.EmailError.Text;

            bool hasError = !string.IsNullOrEmpty(emailValidationMsg) || !string.IsNullOrEmpty(emailErrorText);

            Assert.That(hasError, Is.True, "Lỗi: Không thấy cảnh báo khi nhập email sai định dạng (abc@)!");
        }

        [Test]
        public void S1_12_DangNhap_KhongChonVaiTro()
        {
            _currentTestCaseId = "S.1.12";

            string[] testData = _excelManager.GetTestDataArray(_currentTestCaseId);

            string email = "";
            string password = "";

            foreach (var item in testData)
            {
                if (item.Contains("@")) email = item;
                else if (item.Length >= 6) password = item;
            }

            if (string.IsNullOrEmpty(email)) email = "sv00001@gmail.com";
            if (string.IsNullOrEmpty(password)) password = "123456";

            TestContext.WriteLine($"[DATA] Lấy được -> Email: {email} | Pass: {password} (SẼ KHÔNG CHỌN ROLE)");

            _loginPage.EmailInput.SendKeys(email);
            _loginPage.PasswordInput.SendKeys(password);

            _loginPage.ClickDangNhap();

            System.Threading.Thread.Sleep(500);

            string roleErrorText = _loginPage.RoleError.Text;

            Assert.That(!string.IsNullOrEmpty(roleErrorText), Is.True, "Lỗi: Hệ thống không báo lỗi yêu cầu khi cố tình bỏ trống vai trò!");
        }


        [TearDown]
        public void Teardown()
        {
            string testCaseId = _currentTestCaseId;

            if (!string.IsNullOrEmpty(testCaseId))
            {
                if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
                {
                    string screenshotPath = "";
                    try
                    {
                        screenshotPath = ScreenshotHelper.TakeScreenshot(_driver, testCaseId);
                        TestContext.AddTestAttachment(screenshotPath, "Màn hình lỗi");
                    }
                    catch { }

                    _excelManager.WriteResult(testCaseId, "FAIL", screenshotPath);
                }
                else
                {
                    _excelManager.WriteResult(testCaseId, "PASS", "");
                }
            }

            if (_driver != null)
            {
                _driver.Quit();
                _driver.Dispose();
            }
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            if (_excelManager != null)
            {
                _excelManager.CloseReport();
            }
        }
    }
}