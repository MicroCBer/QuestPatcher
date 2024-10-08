using QuestPatcher.ModBrowser.Models;
using QuestPatcher.Models;
using ReactiveUI;

namespace QuestPatcher.ViewModels.ModBrowser
{
    public class ExternalModViewModel: ViewModelBase
    {
        public ExternalMod Mod { get; }
        
        public OperationLocker Locker { get; }
        
        public string Id => Mod.Id;
        public string Name => Mod.Name;
        public string Description => Mod.Description;
        public string Author => Mod.Author;
        public string Version => Mod.VersionString;
        
        public bool IsChecked { get; set; }
        
        public ExternalModViewModel(ExternalMod mod, OperationLocker locker)
        {
            Mod = mod;
            Locker = locker;
        }
        
        public void ClearSelection()
        {
            IsChecked = false;
            this.RaisePropertyChanged(nameof(IsChecked));
        }
    }
}
