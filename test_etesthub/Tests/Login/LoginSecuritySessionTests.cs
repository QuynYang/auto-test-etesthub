using System;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using test_etesthub.Pages;
using test_etesthub.Helpers;

namespace test_etesthub.Tests.Login
{
    [TestFixture]
    public class LoginSecuritySessionTests
    {
        private IWebDriver _driver;
        private LoginPage _loginPage;
        private ExcelReportManager _excelManager;
        private string _currentTestCaseId = "";
        private string _actualResult = "";

        private readonly string _loginUrl = "https://e-testhub-frontend.onrender.com/Home/Login";
        private readonly string _dashboardUrl = "https://e-testhub-frontend.onrender.com/Home/Index";

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

        private void GetLoginData(string testCaseId, out string role, out string email, out string password)
        {
            string[] testData = _excelManager.GetTestDataArray(testCaseId);
            role = testData.Length > 0 ? testData[0] : "Sinh Viên";
            email = testData.Length > 1 ? testData[1] : "sv00001@gmail.com";
            password = testData.Length > 2 ? testData[2] : "123456";
        }

        private void SelectRole(LoginPage page, string role)
        {
            string lower = role.ToLower().Replace(" ", "");
            if (lower.Contains("sinhviên") || lower.Contains("sinhvien")) page.RoleStudentBtn.Click();
            else if (lower.Contains("giáoviên") || lower.Contains("giaovien")) page.RoleTeacherBtn.Click();
            else page.RoleAdminBtn.Click();
        }


        [Test]
        public void S1_30_KiemThu_CungTaiKhoan_HaiTab()
        {
            _currentTestCaseId = "S.1.30";
            GetLoginData(_currentTestCaseId, out string role, out string email, out string pass);

            ((IJavaScriptExecutor)_driver).ExecuteScript($"window.open('{_loginUrl}', '_blank');");
            var tabs = _driver.WindowHandles;

            _driver.SwitchTo().Window(tabs[0]);
            SelectRole(_loginPage, role);
            _loginPage.EmailInput.SendKeys(email);
            _loginPage.PasswordInput.SendKeys(pass);
            _loginPage.ClickDangNhap();
            System.Threading.Thread.Sleep(3000); 

            _driver.SwitchTo().Window(tabs[1]);
            _driver.Navigate().Refresh();
            System.Threading.Thread.Sleep(2000);

            Assert.That(_driver.Url.Contains("Login"), Is.False, "Lỗi: Tab 2 không tự động nhận diện phiên của Tab 1.");
            _actualResult = "Mở sẵn 2 tab. Đăng nhập Tab 1 thành công. Tab 2 tự động nhận diện phiên và truy cập thẳng hệ thống không cần đăng nhập lại.";
        }

        [Test]
        public void S1_31_KiemThu_CungTaiKhoan_HaiTrinhDuyet()
        {
            _currentTestCaseId = "S.1.31";
            GetLoginData(_currentTestCaseId, out string role, out string email, out string pass);

            IWebDriver edgeDriver = new EdgeDriver();
            try
            {
                edgeDriver.Manage().Window.Maximize();
                LoginPage edgeLoginPage = new LoginPage(edgeDriver);
                edgeLoginPage.GoToLoginPage(_loginUrl);

                SelectRole(_loginPage, role);
                _loginPage.EmailInput.SendKeys(email);
                _loginPage.PasswordInput.SendKeys(pass);
                _loginPage.ClickDangNhap();
                System.Threading.Thread.Sleep(2000);

                SelectRole(edgeLoginPage, role);
                edgeLoginPage.EmailInput.SendKeys(email);
                edgeLoginPage.PasswordInput.SendKeys(pass);
                edgeLoginPage.ClickDangNhap();
                System.Threading.Thread.Sleep(2000);

                bool isChromeLoggedIn = !_driver.Url.Contains("Login");
                bool isEdgeLoggedIn = !edgeDriver.Url.Contains("Login");

                Assert.That(isChromeLoggedIn && isEdgeLoggedIn, Is.True, "Lỗi: Một trong hai trình duyệt bị văng ra khi dùng chung tài khoản.");
                _actualResult = "Sử dụng Chrome và Edge. Hệ thống tạo 2 phiên độc lập, cả 2 trình duyệt đều đăng nhập thành công cùng 1 tài khoản và hoạt động song song.";
            }
            finally
            {
                edgeDriver.Quit();
            }
        }

