using System;
using System.IO;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using System.Linq;

namespace test_etesthub.Helpers
{
    public class ExcelReportManager
    {
        private readonly string _filePath;
        private ExcelPackage _excelPackage;
        private ExcelWorksheet _worksheet;
        private readonly string _sheetName;

        private const int Col_ID = 3;             // Cột C
        private const int Col_Steps = 7;          // Cột G
        private const int Col_TestData = 8;       // Cột H
        private const int Col_ActualResult = 10;  // Cột J: Result (Kết quả thực tế)
        private const int Col_TestScript = 11;    // Cột K: Test Scripts (Tên hàm)
        private const int Col_ResultStatus = 12;  // Cột K
        private const int Col_Screenshot = 14;    // Cột M

        public ExcelReportManager(string excelFileName, string sheetName = "II.1_DangNhap")
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            _filePath = excelFileName.Contains(":") ? excelFileName : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, excelFileName);
            _sheetName = sheetName;
            if (!File.Exists(_filePath))
            {
                throw new FileNotFoundException($"Không tìm thấy file excel: {_filePath}");
            }
        }

        public void OpenReport()
        {
            _excelPackage = new ExcelPackage(new FileInfo(_filePath));
            _worksheet = _excelPackage.Workbook.Worksheets[_sheetName];
        }

        private string GetCellText(int row, int col)
        {
            var cellText = _worksheet.Cells[row, col].Text;
            return string.IsNullOrEmpty(cellText) ? "" : cellText.Trim();
        }

        // HÀM GOM DATA NHIỀU DÒNG CHO 1 TEST CASE
        public string[] GetTestDataArray(string testCaseId)
        {
            var dataList = new System.Collections.Generic.List<string>();
            bool isFound = false;
            string targetId = testCaseId.Replace(" ", "").ToUpper();

            for (int row = 1; row <= 200; row++)
            {
                string currentId = GetCellText(row, Col_ID).Replace(" ", "").ToUpper();

                if (!string.IsNullOrEmpty(currentId))
                {
                    if (currentId == targetId)
                    {
                        isFound = true;
                    }
                    else if (isFound)
                    {
                        break;
                    }
                }

                if (isFound)
                {
                    string data = GetCellText(row, Col_TestData);
                    if (!string.IsNullOrEmpty(data) && data.ToUpper() != "N/A")
                    {
                        dataList.Add(data);
                    }
                }
            }
            return dataList.ToArray();
        }

        // HÀM GHI KẾT QUẢ VÀ DÁN ẢNH LỖI
        public void WriteResult(string testCaseId, string status, string actualResult, string testMethodName, string screenshotPath)
        {
            try
            {
                for (int row = 1; row <= 200; row++)
                {
                    string cellId = GetCellText(row, Col_ID).Replace(" ", "").ToUpper();
                    string targetId = testCaseId.Replace(" ", "").ToUpper();

                    if (cellId == targetId)
                    {
                        _worksheet.Cells[row, Col_ResultStatus].Value = status;
                        _worksheet.Cells[row, Col_ActualResult].Value = actualResult;
                        _worksheet.Cells[row, Col_TestScript].Value = testMethodName;

                        var statusCell = _worksheet.Cells[row, Col_ResultStatus];
                        var actualCell = _worksheet.Cells[row, Col_ActualResult];

                        for (int i = _worksheet.Drawings.Count - 1; i >= 0; i--)
                        {
                            var drawing = _worksheet.Drawings[i];
                            if (drawing.From.Row == row - 1 && drawing.From.Column == Col_Screenshot - 1)
                            {
                                _worksheet.Drawings.Remove(i);
                            }
                        }

                        if (status == "PASS")
                        {
                            statusCell.Style.Font.Color.SetColor(Color.Green);
                            actualCell.Style.Font.Color.SetColor(Color.Green);

                            _worksheet.Row(row).Height = 15;
                        }
                        else
                        {
                            statusCell.Style.Font.Color.SetColor(Color.Red);
                            actualCell.Style.Font.Color.SetColor(Color.Red);

                            if (!string.IsNullOrEmpty(screenshotPath) && File.Exists(screenshotPath))
                            {
                                string picName = $"Error_{testCaseId}_{Guid.NewGuid().ToString().Substring(0, 5)}";
                                ExcelPicture pic = _worksheet.Drawings.AddPicture(picName, new FileInfo(screenshotPath));

                                pic.SetPosition(row - 1, 2, Col_Screenshot - 1, 2);
                                pic.SetSize(120, 80);
                                _worksheet.Row(row).Height = 65; 
                            }
                        }

                        _excelPackage.Save();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI GHI EXCEL TẠI {testCaseId}]: " + ex.Message);
            }
        }

        public void CloseReport()
        {
            if (_excelPackage != null)
            {
                _excelPackage.Save();
                _excelPackage.Dispose();
            }
        }
    }
}