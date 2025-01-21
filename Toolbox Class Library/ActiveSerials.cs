using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;
using WindowsInput;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
namespace RogersToolbox
{
    public class ActiveSerials
    {
        // Initialization
        public List<SerialNumber> Serials { get; set; }
        private InputSimulator inputSimulator = new InputSimulator();
        private int typingSpeed { get; set;}
        private int blitzImportSpeed { get; set; }
        private bool reverseImport { get; set; }
        private int flexiproImportSpeed { get; set; }

        public ActiveSerials(string serialString) 
        {
            Serials = new List<SerialNumber>(); 
            string[] betweenList = serialString.Split("\n");

            foreach (string rawSerial in betweenList)
            {
                Serials.Add(new SerialNumber(rawSerial.Trim())); 
            }
            typingSpeed = Toolbox_Class_Library.Properties.Settings.Default.TypingSpeed;
            blitzImportSpeed = Toolbox_Class_Library.Properties.Settings.Default.BlitzImportSpeed;
            flexiproImportSpeed = Toolbox_Class_Library.Properties.Settings.Default.FlexiProImportSpeed;
        }
        //  Helper Functions
        private async Task SimulateTyping(string text)
        {

            foreach (char c in text)
            {
                inputSimulator.Keyboard.TextEntry(c);  
                await Task.Delay(typingSpeed);  
            }
        }
        private void SimulateTabKey()
        {
            inputSimulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);
        }
        public string GetRemainingSerials()
        {
            return string.Join(Environment.NewLine, Serials.Select(s => s.Serial));
        }
        // Button Click Functions
        public async Task BlitzImport()
        {
            
            var serialsToProcess = new List<SerialNumber>(Serials);

            foreach (SerialNumber serial in serialsToProcess)
            {
                if (serial.Serial == "*")
                {
                    Serials.Remove(serial);
                    break;
                }
                else
                {
                    await SimulateTyping(serial.Serial);

                    // Remove the serial from the original list after processing
                    Serials.Remove(serial);

                    SimulateTabKey(); // Simulate pressing the Tab key or any other key as needed
                    await Task.Delay(blitzImportSpeed); // Delay between serials
                }
                
            }
        }
        private string GetSerialsFromExcel(string filePath)
        {
            try
            {
                Serials = new List<SerialNumber>();
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1); // Load the first sheet

                    // Iterate through rows in the first column and get their values.
                    foreach (var row in worksheet.RowsUsed())
                    {
                        var cellValue = row.Cell(1).GetValue<string>();
                        if (!string.IsNullOrWhiteSpace(cellValue))
                        {
                            Serials.Add(new SerialNumber(cellValue.Trim()));
                        }
                    }
                }
                return string.Join(Environment.NewLine, Serials.Select(s => s.Serial));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to load serials: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return "Error";
            }
        } // Gets all data form the first column of the loaded excel file/
        public string ReverseSerials(string input)
        {
            // Split the input string by newlines
            string[] lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // Reverse the array of lines
            Array.Reverse(lines);

            // Join the reversed lines back into a single string with newlines
            return string.Join(Environment.NewLine, lines);
        }
        public string OpenExcel()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Open an Excel file for use",
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (Toolbox_Class_Library.Properties.Settings.Default.ReverseImport == true)
                {
                    return ReverseSerials(GetSerialsFromExcel(openFileDialog.FileName));
                }
                else 
                {
                   return GetSerialsFromExcel(openFileDialog.FileName); 
                }
            }
            else
            {
                return string.Empty;
            }
        } // Establishes a path to the target excel for importing serials.
        public async Task FlexiImport(ActiveSerials SerialList)
        {
            try 
            {
                string CurrentDevice = (SerialList.Serials[0]).Device;
                int Quantity = (SerialList.Serials).Count;
                DateTime i = DateTime.Now;
                DateTime utcDateTime = i.ToUniversalTime();

                foreach (SerialNumber Serial in SerialList.Serials)
                {

                }
            }
            catch
            {
                Console.WriteLine("There was an Error.");
            }
            

        }
    }
}