using Models.Interfaces;
using Models;
using System.Windows;
using ViewModels;
using ViewModels.Interfaces;

namespace BGG_Played
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow = new MainWindow();
            // Inject dependencies
            IDataProvider dataProvider = new DataCollector();
            IFileReader fileReader = new CSVFileReader();
            IUIMainWindowService service = new MainWindowService(MainWindow);
            MainWindow.DataContext =  new MainWindowViewModel(service, fileReader, dataProvider);
            MainWindow.Show();
        }
    }
}
