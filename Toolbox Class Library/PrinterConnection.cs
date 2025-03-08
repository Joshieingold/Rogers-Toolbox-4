using DocumentFormat.OpenXml.ExtendedProperties;
using RogersToolbox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox_Class_Library.CtrUpdate;

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
            // ADD BARTENDER PATH TO SETTINGS AND DECLARE IT HERE
            string device = (serials.Serials[0]).Device;
        }
        private string[] ConvertSerialsToArray(ActiveSerials serials)
        {
            string stringSerials = serials.ConvertSerialsToString();
            return stringSerials.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        }
        private void PurolatorPrint(ActiveSerials serials, int formatBy)
        {
            
            string puroSheet = FormatSheet(formatBy, ConvertSerialsToArray(serialsToPrint));
            File.WriteAllText(bartenderNotepad, puroSheet + Environment.NewLine);


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
        // Final Processes
        private void DefaultPurolatorPrintButton()
        {
            int formatNumber = FindFormatByDevice(targetDevice);
            PurolatorPrint(serialsToPrint, formatNumber);
        }
    }
}
