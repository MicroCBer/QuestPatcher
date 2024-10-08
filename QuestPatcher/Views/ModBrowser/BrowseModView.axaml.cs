using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace QuestPatcher.Views.ModBrowser
{
    public partial class BrowseModView : UserControl
    {
        public BrowseModView()
        {
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

