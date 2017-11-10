using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Input;
using Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ViewModels
{
    public class GameViewModel : BindableBase
    {
        private Game game;
        private int minPlayed;

        public string Name { get { return game.Name; } }
        public string Id { get { return game.Id; } }
        public int NumPlays { get { return game.NumPlays; } }
        public int MinPlayed
        {
            get { return minPlayed; }
            set
            {
                SetProperty(ref minPlayed, value);
                RaisePropertyChanged(nameof(InTCO));
                RaisePropertyChanged(nameof(OutTCO));
            }
        }
        public bool Own { get { return game.Own; } }
        public bool PrevOwned { get { return game.PrevOwned; } }
        public decimal PricePaid { get { return game.PricePaid; } }
        public decimal CurrValue { get { return game.CurrValue; } }
        public decimal InTCO
        {
            get
            {
                if (NumPlays == 0 || MinPlayed == 0) return PricePaid;
                return PricePaid * 60 / MinPlayed;
            }
        }
        public decimal OutTCO
        {
            get
            {
                if (NumPlays == 0 || MinPlayed == 0) return PricePaid - CurrValue;
                return (PricePaid - CurrValue) * 60 / MinPlayed;
            }
        }

        public GameViewModel(Game game)
        {
            this.game = game;
        }
    }

    public class MainWindowViewModel : BindableBase
    {
        private IUIMainWindowService windowService;
        private ICommand openFile;
        private ICommand getPlaytime;
        private bool running;
        private string userName;

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
                        PrevOwned.Clear();
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
                                Owned.Add(new GameViewModel(item));
                            }
                            foreach (var item in prev)
                            {
                                PrevOwned.Add(new GameViewModel(item));
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
        public ICommand GetPlaytime {
            get
            {
                return getPlaytime ??
                (getPlaytime = new DelegateCommand(async () => await GatherPlaytime())
                    .ObservesProperty(() => Running)
                    .ObservesProperty(() => UserName)
                    .ObservesCanExecute(() => CanGatherTime)
                );
            }
        }
        // TODO readme file + manual
        public ObservableCollection<GameViewModel> Owned { get; }
        public ObservableCollection<GameViewModel> PrevOwned { get; }
        public bool Running { get { return running; } set { SetProperty(ref running, value); } }
        public string UserName { get { return userName; } set { SetProperty(ref userName, value); } }
        public bool CanGatherTime { get
            {
                return !Running
                    && !string.IsNullOrWhiteSpace(UserName)
                    && !UserName.Contains(" ")
                    && (Owned.Count != 0 || PrevOwned.Count != 0);
            }
        }

        private async Task GatherPlaytime()
        {
            Running = true;
            foreach (var game in Owned.Union(PrevOwned))
            {
                game.MinPlayed = await Task.Run(() => DataCollector.GetMinutesPlayedAsync(UserName, game.Id));
            }
            Running = false;
        }

        // ctor
        public MainWindowViewModel(IUIMainWindowService windowService)
        {
            this.windowService = windowService;
            Owned = new ObservableCollection<GameViewModel>();
            Owned.CollectionChanged += (sender, e) => { RaisePropertyChanged(nameof(CanGatherTime)); };
            PrevOwned = new ObservableCollection<GameViewModel>();
            PrevOwned.CollectionChanged += (sender, e) => { RaisePropertyChanged(nameof(CanGatherTime)); };
        }
    }
}
