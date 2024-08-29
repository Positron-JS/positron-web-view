using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroSpeech.Positron;

public enum LogType
{
    Error,
    Warning,
    Trace
}

public class Positron
{
    private readonly WeakEventManager OnUrlRequestedEventManager = new WeakEventManager();
    private readonly WeakEventManager OnDeviceTokenUpdatedEventManager = new WeakEventManager();

    public event EventHandler? OnUrlRequested
    {
        add => OnUrlRequestedEventManager.AddEventHandler(value);
        remove => OnUrlRequestedEventManager.RemoveEventHandler(value);
    }
    public event EventHandler? OnDeviceTokenUpdated
    {
        add => OnDeviceTokenUpdatedEventManager.AddEventHandler(value);
        remove => OnDeviceTokenUpdatedEventManager.RemoveEventHandler(value);
    }

    public static Positron Instance { get; } = new Positron();
    public string? DeviceToken
    {
        get => deviceToken;
        set
        {
            deviceToken = value;
            OnDeviceTokenUpdatedEventManager?.HandleEvent(this, EventArgs.Empty, nameof(OnDeviceTokenUpdated));
        }
    }
    public string? UrlRequested
    {
        get => urlRequested;
        set
        {
            urlRequested = value;
            OnUrlRequestedEventManager.HandleEvent(this, EventArgs.Empty, nameof(OnUrlRequested));
        }
    }

    public Action<LogType, string> Log = delegate { };

    private string? deviceToken;
    private string? urlRequested;
}
