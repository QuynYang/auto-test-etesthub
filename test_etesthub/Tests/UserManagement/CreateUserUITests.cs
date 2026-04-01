using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using test_etesthub.Pages;
using test_etesthub.Helpers;

namespace test_etesthub.Tests.UserManagement
{
    [TestFixture]
    public class CreateUserUITests
    {
        private IWebDriver _driver;
        private LoginPage _loginPage;
        private UserManagementPage _userPage;
        private ExcelReportManager _excelManager;

        private string _currentTestCaseId = "";
        private string _actualResult = "";

        private readonly string _loginUrl = "https://e-testhub-frontend.onrender.com/Home/Login";
        private readonly string _userMgmtUrl = "https://e-testhub-frontend.onrender.com/Admin/UserManagement";
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

            _loginPage.GoToLoginPage(_loginUrl);
            _loginPage.RoleAdminBtn.Click();
            _loginPage.NhapThongTin("admin@gmail.com", "123456");
            _loginPage.ClickDangNhap();
            System.Threading.Thread.Sleep(2000);
        }

        [Test]
        public void S2_1_1_KiemTraDieuHuongVaTieuDe()
        {
            _currentTestCaseId = "S.2.1.1";
            _driver.Navigate().GoToUrl(_userMgmtUrl);
            System.Threading.Thread.Sleep(1000);

            _userPage.AddUserBtn.Click();
            System.Threading.Thread.Sleep(1000);

            bool isCorrectUrl = _driver.Url.Contains("CreateUser");
            string title = _userPage.PageTitle.Text;
            string subtitle = _userPage.PageSubtitle.Text;

            Assert.That(isCorrectUrl, Is.True, "Lỗi: Không chuyển hướng đến trang CreateUser.");
            Assert.That(title, Is.EqualTo("Tạo Người Dùng Mới"), "Lỗi: Sai tiêu đề chính.");
            Assert.That(subtitle, Does.Contain("Thêm người dùng mới"), "Lỗi: Sai tiêu đề phụ.");

            _actualResult = "Trang chuyển hướng thành công. Giao diện hiển thị đúng tiêu đề chính và tiêu đề phụ.";
        }

        [Test]
        public void S2_1_2_KiemTraHienThiDayDuCacTruong()
        {
            _currentTestCaseId = "S.2.1.2";
            _driver.Navigate().GoToUrl(_createUserUrl);

            bool allDisplayed = _userPage.EmailInput.Displayed &&
                                _userPage.FirstNameInput.Displayed &&
                                _userPage.LastNameInput.Displayed &&
                                _userPage.RoleSelect.Displayed &&
                                _userPage.PasswordInput.Displayed &&
                                _userPage.ConfirmPasswordInput.Displayed;

            Assert.That(allDisplayed, Is.True, "Lỗi: Một số trường nhập liệu cơ bản không được hiển thị.");
            _actualResult = "Form hiển thị đầy đủ và rõ ràng 6 trường nhập liệu cơ bản theo đúng thiết kế.";
        }

        [Test]
        public void S2_1_3_KiemTraEmailBatBuoc()
        {
            _currentTestCaseId = "S.2.1.3";
            _driver.Navigate().GoToUrl(_createUserUrl);

            string labelText = _userPage.EmailLabel.Text;
            string isRequired = _userPage.EmailInput.GetAttribute("required");

            Assert.That(labelText, Does.Contain("*"), "Lỗi: Label Email không có dấu sao (*).");
            Assert.That(isRequired, Is.Not.Null, "Lỗi: Thẻ input Email thiếu thuộc tính required.");

            _actualResult = "Label hiển thị chính xác dấu (*). Ô input Email có chứa thuộc tính bắt buộc nhập (required).";
        }

        [Test]
        public void S2_1_4_KiemTraVaiTroLaDropdown()
        {
            _currentTestCaseId = "S.2.1.4";
            _driver.Navigate().GoToUrl(_createUserUrl);

            string tagName = _userPage.RoleSelect.TagName;

            Assert.That(tagName.ToLower(), Is.EqualTo("select"), "Lỗi: Trường Vai Trò không phải là thẻ <select>.");
            _actualResult = "Trường Vai trò được thiết kế dưới dạng thẻ <select> (dropdown) chuẩn xác.";
        }

