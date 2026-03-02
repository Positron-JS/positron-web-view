using Android.Content;
using Android.Locations;
using Android.Webkit;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using MimeKit;
using NeuroSpeech.Positron.Platforms.Android.Core;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;


namespace NeuroSpeech.Positron.Controls;

class PositronWebViewChromeClient : MauiWebChromeClient
{
    private IDispatcher dispatcher;
    private Context context;
    private Android.Webkit.WebView webView;
    private Android.Views.View? fullScreenView;
    private ICustomViewCallback? customViewCallback;
    private Action? hideCustomView;

    public PositronWebViewChromeClient(WebViewHandler handler) : base(handler)
    {
        this.dispatcher = Dispatcher.GetForCurrentThread()!;
        this.context = handler.Context;
        this.webView = handler.PlatformView;
    }

    public override bool OnCreateWindow(
        global::Android.Webkit.WebView? view,
        bool isDialog,
        bool isUserGesture, global::Android.OS.Message? resultMsg)
    {
        if (view != null)
        {
            var result = view.GetHitTestResult();
            var data = result.Extra;
            if (!string.IsNullOrEmpty(data))
            {
                dispatcher.DispatchTask(() => Launcher.OpenAsync(data));
                return false;
            }
        }

        return base.OnCreateWindow(view, isDialog, isUserGesture, resultMsg);
    }

    public override void OnPermissionRequest(PermissionRequest? request)
    {
        dispatcher.DispatchTask(async () =>
        {
            // request?.Grant(request.GetResources());
            var resources = request?.GetResources();
            if (resources == null)
            {
                return;
            }

            foreach (var resource in resources)
            {
                if (resource.EndsWith(".VIDEO_CAPTURE"))
                {
                    await Permissions.RequestAsync<Permissions.Camera>();
                    continue;
                }
                if (resource.EndsWith(".AUDIO_CAPTURE"))
                {
                    await Permissions.RequestAsync<AndroidAudioRecorderPermission>();
                    continue;
                }
            }
            request.Grant(resources);
        });
    }

