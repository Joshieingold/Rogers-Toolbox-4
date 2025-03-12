using RogersToolbox;
using System.Diagnostics;
using System.Text;
using System.IO;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using System.IO.Packaging;
using DocumentFormat.OpenXml.ExtendedProperties;


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
        // Helper Functions
        private string[] ConvertSerialsToArray()
        {
            string stringSerials = serialsToPrint.ConvertSerialsToString();
            return stringSerials.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
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
        
        // Purolator
        private void PurolatorPrint(int formatBy)
        {
            
            string puroSheet = FormatSheet(formatBy, ConvertSerialsToArray());
            File.WriteAllText(bartenderPath, puroSheet + Environment.NewLine);


            string batchFile = targetDevice == "IPTVARXI6HD" || targetDevice == "IPTVTCXI6HD" || targetDevice == "SCXI11BEI"
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
        public string FormatSheet(int numSplit, string[] serials)
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
                formattedList.AppendLine(targetDevice);

                // Append the reversed chunk to the formatted list
                formattedList.AppendLine(string.Join(Environment.NewLine, chunk));
            }

            return formattedList.ToString();
        }



        // Public Processes
        public void DefaultPrintPurolator()
        {
            int formatNumber = FindFormatByDevice(targetDevice);
            if (serialsToPrint.Serials == null || (serialsToPrint.Serials).Count  <= 0)
            {
                Console.WriteLine("\nNo Serials to print.");
            }
            else
            {
                try
                {
                    PurolatorPrint(formatNumber);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nThere was an unexpected error.");
                    Console.Write($"\n{ex}");
                }
            }
                
        }
        public void PrintBarcodes()
        {
            string[] serials = ConvertSerialsToArray();
            string serialString = string.Join(Environment.NewLine, serials);
            if (serials == null || serials.Length == 0)
            {
                Console.Write("\nNo serials to print.");
            }
            else
            {
                string cmdScript = @"@echo off
                              set ""target_printer=55EXP_Barcode""
                              powershell -Command ""Get-WmiObject -Query 'SELECT * FROM Win32_Printer WHERE ShareName=''%target_printer%'' ' | Invoke-WmiMethod -Name SetDefaultPrinter""
                              ""C:\Seagull\BarTender 7.10\Standard\bartend.exe"" /f=C:\BTAutomation\singlebar.btw /p /x";
                try
                {
                    File.WriteAllText(bartenderPath, serialString + Environment.NewLine);
                    ExecuteBatchScript(cmdScript);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nThere was an unexpected error.");
                    Console.Write($"\n{ex}");
                }
            }
        }
        public void PrintLotSheets()
        {
            string[] serials = ConvertSerialsToArray();
            string serialString = string.Join(Environment.NewLine, serials);
            if (serials == null || serials.Length == 0)
            {
                Console.Write("\nNo serials to print.");

            }
            else
            {
                string cmdScript = @"@echo off
                              set ""target_printer=55EXP_2""
                              powershell -Command ""Get-WmiObject -Query 'SELECT * FROM Win32_Printer WHERE ShareName=''%target_printer%'' ' | Invoke-WmiMethod -Name SetDefaultPrinter""
                              ""C:\Seagull\BarTender 7.10\Standard\bartend.exe"" /f=C:\BTAutomation\NewPrintertest.btw /p /x";
                try
                {
                    File.WriteAllText(bartenderPath, serialString + Environment.NewLine);
                    ExecuteBatchScript(cmdScript);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nThere was an unexpected error.");
                }
            }
        }

    }
}
