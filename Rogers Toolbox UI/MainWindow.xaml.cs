using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Windows;
using RogersToolbox;
using System;

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
                    await Task.Delay(6000); // Initial delay before starting the import
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

                // Add cases for other buttons as needed

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
    }
}