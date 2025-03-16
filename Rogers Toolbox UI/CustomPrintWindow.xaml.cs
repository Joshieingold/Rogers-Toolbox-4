using System;
using System.Windows;
using System.Windows.Controls;

namespace Rogers_Toolbox_UI
{
    public partial class CustomPrintWindow : Window
    {
        public int FormatBy { get; private set; }
        public string SelectedDevice { get; private set; }

        public CustomPrintWindow()
        {
            InitializeComponent();
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(FormatByInput.Text, out int formatBy) && !string.IsNullOrWhiteSpace(DeviceNameInput.Text))
            {
                FormatBy = formatBy;
                SelectedDevice = DeviceNameInput.Text;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please enter a valid format and device name.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}