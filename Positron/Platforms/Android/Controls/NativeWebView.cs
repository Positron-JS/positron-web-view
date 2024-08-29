using Android.Webkit;
using Java.Interop;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Handlers;
using NeuroSpeech.Positron.Keyboard;
using NeuroSpeech.Positron.Platforms.Android.Controls;
using NeuroSpeech.Positron.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Android.Views.ViewGroup;

namespace NeuroSpeech.Positron.Controls;


public class JSBridge : Java.Lang.Object
{
    private readonly PositronWebView nativeWebView;

    internal JSBridge(PositronWebView nativeWebView)
    {
        this.nativeWebView = nativeWebView;
    }

    [JavascriptInterface]
    [Export("invokeAction")]
    public void InvokeAction(string data)
    {
        nativeWebView.RunMainThreadJavaScript(data);
    }
}

partial class PositronWebView
{

    internal bool IsPageReady = false;

    static partial void OnStaticPlatformInit()
    {
        Android.Webkit.WebView.SetWebContentsDebuggingEnabled(true);
        Microsoft.Maui.Controls.Application.Current
            .On<Microsoft.Maui.Controls.PlatformConfiguration.Android>()
            .UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
    }

    private Android.Webkit.WebView PlatformView;

    partial void OnPlatformInit()
    {
        // KeyboardService.Instance.KeyboardChanged += Instance_KeyboardChanged;
        this.disposables.Register(BackButtonInterceptor.Instance.InterceptBackButton(delegate {
            this.Eval("androidPressBackButton && androidPressBackButton()");
        }));

    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (this.Handler != null)
        {
            var webViewHandler = (WebViewHandler)this.Handler;
            this.PlatformView = webViewHandler.PlatformView;
            PlatformView.SetWebChromeClient(new PositronWebViewChromeClient(webViewHandler));
            PlatformView.SetWebViewClient(new PositronWebViewClient(webViewHandler, this));
            this.PlatformView.AddJavascriptInterface(new JSBridge(this), "androidBridge");

            Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Application
                .SetWindowSoftInputModeAdjust(this.Window, WindowSoftInputModeAdjust.Resize);

        }
    }

    //private void Instance_KeyboardChanged(object? sender, AndroidKeyboardEventArgs e)
    //{
    //    if(!this.IsPageReady)
    //    {
    //        return;
    //    }

    //    if (e.IsOpen)
    //    {
    //        var height = e.Height;
    //        // get main..
    //        var main = Application.Current.MainPage;
    //        this.Margin = new Thickness(0, 0, 0, main.Height * height);
    //        try
    //        {
    //            this.Eval($"document.body.dataset.keyboard = 'shown'; document.body.dataset.keyboardHeight = {height};");
    //        }
    //        catch { }
    //        return;
    //    }
    //    try
    //    {
    //        this.Margin = new Thickness(0, 0, 0, 0);
    //        this.Eval($"document.body.dataset.keyboard = 'hidden'; document.body.dataset.keyboardHeight = 0;");
    //    }
    //    catch { }
    //}
}
