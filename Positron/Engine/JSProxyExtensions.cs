using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroSpeech.Positron;

public static class JSProxyExtensions
{

    public static IJSValue CreateProxy(this IJSContext context, IJSProxy proxy)
    {
        var wrapped = context.Wrap(proxy);
        var trap = context.CreateObject();
        trap["get"] = context.CreateBoundFunction(3, proxy.Get , "get");
        trap["set"] = context.CreateBoundFunction(3, proxy.Set, "set");
        return context["Proxy"].CreateNewInstance(wrapped, trap);
    }

}
