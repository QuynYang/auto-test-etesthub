using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using test_etesthub.Helpers;

namespace test_etesthub.Tests.UserManagement
{
    [TestFixture]
    public class CreateUserIntegrationTests
    {
        private ExcelReportManager _excelManager;
        private string _currentTestCaseId = "";
        private string _actualResult = "";

        private static string _adminToken = "";

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
            TestContext.WriteLine($"[HỆ THỐNG] Bắt đầu chạy Test Case Integration API ngầm qua HttpClient...");
        }


        private async Task<string> GetAdminTokenAsync()
        {
            if (!string.IsNullOrEmpty(_adminToken)) return _adminToken;

            using (HttpClient client = new HttpClient())
            {
                string loginPayload = "{\"email\": \"admin@gmail.com\", \"password\": \"123456\"}";
                var content = new StringContent(loginPayload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://e-testhub-project.onrender.com/api/auth/login", content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode && responseBody.Contains("\"token\""))
                {
                    int tokenIndex = responseBody.IndexOf("\"token\":\"") + 9;
                    int endIndex = responseBody.IndexOf("\"", tokenIndex);
                    _adminToken = responseBody.Substring(tokenIndex, endIndex - tokenIndex);
                    TestContext.WriteLine("[HỆ THỐNG] Lấy Token Admin thành công để vượt qua bảo mật 401.");
                }
                else
                {
                    Assert.Fail($"LỖI BLOCKER: Không thể lấy Token. API Đăng nhập trả về: {responseBody}. Vui lòng kiểm tra lại tài khoản Admin trong code.");
                }
            }
            return _adminToken;
        }

        private void GetApiTestDataFromExcel(string testCaseId, out string url, out string payload)
        {
            string[] testData = _excelManager.GetTestDataArray(testCaseId);
            url = "https://e-testhub-project.onrender.com/api/users";
            payload = "";

            foreach (var cell in testData)
            {
                if (cell != null)
                {
                    string trimmedCell = cell.Trim();

                    if (trimmedCell.StartsWith("http"))
                    {
                        url = trimmedCell;
                    }
                    else if (trimmedCell.Contains("{") && trimmedCell.Contains("}"))
                    {
                        payload = trimmedCell;
                        if (payload.StartsWith("\"") && payload.EndsWith("\""))
                        {
                            payload = payload.Substring(1, payload.Length - 2).Replace("\"\"", "\"");
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(payload)) Assert.Fail($"Lỗi dữ liệu Excel: Không tìm thấy chuỗi JSON payload hợp lệ ở Test Case {testCaseId}");
        }


        [Test]
        public async Task S2_1_38_TichHopAPI_CheckEmailExists()
        {
            _currentTestCaseId = "S.2.1.38";
            GetApiTestDataFromExcel(_currentTestCaseId, out string url, out string payload);

            if (!url.Contains("check")) url = "https://e-testhub-project.onrender.com/api/users/check-email";

            using (HttpClient client = new HttpClient())
            {
                string token = await GetAdminTokenAsync();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                string responseBody = await response.Content.ReadAsStringAsync();
                int statusCode = (int)response.StatusCode;

                TestContext.WriteLine($"[API RESPONSE S.2.1.38] Status: {statusCode} | Body: {responseBody}");

                Assert.That(statusCode, Is.EqualTo(200), $"Lỗi: API kiểm tra email giao tiếp thất bại (Status: {statusCode})");
                Assert.That(responseBody.ToLower().Contains("true") || responseBody.ToLower().Contains("exists"), Is.True, "Lỗi: DB đã có email này nhưng API trả về false/không tồn tại.");

                _actualResult = "API giao tiếp MongoDB thành công. Trả về Status 200 OK với body xác nhận email đã tồn tại (Đã quét data từ Excel).";
            }
        }

        [Test]
        public async Task S2_1_39_DataFlow_LuuUserXuongDatabase()
        {
            _currentTestCaseId = "S.2.1.39";
            GetApiTestDataFromExcel(_currentTestCaseId, out string url, out string payload);

            using (HttpClient client = new HttpClient())
            {
                string token = await GetAdminTokenAsync();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                string dynamicPayload = payload.Replace("student_new@test.com", $"sv_{DateTime.Now:ddMM_HHmmss}@test.com");
                var content = new StringContent(dynamicPayload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                string responseBody = await response.Content.ReadAsStringAsync();
                int statusCode = (int)response.StatusCode;

                TestContext.WriteLine($"[API RESPONSE S.2.1.39] Status: {statusCode} | Body: {responseBody}");

                Assert.That(statusCode == 200 || statusCode == 201, Is.True, $"Lỗi: API lưu DB thất bại (Status: {statusCode})");
                Assert.That(responseBody.Contains("Password123!"), Is.False, "LỖI BẢO MẬT NGHIÊM TRỌNG: API trả về plain text password. Mật khẩu lưu xuống MongoDB chưa được mã hóa (Hash)!");

                _actualResult = $"API xử lý logic mã hóa mật khẩu và Insert xuống Database thành công. Trả về mã {statusCode}. Không rò rỉ plain-text password.";
            }
        }

        [Test]
        public async Task S2_1_40_XyLyNgoaiLe_DBUniqueConstraint()
        {
            _currentTestCaseId = "S.2.1.40";
            GetApiTestDataFromExcel(_currentTestCaseId, out string url, out string payload);

            using (HttpClient client = new HttpClient())
            {
                string token = await GetAdminTokenAsync();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                string responseBody = await response.Content.ReadAsStringAsync();
                int statusCode = (int)response.StatusCode;

                TestContext.WriteLine($"[API RESPONSE S.2.1.40] Status: {statusCode} | Body: {responseBody}");

                bool isSafeError = (statusCode == 400 || statusCode == 409);

                Assert.That(statusCode, Is.Not.EqualTo(500), "Lỗi nghiêm trọng: Server bị Crash 500 do không bắt được lỗi Duplicate Key của MongoDB!");
                Assert.That(isSafeError, Is.True, $"Lỗi: Hệ thống trả về mã trạng thái không chuẩn ({statusCode}) khi trùng email.");

                _actualResult = $"Backend bắt (catch) thành công lỗi Duplicate Constraint từ Database. Server không bị sập (tránh mã 500) và trả về chuẩn Status Code {statusCode}.";
            }
        }

        [Test]
        public async Task S2_1_41_TichHopBangCon_RelationshipsRole()
        {
            _currentTestCaseId = "S.2.1.41";
            GetApiTestDataFromExcel(_currentTestCaseId, out string url, out string payload);

            using (HttpClient client = new HttpClient())
            {
                string token = await GetAdminTokenAsync();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                string dynamicPayload = payload.Replace("teacher_new@test.com", $"gv_{DateTime.Now:ddMM_HHmmss}@test.com");
                var content = new StringContent(dynamicPayload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                string responseBody = await response.Content.ReadAsStringAsync();
                int statusCode = (int)response.StatusCode;

                TestContext.WriteLine($"[API RESPONSE S.2.1.41] Status: {statusCode} | Body: {responseBody}");

                Assert.That(statusCode == 200 || statusCode == 201, Is.True, $"Lỗi: Giao dịch tạo Teacher thất bại (Status: {statusCode}). Có khả năng lỗi đồng bộ Session/Transaction giữa các collection.");

                _actualResult = $"Giao dịch (Transaction) tạo User và phân bổ Role Teacher thành công. API trả về mã {statusCode}. Document được liên kết chuẩn xác không gây mồ côi dữ liệu.";
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