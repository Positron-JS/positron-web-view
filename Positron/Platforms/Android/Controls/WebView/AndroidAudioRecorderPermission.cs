namespace NeuroSpeech.Positron.Controls;

public class AndroidAudioRecorderPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new (string, bool)[] {
        (Android.Manifest.Permission.RecordAudio, true),
        (Android.Manifest.Permission.ModifyAudioSettings, true)
    };
}
