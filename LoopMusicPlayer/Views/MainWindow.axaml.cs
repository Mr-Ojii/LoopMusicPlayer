using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using LoopMusicPlayer.ViewModels;

namespace LoopMusicPlayer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.AddHandler(DragDrop.DropEvent, OnDrop);
        this.AddHandler(LoadedEvent, OnWindowLoaded);
        this.AddHandler(UnloadedEvent, OnWindowUnloaded);
    }

    private void OnWindowLoaded(object? sender, EventArgs e)
    {
        var viewModel = this.DataContext as MainViewModel;
        if (viewModel is not null)
            viewModel.PropertyChanged += OnChangeTopmost;
    }
    private void OnWindowUnloaded(object? sender, EventArgs e)
    {
        var viewModel = this.DataContext as MainViewModel;
        if (viewModel is not null)
            viewModel.OnWindowUnloaded(sender, e);
    }

    private void OnDrop(object? sender, DragEventArgs e) {
        var files = e.Data.GetFiles();
        if (this.DataContext is not null && files is not null)
            ((MainViewModel)this.DataContext).DropCommandHandler(files);
    }

    private void OnChangeTopmost(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != "TopMost")
            return;

        var viewModel = this.DataContext as MainViewModel;
        if (viewModel is not null)
            this.Topmost = viewModel.TopMost;
    }
}
