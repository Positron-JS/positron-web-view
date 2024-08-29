using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OtaliaStudios.TranscoderLib.Strategy;
using OtaliaStudios.TranscoderLib;

namespace Positron.Platforms.Android.Core;
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
        string inputFile,
        Preset? preset,
        Action<double> progress,
        System.Threading.CancellationToken cancelToken)
    {
        DefaultVideoStrategy.Builder? vs = null;


        var p = preset ?? Preset.Passthrough;
        switch (p)
        {
            case Preset.LowQuality:
            case Preset.Preset640x480:
                vs = DefaultVideoStrategy
                    .AtMost(680, 480)
                    .BitRate(1024 * 1024);
                break;
            case Preset.Preset960x540:
                vs = DefaultVideoStrategy.AtMost(960, 540)
                    .BitRate(2 * 1024 * 1024);
                break;
            case Preset.MediumQuality:
            case Preset.Passthrough:
            case Preset.Preset1280x720:
                vs = DefaultVideoStrategy.AtMost(1280, 720)
                    .BitRate(4 * 1024 * 1024);
                break;
            case Preset.HighestQuality:
            case Preset.Preset1920x1080:
                vs = DefaultVideoStrategy.AtMost(1920, 1080)
                    .BitRate(5 * 1024 * 1024);
                break;
            case Preset.Preset3840x2160:
                vs = DefaultVideoStrategy.AtMost(3840, 2160)
                    .BitRate(8 * 1024 * 1024);
                break;
        }

        var listener = new TranscoderListener(progress);
        var outputFile = new Java.IO.File(FilePickerService.CreateTmpFile("video.mp4"));
        var tr = Transcoder.Into(outputFile.CanonicalPath)
            .AddDataSource(inputFile);
        if (vs != null)
        {
            tr = tr.SetVideoTrackStrategy(vs.Build());
        }
        var future = tr
            .SetListener(listener)
            .Transcode();

        cancelToken.Register(() => future.Cancel(true));

        await listener.Task;

        if (outputFile.Length() == 0)
        {
            outputFile.Delete();
            return inputFile;
        }

        return outputFile.CanonicalPath;
    }

    internal class TranscoderListener : Java.Lang.Object, ITranscoderListener
    {
        private readonly TaskCompletionSource<int> source;
        private readonly Action<double>? progressAction;

        public Task Task => source.Task;

        public TranscoderListener(Action<double>? progressAction = null)
        {
            this.source = new TaskCompletionSource<int>();
            this.progressAction = progressAction;
        }

        public void OnTranscodeCanceled()
        {
            this.source.TrySetCanceled();
        }

        public void OnTranscodeCompleted(int successCode)
        {
            this.source.TrySetResult(successCode);
        }

        public void OnTranscodeFailed(Throwable exception)
        {
            this.source.TrySetException(exception);
        }

        public void OnTranscodeProgress(double progress)
        {
            progressAction?.Invoke(progress);
        }
    }
}
