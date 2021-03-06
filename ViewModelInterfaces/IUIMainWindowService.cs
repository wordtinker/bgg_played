﻿namespace ViewModels.Interfaces
{
    public interface IUIMainWindowService
    {
        void ShowMessage(string message);
        string OpenFileDialog(string fileExtension);
    }
}
