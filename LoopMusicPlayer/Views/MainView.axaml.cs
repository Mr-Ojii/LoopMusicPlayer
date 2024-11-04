using System;
using System.Reflection.Metadata;
using Avalonia.Controls;
using Avalonia.Interactivity;
using LoopMusicPlayer.ViewModels;
using LoopMusicPlayer.DataClass;

namespace LoopMusicPlayer.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        this.AddHandler(LoadedEvent, OnViewLoaded);
        this.PlayListDataGrid.AddHandler(LoadedEvent, OnDataGridLoaded);
        if (!OperatingSystem.IsAndroid())
            this.AddHandler(UnloadedEvent, OnSaveSetting);
    }
    private void OnViewLoaded(object? sender, EventArgs e)
    {
        var model = this.DataContext as MainViewModel;
        if(model is null)
            return;

        // AndroidだとUnloadEventが発火しないので、変えた瞬間保存する
        if (OperatingSystem.IsAndroid())
            model.PropertyChanged += OnSaveSetting;

        this.LoadSetting(ref model);
    }
    private void OnSaveSetting(object? sender, EventArgs e)
    {
        var model = this.DataContext as MainViewModel;
        this.SaveSetting(ref model);
    }

    public void LoadSetting(ref MainViewModel? model)
    {
        if (model is null)
            return;

        var settings = Settings.Load();
        {
            // 簡潔に書きたいが、方法を知らないので、地道に書く
            // Load Settings
            // General
            switch (settings.General.RepeatType)
            {
                case Settings.CGeneral.ERepeatType.Disable:
                    model.DisableRepeat = true;
                    break;
                case Settings.CGeneral.ERepeatType.Single:
                    model.SingleRepeat = true;
                    break;
                case Settings.CGeneral.ERepeatType.All:
                    model.AllRepeat = true;
                    break;
                case Settings.CGeneral.ERepeatType.Random:
                    model.RandomRepeat = true;
                    break;
            }
            model.EnableLoop = settings.General.EnableLoop;
            // Audio
            model.UpdatePeriod = settings.Audio.UpdatePeriod;
            model.BufferLength = settings.Audio.BufferLength;
            switch (settings.Audio.PlaybackType)
            {
                case Settings.CAudio.EPlaybackType.Streaming:
                    model.StreamingPlayback = true;
                    break;
                case Settings.CAudio.EPlaybackType.OnMemory:
                    model.OnMemoryPlayback = true;
                    break;
            }
            model.Volume = settings.Audio.Volume;
            // View
            switch(settings.View.TimeFormat)
            {
                case Settings.CView.ETimeFormat.ElapsedTime:
                    model.ElapsedTime = true;
                    break;
                case Settings.CView.ETimeFormat.SeekTime:
                    model.SeekTime = true;
                    break;
                case Settings.CView.ETimeFormat.RemainingTime:
                    model.RemainingTime = true;
                    break;
            }
            model.TopMost = settings.View.TopMost;
        }
    }

    public void SaveSetting(ref MainViewModel? model)
    {
        if (model is null)
            return;

        var settings = new Settings();
        {
            // 簡潔に書きたいが、方法を知らないので、地道に書く
            // SaveSettings
            // General
            if (model.DisableRepeat)
                settings.General.RepeatType = Settings.CGeneral.ERepeatType.Disable;
            else if (model.SingleRepeat)
                settings.General.RepeatType = Settings.CGeneral.ERepeatType.Single;
            else if (model.AllRepeat)
                settings.General.RepeatType = Settings.CGeneral.ERepeatType.All;
            else if (model.RandomRepeat)
                settings.General.RepeatType = Settings.CGeneral.ERepeatType.Random;
            settings.General.EnableLoop = model.EnableLoop;
            // Audio
            settings.Audio.UpdatePeriod = model.UpdatePeriod;
            settings.Audio.BufferLength = model.BufferLength;
            if (model.StreamingPlayback)
                settings.Audio.PlaybackType = Settings.CAudio.EPlaybackType.Streaming;
            else if (model.OnMemoryPlayback)
                settings.Audio.PlaybackType = Settings.CAudio.EPlaybackType.OnMemory;
            settings.Audio.Volume = model.Volume;
            // View
            if (model.ElapsedTime)
                settings.View.TimeFormat = Settings.CView.ETimeFormat.ElapsedTime;
            else if (model.SeekTime)
                settings.View.TimeFormat = Settings.CView.ETimeFormat.SeekTime;
            else if (model.RemainingTime)
                settings.View.TimeFormat = Settings.CView.ETimeFormat.RemainingTime;
            settings.View.TopMost = model.TopMost;
        }
        settings.Save();
    }
    public void OnDataGridLoaded(object? sender, RoutedEventArgs e)
    {
        var model = this.DataContext as MainViewModel;
        if (model is not null)
            model.OnDataGridLoaded(sender, e);
    }
}
