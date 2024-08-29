using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroSpeech.Positron.Platforms.Android.Core;

public class FilePickerService
{

    public static Task<string> CreateTempPathAsync(Stream stream, string name)
    {
        return Task.Run(async () => {
            var file = CreateTmpFile(name);
            using (var fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                await stream.CopyToAsync(fs);
            }
            return file;
        });
    }

    public static string CreateTmpFile(string originalName)
    {
        do
        {
            var name = originalName.SafeFileName("-" + DateTime.UtcNow.Ticks.ToString());
            var file = new FileInfo(System.IO.Path.Combine(FileSystem.CacheDirectory, name));
            if (!file.Exists)
                return file.FullName;
        } while (true);
    }


    private static string[] allFiles = new string[] { "*/*" };

    public static Dictionary<DevicePlatform, IEnumerable<string>> FileTypesFrom(IEnumerable<string>? types)
    {
        var fileTypes = new Dictionary<DevicePlatform, IEnumerable<string>>();
        if (types != null)
        {
            var accepts = new List<string>(types.Where(x => !string.IsNullOrWhiteSpace(x)));
            if (accepts.Count == 0)
            {
                accepts.Add("*/*");
            }
            fileTypes[DevicePlatform.Android] = accepts;
            fileTypes[DevicePlatform.iOS] = accepts;
        }
        else
        {
            fileTypes[DevicePlatform.Android] = allFiles;
            fileTypes[DevicePlatform.iOS] = allFiles;
        }
        return fileTypes;
    }

    public async Task<FileInfo?> PickFileAsync(string title, string? accept)
    {
        var p = await Permissions.RequestAsync<Permissions.StorageRead>();
        if (p != PermissionStatus.Granted)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Permission Denied",
                $"You must enable file permissions to upload files.\nPlease go to Settings > Apps > {AppInfo.Name} and enable file permissions",
                "Ok");
            return null;
        }


        accept ??= "*/*";

        if (string.IsNullOrWhiteSpace(accept))
        {
            accept = "*/*";
        }

        var types = FileTypesFrom(new string[] { accept });

        await Permissions.RequestAsync<Permissions.StorageRead>();
        var file = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = title,
            FileTypes = new FilePickerFileType(types)
        });
        if (file == null)
        {
            throw new TaskCanceledException();
        }
        using var s = await file.OpenReadAsync();
        var tmp = await FilePickerService.CreateTempPathAsync(s, file.FileName);
        return new FileInfo(tmp);
    }

}