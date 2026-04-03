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

        // URL hệ thống
        private readonly string _loginUrl = "https://e-testhub-frontend.onrender.com/Home/Login";
        private readonly string _userMgmtUrl = "https://e-testhub-frontend.onrender.com/Admin/UserManagement";
        private readonly string _createUserUrl = "https://e-testhub-frontend.onrender.com/Admin/CreateUser";

        // Biến lưu trữ email dùng chung để Test Case S.1.44 có thể lấy lại email do S.1.43 vừa tạo
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

        // Hàm hỗ trợ chọn Role từ text Excel
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

            // 1. ĐỌC DỮ LIỆU TỪ EXCEL
            string[] testData = _excelManager.GetTestDataArray(_currentTestCaseId);

            if (testData.Length < 10)
            {
                Assert.Fail($"Lỗi dữ liệu Excel: Test Case {_currentTestCaseId} yêu cầu ít nhất 10 dòng Test Data nhưng chỉ tìm thấy {testData.Length} dòng.");
            }

            // Map dữ liệu theo thứ tự xuất hiện ở Cột H trong file Excel
            string adminRole = testData[0];        // Quản Trị Viên
            string adminEmail = testData[1];       // admin@gmail.com
            string adminPass = testData[2];        // 123456

            // Xử lý email sinh viên: Thêm timestamp vào email từ Excel để chạy 100 lần không bị trùng
            string baseStudentEmail = testData[3]; // sv@gmail.com
            _sharedStudentEmail = baseStudentEmail.Replace("@", $"_{DateTime.Now:MMddHHmmss}@");

            string studentFirstName = testData[4]; // Nguyễn
            string studentLastName = testData[5];  // Văn A
            string studentRole = testData[6];      // Sinh Viên
            string studentPass = testData[7];      // 123456
            string confirmPass = testData[8];      // 123456

            string loginRole = testData[9];        // Sinh Viên
            string loginPass = testData.Length > 10 ? testData[10] : studentPass; // 123456

            TestContext.WriteLine($"[INFO] Data quét từ Excel: Admin({adminEmail}) tạo User({_sharedStudentEmail})");

            // ==============================================================
            // PHASE 1: ADMIN ĐĂNG NHẬP
            // ==============================================================
            _loginPage.GoToLoginPage(_loginUrl);
            SelectRoleFromExcelText(adminRole);
            _loginPage.NhapThongTin(adminEmail, adminPass);
            _loginPage.ClickDangNhap();
            _wait.Until(d => d.Url.Contains("Admin") || d.Url.Contains("Dashboard"));

            // ==============================================================
            // PHASE 2: ADMIN TẠO USER MỚI
            // ==============================================================
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

            // ==============================================================
            // PHASE 3: TÌM KIẾM USER VỪA TẠO TRONG DANH SÁCH
            // ==============================================================
            _driver.Navigate().GoToUrl(_userMgmtUrl);
            _wait.Until(d => _userPage.SearchInput.Displayed);

            _userPage.SearchInput.SendKeys(_sharedStudentEmail);
            Thread.Sleep(1500);

            var tableBody = _driver.FindElement(By.CssSelector("#usersTable tbody"));
            Assert.That(tableBody.Text.Contains(_sharedStudentEmail), Is.True, "Lỗi: Không tìm thấy sinh viên vừa tạo.");

            // ==============================================================
            // PHASE 4: ADMIN LOGOUT BẰNG CÁCH XÓA PHIÊN
            // ==============================================================
            _driver.Manage().Cookies.DeleteAllCookies();
            _loginPage.GoToLoginPage(_loginUrl);
            _wait.Until(d => _loginPage.LoginButton.Displayed);

            // ==============================================================
            // PHASE 5: SINH VIÊN VỪA TẠO ĐĂNG NHẬP
            // ==============================================================
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

            // 1. ĐỌC DỮ LIỆU TỪ EXCEL
            string[] testData = _excelManager.GetTestDataArray(_currentTestCaseId);

            string loginRole = testData.Length > 0 ? testData[0] : "Sinh Viên";
            string loginPass = testData.Length > 1 ? testData[1] : "123456";

            // Ưu tiên dùng tài khoản vừa được tạo thành công từ Test Case 43
            string studentEmail = !string.IsNullOrEmpty(_sharedStudentEmail) ? _sharedStudentEmail : "sv00001@gmail.com";

            // Bước 1 -> 4: Đăng nhập Sinh viên
            _loginPage.GoToLoginPage(_loginUrl);
            SelectRoleFromExcelText(loginRole);
            _loginPage.NhapThongTin(studentEmail, loginPass);
            _loginPage.ClickDangNhap();

            _wait.Until(d => d.Url.Contains("Student") || d.Url.Contains("Dashboard") || d.Url.Contains("Index"));

            // Bước 8: Giả lập Sinh viên cố tình paste URL của màn hình Admin vào thanh địa chỉ
            TestContext.WriteLine("[HỆ THỐNG] Đang giả lập hành vi truy cập trái phép vào trang Quản lý User của Admin...");
            _driver.Navigate().GoToUrl(_userMgmtUrl);
            Thread.Sleep(1500);

            // Xác nhận hệ thống chặn (Access Denied)
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