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
    public class LoginLogicTests
    {
        private IWebDriver _driver;
        private LoginPage _loginPage;
        private ExcelReportManager _excelManager;
        private string _currentTestCaseId = "";
        private string _actualResult = "";

        private readonly string _loginUrl = "https://e-testhub-frontend.onrender.com/Home/Login";

        private void GetLoginData(string testCaseId, out string role, out string email, out string password)
        {
            string[] testData = _excelManager.GetTestDataArray(testCaseId);

            role = testData.Length > 0 ? testData[0] : "";
            email = testData.Length > 1 ? testData[1] : "";
            password = testData.Length > 2 ? testData[2] : "";

            if (!string.IsNullOrEmpty(email) && !email.Contains("@") && email.Length >= 5 && string.IsNullOrEmpty(password))
            {
                password = email;
                email = "";
            }
        }

        private void SelectRole(string role)
        {
            string lower = role.ToLower().Replace(" ", "");
            if (lower.Contains("sinhviên") || lower.Contains("sinhvien")) _loginPage.RoleStudentBtn.Click();
            else if (lower.Contains("giáoviên") || lower.Contains("giaovien")) _loginPage.RoleTeacherBtn.Click();
            else if (lower.Contains("admin") || lower.Contains("quản trị")) _loginPage.RoleAdminBtn.Click();
        }

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
            GetLoginData(_currentTestCaseId, out string role, out string email, out string password);

            SelectRole(role);
            if (!string.IsNullOrEmpty(email)) _loginPage.EmailInput.SendKeys(email);
            if (!string.IsNullOrEmpty(password)) _loginPage.PasswordInput.SendKeys(password);
            _loginPage.ClickDangNhap();

            OpenQA.Selenium.Support.UI.WebDriverWait wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            try { wait.Until(d => !d.Url.Contains("Login")); } catch { }

            Assert.That(_driver.Url.Contains("Login"), Is.False, "Lỗi: Đăng nhập thành công nhưng vẫn kẹt ở trang Login!");
            _actualResult = $"Đăng nhập thành công với tài khoản {email} ({role}). Hệ thống đã chuyển hướng đúng khỏi trang Login.";
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
            _actualResult = $"Hệ thống đã chặn đăng nhập tài khoản {email} và hiển thị đúng thông báo lỗi sai mật khẩu từ Server.";
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
            _actualResult = $"Hệ thống đã chặn lập tức và hiển thị đúng cảnh báo lỗi định dạng email khi người dùng nhập '{email}'.";
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
            _actualResult = $"Hệ thống đã chặn đăng nhập và báo lỗi bắt buộc chọn vai trò khi cố tình nhập đủ {email} và pass nhưng bỏ trống Role.";
        }

        // NHÓM TEST BỎ TRỐNG DỮ LIỆU (HTML5 VALIDATION)

        [Test]
        public void S1_13_DangNhap_DeTrongCaEmailVaMatKhau()
        {
            _currentTestCaseId = "S.1.13";
            GetLoginData(_currentTestCaseId, out string role, out string email, out string password);

            SelectRole(role);
            // Cố tình bỏ trống
            _loginPage.ClickDangNhap();

            System.Threading.Thread.Sleep(500);

            string emailVal = _loginPage.EmailInput.GetAttribute("validationMessage");
            Assert.That(!string.IsNullOrEmpty(emailVal), Is.True, "Lỗi: Trình duyệt không chặn/báo lỗi khi bỏ trống Email.");
            _actualResult = "Hệ thống chặn submit form và tự động hiển thị thông báo yêu cầu nhập liệu tại ô Email.";
        }

        [Test]
        public void S1_14_DangNhap_DeTrongEmail()
        {
            _currentTestCaseId = "S.1.14";
            GetLoginData(_currentTestCaseId, out string role, out string email, out string password);

            SelectRole(role);
            if (!string.IsNullOrEmpty(password)) _loginPage.PasswordInput.SendKeys(password);
            _loginPage.ClickDangNhap();

            System.Threading.Thread.Sleep(500);

            string emailVal = _loginPage.EmailInput.GetAttribute("validationMessage");
            Assert.That(!string.IsNullOrEmpty(emailVal), Is.True, "Lỗi: Trình duyệt không chặn/báo lỗi khi bỏ trống Email.");
            _actualResult = "Hệ thống chặn submit form và tự động báo lỗi yêu cầu nhập liệu tại ô Email.";
        }

        [Test]
        public void S1_15_DangNhap_DeTrongMatKhau()
        {
            _currentTestCaseId = "S.1.15";
            GetLoginData(_currentTestCaseId, out string role, out string email, out string password);

            SelectRole(role);
            if (!string.IsNullOrEmpty(email)) _loginPage.EmailInput.SendKeys(email);
            _loginPage.ClickDangNhap();

            System.Threading.Thread.Sleep(500);

            string passVal = _loginPage.PasswordInput.GetAttribute("validationMessage");
            Assert.That(!string.IsNullOrEmpty(passVal), Is.True, "Lỗi: Trình duyệt không chặn/báo lỗi khi bỏ trống Mật khẩu.");
            _actualResult = $"Hệ thống chặn submit form và báo lỗi bắt buộc nhập liệu tại ô Password sau khi nhập email {email}.";
        }

        [Test]
        public void S1_16_DangNhap_TaiKhoanSinhVien_ChonRoleGiaoVien()
        {
            _currentTestCaseId = "S.1.16";
            GetLoginData(_currentTestCaseId, out string role, out string email, out string password);

            SelectRole(role);
            if (!string.IsNullOrEmpty(email)) _loginPage.EmailInput.SendKeys(email);
            if (!string.IsNullOrEmpty(password)) _loginPage.PasswordInput.SendKeys(password);
            _loginPage.ClickDangNhap();

            OpenQA.Selenium.Support.UI.WebDriverWait wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            try { wait.Until(d => _loginPage.MainErrorMessage.Displayed); } catch { }

            Assert.That(_loginPage.MainErrorMessage.Displayed, Is.True, "Lỗi: Không báo lỗi từ chối truy cập khi SV chọn nhầm role GV.");
            _actualResult = $"Đăng nhập thất bại. Hệ thống từ chối tài khoản {email} do không khớp với vai trò {role}.";
        }

        [Test]
        public void S1_17_DangNhap_TaiKhoanSinhVien_ChonRoleAdmin()
        {
            _currentTestCaseId = "S.1.17";
            GetLoginData(_currentTestCaseId, out string role, out string email, out string password);

            SelectRole(role);
            if (!string.IsNullOrEmpty(email)) _loginPage.EmailInput.SendKeys(email);
            if (!string.IsNullOrEmpty(password)) _loginPage.PasswordInput.SendKeys(password);
            _loginPage.ClickDangNhap();

            OpenQA.Selenium.Support.UI.WebDriverWait wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            try { wait.Until(d => _loginPage.MainErrorMessage.Displayed); } catch { }

            Assert.That(_loginPage.MainErrorMessage.Displayed, Is.True, "Lỗi: Không báo lỗi từ chối truy cập khi SV chọn nhầm role Admin.");
            _actualResult = $"Đăng nhập thất bại. Hệ thống từ chối tài khoản {email} do không khớp với vai trò {role}.";
        }

        [Test]
        public void S1_18_DangNhap_TaiKhoanGiaoVien_ChonRoleSinhVien()
        {
            _currentTestCaseId = "S.1.18";
            GetLoginData(_currentTestCaseId, out string role, out string email, out string password);

            SelectRole(role);
            if (!string.IsNullOrEmpty(email)) _loginPage.EmailInput.SendKeys(email);
            if (!string.IsNullOrEmpty(password)) _loginPage.PasswordInput.SendKeys(password);
            _loginPage.ClickDangNhap();

            OpenQA.Selenium.Support.UI.WebDriverWait wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            try { wait.Until(d => _loginPage.MainErrorMessage.Displayed); } catch { }

            Assert.That(_loginPage.MainErrorMessage.Displayed, Is.True, "Lỗi: Không báo lỗi từ chối truy cập khi GV chọn nhầm role SV.");
            _actualResult = $"Đăng nhập thất bại. Hệ thống từ chối tài khoản {email} do không khớp với vai trò {role}.";
        }

        [Test]
        public void S1_19_DangNhap_TaiKhoanGiaoVien_ChonRoleAdmin()
        {
            _currentTestCaseId = "S.1.19";
            GetLoginData(_currentTestCaseId, out string role, out string email, out string password);

            SelectRole(role);
            if (!string.IsNullOrEmpty(email)) _loginPage.EmailInput.SendKeys(email);
            if (!string.IsNullOrEmpty(password)) _loginPage.PasswordInput.SendKeys(password);
            _loginPage.ClickDangNhap();

            OpenQA.Selenium.Support.UI.WebDriverWait wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            try { wait.Until(d => _loginPage.MainErrorMessage.Displayed); } catch { }

            Assert.That(_loginPage.MainErrorMessage.Displayed, Is.True, "Lỗi: Không báo lỗi từ chối truy cập khi GV chọn nhầm role Admin.");
            _actualResult = $"Đăng nhập thất bại. Hệ thống từ chối tài khoản {email} do không khớp với vai trò {role}.";
        }

        [Test]
        public void S1_20_DangNhap_TaiKhoanAdmin_ChonRoleSinhVien()
        {
            _currentTestCaseId = "S.1.20";
            GetLoginData(_currentTestCaseId, out string role, out string email, out string password);

            SelectRole(role);
            if (!string.IsNullOrEmpty(email)) _loginPage.EmailInput.SendKeys(email);
            if (!string.IsNullOrEmpty(password)) _loginPage.PasswordInput.SendKeys(password);
            _loginPage.ClickDangNhap();

            OpenQA.Selenium.Support.UI.WebDriverWait wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            try { wait.Until(d => _loginPage.MainErrorMessage.Displayed); } catch { }

            Assert.That(_loginPage.MainErrorMessage.Displayed, Is.True, "Lỗi: Không báo lỗi từ chối truy cập khi Admin chọn nhầm role SV.");
            _actualResult = $"Đăng nhập thất bại. Khối errorMessage hiển thị thông báo lỗi tài khoản {email} không hợp lệ với vai trò {role}.";
        }

        [Test]
        public void S1_21_DangNhap_TaiKhoanAdmin_ChonRoleGiaoVien()
        {
            _currentTestCaseId = "S.1.21";
            GetLoginData(_currentTestCaseId, out string role, out string email, out string password);

            SelectRole(role);
            if (!string.IsNullOrEmpty(email)) _loginPage.EmailInput.SendKeys(email);
            if (!string.IsNullOrEmpty(password)) _loginPage.PasswordInput.SendKeys(password);
            _loginPage.ClickDangNhap();

            OpenQA.Selenium.Support.UI.WebDriverWait wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            try { wait.Until(d => _loginPage.MainErrorMessage.Displayed); } catch { }

            Assert.That(_loginPage.MainErrorMessage.Displayed, Is.True, "Lỗi: Không báo lỗi từ chối truy cập khi Admin chọn nhầm role GV.");
            _actualResult = $"Đăng nhập thất bại. Khối errorMessage hiển thị thông báo lỗi tài khoản {email} không hợp lệ với vai trò {role}.";
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
                    try
                    {
                        screenshotPath = ScreenshotHelper.TakeScreenshot(_driver, testCaseId);
                    }
                    catch { }

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
            if (_excelManager != null)
            {
                _excelManager.CloseReport();
            }
        }
    }
}