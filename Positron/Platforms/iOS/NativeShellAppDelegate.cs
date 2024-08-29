using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using UserNotifications;

namespace Positron.Platforms
{
    public abstract class PositronAppDelegate : MauiUIApplicationDelegate, IUNUserNotificationCenterDelegate
    {
        public event EventHandler<UserInfoEventArgs>? NotificationReceived;

        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {

            // Register your app for remote notifications.
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {

                // For iOS 10 display notification (sent via APNS)
                UNUserNotificationCenter.Current.Delegate = this;

                // iOS 10 or later
                var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;
                UNUserNotificationCenter.Current.RequestAuthorization(authOptions, (granted, error) => {

                    System.Diagnostics.Debug.WriteLine($"{granted}: {error?.LocalizedFailureReason} {error?.LocalizedDescription}");

                    if (error != null)
                    {
                        System.Diagnostics.Debug.Fail(error.LocalizedFailureReason, error.LocalizedDescription);
                    }

                });

            }
            else
            {
                // iOS 9 or before
                var allNotificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
                var settings = UIUserNotificationSettings.GetSettingsForTypes(allNotificationTypes, null);
                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);

            }

            UIApplication.SharedApplication.RegisterForRemoteNotifications();


            return base.FinishedLaunching(uiApplication, launchOptions);
        }

        public void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {

            //Messaging.SharedInstance.AppDidReceiveMessage(userInfo);

            try
            {
                ShowRemoteNotification(application, userInfo);
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debug.Fail("Error", ex.ToString());
                // AtomDevice.Instance.Log(ex);
                TrackError(ex);
            }
        }

        protected abstract void TrackError(Exception ex);

        //public void DidReceiveRegistrationToken(Messaging sender, string token)
        //{
        //    AtomDevice.Instance.RunOnUIThread(async () => {
        //        await CastingService.Instance.RegisterToken(token);
        //    });
        //}

        private string GetAlertMessage(NSDictionary d)
        {
            if (!d.TryGetValue((NSString)"aps", out var aps))
                return "";
            if (!(aps is NSDictionary d1))
                return "";
            if (d1.ContainsKey((NSString)"content-available"))
                return "";
            if (!d1.TryGetValue((NSString)"alert", out var alert))
                return "";
            if (!(alert is NSDictionary d2))
                return "";
            if (d2.TryGetValue((NSString)"message", out var msg))
            {
                if (msg is NSNull)
                    return "";
                return msg.ToString();
            }
            if (d2.TryGetValue((NSString)"body", out msg))
            {
                if (msg is NSNull)
                    return "";
                return msg.ToString();
            }
            return "";
        }

        protected abstract void OnShowEmptyRemoteNotification();

        void ShowRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            var msg = GetAlertMessage(userInfo);

            if (string.IsNullOrWhiteSpace(msg))
            {
                OnShowEmptyRemoteNotification();
                return;
            }

            // NSUserDefaults.StandardUserDefaults.SetString("/messages", "uri");
            Positron.Instance.UrlRequested = "/messages";


            application.ApplicationIconBadgeNumber = (nint)(application.ApplicationIconBadgeNumber) + 1;


            NSNotificationCenter.DefaultCenter.PostNotificationName("ReceivedNotification", userInfo);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            if (Platform.OpenUrl(app, url, options))
                return true;
            Positron.Instance.UrlRequested = url.ToString();
            return base.OpenUrl(app, url, options);
        }

        public void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            // System.Diagnostics.Debug.WriteLine("Registration recevied");

            var type = "prod";

#if DEBUG
            // Messaging.SharedInstance.SetApnsToken(deviceToken, ApnsTokenType.Sandbox);
            // InstanceId.SharedInstance.SetApnsToken(deviceToken, ApnsTokenType.Sandbox);

            type = "sandbox";

#else
            // Messaging.SharedInstance.SetApnsToken(deviceToken, ApnsTokenType.Production);
            //InstanceId.SharedInstance.SetApnsToken(deviceToken, ApnsTokenType.Prod);
