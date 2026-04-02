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
    public class CreateUserBoundaryTests
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

        private void GenerateDynamicData(ref string email, ref string firstName, string originalLastName)
        {
            if (email == "sv@gmail.com")
            {
                string timestamp = DateTime.Now.ToString("ddMMyy_HHmmss");
                email = $"sv{timestamp}@gmail.com";
            }

            Random rnd = new Random();
            if (firstName == "Nguyễn")
            {
                firstName = $"Nguyễn {rnd.Next(1000, 9999)}";
            }

            TestContext.WriteLine($"[DỮ LIỆU BIÊN] Email: '{email}' | Họ: '{firstName}' | Tên: '{originalLastName}'");
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
        public void S2_1_23_MatKhau6KyTu_BangMin()
        {
            _currentTestCaseId = "S.2.1.23";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);
            GenerateDynamicData(ref email, ref firstName, lastName);

            FillForm(email, firstName, lastName, role, pass, confirmPass); 
            ClickSubmitButton();

            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            bool isSuccess = false;
            try { isSuccess = wait.Until(d => d.FindElement(By.Id("successMessage")).Displayed); } catch { }

            Assert.That(isSuccess, Is.True, "Lỗi: Không tạo được User với mật khẩu 6 ký tự (Bằng MIN).");
            _actualResult = $"Hệ thống cho phép submit form mà không cảnh báo lỗi. Tạo user {email} thành công.";
        }

        [Test]
        public void S2_1_24_MatKhau5KyTu_NhoHonMin()
        {
            _currentTestCaseId = "S.2.1.24";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);
            GenerateDynamicData(ref email, ref firstName, lastName);

            FillForm(email, firstName, lastName, role, pass, confirmPass); 
            ClickSubmitButton();
            System.Threading.Thread.Sleep(1000);

            string foundErrorMessage = "";
            try
            {
                var errorSpans = _driver.FindElements(By.CssSelector("span.text-danger, span[data-valmsg-for='Password']"));
                foreach (var span in errorSpans)
                {
                    if (span.Displayed && span.Text.Contains("ít nhất 6 ký tự"))
                    {
                        foundErrorMessage = span.Text;
                        break;
                    }
                }
            }
            catch { }

            Assert.That(!string.IsNullOrEmpty(foundErrorMessage), Is.True, "Lỗi: Hệ thống không báo lỗi khi mật khẩu < 6 ký tự.");
            _actualResult = $"Form bị chặn submit. Hiển thị thông báo lỗi chính xác: '{foundErrorMessage}'.";
        }

        [Test]
        public void S2_1_25_MatKhau20KyTu_BangMax()
        {
            _currentTestCaseId = "S.2.1.25";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);
            GenerateDynamicData(ref email, ref firstName, lastName);

            FillForm(email, firstName, lastName, role, pass, confirmPass); 
            ClickSubmitButton();

            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            bool isSuccess = false;
            try { isSuccess = wait.Until(d => d.FindElement(By.Id("successMessage")).Displayed); } catch { }

            Assert.That(isSuccess, Is.True, "Lỗi: Không tạo được User với mật khẩu 20 ký tự (Bằng MAX).");
            _actualResult = $"Hệ thống cho phép submit form. Tạo user {email} thành công với mật khẩu 20 ký tự.";
        }

        [Test]
        public void S2_1_26_MatKhau21KyTu_LonHonMax()
        {
            _currentTestCaseId = "S.2.1.26";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);
            GenerateDynamicData(ref email, ref firstName, lastName);

            _userPage.PasswordInput.SendKeys(pass); 

            string enteredPass = _userPage.PasswordInput.GetAttribute("value");
            bool isTruncated = enteredPass.Length < pass.Length && enteredPass.Length <= 20;

            Assert.That(isTruncated, Is.True, "Lỗi: Thẻ input Mật khẩu không có maxlength='20', cho phép nhập quá 20 ký tự.");
            _actualResult = $"Ô input đã chặn không cho gõ ký tự thứ 21. Chỉ hiển thị {enteredPass.Length} ký tự do thuộc tính maxlength hợp lệ.";
        }

        [Test]
        public void S2_1_27_Ten1KyTu_BangMin()
        {
            _currentTestCaseId = "S.2.1.27";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);
            GenerateDynamicData(ref email, ref firstName, lastName);

            FillForm(email, firstName, lastName, role, pass, confirmPass);
            ClickSubmitButton();

            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            bool isSuccess = false;
            try { isSuccess = wait.Until(d => d.FindElement(By.Id("successMessage")).Displayed); } catch { }

            Assert.That(isSuccess, Is.True, "Lỗi: Không tạo được User với Tên 1 ký tự (Bằng MIN).");
            _actualResult = $"Hệ thống cho phép submit form không báo lỗi. Tạo user {email} thành công với Tên 1 ký tự.";
        }

        [Test]
        public void S2_1_28_Ten50KyTu_BangMax()
        {
            _currentTestCaseId = "S.2.1.28";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);
            GenerateDynamicData(ref email, ref firstName, lastName); 

            FillForm(email, firstName, lastName, role, pass, confirmPass);
            ClickSubmitButton();

            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            bool isSuccess = false;
            try { isSuccess = wait.Until(d => d.FindElement(By.Id("successMessage")).Displayed); } catch { }

            Assert.That(isSuccess, Is.True, "Lỗi: Không tạo được User với Tên 50 ký tự (Bằng MAX).");
            _actualResult = $"Hệ thống cho phép submit form. Tạo user {email} thành công với Tên 50 ký tự.";
        }

        [Test]
        public void S2_1_29_TenVuotQua50KyTu_LonHonMax()
        {
            _currentTestCaseId = "S.2.1.29";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);
            GenerateDynamicData(ref email, ref firstName, lastName);

            _userPage.LastNameInput.SendKeys(lastName); 

            string enteredName = _userPage.LastNameInput.GetAttribute("value");
            bool isTruncated = enteredName.Length < lastName.Length && enteredName.Length <= 50;

            Assert.That(isTruncated, Is.True, "Lỗi: Thẻ input Tên không có maxlength='50', cho phép nhập quá 50 ký tự.");
            _actualResult = $"Ô input đã chặn không cho gõ ký tự thứ {lastName.Length}. Chỉ nhận {enteredName.Length} ký tự do thuộc tính maxlength hợp lệ.";
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
                        ? "Kiểm tra giá trị biên thành công."
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