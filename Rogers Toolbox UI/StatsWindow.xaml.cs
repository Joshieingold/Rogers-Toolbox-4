using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Toolbox_Class_Library;
using DocumentFormat.OpenXml.Bibliography;

namespace Rogers_Toolbox_UI
{
    /// <summary>
    /// Interaction logic for StatsWindow.xaml
    /// </summary>
    public partial class StatsWindow : Window
    {
        DatabaseConnection db = new DatabaseConnection();
        Dictionary<string, int> Actuals { get; set; }
        Dictionary<string, int> Goals { get; set; }
        int ActualTotal { get; set; }
        int XB8Required { get; set; }
        int XB8Actual { get; set; }
        int XB7fcRequired { get; set; }
        int XB7fcActual { get; set; }
        int XB7FCRequired { get; set; }
        int XB7FCActual { get; set; }
        int Xi6tRequired { get; set; }
        int Xi6tActual { get; set; }
        int Xi6ARequired { get; set; }
        int Xi6AActual { get; set; }
        int XiOneRequired { get; set; }
        int XiOneActual { get; set; }
        int PodsRequired { get; set; }
        int PodsActual { get; set; }
        int RequiredTotal { get; set; }
        int RequiredPerDay { get; set; }
        int DailyAverage { get; set; }
        string month { get; set; }

        public StatsWindow()
        {
            InitializeComponent();
            int monthInt = DateTime.Now.Month;
            month = GetMonthFromInt(monthInt);
            monthSelector.Text = month;
            InitializeDataAsync();
        }
        // GOALS TAB:
        // Helper Functions
        private string GetMonthFromInt(int monthNum)
        {
            switch (monthNum)
            {
                case 1:
                    return "January";
                case 2:
                    return "February";
                case 3:
                    return "March";
                case 4:
                    return "April";
                case 5:
                    return "May";
                case 6:
                    return "June";
                case 7:
                    return "July";
                case 8:
                    return "August";
                case 9:
                    return "September";
                case 10:
                    return "October";
                case 11:
                    return "November";
                case 12:
                    return "December";
                default:
                    return "January";
            }
        }
        private int GetIntFromMonth(string monthStr)
        {
            switch (monthStr)
            {
                case "January":
                    return 1;
                case "February":
                    return 2;
                case "March":
                    return 3;
                case "April":
                    return 4;
                case "May":
                    return 5;
                case "June":
                    return 6;
                case "July":
                    return 7;
                case "August":
                    return 8;
                case "September":
                    return 9;
                case "October":
                    return 10;
                case "November":
                    return 11;
                case "December":
                    return 12;
                default:
                    return 1;
            }
        }
        // Initialize Data
        private async void InitializeDataAsync()
        {
            try
            {
                await SetData(month);
                getTotals();
                InitializeCharts();
                UpdateRequiredPerDay(GetIntFromMonth(month));

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async Task SetData(string month)
        {
            Actuals = await db.PullDeviceData(month);
            Goals = await db.PullGoalsData(month);
        }

        private void getTotals()
        {
            ActualTotal = Actuals.Values.Sum();
            XB8Required = Goals["XB8Required"];
            XB8Actual = Actuals["CGM4981COM"];
            XB7fcRequired = Goals["XB7fcRequired"];
            XB7fcActual = Actuals["CGM4331COM"];
            XB7FCRequired = Goals["XB7FCRequired"];
            XB7FCActual = Actuals["TG4482A"];
            Xi6tRequired = Goals["Xi6tRequired"];
            Xi6tActual = Actuals["IPTVTCXI6HD"];
            Xi6ARequired = Goals["Xi6ARequired"];
            Xi6AActual = Actuals["IPTVARXI6HD"];
            XiOneRequired = Goals["XiOneRequired"];
            XiOneActual = Actuals["SCXI11BEI"];
            PodsRequired = Goals["PodsRequired"];
            PodsActual = Actuals["XE2SGROG1"];

            RequiredTotal = Goals.Values.Sum();
        }

        private void InitializeCharts()
        {
            TotalPieChart.Series = CreateChart(RequiredTotal, ActualTotal);
            XB8Chart.Series = CreateChart(XB8Required, XB8Actual);
            CGMChart.Series = CreateChart(XB7fcRequired, XB7fcActual);
            TGChart.Series = CreateChart(XB7FCRequired, XB7FCActual);
            XI6TChart.Series = CreateChart(Xi6tRequired, Xi6tActual);
            XI6AChart.Series = CreateChart(Xi6ARequired, Xi6AActual);
            XIONEChart.Series = CreateChart(XiOneRequired, XiOneActual);
            PODSChart.Series = CreateChart(PodsRequired, PodsActual);
        }
        private void FetchDataByMonth_Click(object sender, RoutedEventArgs e)
        {
            month = monthSelector.Text;

            InitializeDataAsync();

        }

        // Calculations
        public SeriesCollection CreateChart(int goal, int completed)
        {
            double overflow = Math.Max(0, completed - goal);
            double actual = Math.Max(0, completed - overflow);
            double required = Math.Max(0, goal - (actual + overflow));

            return new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Completed",
                    Values = new ChartValues<double> { actual },
                    DataLabels = false,
                    LabelPoint = chartPoint => $"{chartPoint.Y} ({chartPoint.Participation:P})",
                    StrokeThickness = 1
                },
                new PieSeries
                {
                    Title = "Unfinished",
                    Values = new ChartValues<double> { required },
                    DataLabels = false,
                    LabelPoint = chartPoint => $"{chartPoint.Y} ({chartPoint.Participation:P})",
                    StrokeThickness = 1
                },
                new PieSeries
                {
                    Title = "Overflow",
                    Values = new ChartValues<double> { overflow },
                    DataLabels = false,
                    Fill = new SolidColorBrush(Color.FromRgb(0, 102, 204)),
                    LabelPoint = chartPoint => $"{chartPoint.Y} ({chartPoint.Participation:P})",
                    StrokeThickness = 1
                }
            };
        }

