using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Globalization;

namespace Bend_PSA.Utils
{
    public static class Files
    {
        private static readonly object _lock = new();
        private static readonly object _lockExcel = new();
        private static readonly object _lockCSV = new();

        private static readonly string PathExcel = @"D:\Data_Bend_PSA";

        public static string GetFilePathSetting()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "setting.txt");
        }

        public static void WriteFileToTxt(string filePath, Dictionary<string, string> values)
        {
            lock (_lock)
            {
                try
                {
                    var lines = File.ReadAllLines(filePath).ToList();
                    var keysToUpdate = values.Keys.ToList();

                    var updatedKeys = new HashSet<string>();

                    for (int i = 0; i < lines.Count; i++)
                    {
                        var parts = lines[i].Split(['='], 2);
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim();
                            if (values.TryGetValue(key, out string? value))
                            {
                                lines[i] = $"{key}= {value}";
                                updatedKeys.Add(key);
                            }
                        }
                    }

                    foreach (var key in keysToUpdate)
                    {
                        if (!updatedKeys.Contains(key))
                        {
                            lines.Add($"{key}= {values[key]}");
                        }
                    }

                    File.WriteAllLines(filePath, lines);
                }
                catch (Exception ex)
                {
                    Logs.Log($"Error can not write value to file txt, errror: {ex.Message}");
                }
            }
        }

        public static Dictionary<string, string> ReadValueFileTxt(string filePath, List<string> keys)
        {
            Dictionary<string, string> values = [];

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    string[] parts = line.Split('=');

                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();

                        if (keys.Contains(key))
                        {
                            values[key] = parts[1].Trim();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Log($"Error can not read value from file txt, error: {ex.Message}");
            }

            return values;
        }

        public static string? GetUniqueFilePath(string? filePath)
        {
            try
            {
                string? directory = Path.GetDirectoryName(filePath) ?? @"D:\";
                string? fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                string? extension = Path.GetExtension(filePath);
                int count = 1;

                while (File.Exists(filePath))
                {
                    string newFileName = string.Empty;
                    newFileName = $"{fileNameWithoutExtension}_({count}){extension}";
                    filePath = Path.Combine(directory, newFileName);
                    count++;
                }

                return filePath;
            }
            catch (Exception ex)
            {
                Logs.Log($"Error can not get unique file path, error: {ex.Message}");
                return filePath;
            }
        }

        public static void ExportExcel()
        {
            string dateString = DateTime.Now.ToString("yyyy-MM-dd");

            lock (_lockExcel)
            {
                try
                {
                    if (!Directory.Exists(PathExcel))
                    {
                        Directory.CreateDirectory(PathExcel);
                    }

                    string fileName = $"{dateString}.xlsx";
                    string filePath = Path.Combine(PathExcel, "Export_Excels", fileName);

                    using ExcelPackage package = new ExcelPackage(new FileInfo(filePath));
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Count == 0 ? package.Workbook.Worksheets.Add("Sheet1") : package.Workbook.Worksheets[0];

                    int row = worksheet.Dimension?.Rows + 1 ?? 1;

                    if (row == 1)
                    {
                        worksheet.Cells["A1:V1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells["A1:V1"].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                        worksheet.Cells["A1:B1"].Style.WrapText = true;

                        worksheet.Column(1).Width = 25;
                        worksheet.Column(2).Width = 20;

                        worksheet.Cells[1, 5].Value = "Data_1";
                        worksheet.Cells[1, 6].Value = "Data_2";
                    }

                    if (row == 1)
                    {
                        row += 1;
                    }

                    worksheet.Cells[row, 1].Value = string.Empty;
                    worksheet.Cells[row, 2].Value = string.Empty;
                    worksheet.Cells[row, 3].Value = string.Empty;
                    worksheet.Cells[row, 4].Value = string.Empty;

                    FileInfo fileInfo = new(filePath);
                    package.SaveAs(fileInfo);
                }
                catch (Exception ex)
                {
                    Logs.Log($"Error can not save file excel, error: {ex.Message}");
                }
            }
        }

        public static void SaveCSV()
        {
            //lock (_lockCSV)
            //{
            //    string fileNameCSV = "VNATHSSEM240701-" + (!string.IsNullOrWhiteSpace(rs.BucCoverQR) ? rs.BucCoverQR.Trim() : "Machine_testing") + ".csv";

            //    //check if NAS not exist
            //    if (!Directory.Exists(directoryPath) && type == 2)
            //    {
            //        Global.WriteLog("Not exist file path NAS!");
            //        Global.ShowError("Not exist file path NAS!");
            //        return;
            //    }

            //    string filePath = Path.Combine(directoryPath, fileNameCSV);
            //    fileNameCSV = GetUniqueFilePath(filePath);

            //    try
            //    {
            //        using (var writer = new StreamWriter(fileNameCSV))
            //        using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            //        {
            //            csv.WriteHeader<Result>();
            //            csv.NextRecord();

            //            if (rs.Rs == "1")
            //            {
            //                rs.Rs = "OK";
            //            }
            //            else
            //            {
            //                rs.Rs = "NG";
            //            }

            //            csv.WriteRecord(rs);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Global.WriteLog($"Can not save to file CSV: {ex.Message}");
            //        Global.ShowError($"Can not save to file CSV: {ex.Message}");
            //    }
            //}
        }
    }
}
