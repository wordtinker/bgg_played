using Microsoft.Win32;
using System.Windows;
using ViewModels;

namespace BGG_Played
{
    class MainWindowService : IUIMainWindowService
    {
        private MainWindow mainWindow;
        public void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }
        public string OpenFileDialog(string fileExtension)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = fileExtension
            };
            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
            else
            {
                return null;
            }
        }
        public MainWindowService(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }
    }
}
