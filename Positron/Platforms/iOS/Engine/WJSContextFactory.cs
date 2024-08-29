using Foundation;
using JavaScriptCore;
using Positron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace Positron
{

    partial class JSContextFactory
    {
        static partial void OnPlatformInit()
        {
            JSContextFactory.Instance = new WJSContextFactory();
        }


        [Preserve(AllMembers = true)]
        public class WJSContextFactory : JSContextFactory
        {

            public WJSContextFactory()
            {

            }

            private JSContext CreateContext()
            {
                var c = new JSContext();
                var g = c.GlobalObject;
                if (!g.HasProperty("global"))
                {
                    g.SetProperty(g, "global");
                }

                var text = "(function (t) { return function() { return t(this, Array.from(arguments));}})";
                // var text = "(function (t) { return function() { var a1 = arguments; var a = []; for(var i=0;i<a1.length;i++) a[i] = a1[i]; return t(this, a);};})";
                g.SetProperty(c.EvaluateScript(text), WJSContext.FunctionCreatorName);
                var a = g.GetProperty(WJSContext.FunctionCreatorName);
                if (a.IsUndefined)
                {
                    throw new Exception("a is undefined");
                }
                return c;
            }

            public override IJSContext Create()
            {
                var c = CreateContext();
                return new WJSContext(c);
            }

            public override IJSContext Create(Uri inverseWebSocketUri)
            {
                // warn...
                System.Diagnostics.Debug.WriteLine("This platform does not support full Inspector, only refresh is supported");
                // var context = new WJSContext(CreateContext());
                // var pc = new InspectorClient(context, inverseWebSocketUri);
                //context["disposeGlobal"] = context.CreateFunction(0, (c, a) => {
                //    pc.Dispose();
                //    return c.Undefined;
                //}, "disposeGlobal");
                // return context;
                var c = CreateContext();
                return new WJSContext(c);
            }
        }
    }
}