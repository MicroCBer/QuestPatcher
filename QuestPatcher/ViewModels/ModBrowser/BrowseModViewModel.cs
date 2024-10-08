using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using QuestPatcher.Core;
using QuestPatcher.Core.Models;
using QuestPatcher.ModBrowser;
using QuestPatcher.ModBrowser.Models;
using QuestPatcher.Models;
using ReactiveUI;
using Serilog;

namespace QuestPatcher.ViewModels.ModBrowser
{
    public class BrowseModViewModel: ViewModelBase
    {
        private readonly Window _window;
        private readonly Config _config;
        private readonly InstallManager _installManager;
        private readonly ExternalModManager _externalModManager;
        
        public ObservableCollection<ExternalModViewModel> Mods { get; } = new ObservableCollection<ExternalModViewModel>();
        
        public OperationLocker Locker { get; }
        public ProgressViewModel ProgressView { get; }
        
        private bool _loading = false;
        public bool Loading
        {
            get => _loading;
            private set
            {
                _loading = value;
                this.RaisePropertyChanged();
            }
        }

        public bool ShowButtons => true;
        
        public BrowseModViewModel(Window window, Config config, OperationLocker locker, ProgressViewModel progressView, InstallManager installManager, ExternalModManager externalModManager)
        {
            _window = window;
            _config = config;
            Locker = locker;
            ProgressView = progressView;
            _installManager = installManager;
            _externalModManager = externalModManager;
            
            _installManager.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(InstallManager.InstalledApp))
                {
                    if (_installManager.InstalledApp != null)
                    {
                        Task.Run(() => LoadMods(_installManager.InstalledApp));
                    }
                }
            };
        }
        
        private async Task LoadMods(ApkInfo installedApp)
        {
            Loading = true;
            Mods.Clear();
            var mods = await _externalModManager.GetAvailableMods(installedApp.Version);
            if (mods != null)
            {
                foreach (var mod in mods)
                {
                    Mods.Add(new ExternalModViewModel(mod, Locker));
                }
            }
            Loading = false;
        }

        public async Task OnInstallClicked()
        {
            try
            {
                Locker.StartOperation();
                foreach (var modVm in Mods)
                {
                    if (modVm.IsChecked)
                    {
                        // Install mod
                        var mod = modVm.Mod;
                        Log.Debug("Installing {Mod}", mod);
                        await _externalModManager.InstallMod(mod);
                        modVm.ClearSelection();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                Locker.FinishOperation();
            }
        }
    }
}
