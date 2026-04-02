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
    public class CreateUserLogicTests
    {
        private IWebDriver _driver;
        private LoginPage _loginPage;
        private UserManagementPage _userPage;
        private ExcelReportManager _excelManager;

        private string _currentTestCaseId = "";
        private string _actualResult = "";

        private readonly string _loginUrl = "https://e-testhub-frontend.onrender.com/Home/Login";
        private readonly string _createUserUrl = "https://e-testhub-frontend.onrender.com/Admin/CreateUser";

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
            string timestamp = DateTime.Now.ToString("ddMMyy_HHmmss");

            if (email == "admin@gmail.com") email = $"admin{timestamp}@gmail.com";
            else if (email == "gv@gmail.com") email = $"gv{timestamp}@gmail.com";
            else if (email == "sv@gmail.com") email = $"sv{timestamp}@gmail.com";

            if (firstName == "Nguyễn" && lastName == "Văn A")
            {
                Random rnd = new Random();
                string[] hoList = { "Trần", "Lê", "Phạm", "Hoàng", "Huỳnh", "Phan", "Vũ", "Võ" };
                string[] tenList = { "Thị B", "Văn C", "Minh D", "Hữu E", "Thanh F", "Ngọc G" };

                firstName = hoList[rnd.Next(hoList.Length)];
                lastName = tenList[rnd.Next(tenList.Length)] + $" {rnd.Next(100, 999)}";
            }

            TestContext.WriteLine($"[DỮ LIỆU TẠO MỚI] Email: '{email}' | Họ: '{firstName}' | Tên: '{lastName}'");
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

        private void ClickSubmitButton()
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", _userPage.CreateUserBtn);
            System.Threading.Thread.Sleep(500);
            try { _userPage.CreateUserBtn.Click(); }
            catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", _userPage.CreateUserBtn); }
        }



        [Test]
        public void S2_1_30_TaoTaiKhoanAdmin()
        {
            _currentTestCaseId = "S.2.1.30";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);
            GenerateDynamicData(ref email, ref firstName, ref lastName);

            FillForm(email, firstName, lastName, role, pass, confirmPass);
            ClickSubmitButton();

            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            bool isSuccess = false;
            try { isSuccess = wait.Until(d => d.FindElement(By.Id("successMessage")).Displayed); } catch { }

            Assert.That(isSuccess, Is.True, "Lỗi: Không tạo được User với phân quyền Admin.");
            _actualResult = $"Form gửi request qua API thành công. Khối successMessage hiển thị thông báo tạo thành công tài khoản {email} với phân quyền Quản Trị Viên.";
        }

        [Test]
        public void S2_1_31_TaoTaiKhoanGiaoVien()
        {
            _currentTestCaseId = "S.2.1.31";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);
            GenerateDynamicData(ref email, ref firstName, ref lastName);

            FillForm(email, firstName, lastName, role, pass, confirmPass);
            ClickSubmitButton();

            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            bool isSuccess = false;
            try { isSuccess = wait.Until(d => d.FindElement(By.Id("successMessage")).Displayed); } catch { }

            Assert.That(isSuccess, Is.True, "Lỗi: Không tạo được User với phân quyền Giáo viên.");
            _actualResult = $"Form gửi request qua API thành công. Khối successMessage hiển thị thông báo tạo thành công tài khoản {email} với phân quyền Giáo Viên.";
        }

        [Test]
        public void S2_1_32_TaoTaiKhoanSinhVien()
        {
            _currentTestCaseId = "S.2.1.32";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);
            GenerateDynamicData(ref email, ref firstName, ref lastName);

            FillForm(email, firstName, lastName, role, pass, confirmPass);
            ClickSubmitButton();

            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            bool isSuccess = false;
            try { isSuccess = wait.Until(d => d.FindElement(By.Id("successMessage")).Displayed); } catch { }

            Assert.That(isSuccess, Is.True, "Lỗi: Không tạo được User với phân quyền Sinh viên.");
            _actualResult = $"Form gửi request qua API thành công. Khối successMessage hiển thị thông báo tạo thành công tài khoản {email} với phân quyền Sinh Viên.";
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
                        ? "Kiểm tra Logic Role thành công."
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