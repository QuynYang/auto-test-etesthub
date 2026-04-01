using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace test_etesthub.Pages
{
    public class LoginPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public LoginPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // Định nghĩa các Elements
        public IWebElement Logo => _driver.FindElement(By.CssSelector(".navbar-brand"));

        public IWebElement IntroBlockLeft => _driver.FindElement(By.CssSelector(".etesthub-left-content"));

        // Form đăng nhập
        public IWebElement LoginFormRight => _driver.FindElement(By.Id("loginForm"));

        public IWebElement RoleButtonsGroup => _driver.FindElement(By.CssSelector(".d-grid.gap-2"));

        public IWebElement EmailInput => _driver.FindElement(By.Id("emailInput"));
        public IWebElement PasswordInput => _driver.FindElement(By.Id("passwordInput"));
        public IWebElement LoginButton => _driver.FindElement(By.Id("loginButton"));

        public IWebElement LoginButtonText => _driver.FindElement(By.Id("loginButtonText"));

        public IWebElement ForgotPasswordLink => _driver.FindElement(By.Id("forgotPasswordLink"));
        public IWebElement ForgotPasswordForm => _driver.FindElement(By.Id("forgotPasswordForm"));
        public IWebElement RoleStudentBtn => _driver.FindElement(By.Id("roleStudentBtn"));
        public IWebElement RoleAdminBtn => _driver.FindElement(By.Id("roleAdminBtn"));
        public IWebElement RoleTeacherBtn => _driver.FindElement(By.Id("roleTeacherBtn"));
        public IWebElement MainErrorMessage => _driver.FindElement(By.Id("errorMessage"));

        // Thông báo lỗi
        public IWebElement EmailError => _driver.FindElement(By.Id("emailError"));
        public IWebElement RoleError => _driver.FindElement(By.Id("roleError"));
        public IWebElement PasswordError => _driver.FindElement(By.Id("passwordError"));

        // Các hàm hỗ trợ tương tác với trang
        public void GoToLoginPage(string url)
        {
            _driver.Navigate().GoToUrl(url);
        }

        public void NhapThongTin(string email, string password)
        {
            EmailInput.Clear();
            EmailInput.SendKeys(email);

            PasswordInput.Clear();
            PasswordInput.SendKeys(password);
        }

        public void ClickDangNhap()
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", LoginButton);

            System.Threading.Thread.Sleep(200);

            try
            {
                LoginButton.Click();
            }
            catch (ElementClickInterceptedException)
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", LoginButton);
            }
        }

        public void ClickQuenMatKhau()
        {
            ForgotPasswordLink.Click();
        }
    }
}