using System;
using System.IO;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;

namespace test_etesthub.Helpers
{
    public class ExcelReportManager
    {
        private readonly string _filePath;
        private ExcelPackage _excelPackage;
        private ExcelWorksheet _worksheet;

        private const int Col_ID = 3;             // Cột C
        private const int Col_Steps = 7;          // Cột G
        private const int Col_TestData = 8;       // Cột H
        private const int Col_ResultStatus = 12;  // Cột K
        private const int Col_Screenshot = 14;    // Cột M

        public ExcelReportManager(string excelFileName)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            _filePath = excelFileName.Contains(":") ? excelFileName : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, excelFileName);

            if (!File.Exists(_filePath))
            {
                throw new FileNotFoundException($"Không tìm thấy file excel: {_filePath}");
            }
        }

        public void OpenReport()
        {
            _excelPackage = new ExcelPackage(new FileInfo(_filePath));
            _worksheet = _excelPackage.Workbook.Worksheets["II.1_DangNhap"];
        }

        private string GetCellText(int row, int col)
        {
            var cellText = _worksheet.Cells[row, col].Text;
            return string.IsNullOrEmpty(cellText) ? "" : cellText.Trim();
        }

        // 1. HÀM GOM DATA NHIỀU DÒNG CHO 1 TEST CASE
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

        // 2. HÀM GHI KẾT QUẢ VÀ DÁN ẢNH LỖI
        public void WriteResult(string testCaseId, string status, string screenshotPath)
        {
            try
            {
                for (int row = 1; row <= 200; row++)
                {
                    string cellId = GetCellText(row, Col_ID).Replace(" ", "").ToUpper();
                    string targetId = testCaseId.Replace(" ", "").ToUpper();

                    if (cellId == targetId)
                    {
                        var statusCell = _worksheet.Cells[row, Col_ResultStatus];
                        statusCell.Value = status;

                        if (status == "PASS")
                        {
                            statusCell.Style.Font.Color.SetColor(Color.Green);
                            Console.WriteLine($"[THÀNH CÔNG] Đã ghi PASS cho {testCaseId} vào dòng {row}");
                        }
                        else
                        {
                            statusCell.Style.Font.Color.SetColor(Color.Red);
                            Console.WriteLine($"[THÀNH CÔNG] Đã ghi FAIL và dán ảnh cho {testCaseId} vào dòng {row}");

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