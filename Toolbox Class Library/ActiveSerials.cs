using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;
using WindowsInput;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using Toolbox_Class_Library;
using System.Drawing;
using System.Windows.Forms;
using System.Printing;
using System.Linq.Expressions;
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
        private int wmsImportSpeed { get; set; }
        private string flexiProCheckPixel { get; set; }
        private string wmsCheckPixel { get; set; }
        private string wmsFailString { get; set; }
        private string user { get; set; }
        private int excelColumn { get; set; }
        public List<string> passList { get; set; }
        public List<string> failList { get; set; }
        private bool pushFlexiProData { get; set; }
        private readonly Action? serialsUpdatedCallback; // Callback for UI update


        public ActiveSerials(string serialString, Action? serialsUpdatedCallback = null)
        {
            Serials = new List<SerialNumber>();
            string[] betweenList = serialString.Split("\n");
            passList = [];
            failList = [];

            foreach (string rawSerial in betweenList)
            {
                Serials.Add(new SerialNumber(rawSerial.Trim()));
            }
            // Data from Settings
            typingSpeed = Toolbox_Class_Library.Properties.Settings.Default.TypingSpeed;
            blitzImportSpeed = Toolbox_Class_Library.Properties.Settings.Default.BlitzImportSpeed;
            flexiproImportSpeed = Toolbox_Class_Library.Properties.Settings.Default.FlexiProImportSpeed;
            flexiProCheckPixel = Toolbox_Class_Library.Properties.Settings.Default.FlexiproPixel;
            wmsCheckPixel = Toolbox_Class_Library.Properties.Settings.Default.WmsPixel;
            user = Toolbox_Class_Library.Properties.Settings.Default.Username;
            wmsFailString = Toolbox_Class_Library.Properties.Settings.Default.WmsFailAutomation;
            wmsImportSpeed = Toolbox_Class_Library.Properties.Settings.Default.WmsImportSpeed;
            excelColumn = Toolbox_Class_Library.Properties.Settings.Default.ExcelColumn;
            pushFlexiProData = Toolbox_Class_Library.Properties.Settings.Default.PushFlexiProData;
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
        public string ReverseSerials(string input)
        {
            // Split the input string by newlines
            string[] lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // Reverse the array of lines
            Array.Reverse(lines);

            // Join the reversed lines back into a single string with newlines
            return string.Join(Environment.NewLine, lines);
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
                        var cellValue = row.Cell(excelColumn).GetValue<string>();
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
        private void WmsFailAutomation(string[] failSplit)
        {
            var sim = new InputSimulator();
            if (failSplit[1] == "X")
            {
                sim.Keyboard.ModifiedKeyStroke(WindowsInput.Native.VirtualKeyCode.CONTROL, WindowsInput.Native.VirtualKeyCode.VK_X);
            }
            else if (failSplit[1] == "A")
            {
                sim.Keyboard.ModifiedKeyStroke(WindowsInput.Native.VirtualKeyCode.CONTROL, WindowsInput.Native.VirtualKeyCode.VK_A);
            }
            else
            {
                Console.Write("Unhandled Key");
            }

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
            var serialsToProcess = new List<SerialNumber>(Serials); // Create a copy of the serials to loop through
            List<string> shadowSerials = new List<string>(); // Create a list to push to database at the end
            string device = Serials[0].Device;
            DateTime i = DateTime.Now;
            DateTime utcDateTime = i.ToUniversalTime();
            DatabaseConnection FlexiProConnection = new DatabaseConnection();
            foreach (SerialNumber copySerial in serialsToProcess)
            {
                try
                {

                    bool isPixelGood = CheckPixel("(250, 250, 250)", GetCurrentPixel(flexiProCheckPixel)); // Should be 250s!
                    while (isPixelGood == false)
                    {
                        await Task.Delay(flexiproImportSpeed);
                        isPixelGood = CheckPixel("(250, 250, 250)", GetCurrentPixel(flexiProCheckPixel)); // Should be 250s!
                        Console.WriteLine("Pixel Failed, awaiting retry");
                    }
                    if (isPixelGood == true)
                    {
                        if (copySerial.Serial == "*")
                        {
                            Serials.Remove(copySerial);
                            Console.WriteLine("Serial validated as a break. Ending Import");
                            break;
                        }
                        else if (copySerial.Serial == "")
                        {
                            Console.WriteLine("Serial is null and is being passed.");
                        }
                        else
                        {
                            Console.WriteLine("Serial is validated and being imported.");
                            await SimulateTyping(copySerial.Serial);
                            shadowSerials.Add(copySerial.Serial); // Add to shadow list for database push at the end
                            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                            {
                                Serials.Remove(copySerial);
                            });
                            await Task.Delay(100); // NEW MAYBE THIS IS THE ISSUE?
                            SimulateTabKey();
                            await Task.Delay(flexiproImportSpeed);

                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle the exception (e.g., log it, show a message box, etc.)
                    await FlexiProConnection.PushDeviceData(device, shadowSerials.Count(), utcDateTime, user, shadowSerials);
                    System.Windows.MessageBox.Show($"Error during FlexiPro import: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                }
                try
                {
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        serialsUpdatedCallback?.Invoke(); // Notify UI to update
                    });
                }
                catch
                {
                    await FlexiProConnection.PushDeviceData(device, shadowSerials.Count(), utcDateTime, user, shadowSerials);
                    System.Windows.MessageBox.Show("Error updating UI", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                }
            }
            if (pushFlexiProData)
            {
                Console.WriteLine($"Date: {utcDateTime}\nDevice: {device}\nCount: {shadowSerials.Count()}\nUser: {user}");
                await FlexiProConnection.PushDeviceData(device, shadowSerials.Count(), utcDateTime, user, shadowSerials);
            }
            else
            {
                Console.WriteLine("Push FlexiPro Data is disabled, Data will be directed to serials Database.");
                Console.WriteLine($"Date: {utcDateTime}\nDevice: {device}\nCount: {shadowSerials.Count()}\nUser: {user}");
                await FlexiProConnection.PushSerialsData(device, shadowSerials.Count(), utcDateTime, user, shadowSerials);
            }

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
        } // Establishes a path to the target excel for importing serials
        public async Task WmsImport()
        {
            var serialsToProcess = new List<SerialNumber>(Serials);
            string[] failAutomationSplit = wmsFailString.Split(" + ");
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
                    await Task.Delay(200);
                    SimulateTabKey();
                    await Task.Delay(1000);
                    bool isPixelGood = CheckPixel("(0, 0, 0)", GetCurrentPixel(wmsCheckPixel));
                    if (isPixelGood == false)
                    {
                        failList.Add(serial.Serial);
                        WmsFailAutomation(failAutomationSplit);
                    }
                    else
                    {
                        passList.Add(serial.Serial);
                    }

                    await Task.Delay(4 / 2);
                    serialsUpdatedCallback?.Invoke(); // Notify UI to update
                }
                await Task.Delay(wmsImportSpeed);
            }
            // Create new Failed and passed list window with the aforementioned data.
            
        }

    }
}

