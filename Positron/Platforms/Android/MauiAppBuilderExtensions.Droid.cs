using Firebase.Messaging;
using Microsoft.Maui.LifecycleEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroSpeech.Positron;


class SuccessListener : Java.Lang.Object, Android.Gms.Tasks.IOnSuccessListener
{
    private readonly Action<string> action;

    public SuccessListener(Action<string> action)
    {
        this.action = action;
    }


    public void OnSuccess(Java.Lang.Object? result)
    {
        this.action(result!.ToString());
    }
}

public static partial class MauiAppBuilderExtensions
{
    public static MauiAppBuilder RegisterPushServices(this MauiAppBuilder builder)
    {
        builder.ConfigureLifecycleEvents(events =>
        {
            events.AddAndroid(android => android.OnCreate((activity, _) => {
                // CrossFirebase.Initialize(activity, () => activity, CreateCrossFirebaseSettings());
                activity.RunOnUiThread(() => {
                    try {
                        FirebaseMessaging.Instance.GetToken()
                            .AddOnSuccessListener(activity, new SuccessListener((token) => {
                                Positron.Instance.DeviceToken = token;
                            }));
                    } catch (Exception ex) {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                });
                
            }
            ));
        });
        // builder.Services.AddSingleton(_ => CrossFirebaseAuth.Current);
        return builder;
    }

    //private static CrossFirebaseSettings CreateCrossFirebaseSettings()
    //{
    //    return new CrossFirebaseSettings(isAuthEnabled: true);
    //}
}