#endif
            var data = deviceToken.ToArray();
            var sb = new StringBuilder();
            foreach (var b in data)
            {
                sb.Append($"{b:X2}");
            }
            var token = sb.ToString();
            var bundle = NSBundle.MainBundle.BundleIdentifier;
            // DependencyService.Get<IAppService>().DeviceToken = $"ios-{type}:{bundle}:{token}";
            Positron.Instance.DeviceToken = $"ios-{type}:{bundle}:{token}";

            //var service = Xamarin.Forms.DependencyService.Get<PushService>() as ApplePushService;
            //service.OnRegisterdForRemotenotifications(deviceToken);

            //base.RegisteredForRemoteNotifications(application, deviceToken);
        }

        public void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            TrackError(new Exception(error.ToString()));

        }

        //public override void HandleEventsForBackgroundUrl(
        //    UIApplication application,
        //    string sessionIdentifier,
        //    [BlockProxy(typeof(AdAction))]
        //            Action completionHandler)
        //{
        //    var configuration = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration(sessionIdentifier);

        //    AppUrlSessionDelegate.Instance.CompletionHandler = completionHandler;

        //    var session = NSUrlSession.FromWeakConfiguration(configuration, AppUrlSessionDelegate.Instance, NSOperationQueue.MainQueue);


        //}

        //[Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:")]
        //public void DidReceiveRemoteNotificationCenter(UNUserNotificationCenter sender, UNNotificationResponse response, Action completedHandler)
        //{

        //}


        private static Action<UIBackgroundFetchResult>? LastCompletionHandler;

        public static void CompleteBackgroundFetch(UIBackgroundFetchResult r)
        {
            LastCompletionHandler?.Invoke(r);
            LastCompletionHandler = null;
        }

        // To receive notifications in foregroung on iOS 9 and below.
        // To receive notifications in background in any iOS version
        // [Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:")]
        public void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            // If you are receiving a notification message while your app is in the background,
            // this callback will not be fired 'till the user taps on the notification launching the application.

            // If you disable method swizzling, you'll need to call this method. 
            // This lets FCM track message delivery and analytics, which is performed
            // automatically with method swizzling enabled.
            //Messaging.GetInstance ().AppDidReceiveMessage (userInfo);

            //PushMessages.Instance.OnMessageReceived(userInfo);

            // AtomDevice.Instance.Log("Notification Received");

            LastCompletionHandler = completionHandler;

            ShowRemoteNotification(application, userInfo);



            if (NotificationReceived == null)
                return;

            var e = new UserInfoEventArgs { UserInfo = userInfo };
            NotificationReceived(this, e);
        }

        // To receive notifications in foreground on iOS 10 devices.
        [Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
        public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
        {

            // ShowRemoteNotification(application, userInfo);

            //PushMessages.Instance.OnMessageReceived(notification.Request.Content.UserInfo);
            if (NotificationReceived == null)
                return;

            var e = new UserInfoEventArgs { UserInfo = notification.Request.Content.UserInfo };
            NotificationReceived(this, e);
        }

        //public void ApplicationReceivedRemoteMessage(RemoteMessage remoteMessage)
        //{

        //    System.Diagnostics.Debug.WriteLine(remoteMessage.ToString());

        //}

        ////////////////////
        //////////////////// END OF WORKAROUND
        ////////////////////
        ///// 
        //void TokenRefreshNotification(object sender, NSNotificationEventArgs e)
        //{
        //    // This method will be fired everytime a new token is generated, including the first
        //    // time. So if you need to retrieve the token as soon as it is available this is where that
        //    // should be done.
        //    var refreshedToken = InstanceId.SharedInstance.Token;

        //    ConnectToFCM();

        //    AtomDevice.Instance.RunOnUIThread(async () => {
        //        await CastingService.Instance.RegisterToken(refreshedToken);
        //    });

        //    // TODO: If necessary send token to application server.
        //}

        //public static void ConnectToFCM()
        //{
        //    Messaging.SharedInstance.ShouldEstablishDirectChannel = true;
        //    //Messaging.SharedInstance.Connect(error => {
        //    //    if (error != null)
        //    //    {
        //    //        AtomDevice.Instance.Log(error.LocalizedFailureReason, error.LocalizedDescription);
        //    //        //System.Diagnostics.Debug.Fail(error.LocalizedFailureReason, error.LocalizedDescription);
        //    //    }
        //    //    else
        //    //    {
        //    //        System.Diagnostics.Debug.WriteLine("FCM Connection successful!!");

        //    //    }
        //    //});
        //}

        public static void ShowMessage(string title, string message, UIViewController fromViewController, Action actionForOk = null)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, (obj) => {
                    if (actionForOk != null)
                    {
                        actionForOk();
                    }
                }));
                fromViewController.PresentViewController(alert, true, null);
            }
            else
            {
                new UIAlertView(title, message, (IUIAlertViewDelegate)null, "Ok", null).Show();
            }
        }
    }

    public class UserInfoEventArgs
    {
        public NSDictionary UserInfo { get; set; }
    }
}
