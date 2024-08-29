using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using JavaScriptCore;
using UIKit;

namespace Positron
{
    [Protocol]
    public interface IJSClrFunction : IJSExport
    {

        [Export("c")]
        JSValue Execute();

    }

    [Protocol]
    public interface IJSClrFunction1 : IJSExport
    {

        [Export("c:")]
        JSValue Execute(JSValue a1);

    }

    [Protocol]
    public interface IJSClrFunction2 : IJSExport
    {

        [Export("c::")]
        JSValue Execute(JSValue a1, JSValue a2);

    }

    [Protocol]
    public interface IJSClrFunction3 : IJSExport
    {

        [Export("c:::")]
        JSValue Execute(JSValue a1, JSValue a2, JSValue a3);

    }

    [Protocol]
    public interface IJSClrFunction4 : IJSExport
    {

        [Export("c::::")]
        JSValue Execute(JSValue a1, JSValue a2, JSValue a3, JSValue a4);

    }

    [Protocol]
    public interface IJSClrFunction5 : IJSExport
    {

        [Export("c:::::")]
        JSValue Execute(JSValue a1, JSValue a2, JSValue a3, JSValue a4, JSValue a5);

    }

    [Protocol]
    public interface IJSClrFunction6 : IJSExport
    {

        [Export("c::::::")]
        JSValue Execute(JSValue a1, JSValue a2, JSValue a3, JSValue a4, JSValue a5, JSValue a6);

    }

    [Protocol]
    public interface IJSClrFunction7 : IJSExport
    {

        [Export("c:::::::")]
        JSValue Execute(JSValue a1, JSValue a2, JSValue a3, JSValue a4, JSValue a5, JSValue a6, JSValue a7);

    }

    [Protocol]
    public interface IJSClrFunction8 : IJSExport
    {

        [Export("c::::::::")]
        JSValue Execute(JSValue a1, JSValue a2, JSValue a3, JSValue a4, JSValue a5, JSValue a6, JSValue a7, JSValue v8);

    }

    [Protocol]
    public interface IJSClrFunction9 : IJSExport
    {

        [Export("c:::::::::")]
        JSValue Execute(JSValue a1, JSValue a2, JSValue a3, JSValue a4, JSValue a5, JSValue a6, JSValue a7, JSValue v8, JSValue v9);

    }

    [Protocol]
    public interface IJSClrFunction10 : IJSExport
    {

        [Export("c::::::::::")]
        JSValue Execute(JSValue a1, JSValue a2, JSValue a3, JSValue a4, JSValue a5, JSValue a6, JSValue a7, JSValue v8, JSValue v9, JSValue v10);

    }

    internal delegate JSValue JSInvoker(params JSValue[] args);

    public class JSClrFunction
    {

        public static JSValue From(WJSContext context, int args, Func<WJSContext, JSValue[], JSValue> func, string debugDescription)
        {
            JSInvoker invoker = (a) =>
            {
                try
                {
                    return func(context, a);
                }
                catch (Exception ex)
                {
                    JSContext jsContext = context.context;
                    jsContext.Exception = JSValue.CreateError(ex.ToString(), jsContext);
                    return JSValue.Undefined(jsContext);
                }
            };
            JSClrX fx = null;
            switch (args)
            {
                case 0:
                    fx = (new JSClrFunction0 { func = invoker });
                    break;
                case 1:
                    fx = (new JSClrFunction1 { func = invoker });
                    break;
                case 2:
                    fx = (new JSClrFunction2 { func = invoker });
                    break;
                case 3:
                    fx = (new JSClrFunction3 { func = invoker });
                    break;
                case 4:
                    fx = (new JSClrFunction4 { func = invoker });
                    break;
                case 5:
                    fx = (new JSClrFunction5 { func = invoker });
                    break;
                case 6:
                    fx = (new JSClrFunction6 { func = invoker });
                    break;
                case 7:
                    fx = (new JSClrFunction7 { func = invoker });
                    break;
                case 8:
                    fx = (new JSClrFunction8 { func = invoker });
                    break;
                case 9:
                    fx = (new JSClrFunction9 { func = invoker });
                    break;
                case 10:
                default:
                    fx = (new JSClrFunction10 { func = invoker });
                    break;
            }
            JSValue v = JSValue.From(fx, context.context);
            v = v.GetProperty("c").Invoke("bind", v);
            if (debugDescription != null)
            {
                v.SetProperty((NSString)debugDescription, "name");
            }
            return v;
        }

        public class JSClrX : NSObject
        {
            internal JSInvoker func;
        }

        public class JSClrFunction0 : JSClrX, IJSClrFunction
        {
            public JSValue Execute()
            {
                return func();
            }
        }

        public class JSClrFunction1 : JSClrX, IJSClrFunction1
        {
            public JSValue Execute(JSValue a1)
            {
                return func(a1);
            }
        }

        public class JSClrFunction2 : JSClrX, IJSClrFunction2
        {

            public JSValue Execute(JSValue a1, JSValue a2)
            {
                return func(a1, a2);
            }
        }

        public class JSClrFunction3 : JSClrX, IJSClrFunction3
        {

            public JSValue Execute(JSValue a1, JSValue a2, JSValue a3)
            {
                return func(a1, a2, a3);
            }
        }

        public class JSClrFunction4 : JSClrX, IJSClrFunction4
        {
            public JSValue Execute(JSValue a1, JSValue a2, JSValue a3, JSValue a4)
            {
                return func(a1, a2, a3, a4);
            }
        }

        public class JSClrFunction5 : JSClrX, IJSClrFunction5
        {
            public JSValue Execute(JSValue a1, JSValue a2, JSValue a3, JSValue a4, JSValue a5)
            {
                return func(a1, a2, a3, a4, a5);
            }
        }

        public class JSClrFunction6 : JSClrX, IJSClrFunction6
        {

            public JSValue Execute(JSValue a1, JSValue a2, JSValue a3, JSValue a4, JSValue a5, JSValue a6)
            {
                return func(a1, a2, a3, a4, a5, a6);
            }
        }

        public class JSClrFunction7 : JSClrX, IJSClrFunction7
        {

            public JSValue Execute(JSValue a1, JSValue a2, JSValue a3, JSValue a4, JSValue a5, JSValue a6, JSValue a7)
            {
                return func(a1, a2, a3, a4, a5, a6, a7);
            }
        }

        public class JSClrFunction8 : JSClrX, IJSClrFunction8
        {

            public JSValue Execute(JSValue a1, JSValue a2, JSValue a3, JSValue a4, JSValue a5, JSValue a6, JSValue a7, JSValue v8)
            {
                return func(a1, a2, a3, a4, a5, a6, a7, v8);
            }
        }

        public class JSClrFunction9 : JSClrX, IJSClrFunction9
        {

            public JSValue Execute(JSValue a1, JSValue a2, JSValue a3, JSValue a4, JSValue a5, JSValue a6, JSValue a7, JSValue v8, JSValue v9)
            {
                return func(a1, a2, a3, a4, a5, a6, a7, v8, v9);
            }
        }

        public class JSClrFunction10 : JSClrX, IJSClrFunction10
        {

            public JSValue Execute(JSValue a1, JSValue a2, JSValue a3, JSValue a4, JSValue a5, JSValue a6, JSValue a7, JSValue v8, JSValue v9, JSValue v10)
            {
                return func(a1, a2, a3, a4, a5, a6, a7, v8, v9, v10);
            }
        }


    }

}
