using Microsoft.Win32;
using System.Windows;
using ViewModels.Interfaces;

namespace BGG_Played
{
    class MainWindowService : IUIMainWindowService
    {
        private Window mainWindow;
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
        public MainWindowService(Window mainWindow)
        {
            this.mainWindow = mainWindow;
        }
    }
}
