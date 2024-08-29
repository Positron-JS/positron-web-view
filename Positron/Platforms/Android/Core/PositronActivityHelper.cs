using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Webkit;
using AndroidUri = Android.Net.Uri;
using Debug = System.Diagnostics.Debug;
using Intent = Android.Content.Intent;

using MauiApp = Microsoft.Maui.Controls.Application;

namespace Positron.Platforms.Android.Core;

public class HybridActivityHelper
{
    internal static string Authority => Platform.AppContext.PackageName + ".fileProvider";

    internal static AndroidUri GetUriForFile(Java.IO.File file) =>
        FileProvider.GetUriForFile(Platform.AppContext, Authority, file);

    internal const string EssentialsFolderHash = "2203693cc04e0be7f4f024d5f9499e13";


    const string storageTypePrimary = "primary";
    const string storageTypeRaw = "raw";
    const string storageTypeImage = "image";
    const string storageTypeVideo = "video";
    const string storageTypeAudio = "audio";
    static readonly string[] contentUriPrefixes =
    {
            "content://downloads/public_downloads",
            "content://downloads/my_downloads",
            "content://downloads/all_downloads",
        };
    internal const string UriSchemeFile = "file";
    internal const string UriSchemeContent = "content";

    internal const string UriAuthorityExternalStorage = "com.android.externalstorage.documents";
    internal const string UriAuthorityDownloads = "com.android.providers.downloads.documents";
    internal const string UriAuthorityMedia = "com.android.providers.media.documents";

    public const int PickFileRequestCode = 1011;
    public const int CaptureFileRequestCode = 1012;

    public static async Task<List<string>> PickFileAsync(
        string title = "Pick Document",
        bool allowMultiple = false,
        bool capture = false,
        params string[] mimeTypes
    )
    {

        if (capture)
        {
            return await CaptureFileAsync(mimeTypes);
        }

        // check if only image...
        var intent = await CreateIntent(title, allowMultiple, mimeTypes);

        var result = await IntermediateActivity.StartAsync(intent, CaptureFileRequestCode);

        if (result == null)
        {
            throw new TaskCanceledException();
        }

        var firstMime = mimeTypes.FirstOrDefault();
        if (firstMime != null && firstMime.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return await MediaHelper.Instance.SaveImageToFile(result);

        }

        return CreateResult(result);
    }

    private static async Task<List<string>> CaptureFileAsync(string[] mimeTypes)
    {
        var task = new TaskCompletionSource<Task<List<string>>>();

        var tmpName = "file.jpg";

        var action = MediaStore.ActionImageCapture;

        if (mimeTypes.Length > 0)
        {
            var first = mimeTypes[0];
            if (first.StartsWith("video/"))
            {
                tmpName = "file.mp4";
                action = MediaStore.ActionVideoCapture;
            }
            else if (first.StartsWith("audio/"))
            {
                tmpName = "file.mp4";
                action = MediaStore.ActionVideoCapture;
            }
        }
        var tmpFile = FilePickerService.CreateTmpFile(tmpName);

        var intent = new Intent(action);
        intent.AddFlags(ActivityFlags.GrantReadUriPermission);
        intent.AddFlags(ActivityFlags.GrantWriteUriPermission);
        var outputUri = GetUriForFile(new Java.IO.File(tmpFile));
        intent.PutExtra(MediaStore.ExtraOutput, outputUri);

        var result = await IntermediateActivity.StartAsync(intent, CaptureFileRequestCode);

        if (result ==  null)
        {
            throw new TaskCanceledException();
        }

        return CreateResult(result);
    }

