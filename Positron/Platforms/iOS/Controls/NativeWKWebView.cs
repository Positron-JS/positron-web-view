using CoreGraphics;
using Foundation;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using NeuroSpeech.Positron.Controls;
using NeuroSpeech.Positron.Platforms.iOS.Controls.WebView;
using NeuroSpeech.Positron.Platforms.iOS.Keyboard;
using NeuroSpeech.Positron.Resources;
using ObjCRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using WebKit;

namespace NeuroSpeech.Positron.Platforms.iOS.Controls;

internal class MainScriptInvoker : NSObject, IWKScriptMessageHandlerWithReply
{
    private readonly Action<string, Action<string>> messageAction;

    public MainScriptInvoker(Action<string,Action<string>> MessageAction)
    {
        messageAction = MessageAction;
    }

    public void DidReceiveScriptMessage(
        WKUserContentController userContentController, 
        WKScriptMessage message, 
        Action<NSObject, NSString> replyHandler)
    {
        messageAction(message.Body.ToString(), (msg) => replyHandler((NSString)msg, null!));
    }
}

class NativeWebViewUserContentController: WebKit.WKUserContentController {


    public NativeWebViewUserContentController()
    {
        var script = Scripts.Positron;
        this.AddUserScript(new WKUserScript((NSString)script, WebKit.WKUserScriptInjectionTime.AtDocumentStart, false));

    }

    
}

internal class NativeWKWebView : MauiWKWebView
{
    private static WKWebViewConfiguration Init(WKWebViewConfiguration configuration)
    {
        configuration.AllowsInlineMediaPlayback = true;
        configuration.Preferences.JavaScriptCanOpenWindowsAutomatically = true;
        // configuration.Preferences.JavaScriptEnabled = true;
        // configuration.Preferences.JavaScriptEnabled = true;
        configuration.DefaultWebpagePreferences ??= new WKWebpagePreferences();
        configuration.DefaultWebpagePreferences.AllowsContentJavaScript = true;
        configuration.MediaTypesRequiringUserActionForPlayback = WKAudiovisualMediaTypes.None;
        configuration.UserContentController = new NativeWebViewUserContentController();
        return configuration;
    }

    public NativeWKWebView(
        CGRect frame,
        WebViewHandler handler,
        WKWebViewConfiguration configuration) : base(frame, handler, Init(configuration))
    {
        var viewPortScript = @"
            window.visualViewport.addEventListener(""scroll"", (e) => {{
                const vp = window.visualViewport;
                window.scrollBy({
                    left: -vp.offsetLeft,
                    top: -vp.offsetTop,
                    behavior: ""instant""
                });
            }});
            window.visualViewport.addEventListener(""resize"", () => {
                document.body.style.position = ""absolute"";
                document.body.style.height = window.visualViewport.height + ""px"";
                document.body.style.width = window.visualViewport.width + ""px"";
            });
        ";
        this.Inspectable = true;
        if (handler.VirtualView is PositronWebView nativeWebView)
        {

            this.UIDelegate = new NativeWebViewUIDelegate(handler);
            // this.NavigationDelegate = new NativeWebViewNavigationDelegate(this.NavigationDelegate);

            if (configuration.UserContentController is NativeWebViewUserContentController nwc)
            {
                nwc.AddScriptMessageHandler(new MainScriptInvoker((s, a) =>
                {
                    nativeWebView.RunMainThreadJavaScript(s);
                    a("queued");
                }), WKContentWorld.Page, "mainScript");

                nwc.AddUserScript(new WKUserScript((NSString)viewPortScript, WKUserScriptInjectionTime.AtDocumentEnd, true));

            }
        }



        ScrollView.Bounces = false;
        AutosizesSubviews = true;
        ClipsToBounds = true;
        ContentMode = UIKit.UIViewContentMode.ScaleAspectFit;
        this.ScrollView.Delegate = NativeWebViewScrollViewDelegate.Instance;
        this.ScrollView.ScrollEnabled = false;
        this.AllowsBackForwardNavigationGestures = false;
        this.ScrollView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
        // this.RemoveConstraints(this.Constraints);
        // KeyboardService.Install(this, (handler.VirtualView as NativeWebView)!);
    }

    public override void LayoutSubviews()
    {
        base.LayoutSubviews();
        // ScrollView.Frame = Bounds;
    }

}

internal class NativeWebViewScrollViewDelegate : UIScrollViewDelegate
{

    // MARK: - Shared delegate
    internal static UIScrollViewDelegate Instance = new NativeWebViewScrollViewDelegate();


    public override UIView ViewForZoomingInScrollView(UIScrollView scrollView)
    {
        return null!;
    }


}