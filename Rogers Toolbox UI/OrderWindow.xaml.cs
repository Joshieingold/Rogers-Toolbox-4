using RogersToolbox;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WindowsInput;
using static Google.Cloud.Firestore.V1.StructuredAggregationQuery.Types.Aggregation.Types;

namespace Rogers_Toolbox_UI
{
    public partial class OrderWindow : Window
    {
        public List<ActiveSerials> SerialsToImport { get; set; } = new List<ActiveSerials>();
        private InputSimulator inputSimulator = new();
        public string OrderNumberText { get; set; }
        public bool initialImport = true;
        private int typingSpeed = Toolbox_Class_Library.Properties.Settings.Default.TypingSpeed;

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
                }
            }

            // Render all serial entries now
            for (int i = 0; i < SerialsToImport.Count; i++)
            {
                AddSerialEntry(i);
            }
        }
        // LOOKS LIKE BLITZ IMPORT ACTUALLY DELETES SERIALS FROM THE LIST.
        private async Task SimulateTyping(string text)
        {

            foreach (char c in text)
            {
                inputSimulator.Keyboard.TextEntry(c);
                await Task.Delay(typingSpeed);
            }
        }
        private void AddSerialEntry(int index)
        {
            ActiveSerials serial = SerialsToImport[index];

            var stack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 5),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Checkbox
            // To style the CheckBox in WPF, you should define a Style in your XAML file (OrderWindow.xaml).
            // Example: Add this inside <Window.Resources> in your OrderWindow.xaml

            

            // Then, in your C# code, apply the style to your CheckBox:
            var checkBox = new CheckBox
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 5, 0),
                Style = (Style)FindResource("CustomCheckBoxStyle")
            };

            // Device Button
            var deviceButton = new Button
            {
                Content = serial.Serials.Count >= 1 ? serial.Serials[0].Device : "[Unknown Device]",
                Margin = new Thickness(0, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Capture index explicitly so it binds correctly
            deviceButton.Click += async (sender, args) =>
            {
                checkBox.IsChecked = true;
                try
                {
                    await OrderImportAutomation(SerialsToImport[index].Serials[0].Device, SerialsToImport[index].Serials.Count());
                    await SerialsToImport[index].BlitzImport();
                    if (AllSerialsChecked())
                    {
                        await Task.Delay(2000);
                        await SimulateTyping("DROPCRER");
                        await Task.Delay(500);
                        inputSimulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);
                        await Task.Delay(1500);
                        // CTRL+X, 4, ENTER, LPN, ENTER, LPN, ENTER
                        inputSimulator.Keyboard.ModifiedKeyStroke(WindowsInput.Native.VirtualKeyCode.CONTROL, WindowsInput.Native.VirtualKeyCode.VK_X);
                        await Task.Delay(500);
                        await SimulateTyping("4");
                        await Task.Delay(500);
                        inputSimulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);
                        await Task.Delay(500);
                        await SimulateTyping($"{LpnBox.Text}");
                        await Task.Delay(500);
                        inputSimulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);
                        await Task.Delay(500);
                        await SimulateTyping($"{LpnBox.Text}");
                        await Task.Delay(500);
                        inputSimulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);
                        await Task.Delay(500);
                    }
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


        private async Task OrderImportAutomation(string deviceName, int number_of_serials)
        {
            // wait a little bit.
            await Task.Delay(3500);
            if (initialImport == true || InventoryBox.Text != "")
            {
                await SimulateTyping(LpnBox.Text);
                await Task.Delay(500);
                inputSimulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);
                await Task.Delay(1000);
                if (InventoryBox.Text != "")
                {
                    await SimulateTyping(InventoryBox.Text);
                }
                else
                {
                    await SimulateTyping("S.FLOOR.CRETAILR");
                }
                await Task.Delay(500);
                inputSimulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);
                await Task.Delay(1000);
                initialImport = false;
            }
            await SimulateTyping(deviceName);
            await Task.Delay(400);
            inputSimulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);
            await Task.Delay(400);
            await SimulateTyping(number_of_serials.ToString());
            await Task.Delay(400);
            inputSimulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);
            await Task.Delay(700);
            // IF ALL SERIALS ARE CHECKED YOURE GONNA CHECK FOR FLEXIPRO PIXEL.
            // IF ALL CHECK BOXES ARE CHECKED YOURE ALSO GONNA WRITE DROPCRER 
            

        }
        // Add this method to OrderWindow class
        private bool AllSerialsChecked()
        {
            foreach (var child in SerialsStackPanel.Children)
            {
                if (child is StackPanel stackPanel)
                {
                    foreach (var element in stackPanel.Children)
                    {
                        if (element is CheckBox checkBox)
                        {
                            if (checkBox.IsChecked != true)
                                return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
