using System.Windows;
using RogersToolbox;

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
                    UpdateMessage("Trying to Blitz Import :(");
                    await CurrentSerials.BlitzImport(); // Ensure BlitzImport is awaited
                    UpdateSerialsDisplay(); // Update the display after blitz import
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