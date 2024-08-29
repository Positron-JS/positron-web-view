using NeuroSpeech.Positron;
using System;
using System.Collections.Generic;
using System.Text;
using YantraJS.Core.Clr;

namespace YantraJS.Core;


public static class JSValueExt
{
    public static JSValue ToJSValue(this IJSValue value)
    {
        return value == null ? JSNull.Value : value as JSValue;
    }
}

public partial class JSValue : IJSValue
{
    IJSValue IJSValue.this[string name]
    {
        get => this[name];
        set => this[name] = value.ToJSValue();
    }
    IJSValue IJSValue.this[IJSValue keyOrSymbol]
    {
        get => this[keyOrSymbol as JSValue];
        set => this[keyOrSymbol as JSValue] = value.ToJSValue();
    }
    IJSValue IJSValue.this[int name]
    {
        get => this[(uint)name];
        set => this[(uint)name] = value.ToJSValue();
    }


    public IJSContext Context => JSContext.Current;

    public bool IsValueNull => this.IsNull;

    public bool IsDate => this is JSDate;

    public bool IsWrapped => this is ClrProxy;

    public long LongValue => this.BigIntValue;

    public float FloatValue => (float)this.DoubleValue;

    public DateTime DateValue => (this as JSDate).value.LocalDateTime;

    public IEnumerable<NeuroSpeech.Positron.JSProperty> Entries {
        get
        {
            foreach(var (Key, Value) in this.GetAllEntries(false))
            {
                yield return new NeuroSpeech.Positron.JSProperty(Key.ToString(), Value);
            }
        }
    }

    public string DebugView => this.ToDetailString();

    public IJSValue CreateNewInstance(params IJSValue[] args)
    {
        var a = new Arguments(args);
        return this.CreateInstance(a);
    }

    public IJSValue CreateNewInstance()
    {
        return this.CreateInstance(Arguments.Empty);
    }

    public IJSValue CreateNewInstance(IJSValue arg1)
    {
        return this.CreateInstance((JSValue)arg1);
    }

    public IJSValue CreateNewInstance(IJSValue arg1, IJSValue arg2)
    {
        return this.CreateInstance((JSValue)arg1, (JSValue)arg2);
    }

    public IJSValue CreateNewInstance(IJSValue arg1, IJSValue arg2, IJSValue arg3)
    {
        return this.CreateInstance((JSValue)arg1, (JSValue)arg2, (JSValue)arg3);
    }

    public IJSValue CreateNewInstance(IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4)
    {
        return this.CreateInstance((JSValue)arg1, (JSValue)arg2, (JSValue)arg3, (JSValue)arg4);
    }

    public IJSValue CreateNewInstance(IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5)
    {
        return this.CreateInstance(new Arguments(new JSValue[] { (JSValue)arg1, (JSValue)arg2, (JSValue)arg3, (JSValue)arg4, (JSValue) arg5 }));
    }

    public IJSValue CreateNewInstance(IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6)
    {
        return this.CreateInstance(new Arguments(new JSValue[] { (JSValue)arg1, (JSValue)arg2, (JSValue)arg3, (JSValue)arg4, (JSValue)arg5, (JSValue) arg6 }));
    }

    public IJSValue CreateNewInstance(IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6, IJSValue arg7)
    {
        return this.CreateInstance(new Arguments(new JSValue[] { (JSValue)arg1, (JSValue)arg2, (JSValue)arg3, (JSValue)arg4, (JSValue)arg5, (JSValue)arg6, (JSValue) arg7 }));
    }

    public IJSValue CreateNewInstance(IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6, IJSValue arg7, IJSValue arg8)
    {
        return this.CreateInstance(new Arguments(new JSValue[] { (JSValue)arg1, (JSValue)arg2, (JSValue)arg3, (JSValue)arg4, (JSValue)arg5, (JSValue)arg6, (JSValue)arg7, (JSValue) arg8 }));
    }

    public void DefineProperty(string name, JSPropertyDescriptor pd)
    {
        JSFunction pget = null;
        JSFunction pset = null;
        JSValue pvalue = null;
        var value = pd.Value as JSValue;
        var get = pd.Get as JSFunction;
        var set = pd.Set as JSFunction;
        var pt = JSPropertyAttributes.Empty;
        if (pd.Configurable ?? false)
            pt |= JSPropertyAttributes.Configurable;
        if (pd.Enumerable ?? false)
            pt |= JSPropertyAttributes.Enumerable;
        if (!pd.Writable ?? false)
            pt |= JSPropertyAttributes.Readonly;
        if (get != null)
        {
            pt |= JSPropertyAttributes.Property;
            pget = get;
        }
        if (set != null)
        {
            pt |= JSPropertyAttributes.Property;
            pset = set;
        }
        if (get == null && set == null)
        {
            pt |= JSPropertyAttributes.Value;
            pvalue = value;
            pget = value as JSFunction;
        }
        var pAttributes = pt;
        var target = this as JSObject;
        ref var ownProperties = ref target.GetOwnProperties();
        KeyString key = name;
        ownProperties[key.Key] = new JSProperty(key, pget, pset, pvalue, pAttributes);
    }

