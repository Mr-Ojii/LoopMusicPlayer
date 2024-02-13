using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
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
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };

            var services = new ServiceCollection();

            services.AddSingleton<IFilesService>(x => new FilesService(desktop.MainWindow));

            Services = services.BuildServiceProvider();
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
