using Gtk;
using ManagedBass;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using LoopMusicPlayer.Core;
using LoopMusicPlayer.TagReaderExtensionMethods;
using UI = Gtk.Builder.ObjectAttribute;

namespace LoopMusicPlayer
{
    internal class MainWindow : Window
    {
        [UI] private Button _playbutton = null;
        [UI] private Button _pausebutton = null;
        [UI] private Button _stopbutton = null;
        [UI] private Button _previousbutton = null;
        [UI] private Button _nextbutton = null;
        [UI] private Button _ejectbutton = null;
        [UI] private VolumeButton _volumebutton = null;
        [UI] private TreeView _treeview = null;
        [UI] private ListStore _liststore = null;

        [UI] private TreeViewColumn _titlecolumn = null;
        [UI] private TreeViewColumn _timecolumn = null;
        [UI] private TreeViewColumn _loopcolumn = null;
        [UI] private TreeViewColumn _artistcolumn = null;
        [UI] private TreeViewColumn _pathcolumn = null;
        private CellRendererText TitleNameCell = null;
        private CellRendererText TimeCell = null;
        private CellRendererText LoopCell = null;
        private CellRendererText ArtistNameCell = null;
        private CellRendererText PathCell = null;


        [UI] private MenuItem _filemenu = null;
        [UI] private MenuItem _viewmenu = null;
        [UI] private MenuItem _settingmenu = null;
        [UI] private MenuItem _othermenu = null;

        [UI] private ImageMenuItem _listaddmenu = null;
        [UI] private ImageMenuItem _listdeletemenu = null;
        [UI] private ImageMenuItem _listclearmenu = null;
        [UI] private ImageMenuItem _quitmenu = null;

        [UI] private CheckMenuItem _showgridlinemenu = null;
        [UI] private RadioMenuItem _labelelpsedtimemenu = null;
        [UI] private RadioMenuItem _labelseektimemenu = null;
        [UI] private RadioMenuItem _labelremainingtimemenu = null;
        [UI] private CheckMenuItem _windowkeepabovemenu = null;


        [UI] private RadioMenuItem _singleplay = null;
        [UI] private RadioMenuItem _singlerepeat = null;
        [UI] private RadioMenuItem _allrepeat = null;
        [UI] private RadioMenuItem _randomplay = null;

        [UI] private MenuItem _preferencesmenu = null;

        [UI] private ImageMenuItem _deviceinfomenu = null;
        [UI] private ImageMenuItem _aboutmenu = null;

        [UI] private DrawingArea _seekbararea = null;

        [UI] private Label _labeltitle = null;
        [UI] private Label _labelpath = null;
        [UI] private Label _labelnowtime = null;
        [UI] private Label _labellooptime = null;
        [UI] private Label _labelloopcount = null;
        [UI] private Button _loopcountupbutton = null;
        [UI] private Button _loopcountdownbutton = null;

        private Player player = null;
        private uint LoopCount = 0;
        private bool IsEnded = false;

        private CancellationTokenSource cts = null;
        private Setting setting = null;
        private List<LanguageData> languagesdata = new List<LanguageData>();
        private LanguageData nowlang = LanguageData.Default;