        [Test]
        public void S2_1_5_KiemTraSoLuongTuyChonVaiTro()
        {
            _currentTestCaseId = "S.2.1.5";
            _driver.Navigate().GoToUrl(_createUserUrl);

            SelectElement select = new SelectElement(_userPage.RoleSelect);
            IList<IWebElement> options = select.Options;

            Assert.That(options.Count, Is.EqualTo(4), "Lỗi: Số lượng tùy chọn trong dropdown không đúng (Phải là 4).");
            Assert.That(options[0].Text, Is.EqualTo("Chọn vai trò"));
            Assert.That(options[1].Text, Is.EqualTo("Sinh Viên"));
            Assert.That(options[2].Text, Is.EqualTo("Giáo Viên"));
            Assert.That(options[3].Text, Is.EqualTo("Quản Trị Viên"));

            _actualResult = "Dropdown hiển thị đầy đủ 4 tùy chọn theo đúng thứ tự thiết kế.";
        }

        [Test]
        public void S2_1_6_KiemTraTruongMatKhauAnKyTu()
        {
            _currentTestCaseId = "S.2.1.6";
            _driver.Navigate().GoToUrl(_createUserUrl);

            string passType = _userPage.PasswordInput.GetAttribute("type");
            string confirmPassType = _userPage.ConfirmPasswordInput.GetAttribute("type");

            _userPage.PasswordInput.SendKeys("TestPass123");
            _userPage.ConfirmPasswordInput.SendKeys("TestPass123");

            Assert.That(passType, Is.EqualTo("password"), "Lỗi: Ô Mật khẩu không ẩn ký tự.");
            Assert.That(confirmPassType, Is.EqualTo("password"), "Lỗi: Ô Xác nhận Mật khẩu không ẩn ký tự.");

            _actualResult = "Cả 2 ô Mật khẩu đều có thuộc tính type='password'. Ký tự nhập vào bị ẩn thành dấu chấm/sao.";
        }

        [Test]
        public void S2_1_7_KiemTraHienThiNutTaoNguoiDung()
        {
            _currentTestCaseId = "S.2.1.7";
            _driver.Navigate().GoToUrl(_createUserUrl);

            bool isDisplayed = _userPage.CreateUserBtn.Displayed;
            bool isEnabled = _userPage.CreateUserBtn.Enabled;
            string classAttr = _userPage.CreateUserBtn.GetAttribute("class");
            string text = _userPage.CreateUserBtn.Text;

            Assert.That(isDisplayed && isEnabled, Is.True, "Lỗi: Nút Tạo Người Dùng không hiển thị hoặc bị Disable.");
            Assert.That(classAttr, Does.Contain("btn-primary"), "Lỗi: Nút không có màu sắc thiết kế chuẩn (btn-primary).");
            Assert.That(text, Does.Contain("Tạo Người Dùng"), "Lỗi: Sai Text trên nút.");

            _actualResult = "Nút hiển thị rõ ràng, màu sắc nổi bật (btn-primary) và ở trạng thái Enable (có thể click).";
        }

        [Test]
        public void S2_1_8_KiemTraChucNangNutQuayLai()
        {
            _currentTestCaseId = "S.2.1.8";
            _driver.Navigate().GoToUrl(_createUserUrl);

            _userPage.BackBtn.Click();
            System.Threading.Thread.Sleep(1500);

            Assert.That(_driver.Url.Contains("UserManagement"), Is.True, "Lỗi: Nút Quay Lại không chuyển hướng về trang Quản Lý.");

            _actualResult = "Click thành công. Hệ thống lập tức chuyển hướng (redirect) quay trở lại trang danh sách Quản Lý Người Dùng.";
        }

        [Test]
        public void S2_1_9_KiemTraThongBaoLoiBoTrong()
        {
            _currentTestCaseId = "S.2.1.9";
            _driver.Navigate().GoToUrl(_createUserUrl);

            _userPage.CreateUserBtn.Click();
            System.Threading.Thread.Sleep(500);

            string emailValidation = _userPage.EmailInput.GetAttribute("validationMessage");
            string firstNameValidation = _userPage.FirstNameInput.GetAttribute("validationMessage");

            bool hasValidationError = !string.IsNullOrEmpty(emailValidation) || !string.IsNullOrEmpty(firstNameValidation);

            Assert.That(hasValidationError, Is.True, "Lỗi: Form vẫn submit thành công dù bỏ trống các trường bắt buộc.");

            _actualResult = "Form bị chặn submit. Trình duyệt báo lỗi yêu cầu nhập liệu ngay tại các trường bắt buộc đang bị bỏ trống.";
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
                        ? "Kiểm tra UI thành công."
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