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
            targetDevice = serials.Serials[0].Device;
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
                ? $@"@echo off
                    set ""target_printer=55EXP_Purolator""
                    powershell -Command ""Get-WmiObject -Query 'SELECT * FROM Win32_Printer WHERE ShareName=''55EXP_Purolator'' ' | Invoke-WmiMethod -Name SetDefaultPrinter""
                    ""C:\Seagull\BarTender 7.10\Standard\bartend.exe"" /f=C:\BTAutomation\XI6.btw /p /x"
                : $@"@echo off
                    set ""target_printer=55EXP_Purolator""
                    powershell -Command ""Get-WmiObject -Query 'SELECT * FROM Win32_Printer WHERE ShareName=''55EXP_Purolator'' ' | Invoke-WmiMethod -Name SetDefaultPrinter""
                    ""C:\Seagull\BarTender 7.10\Standard\bartend.exe"" /f=C:\BTAutomation\CODA.btw /p /x";

            try
            {
                ExecuteBatchScript(batchFile);
            }
            catch
            {
                Console.WriteLine("Print Failed");
            }
        }

        private int FindFormatByDevice(string device)
        {
            return (device == "IPTVARXI6HD" || device == "IPTVTCXI6HD" || device == "SCXI11BEI") ? 10 : 8;
        }

        public void ExecuteBatchScript(string scriptContent)
        {
            string tempFilePath = "temp_cmd.bat";

            File.WriteAllText(tempFilePath, scriptContent);

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

            File.Delete(tempFilePath);
        }

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
                List<string> chunk = serials.Skip(i).Take(numSplit).ToList();
                chunk.Reverse();

                formattedList.AppendLine(device);
                formattedList.AppendLine(string.Join(Environment.NewLine, chunk));
            }

            return formattedList.ToString();
        }

        private void DefaultPurolatorPrintButton()
        {
            int formatNumber = FindFormatByDevice(targetDevice);
            PurolatorPrint(serialsToPrint, formatNumber, targetDevice);
        }

        private void CustomPurolatorPrintButton()
        {
            int formatNumber = 7389; // Receive from input.
            string device = "temp"; // Receive from input.
            PurolatorPrint(serialsToPrint, formatNumber, device);
        }

        public void CreateLotSheet()
        {
            try
            {
                string[] serialString = ConvertSerialsToArray(serialsToPrint);
                File.WriteAllText(bartenderPath, serialString + Environment.NewLine);

                if (!IsPrinterAvailable("55EXP_2"))
                {
                    Console.WriteLine("Printer '55EXP_2' is unavailable. Lot sheet creation aborted.");
                    return;
                }

                string cmdScript = @"@echo off
                    set ""target_printer=55EXP_2""
                    powershell -Command ""Get-WmiObject -Query 'SELECT * FROM Win32_Printer WHERE ShareName=''55EXP_2'' ' | Invoke-WmiMethod -Name SetDefaultPrinter""
                    ""C:\Seagull\BarTender 7.10\Standard\bartend.exe"" /f=C:\BTAutomation\NewPrintertest.btw /p /x";

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
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }

        public void CreateBarcodes()
        {
            try
            {
                string[] serialString = ConvertSerialsToArray(serialsToPrint);
                File.WriteAllText(bartenderPath, serialString + Environment.NewLine);

                if (!IsPrinterAvailable("55EXP_Barcode"))
                {
                    Console.WriteLine("Printer '55EXP_Barcode' is unavailable. Barcode creation aborted.");
                    return;
                }

                string cmdScript = @"@echo off
                    set ""target_printer=55EXP_Barcode""
                    powershell -Command ""Get-WmiObject -Query 'SELECT * FROM Win32_Printer WHERE ShareName=''55EXP_Barcode'' ' | Invoke-WmiMethod -Name SetDefaultPrinter""
                    ""C:\Seagull\BarTender 7.10\Standard\bartend.exe"" /f=C:\BTAutomation\singlebar.btw /p /x";

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
        }

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
                return false;
            }
        }
    }
}