using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models.Interfaces;
using ViewModels.Interfaces;

namespace ViewModels
{
    public class GameViewModel : BindableBase
    {
        private IGame game;
        private MainWindowViewModel vm;
        private int minPlayed;

        public string Name { get => game.Name; }
        public string Id { get => game.Id; }
        public int NumPlays { get => game.NumPlays; }
        public int MinPlayed
        {
            get => minPlayed;
            set
            {
                SetProperty(ref minPlayed, value);
                RaisePropertyChanged(nameof(InTCO));
                RaisePropertyChanged(nameof(OutTCO));
                RaisePropertyChanged(nameof(Debt));
            }
        }
        public bool Own { get => game.Own; }
        public bool PrevOwned { get => game.PrevOwned; }
        public decimal PricePaid { get => game.PricePaid; }
        public decimal CurrValue { get => game.CurrValue; }
        public decimal InTCO
        {
            get
            {
                if (NumPlays == 0 || MinPlayed == 0) return PricePaid;
                return PricePaid * 60.0m / MinPlayed;
            }
        }
        public decimal OutTCO
        {
            get
            {
                if (NumPlays == 0 || MinPlayed == 0) return PricePaid - CurrValue;
                return (PricePaid - CurrValue) * 60.0m / MinPlayed;
            }
        }
        public double Debt
        {
            get
            {
                if (vm.HourRate == 0) return 0;
                decimal shortage = PricePaid - CurrValue - MinPlayed / 60.0m * vm.HourRate;
                if (shortage < 0) return 0;
                return (double)(shortage / vm.HourRate);
            }
        }
        public GameViewModel(IGame game, MainWindowViewModel vm)
        {
            this.game = game;
            this.vm = vm;
            vm.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(vm.HourRate))
                {
                    RaisePropertyChanged(nameof(Debt));
                }
            };
        }
    }

    public class MainWindowViewModel : BindableBase
    {
        private IUIMainWindowService windowService;
        private IFileReader fileReader;
        private IDataProvider dataProvider;
        private ICommand openFile;
        private ICommand getPlaytime;
        private bool running;
        private string userName;
        private decimal hourRate;

        public ICommand OpenFile
        {
            get
            {
                return openFile ??
                (openFile = new DelegateCommand(() =>
                {
                    string fileName = windowService.OpenFileDialog(fileReader.Extension);
                    if (fileName != null)
                    {
                        Owned.Clear();
                        PrevOwned.Clear();
                        try
                        {
                            // Read File
                            List<IGame> games = fileReader.ReadGames(fileName).ToList();
                            var owned = from game in games
                                        where game.Own == true
                                        where game.PricePaid > 0
                                        select game;
                            var prev = from game in games
                                       where game.PrevOwned == true
                                       where game.NumPlays > 0
                                       select game;
                            foreach (var item in owned)
                            {
                                Owned.Add(new GameViewModel(item, this));
                            }
                            foreach (var item in prev)
                            {
                                PrevOwned.Add(new GameViewModel(item, this));
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
        public ObservableCollection<GameViewModel> Owned { get; }
        public ObservableCollection<GameViewModel> PrevOwned { get; }
        public bool Running { get { return running; } set { SetProperty(ref running, value); } }
        public string UserName { get { return userName; } set { SetProperty(ref userName, value); } }
        public decimal HourRate { get { return hourRate; } set { SetProperty(ref hourRate, value); } }
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
            List<IPlay> plays = await Task.Run(() => dataProvider.GetPlaysAsync(UserName));
            foreach (var game in Owned.Union(PrevOwned))
            {
                game.MinPlayed = plays.Where(p => p.GameId == game.Id).Sum(p => p.Minutes);
            }
            Running = false;
        }

        // ctor
        public MainWindowViewModel(
            IUIMainWindowService windowService,
            IFileReader fileReader,
            IDataProvider dataProvider)
        {
            this.windowService = windowService;
            this.fileReader = fileReader;
            this.dataProvider = dataProvider;
            Owned = new ObservableCollection<GameViewModel>();
            Owned.CollectionChanged += (sender, e) => { RaisePropertyChanged(nameof(CanGatherTime)); };
            PrevOwned = new ObservableCollection<GameViewModel>();
            PrevOwned.CollectionChanged += (sender, e) => { RaisePropertyChanged(nameof(CanGatherTime)); };
            HourRate = 200;
        }
    }
}
