using RogersToolbox;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Management;

namespace Toolbox_Class_Library
{
    public class PrinterConnection
    {
        public ActiveSerials serialsToPrint { get; set; }
        private string bartenderPath { get; set; }
        private string targetDevice { get; set; }

        public PrinterConnection(ActiveSerials serials)
        {
            serialsToPrint = serials;
            bartenderPath = Toolbox_Class_Library.Properties.Settings.Default.BartenderPath;
            string device = (serials.Serials[0]).Device;
        }
        private string[] ConvertSerialsToArray(ActiveSerials serials)
        {
            string stringSerials = serials.ConvertSerialsToString();
            return stringSerials.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        }
        private void PurolatorPrint(ActiveSerials serials, int formatBy, string device)
        {
            
            string puroSheet = FormatSheet(formatBy, ConvertSerialsToArray(serialsToPrint), device);
            File.WriteAllText(bartenderPath, puroSheet + Environment.NewLine);
            

            string batchFile = device == "IPTVARXI6HD" || device == "IPTVTCXI6HD" || device == "SCXI11BEI"
                    ?
                  // Use Xi6 if the device is a cablebox.
                    @"@echo off
               set ""target_printer=55EXP_Purolator""
               powershell -Command ""Get-WmiObject -Query 'SELECT * FROM Win32_Printer WHERE ShareName=''%target_printer%'' ' | Invoke-WmiMethod -Name SetDefaultPrinter""
               ""C:\Seagull\BarTender 7.10\Standard\bartend.exe"" /f=C:\BTAutomation\XI6.btw /p /x"
                    :
                  // Use CODA file if the device is not a cablebox.  
                    @"@echo off
               set ""target_printer=55EXP_Purolator""
               powershell -Command ""Get-WmiObject -Query 'SELECT * FROM Win32_Printer WHERE ShareName=''%target_printer%'' ' | Invoke-WmiMethod -Name SetDefaultPrinter""
               ""C:\Seagull\BarTender 7.10\Standard\bartend.exe"" /f=C:\BTAutomation\CODA.btw /p /x";
            try
            {
                ExecuteBatchScript(batchFile);
            }
            catch
            {
                Console.Write("Print Failed");
            }
        }
        private int FindFormatByDevice(string device)
        {
            return (device == "IPTVARXI6HD" || device == "IPTVTCXI6HD" || device == "SCXI11BEI") ? 10 : 8;
        }

        public void ExecuteBatchScript(string scriptContent)
        {
            string tempFilePath = "temp_cmd.bat";

            // Write script content to a temporary file
            File.WriteAllText(tempFilePath, scriptContent);

            // Execute the batch file
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {tempFilePath}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            // Clean up temporary file
            File.Delete(tempFilePath);


        } // executes a cmd script given to it.

        public string FormatSheet(int numSplit, string[] serials, string device)
        {
            
            if (serials == null || serials.Length == 0)
            {
                return "No serials available.";
            }

            int totalStrings = serials.Length;
            StringBuilder formattedList = new StringBuilder();

            for (int i = 0; i < totalStrings; i += numSplit)
            {
                // Split into chunks and reverse each chunk
                List<string> chunk = serials.Skip(i).Take(numSplit).ToList();
                chunk.Reverse();

                // Example placeholder for device determination
                formattedList.AppendLine(device);

                // Append the reversed chunk to the formatted list
                formattedList.AppendLine(string.Join(Environment.NewLine, chunk));
            }

            return formattedList.ToString();
        }
        

        // Final Processes
        private void DefaultPurolatorPrintButton()
        {
            int formatNumber = FindFormatByDevice(targetDevice);
            PurolatorPrint(serialsToPrint, formatNumber, targetDevice);
        }
        private void CustomPurolatorPrintButton()
        {
            int formatNumber = // Recieve from input.
            string device = // recieve from input.
            PurolatorPrint(serialsToPrint, formatNumber);
        }
        public void CreateLotSheet()
        {
            try
            {

                // Read serials from TextBox

                string[] serialString = ConvertSerialsToArray(serialsToPrint);

                // Write serials to the lot sheet in Notepad
                File.WriteAllText(bartenderPath, serialString + Environment.NewLine);

                // Check printer availability
                if (!IsPrinterAvailable("55EXP_2"))
                {
                    Console.WriteLine("Printer '55EXP_2' is unavailable. Lot sheet creation aborted.");
                    return;
                }

                // Create batch script
                string cmdScript = @"@echo off
                              set ""target_printer=55EXP_2""
                              powershell -Command ""Get-WmiObject -Query 'SELECT * FROM Win32_Printer WHERE ShareName=''%target_printer%'' ' | Invoke-WmiMethod -Name SetDefaultPrinter""
                              ""C:\Seagull\BarTender 7.10\Standard\bartend.exe"" /f=C:\BTAutomation\NewPrintertest.btw /p /x";

                // Execute the batch script
                try
                {
                    ExecuteBatchScript(cmdScript);
                }
                catch
                {
                    Console.WriteLine("Lot Sheets Failed");
                }
            }
            catch (Exception ex)
            {
                Console.Write($"An unexpected error occurred: {ex.Message}");
            }

        } // prints all serials to a lot sheet.
        public void CreateBarcodes()
        {
            try
            {

                // Read serials from TextBox
                string[] serialString = ConvertSerialsToArray(serialsToPrint);

                // Write serials to the barcode file in Notepad
                File.WriteAllText(bartenderPath, serialString + Environment.NewLine);

                // Check printer availability
                if (!IsPrinterAvailable("55EXP_Barcode"))
                {
                    Console.WriteLine("Printer '55EXP_Barcode' is unavailable. Barcode creation aborted.");
                    return;
                }

                // Create batch script
                string cmdScript = @"@echo off
                              set ""target_printer=55EXP_Barcode""
                              powershell -Command ""Get-WmiObject -Query 'SELECT * FROM Win32_Printer WHERE ShareName=''%target_printer%'' ' | Invoke-WmiMethod -Name SetDefaultPrinter""
                              ""C:\Seagull\BarTender 7.10\Standard\bartend.exe"" /f=C:\BTAutomation\singlebar.btw /p /x";

                // Execute the batch script
                try
                {
                    ExecuteBatchScript(cmdScript);
                }
                catch
                {
                    Console.WriteLine("Failed to execute the barcode batch script.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        } // prints all serials to the barcode printer.
        private bool IsPrinterAvailable(string printerName)
        {
            try
            {
                var query = $"SELECT * FROM Win32_Printer WHERE ShareName='{printerName}'";
                var searcher = new ManagementObjectSearcher(query);
                var results = searcher.Get();

                return results.Count > 0;
            }
            catch
            {
                // Log or handle any issues while querying the printer
                return false;
            }
        }
    }

}
