using Foundation;
using Positron.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using WebKit;

namespace Positron.Platforms.iOS.Keyboard
{
    class KeyboardService
    {

        static double UpdateHeightMargin(
            WKWebView iOSWebView,
            PositronWebView webView,
            System.Drawing.RectangleF rect) {
            var height = rect.Height;

            try
            {
                var eventName = "keyboardHidden";
                var keyboard = "hidden";
                var styleHeight = "''";
                var stylePosition = "''";
                if (height > 0)
                {
                    keyboard = "visible";
                    eventName = "keyboardVisible";
                    styleHeight = "window.visualViewport.height + 'px'";
                    stylePosition = "'absolute'";
                }
                webView.Eval(@$"
setTimeout(() => {{
    document.body.dataset.keyboard = '{keyboard}';
    document.body.dataset.keyboardHeight = {height};
    document.body.dispatchEvent(new CustomEvent('{eventName}', {{ bubbles: true, detail: {{ height: {height} }} }}));
    document.body.style.height = {styleHeight};
    document.body.style.position = {stylePosition};
    if ({height}) {{
        window.scrollTo(0,0);
    }}
}}, 1);
");
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return height;
        }
        public static IDisposable Install(WKWebView iOSWebView, PositronWebView webView)
        {


            var defaultCenter = NSNotificationCenter.DefaultCenter;
            var didShow = defaultCenter.AddObserver(UIKeyboard.DidShowNotification, (n) => {
                if (n.UserInfo == null)
                {
                    return;
                }
                NSValue result = (NSValue)n.UserInfo.ObjectForKey(new NSString(UIKeyboard.FrameEndUserInfoKey));
                UpdateHeightMargin(iOSWebView, webView, result.RectangleFValue);
            });
            var didChange = defaultCenter.AddObserver(UIKeyboard.DidChangeFrameNotification, (n) => {
                if (n.UserInfo == null)
                {
                    return;
                }
                NSValue result = (NSValue)n.UserInfo.ObjectForKey(new NSString(UIKeyboard.FrameEndUserInfoKey));
                UpdateHeightMargin(iOSWebView, webView, result.RectangleFValue);
            });
            var didHide = defaultCenter.AddObserver(UIKeyboard.DidHideNotification, (n) => {
                UpdateHeightMargin(iOSWebView, webView, new System.Drawing.RectangleF(0,0,0,0));
            });

            return new DisposableAction(delegate {
                defaultCenter.RemoveObserver(didShow);
                defaultCenter.RemoveObserver(didChange);
                defaultCenter.RemoveObserver(didHide);
            });
        }

    }
}
