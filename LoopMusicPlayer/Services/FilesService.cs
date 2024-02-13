using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace LoopMusicPlayer.Services;

public class FilesService : IFilesService
{
    private readonly Control _target;

    public FilesService(Control target)
    {
        _target = target;
    }

    public async Task<IReadOnlyList<IStorageFile>> OpenFileAsync()
    {
        var topLevel = TopLevel.GetTopLevel(_target);

        if (topLevel is null)
            return new List<IStorageFile>();

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Open File",
            AllowMultiple = true
        });

        return files;
    }

    public async Task<IStorageFile?> SaveFileAsync()
    {
        var topLevel = TopLevel.GetTopLevel(_target);

        if (topLevel is null)
            return null;

        return await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Save File"
        });
    }

    public async Task<IStorageFile?> OpenFileAsync(Uri path)
    {
        var topLevel = TopLevel.GetTopLevel(_target);

        if (topLevel is null)
            return null;

        return await topLevel.StorageProvider.TryGetFileFromPathAsync(path);
    }
}
