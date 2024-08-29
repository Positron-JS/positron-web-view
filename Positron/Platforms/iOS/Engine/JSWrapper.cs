using Foundation;
using JavaScriptCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeuroSpeech.Positron;

//[Protocol]
//public partial interface IJSWrapper: IJSExport
//{
//    [Export("addEventListener::")]
//    JSValue AddEventListener(JSValue value, JSValue el);



//    [Export("appendChild:")]
//    void AppendChild(JSValue value);

//    [Export("dispatchEvent:")]
//    void DispatchEvent(JSValue value);
//}

public partial class JSWrapper: NSObject
    //, IJSWrapper
{

    // public WeakReference Target { get; }

    public object Target { get; }

    //public object instance;
    //private void Keep(object target)
    //{
    //    this.instance = target as DataTemplate;
    //}

    internal T As<T>()
    {
        // return (T)(Target.IsAlive ? Target.Target : throw new ObjectDisposedException("Object disposed"));
        return Target is T t 
            ? t 
            : throw new InvalidCastException(
                $"Unable to cast from {Target?.GetType()?.FullName} to {typeof(T).FullName}");
    }

    internal static ObjCRuntime.Class SelfClass;
    static JSWrapper()
    {
        SelfClass = new ObjCRuntime.Class(typeof(JSWrapper));
    }

    public JSWrapper(object target)
    {
        // this.Target = new WeakReference(target);
        // this.Keep(target);
        Target = target;
    }

    public static JSWrapper Register(JSContext context, object obj)
    {
        if (obj == null)
            throw new ArgumentNullException("Cannot register null");
        if (obj is JSWrapper w)
        {
            return w;
        }
        if (obj is JSManagedValue mv)
        {
            return Register(context, mv.Value);
        }
        return new JSWrapper(obj);
    }

    public static JSWrapper FromKey(JSValue key, bool throwIfNotFound = true)
    {
        if(! (key.ToObject(SelfClass) is JSWrapper wrapper))
        {
            if (throwIfNotFound) throw new InvalidOperationException($"Object is not of type JSWrapper");
            return null;
        }
        return wrapper;
    }

    //public JSValue Value(JSContext context)
    //{
    //    return JSValue.From(this, context);
    //}

    //public JSValue AddEventListener(JSValue eventName, JSValue handler)
    //{
    //    var bridge = eventName.Context.GlobalObject.GetProperty("bridge");
    //    return bridge.Invoke("addEventHandler", eventName, handler);
    //}

    //public void AppendChild(JSValue value)
    //{
    //    var wrapper = JSWrapper.FromKey(value);
    //    var t = this.Target.Target;
    //    XamlTypeInfo.Get(t.GetType()).Add(t, wrapper.As<object>());
    //}

    //public void DispatchEvent(JSValue value)
    //{
    //    var t = this.Target.Target;
    //    AttachedEvents.Invoke(t as Element, value.GetProperty("type").ToString(), new WJSValue(value));
    //}
}
