using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YantraJS.Core;

namespace Positron
{
    partial class JSContextFactory
    {
        static partial void OnPlatformInit()
        {
            JSContextFactory.Instance = new YantraContextFactory();
        }


        internal class YantraContextFactory : JSContextFactory
        {
            private JSContext CreateContext()
            {
                var c = new JSContext();
                c[KeyStrings.global] = c;
                return c;
            }

            public override IJSContext Create()
            {
                return this.CreateContext();
            }

            public override IJSContext Create(Uri inverseWebSocketUri)
            {
                return this.CreateContext();
            }
        }
    }
}