    public override void OnGeolocationPermissionsShowPrompt(string? origin, GeolocationPermissions.ICallback? callback)
    {
        dispatcher.DispatchTask(async () =>
        {
            try
            {

                // check if not enabled...
                if (Microsoft.Maui.ApplicationModel.Platform.AppContext.GetSystemService(Context.LocationService) is LocationManager lm && lm.IsProviderEnabled(LocationManager.GpsProvider))
                {
                    var state = await Permissions.RequestAsync<AndroidCorseLocationPermission>();
                    if (state != PermissionStatus.Granted && state != PermissionStatus.Restricted)
                    {
                        throw new InvalidOperationException($"Permission not granted {state}");
                    }
                    callback?.Invoke(origin, true, true);
                    return;
                }

                // throw new InvalidOperationException("Location Service is not enabled");
                callback?.Invoke(origin, false, false);

            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "Ok");
                callback?.Invoke(origin, false, false);
            }
        });
    }

    public override bool OnShowFileChooser(Android.Webkit.WebView webView, IValueCallback filePathCallback, FileChooserParams fileChooserParams)
    {
        if (webView == null)
        {
            return false;
        }

        var multiple = fileChooserParams?.Mode == ChromeFileChooserMode.OpenMultiple;
        var fileTypes = FilePickerService.FileTypesFrom(fileChooserParams?.GetAcceptTypes());
        var options = new PickOptions
        {
            PickerTitle = fileChooserParams?.Title ?? "Choose Files",
            FileTypes = new FilePickerFileType(fileTypes),
        };
        Application.Current.MainPage.Dispatcher.DispatchAsync(async () =>
        {
            try
            {
                //var p = await Permissions.RequestAsync<Permissions.StorageRead>();
                //if (p != PermissionStatus.Granted)
                //{
                //    await Application.Current.MainPage.DisplayAlert(
                //        "Permission Denied",
                //        $"You must enable file permissions to upload files.\nPlease go to Settings > Apps > {AppInfo.Name} and enable file permissions",
                //        "Ok");
                //    filePathCallback?.OnReceiveValue(null);
                //    return;
                //}

                var filesTask = await PositronFilePicker.PlatformPickAsync(options, multiple);

                if (filesTask == null)
                {
                    filePathCallback?.OnReceiveValue(null);
                    return;
                }

                // copy file to temporary path...

                using var progress = new ProgressPanel("Converting");
                using var d = BackButtonInterceptor.Instance.InterceptBackButton(() => progress.Cancel());

                var files = filesTask;

                Dictionary<string, double> progressMap = new Dictionary<string, double>();

                var all = await Task.WhenAll(files.Select(async x =>
                {
                    var tempFile = x;
                    var mimeType = MimeTypes.GetMimeType(tempFile);
                    if (mimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
                    {
                        var oldFile = tempFile;
                        try
                        {
                            tempFile = await AndroidHybridMedia.EncodeMP4Async(webView.Context, tempFile, Preset.MediumQuality, (n) =>
                            {
                                double progressValue = 0;
                                lock (progressMap)
                                {
                                    progressMap[tempFile] = n;
                                    progressValue = progressMap.Values.Average();
                                }
                                Application.Current.Dispatcher.Dispatch(() =>
                                {
                                    progress.Progress = progressValue;
                                });
                            }, progress.CancelToken);
                            if (tempFile != oldFile)
                            {
                                System.IO.File.Delete(oldFile);
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex);
                        }
                    }
                    return tempFile;
                }).ToList());
                var uris = all.Select(x => Android.Net.Uri.FromFile(new Java.IO.File(x))).ToArray();
                filePathCallback?.OnReceiveValue(uris);
            }
            catch (TaskCanceledException)
            {
                filePathCallback?.OnReceiveValue(null);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "Ok");
                // HybridApplication.LogException(ex);
                System.Diagnostics.Debug.WriteLine(ex);
                filePathCallback?.OnReceiveValue(null);
            }
        });

        return true;
    }

    public override void OnHideCustomView()
    {
        hideCustomView?.Invoke();
    }

    public override void OnShowCustomView(Android.Views.View? view, ICustomViewCallback? callback)
    {
        if (this.fullScreenView != null)
        {
            OnHideCustomView();
            return;
        }

        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;

        this.fullScreenView = view;
        this.customViewCallback = callback;

        var grid = webView.Parent as Grid;
        if (grid != null)
        {
            // var v = new AndroidNativeViewElement(view);
            var v =  view.ToView();
            // var v = new NativeViewWrapper(view);
            // var v = new Microsoft.Maui.Controls.Compatibility.Platform.Android.
            v.HorizontalOptions = LayoutOptions.Fill;
            v.VerticalOptions = LayoutOptions.Fill;
            grid.Children.Add(v);
            var d = BackButtonInterceptor.Instance.InterceptBackButton(() => OnHideCustomView());

            //var closeButton = new CloseButton
            //{
            //    Margin = new Xamarin.Forms.Thickness(0, 10, 10, 0),
            //    HorizontalOptions = LayoutOptions.End,
            //    VerticalOptions = LayoutOptions.Start,
            //    Command = new Command(() =>
            //    {
            //        OnHideCustomView();
            //    })
            //};

            // grid.Children.Add(closeButton);

            hideCustomView = () => {
                v.Dispatcher.Dispatch(() => {
                    this.fullScreenView = null;
                    grid.Children.Remove(v);
                    callback?.OnCustomViewHidden();
                    hideCustomView = null;
                    d.Dispose();
                });

                // HybridRunner.RunAsync(() => webView.EvaluateJavaScriptAsync("__androidHideCustomView()"));
                // this.fullScreenView = null;
                // // grid.Children.Remove(closeButton);
                // grid.Children.Remove(v);
                // callback?.OnCustomViewHidden();
                // hideCustomView = null;
                // d.Dispose();
            };
        }

    }
}
