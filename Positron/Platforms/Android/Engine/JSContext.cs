using NeuroSpeech.Positron;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YantraJS.Core;
using YantraJS.Core.Clr;
using YantraJS.Emit;
using ErrorEventArgs = NeuroSpeech.Positron.ErrorEventArgs;

namespace YantraJS.Core;


public partial class JSContext : IJSContext
{

    IJSValue IJSContext.this[string name] {
        get => this[name];
        set => this[name] = value.ToJSValue(); 
    }
    IJSValue IJSContext.this[IJSValue keyOrSymbol] { 
        get => this[keyOrSymbol as JSValue]; 
        set => this[keyOrSymbol as JSValue] = value.ToJSValue(); 
    }

    //public IJSValue this[string name] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    //public IJSValue this[IJSValue keyOrSymbol] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public IJSValue Undefined => JSUndefined.Value;

    public IJSValue Null => JSNull.Value;

    public IJSValue True => JSBoolean.True;

    public IJSValue False => JSBoolean.False;

    public string Stack => new JSException("").JSStackTrace.ToString();

    public ClrClassFactory ClassFactory { get; set; } = ClrClassFactory.Default;

    public event EventHandler<ErrorEventArgs> ErrorEvent;

    partial void OnError(Exception ex) {
        ErrorEvent?.Invoke(this, new ErrorEventArgs() { 
             Error = ex.Message,
             Stack = ex.StackTrace
        });
    }

    public IJSArray CreateArray()
    {
        return new AtomEnumerable(new JSArray());
    }

    public IJSValue CreateDate(DateTime value)
    {
        return new JSDate(value);
    }

    public IJSValue CreateFunction(int numberOfParameters, Func<IJSContext, IList<IJSValue>, IJSValue> func, string debugDescription)
    {
        return new JSFunction((in Arguments a) => {
            return func(this, a.ToList()).ToJSValue();
        }, debugDescription, numberOfParameters);
    }

    public IJSValue CreateNumber(double number)
    {
        return new JSNumber(number);
    }

    public IJSValue CreateObject()
    {
        return new JSObject();
    }

    public IJSValue CreateString(string text)
    {
        return new JSString(text);
    }

    public IJSValue CreateSymbol(string name)
    {
        return new JSSymbol(name);
    }

    public IJSValue Evaluate(string script, string location = null)
    {
        using var s = this.SetTemporaryCodeCache();
        return this.Eval(script, location);
    }

    private IDisposable SetTemporaryCodeCache()
    {
        var oldCodeCache = this.CodeCache;
        this.CodeCache = new DictionaryCodeCache();
        return new DisposableAction(() =>
        {
            this.CodeCache = oldCodeCache;
        });
    }

    public void RunOnUIThread(Func<Task> task)
    {
        var current = this;
        synchronizationContext.Post(async (a) => {
            JSContext.Current = current;
            _current.Value = current;
            Func<Task> t = a as Func<Task>;
            try
            {
                await t();
            } catch (Exception ex)
            {
                ReportError(ex);
            }
        }, task);
    }

    public IJSValue Wrap(object value)
    {
        return ClrProxy.From(value);
    }

    public Task EvaluateAsync(string script, string location = null)
    {
        return Task.FromResult(this.Eval(script, location));
    }

    public IJSValue CreateConstructor(int numberOfParameters, Func<IJSContext, IList<IJSValue>, IJSValue> func, string name)
    {
        return new JSFunction((in Arguments a) => {
            var list = new List<IJSValue>();
            for (int i = 0; i < a.Length; i++)
            {
                list.Add(a[i]);
            }
            return (JSValue)func(this, list);
        }, name, $"class {name} {{}}", createPrototype: true);
    }

    public IJSValue CreateBoundFunction(int numberOfParameters, WJSBoundFunction func, string debugDescription)
    {
        return new JSFunction((in Arguments a) => {
            var list = new List<IJSValue>();
            for (int i = 0; i < a.Length; i++)
            {
                list.Add(a[i]);
            }
            return (JSValue)func(this, a.This, list);
        }, debugDescription);
    }
}
