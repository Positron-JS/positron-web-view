using Android.App;
using Firebase.Messaging;

namespace NeuroSpeech.Positron.Platforms.Android;

[Service(Exported = false)]
[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
public class PushNotificationFirebaseMessagingService : FirebaseMessagingService
{
    int _messageId;

    public override void OnNewToken(string token)
    {
        Positron.Instance.DeviceToken = token;
    }

    public override void OnMessageReceived(RemoteMessage message)
    {
        base.OnMessageReceived(message);

        if (message.Data.TryGetValue("action", out var messageAction))
        {
            Positron.Instance.MessageAction = messageAction;
        }
    }
}