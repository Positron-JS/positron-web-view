using Android.Content;
using Android.Locations;
using Android.Webkit;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Positron.Controls
{
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
                // var v = view.;
                // v.HorizontalOptions = LayoutOptions.Fill;
                // v.VerticalOptions = LayoutOptions.Fill;
                // grid.Children.Add(v);
                // var d = BackButtonInterceptor.Instance.InterceptBackButton(() => OnHideCustomView());

                //var closeButton = new CloseButton { 
                //    Margin = new Xamarin.Forms.Thickness(0,10,10,0),
                //    HorizontalOptions = LayoutOptions.End,
                //    VerticalOptions = LayoutOptions.Start,
                //    Command = new Command(() => {
                //        OnHideCustomView();
                //    })
                //};

                //grid.Children.Add(closeButton);

                hideCustomView = () => {
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
}
