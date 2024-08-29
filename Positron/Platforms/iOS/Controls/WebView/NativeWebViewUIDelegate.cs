using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebKit;

namespace NeuroSpeech.Positron.Platforms.iOS.Controls.WebView;

internal class NativeWebViewUIDelegate : MauiWebViewUIDelegate
{
    private IDispatcher dispatcher;

    public NativeWebViewUIDelegate(IWebViewHandler handler) : base(handler)
    {
        this.dispatcher = Dispatcher.GetForCurrentThread()!;
    }

    public override WKWebView? CreateWebView(WKWebView webView, WKWebViewConfiguration configuration, WKNavigationAction navigationAction, WKWindowFeatures windowFeatures)
    {
        // if(navigationAction.NavigationType == WKNavigationType.LinkActivated)
        // {
        var url = navigationAction.Request?.Url?.ToString();
        if (!string.IsNullOrEmpty(url))
        {
            // HybridRunner.RunAsync(() => Xamarin.Essentials.Browser.OpenAsync(url, Xamarin.Essentials.BrowserLaunchMode.External));
            dispatcher.DispatchTask(() => Browser.Default.OpenAsync(url, BrowserLaunchMode.External));
        }
        // }
        return null;
        // return base.CreateWebView(webView, configuration, navigationAction, windowFeatures);
    }

    public override void RequestMediaCapturePermission(WKWebView webView, WKSecurityOrigin origin, WKFrameInfo frame, WKMediaCaptureType type, Action<WKPermissionDecision> decisionHandler)
    {
        decisionHandler(WKPermissionDecision.Grant);
    }


}
