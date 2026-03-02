using AndroidX.Media3.Common;
using AndroidX.Media3.Effect;
using AndroidX.Media3.Transformer;

namespace NeuroSpeech.Positron.Platforms.Android.Core;
public enum Preset
{
    LowQuality = 0,
    MediumQuality = 1,
    HighestQuality = 2,
    Preset640x480 = 3,
    Preset960x540 = 4,
    Preset1280x720 = 5,
    Preset1920x1080 = 6,
    Preset3840x2160 = 7,
    Passthrough = 9,
}
public class AndroidHybridMedia
{
    public static async Task<string> EncodeMP4Async(
        global::Android.Content.Context context,
        string inputFile,
        Preset? preset,
        Action<double> progress,
        System.Threading.CancellationToken cancelToken)
    {

        var inputMediaFile = global::Android.Net.Uri.FromFile(new Java.IO.File(inputFile));

        var inputMedia = MediaItem.FromUri(inputFile);

        var bitRate = 1024 * 1024;
        var targetHeight = 480;

        var p = preset ?? Preset.Passthrough;
        switch (p)
        {
            case Preset.LowQuality:
            case Preset.Preset640x480:
                break;
            case Preset.Preset960x540:
                bitRate = 2 * 1024 * 1024;
                targetHeight = 540;
                break;
            case Preset.MediumQuality:
            case Preset.Passthrough:
            case Preset.Preset1280x720:
                bitRate = 4 * 1024 * 1024;
                targetHeight = 720;
                break;
            case Preset.HighestQuality:
            case Preset.Preset1920x1080:
                bitRate = 5 * 1024 * 1024;
                targetHeight = 1080;
                break;
            case Preset.Preset3840x2160:
                bitRate = 8 * 1024 * 1024;
                targetHeight = 2160;
                break;
        }

        var ef = new DefaultEncoderFactory.Builder(context)
            .SetRequestedVideoEncoderSettings(new VideoEncoderSettings.Builder().SetBitrate(bitRate).Build())
            .Build();

        var listneer = new TranscoderListener();

        var transformer = new Transformer.Builder(context)
            .SetEncoderFactory(ef)
            .SetVideoMimeType(MimeTypes.VideoH264)
            .SetAudioMimeType(MimeTypes.AudioAac)
            .AddListener(listneer)
            .Build();

        var videoEffects = new Effects(new List<global::AndroidX.Media3.Common.Audio.IAudioProcessor>() {  },
            new System.Collections.Generic.List<global::AndroidX.Media3.Common.IEffect>() { Presentation.CreateForHeight(targetHeight) });

        var editedItem = new EditedMediaItem.Builder(inputMedia)
                .SetEffects(videoEffects)
                .Build();

        var outputFile = new Java.IO.File(FilePickerService.CreateTmpFile("video.mp4"));
        transformer.Start(editedItem, outputFile.Path);

        var pg = new ProgressHolder();

        while(!listneer.Task.IsCompleted)
        {
            await Task.Delay(1000);
            // get progress...
            transformer.GetProgress(pg);

            progress((double)pg.Progress / 100);
        }

        if (outputFile.Length() == 0)
        {
            outputFile.Delete();
            return inputFile;
        }

        return outputFile.CanonicalPath;
    }

    internal class TranscoderListener : Java.Lang.Object
        ,Transformer.IListener
    {
        private readonly TaskCompletionSource<int> source;
        private readonly Action<double>? progressAction;

        public Task Task => source.Task;

        public TranscoderListener(Action<double>? progressAction = null)
        {
            this.source = new TaskCompletionSource<int>();
            this.progressAction = progressAction;
        }

        void Transformer.IListener.OnCompleted(Composition? composition, ExportResult? exportResult)
        {
            source.TrySetResult(0);
        }

        void Transformer.IListener.OnError(Composition? composition, ExportResult? exportResult, ExportException? exportException)
        {
            source.TrySetException(exportException ?? new System.Exception("Unknown exception"));
        }        
    }
}
