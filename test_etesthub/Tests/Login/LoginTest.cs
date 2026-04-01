using test_etesthub.Helpers;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using test_etesthub.Pages;

namespace test_etesthub.Tests.Login
{
    [TestFixture]
    public class LoginTests
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
            _excelManager = new ExcelReportManager(@"C:\Users\vungo\OneDrive\Tài liệu\2025\BDCLPM\LT\baocao.xlsx");
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
        public void S1_1_KiemTraLayoutTrangDangNhap()
        {
            _currentTestCaseId = "S.1.1";

            Assert.That(_loginPage.Logo.Displayed, Is.True, "Logo E-TestHub không hiển thị.");
            Assert.That(_loginPage.IntroBlockLeft.Displayed, Is.True, "Khối giới thiệu bên trái không hiển thị.");
            Assert.That(_loginPage.LoginFormRight.Displayed, Is.True, "Form đăng nhập bên phải không hiển thị.");
            Assert.That(_loginPage.RoleButtonsGroup.Displayed, Is.True, "3 nút vai trò không hiển thị.");
            Assert.That(_loginPage.EmailInput.Displayed, Is.True, "Ô nhập email không hiển thị.");
            Assert.That(_loginPage.LoginButton.Displayed, Is.True, "Nút submit không hiển thị.");
            _actualResult = "Giao diện hiển thị đầy đủ Logo, khối giới thiệu và form đăng nhập, không phát hiện lỗi layout.";
        }

        [Test]
        public void S1_2_KiemTraHienThiVaPlaceholderEmail()
        {
            _currentTestCaseId = "S.1.2";

            Assert.That(_loginPage.EmailInput.Displayed, Is.True, "Ô emailInput không hiển thị.");

            string actualPlaceholder = _loginPage.EmailInput.GetAttribute("placeholder");
            Assert.That(actualPlaceholder, Is.EqualTo("Nhập email của bạn"), "Placeholder của email không chính xác.");
            _actualResult = "Ô nhập Email hiển thị rõ ràng và có placeholder chính xác 'Nhập email của bạn'.";
        }

        [Test]
        public void S1_3_KiemTraHienThiVaAnKyTuMatKhau()
        {
            _currentTestCaseId = "S.1.3";

            string typeAttribute = _loginPage.PasswordInput.GetAttribute("type");
            Assert.That(typeAttribute, Is.EqualTo("password"), "Ô mật khẩu không phải dạng type='password', ký tự sẽ bị lộ.");

            _loginPage.PasswordInput.SendKeys("Abc12345");
            string enteredValue = _loginPage.PasswordInput.GetAttribute("value");

            Assert.That(enteredValue, Is.EqualTo("Abc12345"), "Không thể nhập dữ liệu vào ô mật khẩu.");
            _actualResult = "Ô nhập mật khẩu hiển thị đúng định dạng (type='password'), ký tự khi nhập được ẩn an toàn.";
        }

        [Test]
        public void S1_4_KiemTraNutDangNhapTrangThaiVaClick()
        {
            _currentTestCaseId = "S.1.4";

            Assert.That(_loginPage.LoginButton.Displayed, Is.True, "Nút đăng nhập không hiển thị.");
            Assert.That(_loginPage.LoginButton.Enabled, Is.True, "Nút đăng nhập đang bị Disable.");

            Assert.That(_loginPage.LoginButtonText.Text.Trim(), Is.EqualTo("Đăng nhập"), "Text của nút không phải là 'Đăng nhập'.");

            _loginPage.ClickDangNhap();

            System.Threading.Thread.Sleep(500);

            string emailValidationMsg = _loginPage.EmailInput.GetAttribute("validationMessage");
            string roleErrorText = _loginPage.RoleError.Text; 
            bool hasError = !string.IsNullOrEmpty(emailValidationMsg) || !string.IsNullOrEmpty(roleErrorText);

            Assert.That(hasError, Is.True, "Không hiển thị thông báo lỗi yêu cầu nhập liệu khi nhấn Đăng nhập.");
            _actualResult = "Nút Đăng nhập hiển thị đúng text, không bị disable và kích hoạt thành công cảnh báo bỏ trống khi click.";
        }

        [Test]
        public void S1_5_KiemTraLinkQuenMatKhau()
        {
            _currentTestCaseId = "S.1.5";

            Assert.That(_loginPage.ForgotPasswordLink.Displayed, Is.True, "Link Quên mật khẩu không hiển thị.");
            Assert.That(_loginPage.ForgotPasswordLink.Text.Trim(), Is.EqualTo("Quên mật khẩu?"), "Text không phải là 'Quên mật khẩu?'.");

            _loginPage.ClickQuenMatKhau();
            System.Threading.Thread.Sleep(500);

            Assert.That(_loginPage.LoginFormRight.Displayed, Is.False, "Form đăng nhập chưa bị ẩn đi.");
            Assert.That(_loginPage.ForgotPasswordForm.Displayed, Is.True, "Form Quên mật khẩu chưa xuất hiện.");
            _actualResult = "Link 'Quên mật khẩu?' hiển thị chính xác và có thể tương tác trên form đăng nhập.";
        }

