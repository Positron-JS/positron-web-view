using System;
using System.Collections.Generic;
using System.Text;

namespace NeuroSpeech.Positron;

public class ClrClassFactory
{

    public static ClrClassFactory Default = new ClrClassFactory();

    private Dictionary<Type, ClrClassInterop> cache = new Dictionary<Type, ClrClassInterop>();

    public ClrClassInterop Create(Type type)
    {
        return cache.GetOrCreate(type, Factory);
    }

    protected virtual ClrClassInterop Factory(Type arg)
    {
        return new ClrClassInterop(arg);
    }
}
