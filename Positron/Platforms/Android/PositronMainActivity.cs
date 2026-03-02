using Android.App;
using Android.Content;
using Android.OS;
using Firebase.Messaging;

namespace NeuroSpeech.Positron;

public class PositronMainActivity: MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        HandleIntent(Intent);
        CreateNotificationChannelIfNeeded();
    }

    protected override void OnNewIntent(Intent intent)
    {
        base.OnNewIntent(intent);
        HandleIntent(intent);
    }

    protected virtual void HandleIntent(Intent intent)
    {
        // FirebaseCloudMessagingImplementation.OnNewIntent(intent);
        // FirebaseMessaging.Instance.
        if (intent?.HasExtra("action") == true)
        {
            var action = intent.GetStringExtra("action");
            if (!string.IsNullOrEmpty(action))
            {
                Positron.Instance.MessageAction = action;
            }
        }
    }

    protected virtual void CreateNotificationChannelIfNeeded()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            CreateNotificationChannel();
        }
    }

    private void CreateNotificationChannel()
    {
        var channelId = $"{PackageName}.general";
        var notificationManager = (NotificationManager)GetSystemService(NotificationService);
        var channel = new NotificationChannel(channelId, "General", NotificationImportance.Default);
        notificationManager.CreateNotificationChannel(channel);
        // FirebaseCloudMessagingImplementation.ChannelId = channelId;
        //FirebaseCloudMessagingImplementation.SmallIconRef = Resource.Drawable.ic_push_small;
    }
}