    public bool DeleteProperty(string name)
    {
        return this.Delete(name).BooleanValue;
    }

    public bool HasProperty(string name)
    {
        ref var ownProperties = ref (this as JSObject).GetOwnProperties();
        KeyString key = name;
        return ownProperties.HasKey(key.Key);
    }

    public IJSValue InvokeFunction(IJSValue thisValue, params IJSValue[] args)
    {
        var a = new Arguments(thisValue, args);
        return InvokeFunction(a);
    }

    public IJSValue InvokeFunction(IJSValue thisValue)
    {
        return this.Call((JSValue) thisValue);
    }

    public IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1)
    {
        return this.Call((JSValue)thisValue, (JSValue) arg1);
    }

    public IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1, IJSValue arg2)
    {
        return this.Call((JSValue)thisValue, (JSValue)arg1, (JSValue) arg2);
    }

    public IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1, IJSValue arg2, IJSValue arg3)
    {
        return this.Call((JSValue)thisValue, (JSValue)arg1, (JSValue)arg2, (JSValue) arg3);
    }

    public IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4)
    {
        return this.InvokeFunction(new Arguments(thisValue, new[] { arg1, arg2, arg3, arg4}));
    }

    public IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5)
    {
        return this.InvokeFunction(new Arguments(thisValue, new[] { arg1, arg2, arg3, arg4, arg5 }));
    }

    public IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6)
    {
        return this.InvokeFunction(new Arguments(thisValue, new[] { arg1, arg2, arg3, arg4, arg5, arg6 }));
    }

    public IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6, IJSValue arg7)
    {
        return this.InvokeFunction(new Arguments(thisValue, new[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 }));
    }

    public IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6, IJSValue arg7, IJSValue arg8)
    {
        return this.InvokeFunction(new Arguments(thisValue, new[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 }));
    }

    public IJSValue InvokeFunction(IJSValue thisValue, IList<IJSValue> args)
    {
        return this.InvokeFunction(new Arguments(thisValue, args.ToArray()));
    }

    public IJSValue InvokeMethod(string name, params IJSValue[] args)
    {
        var fx = GetMethod(name);
        var a = new Arguments(this, args);
        return fx(a);
    }

    public IJSValue InvokeMethod(string name)
    {
        var fx = GetMethod(name);
        var a = new Arguments(this);
        return fx(a);
    }

    public IJSValue InvokeMethod(string name, IJSValue arg1)
    {
        var fx = GetMethod(name);
        var a = new Arguments(this, (JSValue)arg1);
        return fx(a);

    }

    public IJSValue InvokeMethod(string name, IJSValue arg1, IJSValue arg2)
    {
        var fx = GetMethod(name);
        var a = new Arguments(this, (JSValue)arg1, (JSValue) arg2);
        return fx(a);
    }

    public IJSValue InvokeMethod(string name, IJSValue arg1, IJSValue arg2, IJSValue arg3)
    {
        var fx = GetMethod(name);
        var a = new Arguments(this, (JSValue)arg1, (JSValue)arg2, (JSValue) arg3);
        return fx(a);
    }

    public IJSValue InvokeMethod(string name, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4)
    {
        var fx = GetMethod(name);
        var a = new Arguments(this, (JSValue)arg1, (JSValue)arg2, (JSValue)arg3, (JSValue) arg4);
        return fx(a);
    }

    public IJSValue InvokeMethod(string name, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5)
    {
        var fx = GetMethod(name);
        var a = new Arguments(this, new[] { arg1, arg2, arg3, arg4, arg5} );
        return fx(a);
    }

    public IJSValue InvokeMethod(string name, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6)
    {
        var fx = GetMethod(name);
        var a = new Arguments(this, new[] { arg1, arg2, arg3, arg4, arg5, arg6 });
        return fx(a);
    }

    public IJSValue InvokeMethod(string name, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6, IJSValue arg7)
    {
        var fx = GetMethod(name);
        var a = new Arguments(this, new[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 });
        return fx(a);
    }

    public IJSValue InvokeMethod(string name, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6, IJSValue arg7, IJSValue arg8)
    {
        var fx = GetMethod(name);
        var a = new Arguments(this, new[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 });
        return fx(a);
    }

    public IList<IJSValue> ToArray()
    {
        return new AtomEnumerable(this);
    }

    public T Unwrap<T>()
    {
        return (T)(this as ClrProxy).value;
    }

    bool IJSValue.InstanceOf(IJSValue jsClass)
    {
        return this.InstanceOf(jsClass.ToJSValue()).BooleanValue;
    }
}
