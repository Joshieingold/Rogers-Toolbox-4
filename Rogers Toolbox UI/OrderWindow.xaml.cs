using RogersToolbox;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Rogers_Toolbox_UI
{
    public partial class OrderWindow : Window
    {
        public List<ActiveSerials> SerialsToImport { get; set; } = new List<ActiveSerials>();
        public string OrderNumberText { get; set; }
        public bool initialImport = true;

        public OrderWindow(string inputSerials, int orderNumber)
        {
            InitializeComponent();
            OrderNumberText = $"Order Number {orderNumber}";
            OrderHeader.Content = OrderNumberText;

            // Normalize to \n for consistent splitting
            string normalized = inputSerials.Replace("\r\n", "\n");

            // Split by double newlines to get separate serial groups
            string[] raw_List = normalized.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string serial_list in raw_List)
            {
                if (!string.IsNullOrWhiteSpace(serial_list))
                {
                    ActiveSerials newSerial = new ActiveSerials(serial_list);
                    SerialsToImport.Add(newSerial);
                    AddSerialEntry(newSerial);
                }
            }
        }

        private void AddSerialEntry(ActiveSerials serial)
        {
            var stack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 5),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Checkbox
            var checkBox = new CheckBox
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 5, 0)
            };

            // Device Button
            var deviceButton = new Button
            {
                Content = serial.Serials.Count >= 1 ? serial.Serials[0].Device : "[Unknown Device]",
                Margin = new Thickness(0, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            deviceButton.Click += async (sender, args) =>
            {
                checkBox.IsChecked = true;

                try
                {
                    await serial.BlitzImport();
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during BlitzImport:\n{ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            // Info Icon (ToolTip)
            var infoIcon = new TextBlock
            {
                Text = "🛈",
                FontSize = 16,
                Foreground = Brushes.Gray,
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = serial.ConvertSerialsToString()
            };

            stack.Children.Add(checkBox);
            stack.Children.Add(deviceButton);
            stack.Children.Add(infoIcon);

            SerialsStackPanel.Children.Add(stack);
        }
     
    }

}
