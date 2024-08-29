using Foundation;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebKit;

namespace Positron.Platforms.iOS.Controls.WebView
{
    internal class NativeWebViewNavigationDelegate : WKNavigationDelegate
    {
        private readonly IWKNavigationDelegate navigationDelegate;
        private IDispatcher dispatcher;

        public NativeWebViewNavigationDelegate(IWKNavigationDelegate navigationDelegate)
        {
            this.dispatcher = Dispatcher.GetForCurrentThread()!;
            this.navigationDelegate = navigationDelegate;
        }

        public override void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error)
        {
            navigationDelegate.DidFailNavigation(webView, navigation, error);
        }

        public override void DidFailProvisionalNavigation(WKWebView webView, WKNavigation navigation, NSError error)
        {
            navigationDelegate.DidFailProvisionalNavigation(webView, navigation, error);
        }

        public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            navigationDelegate.DidFinishNavigation(webView, navigation);
        }

        public override void DidStartProvisionalNavigation(WKWebView webView, WKNavigation navigation)
        {
            navigationDelegate.DidStartProvisionalNavigation(webView, navigation);
        }

        // https://stackoverflow.com/questions/37509990/migrating-from-uiwebview-to-wkwebview
        public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
        {
            if (navigationAction.ShouldPerformDownload)
            {
                var url = navigationAction.Request.Url.ToString();
                decisionHandler(WKNavigationActionPolicy.Download);
                dispatcher.DispatchTask(() => Browser.OpenAsync(url, BrowserLaunchMode.External));
                return;
            }
            try
            {
                navigationDelegate.DecidePolicy(webView, navigationAction, decisionHandler);
            } catch (Exception ex)
            {
                decisionHandler(WKNavigationActionPolicy.Allow);
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        public override void DecidePolicy(WKWebView webView, WKNavigationResponse navigationResponse, Action<WKNavigationResponsePolicy> decisionHandler)
        {
            // navigationDelegate.DecidePolicy(webView, navigationResponse, decisionHandler);
            decisionHandler(WKNavigationResponsePolicy.Allow);
        }
    }
}
