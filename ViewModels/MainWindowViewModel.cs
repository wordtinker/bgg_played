using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Input;
using Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private IUIMainWindowService windowService;
        private ICommand openFile;

        public ICommand OpenFile
        {
            get
            {
                return openFile ??
                (openFile = new DelegateCommand(() =>
                {
                    string fileName = windowService.OpenFileDialog(CSVFileReader.Extension);
                    if (fileName != null)
                    {
                        Owned.Clear();
                        try
                        {
                            // Read File
                            List<Game> games = CSVFileReader.ReadGames(fileName).ToList();
                            var owned = from game in games
                                        where game.Own == true
                                        select game;
                            var prev = from game in games
                                       where game.PrevOwned == true
                                       where game.NumPlays > 0
                                       select game;
                            foreach (var item in owned)
                            {
                                Owned.Add(item);
                            }
                            foreach (var item in prev)
                            {
                                PrevOwned.Add(item);
                            }
                        }
                        catch (System.Exception)
                        {
                            windowService.ShowMessage("File is broken.");
                        }
                    }
                }));
            }
        }
        // TODO Game or GameVM ???
        // TODO readme file + manual
        public ObservableCollection<Game> Owned { get; }
        public ObservableCollection<Game> PrevOwned { get; }

        // ctor
        public MainWindowViewModel(IUIMainWindowService windowService)
        {
            this.windowService = windowService;
            Owned = new ObservableCollection<Game>();
            PrevOwned = new ObservableCollection<Game>();
        }
    }
}
