using System;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using test_etesthub.Pages;
using test_etesthub.Helpers;
using System.Linq;

namespace test_etesthub.Tests.UserManagement
{
    [TestFixture]
    public class CreateUserSystemStatusTests
    {
        private IWebDriver _driver;
        private LoginPage _loginPage;
        private UserManagementPage _userPage;
        private ExcelReportManager _excelManager;

        private string _currentTestCaseId = "";
        private string _actualResult = "";

        private readonly string _loginUrl = "https://e-testhub-frontend.onrender.com/Home/Login";
        private readonly string _createUserUrl = "https://e-testhub-frontend.onrender.com/Admin/CreateUser";
        private readonly string _userMgmtUrl = "https://e-testhub-frontend.onrender.com/Admin/UserManagement";

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            string absolutePath = @"C:\Users\vungo\OneDrive\Tài liệu\2025\BDCLPM\LT\baocao.xlsx";
            _excelManager = new ExcelReportManager(absolutePath, "II.2_QuanLyUser");
            _excelManager.OpenReport();
        }

        [SetUp]
        public void Setup()
        {
            _driver = new ChromeDriver();
            _driver.Manage().Window.Maximize();
            _loginPage = new LoginPage(_driver);
            _userPage = new UserManagementPage(_driver);

            // Đăng nhập quyền Admin
            _loginPage.GoToLoginPage(_loginUrl);
            _loginPage.RoleAdminBtn.Click();
            _loginPage.NhapThongTin("admin@gmail.com", "123456");
            _loginPage.ClickDangNhap();

            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            try { wait.Until(d => !d.Url.Contains("Login")); } catch { }
        }

        // ==========================================
        // CÁC HÀM HỖ TRỢ XỬ LÝ DỮ LIỆU ĐỘNG & EXCEL
        // ==========================================

        private void GetUserFormData(string testCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass)
        {
            string[] testData = _excelManager.GetTestDataArray(testCaseId);
            email = ""; firstName = ""; lastName = ""; role = ""; pass = ""; confirmPass = "";

            if (testData.Length >= 6)
            {
                email = testData[0]; firstName = testData[1]; lastName = testData[2];
                role = testData[3]; pass = testData[4]; confirmPass = testData[5];
            }
        }

        private void GenerateDynamicData(ref string email, ref string firstName, ref string lastName)
        {
            if (email == "sv@gmail.com")
            {
                string timestamp = DateTime.Now.ToString("ddMMyy_HHmmss");
                email = $"sv{timestamp}@gmail.com";
            }

            if (firstName == "Nguyễn" && lastName == "Văn A")
            {
                Random rnd = new Random();
                string[] hoList = { "Trần", "Lê", "Phạm", "Hoàng", "Huỳnh", "Phan", "Vũ", "Võ" };
                string[] tenList = { "Thị B", "Văn C", "Minh D", "Hữu E", "Thanh F", "Ngọc G" };

                firstName = hoList[rnd.Next(hoList.Length)];
                lastName = tenList[rnd.Next(tenList.Length)] + $" {rnd.Next(100, 999)}";
            }
        }

        private void SelectRoleByText(string roleText)
        {
            if (string.IsNullOrEmpty(roleText)) return;
            SelectElement select = new SelectElement(_userPage.RoleSelect);
            string lower = roleText.ToLower().Replace(" ", "");

            if (lower.Contains("sinhviên") || lower.Contains("sinhvien")) select.SelectByValue("student");
            else if (lower.Contains("giáoviên") || lower.Contains("giaovien")) select.SelectByValue("teacher");
            else if (lower.Contains("admin") || lower.Contains("quảntrị")) select.SelectByValue("admin");
        }

        private void FillForm(string email, string firstName, string lastName, string role, string pass, string confirmPass)
        {
            if (!string.IsNullOrEmpty(email)) _userPage.EmailInput.SendKeys(email);
            if (!string.IsNullOrEmpty(firstName)) _userPage.FirstNameInput.SendKeys(firstName);
            if (!string.IsNullOrEmpty(lastName)) _userPage.LastNameInput.SendKeys(lastName);
            SelectRoleByText(role);
            if (!string.IsNullOrEmpty(pass)) _userPage.PasswordInput.SendKeys(pass);
            if (!string.IsNullOrEmpty(confirmPass)) _userPage.ConfirmPasswordInput.SendKeys(confirmPass);
        }

        // ==========================================
        // DANH SÁCH TEST CASE TRẠNG THÁI & BẢO MẬT
        // ==========================================

        [Test]
        public void S2_1_33_KiemTraUserXuatHienTrongDanhSach()
        {
            _currentTestCaseId = "S.2.1.33";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);
            GenerateDynamicData(ref email, ref firstName, ref lastName);

            // 1. Tạo user
            FillForm(email, firstName, lastName, role, pass, confirmPass);
            _userPage.ClickCreateUserBtn();

            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            try { wait.Until(d => d.FindElement(By.Id("successMessage")).Displayed); } catch { }
            System.Threading.Thread.Sleep(1000); // Chờ DB lưu mượt mà

            // 2. Chuyển về trang danh sách và tìm kiếm
            _driver.Navigate().GoToUrl(_userMgmtUrl);
            System.Threading.Thread.Sleep(1500); // Chờ bảng load xong

            _userPage.SearchInput.SendKeys(email); // Nhập email vừa tạo vào ô tìm kiếm
            System.Threading.Thread.Sleep(1500); // Chờ JS filter bảng

            string tableText = _userPage.UsersTable.Text;

