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

            var OSPlatformDirName = AppContext.BaseDirectory + @"Libs/" + osplatform + "/";

            if(Directory.Exists(OSPlatformDirName))
            {
                DirectoryInfo info = new DirectoryInfo(OSPlatformDirName);

                //実行ファイルの階層にライブラリをコピー
                foreach (FileInfo fileinfo in info.GetFiles())
                {
                    fileinfo.CopyTo(AppContext.BaseDirectory + fileinfo.Name, true);
                }
            }

            var PlatformDirName = AppContext.BaseDirectory + @"Libs/" + osplatform + "-" + platform + "/";

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
