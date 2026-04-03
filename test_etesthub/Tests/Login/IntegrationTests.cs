using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using test_etesthub.Pages;
using test_etesthub.Helpers;

namespace test_etesthub.Tests.Login
{
    [TestFixture]
    public class IntegrationTests
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;
        private LoginPage _loginPage;
        private UserManagementPage _userPage;
        private ExcelReportManager _excelManager;

        private string _currentTestCaseId = "";
        private string _actualResult = "";

        private readonly string _loginUrl = "https://e-testhub-frontend.onrender.com/Home/Login";
        private readonly string _userMgmtUrl = "https://e-testhub-frontend.onrender.com/Admin/UserManagement";
        private readonly string _createUserUrl = "https://e-testhub-frontend.onrender.com/Admin/CreateUser";

        private static string _sharedStudentEmail = "";

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            string absolutePath = @"C:\Users\vungo\OneDrive\Tài liệu\2025\BDCLPM\LT\baocao.xlsx";
            _excelManager = new ExcelReportManager(absolutePath, "II.1_DangNhap");
            _excelManager.OpenReport();
        }

        [SetUp]
        public void Setup()
        {
            _driver = new ChromeDriver();
            _driver.Manage().Window.Maximize();
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));

            _loginPage = new LoginPage(_driver);
            _userPage = new UserManagementPage(_driver);
        }

        private void SelectRoleFromExcelText(string roleText)
        {
            string lower = roleText.ToLower().Replace(" ", "");
            if (lower.Contains("sinhviên")) _loginPage.RoleStudentBtn.Click();
            else if (lower.Contains("giáoviên")) _loginPage.RoleTeacherBtn.Click();
            else if (lower.Contains("quảntrịviên") || lower.Contains("admin")) _loginPage.RoleAdminBtn.Click();
        }

        [Test, Order(1)]
        public void S1_43_IntegrationTest_AdminCreateUserAndStudentLogin()
        {
            _currentTestCaseId = "S.1.43";

            string[] testData = _excelManager.GetTestDataArray(_currentTestCaseId);

            if (testData.Length < 10)
            {
                Assert.Fail($"Lỗi dữ liệu Excel: Test Case {_currentTestCaseId} yêu cầu ít nhất 10 dòng Test Data nhưng chỉ tìm thấy {testData.Length} dòng.");
            }

            string adminRole = testData[0];       
            string adminEmail = testData[1];       
            string adminPass = testData[2];       


            string baseStudentEmail = testData[3]; 
            _sharedStudentEmail = baseStudentEmail.Replace("@", $"_{DateTime.Now:MMddHHmmss}@");

            string studentFirstName = testData[4]; 
            string studentLastName = testData[5];  
            string studentRole = testData[6];     
            string studentPass = testData[7];     
            string confirmPass = testData[8];     

            string loginRole = testData[9];       
            string loginPass = testData.Length > 10 ? testData[10] : studentPass;

            TestContext.WriteLine($"[INFO] Data quét từ Excel: Admin({adminEmail}) tạo User({_sharedStudentEmail})");

            _loginPage.GoToLoginPage(_loginUrl);
            SelectRoleFromExcelText(adminRole);
            _loginPage.NhapThongTin(adminEmail, adminPass);
            _loginPage.ClickDangNhap();
            _wait.Until(d => d.Url.Contains("Admin") || d.Url.Contains("Dashboard"));


            _driver.Navigate().GoToUrl(_createUserUrl);
            _wait.Until(d => _userPage.EmailInput.Displayed);

            _userPage.EmailInput.SendKeys(_sharedStudentEmail);
            _userPage.FirstNameInput.SendKeys(studentFirstName);
            _userPage.LastNameInput.SendKeys(studentLastName);

            var roleSelect = new SelectElement(_userPage.RoleSelect);
            if (studentRole.ToLower().Contains("sinh viên")) roleSelect.SelectByValue("student");
            else if (studentRole.ToLower().Contains("giáo viên")) roleSelect.SelectByValue("teacher");
            else roleSelect.SelectByValue("admin");

            _userPage.PasswordInput.SendKeys(studentPass);
            _userPage.ConfirmPasswordInput.SendKeys(confirmPass);
            _userPage.ClickCreateUserBtn();
            Thread.Sleep(2000);


            _driver.Navigate().GoToUrl(_userMgmtUrl);
            _wait.Until(d => _userPage.SearchInput.Displayed);

            _userPage.SearchInput.SendKeys(_sharedStudentEmail);
            Thread.Sleep(1500);

            var tableBody = _driver.FindElement(By.CssSelector("#usersTable tbody"));
            Assert.That(tableBody.Text.Contains(_sharedStudentEmail), Is.True, "Lỗi: Không tìm thấy sinh viên vừa tạo.");


            _driver.Manage().Cookies.DeleteAllCookies();
            _loginPage.GoToLoginPage(_loginUrl);
            _wait.Until(d => _loginPage.LoginButton.Displayed);


            SelectRoleFromExcelText(loginRole);
            _loginPage.NhapThongTin(_sharedStudentEmail, loginPass);
            _loginPage.ClickDangNhap();

            _wait.Until(d => d.Url.Contains("Student") || d.Url.Contains("Dashboard") || d.Url.Contains("Index"));

            bool isDashboardLoaded = _driver.FindElements(By.CssSelector(".greeting")).Count > 0 || !_driver.Url.Contains("Login");
            Assert.That(isDashboardLoaded, Is.True, "Lỗi: Tài khoản sinh viên tạo từ Excel không thể đăng nhập.");

            _actualResult = $"Toàn trình Integration thành công. Admin đã tạo tài khoản Sinh viên mới ({_sharedStudentEmail}) và Sinh viên đăng nhập thành công.";
        }

        [Test, Order(2)]
        public void S1_44_IntegrationTest_KiemTraPhanQuyenSinhVien()
        {
            _currentTestCaseId = "S.1.44";

            string[] testData = _excelManager.GetTestDataArray(_currentTestCaseId);

            string loginRole = testData.Length > 0 ? testData[0] : "Sinh Viên";
            string loginPass = testData.Length > 1 ? testData[1] : "123456";

            string studentEmail = !string.IsNullOrEmpty(_sharedStudentEmail) ? _sharedStudentEmail : "sv00001@gmail.com";

            _loginPage.GoToLoginPage(_loginUrl);
            SelectRoleFromExcelText(loginRole);
            _loginPage.NhapThongTin(studentEmail, loginPass);
            _loginPage.ClickDangNhap();

            _wait.Until(d => d.Url.Contains("Student") || d.Url.Contains("Dashboard") || d.Url.Contains("Index"));

            TestContext.WriteLine("[HỆ THỐNG] Đang giả lập hành vi truy cập trái phép vào trang Quản lý User của Admin...");
            _driver.Navigate().GoToUrl(_userMgmtUrl);
            Thread.Sleep(1500);

            string currentUrl = _driver.Url;
            string pageText = _driver.FindElement(By.TagName("body")).Text.ToLower();

            bool isBlocked = currentUrl.Contains("AccessDenied") ||
                             currentUrl.Contains("Login") ||
                             pageText.Contains("không có quyền") ||
                             pageText.Contains("access denied");

            Assert.That(isBlocked, Is.True, "LỖI BẢO MẬT: Sinh viên có thể truy cập được vào URL Quản lý User của Admin!");

            _actualResult = "Integration Test Bảo mật phân quyền thành công. Hệ thống đã chặn đứng Sinh viên khi cố truy cập URL của Admin.";
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
                        ? "Integration Test UI thành công."
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