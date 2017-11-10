using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ViewModels;

namespace BGG_Played
{

    public class ValueToBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                decimal val = (decimal)values[0];
                decimal hourRate = (decimal)values[1];
                if (val <= hourRate)
                {
                    return Brushes.LightGreen;
                }
                else
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            catch (Exception)
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            MainWindowService service = new MainWindowService(this);
            this.DataContext = new MainWindowViewModel(service);
            InitializeComponent();
        }
    }
}
