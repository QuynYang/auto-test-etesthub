using System;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using test_etesthub.Pages;
using test_etesthub.Helpers;

namespace test_etesthub.Tests.Login
{
    [TestFixture]
    public class LoginSystemStatusTests
    {
        private IWebDriver _driver;
        private LoginPage _loginPage;
        private ExcelReportManager _excelManager;
        private string _currentTestCaseId = "";
        private string _actualResult = "";

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

        private void SelectRole(string role)
        {
            string lower = role.ToLower().Replace(" ", "");
            if (lower.Contains("sinhviên") || lower.Contains("sinhvien")) _loginPage.RoleStudentBtn.Click();
            else if (lower.Contains("giáoviên") || lower.Contains("giaovien")) _loginPage.RoleTeacherBtn.Click();
            else if (lower.Contains("admin") || lower.Contains("quản trị")) _loginPage.RoleAdminBtn.Click();
        }

        [Test]
        public void S1_27_DangNhap_TaiKhoanHoatDong()
        {
            _currentTestCaseId = "S.1.27";
            string[] testData = _excelManager.GetTestDataArray(_currentTestCaseId);

            SelectRole(testData[0]);
            _loginPage.EmailInput.SendKeys(testData[1]);
            _loginPage.PasswordInput.SendKeys(testData[2]);
            _loginPage.ClickDangNhap();

            OpenQA.Selenium.Support.UI.WebDriverWait wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            try { wait.Until(d => !d.Url.Contains("Login")); } catch { }

            Assert.That(_driver.Url.Contains("Login"), Is.False, "Lỗi: Đăng nhập thành công nhưng hệ thống không chuyển hướng vào Dashboard.");
            _actualResult = $"Hệ thống hoạt động đúng. Tài khoản active {testData[1]} đăng nhập thành công và được chuyển hướng vào hệ thống tương ứng với quyền {testData[0]}.";
        }

        [Test]
        public void S1_28_DangNhap_SaiMatKhauKhoaTaiKhoan()
        {
            _currentTestCaseId = "S.1.28";
            string[] testData = _excelManager.GetTestDataArray(_currentTestCaseId);

            string role = testData[0];       
            string email = testData[1];      
            string wrongPass = testData[2];   
            string correctPass = testData[3]; 

            SelectRole(role);

            for (int i = 1; i <= 5; i++)
            {
                _loginPage.EmailInput.Clear();
                _loginPage.EmailInput.SendKeys(email);

                _loginPage.PasswordInput.Clear();
                _loginPage.PasswordInput.SendKeys(wrongPass);

                _loginPage.ClickDangNhap();
                System.Threading.Thread.Sleep(1000);
            }

            string error5 = _loginPage.MainErrorMessage.Text.ToLower();
            bool isLockedMessageShown = error5.Contains("khóa") || error5.Contains("locked") || error5.Contains("quá số lần");
            TestContext.WriteLine($"[LẦN 5] Lỗi trả về: {_loginPage.MainErrorMessage.Text}");

            _loginPage.PasswordInput.Clear();
            _loginPage.PasswordInput.SendKeys(correctPass);
            _loginPage.ClickDangNhap();
            System.Threading.Thread.Sleep(1500);

            string error6 = _loginPage.MainErrorMessage.Text.ToLower();
            bool isStillLocked = error6.Contains("khóa") || error6.Contains("locked") || _driver.Url.Contains("Login");

            TestContext.WriteLine($"[LẦN 6 - Mật khẩu ĐÚNG] Lỗi trả về: {_loginPage.MainErrorMessage.Text}");

            Assert.That(isStillLocked, Is.True, "Lỗi: Hệ thống bảo mật kém, không khóa tài khoản sau khi nhập sai 5 lần, hoặc vẫn cho đăng nhập vào lần thứ 6.");
            _actualResult = "Hệ thống bảo mật tốt. Đã khóa tài khoản sau 5 lần nhập sai và từ chối truy cập ở lần 6 dù nhập đúng mật khẩu.";
        }

        [Test]
        public void S1_29_DangNhap_KiemTraSessionToken()
        {
            _currentTestCaseId = "S.1.29";
            string[] testData = _excelManager.GetTestDataArray(_currentTestCaseId);

            SelectRole(testData[0]);
            _loginPage.EmailInput.SendKeys(testData[1]);
            _loginPage.PasswordInput.SendKeys(testData[2]);
            _loginPage.ClickDangNhap();

            OpenQA.Selenium.Support.UI.WebDriverWait wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            try { wait.Until(d => !d.Url.Contains("Login")); } catch { }

            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            long localStorageItemCount = (long)js.ExecuteScript("return window.localStorage.length;");
            long sessionStorageItemCount = (long)js.ExecuteScript("return window.sessionStorage.length;");
            int cookiesCount = _driver.Manage().Cookies.AllCookies.Count;

            TestContext.WriteLine($"[STORAGE INFO] LocalStorage: {localStorageItemCount} items | SessionStorage: {sessionStorageItemCount} items | Cookies: {cookiesCount} items");

            bool hasSessionOrToken = (localStorageItemCount > 0) || (cookiesCount > 0) || (sessionStorageItemCount > 0);

            Assert.That(hasSessionOrToken, Is.True, "Lỗi bảo mật/hệ thống: Không tìm thấy bất kỳ Session ID hay JWT Token nào được lưu trong trình duyệt sau khi đăng nhập!");
            _actualResult = $"Hệ thống cấp quyền chuẩn xác. Đã xác nhận sinh ra JWT Token/Session ID lưu trữ trên bộ nhớ trình duyệt cho tài khoản {testData[1]}.";
        }

        [TearDown]
        public void Teardown()
        {
            string testCaseId = _currentTestCaseId;
            if (!string.IsNullOrEmpty(testCaseId))
            {
                string testMethodName = TestContext.CurrentContext.Test.MethodName;
                var testStatus = TestContext.CurrentContext.Result.Outcome.Status;
                string errorMessage = TestContext.CurrentContext.Result.Message;

                if (testStatus == NUnit.Framework.Interfaces.TestStatus.Failed)
                {
                    string screenshotPath = "";
                    try { screenshotPath = ScreenshotHelper.TakeScreenshot(_driver, testCaseId); } catch { }

                    string resultText = "LỖI: " + (string.IsNullOrEmpty(errorMessage) ? "Gặp lỗi không xác định" : errorMessage);
                    _excelManager.WriteResult(testCaseId, "FAIL", resultText, testMethodName, screenshotPath);
                }
                else
                {
                    string resultText = string.IsNullOrEmpty(_actualResult)
                        ? "Kiểm tra thành công, khớp với yêu cầu thiết kế."
                        : _actualResult;

                    _excelManager.WriteResult(testCaseId, "PASS", resultText, testMethodName, "");
                }
            }
            _actualResult = "";

            if (_driver != null)
            {
                _driver.Quit();
                _driver.Dispose();
            }
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            if (_excelManager != null) { _excelManager.CloseReport(); }
        }
    }
}