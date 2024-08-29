using Android.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeuroSpeech.Positron;

public class BackButtonInterceptor : AndroidX.Activity.OnBackPressedCallback
{
    public static BackButtonInterceptor Instance = new BackButtonInterceptor(true);

    private List<Action> actions = new List<Action>();

    public BackButtonInterceptor(bool enabled) : base(enabled)
    {

        if (Platform.CurrentActivity is AndroidX.Activity.ComponentActivity ca)
        {
            ca.OnBackPressedDispatcher.AddCallback(this);
        }

    }

    public BackButtonInterceptor(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }

    public IDisposable InterceptBackButton(Action action)
    {
        actions.Add(action);
        return new DisposableAction(delegate {
            actions.Remove(action);
        });
    }

    public override void HandleOnBackPressed()
    {
        bool handled = false;
        foreach (var action in actions)
        {
            handled = true;
            action();
        }
        if (!handled)
        {
            Platform.CurrentActivity?.Finish();
        }
    }
}
