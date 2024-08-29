using Android.Graphics;
using Android.Webkit;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Positron.Controls;
using Positron.Keyboard;
using Positron.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Android.Views.ViewGroup;

namespace Positron.Platforms.Android.Controls
{
    class NativeWebViewClient : MauiWebViewClient
    {
        private readonly global::Android.Webkit.WebView platformView;
        private readonly PositronWebView nativeWebView;

        public NativeWebViewClient(WebViewHandler handler, PositronWebView nativeWebView) : base(handler)
        {

            this.platformView = handler.PlatformView;
            this.nativeWebView = nativeWebView;

           
        }

        public override void OnPageStarted(global::Android.Webkit.WebView? view, string? url, Bitmap? favicon)
        {
            base.OnPageStarted(view, url, favicon);
            KeyboardService.Instance.Refresh();
        }

        public override void OnPageFinished(global::Android.Webkit.WebView? view, string? url)
        {
            base.OnPageFinished(view, url);
            this.nativeWebView.Eval(Scripts.Positron);
            this.nativeWebView.IsPageReady = true;
            KeyboardService.Instance.Refresh();
        }


        class MessageCallback: WebMessagePort.WebMessageCallback
        {
            private readonly PositronWebView client;
            private readonly WebMessagePort sender;

            public MessageCallback(PositronWebView client, WebMessagePort sender)
            {
                this.client = client;
                this.sender = sender;
            }

            public override void OnMessage(WebMessagePort? port, WebMessage? message)
            {
                client.RunMainThreadJavaScript(message.Data);
            }
        }
    }
}
