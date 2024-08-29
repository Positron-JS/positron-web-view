using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuroSpeech.Positron;


public interface IJSService
{
}

public class JSService
{


    public static IJSValue Create(IJSContext context, IJSService value)
    {
        var jobj = context.CreateObject();
        var type = value.GetType();

        foreach (var m in type.GetCachedMethods())
        {
            var item = m.Member;
            try
            {
                var paramList = item.GetParameters();
                int total = paramList.Length;
                IJSValue clrFunction;

                if (paramList.Length == 0)
                {
                    clrFunction = context.CreateFunction(0, (c, a) => {
                        return c.Marshal(item.Invoke(value, new object[] { }));
                    }, m.FullName);
                }
                else
                {
                    if (paramList[0].ParameterType == typeof(IJSContext))
                    {
                        if (paramList.Length == 2 && paramList[1].ParameterType == typeof(IList<IJSValue>))
                        {
                            if (item.ReturnType != typeof(IJSValue))
                            {
                                throw new ArgumentException("Return type of Method with IList<IJSValue> parameter must be IJSValue");
                            }
                            clrFunction = context.CreateFunction(-1, (c, x) =>
                            {
                                return (IJSValue)item.Invoke(value, new object[] { c, x });
                            }, m.FullName);

                        }
                        else
                        {

                            clrFunction = context.CreateFunction(total, (c, x) =>
                            {

                                var plist = new object[total];
                                int i = 0;
                                int px = 0;
                                int l = x.Count;
                                foreach (var p in paramList)
                                {
                                    if (i == 0)
                                    {
                                        plist[0] = context;
                                        i++;
                                        continue;
                                    }
                                    plist[i++] = px >= l ? null : c.Deserialize(x[px++], p.ParameterType);
                                }
                                return c.Marshal(item.Invoke(value, plist));
                            }, m.FullName);
                        }
                    }
                    else
                    {
                        clrFunction = context.CreateFunction(total, (c, x) =>
                        {

                            var plist = new object[total];
                            int i = 0;
                            int l = x.Count;
                            foreach (var p in paramList)
                            {
                                plist[i] = i >= l ? null : c.Deserialize(x[i++], p.ParameterType);
                            }
                            return c.Marshal(item.Invoke(value, plist));
                        }, m.FullName);
                    }

                }
                jobj[m.Name] = clrFunction;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to execute {item.Name}");
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        foreach (var cp in type.GetCachedProperties())
        {
            var property = cp.Member;
            try
            {
                var d = new JSPropertyDescriptor
                {
                    Enumerable = true,
                    Configurable = true
                };
                if (property.CanRead)
                {
                    d.Get = context.CreateFunction(0, (c, x) => 
                        c.Marshal(property.GetValue(value)), cp.FullNameGet);
                }
                if (property.CanWrite)
                {
                    d.Set = context.CreateFunction(1, (c, x) =>
                    {
                        var a = x[0];
                        property.SetValue(value, c.Deserialize(a, property.PropertyType));
                        return a;
                    }, cp.FullNameSet);
                }
                jobj.DefineProperty(cp.Name, d);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to execute {property.Name}");
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        return jobj;
    }

}
