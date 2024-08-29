using Foundation;
using JavaScriptCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UIKit;

namespace Positron
{

    public class WJSContext : IJSContext
    {
        internal const string SymbolName = "_$_web__atoms__wrapped_reference_4923093249";

        private static ConditionalWeakTable<JSContext, List<WeakReference<EventHandler<ErrorEventArgs>>>> contextErrorHandlers
            = new ConditionalWeakTable<JSContext, List<WeakReference<EventHandler<ErrorEventArgs>>>>();

        public event EventHandler<ErrorEventArgs> ErrorEvent
        {
            add
            {
                if(!contextErrorHandlers.TryGetValue(this.context,out var errorHandlers))
                {
                    errorHandlers = new List<WeakReference<EventHandler<ErrorEventArgs>>>();
                    contextErrorHandlers.Add(context, errorHandlers);
                    context.ExceptionHandler = (s, e) => {
                        var arg = new ErrorEventArgs
                        {
                            Error = e.ToString(),
                            Stack = e.HasProperty("stack") ? e.GetProperty("stack").ToString() : null
                        };
                        List<WeakReference<EventHandler<ErrorEventArgs>>> deleted = null;
                        foreach (var h in errorHandlers)
                        {
                            if (!h.TryGetTarget(out var hh))
                            {
                                deleted = deleted ?? new List<WeakReference<EventHandler<ErrorEventArgs>>>();
                                deleted.Add(h);
                                continue;
                            }
                            hh(s, arg);
                        }
                        if (deleted != null)
                        {
                            foreach (var d in deleted)
                            {
                                errorHandlers.Remove(d);
                            }
                        }
                    };
                }
                errorHandlers.Add(new WeakReference<EventHandler<ErrorEventArgs>>(value));
            }
            remove
            {
                // this.context.ExceptionHandler = null;
                if (contextErrorHandlers.TryGetValue(this.context, out var errorHandlers))
                {
                    var index = errorHandlers.FindIndex((x) => x.TryGetTarget(out var hh) && hh == value);
                    if (index != -1)
                    {
                        errorHandlers.RemoveAt(index);
                    }
                }
            }
        }

        // internal static WJSValue Null;

        internal readonly JSContext context;
        internal readonly WJSValue Null;
        internal readonly WJSValue Undefined;
        internal readonly WJSValue True;
        internal readonly WJSValue False;

        internal const string FunctionCreatorName = "WebAtoms_Function_Creator";

        internal const string ConstructorCreatorName = "WebAtoms_Constructor_Creator";

        internal JSValue CreateFunctionInternal(JSValue fx)
        {
            var g = this.context.GlobalObject.GetProperty(FunctionCreatorName);
            var r = g.Call(fx);
            return r;
        }

        public ClrClassFactory ClassFactory { get; set; } = ClrClassFactory.Default;

        public WJSContext(JSContext context)
        {
            this.context = context;
            this.Null = new WJSValue(JSValue.Null(context));
            this.Undefined = new WJSValue(JSValue.Undefined(context));
            this.True = new WJSValue(JSValue.From(true, context));
            this.False = new WJSValue(JSValue.From(false, context));
        }

        public IJSValue CreateNull()
        {
            return new WJSValue(JSValue.Null(context));
        }

        public IJSValue CreateSymbol(string name)
        {
            return new WJSValue(JSValue.CreateSymbol(name, context));
        }

        public void RunOnUIThread(Func<Task> task)
        {
            UIApplication.SharedApplication.InvokeOnMainThread(async () => {
                try
                {
                    await task();
                } catch (Exception ex)
                {
                    Positron.Instance.Log(LogType.Error, ex.ToString());
                }
            });
        }

        IJSValue IJSContext.Undefined => Undefined;

        IJSValue IJSContext.Null => Null;

        IJSValue IJSContext.True => True;

        IJSValue IJSContext.False => False;

        public IJSValue Evaluate(string script, string location)
        {
            JSValue value;
            if (string.IsNullOrWhiteSpace(location))
                value = this.context.EvaluateScript(script);
            else
                value = this.context.EvaluateScript(script, NSUrl.FromString(location));
            return new WJSValue(value);
        }


