using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Windows;
using RogersToolbox;
using System;
using System.Windows.Controls;
using Toolbox_Class_Library.CtrUpdate;
using Toolbox_Class_Library;

namespace Rogers_Toolbox_UI
{


    public partial class MainWindow : Window
    {
        // Define CurrentSerials at the class level
        private ActiveSerials CurrentSerials;
        public string StartupText { get; set; } = $"Welcome to the Rogers Toolbox 4.0 {Toolbox_Class_Library.Properties.Settings.Default.Username}";
        private bool IsOnline = true;
        private DatabaseConnection dbConnection;

        public MainWindow()
        {
            InitializeComponent();
            dbConnection = new DatabaseConnection("hi"); // Initialize without a parameter
            InitializeDataAsync(); // Call the async method
            LoadTheme();
            DataContext = this;
        }

        private async void InitializeDataAsync()
        {
            try
            {
                CurrentSerials = new ActiveSerials(""); // Initialize CurrentSerials with an empty string or any default value
                
                IsOnline = await dbConnection.CheckIsOnline();
                
                if (!IsOnline)
                {
                    Console.WriteLine($"Rogers Toolbox is Offline. Sorry {Toolbox_Class_Library.Properties.Settings.Default.Username}..");
                    this.Close();
                }
                else
                {
                    Console.WriteLine($"Rogers Toolbox is Online! Welcome {Toolbox_Class_Library.Properties.Settings.Default.Username}!");
                }
                // Update UI or perform other actions based on IsOnline
                // For example, you might want to display a message or enable/disable controls
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during initialization: {ex.Message}");
                // Handle the error (e.g., show a message box)
            }
        } // Initialize Data
        private void LoadTheme()
        {
            string selectedTheme = Toolbox_Class_Library.Properties.Settings.Default.Theme;
            // Load the appropriate theme based on the setting
            switch (selectedTheme)
            {
                case "Dark":
                    ApplyTheme("Themes/DarkTheme.xaml");
                    break;
                case "Rogers":
                    ApplyTheme("Themes/RogersTheme.xaml");
                    break;
                case "Club":
                    ApplyTheme("Themes/ClubTheme.xaml");
                    break;
                case "Coffee":
                    ApplyTheme("Themes/CoffeeTheme.xaml");
                    break;
                case "Fade":
                    ApplyTheme("Themes/FadeTheme.xaml");
                    break;
                case "Neon":
                    ApplyTheme("Themes/NeonTheme.xaml");
                    break;
                default:
                    ApplyTheme("Themes/DarkTheme.xaml"); // Default to Dark if not set
                    break;
            }
        }

        public void ApplyTheme(string themePath)
        {
            ResourceDictionary newTheme = (ResourceDictionary)Application.LoadComponent(new Uri(themePath, UriKind.Relative));
     
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(newTheme);
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;

            // Update CurrentSerials based on the TextBox input
            CurrentSerials = GetTextboxText();
            IsOnline = await dbConnection.CheckIsOnline();
            if (!IsOnline)
            {
                this.Close();
            }

            switch (button.Name)
            {
                case "BlitzButton":
                    Stopwatch stopwatch = new Stopwatch();
                    UpdateMessage("Starting Blitz Import, Please click target Location");
                    await Task.Delay(6000);

                    stopwatch.Start();

                    // Create ActiveSerials instance with a callback for updates
                    CurrentSerials = new ActiveSerials(TextBox.Text, UpdateSerialsDisplay);

                    await CurrentSerials.BlitzImport(); // Runs and updates display while processing

                    stopwatch.Stop();

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
                case "SettingsButton":
                    SettingsWindow settingsWindow = new SettingsWindow(this);
                    settingsWindow.ShowDialog();
                    break;
                case "CTRButton":

                    CtrUpdate ctrUpdate = new CtrUpdate();
                    ctrUpdate.Run();
                    break;
                case "FormatSerialsButton":
                    string serialsText = TextBox.Text;
                    FormatWindow formatWindow = new FormatWindow(serialsText);
                    formatWindow.Owner = this; // Set MainWindow as the owner
                    formatWindow.ShowDialog(); // Show as a modal dialog

                    // After closing, update TextBox if FormattedSerials is set
                    if (!string.IsNullOrEmpty(formatWindow.FormattedSerials))
                    {
                        TextBox.Text = formatWindow.FormattedSerials;
                    }
                    break;

                default:
                    UpdateMessage("Didn't read anything :(");
                    break;
            }
        } // Handles what happens when a button is clicked
        private ActiveSerials GetTextboxText()
        {
            // Split the TextBox text into lines and create a new ActiveSerials instance
            string[] lines = TextBox.Text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            string output = string.Join("\n", lines);
            ActiveSerials tempList = new ActiveSerials(output);

            return tempList;
        } // Gets the Textbox text and converts it to our current Active Serials
        public void UpdateMessage(string text)
        {
            InfoBox.Content = (text);
        }  // Updates the Textbox that communicates to the user. 
        private void UpdateSerialsDisplay()
        {
            TextBox.Clear();
            TextBox.Text = CurrentSerials.GetRemainingSerials(); // Get remaining serials from CurrentSerials
            int remainingSerials = CurrentSerials.Serials.Count; // Update remaining serials count
            InfoBox.Content = ($"{remainingSerials} Serials Loaded");
        } // Specifically Updates the Textbox to communicate with the user how many serials are currently loaded.
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) 
        {
            // Split the text of the TextBox into lines based on newline characters
            var lines = TextBox.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // Create the line number text (e.g., 1, 2, 3, ...)
            var lineNumbers = string.Join("\n", lines.Select((line, index) => $"{index + 1}:"));

            // Update the line number label with the new line numbers
            LineNumberLabel.Text = lineNumbers;
            UpdateMessage($"{lines.Count()} Serials Loaded");


        } // Detects when the input box has changed and updates the user textbox with how many there are in real time.
    }
}

