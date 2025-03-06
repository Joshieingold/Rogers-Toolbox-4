using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Windows;
using Toolbox_Class_Library.Properties;


namespace Toolbox_Class_Library.CtrUpdate
{
    public class CtrUpdate
    {
        public List<CTR> AllCtrs { get; set; } = new();

        public CtrUpdate()
        {
            // Deserialize contractor data from JSON
            string jsonData = Settings.Default.ContractorData;
            Console.WriteLine("\n");
            Console.WriteLine(jsonData);
            Console.WriteLine("\n");
            List<ContractorCategory> categories = string.IsNullOrWhiteSpace(jsonData)
                ? new List<ContractorCategory>()
                : JsonSerializer.Deserialize<List<ContractorCategory>>(jsonData) ?? new List<ContractorCategory>();

            // Split the stored CTR order into a list of IDs
            string[] allCtrStringList = Settings.Default.CtrOrder.Split(", ");

            foreach (string ctrString in allCtrStringList)
            {
                var matchingCategory = categories.FirstOrDefault(category => category.CtrIDs.Contains(ctrString));

                if (matchingCategory != null)
                {
                    

                    CTR newCtr = new CTR(ctrString, matchingCategory.Devices, string.Join(", ", Settings.Default.GroupedDevices));
                    AllCtrs.Add(newCtr);
                }
            }
        }
        public void Test()
        {
            foreach (CTR ctr in AllCtrs)
            {
                Console.Write(ctr.ToString());
            }
            
        }
        private void CombineExcels()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select Excel Files to Combine",
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var filePaths = openFileDialog.FileNames;
                try
                {
                    CombineExcelFiles(filePaths);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Failed to combine files: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        } // Opens file dialog asking for paths to multiple excel files.
        private void CombineExcelFiles(string[] filePaths)
        {
            // creates a new workbook
            var combinedWorkbook = new XLWorkbook();
            var combinedWorksheet = combinedWorkbook.Worksheets.Add("Combined");

            int currentRow = 1;

            foreach (var filePath in filePaths)
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1); // Use the first worksheet
                    var rows = worksheet.RowsUsed();
                    // gets all data from all rows and columns of selected files and adds them to the combined sheet.
                    foreach (var row in rows)
                    {
                        for (int col = 1; col <= row.LastCellUsed().Address.ColumnNumber; col++)
                        {
                            combinedWorksheet.Cell(currentRow, col).Value = row.Cell(col).Value;
                        }
                        currentRow++;
                    }
                }
            }

            SaveCombinedExcelFile(combinedWorkbook);
        } // Takes the paths to excel files and combines them into one excel file.
        private void SaveCombinedExcelFile(XLWorkbook combinedWorkbook)
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Save Combined Excel File",
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = $"ContractorData - {date}.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                combinedWorkbook.SaveAs(saveFileDialog.FileName);
            }
        } // Allows the user to save the combined file somewhere on their pc


        private void ProcessCTRUpdate()
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Select Excel File for CTR Update",
                    Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string workbookPath = openFileDialog.FileName;

                    try
                    {
                        using (var workbookInstance = new XLWorkbook(workbookPath))
                        {
                            var sheet = workbookInstance.Worksheet(1); // Process the first sheet
                            Stopwatch stopwatch = new Stopwatch();
                            stopwatch.Start();
                            UpdateCTRS(sheet);
                            stopwatch.Stop();
                            TimeSpan ts = stopwatch.Elapsed;
                            string elapsedTime = String.Format("{0:00}h : {1:00}m : {2:00}s : {3:00} ms",
                            ts.Hours, ts.Minutes, ts.Seconds,
                            ts.Milliseconds / 10);
                            Console.WriteLine($"Ctr update completed in: {elapsedTime}");
                    }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Failed to process CTR update: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            } // executes the main process by grabbing all neccesary data.
        private void UpdateCTRS(IXLWorksheet sheet)
            {
                string[] UpdateList = Settings.Default.CtrOrder.Split(", ");
                foreach (var row in sheet.RowsUsed())
                {
                    var RowCtrID = row.Cell(8).GetValue<string>();
                    var RowInventoryType = row.Cell(10).GetValue<string>();
                    var RowWarehouseCtrID = row.Cell(2).GetValue<string>();

                    if (UpdateList.Contains(RowCtrID) || UpdateList.Contains(RowWarehouseCtrID))
                    {
                        var matchedCtr = this.AllCtrs.FirstOrDefault(ctr => ctr.Name == RowCtrID || ctr.Name == RowWarehouseCtrID);
                        if (matchedCtr != null)
                        {
                            if (RowInventoryType.StartsWith("CTR.Subready."))
                            {
                                var RowDeviceCode = row.Cell(6).GetValue<string>();
                                matchedCtr.DevicePlusCounter(RowDeviceCode);
                            }
                            if (UpdateList.Contains(RowWarehouseCtrID))
                            {
                                var RowDeviceCode = row.Cell(6).GetValue<string>();
                                matchedCtr.DevicePlusCounter(RowDeviceCode);
                            }

                        }
                    }

                }
            } 
        
        
        public void Run()
        {
            // Should how an Empty set of CTRS
            Test();
            // Gets the user to combine x excels and creates a single file.
            CombineExcels();
            // Gets the user to select the file they want to analyze and analyzes it.
            ProcessCTRUpdate();
            // Should show a filled out set of CTRS
            Test();

        }
    }
}