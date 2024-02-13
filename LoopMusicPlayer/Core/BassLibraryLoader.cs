using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BassLibraryLoader;

internal partial class CBassLibraryLoader : IDisposable
{
    private static partial class NativeMethods
    {
        [LibraryImport("libdl.so", EntryPoint = "dlopen", StringMarshalling = StringMarshalling.Utf8)]
        public static partial nint dlopen(string fileName, int flags);
        [LibraryImport("libdl.so", EntryPoint = "dlclose")]
        public static partial int dlclose(nint libraryHandle);

        [LibraryImport("libdl.so.1", EntryPoint = "dlopen", StringMarshalling = StringMarshalling.Utf8)]
        public static partial nint dlopen1(string fileName, int flags);
        [LibraryImport("libdl.so.1", EntryPoint = "dlclose")]
        public static partial int dlclose1(nint libraryHandle);

        [LibraryImport("libdl.so.2", EntryPoint = "dlopen", StringMarshalling = StringMarshalling.Utf8)]
        public static partial nint dlopen2(string fileName, int flags);
        [LibraryImport("libdl.so.2", EntryPoint = "dlclose")]
        public static partial int dlclose2(nint libraryHandle);

        public static List<Func<string, int, nint>> openFuncs = new()
        {
            dlopen,
            dlopen1,
            dlopen2,
        };
        public static List<Func<nint, int>> closeFuncs = new()
        {
            dlclose,
            dlclose1,
            dlclose2,
        };
    }

    nint libraryHandle = nint.Zero;

    public CBassLibraryLoader()
    {
        if (!OperatingSystem.IsLinux())
            return;

        foreach (var openFunc in NativeMethods.openFuncs)
        {
            try
            {
                this.libraryHandle = openFunc(AppContext.BaseDirectory + "libbass.so", 0x101);
            }
            catch
            {
                this.libraryHandle = nint.Zero;
            }
            if (this.libraryHandle != nint.Zero)
                break;
        }
    }

    public void Dispose()
    {
        if (!OperatingSystem.IsLinux() || this.libraryHandle == nint.Zero)
            return;


        foreach (var closeFunc in NativeMethods.closeFuncs)
        {
            try
            {
                if (closeFunc(this.libraryHandle) == 0)
                {
                    this.libraryHandle = nint.Zero;
                    break;
                }
            }
            catch
            {
                // 全探索したいので例外を握りつぶす
            }
        }
    }
}
