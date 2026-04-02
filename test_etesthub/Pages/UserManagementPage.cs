using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace test_etesthub.Pages
{
    public class UserManagementPage
    {
        private readonly IWebDriver _driver;

        public UserManagementPage(IWebDriver driver)
        {
            _driver = driver;
        }

        public IWebElement AddUserBtn => _driver.FindElement(By.CssSelector(".action-buttons a.btn-primary"));

        public IWebElement PageTitle => _driver.FindElement(By.CssSelector(".create-user-title"));
        public IWebElement PageSubtitle => _driver.FindElement(By.CssSelector(".create-user-subtitle"));

        public IWebElement EmailInput => _driver.FindElement(By.Id("Email"));
        public IWebElement EmailLabel => _driver.FindElement(By.CssSelector("label[for='Email']"));

        public IWebElement FirstNameInput => _driver.FindElement(By.Id("firstName"));
        public IWebElement LastNameInput => _driver.FindElement(By.Id("lastName"));

        public IWebElement RoleSelect => _driver.FindElement(By.Id("roleSelect"));

        public IWebElement PasswordInput => _driver.FindElement(By.Id("Password"));
        public IWebElement ConfirmPasswordInput => _driver.FindElement(By.Id("ConfirmPassword"));

        public IWebElement CreateUserBtn => _driver.FindElement(By.Id("createUserBtn"));
        public IWebElement BackBtn => _driver.FindElement(By.XPath("//a[contains(@class, 'btn-secondary') and contains(., 'Quay Lại')]"));

        public IWebElement SearchInput => _driver.FindElement(By.Id("searchInput"));
        public IWebElement UsersTable => _driver.FindElement(By.Id("usersTable"));
        // CÁC HÀM HỖ TRỢ TƯƠNG TÁC THÔNG MINH
        public void ScrollToElement(IWebElement element)
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", element);
            System.Threading.Thread.Sleep(300); // Đợi cuộn
        }

        public void ClickCreateUserBtn()
        {
            ScrollToElement(CreateUserBtn);
            try
            {
                CreateUserBtn.Click();
            }
            catch (ElementClickInterceptedException)
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", CreateUserBtn);
            }
        }

        public void ClickBackBtn()
        {
            ScrollToElement(BackBtn);
            try
            {
                BackBtn.Click();
            }
            catch (ElementClickInterceptedException)
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", BackBtn);
            }
        }
    }
}