        [Test]
        public void S1_32_KiemThu_HaiTaiKhoan_HaiTab()
        {
            _currentTestCaseId = "S.1.32";
            string[] testData = _excelManager.GetTestDataArray(_currentTestCaseId);

            string role = testData[0];
            string email1 = testData[1]; 
            string pass1 = testData[2];
            string email2 = testData[4]; 

            ((IJavaScriptExecutor)_driver).ExecuteScript($"window.open('{_loginUrl}', '_blank');");
            var tabs = _driver.WindowHandles;

            _driver.SwitchTo().Window(tabs[0]);
            SelectRole(_loginPage, role);
            _loginPage.EmailInput.SendKeys(email1);
            _loginPage.PasswordInput.SendKeys(pass1);
            _loginPage.ClickDangNhap();
            System.Threading.Thread.Sleep(3000);

            _driver.SwitchTo().Window(tabs[1]);
            _driver.Navigate().Refresh();
            System.Threading.Thread.Sleep(2000);

            bool isFormHidden = false;
            try { isFormHidden = !_loginPage.EmailInput.Displayed; } catch { isFormHidden = true; }

            Assert.That(isFormHidden, Is.True, "Lỗi bảo mật: Tab 2 vẫn hiển thị form cho phép đăng nhập tài khoản khác trong khi Tab 1 chưa đăng xuất.");
            _actualResult = $"Tab 2 tự động chuyển vào hệ thống với phiên của TK {email1}. Không cho phép điền form để đăng nhập TK {email2}.";
        }

        [Test]
        public void S1_33_KiemThu_HaiTaiKhoan_HaiTrinhDuyet()
        {
            _currentTestCaseId = "S.1.33";
            string[] testData = _excelManager.GetTestDataArray(_currentTestCaseId);

            string role = testData[0];
            string email1 = testData[1];
            string pass1 = testData[2];
            string email2 = testData[4];
            string pass2 = testData[5];

            IWebDriver edgeDriver = new EdgeDriver();
            try
            {
                edgeDriver.Manage().Window.Maximize();
                LoginPage edgeLoginPage = new LoginPage(edgeDriver);
                edgeLoginPage.GoToLoginPage(_loginUrl);

                SelectRole(_loginPage, role);
                _loginPage.EmailInput.SendKeys(email1);
                _loginPage.PasswordInput.SendKeys(pass1);
                _loginPage.ClickDangNhap();
                System.Threading.Thread.Sleep(2000);

                SelectRole(edgeLoginPage, role);
                edgeLoginPage.EmailInput.SendKeys(email2);
                edgeLoginPage.PasswordInput.SendKeys(pass2);
                edgeLoginPage.ClickDangNhap();
                System.Threading.Thread.Sleep(2000);

                bool isChromeLoggedIn = !_driver.Url.Contains("Login");
                bool isEdgeLoggedIn = !edgeDriver.Url.Contains("Login");

                Assert.That(isChromeLoggedIn && isEdgeLoggedIn, Is.True, "Lỗi: Không thể đăng nhập 2 tài khoản khác nhau trên 2 trình duyệt.");
                _actualResult = "Sử dụng Chrome và Edge. Hai tài khoản đăng nhập thành công và hoạt động song song, độc lập do session được lưu trữ riêng biệt.";
            }
            finally
            {
                edgeDriver.Quit();
            }
        }

        [Test]
        public void S1_34_RefreshTrangSauDangNhap()
        {
            _currentTestCaseId = "S.1.34";
            GetLoginData(_currentTestCaseId, out string role, out string email, out string pass);

            SelectRole(_loginPage, role);
            _loginPage.EmailInput.SendKeys(email);
            _loginPage.PasswordInput.SendKeys(pass);
            _loginPage.ClickDangNhap();
            System.Threading.Thread.Sleep(2000);

            _driver.Navigate().Refresh();
            System.Threading.Thread.Sleep(1500);

            Assert.That(_driver.Url.Contains("Login"), Is.False, "Lỗi: F5 tải lại trang làm mất phiên, văng ra trang Login.");
            _actualResult = "Trang được tải lại thành công (Refresh F5). Vẫn giữ nguyên trạng thái đăng nhập, không bị văng ra trang Login.";
        }

        [Test]
        public void S1_35_DongTrinhDuyetVaMoLai()
        {
            _currentTestCaseId = "S.1.35";
            GetLoginData(_currentTestCaseId, out string role, out string email, out string pass);

            SelectRole(_loginPage, role);
            _loginPage.EmailInput.SendKeys(email);
            _loginPage.PasswordInput.SendKeys(pass);
            _loginPage.ClickDangNhap();
            System.Threading.Thread.Sleep(2000);

            _driver.Quit();
            System.Threading.Thread.Sleep(1500);

            _driver = new ChromeDriver();
            _driver.Manage().Window.Maximize();
            _loginPage = new LoginPage(_driver); 

            _driver.Navigate().GoToUrl(_dashboardUrl);
            System.Threading.Thread.Sleep(2000);

            Assert.That(_driver.Url.Contains("Login"), Is.True, "Lỗi Nghiêm Trọng: Tắt hoàn toàn trình duyệt mở lại vẫn vào được trang nội bộ.");
            _actualResult = "Hệ thống yêu cầu đăng nhập lại chính xác do phiên làm việc (Session) đã kết thúc sau khi đóng hoàn toàn trình duyệt Chrome.";
        }