    private static async Task<Intent> CreateIntent(string title, bool allowMultiple, string[] mimeTypes)
    {
        Intent intent;
        var action = Intent.ActionOpenDocument;

        if (mimeTypes.Length == 1 && mimeTypes[0].StartsWith("image/"))
        {


            // pick image...
            action = Intent.ActionGetContent;
            intent = new Intent(action);
            intent.SetType("image/*");
            return Intent.CreateChooser(intent, title);

        }

        if (mimeTypes.Length == 1 && mimeTypes[0].StartsWith("video/"))
        {
            // pick image...
            action = Intent.ActionGetContent;
            intent = new Intent(action);
            intent.SetType("video/*");
            return Intent.CreateChooser(intent, title);

        }

        intent = new Intent(action);
        intent.SetType("*/*");
        intent.PutExtra(Intent.ExtraAllowMultiple, allowMultiple);
        if (title != null)
        {
            intent.PutExtra(Intent.ExtraTitle, title);
        }
        if (mimeTypes?.Length > 0)
        {
            intent.PutExtra(Intent.ExtraMimeTypes, mimeTypes.ToArray());
        }

        return intent;
    }

    private static List<string> CreateResult(Intent intent)
    {
        var resultList = new List<string>();
        if (intent.ClipData == null)
        {
            var path = EnsurePhysicalPath(intent.Data);
            resultList.Add(path);
        }
        else
        {
            for (var i = 0; i < intent.ClipData.ItemCount; i++)
            {
                var uri = intent.ClipData.GetItemAt(i).Uri;
                var path = EnsurePhysicalPath(uri);
                resultList.Add(path);
            }
        }
        return resultList;
    }

