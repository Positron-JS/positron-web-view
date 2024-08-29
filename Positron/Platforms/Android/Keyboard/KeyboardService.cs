using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Android.Views.ViewTreeObserver;
using View = Android.Views.View;

namespace Positron.Keyboard
{
    public class AndroidKeyboardEventArgs
    {
        public bool IsOpen { get; set; }
        public double Height { get; internal set; }
    }

    internal class KeyboardService : Java.Lang.Object,
            IOnGlobalLayoutListener
    // , View.IOnApplyWindowInsetsListener
    {

        private static KeyboardService? _instance;
        private View? decorView;

        public static KeyboardService Instance => _instance ?? (_instance = new KeyboardService());

        public event EventHandler<AndroidKeyboardEventArgs>? KeyboardChanged;

        public KeyboardService()
        {
            Init();
        }

        private void Init(int max = 10)
        {
            decorView = Platform.CurrentActivity?.Window?.DecorView;
            if (decorView == null)
            {
                if (max == 0)
                {
                    return;
                }
                // postpone...
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(100);
                    Init(max--);
                });
                return;
            }

            decorView.ViewTreeObserver!.AddOnGlobalLayoutListener(this);
            // decorView.SetOnApplyWindowInsetsListener(this);
        }

        private double lastHeight;

        //public WindowInsets? OnApplyWindowInsets(View? v, WindowInsets? insets)
        //{
        //    var ime = WindowInsetsCompat.Type.Ime();
        //    bool isVisible = insets.IsVisible(ime);
        //    double height = 0;
        //    if (isVisible)
        //    {
        //        var inset = insets.GetInsets(ime);
        //        height = (double)inset.Bottom / (double)v.Height;
        //    }
        //    if (height != lastHeight)
        //    {
        //        lastHeight = height;
        //        KeyboardChanged?.Invoke(this, new AndroidKeyboardEventArgs()
        //        {
        //            IsOpen = isVisible,
        //            Height = height
        //        });
        //    }
        //    return insets;
        //}

        private double minSize = 0;

        public void Refresh()
        {
            lastHeight = -1;
            OnGlobalLayout();
        }

        public void OnGlobalLayout()
        {
            if (decorView == null)
                return;
            //    HybridRunner.TriggerOnce(OnKeybordLayoutChanged);
            //}

            //private async Task OnKeybordLayoutChanged()
            //{
            //    await Task.Delay(100);
            Rect rect = new Rect();
            decorView.GetWindowVisibleDisplayFrame(rect);
            double screenHeight = decorView.RootView.Height;
            double keyboardHeight = screenHeight - rect.Height;
            if (rect.Height == 0) {
                return;
            }
            if (keyboardHeight != lastHeight)
            {
                lastHeight = keyboardHeight;
                double heightInPercentage = keyboardHeight / screenHeight;
                System.Diagnostics.Debug.WriteLine($"lastHeight = {lastHeight}, height% = {heightInPercentage}, screen = ${screenHeight} , rect = {rect.Bottom}");
                KeyboardChanged?.Invoke(this, new AndroidKeyboardEventArgs()
                {
                    IsOpen = heightInPercentage > 0.1,
                    Height = heightInPercentage
                });
            }
        }
    }
}
