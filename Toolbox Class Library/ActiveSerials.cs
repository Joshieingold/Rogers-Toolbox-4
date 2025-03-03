using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;
using WindowsInput;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using Toolbox_Class_Library;
using System.Drawing;
using System.Windows.Forms;
namespace RogersToolbox
{
    public class ActiveSerials
    {
        // Initialization
        public List<SerialNumber> Serials { get; set; }
        private InputSimulator inputSimulator = new InputSimulator();
        private int typingSpeed { get; set; }
        private int blitzImportSpeed { get; set; }
        private bool reverseImport { get; set; }
        private int flexiproImportSpeed { get; set; }
        private string flexiProCheckPixel { get; set; }
        private string wmsCheckPixel { get; set; }
        private readonly Action? serialsUpdatedCallback; // Callback for UI update


        public ActiveSerials(string serialString, Action? serialsUpdatedCallback = null)
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
            flexiProCheckPixel = Toolbox_Class_Library.Properties.Settings.Default.FlexiproPixel;
            wmsCheckPixel = Toolbox_Class_Library.Properties.Settings.Default.WmsPixel;

            this.serialsUpdatedCallback = serialsUpdatedCallback;
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
        public string ConvertSerialsToString()
        {

            return string.Join("\n", this.Serials.Select(s => s.Serial));

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
                    Serials.Remove(serial);
                    SimulateTabKey();
                    await Task.Delay(blitzImportSpeed);

                    serialsUpdatedCallback?.Invoke(); // Notify UI to update
                }
            }

        }
        public async Task FlexiProImport()
        {
            var serialsToProcess = new List<SerialNumber>(Serials);
            int count = 0;
            string device = Serials[0].Device;
            DateTime i = DateTime.Now;
            DateTime utcDateTime = i.ToUniversalTime();
            DatabaseConnection FlexiProConnection = new DatabaseConnection("Push");
            foreach (SerialNumber serial in serialsToProcess)
            {
                bool isPixelGood = CheckPixel("(250, 250, 250)", GetCurrentPixel(flexiProCheckPixel));
                while (isPixelGood == false)
                {
                    await Task.Delay(700);
                    isPixelGood = CheckPixel("(250, 250, 250)", GetCurrentPixel(flexiProCheckPixel));
                }
                if (isPixelGood == true)
                {
                    if (serial.Serial == "*")
                    {
                        Serials.Remove(serial);
                        break;
                    }
                    else
                    {
                        await SimulateTyping(serial.Serial);
                        Serials.Remove(serial);
                        SimulateTabKey();
                        count += 1;



                        await Task.Delay(flexiproImportSpeed);

                        serialsUpdatedCallback?.Invoke(); // Notify UI to update
                    }
                }
                FlexiProConnection.PushDeviceData(device, count, utcDateTime);
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

        private bool CheckPixel(string colorWanted, string colorFound)
        {
            if (colorWanted == colorFound)
            {
                return true; // Returns True if they match
            }
            else
            {
                return false;
            }
        } // Checks between the color the programmer wants and the color found at the pixel on the screen stipulated.
        private string GetCurrentPixel(string pixelSource)
        {
            string[] cords = pixelSource.Split(", ");
            int xCord = Convert.ToInt32(cords[0]);
            int yCord = Convert.ToInt32(cords[1]);
            System.Drawing.Point ixelCords = new System.Drawing.Point(xCord, yCord);

            // Capture the screen
            Bitmap screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (Graphics graphics = Graphics.FromImage(screenshot))
            {
                graphics.CopyFromScreen(new System.Drawing.Point(0, 0), new System.Drawing.Point(0, 0), screenshot.Size);
            }

            // Get the color of the pixel at the specified coordinates
            Color pixelColor = screenshot.GetPixel(xCord, yCord);

            // Format the color as "(R, G, B)"
            string colorFound = $"({pixelColor.R}, {pixelColor.G}, {pixelColor.B})";

            return colorFound;
        } // Finds the color of a pixel on the screen.
        }
    } 
