using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using test_etesthub.Helpers;

namespace test_etesthub.Tests.Login
{
    [TestFixture]
    public class LoginIntegrationTests
    {
        private ExcelReportManager _excelManager;
        private string _currentTestCaseId = "";
        private string _actualResult = "";

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
            TestContext.WriteLine($"[HỆ THỐNG] Bắt đầu chạy Test Case API ngầm qua HttpClient...");
        }

        private void GetApiTestDataFromExcel(string testCaseId, out string url, out string payload)
        {
            string[] testData = _excelManager.GetTestDataArray(testCaseId);
            url = ""; payload = "";

            foreach (var cell in testData)
            {
                if (cell != null)
                {
                    string trimmedCell = cell.Trim();
                    if (trimmedCell.StartsWith("http")) url = trimmedCell;
                    if (trimmedCell.StartsWith("{")) payload = trimmedCell;
                }
            }

            if (string.IsNullOrEmpty(url)) Assert.Fail($"Lỗi dữ liệu Excel: Không tìm thấy URL hợp lệ ở Test Case {testCaseId}");
            if (string.IsNullOrEmpty(payload)) Assert.Fail($"Lỗi dữ liệu Excel: Không tìm thấy chuỗi JSON payload hợp lệ ở Test Case {testCaseId}");

            TestContext.WriteLine($"[EXCEL DATA] URL: {url}");
            TestContext.WriteLine($"[EXCEL DATA] Payload: {payload}");
        }



        [Test]
        public async Task S1_40_KiemTraApiDangNhap_TraVeTokenHopLe()
        {
            _currentTestCaseId = "S.1.40";

            GetApiTestDataFromExcel(_currentTestCaseId, out string url, out string payload);

            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                string responseBody = await response.Content.ReadAsStringAsync();
                int statusCode = (int)response.StatusCode;

                TestContext.WriteLine($"[API RESPONSE S.1.40] Status: {statusCode} | Body: {responseBody}");

                Assert.That(statusCode, Is.EqualTo(200), $"Lỗi: API giao tiếp thất bại, trả về Status Code {statusCode}");
                Assert.That(responseBody.Contains("token") || responseBody.Contains("session") || responseBody.ToLower().Contains("success"), Is.True, "Lỗi: Response Body không chứa Token/Session hợp lệ.");

                _actualResult = "API giao tiếp Database thành công. Trả về Status Code 200 OK. Response Body chứa Token/Session hợp lệ (Dữ liệu bốc từ Excel).";
            }
        }

        [Test]
        public async Task S1_41_KiemTraApiDangNhap_XuLyLoiAnToan()
        {
            _currentTestCaseId = "S.1.41";

            GetApiTestDataFromExcel(_currentTestCaseId, out string url, out string payload);

            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                string responseBody = await response.Content.ReadAsStringAsync();
                int statusCode = (int)response.StatusCode;

                TestContext.WriteLine($"[API RESPONSE S.1.41] Status: {statusCode} | Body: {responseBody}");

                bool isSafeError = (statusCode == 401 || statusCode == 404 || statusCode == 400);

                Assert.That(statusCode, Is.Not.EqualTo(500), "Lỗi nghiêm trọng: Backend bị crash (Internal Server Error) khi không tìm thấy tài khoản!");
                Assert.That(isSafeError, Is.True, $"Lỗi: Backend trả về mã lỗi không chuẩn (Status: {statusCode}).");

                _actualResult = $"API xử lý lỗi an toàn (không bị crash 500). Trả về chuẩn Status Code {statusCode} kèm message lỗi (Data bốc từ Excel).";
            }
        }

        [Test]
        public async Task S1_42_KiemTraApiDangNhap_KhoaTaiKhoanDB()
        {
            _currentTestCaseId = "S.1.42";

            GetApiTestDataFromExcel(_currentTestCaseId, out string url, out string payload);

            using (HttpClient client = new HttpClient())
            {
                string lastResponseBody = "";
                int lastStatusCode = 0;

                TestContext.WriteLine($"[API S.1.42] Đang giả lập Spam đăng nhập sai liên tục bằng dữ liệu từ Excel...");

                for (int i = 1; i <= 6; i++)
                {
                    var content = new StringContent(payload, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);

                    lastStatusCode = (int)response.StatusCode;
                    lastResponseBody = await response.Content.ReadAsStringAsync();

                    await Task.Delay(300);
                }

                TestContext.WriteLine($"[API RESPONSE LẦN 6] Status: {lastStatusCode} | Body: {lastResponseBody}");

                string responseLower = lastResponseBody.ToLower();
                bool isLocked = responseLower.Contains("khóa") || responseLower.Contains("locked") || responseLower.Contains("quá số lần") || lastStatusCode == 403 || lastStatusCode == 423;

                Assert.That(isLocked, Is.True, "Lỗi tích hợp: API không trả về trạng thái Khóa tài khoản sau 5 lần nhập sai. Quá trình UPDATE xuống DB có thể đã thất bại.");

                _actualResult = "Tích hợp API và DB thành công. Lệnh UPDATE xuống DB đã chạy. Trả về thông báo bị khóa chuẩn xác (Dữ liệu bốc từ Excel).";
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
                    string resultText = "LỖI: " + (string.IsNullOrEmpty(errorMessage) ? "Gặp lỗi không xác định" : errorMessage);
                    _excelManager.WriteResult(testCaseId, "FAIL", resultText, testMethodName, "");
                }
                else
                {
                    string resultText = string.IsNullOrEmpty(_actualResult)
                        ? "Kiểm tra Integration API thành công."
                        : _actualResult;

                    _excelManager.WriteResult(testCaseId, "PASS", resultText, testMethodName, "");
                }
            }
            _actualResult = "";
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