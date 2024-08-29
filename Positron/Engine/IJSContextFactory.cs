using System;

namespace NeuroSpeech.Positron;

/// <summary>
/// Creates JavaScript Engine that implements IJSContext interface
/// </summary>
public abstract partial class JSContextFactory
{

    static JSContextFactory()
    {
        OnPlatformInit();
    }

    public static JSContextFactory Instance;

    static partial void OnPlatformInit();

    /// <summary>
    /// Creates new JavaScript Engine
    /// </summary>
    /// <returns></returns>
    public abstract IJSContext Create();

    public abstract IJSContext Create(Uri inverseWebSocketUri);
}
