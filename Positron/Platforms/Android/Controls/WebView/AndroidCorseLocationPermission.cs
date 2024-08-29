namespace NeuroSpeech.Positron.Controls;

public class AndroidCorseLocationPermission : Permissions.LocationWhenInUse
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new (string, bool)[]
    {
        (Android.Manifest.Permission.AccessCoarseLocation, true),
    };
}
