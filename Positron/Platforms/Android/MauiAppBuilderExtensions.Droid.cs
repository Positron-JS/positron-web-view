using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Bundled.Platforms.Android;
using Plugin.Firebase.Bundled.Shared;
using Plugin.Firebase.CloudMessaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroSpeech.Positron;

public static partial class MauiAppBuilderExtensions
{
    public static MauiAppBuilder RegisterPushServices(this MauiAppBuilder builder)
    {
        builder.ConfigureLifecycleEvents(events =>
        {
            events.AddAndroid(android => android.OnCreate((activity, _) => {
                CrossFirebase.Initialize(activity, () => activity, CreateCrossFirebaseSettings());
                activity.RunOnUiThread(async () => {
                    try {
                        CrossFirebaseCloudMessaging.Current.TokenChanged += (s, e) => {
                            Positron.Instance.DeviceToken = e.Token;
                        };
                        var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
                        Positron.Instance.DeviceToken = token;
                    } catch (Exception ex) {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                });
                
            }
            ));
        });
        builder.Services.AddSingleton(_ => CrossFirebaseAuth.Current);
        return builder;
    }

    private static CrossFirebaseSettings CreateCrossFirebaseSettings()
    {
        return new CrossFirebaseSettings(isAuthEnabled: true);
    }
}
