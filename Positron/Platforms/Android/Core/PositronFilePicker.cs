using Android.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using AndroidUri = Android.Net.Uri;
using Microsoft.Maui.ApplicationModel;
using Android.Provider;



namespace Positron.Platforms.Android.Core;

static class PlatformUtils
{
    internal const int requestCodeFilePicker = 11001;
    internal const int requestCodeMediaPicker = 11002;
    internal const int requestCodeMediaCapture = 11003;
    internal const int requestCodePickContact = 11004;

    internal const int requestCodeStart = 12000;
}

    public static class FileMimeTypes
{
    internal const string All = "*/*";

    internal const string ImageAll = "image/*";
    internal const string ImagePng = "image/png";
    internal const string ImageJpg = "image/jpeg";

    internal const string VideoAll = "video/*";

    internal const string EmailMessage = "message/rfc822";

    internal const string Pdf = "application/pdf";

    internal const string TextPlain = "text/plain";

    internal const string OctetStream = "application/octet-stream";
}

internal class PositronFilePicker
{
    public static async Task<List<string>> PlatformPickAsync(PickOptions options, bool allowMultiple = false)
    {
        // Essentials supports >= API 19 where this action is available
        var action = Intent.ActionOpenDocument;

        var intent = new Intent(action);
        intent.SetType(FileMimeTypes.All);
        intent.PutExtra(Intent.ExtraAllowMultiple, allowMultiple);

        var allowedTypes = options?.FileTypes?.Value?.ToArray();
        if (allowedTypes?.Length > 0)
            intent.PutExtra(Intent.ExtraMimeTypes, allowedTypes);

        var pickerIntent = Intent.CreateChooser(intent, options?.PickerTitle ?? "Select file");

        try
        {
            var resultList = new List<string>();

            var result = await IntermediateActivity.StartAsync(pickerIntent, PlatformUtils.requestCodeFilePicker);

            if (result.ClipData == null)
            {
                var path = await FileSystemUtils.EnsurePhysicalPath(result.Data);
                resultList.Add(path);
            }
            else
            {
                for (var i = 0; i < result.ClipData.ItemCount; i++)
                {
                    var uri = result.ClipData.GetItemAt(i).Uri;
                    var path = await FileSystemUtils.EnsurePhysicalPath(uri);
                    resultList.Add(path);
                }
            }


            return resultList;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }
}