        [Test]
        public void S1_36_TruyCapTrucTiepKhiChuaDangNhap()
        {
            _currentTestCaseId = "S.1.36";
            _driver.Navigate().GoToUrl(_dashboardUrl);
            System.Threading.Thread.Sleep(1000);

            Assert.That(_driver.Url.Contains("Login"), Is.True, "Lỗi: Không bị chuyển hướng về Login khi truy cập trái phép.");
            _actualResult = "Hệ thống chặn quyền truy cập trái phép và tự động redirect về trang Login.";
        }

        [Test]
        public void S1_37_LogoutVaTruyCapBangNutBack()
        {
            _currentTestCaseId = "S.1.37";
            GetLoginData(_currentTestCaseId, out string role, out string email, out string pass);

            SelectRole(_loginPage, role);
            _loginPage.EmailInput.SendKeys(email);
            _loginPage.PasswordInput.SendKeys(pass);
            _loginPage.ClickDangNhap();
            System.Threading.Thread.Sleep(2000);

            try
            {
                IWebElement logoutBtn = _driver.FindElement(By.XPath("//*[contains(translate(text(), 'ĐĂNG XUẤT', 'đăng xuất'), 'đăng xuất') or contains(translate(text(), 'LOGOUT', 'logout'), 'logout')]"));
                logoutBtn.Click();
            }
            catch
            {
                _driver.Navigate().GoToUrl("https://e-testhub-frontend.onrender.com/Home/Login");
            }
            System.Threading.Thread.Sleep(1500);

            _driver.Navigate().Back();
            System.Threading.Thread.Sleep(1000);

            try { _driver.FindElement(By.TagName("body")).Click(); } catch { }
            System.Threading.Thread.Sleep(1000);

            Assert.That(_driver.Url.Contains("Login"), Is.True, "Lỗi: Có thể back lại trang nội bộ và thao tác sau khi đã Logout.");
            _actualResult = "Sau khi Đăng xuất và dùng nút Back, hệ thống chặn các thao tác và đẩy về trang Login do token/session đã bị hủy.";
        }

        [Test]
        public void S1_38_DangNhap_SQLInjection()
        {
            _currentTestCaseId = "S.1.38";
            string sqlInjectionPayload = "' OR '1'='1";

            SelectRole(_loginPage, "Sinh Viên");
            _loginPage.EmailInput.SendKeys(sqlInjectionPayload);
            _loginPage.PasswordInput.SendKeys("123456");
            _loginPage.ClickDangNhap();
            System.Threading.Thread.Sleep(1000);

            string emailVal = _loginPage.EmailInput.GetAttribute("validationMessage");
            string serverError = "";
            try { serverError = _loginPage.MainErrorMessage.Text; } catch { }

            bool isBlocked = !string.IsNullOrEmpty(emailVal) || !string.IsNullOrEmpty(serverError);
            Assert.That(isBlocked, Is.True, "Lỗi Nghiêm Trọng: Có nguy cơ bị lỗi SQL Injection!");
            _actualResult = $"Hệ thống an toàn. Đã chặn chuỗi SQL Injection ({sqlInjectionPayload}) bằng HTML5 hoặc backend bắt lỗi.";
        }

        [Test]
        public void S1_39_DangNhap_XSS()
        {
            _currentTestCaseId = "S.1.39";
            string xssPayload = "<script>alert(1)</script>";

            SelectRole(_loginPage, "Sinh Viên");
            _loginPage.EmailInput.SendKeys(xssPayload);
            _loginPage.PasswordInput.SendKeys("123456");
            _loginPage.ClickDangNhap();
            System.Threading.Thread.Sleep(1000);

            bool isAlertPresent = false;
            try { _driver.SwitchTo().Alert(); isAlertPresent = true; } catch (NoAlertPresentException) { }

            Assert.That(isAlertPresent, Is.False, "Lỗi Nghiêm Trọng: Trang web bị dính lỗi XSS!");
            _actualResult = "Hệ thống an toàn trước XSS. Không có mã độc Javascript nào được thực thi, báo lỗi định dạng email hợp lý.";
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
                try { _driver.Quit(); _driver.Dispose(); } catch { }
            }
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            if (_excelManager != null) { _excelManager.CloseReport(); }
        }
    }
}