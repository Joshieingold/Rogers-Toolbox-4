using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Windows;
using RogersToolbox;
using System;
using System.Windows.Controls;

namespace Rogers_Toolbox_UI
{
    public partial class MainWindow : Window
    {
        // Define CurrentSerials at the class level
        private ActiveSerials CurrentSerials;

        public MainWindow()
        {
            InitializeComponent();
            InitializeData();
        }

        public void InitializeData()
        {
            // Initialize CurrentSerials with an empty string or any default value
            CurrentSerials = new ActiveSerials("");
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;

            // Update CurrentSerials based on the TextBox input
            CurrentSerials = GetTextboxText();

            switch (button.Name)
            {
                case "BlitzButton":
                    Stopwatch stopwatch = new Stopwatch();
                    UpdateMessage("Starting Blitz Import, Please click target Location");
                    await Task.Delay(6000); 
                    stopwatch.Start();
                    await CurrentSerials.BlitzImport(); 
                    UpdateSerialsDisplay();
                    stopwatch.Stop(); // TIME!

                    TimeSpan ts = stopwatch.Elapsed;
                    string elapsedTime = String.Format("{0:00}h : {1:00}m : {2:00}s : {3:00} ms",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                    UpdateMessage($"Import Completed in {elapsedTime}");
                    break;
                case "OpenExcelButton":
                    ActiveSerials TempList = new ActiveSerials(CurrentSerials.OpenExcel());
                    CurrentSerials = TempList;
                    UpdateSerialsDisplay();
                    break;


                default:
                    UpdateMessage("Didn't read anything :(");
                    break;
            }
        }

        private ActiveSerials GetTextboxText()
        {
            // Split the TextBox text into lines and create a new ActiveSerials instance
            string[] lines = TextBox.Text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            string output = string.Join("\n", lines);
            ActiveSerials tempList = new ActiveSerials(output);

            return tempList;
        }
        private void UpdateMessage(string text)
        {
            InfoBox.Content = (text);
        }
        private void UpdateSerialsDisplay()
        {
            TextBox.Clear();
            TextBox.Text = CurrentSerials.GetRemainingSerials(); // Get remaining serials from CurrentSerials
            int remainingSerials = CurrentSerials.Serials.Count; // Update remaining serials count
            InfoBox.Content = ($"{remainingSerials} Serials Loaded");
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) // Counts the new lines in the textbox to allow for a serial count next to the serials.
        {
            // Split the text of the TextBox into lines based on newline characters
            var lines = TextBox.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // Create the line number text (e.g., 1, 2, 3, ...)
            var lineNumbers = string.Join("\n", lines.Select((line, index) => $"{index + 1}:"));

            // Update the line number label with the new line numbers
            LineNumberLabel.Text = lineNumbers;


        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) // Counts the new lines in the textbox to allow for a serial count next to the serials.
        {
            // Split the text of the TextBox into lines based on newline characters
            var lines = TextBox.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // Create the line number text (e.g., 1, 2, 3, ...)
            var lineNumbers = string.Join("\n", lines.Select((line, index) => $"{index + 1}:"));

            // Update the line number label with the new line numbers
            LineNumberLabel.Text = lineNumbers;
            InfoBox.Content = ($"{lines.Count()} Serials Loaded");

        }
    }
}