        public MainWindow() : this(new Builder("MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            this.Icon = new Gdk.Pixbuf(Assembly.GetExecutingAssembly().GetManifestResourceStream("LoopMusicPlayer.icon.ico"));
            Player.Init(AppContext.BaseDirectory);

            Bass.Configure(Configuration.UpdatePeriod, 1);
            Bass.Configure(Configuration.PlaybackBufferLength, 50);

            builder.Autoconnect(this);

            _seekbararea.AddTickCallback(onframetick);

            TitleNameCell = new Gtk.CellRendererText();
            TimeCell = new Gtk.CellRendererText();
            LoopCell = new Gtk.CellRendererText();
            ArtistNameCell = new Gtk.CellRendererText();
            PathCell = new Gtk.CellRendererText();

            _titlecolumn.PackStart(TitleNameCell, true);
            _timecolumn.PackStart(TimeCell, true);
            _loopcolumn.PackStart(LoopCell, true);
            _artistcolumn.PackStart(ArtistNameCell, true);
            _pathcolumn.PackStart(PathCell, true);

            _titlecolumn.AddAttribute(TitleNameCell, "text", 0);
            _timecolumn.AddAttribute(TimeCell, "text", 1);
            _loopcolumn.AddAttribute(LoopCell, "text", 2);
            _artistcolumn.AddAttribute(ArtistNameCell, "text", 3);
            _pathcolumn.AddAttribute(PathCell, "text", 4);

            DeleteEvent += Window_DeleteEvent;

            var targets = new[] {
                new TargetEntry("text/uri-list",TargetFlags.OtherApp,0)
            };
            Drag.DestSet(this, DestDefaults.All, targets, Gdk.DragAction.Copy | Gdk.DragAction.Move);
            DragDataReceived += TreeViewDragDataReceived;
            this.AddEvents((int)(Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask));

            UpdateLoopCountLabel();
            _aboutmenu.Activated += ShowAbout;
            _deviceinfomenu.Activated += ShowDeviceInfo;
            _listaddmenu.Activated += OpenFileFromMenu;
            _seekbararea.Drawn += DrawingArea_OnDraw;
            _quitmenu.Activated += WindowQuit;
            _preferencesmenu.Activated += ShowPreferenceDialog;
            _listclearmenu.Activated += ListClear;
            _listdeletemenu.Activated += ListDelete;
            _treeview.RowActivated += ActivateLow;
            _treeview.ButtonPressEvent += ClickLow;
            _volumebutton.ValueChanged += VolumeChanged;
            _pausebutton.Clicked += PauseClicked;
            _stopbutton.Clicked += StopClicked;
            _playbutton.Clicked += PlayClicked;
            _previousbutton.Clicked += PreviousClicked;
            _nextbutton.Clicked += NextClicked;
            _ejectbutton.Clicked += EjectClicked;
            _seekbararea.AddEvents((int)(Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask));
            _seekbararea.ButtonPressEvent += SeekBarButtonPress;
            _seekbararea.ButtonReleaseEvent += SeekBarButtonRelease;
            _windowkeepabovemenu.Toggled += WindowAboveToggled;
            _showgridlinemenu.Toggled += ShowGridMenuToggled;
            _loopcountupbutton.Clicked += LoopCountUpButtonClicked;
            _loopcountdownbutton.Clicked += LoopCountDownButtonClicked;
            _singleplay.Toggled += LoopMethodToggled;
            _singlerepeat.Toggled += LoopMethodToggled;
            _allrepeat.Toggled += LoopMethodToggled;
            _randomplay.Toggled += LoopMethodToggled;
            this.Destroyed += OnWindowDestroyed;

            {
                setting = new Setting();
                _showgridlinemenu.Active = setting.SettingStruct.IsShowGridLine;
                switch (setting.SettingStruct.TimePositionShowMethod)
                {
                    case Setting.ETimePositionShowMethod.ElapsedTime:
                        _labelelpsedtimemenu.Active = true;
                        break;
                    case Setting.ETimePositionShowMethod.SeekTime:
                        _labelseektimemenu.Active = true;
                        break;
                    case Setting.ETimePositionShowMethod.RemainingTime:
                        _labelremainingtimemenu.Active = true;
                        break;
                }
                _windowkeepabovemenu.Active = setting.SettingStruct.IsWindowKeepAbove;
                switch (setting.SettingStruct.RepeatMethod)
                {
                    case Setting.ERepeatMethod.SinglePlay:
                        _singleplay.Active = true;
                        break;
                    case Setting.ERepeatMethod.SingleRepeat:
                        _singlerepeat.Active = true;
                        break;
                    case Setting.ERepeatMethod.AllRepeat:
                        _allrepeat.Active = true;
                        break;
                    case Setting.ERepeatMethod.RandomPlay:
                        _randomplay.Active = true;
                        break;
                }
                _volumebutton.Value = setting.SettingStruct.Volume;
            }

            //言語ファイルのロード
            try
            {
                DirectoryInfo info = new DirectoryInfo(AppContext.BaseDirectory + @"lang/");

                foreach (FileInfo fileinfo in info.GetFiles())
                {
                    if (fileinfo.Extension.ToLower() == ".json")
                    {
                        try
                        {
                            LanguageData data = JsonSerializer.Deserialize<LanguageData>(fileinfo.Open(FileMode.Open));
                            if(!string.IsNullOrEmpty(data.LanguageName)) {
                                languagesdata.Add(data);
                                if(setting.SettingStruct.LanguageName == data.LanguageName) {
                                    ApplyLanguage(data);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Trace.TraceError(e.ToString());
                        }
                    }
                }
                languagesdata.Sort((a, b) => a.LanguageName.CompareTo(b.LanguageName));
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }

            cts = new CancellationTokenSource();
            Task.Factory.StartNew(JudgeEndLoop);
        }

        private void ApplyLanguage(int index) {
            if (index >= 0 && index < languagesdata.Count)
                ApplyLanguage(languagesdata[index]);
            else
                ApplyLanguage(LanguageData.Default);
        }

        private void ApplyLanguage(LanguageData data)
        {
            nowlang = data;

            _filemenu.Label = (nowlang.Data.File ?? LanguageData.Default.Data.File) + "(_F)";
            _viewmenu.Label = (nowlang.Data.View ?? LanguageData.Default.Data.View) + "(_V)";
            _settingmenu.Label = (nowlang.Data.Setting ?? LanguageData.Default.Data.Setting) + "(_S)";
            _othermenu.Label = (nowlang.Data.Others ?? LanguageData.Default.Data.Others) + "(_O)";
            _listaddmenu.Label = (nowlang.Data.AddFile ?? LanguageData.Default.Data.AddFile) + "(_A)";
            _listdeletemenu.Label = (nowlang.Data.DeleteFile ?? LanguageData.Default.Data.DeleteFile) + "(_D)";
            _listclearmenu.Label = (nowlang.Data.ClearFile ?? LanguageData.Default.Data.ClearFile) + "(_C)";
            _quitmenu.Label = (nowlang.Data.Quit ?? LanguageData.Default.Data.Quit) + "(_Q)";
            _showgridlinemenu.Label = (nowlang.Data.GridLine ?? LanguageData.Default.Data.GridLine) + "(_G)";
            _labelelpsedtimemenu.Label = (nowlang.Data.ElapsedTime ?? LanguageData.Default.Data.ElapsedTime) + "(_T)";
            _labelseektimemenu.Label = (nowlang.Data.SeekTime ?? LanguageData.Default.Data.SeekTime) + "(_S)";
            _labelremainingtimemenu.Label = (nowlang.Data.RemainingTime ?? LanguageData.Default.Data.RemainingTime) + "(_R)";
            _windowkeepabovemenu.Label = (nowlang.Data.WindowOnTop ?? LanguageData.Default.Data.WindowOnTop) + "(_A)";
            _singleplay.Label = (nowlang.Data.SinglePlay ?? LanguageData.Default.Data.SinglePlay) + "(_P)";
            _singlerepeat.Label = (nowlang.Data.SingleRepeat ?? LanguageData.Default.Data.SingleRepeat) + "(_S)";
            _allrepeat.Label = (nowlang.Data.AllRepeat ?? LanguageData.Default.Data.AllRepeat) + "(_A)";
            _randomplay.Label = (nowlang.Data.RandomPlay ?? LanguageData.Default.Data.RandomPlay) + "(_R)";
            _preferencesmenu.Label = (nowlang.Data.Preference ?? LanguageData.Default.Data.Preference) + "(_F)";
            _deviceinfomenu.Label = (nowlang.Data.DeviceInfo ?? LanguageData.Default.Data.DeviceInfo) + "(_D)";
            _aboutmenu.Label = (nowlang.Data.About ?? LanguageData.Default.Data.About) + "(_A)";

            setting.SettingStruct.LanguageName = nowlang.LanguageName;
        }

        private void JudgeEndLoop()
        {
            while(!cts.IsCancellationRequested) {
                Endsita();
                Thread.Sleep(100);
            }
        }

        public void OnWindowDestroyed(object o, EventArgs args) {
            cts.Cancel();

            {
                setting.SettingStruct.IsShowGridLine = _showgridlinemenu.Active;

                if (_labelelpsedtimemenu.Active)
                {
                    setting.SettingStruct.TimePositionShowMethod = Setting.ETimePositionShowMethod.ElapsedTime;
                } else if (_labelseektimemenu.Active)
                {
                    setting.SettingStruct.TimePositionShowMethod = Setting.ETimePositionShowMethod.SeekTime;
                } else if (_labelremainingtimemenu.Active)
                {
                    setting.SettingStruct.TimePositionShowMethod = Setting.ETimePositionShowMethod.RemainingTime;
                }

                setting.SettingStruct.IsWindowKeepAbove = _windowkeepabovemenu.Active;

                if(_singleplay.Active)
                {
                    setting.SettingStruct.RepeatMethod = Setting.ERepeatMethod.SinglePlay;
                } else if(_singlerepeat.Active)
                {
                    setting.SettingStruct.RepeatMethod = Setting.ERepeatMethod.SingleRepeat;
                } else if(_allrepeat.Active)
                {
                    setting.SettingStruct.RepeatMethod = Setting.ERepeatMethod.AllRepeat;
                } else if(_randomplay.Active)
                {
                    setting.SettingStruct.RepeatMethod = Setting.ERepeatMethod.RandomPlay;
                }
                setting.SettingStruct.Volume = _volumebutton.Value;
            }

            setting.SaveJson();
        }

        private void EjectClicked(object o, EventArgs args)
        {
            this.player?.Dispose();
            this.player = null;

            this._labeltitle.Text = "";
            this._labelpath.Text = "";
            this._labellooptime.Text = "";
            this._labelnowtime.Text = "";
        }

        private void ShowGridMenuToggled(object o, EventArgs args)
        {
            this._treeview.EnableGridLines = _showgridlinemenu.Active ? TreeViewGridLines.Both : TreeViewGridLines.None;
        }

        private void WindowAboveToggled(object o, EventArgs args) 
        {
            this.KeepAbove = _windowkeepabovemenu.Active;
        }

        private void OnLoop(object o, EventArgs args)
        {
            UpdateLoopCountLabel();
        }

        private void OnEnd(object o, EventArgs args)
        {
            this.IsEnded = true;
        }

        private void Endsita()
        {
            if (this.IsEnded)
            {
                this.IsEnded = false;
                if (_randomplay.Active)
                {
                    Random rand = new Random();
                    if (_liststore.GetIterFirst(out var iter))
                    {
                        int randnum = rand.Next(0, _liststore.IterNChildren());
                        for (int i = 0; i < randnum; i++) 
                        {
                            _liststore.IterNext(ref iter);
                        }
                        CreatePlayer(_treeview.Model.GetValue(iter, 4) as string);
                        this.player?.Play();
                    }
                }
                else if (_allrepeat.Active)
                {
                    if (_liststore.GetIterFirst(out var iter))
                    {
                        bool finded = false;

                        do
                        {
                            if (this.player.FilePath == (_liststore.GetValue(iter, 4) as string))
                            {
                                finded = true;
                                break;
                            }
                        } while (_liststore.IterNext(ref iter));

                        string path;
                        if (!(finded && _liststore.IterNext(ref iter)))
                        {
                            _liststore.GetIterFirst(out iter);
                        }
                        path = _treeview.Model.GetValue(iter, 4) as string;
                        CreatePlayer(path);
                        this.player?.Play();
                    }
                }
            }
        }

        private void LoopMethodToggled(object o, EventArgs args) 
        {
            UpdateLoopFlag();
        }

        private void LoopCountUpButtonClicked(object o, EventArgs args)
        {
            if (this.LoopCount != uint.MaxValue && this.player != null)
                this.LoopCount++;
            UpdateLoopCountLabel();
        }
        
        private void LoopCountDownButtonClicked(object o, EventArgs args)
        {
            if (this.LoopCount != uint.MinValue && this.player != null)
                this.LoopCount--;
            UpdateLoopCountLabel();
        }

        private void UpdateLoopCountLabel() 
        {
            if (this.player != null)
            {
                this._labelloopcount.Text = $"{this.player.LoopCount} / {this.LoopCount}";
            }
            else
                this._labelloopcount.Text = $"";
            UpdateLoopFlag();
        }

        private void UpdateLoopFlag()
        {
            if (this.player != null)
            {
                if (this.player.SamplePosition < this.player.LoopEnd) 
                {
                    if (!_singlerepeat.Active)
                        this.player.NextIsLoop = (this.LoopCount > this.player.LoopCount);
                    else
                        this.player.NextIsLoop = true;
                }
                else
                {
                    this.player.NextIsLoop = false;
                }
            }

        }

        private void CreatePlayer(string path)
        {
            try
            {
                this.player?.Dispose();
                this.player = new Player(path, _volumebutton.Value);
                this.player.LoopAction += OnLoop;
                this.player.EndAction += OnEnd;
                UpdateLoopFlag();
            }
            catch(Exception e)
            {
                Trace.TraceError(e.ToString());
                this.player?.Dispose();
                this.player = null;

                this._labeltitle.Text = "Error occurred while loading the file.";
                this._labelpath.Text = "";
                this._labellooptime.Text = "";
                this._labelnowtime.Text = "";

                return;
            }
            this._labeltitle.Text = this.player.Title;
            if (!string.IsNullOrEmpty(this.player.Artist))
                this._labeltitle.Text = this._labeltitle.Text + " / " + this.player.Artist;
            this._labelpath.Text = this.player.FilePath;
            this._labellooptime.Text = "Looptime: " + this.player.LoopStartTime.ToString(@"hh\:mm\:ss\.ff") + " - " + this.player.LoopEndTime.ToString(@"hh\:mm\:ss\.ff");
            UpdateLoopCountLabel();
        }

        private void SeekBarButtonRelease(object o, ButtonReleaseEventArgs args)
        {
            if (this.player != null)
            {
                DrawingArea area = o as DrawingArea;
                int clickedx = (int)Math.Max(Math.Min(args.Event.X - 5, (area.AllocatedWidth - 10)), 0);

                double ratio = clickedx / (double)(area.AllocatedWidth - 10);

                this.player.Seek((long)(ratio * this.player.TotalSamples));

                if (this._singlerepeat.Active)
                    this.player.NextIsLoop = !((ratio * this.player.TotalSamples) >= this.player.LoopEnd);
                else
                        this.player.NextIsLoop = (!((ratio * this.player.TotalSamples) >= this.player.LoopEnd)) && (this.LoopCount > this.player.LoopCount);
            }
        }

        private void SeekBarButtonPress(object o, ButtonPressEventArgs args)
        {

        }

        private void PreviousClicked(object o, EventArgs args)
        {
            if (_treeview.Selection.GetSelected(out var iter))
            {
                _liststore.IterPrevious(ref iter);
                _treeview.Selection.SelectIter(iter);
            }
        }

        private void NextClicked(object o, EventArgs args)
        {
            if (_treeview.Selection.GetSelected(out var iter))
            {
                _liststore.IterNext(ref iter);
                _treeview.Selection.SelectIter(iter);
            }
        }

        private void PlayClicked(object o, EventArgs args)
        {
            if (_treeview.Selection.GetSelected(out var iter))
            {
                string path = _treeview.Model.GetValue(iter, 4) as string;
                CreatePlayer(path);
            }

            this.player?.Play();
        }

        private void StopClicked(object o, EventArgs args) 
        {
            this.player?.Stop();
        }

        private void PauseClicked(object o, EventArgs args)
        {
            this.player?.Pause();
        }

        private void VolumeChanged(object o, ValueChangedArgs args) 
        {
            player?.ChangeVolume(_volumebutton.Value);
        }

        private bool onframetick(Widget widget, Gdk.FrameClock frame_clock)
        {
            if (this.player != null)
            {
                if (!this.player.CheckDeviceEnable()) 
                {
                    this.player.Dispose();
                    this.player = null;
                    Player.Free();
                    Player.Init(AppContext.BaseDirectory);

                    this._labeltitle.Text = "Device Disconnected";
                    this._labelpath.Text = "";
                    this._labellooptime.Text = "";
                    this._labelnowtime.Text = "";

                    return true;
                }

                if (this.player.Status() == PlaybackState.Playing)
                {
                    if (this._labelseektimemenu.Active)
                        this._labelnowtime.Text = this.player.TimePosition.ToString(@"hh\:mm\:ss\.ff") + " / " + this.player.TotalTime.ToString(@"hh\:mm\:ss\.ff");
                    else if (this._labelelpsedtimemenu.Active)
                        this._labelnowtime.Text = "+" + (this.player.LoopCount * (this.player.LoopEndTime - this.player.LoopStartTime) + this.player.TimePosition).ToString(@"hh\:mm\:ss\.ff") + " / " + this.player.TotalTime.ToString(@"hh\:mm\:ss\.ff");
                    else if (this._labelremainingtimemenu.Active)
                        this._labelnowtime.Text = "-" + (this.player.TotalTime - this.player.TimePosition).ToString(@"hh\:mm\:ss\.ff") + " / " + this.player.TotalTime.ToString(@"hh\:mm\:ss\.ff");
                    _seekbararea.QueueDraw();
                }
            }
            return true;
        }

        private void TreeViewDragDataReceived(object o, DragDataReceivedArgs args)
        {
            if (args.SelectionData.Length > 0)
            {
                string motofiles = System.Text.Encoding.UTF8.GetString(args.SelectionData.Data);
                System.Text.Encoding enc = System.Text.Encoding.UTF8;
                string files = System.Web.HttpUtility.UrlDecode(motofiles, enc);
                files = files.Replace("\0", "");
                files = files.Replace("\r", "");
                string[] fileArray = files.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < fileArray.Length; i++)
                {
                    if (fileArray[i].StartsWith("file://"))
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            fileArray[i] = fileArray[i].Substring(8);
                        }
                        else
                        {
                            fileArray[i] = fileArray[i].Substring(7);
                        }
                }
                AddSongs(fileArray);
            }
        }

        private void ActivateLow(object o, RowActivatedArgs args)
        {
            _treeview.Model.GetIter(out TreeIter iter, args.Path);
            string path = _treeview.Model.GetValue(iter, 4) as string;

            CreatePlayer(path);
            this.player?.Play();
        }
        private void ClickLow(object o, ButtonPressEventArgs args)
        {
            if (args.Event.Button == 3)
            {
                Menu menu = new Menu();
                MenuItem menuItem = new MenuItem(nowlang.Data.DeleteFile ?? LanguageData.Default.Data.DeleteFile);
                menuItem.Activated += ListDelete;
                menu.Add(menuItem);
                MenuItem menuItem2 = new MenuItem(nowlang.Data.ClearFile ?? LanguageData.Default.Data.ClearFile);
                menuItem2.Activated += ListClear;
                menu.Add(menuItem2);
                menu.ShowAll();
                menu.Popup();
            }
        }

        private void ListClear(object o, EventArgs args)
        {
            _liststore.Clear();
        }

        private void ListDelete(object o, EventArgs args)
        {
            if (_treeview.Selection.GetSelected(out var iter))
            {
                _liststore.Remove(ref iter);
            }
        }

        private void ShowPreferenceDialog(object o, EventArgs args)
        {
            Window w = new Window("LoopMusicPlayer Preference");
            w.SetDefaultSize(500,100);
            w.Resizable = false;
            w.Icon = this.Icon;
            
            Grid grid = new Grid();
            grid.ColumnHomogeneous = true;
            grid.RowHomogeneous = true;
            Label langLabel = new Label("Language");
            ComboBox comboBox = new ComboBox();
            ListStore store = new ListStore(typeof(string));
            CellRendererText rendererText = new CellRendererText();

            comboBox.PackStart(rendererText, true);
            comboBox.AddAttribute(rendererText, "text", 0);
            comboBox.Model = store;
            store.AppendValues(LanguageData.Default.LanguageName);
            for(int i = 0; i < languagesdata.Count; i++) {
                store.AppendValues(languagesdata[i].LanguageName);
            }
            comboBox.Active = languagesdata.IndexOf(nowlang) + 1;
            grid.Attach(langLabel,0,0,1,1);
            grid.Attach(comboBox,1,0,1,1);
            w.Add(grid);
            w.ShowAll();
            comboBox.Changed += ComboChangedEvent;
        }

        private void ComboChangedEvent(object o, EventArgs args) {
            ComboBox c = o as ComboBox;
            ApplyLanguage(c.Active - 1);
        }
        private void WindowQuit(object o, EventArgs args)
        {
            Close();
        }

        private void DrawingArea_OnDraw(object o, DrawnArgs args)
        {
            Widget widget = o as Widget;
            Cairo.Context cr = args.Cr;
            cr.SetSourceRGB(0.9, 0.9, 0.9);
            cr.Rectangle(0, 0, widget.Allocation.Width, widget.Allocation.Height);

            cr.Fill();

            cr.SetSourceRGB(1.0, 1.0, 1.0);
            cr.Rectangle(5, 5, widget.Allocation.Width - 10, widget.Allocation.Height - 10);

            cr.Fill();

            if (this.player != null) 
            {
                if (this.player.IsLoop) 
                {
                    cr.SetSourceRGB(0.21, 0.517, 0.894);
                    cr.Rectangle((int)((widget.Allocation.Width - 10) * ((double)this.player.LoopStart / player.TotalSamples)) + 5, 5, (int)((widget.Allocation.Width - 10) * ((double)(this.player.LoopEnd - this.player.LoopStart) / player.TotalSamples)), widget.Allocation.Height - 10);
                    cr.Fill();
                }
                cr.SetSourceRGB(0, 0, 0);
                cr.Rectangle(((widget.Allocation.Width - 10) * ((double)player.SamplePosition / player.TotalSamples)), 0, 10, widget.Allocation.Height);
                cr.Fill();

                cr.SetSourceRGB(1.0, 1.0, 1.0);
                cr.Rectangle(((widget.Allocation.Width - 10) * ((double)player.SamplePosition / player.TotalSamples)) + 1, 1, 8, widget.Allocation.Height - 2);
                cr.Fill();
            }
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            this.player?.Dispose();
            Player.Free();
            Application.Quit();
        }

        private void OpenFileFromMenu(object sender, EventArgs a)
        {
            Gtk.FileChooserDialog dialog = new Gtk.FileChooserDialog("Open File", null, FileChooserAction.Open, "Open", Gtk.ResponseType.Accept, "Cancel", Gtk.ResponseType.Cancel);
            dialog.SelectMultiple = true;
            Gtk.FileFilter filter = new Gtk.FileFilter();
            filter.Name = "ogg File";
            filter.AddPattern("*.ogg");
            dialog.AddFilter(filter);
            filter = new Gtk.FileFilter();
            filter.Name = "All File";
            filter.AddPattern("*");
            dialog.AddFilter(filter);
            if (dialog.Run() == (int)ResponseType.Accept)
            {
                AddSongs(dialog.Filenames);
            }
            dialog.Destroy();
        }

        private void AddSongs(string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                if (File.Exists(paths[i]))
                {
                    try
                    {
                        using (var ii = new Player(paths[i], 1.0))
                        {
                            string title = !string.IsNullOrEmpty(ii.Tags.Title) ? ii.Tags.Title : System.IO.Path.GetFileName(paths[i]);
                            string time = ii.TotalTime.ToString();
                            string artist = !string.IsNullOrEmpty(ii.Tags.Artist) ? ii.Tags.Artist : "";
                            string loop = !string.IsNullOrEmpty(ii.Tags.GetTag("LOOPSTART")) && (!string.IsNullOrEmpty(ii.Tags.GetTag("LOOPLENGTH")) || !string.IsNullOrEmpty(ii.Tags.GetTag("LOOPEND"))) ? "Loop" : "";
                            string path = paths[i];

                            _liststore.AppendValues(title, time, loop, artist, path);
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError(e.ToString());
                    }
                }
            }
        }

        private void ShowDeviceInfo(object sender, EventArgs a)
        {
            if (!Bass.GetDeviceInfo(Bass.CurrentDevice, out var device_info))
                using (var dia = new MessageDialog(this, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, false, $"Failed to get DeviceInfo."))
                    dia.Run();
                
            if (!Bass.GetInfo(out var info))
                using (var dia = new MessageDialog(this, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, false, $"Failed to get BassInfo."))
                    dia.Run();

            using (var dia = new MessageDialog(this, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, false,
                    $"Name: {device_info.Name}\n" +
                    $"Frequency: {info.SampleRate}\n" +
                    $"Latency: {info.Latency}\n" +
                    $"SpeakerCount: {info.SpeakerCount}"))
            {
                dia.KeepAbove = true;
                dia.Run();
            }
        }

        private void ShowAbout(object sender, EventArgs a)
        {
            using (var dia = new AboutDialog())
            {
                dia.Icon = this.Icon;
                dia.Logo = this.Icon;
                dia.Documenters = new string[] { "Mr-Ojii" };
                dia.Authors = new string[] { "Mr-Ojii" };
                dia.LicenseType = License.MitX11;
                dia.Version = $"LoopMusicPlayer {Assembly.GetExecutingAssembly().GetName().Version}\n" +
                    $"{RuntimeInformation.FrameworkDescription}\n" +
                    $"BASS {Bass.Version}";
                dia.Title = "About LoopMusicPlayer";
                dia.ProgramName = "LoopMusicPlayer";
                dia.Copyright = "(c) 2021-2022  Mr-Ojii";
                dia.KeepAbove = true;
                dia.Run();
            }
        }
    }
}
