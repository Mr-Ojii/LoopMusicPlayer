using System.Threading.Tasks;
using System.Collections.Generic;
using Avalonia.Platform.Storage;
using System;

namespace LoopMusicPlayer.Services;

public interface IFilesService
{
    public Task<IReadOnlyList<IStorageFile>> OpenFileAsync();
    public Task<IStorageFile?> OpenFileAsync(Uri path);
    public Task<IStorageFile?> SaveFileAsync();
}
