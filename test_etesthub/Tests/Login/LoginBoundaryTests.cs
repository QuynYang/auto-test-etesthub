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
    public class LoginBoundaryTests
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
        public void S1_22_DangNhap_MatKhau5KyTu_NhoHonMin()
        {
            _currentTestCaseId = "S.1.22";
            string[] testData = _excelManager.GetTestDataArray(_currentTestCaseId);

            SelectRole(testData[0]);
            _loginPage.EmailInput.SendKeys(testData[1]);
            _loginPage.PasswordInput.SendKeys(testData[2]);
            _loginPage.ClickDangNhap();

            System.Threading.Thread.Sleep(500);

            string passVal = _loginPage.PasswordInput.GetAttribute("validationMessage");
            string passError = _loginPage.PasswordError.Text;
            bool isBlocked = !string.IsNullOrEmpty(passVal) || !string.IsNullOrEmpty(passError);

            Assert.That(isBlocked, Is.True, "Lỗi: Hệ thống không chặn khi mật khẩu có 5 ký tự (nhỏ hơn MIN).");
            _actualResult = $"Hệ thống chặn submit form và báo lỗi hợp lệ khi nhập mật khẩu {testData[2].Length} ký tự.";
        }

        [Test]
        public void S1_23_DangNhap_MatKhau19KyTu_NhoHonMax()
        {
            _currentTestCaseId = "S.1.23";
            string[] testData = _excelManager.GetTestDataArray(_currentTestCaseId);

            SelectRole(testData[0]);
            _loginPage.EmailInput.SendKeys(testData[1]);
            _loginPage.PasswordInput.SendKeys(testData[2]);
            _loginPage.ClickDangNhap();

            OpenQA.Selenium.Support.UI.WebDriverWait wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            try { wait.Until(d => _loginPage.MainErrorMessage.Displayed); } catch { }

            Assert.That(_loginPage.MainErrorMessage.Displayed, Is.True, "Lỗi: Không đẩy được request lên Server để báo lỗi 'Sai mật khẩu'.");
            _actualResult = $"Hệ thống xử lý bình thường (Frontend không chặn) với mật khẩu {testData[2].Length} ký tự và trả về lỗi từ Server do sai mật khẩu.";
        }

        [Test]
        public void S1_24_DangNhap_MatKhau20KyTu_BangMax()
        {
            _currentTestCaseId = "S.1.24";
            string[] testData = _excelManager.GetTestDataArray(_currentTestCaseId);

            SelectRole(testData[0]);
            _loginPage.EmailInput.SendKeys(testData[1]);
            _loginPage.PasswordInput.SendKeys(testData[2]);
            _loginPage.ClickDangNhap();

            OpenQA.Selenium.Support.UI.WebDriverWait wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            try { wait.Until(d => _loginPage.MainErrorMessage.Displayed); } catch { }

            Assert.That(_loginPage.MainErrorMessage.Displayed, Is.True, "Lỗi: Hệ thống chặn validation sai khi nhập mật khẩu 20 ký tự.");
            _actualResult = $"Hệ thống xử lý bình thường (Frontend không chặn) với mật khẩu {testData[2].Length} ký tự (Max hợp lệ) và trả về lỗi từ Server.";
        }

        [Test]
        public void S1_25_DangNhap_MatKhau21KyTu_VuotQuaMax()
        {
            _currentTestCaseId = "S.1.25";
            string[] testData = _excelManager.GetTestDataArray(_currentTestCaseId);

            SelectRole(testData[0]);
            _loginPage.EmailInput.SendKeys(testData[1]);
            _loginPage.PasswordInput.SendKeys(testData[2]);

            string enteredPass = _loginPage.PasswordInput.GetAttribute("value");
            bool isTruncated = enteredPass.Length < testData[2].Length;

            if (isTruncated)
            {
                Assert.That(isTruncated, Is.True);
                _actualResult = $"Hệ thống thiết lập maxlength. Chặn không cho nhập ký tự thứ {testData[2].Length}, chỉ giữ lại {enteredPass.Length} ký tự trong ô mật khẩu.";
            }
            else
            {
                _loginPage.ClickDangNhap();
                System.Threading.Thread.Sleep(500);

                string passVal = _loginPage.PasswordInput.GetAttribute("validationMessage");
                string passError = _loginPage.PasswordError.Text;
                bool hasError = !string.IsNullOrEmpty(passVal) || !string.IsNullOrEmpty(passError);

                Assert.That(hasError, Is.True, "Lỗi: Hệ thống không có maxlength cũng không chặn validation khi mật khẩu vượt quá Max.");
                _actualResult = $"Hệ thống cho phép gõ nhưng đã chặn submit form và báo lỗi cho mật khẩu vượt giới hạn (21 ký tự).";
            }
        }

        [Test]
        public void S1_26_DangNhap_EmailVuotQuaMax()
        {
            _currentTestCaseId = "S.1.26";
            string[] testData = _excelManager.GetTestDataArray(_currentTestCaseId);

            SelectRole(testData[0]);
            _loginPage.EmailInput.SendKeys(testData[1]); 
            _loginPage.PasswordInput.SendKeys(testData[2]);

            string enteredEmail = _loginPage.EmailInput.GetAttribute("value");
            bool isTruncated = enteredEmail.Length < testData[1].Length;

            if (isTruncated)
            {
                Assert.That(isTruncated, Is.True);
                _actualResult = $"Hệ thống bảo mật tốt. Cắt bớt phần vượt quá giới hạn độ dài của Email, chỉ giữ lại {enteredEmail.Length} ký tự.";
            }
            else
            {
                _loginPage.ClickDangNhap();
                System.Threading.Thread.Sleep(500);

                string emailVal = _loginPage.EmailInput.GetAttribute("validationMessage");
                string emailError = _loginPage.EmailError.Text;
                bool hasError = !string.IsNullOrEmpty(emailVal) || !string.IsNullOrEmpty(emailError);

                Assert.That(hasError, Is.True, "Lỗi: Email cực dài không bị chặn bởi HTML maxlength cũng không bị chặn khi Submit.");
                _actualResult = $"Hệ thống cho phép gõ nhưng đã chặn submit form và báo lỗi cho email vượt giới hạn ký tự.";
            }
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
                        ? "Kiểm tra biên thành công, khớp với yêu cầu thiết kế."
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