using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.ReactiveUI;

namespace LoopMusicPlayer.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        string LibsDir = AppContext.BaseDirectory + @"Libs/";
        if (Directory.Exists(LibsDir))
        {
            //Libsディレクトリがあったら実行ファイルの階層にライブラリをコピーする
            //メソッドが呼ばれた時に動的ロードする系なので、最初にコピーすればいい
            try
            {
                string osplatform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" : (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" : "linux");
                string platform = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
                var OSPlatformDirName = LibsDir + osplatform + "/";

                if(Directory.Exists(OSPlatformDirName))
                {
                    DirectoryInfo info = new DirectoryInfo(OSPlatformDirName);

                    foreach (FileInfo fileinfo in info.GetFiles())
                        fileinfo.CopyTo(AppContext.BaseDirectory + fileinfo.Name, true);
                }

                var PlatformDirName = LibsDir + osplatform + "-" + platform + "/";

                if(Directory.Exists(PlatformDirName))
                {
                    DirectoryInfo info = new DirectoryInfo(PlatformDirName);

                    foreach (FileInfo fileinfo in info.GetFiles())
                        fileinfo.CopyTo(AppContext.BaseDirectory + fileinfo.Name, true);
                }
                //コピーしたら消し、次回起動から高速化
                Directory.Delete(LibsDir, true);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }
        }

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
