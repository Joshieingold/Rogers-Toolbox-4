using ClosedXML.Excel;
using RogersToolbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Rogers_Toolbox_UI
{
    public partial class FormatWindow : Window
    {
        private List<string> TempSerials;  // List to hold serials
        public string FormattedSerials { get; private set; } = string.Empty;

        // Constructor that accepts a string of serials (from TextBox in MainWindow)
        public FormatWindow(string textBoxSerials)
        {
            InitializeComponent();
            TempSerials = textBoxSerials.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();  // Split serials by new lines
        }

        private void ChunkSerials(int ChunkBy)
        {
            List<string> tempList = new List<string>();

            // Iterate over the TempSerials in chunks
            for (int i = 0; i < TempSerials.Count; i++)
            {
                // Add the current serial to the temp list
                tempList.Add(TempSerials[i]);

                // Check if we need to insert an empty line (after every ChunkBy serials)
                if ((i + 1) % ChunkBy == 0)
                {
                    tempList.Add(""); // Add an empty line
                }
            }

            // If the last chunk doesn't end on a multiple of ChunkBy, don't add an extra empty line
            if (tempList.Count > 0 && !string.IsNullOrEmpty(tempList.Last()))
            {
                tempList.Add(""); // Add one more empty line if needed
            }

            // Convert the list to a single string with newlines
            TempSerials = tempList;
        }

        private void RemoveDuplicates()
        {
            if (TempSerials == null || TempSerials.Count == 0)
            {
                MessageBox.Show("The serials list is empty.");
                return;
            }

            // Find duplicates
            var duplicates = TempSerials.GroupBy(s => s)
                                        .Where(g => g.Count() > 1)
                                        .Select(g => g.Key)
                                        .ToList();

            // Remove duplicates to get unique serials
            var uniqueSerials = TempSerials.Distinct().ToList();

            // Format the output
            string result = "Duplicates:\n" + string.Join("\n", duplicates) +
                            "\n\nUnique:\n" + string.Join("\n", uniqueSerials);

            // Send the formatted text back to the MainWindow's TextBox
            TempSerials = new List<string> { result };
            this.Close();
        }

        private void SerialsToLine(string delimiter)
        {
            // Ensure TempSerials is not null and has values
            if (TempSerials != null && TempSerials.Count > 0)
            {
                string copyText = string.Join(delimiter, TempSerials);

                // Ensure copyText is not empty before copying to clipboard
                if (!string.IsNullOrEmpty(copyText))
                {
                    System.Windows.Forms.Clipboard.SetText(copyText);
                }
            }
        }

        private void MakeCapital(object sender, RoutedEventArgs e)
        {
            // Convert all serials to uppercase
            TempSerials = TempSerials.Select(serial => serial.ToUpper()).ToList();
            
        }

        private string ReturnFormattedSerials()
        {
            return string.Join("\n", TempSerials);  // Join the list back to a formatted string
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            FormattedSerials = ReturnFormattedSerials(); // Ensure the formatted serials are set before closing
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Close the FormatWindow
        }

        private void SplitButton_Click(object sender, RoutedEventArgs e)
        {
            // Try to get the value from the ChunkSizeTextBox
            if (int.TryParse(ChunkSizeTextBox.Text, out int chunkSize) && chunkSize > 0)
            {
                // Call ChunkSerials with the chunk size entered by the user
                ChunkSerials(chunkSize);

                // Optionally, close the window after splitting serials
                this.Close();
            }
        }
        private void SerialsToLine_Click(object sender, RoutedEventArgs e)
        {
            if ((DelimiterTextBox.Text).Length > 0 )
            {
                SerialsToLine(DelimiterTextBox.Text);
                this.Close();
            }
        }
        private void SerialsToLineCommaSpace_Click(object sender, RoutedEventArgs e)
        {

                SerialsToLine(", ");
                this.Close();
           

            
        }
        private void RemoveDuplicates_Click(object sender, RoutedEventArgs e)
        {
            RemoveDuplicates();
            this.Close();
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
                FileName = $"Combined Excel - {date}.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                combinedWorkbook.SaveAs(saveFileDialog.FileName);
            }
        } // Allows the user to save the combined file somewhere on their pc
        private void CombineExcel_Click(object sender, RoutedEventArgs e)
        {
            CombineExcels();
            this.Close();
        }
    }
}
