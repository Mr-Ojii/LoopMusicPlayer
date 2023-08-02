using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Gtk;

namespace LoopMusicPlayer;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            string osplatform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" : (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" : "linux");
            string platform = "";
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X86:
                    platform = "x86";
                    break;
                case Architecture.X64:
                    platform = "x64";
                    break;
                case Architecture.Arm:
                    platform = "arm";
                    break;
                case Architecture.Arm64:
                    platform = "arm64";
                    break;
                default:
                    throw new PlatformNotSupportedException();
            }

            var OSPlatformDirName = AppContext.BaseDirectory + @"dll/" + osplatform + "/";

            if(Directory.Exists(OSPlatformDirName))
            {
                DirectoryInfo info = new DirectoryInfo(OSPlatformDirName);

                //実行ファイルの階層にライブラリをコピー
                foreach (FileInfo fileinfo in info.GetFiles())
                {
                    fileinfo.CopyTo(AppContext.BaseDirectory + fileinfo.Name, true);
                }
            }

            var PlatformDirName = AppContext.BaseDirectory + @"dll/" + osplatform + "-" + platform + "/";

            if(Directory.Exists(PlatformDirName))
            {
                DirectoryInfo info = new DirectoryInfo(PlatformDirName);

                //実行ファイルの階層にライブラリをコピー
                foreach (FileInfo fileinfo in info.GetFiles())
                {
                    fileinfo.CopyTo(AppContext.BaseDirectory + fileinfo.Name, true);
                }
            }
        }
        catch (Exception e)
        {
            Trace.TraceError(e.ToString());
        }

        Application.Init();

        var app = new Application("Mr-Ojii.LoopMusicPlayer.LoopMusicPlayer", GLib.ApplicationFlags.None);
        app.Register(GLib.Cancellable.Current);

        var win = new MainWindow();
        app.AddWindow(win);

        win.Show();
        Application.Run();
    }
}