        [Test]
        public void S1_6_KiemTraViTriThongBaoLoi()
        {
            _currentTestCaseId = "S.1.6";

            _loginPage.ClickDangNhap();
            System.Threading.Thread.Sleep(500);

            int emailInputY = _loginPage.EmailInput.Location.Y;
            int emailErrorY = _loginPage.EmailError.Location.Y;

            Assert.That(emailErrorY, Is.GreaterThanOrEqualTo(emailInputY),
                "UX Lỗi: Thông báo lỗi Email không nằm bên dưới ô nhập liệu!");

            int passwordInputY = _loginPage.PasswordInput.Location.Y;
            int passwordErrorY = _loginPage.PasswordError.Location.Y;

            Assert.That(passwordErrorY, Is.GreaterThanOrEqualTo(passwordInputY),
                "UX Lỗi: Thông báo lỗi Password không nằm bên dưới ô mật khẩu!");
            _actualResult = "Thông báo lỗi hiển thị chuẩn UX (nằm ngay bên dưới ô nhập liệu Email và Password tương ứng).";
        }

        [Test]
        public void S1_7_KiemTraThuTuTab()
        {
            _currentTestCaseId = "S.1.7";

            _loginPage.EmailInput.Click();
            Assert.That(_driver.SwitchTo().ActiveElement().GetAttribute("id"), Is.EqualTo("emailInput"));

            _driver.SwitchTo().ActiveElement().SendKeys(Keys.Tab);
            Assert.That(_driver.SwitchTo().ActiveElement().GetAttribute("id"), Is.EqualTo("passwordInput"), "Tab từ Email không nhảy sang Password.");

            _driver.SwitchTo().ActiveElement().SendKeys(Keys.Tab);
            Assert.That(_driver.SwitchTo().ActiveElement().GetAttribute("id"), Is.EqualTo("rememberMe"), "Tab từ Password không nhảy sang Ghi nhớ đăng nhập.");

            _driver.SwitchTo().ActiveElement().SendKeys(Keys.Tab);
            Assert.That(_driver.SwitchTo().ActiveElement().GetAttribute("id"), Is.EqualTo("forgotPasswordLink"), "Tab không nhảy sang Quên mật khẩu.");

            _driver.SwitchTo().ActiveElement().SendKeys(Keys.Tab);
            Assert.That(_driver.SwitchTo().ActiveElement().GetAttribute("id"), Is.EqualTo("loginButton"), "Tab cuối cùng không trỏ vào nút Đăng nhập.");
            _actualResult = "Thứ tự phím Tab di chuyển logic đúng chuẩn: Email -> Password -> Ghi nhớ Đăng nhập -> Quên mật khẩu -> Nút Đăng nhập.";
        }

        [Test]
        public void S1_8_KiemTraGiaoDienResponsive()
        {
            _currentTestCaseId = "S.1.8";

            // màn hình Mobile
            _driver.Manage().Window.Size = new System.Drawing.Size(390, 844);
            System.Threading.Thread.Sleep(1000);

            Assert.That(_loginPage.LoginFormRight.Displayed, Is.True, "Trên Mobile: Form đăng nhập bị mất tích!");
            Assert.That(_loginPage.LoginFormRight.Size.Width, Is.LessThanOrEqualTo(390), "Trên Mobile: Form đăng nhập bị tràn màn hình chiều ngang!");

            // màn hình Tablet
            _driver.Manage().Window.Size = new System.Drawing.Size(768, 1024);
            System.Threading.Thread.Sleep(1000);

            Assert.That(_loginPage.LoginFormRight.Displayed, Is.True, "Trên Tablet: Form đăng nhập bị mất!");
            Assert.That(_loginPage.RoleButtonsGroup.Displayed, Is.True, "Trên Tablet: Không thấy các nút chọn vai trò!");

            //màn hình PC
            _driver.Manage().Window.Size = new System.Drawing.Size(1920, 1080);
            System.Threading.Thread.Sleep(1000);

            Assert.That(_loginPage.IntroBlockLeft.Displayed, Is.True, "Trên PC: Khối giới thiệu bên trái bị lỗi không hiển thị!");
            _actualResult = "Đã xác nhận form đăng nhập tự động co giãn, không bị tràn viền hay vỡ layout trên đa thiết bị (Mobile 390px, Tablet 768px, PC).";
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