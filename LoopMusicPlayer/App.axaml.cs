using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using LoopMusicPlayer.ViewModels;
using LoopMusicPlayer.Views;
using LoopMusicPlayer.Services;
using LoopMusicPlayer.Core;
using ManagedBass;

namespace LoopMusicPlayer;

public partial class App : Application
{
    public override void Initialize()
    {
        Player.Init(AppContext.BaseDirectory);

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Assets.Resources.Culture = CultureInfo.CurrentCulture;
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var model = new MainViewModel();
            var mainWindow = new MainWindow
            {
                DataContext = model
            };
            desktop.MainWindow = mainWindow;

            var services = new ServiceCollection();

            services.AddSingleton<IFilesService>(x => new FilesService(mainWindow));

            Services = services.BuildServiceProvider();

            if (desktop.Args is { Length: > 0 } args && mainWindow.Content is MainView mainView)
            {
                bool isOpened = false;
                mainView.Loaded += async (_, _) =>
                {
                    if (isOpened)
                        return;

                    isOpened = true;
                    await model.OpenFilesFromArgs(args);
                };
            }
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };

            var services = new ServiceCollection();

            services.AddSingleton<IFilesService>(x => new FilesService(singleViewPlatform.MainView));

            Services = services.BuildServiceProvider();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public new static App? Current => Application.Current as App;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider? Services { get; private set; }
}