        public Task EvaluateAsync(string script, string location)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(location))
                    this.context.EvaluateScript(script);
                else
                    this.context.EvaluateScript(script, NSUrl.FromString(location));
            });            
        }

        public IJSValue CreateString(string text)
        {
            return new WJSValue(JSValue.From(text, context));
        }

        public IJSValue CreateNumber(double number)
        {
            return new WJSValue(JSValue.From(number, context));
        }

        public IJSValue CreateDate(DateTime dt) {
            return new WJSValue(JSValue.From((NSDate)dt, context));
        }

        // public IJSValue Convert(object value)
        //{
        //    if (value == null)
        //    {
        //        return null;
        //    }
        //    if (value is NSNull)
        //    {
        //        return null;
        //    }
        //    JSValue v;
        //    if (value is IJSValue ijv) return ijv;
        //    if (value is JSValue jv)
        //    {
        //        if (jv.IsNull)
        //            return null;
        //        return new WJSValue(jv);
        //    }
        //    if (value is IJSService jvs)
        //    {
        //        // build...
        //        return JSService.Create(this, jvs);
        //    }
        //    if (value is string s || value.GetType().IsEnum)
        //        v = JSValue.From(value.ToString(), context);
        //    else if (value is int i)
        //        v = JSValue.From(i, context);
        //    else if (value is float f)
        //        v = JSValue.From(f, context);
        //    else if (value is double d)
        //        v = JSValue.From(d, context);
        //    else if (value is decimal dec)
        //        v = JSValue.From((double)dec, context);
        //    else if (value is bool b)
        //        v = JSValue.From(b, context);
        //    else if (value is DateTime dt)
        //        v = JSValue.From((NSDate)dt, context);
        //    else if (value is AtomEnumerable en) {
        //        return en.array;
        //    }
        //    //else if (value is System.Collections.IDictionary dictionary)
        //    //{
        //    //    return this.ToJSValue(dictionary);
        //    //}
        //    else if (value is Task<IJSValue> task)
        //    {
        //        return this.CreatePromiseWithResult(task);
        //    }
        //    else
        //    {
        //        var wrapped = new JSWrapper(value);
        //        v = JSValue.From((NSObject)wrapped, context);
        //        v.SetProperty((NSString)"wrapped", SymbolName);
        //        if (value is Element)
        //        {
        //            var w = new WJSValue(v);
        //            // w["_type"] = new WJSValue(JSValue.From(value.GetType().FullName, context));
        //            w.DefineProperty("expand", new JSPropertyDescriptor
        //            {
        //                Enumerable = true,
        //                Configurable = true,
        //                Get = CreateFunction(0, (c, a) =>
        //                {
        //                    return this.Serialize(value, SerializationMode.Reference);
        //                }, "Expand")
        //            });
        //            return w;
        //        }
        //    }
        //    return new WJSValue(v);
        //}

        public IJSValue Wrap(object value)
        {
            var wrapped = new JSWrapper(value);
            var v = JSValue.From(wrapped, context);
            v.SetProperty((NSString)"wrapped", SymbolName);
            // var p = v.GetProperty("__proto__");
            // var a1 = p.GetProperty("appendChild");
            // v.SetProperty(p.GetProperty("appendChild"), "appendChild");
            // v.SetProperty(p.GetProperty("dispatchEvent"), "dispatchEvent");

            //if (value is Element)
            //{
            //    var w = new WJSValue(v);
            //    // w["_type"] = new WJSValue(JSValue.From(value.GetType().FullName, context));
            //    w.DefineProperty("expand", new JSPropertyDescriptor
            //    {
            //        Enumerable = true,
            //        Configurable = true,
            //        Get = CreateFunction(0, (c, a) =>
            //        {
            //            return this.Marshal(value, SerializationMode.Reference);
            //        }, "Expand")
            //    });
            //    return w;
            //}

            return new WJSValue(v);
        }

        public IJSValue CreateObject()
        {
            return new WJSValue(JSValue.CreateObject(context));
        }

        public IJSValue CreateBoundFunction(int numberOfParameters,
            WJSBoundFunction func, string debugDescription) {
            var fx = JSClrFunction.From(this, 2, (c, p) =>
            {
                var @this = p[0];
                var @params = p[1];
                var v = func(c, @this.ToIJSValue(), new AtomEnumerable(@params.ToIJSValue()));
                return v.ToJSValue(c.context);
            }, debugDescription);

            // transfer array...

            // context.GlobalObject.SetProperty(fx,"_$_fx");
            fx = context.GlobalObject.Invoke(FunctionCreatorName, fx);

            return new WJSValue(fx);
        }

        public IJSValue CreateFunction(int numberOfParameters, Func<IJSContext, IList<IJSValue>, IJSValue> func, string debugDescription)
        {
            return new WJSValue(JSClrFunction.From(this, numberOfParameters, (c, p) =>
            {
                var v = func(c, p.ToWJSArray(this));
                return v.ToJSValue(c.context);
            }, debugDescription));
        }

        public IJSValue CreateConstructor(int numberOfParameters, Func<IJSContext, IList<IJSValue>, IJSValue> func, string name)
        {
            var fsx = JSClrFunction.From(this, numberOfParameters, (c, p) => {
                var v = func(c, p.ToWJSArray(this));
                return v.ToJSValue(c.context);
            }, name);

            var temp = context.EvaluateScript($"((p) => function {name}() {{ return p( ... arguments); }})");
            return new WJSValue(temp.Call(fsx));
        }


        public IJSArray CreateArray()
        {
            var a = context.GlobalObject.GetProperty("Array").Construct();
            return new AtomEnumerable(new WJSValue(a));
        }

        public bool HasProperty(string name)
        {
            return this.context.GlobalObject.HasProperty(name);
        }

        public IJSValue this[string name]
        {
            get => new WJSValue(this.context.GlobalObject.GetProperty(name));
            set => this.context.GlobalObject.SetProperty(value.ToJSValue(context), name);
        }

        public IJSValue this[IJSValue key]
        {
            get => this[key.ToString()];
            set => this[key.ToString()] = value;
        }

        public bool DeleteProperty(string name)
        {
            return this.context.GlobalObject.DeleteProperty(name);
        }

        public string Stack => this.context.EvaluateScript("(new Error('')).stack").ToString();

        private bool disposed;

        void IDisposable.Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            if (this.context.GlobalObject.HasProperty("disposeGlobal"))
            {
                this.context.GlobalObject.Invoke("disposeGlobal");
                this.context.GlobalObject.DeleteProperty("disposeGlobal");
            }
            this.context.Dispose();
        }
    }
}