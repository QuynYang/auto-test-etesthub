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
    public class CreateUserValidationTests
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
            else
            {
                foreach (var item in testData)
                {
                    if (item.Contains("@")) email = item;
                    else if (item.Contains("Viên") || item.Contains("Vien") || item.Contains("Admin")) role = item;
                    else if (item.Length >= 6 && item.All(char.IsDigit)) { if (pass == "") pass = item; else confirmPass = item; }
                    else if (item == "Nguyễn" || item == "  ") firstName = item;
                    else if (item == "Văn A" || item == "  ") lastName = item;
                }

                if (string.IsNullOrEmpty(email) && testCaseId != "S.2.1.13") email = "sv@gmail.com";
                if (string.IsNullOrEmpty(firstName) && testCaseId != "S.2.1.14" && testCaseId != "S.2.1.20") firstName = "Nguyễn";
                if (string.IsNullOrEmpty(lastName) && testCaseId != "S.2.1.15" && testCaseId != "S.2.1.20") lastName = "Văn A";
                if (string.IsNullOrEmpty(role) && testCaseId != "S.2.1.16") role = "Sinh Viên";
                if (string.IsNullOrEmpty(pass) && testCaseId != "S.2.1.18") pass = "123456";
                if (string.IsNullOrEmpty(confirmPass) && testCaseId != "S.2.1.19" && testCaseId != "S.2.1.17") confirmPass = "123456";
            }
        }

        private void GenerateDynamicData(ref string email, ref string firstName, ref string lastName)
        {
            if (email == "sv@gmail.com" || email == " sv@gmail.com ") 
            {
                string timestamp = DateTime.Now.ToString("ddMMyy_HHmmss");
                if (email.Contains(" ")) email = $" sv{timestamp}@gmail.com "; 
                else email = $"sv{timestamp}@gmail.com";
            }

            if (firstName == "Nguyễn" && lastName == "Văn A")
            {
                Random rnd = new Random();
                string[] hoList = { "Trần", "Lê", "Phạm", "Hoàng", "Huỳnh", "Phan", "Vũ", "Võ" };
                string[] tenList = { "Thị B", "Văn C", "Minh D", "Hữu E", "Thanh F", "Ngọc G" };

                firstName = hoList[rnd.Next(hoList.Length)];
                lastName = tenList[rnd.Next(tenList.Length)] + $" {rnd.Next(100, 999)}";
            }

            TestContext.WriteLine($"[DỮ LIỆU] Email: '{email}' | Họ: '{firstName}' | Tên: '{lastName}'");
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

        // Hàm dùng chung điền form
        private void FillForm(string email, string firstName, string lastName, string role, string pass, string confirmPass)
        {
            if (!string.IsNullOrEmpty(email)) _userPage.EmailInput.SendKeys(email);
            if (!string.IsNullOrEmpty(firstName)) _userPage.FirstNameInput.SendKeys(firstName);
            if (!string.IsNullOrEmpty(lastName)) _userPage.LastNameInput.SendKeys(lastName);
            SelectRoleByText(role);
            if (!string.IsNullOrEmpty(pass)) _userPage.PasswordInput.SendKeys(pass);
            if (!string.IsNullOrEmpty(confirmPass)) _userPage.ConfirmPasswordInput.SendKeys(confirmPass);
        }


        [Test]
        public void S2_1_10_TaoUserThanhCong()
        {
            _currentTestCaseId = "S.2.1.10";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);
            GenerateDynamicData(ref email, ref firstName, ref lastName);

            FillForm(email, firstName, lastName, role, pass, confirmPass);

            _userPage.ClickCreateUserBtn();
            System.Threading.Thread.Sleep(2000);

            bool isSuccess = false;
            try
            {
                IWebElement successMsg = _driver.FindElement(By.Id("successMessage"));
                isSuccess = successMsg.Displayed && successMsg.Text.Contains("thành công");
            }
            catch { }

            Assert.That(isSuccess, Is.True, "Lỗi: Không hiển thị thông báo tạo thành công hoặc lỗi từ phía API.");
            _actualResult = $"Hệ thống xử lý thành công. Hiển thị thông báo (successMessage). Đã tạo mới user {email}.";
        }

        [Test]
        public void S2_1_11_EmailDaTonTai()
        {
            _currentTestCaseId = "S.2.1.11";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);

            email = "sv00001@gmail.com";

            FillForm(email, firstName, lastName, role, pass, confirmPass);
            _userPage.ClickCreateUserBtn();

            System.Threading.Thread.Sleep(1000);

            bool isErrorShown = false;
            string foundErrorMessage = "";

            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            try
            {
                isErrorShown = wait.Until(d =>
                {
                    var errorSpans = d.FindElements(By.CssSelector("span.text-danger, span[data-valmsg-for='Email'], #emailError"));

                    foreach (var span in errorSpans)
                    {
                        if (span.Displayed && (span.Text.Contains("tồn tại") || span.Text.Contains("sử dụng")))
                        {
                            foundErrorMessage = span.Text;
                            return true;
                        }
                    }

                    var globalAlert = d.FindElements(By.Id("errorMessage")).FirstOrDefault();
                    if (globalAlert != null && globalAlert.Displayed && (globalAlert.Text.Contains("tồn tại") || globalAlert.Text.Contains("sử dụng")))
                    {
                        foundErrorMessage = globalAlert.Text;
                        return true;
                    }

                    return false;
                });
            }
            catch (WebDriverTimeoutException)
            {
                isErrorShown = false;
            }

            TestContext.WriteLine($"[DEBUG] Trạng thái báo lỗi: {isErrorShown} | Nội dung lỗi: '{foundErrorMessage}'");

            Assert.That(isErrorShown, Is.True, "Lỗi: Đã điền full form và click Submit nhưng hệ thống không báo lỗi Email đã sử dụng.");
            _actualResult = $"Hệ thống đã chặn tạo mới và hiển thị thông báo lỗi chính xác: '{foundErrorMessage}'.";
        }

        [Test]
        public void S2_1_12_EmailSaiDinhDang()
        {
            _currentTestCaseId = "S.2.1.12";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);

            email = "sv";

            FillForm(email, firstName, lastName, role, pass, confirmPass);

            _userPage.ClickCreateUserBtn();

            bool isErrorShown = false;
            string foundErrorMessage = "";

            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            try
            {
                isErrorShown = wait.Until(d =>
                {
                    try
                    {
                        var emailInput = d.FindElement(By.Id("Email"));
                        string html5Val = emailInput.GetAttribute("validationMessage");
                        if (!string.IsNullOrEmpty(html5Val))
                        {
                            foundErrorMessage = html5Val;
                            return true;
                        }

                        var errorSpans = d.FindElements(By.CssSelector("span.text-danger, span[data-valmsg-for='Email'], #emailError, span.field-validation-error"));
                        foreach (var span in errorSpans)
                        {
                            if (span.Displayed && !string.IsNullOrEmpty(span.Text.Trim()))
                            {
                                foundErrorMessage = span.Text.Trim();
                                return true;
                            }
                        }
                    }
                    catch (Exception) { }

                    return false;
                });
            }
            catch (WebDriverTimeoutException)
            {
                isErrorShown = false;
            }

            TestContext.WriteLine($"[DEBUG] Trạng thái báo lỗi: {isErrorShown} | Nội dung lỗi: '{foundErrorMessage}'");

            Assert.That(isErrorShown, Is.True, "Lỗi: Hệ thống đã cho phép tạo User với Email sai định dạng (Test case này Fail tức là Backend/Frontend đang có bug bảo mật).");
            _actualResult = $"Hệ thống đã chặn submit form và báo lỗi định dạng: '{foundErrorMessage}'.";
        }

        [Test]
        public void S2_1_13_BoTrongEmail()
        {
            _currentTestCaseId = "S.2.1.13";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);

            FillForm("", firstName, lastName, role, pass, confirmPass);
            _userPage.ClickCreateUserBtn();
            System.Threading.Thread.Sleep(500);

            string emailVal = _userPage.EmailInput.GetAttribute("validationMessage");
            string spanError = "";
            try { spanError = _driver.FindElement(By.CssSelector("span[data-valmsg-for='Email']")).Text; } catch { }

            bool hasError = !string.IsNullOrEmpty(emailVal) || !string.IsNullOrEmpty(spanError);

            Assert.That(hasError, Is.True, "Lỗi: Không bắt buộc nhập Email.");
            _actualResult = "Form bị chặn submit do thuộc tính required. Báo lỗi yêu cầu người dùng phải nhập Email.";
        }

        [Test]
        public void S2_1_14_BoTrongHo()
        {
            _currentTestCaseId = "S.2.1.14";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);
            GenerateDynamicData(ref email, ref firstName, ref lastName);

            FillForm(email, "", lastName, role, pass, confirmPass);
            _userPage.ClickCreateUserBtn();
            System.Threading.Thread.Sleep(500);

            string valMsg = _userPage.FirstNameInput.GetAttribute("validationMessage");
            string spanError = "";
            try { spanError = _driver.FindElement(By.Id("firstNameError")).Text; } catch { }

            bool hasError = !string.IsNullOrEmpty(valMsg) || !string.IsNullOrEmpty(spanError);

            Assert.That(hasError, Is.True, "Lỗi: Không bắt buộc nhập Họ.");
            _actualResult = "Form bị chặn submit. Cảnh báo required hiển thị tại trường Họ.";
        }

        [Test]
        public void S2_1_15_BoTrongTen()
        {
            _currentTestCaseId = "S.2.1.15";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);
            GenerateDynamicData(ref email, ref firstName, ref lastName);

            FillForm(email, firstName, "", role, pass, confirmPass);
            _userPage.ClickCreateUserBtn();
            System.Threading.Thread.Sleep(500);

            string valMsg = _userPage.LastNameInput.GetAttribute("validationMessage");
            string spanError = "";
            try { spanError = _driver.FindElement(By.Id("lastNameError")).Text; } catch { }

            bool hasError = !string.IsNullOrEmpty(valMsg) || !string.IsNullOrEmpty(spanError);

            Assert.That(hasError, Is.True, "Lỗi: Không bắt buộc nhập Tên.");
            _actualResult = "Form bị chặn submit. Cảnh báo required hiển thị tại trường Tên.";
        }

        [Test]
        public void S2_1_16_KhongChonVaiTro()
        {
            _currentTestCaseId = "S.2.1.16";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);
            GenerateDynamicData(ref email, ref firstName, ref lastName);

            FillForm(email, firstName, lastName, "", pass, confirmPass);
            _userPage.ClickCreateUserBtn();
            System.Threading.Thread.Sleep(500);

            string valMsg = _userPage.RoleSelect.GetAttribute("validationMessage");
            string spanError = "";
            try { spanError = _driver.FindElement(By.CssSelector("span[data-valmsg-for='Role']")).Text; } catch { }

            bool hasError = !string.IsNullOrEmpty(valMsg) || !string.IsNullOrEmpty(spanError);

            Assert.That(hasError, Is.True, "Lỗi: Hệ thống cho phép tạo User mà không có Vai trò.");
            _actualResult = "Form bị chặn submit. Trình duyệt báo lỗi yêu cầu chọn ít nhất một vai trò hợp lệ trong danh sách dropdown.";
        }

        [Test]
        public void S2_1_17_MatKhauKhongKhop()
        {
            _currentTestCaseId = "S.2.1.17";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);

            _userPage.PasswordInput.SendKeys(pass);
            _userPage.ConfirmPasswordInput.SendKeys(confirmPass);
            _userPage.ConfirmPasswordInput.SendKeys(Keys.Tab); 

            System.Threading.Thread.Sleep(1000);

            bool isMismatchErrorShown = false;
            try
            {
                IWebElement mismatchError = _driver.FindElement(By.Id("passwordMismatchError"));
                isMismatchErrorShown = mismatchError.Displayed;
            }
            catch { }

            Assert.That(isMismatchErrorShown, Is.True, "Lỗi: Javascript xác nhận mật khẩu không hoạt động.");
            _actualResult = "Javascript tự động kiểm tra. Thẻ input bị báo lỗi is-invalid và hiển thị dòng text 'Mật khẩu xác nhận không khớp'.";
        }

        [Test]
        public void S2_1_18_BoTrongMatKhau()
        {
            _currentTestCaseId = "S.2.1.18";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);

            FillForm(email, firstName, lastName, role, "", confirmPass);
            _userPage.ClickCreateUserBtn();
            System.Threading.Thread.Sleep(500);

            string valMsg = _userPage.PasswordInput.GetAttribute("validationMessage");
            string spanError = "";
            try { spanError = _driver.FindElement(By.CssSelector("span[data-valmsg-for='Password']")).Text; } catch { }

            bool hasError = !string.IsNullOrEmpty(valMsg) || !string.IsNullOrEmpty(spanError);

            Assert.That(hasError, Is.True, "Lỗi: Không bắt buộc nhập Mật khẩu.");
            _actualResult = "Form bị chặn submit do thuộc tính required. Báo lỗi dưới ô mật khẩu.";
        }

        [Test]
        public void S2_1_19_BoTrongXacNhanMatKhau()
        {
            _currentTestCaseId = "S.2.1.19";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);

            FillForm(email, firstName, lastName, role, pass, "");
            _userPage.ClickCreateUserBtn();
            System.Threading.Thread.Sleep(500);

            string valMsg = _userPage.ConfirmPasswordInput.GetAttribute("validationMessage");
            string spanError = "";
            try { spanError = _driver.FindElement(By.CssSelector("span[data-valmsg-for='ConfirmPassword']")).Text; } catch { }

            bool hasError = !string.IsNullOrEmpty(valMsg) || !string.IsNullOrEmpty(spanError);

            Assert.That(hasError, Is.True, "Lỗi: Không bắt buộc Xác nhận Mật khẩu.");
            _actualResult = "Form bị chặn submit do thuộc tính required. Báo lỗi dưới ô xác nhận mật khẩu.";
        }

        [Test]
        public void S2_1_20_BoTrongHoTenBangSpace()
        {
            _currentTestCaseId = "S.2.1.20";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);
            GenerateDynamicData(ref email, ref firstName, ref lastName);

            FillForm(email, "   ", "   ", role, pass, confirmPass);
            _userPage.ClickCreateUserBtn();
            System.Threading.Thread.Sleep(1000);

            bool isPrevented = !_driver.Url.Contains("UserManagement");
            Assert.That(isPrevented, Is.True, "Lỗi Nghiêm trọng: Hệ thống cho phép tạo User chỉ chứa toàn khoảng trắng (Space)!");
            _actualResult = "Hệ thống chặn submit form và báo lỗi yêu cầu nhập Họ Tên hợp lệ, không chấp nhận dữ liệu rỗng.";
        }

        [Test]
        public void S2_1_21_KiemTraTinhNangTrim()
        {
            _currentTestCaseId = "S.2.1.21";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);

            email = " sv@gmail.com ";
            GenerateDynamicData(ref email, ref firstName, ref lastName);

            FillForm(email, firstName, lastName, role, pass, confirmPass);
            _userPage.ClickCreateUserBtn();
            System.Threading.Thread.Sleep(2000);

            bool isSuccess = false;
            try
            {
                isSuccess = _driver.FindElement(By.Id("successMessage")).Displayed;
            }
            catch { }

            Assert.That(isSuccess, Is.True, "Lỗi: Tính năng Trim không hoạt động, hệ thống báo lỗi định dạng do khoảng trắng thừa.");
            _actualResult = $"Hệ thống tự động cắt bỏ khoảng trắng (Trim) cho chuỗi email. Tạo user thành công.";
        }

        [Test]
        public void S2_1_22_NhapKyTuDacBietVaoTen()
        {
            _currentTestCaseId = "S.2.1.22";
            _driver.Navigate().GoToUrl(_createUserUrl);

            GetUserFormData(_currentTestCaseId, out string email, out string firstName, out string lastName, out string role, out string pass, out string confirmPass);

            firstName = "Nguyễn 123";
            lastName = "V@n A!";

            FillForm(email, firstName, lastName, role, pass, confirmPass);
            _userPage.ClickCreateUserBtn();
            System.Threading.Thread.Sleep(1000);

            bool isPrevented = !_driver.Url.Contains("UserManagement");
            Assert.That(isPrevented, Is.True, "Lỗi: Không có filter chặn nhập Ký tự đặc biệt hoặc số vào Tên người dùng.");
            _actualResult = "Form báo lỗi validation, không cho phép nhập số hoặc ký tự đặc biệt (!@#) vào Họ và Tên.";
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
                        ? "Kiểm tra Validation thành công."
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