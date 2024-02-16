using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Reactive;
using System.Web;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Platform.Storage;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LoopMusicPlayer.DataClass;
using LoopMusicPlayer.Services;
using LoopMusicPlayer.Core;
using ManagedBass;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Xml.Schema;
using System.Linq;
using System.IO;

namespace LoopMusicPlayer.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    // Player

    [ObservableProperty] public string _title = "";
    [ObservableProperty] public string _playTime = "";
    [ObservableProperty] public string _loopTime = "";
    [ObservableProperty] public string _filePath = "";
    [ObservableProperty] public string _loopCount = $"0 / 0";
    private bool isEnded = false;
    private bool isLooped = false;

    private void setNewLoopCount(object? sender, EventArgs e)
    {
        if (this.Player is null)
        {
            this.LoopCount = $"0 / {this._max_loop}";
            return;
        }

        this.LoopCount = $"{this.Player.LoopCount} / {this._max_loop}";

        if ((this._max_loop != 0 && this._max_loop <= this.Player.LoopCount) || !this.EnableLoop || this.Player.SamplePosition > this.Player.LoopEnd)
            this.Player.NextIsLoop = false;
        else
            this.Player.NextIsLoop = true;
    }

    private void OnEnd(object? sender, EventArgs e)
    {
        this.isEnded = true;
    }
    private void OnLoop(object? sender, EventArgs e)
    {
        this.isLooped = true;
    }

    private uint _max_loop = 0;


    [ObservableProperty] public double _volume = 1.0;

    public ObservableCollection<PlayListItem> PlayList { get; set; }
    public ObservableCollection<string> Errors { get; set; }

    [ObservableProperty] public double _progress = -1;

    [ObservableProperty] public double _loopStart = -1;
    [ObservableProperty] public double _loopEnd = -1;

    [ObservableProperty] public Player? _player = null;


    // Option
    [ObservableProperty] public bool _elapsedTime = false;
    [ObservableProperty] public bool _seekTime = false;
    [ObservableProperty] public bool _remainingTime = false;

    [ObservableProperty] private bool _topMost = false;

    [ObservableProperty] public bool _disableRepeat = false;
    [ObservableProperty] public bool _singleRepeat = false;
    [ObservableProperty] public bool _allRepeat = false;
    [ObservableProperty] public bool _randomRepeat = false;

    [ObservableProperty] public bool _enableLoop = true;

    [ObservableProperty] public int _updatePeriod = 100;
    [ObservableProperty] public int _bufferLength = 500;

    // Info
    private DeviceInfo device_info;
    private BassInfo bass_info;

    public string DeviceName => device_info.Name;
    public int DeviceFrequency => bass_info.SampleRate;
    public int DeviceLatency => bass_info.Latency;
    public int SpeakerCount => bass_info.SpeakerCount;

    // ThirdPartyLicenses

    public ObservableCollection<LicenseItem> LicenseList { get; set; }

    // About
    public string AppName => $"{Assembly.GetExecutingAssembly().GetName().Name}";
    public string Version => $"{Assembly.GetExecutingAssembly().GetName().Version}";
    public string ManagedBassVersion => $"{typeof(Bass).Assembly.GetName().Version?.ToString(3)}";
    public string BASSVersion => $"{Bass.Version}";
    public string AvaloniaVersion => $"{typeof(AvaloniaObject).Assembly.GetName().Version?.ToString(3)}";
    public string DotnetVersion => RuntimeInformation.FrameworkDescription.Substring(5); //".NET "を切る
    public string Copyright => "(c) 2021-2024 Mr-Ojii";



    private int _playingIndex = 0;


    private DispatcherTimer dispatcherTimer;
    private DataGrid? dataGrid = null;

    public MainViewModel()
    {
        PlayList = new ObservableCollection<PlayListItem>();
        Errors = new ObservableCollection<string>();
        dispatcherTimer = new(TimeSpan.FromMilliseconds(100), DispatcherPriority.Normal, DrawCall);
        dispatcherTimer.Start();
        PropertyChanged += CheckPropertyChanged;

        // ライセンスリストを構築
        LicenseList = new ObservableCollection<LicenseItem>();
        var licenseNamespace = "LoopMusicPlayer.ThirdPartyLicenses.";
        var licenseNames = typeof(MainViewModel).Assembly.GetManifestResourceNames().Where(s => s.StartsWith(licenseNamespace));
        foreach (var ln in licenseNames)
        {
            var stream = typeof(MainViewModel).Assembly.GetManifestResourceStream(ln);
            if (stream is null)
                continue;

            // namespaceと拡張子を削除
            string name = ln.Substring(licenseNamespace.Length);
            name = name.Substring(0, name.LastIndexOf("."));
            using (var sr = new StreamReader(stream))
            {
                LicenseList.Add(new LicenseItem(
                    name,
                    sr.ReadToEnd()
                ));
            }
        }
        //


        if (!Bass.GetDeviceInfo(Bass.CurrentDevice, out device_info))
            return;
        if (!Bass.GetInfo(out bass_info))
            return;
    }

    private void CheckPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(EnableLoop):
                this.setNewLoopCount(sender, e);
                break;
            case nameof(UpdatePeriod):
                Bass.Configure(Configuration.UpdatePeriod, this.UpdatePeriod);
                break;
            case nameof(BufferLength):
                Bass.Configure(Configuration.PlaybackBufferLength, this.BufferLength);
                break;
        }
    }

    private async void DrawCall(object? sender, EventArgs e)
    {
        if (this.Player is not null)
        {
            if (this.SeekTime)
                this.PlayTime = this.Player.TimePosition.ToString(@"hh\:mm\:ss\.ff") + " / " + this.Player.TotalTime.ToString(@"hh\:mm\:ss\.ff");
            else if (this.ElapsedTime)
                this.PlayTime = "+" + (this.Player.LoopCount * (this.Player.LoopEndTime - this.Player.LoopStartTime) + this.Player.TimePosition).ToString(@"hh\:mm\:ss\.ff") + " / " + this.Player.TotalTime.ToString(@"hh\:mm\:ss\.ff");
            else if (this.RemainingTime)
                this.PlayTime = "-" + (this.Player.TotalTime - this.Player.TimePosition).ToString(@"hh\:mm\:ss\.ff") + " / " + this.Player.TotalTime.ToString(@"hh\:mm\:ss\.ff");
            this.Progress = this.Player.SamplePosition / (double)this.Player.TotalSamples;
        }
        if (this.isEnded)
        {
            // CallBack内で次の曲にするとおかしくなるので、DrawCall内で次の曲にする

            if (this.SingleRepeat)
                await UpdatePlayer(this.PlayList[this._playingIndex].File);
            if (this.AllRepeat)
                await Next();
            else if (this.RandomRepeat)
                await RandomChoice();

            this.isEnded = false;
        }
        if (this.isLooped)
        {
            // CallBackでループ処理するとおかしくなるので、DrawCall内で
            this.setNewLoopCount(this, EventArgs.Empty);
            this.isLooped = false;
        }
    }

    private static FilePickerFileType Audio { get; } = new("Supported Audio")
    {
        Patterns = new[] { "*.ogg", "*.mp3", "*.opus", "*.flac", "*.wv" },
        AppleUniformTypeIdentifiers = new[] { "public.audio" },
        MimeTypes = new[] { "audio/*" }
    };

    [RelayCommand]
    private async Task OpenFile(CancellationToken token)
    {
        var filesService = GetFilesService();

        var files = await filesService.OpenFileAsync();
        if (files is null) return;

        foreach (var file in files)
        {
            try
            {
                await AddToPlayList(file);
            }
            catch (Exception e)
            {
                Errors.Add(e.ToString());
            }
        }
    }
    private string FileToPath(IStorageFile file)
    {
        if (OperatingSystem.IsAndroid())
        {
            return HttpUtility.UrlDecode(file.Path.AbsoluteUri);
        }
        else
        {
            return HttpUtility.UrlDecode(file.Path.AbsolutePath);
        }
    }

    partial void OnVolumeChanged(double value)
        => this.Player?.ChangeVolume(this.Volume);

    public async Task AddToPlayList(IStorageFile file)
    {
        string path = FileToPath(file);
        using (var stream = await file.OpenReadAsync())
        {
            using (var ii = new Player(path, 1.0, stream))
            {
                string title = !string.IsNullOrEmpty(ii.Title) ? ii.Title : System.IO.Path.GetFileName(path);
                string time = ii.TotalTime.ToString();
                string artist = !string.IsNullOrEmpty(ii.Artist) ? ii.Artist : "";
                bool loop = ii.IsLoop;

                PlayList.Add(new PlayListItem(title, time, artist, loop, path, file));
            }
        }
    }

    public async Task UpdatePlayer(IStorageFile file)
    {
        var stream = await file.OpenReadAsync();
        var path = FileToPath(file);
        bool playing = true;
        if (this.Player is not null)
            playing = this.Player.Status() == PlaybackState.Playing;
        this.Player?.Dispose();
        this.LoopStart = -1;
        this.LoopEnd = -1;
        this.Player = new Player(path, this.Volume, stream);
        if (!string.IsNullOrEmpty(this.Player.Artist))
            this.Title = this.Player.Title + " / " + this.Player.Artist;
        else
            this.Title = this.Player.Title;
        this.FilePath = path;
        this.LoopTime = "Looptime: " + this.Player.LoopStartTime.ToString(@"hh\:mm\:ss\.ff") + " - " + this.Player.LoopEndTime.ToString(@"hh\:mm\:ss\.ff");
        if (this.Player.IsLoop)
        {
            this.LoopStart = this.Player.LoopStart / (double)this.Player.TotalSamples;
            this.LoopEnd = this.Player.LoopEnd /(double)this.Player.TotalSamples;
        }
        this.Player.EndAction += this.OnEnd;
        this.Player.LoopAction += this.OnLoop;
        this.setNewLoopCount(this, EventArgs.Empty);
        if (playing)
            this.Player.Play();
    }

    [RelayCommand]
    private void Play() => this.Player?.Play();
    [RelayCommand]
    private void Pause() => this.Player?.Pause();
    [RelayCommand]
    private void Stop() => this.Player?.Stop();

    [RelayCommand]
    private async Task Prev()
    {
        if (this.PlayList.Count == 0)
            return;

        this._playingIndex = (this._playingIndex + this.PlayList.Count - 1) % this.PlayList.Count;

        await UpdatePlayer(this.PlayList[this._playingIndex].File);
    }

    [RelayCommand]
    private async Task Next()
    {
        if (this.PlayList.Count == 0)
            return;

        this._playingIndex = (this._playingIndex + this.PlayList.Count + 1) % this.PlayList.Count;

        await UpdatePlayer(this.PlayList[this._playingIndex].File);
    }

    [RelayCommand]
    private async Task RandomChoice()
    {
        if (this.PlayList.Count == 0)
            return;

        this._playingIndex = Random.Shared.Next(this.PlayList.Count);

        await UpdatePlayer(this.PlayList[this._playingIndex].File);
    }

    [RelayCommand]
    private void SkipM5()
    {
        if(this.Player is null)
            return;

        long to = Math.Clamp(this.Player.SamplePosition - 5 * this.Player.SampleRate, 0, this.Player.TotalSamples - 1);

        this.Player.Pause();
        this.Player.Seek(to);
        this.Player.Pause();
    }

    [RelayCommand]
    private void SkipP5()
    {
        if(this.Player is null)
            return;

        long to = Math.Clamp(this.Player.SamplePosition + 5 * this.Player.SampleRate, 0, this.Player.TotalSamples - 1);

        this.Player.Pause();
        this.Player.Seek(to);
        this.Player.Pause();
    }

    [RelayCommand]
    private void Delete()
    {
        if (this.dataGrid is null)
            return;

        int _selectedIndex = this.dataGrid.SelectedIndex;

        if (this._playingIndex >= _selectedIndex)
            _playingIndex--;

        this.PlayList.RemoveAt(_selectedIndex);
    }

    [RelayCommand]
    private void SongUp()
    {
        if (this.dataGrid is null)
            return;

        int _selectedIndex = this.dataGrid.SelectedIndex;
        if (_selectedIndex == 0)
            return;

        (this.PlayList[_selectedIndex], this.PlayList[_selectedIndex - 1]) = (this.PlayList[_selectedIndex - 1], this.PlayList[_selectedIndex]);

        if (this._playingIndex == _selectedIndex)
            this._playingIndex -= 1;
        else if(this._playingIndex == _selectedIndex - 1)
            this._playingIndex += 1;
    }
    [RelayCommand]
    private void SongDown()
    {
        if (this.dataGrid is null)
            return;

        int _selectedIndex = this.dataGrid.SelectedIndex;
        if (_selectedIndex == this.PlayList.Count - 1)
            return;

        (this.PlayList[_selectedIndex], this.PlayList[_selectedIndex + 1]) = (this.PlayList[_selectedIndex + 1], this.PlayList[_selectedIndex]);

        if (this._playingIndex == _selectedIndex)
            this._playingIndex += 1;
        else if(this._playingIndex == _selectedIndex + 1)
            this._playingIndex -= 1;
    }

    [RelayCommand]
    private void Clear()
    {
        this.PlayList.Clear();
    }

    [RelayCommand]
    private void MaxLoopIncrement()
    {
        if (this._max_loop != uint.MaxValue)
            this._max_loop++;
        this.setNewLoopCount(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void MaxLoopDecrement()
    {
        if (this._max_loop != uint.MinValue)
            this._max_loop--;
        this.setNewLoopCount(this, EventArgs.Empty);
    }

    public void OnDataGridLoaded(object? sender, RoutedEventArgs e)
    {
        var dg = sender as DataGrid;
        if (dg is not null)
        {
            dg.LoadingRow += OnLoadingRow;
        }
        this.dataGrid = dg;
    }

    public void OnLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        if (e is not null)
            e.Row.DoubleTapped += OnDoubleTapped;
    }

    public async void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        var row = sender as DataGridRow;
        if (row is null)
            return;
        var item = row.DataContext as PlayListItem;
        if (item is null)
            return;

        this._playingIndex = this.PlayList.IndexOf(item);

        await UpdatePlayer(item.File);
    }

    public async void DropCommandHandler(IEnumerable<IStorageItem> files)
    {
        foreach (var file in files)
        {
            try
            {
                var filesService = this.GetFilesService();
                IStorageFile? s_file = await filesService.OpenFileAsync(file.Path);
                if (s_file is not null)
                    await AddToPlayList(s_file);
            }
            catch (Exception e)
            {
                Errors.Add(e.ToString());
            }
        }
    }

    private IFilesService GetFilesService()
    {
        var filesService = (IFilesService?)App.Current?.Services?.GetService(typeof(IFilesService));
        if (filesService is null)
            throw new NullReferenceException("Missing File Service instance.");

        return filesService;
    }
}