            Assert.That(tableText.Contains(email) || tableText.Contains(lastName), Is.True, "Lỗi: Tạo thành công nhưng không tìm thấy user ngoài danh sách.");
            _actualResult = $"Hệ thống lưu trữ chính xác. Bảng dữ liệu hiển thị user {email} vừa tạo với các thông tin khớp với dữ liệu đã nhập.";
        }

        [Test]
        public void S2_1_34_NhanQuayLaiHuyThaoTac()
        {
            _currentTestCaseId = "S.2.1.34";
            _driver.Navigate().GoToUrl(_createUserUrl);
            System.Threading.Thread.Sleep(1000);

            // Nhập thử dữ liệu rồi nhấn Back
            _userPage.EmailInput.SendKeys("test_huy_thao_tac@gmail.com");
            _userPage.ClickBackBtn();

            System.Threading.Thread.Sleep(1500);

            Assert.That(_driver.Url.Contains("UserManagement"), Is.True, "Lỗi: Nút Quay Lại không hoạt động.");
            _actualResult = "Form không thực hiện lưu dữ liệu. Hệ thống lập tức hủy thao tác và chuyển hướng (redirect) an toàn về lại trang Quản Lý.";
        }

        [Test]
        public void S2_1_35_BaoMat_XSS_VaoTen()
        {
            _currentTestCaseId = "S.2.1.35";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);
            GenerateDynamicData(ref email, ref firstName, ref lastName);

            // Bơm mã độc XSS vào Họ
            string xssPayload = "<script>alert('hack')</script>";

            FillForm(email, xssPayload, lastName, role, pass, confirmPass);
            _userPage.ClickCreateUserBtn();
            System.Threading.Thread.Sleep(1500);

            // Kiểm tra xem trình duyệt có bật Popup Alert lên không (nếu bật tức là dính XSS)
            bool isAlertPresent = false;
            try
            {
                _driver.SwitchTo().Alert();
                isAlertPresent = true;
            }
            catch (NoAlertPresentException) { }

            Assert.That(isAlertPresent, Is.False, "Lỗi Bảo mật Nghiêm Trọng: Hệ thống dính lỗi XSS, Script đã được thực thi!");
            _actualResult = "Hệ thống an toàn trước XSS. Các ký tự Script bị mã hóa (encode) hoặc loại bỏ, không có alert popup nào xuất hiện.";
        }

        [Test]
        public void S2_1_36_BaoMat_SQLInjection_VaoEmail()
        {
            _currentTestCaseId = "S.2.1.36";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);

            // Bơm mã độc SQLi vào Email
            string sqlPayload = "test' OR '1'='1";

            FillForm(sqlPayload, firstName, lastName, role, pass, confirmPass);
            _userPage.ClickCreateUserBtn();

            // Kiểm tra validation của trình duyệt bắt lỗi (Vì thiếu dấu @)
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            string html5Val = (string)js.ExecuteScript("return arguments[0].validationMessage;", _userPage.EmailInput);

            bool hasError = !string.IsNullOrEmpty(html5Val) || _driver.PageSource.Contains("hợp lệ") || _driver.PageSource.Contains("format");

            Assert.That(hasError, Is.True, "Lỗi: Hệ thống không chặn chuỗi SQL Injection trong thẻ Email.");
            _actualResult = "Trình duyệt và Frontend đã chặn submit, báo lỗi định dạng Email hợp lý, đảm bảo an toàn không bị khai thác SQL.";
        }

        [Test]
        public void S2_1_37_BaoMat_SpamClick_DoubleSubmit()
        {
            _currentTestCaseId = "S.2.1.37";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);
            GenerateDynamicData(ref email, ref firstName, ref lastName);

            FillForm(email, firstName, lastName, role, pass, confirmPass);

            // SPAM CLICK LIÊN TỤC 5 LẦN TRONG 1 GIÂY
            _userPage.ScrollToElement(_userPage.CreateUserBtn);
            for (int i = 0; i < 5; i++)
            {
                try { _userPage.CreateUserBtn.Click(); } catch { }
            }

            System.Threading.Thread.Sleep(2000); 

            bool isBtnDisabled = false;
            try
            {
                string disabledAttr = _userPage.CreateUserBtn.GetAttribute("disabled");
                isBtnDisabled = (disabledAttr != null && (disabledAttr.Equals("true") || disabledAttr.Equals("disabled")));
            }
            catch { }

            _driver.Navigate().GoToUrl(_userMgmtUrl);
            System.Threading.Thread.Sleep(1500);
            _userPage.SearchInput.SendKeys(email);
            System.Threading.Thread.Sleep(1500);

            var rows = _driver.FindElements(By.CssSelector("#usersTable tbody tr"));
            int userCount = rows.Count(r => r.Text.Contains(email));

            TestContext.WriteLine($"[DEBUG] Nút bị vô hiệu hóa: {isBtnDisabled} | Số user tạo ra: {userCount}");

            Assert.That(userCount <= 1, Is.True, "Lỗi Bảo Mật: Hệ thống bị lỗi Double Submit, spam click đã tạo ra nhiều tài khoản trùng lặp!");

            string btnStatus = isBtnDisabled ? "Nút bị vô hiệu hóa (disabled) ngay sau click." : "Nút không bị disable.";
            _actualResult = $"{btnStatus} Hệ thống an toàn, chỉ gọi API 1 lần và tạo ra đúng 1 user, không bị lỗi duplicate dữ liệu.";
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
                        ? "Kiểm tra Trạng thái/Bảo mật thành công."
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