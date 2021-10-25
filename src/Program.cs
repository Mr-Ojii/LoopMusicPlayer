using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Gtk;

namespace LoopMusicPlayer
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                string osplatform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" : (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" : "linux");
                string platform = Environment.Is64BitProcess ? "x64" : "x86";
                DirectoryInfo info = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"/dll/" + osplatform + "-" + platform + "/");

                //exeÇÃäKëwÇ…dllÇÉRÉsÅ[
                foreach (FileInfo fileinfo in info.GetFiles())
                {
                    fileinfo.CopyTo(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/" + fileinfo.Name, true);
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
}