        private void UpdateRequiredPerDay(int selectedMonth) // Updates required per day data.
        {
            int year = DateTime.Now.Year;

            // If the selected month is December and the current month is January, use the previous year
            if (selectedMonth == 12)
            {
                year -= 1; // Set to the previous year
            }

            int totalWeekdays = GetWeekdaysInMonth(year, selectedMonth);
            int weekdaysSoFar;

            // Check if the selected month is the current month
            if (selectedMonth == DateTime.Now.Month && DateTime.Now.Year == DateTime.Now.Year)
            {
                weekdaysSoFar = GetWeekdaysSoFar(DateTime.Now.Year, selectedMonth, DateTime.Now.Day);
            }
            else
            {
                weekdaysSoFar = totalWeekdays; // For past months, consider all weekdays
            }

            int remainingDays = totalWeekdays - weekdaysSoFar;



            // Calculate Daily Average based on actual total and weekdays so far
            if (weekdaysSoFar > 0)
            {
                DailyAverage = ActualTotal / weekdaysSoFar; // Average over weekdays so far
            }
            else
            {
                DailyAverage = 0; // No weekdays so far
            }

            // Calculate Required Per Day
            if (remainingDays > 0)
            {
                RequiredPerDay = (RequiredTotal - ActualTotal) / remainingDays; // Required per day based on remaining days
            }
            else
            {
                RequiredPerDay = 0; // No remaining days
            }

            // Update UI labels if necessary
            RequiredPerDayLabel.Content = $"Daily Required: {RequiredPerDay}";
            DailyAverageLabel.Content = $"Average Daily Completed: {DailyAverage}";
        }
 
        static int GetWeekdaysInMonth(int year, int month)
        {
            int weekdays = 0;
            int daysInMonth = DateTime.DaysInMonth(year, month);

            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime currentDate = new DateTime(year, month, day);
                if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    weekdays++;
                }
            }

            return weekdays;
        }
        static int GetWeekdaysSoFar(int year, int month, int currentDay)
        {
            int weekdays = 0;

            for (int day = 1; day <= currentDay; day++)
            {
                DateTime currentDate = new DateTime(year, month, day);
                if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    weekdays++;
                }
            }

            return weekdays;
        }
        // GOALS TAB
    }
}
