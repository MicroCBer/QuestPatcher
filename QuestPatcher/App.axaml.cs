using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using QuestPatcher.Core;
using QuestPatcher.Services;
using QuestPatcher.Views;
using Serilog;

namespace QuestPatcher
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnAppDomainUnhandledException(object? sender, UnhandledExceptionEventArgs args)
        {
            if(!args.IsTerminating)
            {
                return;
            }
            
            Log.Error($"Unhandled exception, QuestPatcher quitting!: {args.ExceptionObject}");
            Log.CloseAndFlush();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
                
                try
                {
                    QuestPatcherService questPatcherService = new QuestPatcherUIService(desktop);
                    desktop.Exit += (_, _) =>
                    {
                        questPatcherService.CleanUp();
                    };
                }
                catch (Exception ex)
                {
                    DialogBuilder dialog = new()
                    {
                        Title = "Critical Error",
                        Text = "QuestPatcher encountered a critical error during early startup, which was unrecoverable.",
                        HideCancelButton = true
                    };
                    dialog.WithException(ex);
                    dialog.OpenDialogue(null, WindowStartupLocation.CenterScreen);
                }
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