    private static string EnsurePhysicalPath(AndroidUri uri, bool requireExtendedAccess = true)
    {
        // if this is a file, use that
        if (uri.Scheme.Equals(UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            return uri.Path;

        // try resolve using the content provider
        var absolute = ResolvePhysicalPath(uri, requireExtendedAccess);
        if (!string.IsNullOrWhiteSpace(absolute) && Path.IsPathRooted(absolute))
            return absolute;

        // fall back to just copying it
        var cached = CacheContentFile(uri);
        if (!string.IsNullOrWhiteSpace(cached) && Path.IsPathRooted(cached))
            return cached;

        throw new FileNotFoundException($"Unable to resolve absolute path or retrieve contents of URI '{uri}'.");
    }

    static string CacheContentFile(AndroidUri uri)
    {
        if (!uri.Scheme.Equals(UriSchemeContent, StringComparison.OrdinalIgnoreCase))
            return null;

        Debug.WriteLine($"Copying content URI to local cache: '{uri}'");

        // open the source stream
        using var srcStream = OpenContentStream(uri, out var extension);
        if (srcStream == null)
            return null;

        // resolve or generate a valid destination path
        var filename = GetColumnValue(uri, MediaStore.Files.FileColumns.DisplayName) ?? Guid.NewGuid().ToString("N");
        if (!Path.HasExtension(filename) && !string.IsNullOrEmpty(extension))
            filename = Path.ChangeExtension(filename, extension);

        // create a temporary file
        var hasPermission = Permissions.IsDeclaredInManifest(global::Android.Manifest.Permission.WriteExternalStorage);
        var root = hasPermission
            ? Platform.AppContext.ExternalCacheDir
            : Platform.AppContext.CacheDir;
        var tmpFile = GetEssentialsTemporaryFile(root, filename);

        // copy to the destination
        using var dstStream = File.Create(tmpFile.CanonicalPath);
        srcStream.CopyTo(dstStream);

        return tmpFile.CanonicalPath;
    }


    internal static Java.IO.File GetEssentialsTemporaryFile(Java.IO.File root, string fileName)
    {
        // create the directory for all Essentials files
        var rootDir = new Java.IO.File(root, EssentialsFolderHash);
        rootDir.Mkdirs();
        rootDir.DeleteOnExit();

        // create a unique directory just in case there are multiple file with the same name
        var tmpDir = new Java.IO.File(rootDir, Guid.NewGuid().ToString("N"));
        tmpDir.Mkdirs();
        tmpDir.DeleteOnExit();

        // create the new temporary file
        var tmpFile = new Java.IO.File(tmpDir, fileName);
        tmpFile.DeleteOnExit();

        return tmpFile;
    }


    static Stream OpenContentStream(AndroidUri uri, out string extension)
    {
        var isVirtual = IsVirtualFile(uri);
        if (isVirtual)
        {
            Debug.WriteLine($"Content URI was virtual: '{uri}'");
            return GetVirtualFileStream(uri, out extension);
        }

        extension = GetFileExtension(uri);
        return Platform.AppContext.ContentResolver.OpenInputStream(uri);
    }

    static Stream GetVirtualFileStream(AndroidUri uri, out string extension)
    {
        var mimeTypes = Platform.AppContext.ContentResolver.GetStreamTypes(uri, "*/*");
        if (mimeTypes?.Length >= 1)
        {
            var mimeType = mimeTypes[0];

            var stream = Platform.AppContext.ContentResolver
                .OpenTypedAssetFileDescriptor(uri, mimeType, null)
                .CreateInputStream();

            extension = MimeTypeMap.Singleton.GetExtensionFromMimeType(mimeType);

            return stream;
        }

        extension = null;
        return null;
    }

    static bool IsVirtualFile(AndroidUri uri)
    {
        if (!DocumentsContract.IsDocumentUri(Platform.AppContext, uri))
            return false;

        var value = GetColumnValue(uri, DocumentsContract.Document.ColumnFlags);
        if (!string.IsNullOrEmpty(value) && int.TryParse(value, out var flagsInt))
        {
            var flags = (DocumentContractFlags)flagsInt;
            return flags.HasFlag(DocumentContractFlags.VirtualDocument);
        }

        return false;
    }

    static string GetFileExtension(AndroidUri uri)
    {
        var mimeType = Platform.AppContext.ContentResolver.GetType(uri);

        return mimeType != null
            ? MimeTypeMap.Singleton.GetExtensionFromMimeType(mimeType)
            : null;
    }


    private static string? ResolvePhysicalPath(AndroidUri uri, bool requireExtendedAccess)
    {
        if (uri.Scheme.Equals(UriSchemeFile, StringComparison.OrdinalIgnoreCase))
        {
            // if it is a file, then return directly

            var resolved = uri.Path;
            if (File.Exists(resolved))
                return resolved;
        }
        else if (!requireExtendedAccess || !HasApiLevel(29))
        {
            // if this is on an older OS version, or we just need it now

            if (HasApiLevel((int)BuildVersionCodes.Kitkat) && DocumentsContract.IsDocumentUri(Platform.AppContext, uri))
            {
                var resolved = ResolveDocumentPath(uri);
                if (File.Exists(resolved))
                    return resolved;
            }
            else if (uri.Scheme.Equals(UriSchemeContent, StringComparison.OrdinalIgnoreCase))
            {
                var resolved = ResolveContentPath(uri);
                if (File.Exists(resolved))
                    return resolved;
            }
        }

        return null;
    }

    static string GetDataFilePath(AndroidUri contentUri, string selection = null, string[] selectionArgs = null)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        const string column = MediaStore.Files.FileColumns.Data;
#pragma warning restore CS0618 // Type or member is obsolete

        // ask the content provider for the data column, which may contain the actual file path
        var path = GetColumnValue(contentUri, column, selection, selectionArgs);
        if (!string.IsNullOrEmpty(path) && Path.IsPathRooted(path))
            return path;

        return null;
    }

    static string GetColumnValue(AndroidUri contentUri, string column, string selection = null, string[] selectionArgs = null)
    {
        try
        {
            var value = QueryContentResolverColumn(contentUri, column, selection, selectionArgs);
            if (!string.IsNullOrEmpty(value))
                return value;
        }
        catch
        {
            // Ignore all exceptions and use null for the error indicator
        }

        return null;
    }

    static string QueryContentResolverColumn(AndroidUri contentUri, string columnName, string selection = null, string[] selectionArgs = null)
    {
        string text = null;

        var projection = new[] { columnName };
        using var cursor = Platform.AppContext.ContentResolver.Query(contentUri, projection, selection, selectionArgs, null);
        if (cursor?.MoveToFirst() == true)
        {
            var columnIndex = cursor.GetColumnIndex(columnName);
            if (columnIndex != -1)
                text = cursor.GetString(columnIndex);
        }

        return text;
    }

    static string ResolveDocumentPath(AndroidUri uri)
    {
        Debug.WriteLine($"Trying to resolve document URI: '{uri}'");

        var docId = DocumentsContract.GetDocumentId(uri);

        var docIdParts = docId?.Split(':');
        if (docIdParts == null || docIdParts.Length == 0)
            return null;

        if (uri.Authority.Equals(UriAuthorityExternalStorage, StringComparison.OrdinalIgnoreCase))
        {
            Debug.WriteLine($"Resolving external storage URI: '{uri}'");

            if (docIdParts.Length == 2)
            {
                var storageType = docIdParts[0];
                var uriPath = docIdParts[1];

                // This is the internal "external" memory, NOT the SD Card
                if (storageType.Equals(storageTypePrimary, StringComparison.OrdinalIgnoreCase))
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    var root = global::Android.OS.Environment.ExternalStorageDirectory.Path;
#pragma warning restore CS0618 // Type or member is obsolete

                    return Path.Combine(root, uriPath);
                }

                // TODO: support other types, such as actual SD Cards
            }
        }
        else if (uri.Authority.Equals(UriAuthorityDownloads, StringComparison.OrdinalIgnoreCase))
        {
            Debug.WriteLine($"Resolving downloads URI: '{uri}'");

            // NOTE: This only really applies to older Android vesions since the privacy changes

            if (docIdParts.Length == 2)
            {
                var storageType = docIdParts[0];
                var uriPath = docIdParts[1];

                if (storageType.Equals(storageTypeRaw, StringComparison.OrdinalIgnoreCase))
                    return uriPath;
            }

            // ID could be "###" or "msf:###"
            var fileId = docIdParts.Length == 2
                ? docIdParts[1]
                : docIdParts[0];

            foreach (var prefix in contentUriPrefixes)
            {
                var uriString = prefix + "/" + fileId;
                var contentUri = AndroidUri.Parse(uriString);

                if (GetDataFilePath(contentUri) is string filePath)
                    return filePath;
            }
        }
        else if (uri.Authority.Equals(UriAuthorityMedia, StringComparison.OrdinalIgnoreCase))
        {
            Debug.WriteLine($"Resolving media URI: '{uri}'");

            if (docIdParts.Length == 2)
            {
                var storageType = docIdParts[0];
                var uriPath = docIdParts[1];

                AndroidUri contentUri = null;
                if (storageType.Equals(storageTypeImage, StringComparison.OrdinalIgnoreCase))
                    contentUri = MediaStore.Images.Media.ExternalContentUri;
                else if (storageType.Equals(storageTypeVideo, StringComparison.OrdinalIgnoreCase))
                    contentUri = MediaStore.Video.Media.ExternalContentUri;
                else if (storageType.Equals(storageTypeAudio, StringComparison.OrdinalIgnoreCase))
                    contentUri = MediaStore.Audio.Media.ExternalContentUri;

                if (contentUri != null && GetDataFilePath(contentUri, $"{MediaStore.MediaColumns.Id}=?", new[] { uriPath }) is string filePath)
                    return filePath;
            }
        }

        Debug.WriteLine($"Unable to resolve document URI: '{uri}'");

        return null;
    }

    private static string ResolveContentPath(AndroidUri uri)
    {

        if (GetDataFilePath(uri) is string filePath)
            return filePath;

        return null;
    }

    private static bool HasApiLevel(int v)
    {
        return (int)Build.VERSION.SdkInt >= v;
    }
}