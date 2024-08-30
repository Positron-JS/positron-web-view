using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NeuroSpeech.Positron;


public class ClrClassInterop
{
    public static Func<Type, ClrClassInterop> Factory = (t) => new ClrClassInterop(t);

    protected Type type;
    private string name;
    protected readonly List<(string name, PropertyInfo property)> properties = new List<(string name, PropertyInfo property)>();
    protected readonly Dictionary<string, List<MethodInfo>> methods = new Dictionary<string, List<MethodInfo>>();
    protected readonly Dictionary<string, List<MethodInfo>> staticMethods = new Dictionary<string, List<MethodInfo>>();
    protected readonly List<(string name, FieldInfo field)> fields = new List<(string name, FieldInfo field)>();
    protected readonly List<ConstructorInfo> constructors;

    public ClrClassInterop(Type type)
    {
        this.type = type;

        string typeName = type.Name;
        int index = typeName.LastIndexOf('`');
        if (index != -1)
            typeName = typeName.Substring(0, index);

        this.name = typeName;

        if (type.IsEnum)
            return;

        var list = this.properties;
        foreach(var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            list.Add((property.Name.ToCamelCase(), property));
        }

        foreach(var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            fields.Add((field.Name.ToCamelCase(), field));
        }

        foreach(var x in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            var key = x.Name.ToCamelCase();
            var target = x.IsStatic ? this.staticMethods : this.methods;
            if(target.TryGetValue(key, out var ml))
            {
                int i = 0;
                foreach(var existing in ml)
                {
                    if(existing.GetParameters().Length < x.GetParameters().Length)
                    {
                        break;
                    }
                    i++;
                }
                ml.Insert(i, x);
                continue;
            }
            ml = new List<MethodInfo> { x };
            target[key] = ml;
        }
        
        this.constructors = type.GetConstructors()
            .OrderByDescending(x => x.GetParameters()?.Length ?? 0)
            .ToList();

        this.NumberOfConstructorParameters = constructors.FirstOrDefault()?.GetParameters()?.Length ?? -1;
        this.EmptyConstructor = constructors.LastOrDefault(x => x.GetParameters()?.Length == 0);
    }

    private static object[] Empty = new object[] { };
    protected readonly int NumberOfConstructorParameters;
    protected readonly ConstructorInfo EmptyConstructor;

    public virtual IJSValue CreateClass(IJSContext context)
    {
        var maxParameters = NumberOfConstructorParameters;

        IJSValue constructor;
        constructor = context.CreateConstructor(maxParameters, (c, p) =>
            Create(c, p, constructors), name);

        if (type.IsEnum)
        {
            foreach(var v in type.GetEnumNames())
            {
                var key = v.ToCamelCase();
                constructor.DefineProperty(key, new JSPropertyDescriptor
                {
                    Value = context.Wrap(Enum.Parse(type, v)),
                    Enumerable = true,
                    Configurable = true,
                    Writable = true
                });
            }
            return constructor;
        }

        var prototype = constructor["prototype"];

        foreach(var (name,field) in fields)
        {
            if (field.IsStatic)
            {
                if (field.IsInitOnly)
                {
                    constructor.DefineProperty(name, new JSPropertyDescriptor { 
                        Enumerable = true,
                        Configurable = true,
                        Writable = true,
                        Get = context.CreateFunction(0, (c,a) => {
                            var v = c.Marshal(field.GetValue(null));
                            constructor.DefineProperty(name, new JSPropertyDescriptor { 
                                Enumerable = true,
                                Configurable = true,
                                Writable = true,
                                Value = v
                            });
                            return v;
                        }, name)
                    });
                    continue;
                }
                if (field.IsLiteral)
                {
                    constructor.DefineProperty(name, new JSPropertyDescriptor
                    {
                        Enumerable = true,
                        Configurable = true,
                        Writable = true,
                        Value = context.Marshal(field.GetRawConstantValue(), SerializationMode.Reference)
                    });
                    continue;
                }

                constructor.DefineProperty(name, new JSPropertyDescriptor { 
                    Enumerable = true,
                    Configurable = true,
                    Get = context.CreateFunction(0, (c,a) => 
                        c.Marshal( field.GetValue(null)) , name)
                });
                continue;
            }
            prototype.DefineProperty(name, new JSPropertyDescriptor
            {
                Enumerable = true,
                Configurable = true,
                Get = context.CreateBoundFunction(0, (c, t, a) => c.Marshal(field.GetValue(c.Deserialize(t,type))), name)
            });
        }

        foreach (var m in methods)
        {
            //if(m.Key == "toString")
            //{
            //    prototype.DefineProperty(m.Key, new JSPropertyDescriptor
            //    {
            //        Value = context.CreateBoundFunction(2, (c, t, p) => c.CreateString( c.Deserialize(t,type).ToString()), m.Key),
            //        Enumerable = true,
            //        Configurable = true,
            //        Writable = true
            //    });
            //    continue;
            //}
            if(m.Value.Count == 1)
            {
                prototype.DefineProperty(m.Key, new JSPropertyDescriptor
                {
                    Value = context.CreateBoundFunction(7, (c, t, p) => Invoke(t, c, p, m.Value[0]), m.Key),
                    Enumerable = true,
                    Configurable = true,
                    Writable = true
                });
                continue;
            }
            prototype.DefineProperty(m.Key, new JSPropertyDescriptor
            {
                Value = context.CreateBoundFunction(7, (c, t, p) => Invoke(t, c, p, m.Value), m.Key),
                Enumerable = true,
                Configurable = true,
                Writable = true
            });
        }
        foreach (var (key, property) in properties)
        {
            if (property.CanRead ? property.GetMethod.IsStatic : property.SetMethod.IsStatic)
            {
                constructor.DefineProperty(key, new JSPropertyDescriptor()
                {
                    Get = context.CreateFunction(2, (c, p) => c.Marshal(property.GetValue(null)), key),
                    Set = context.CreateFunction(2, (c, p) =>
                    {
                        var v = c.Deserialize(p[0], property.PropertyType);
                        property.SetValue(null, v);
                        return p[0];
                    }, key),
                    Configurable = true,
                    Enumerable = true,
                    Writable = true
                });
                continue;

            }
            CreateProperty(context, prototype, key, property);
        }


        foreach (var m in staticMethods)
        {
            if (m.Value.Count == 1)
            {
                var method = m.Value[0];                    
                constructor.DefineProperty(m.Key, new JSPropertyDescriptor
                {
                    Value = context.CreateFunction(7, (c, p) => Invoke(null, c, p, method), m.Key),
                    Enumerable = true,
                    Configurable = true,
                    Writable = true
                });
                continue;
            }
            constructor.DefineProperty(m.Key, new JSPropertyDescriptor
            {
                Value = context.CreateFunction(7, (c, p) => Invoke(null, c, p, m.Value), m.Key),
                Enumerable = true,
                Writable = true,
                Configurable = true
            });
        }
        if (type.BaseType != null)
        {
            var g = context["global"];
            var bc = context.CreateClass(type.BaseType);
            var Object = g["Object"];
            var bp = bc["prototype"];
            Object.InvokeMethod("setPrototypeOf", constructor, bc);
            Object.InvokeMethod("setPrototypeOf", prototype, bp);
        }

        return constructor;
    }

    protected virtual void CreateProperty(IJSContext context, IJSValue prototype, string key, PropertyInfo property)
    {
        var get = context.CreateBoundFunction(2, (c, t, p) =>
            c.Marshal(property.GetValue(c.Deserialize(t, type))), key);
        var set = context.CreateBoundFunction(2, (c, t, p) =>
        {
            var v = c.Deserialize(p[0], property.PropertyType);
            property.SetValue(c.Deserialize(t, type), v);
            return p[0];
        }, key);
        prototype.DefineProperty(key, new JSPropertyDescriptor()
        {
            Get = get,
            Set = set,
            Configurable = true,
            Enumerable = true,
            Writable = true
        });
    }

    private IJSValue Create(
        IJSContext c,
        IList<IJSValue> args, 
        List<ConstructorInfo> methods)
    {
        object retVal;
        var method = GetBestMatch(type, args, methods, out var clrArgs);
        retVal = method.Invoke(clrArgs);
        return c.Marshal(retVal, SerializationMode.WeakReference);
    }

    private IJSValue Invoke(
        IJSValue @this,
        IJSContext c,
        IList<IJSValue> args,
        MethodInfo method)
    {
        object retVal;
        var target = c.Deserialize(@this, type);
        var argTypes = method.GetParameters();
        var l = argTypes.Length;
        var p = new object[l];
        for (int i = 0; i < l; i++)
        {
            var pm = argTypes[i];
            if (i < args.Count)
            {
                var vi = args[i];
                if (vi.CanConvertTo(pm.ParameterType, out var cv))
                {
                    p[i] = cv;
                    continue;
                }
                p[i] = c.Deserialize(vi, pm.ParameterType);
                continue;
            }
            if(pm.HasDefaultValue)
            {
                p[i] = pm.DefaultValue;
                continue;
            }
            throw new InvalidOperationException($"Parameter count did not match");
        }
        retVal = method.Invoke(target, p);
        if (method.ReturnType == typeof(void))
            return c.Undefined;
        return c.Marshal(retVal, SerializationMode.WeakReference);
    }


    private IJSValue Invoke(
        IJSValue @this,
        IJSContext c,
        IList<IJSValue> args,
        List<MethodInfo> methods)
    {
        object retVal;
        var method = GetBestMatch(type, args, methods, out var clrArgs);
        var target = c.Deserialize(@this, type);
        retVal = method.Invoke(target, clrArgs);
        if (method.ReturnType == typeof(void))
            return c.Undefined;
        return c.Marshal(retVal, SerializationMode.WeakReference);
    }

    internal static T GetBestMatch<T>(Type type, IList<IJSValue> values, IList<T> methods, out object[] args)
        where T: MethodBase
    {
        var name = "";
        foreach(var m in methods)
        {
            name = m.Name;
            var ps = m.GetParameters();
            args = new object[ps.Length];
            bool success = true;
            for (int i = 0; i < ps.Length; i++)
            {
                var p = ps[i];
                if(values.Count > i)
                {
                    var a = values[i];
                    if (a.CanConvertTo(p.ParameterType, out var r))
                    {
                        args[i] = r;
                        continue;
                    }
                    success = false;
                    break;
                }
                if(p.HasDefaultValue)
                {
                    args[i] = p.DefaultValue;
                    continue;
                }
                success = false;
                break;
            }
            if(success)
                return m;
        }
        throw new ArgumentException($"No parameter types match for {type.FullName}.{name}");
    }

    internal static ConstructorInfo GetBestMatch(Type type, Type[] types, ConstructorInfo[] methods)
    {
        foreach (var method in methods)
        {
            var p = method.GetParameters();
            bool success = true;
            for (int i = 0; i < p.Length; i++)
            {
                if (types.Length > i)
                {
                    if (!p[i].ParameterType.IsAssignableFrom(types[i]))
                    {
                        success = false;
                        break;
                    }
                }
                else
                {
                    if (!p[i].HasDefaultValue)
                    {
                        success = false;
                        break;
                    }
                }
            }
            if (success) return method;
        }
        throw new ArgumentException($"No parameter types match for {type.FullName} for {string.Join(",", types.Select(t => t.Name))}");
    }

    internal static MethodInfo GetBestMatch(Type type, Type[] types, List<MethodInfo> methods)
    {
        foreach (var method in methods)
        {
            var p = method.GetParameters();
            bool success = true;
            for (int i = 0; i < p.Length; i++)
            {
                if (types.Length > i)
                {
                    if (!p[i].ParameterType.IsAssignableFrom(types[i]))
                    {
                        success = false;
                        break;
                    }
                }
                else
                {
                    if (!p[i].HasDefaultValue)
                    {
                        success = false;
                        break;
                    }
                }
            }
            if (success) return method;
        }
        throw new ArgumentException($"No parameter types match for method {type.FullName}.{methods.First().Name} for {string.Join(",", types.Select(t => t.Name))}");
    }


    private static (object[] pvalues,Type[] types) CreateMethodTypes(IList<IJSValue> args, int argCount)
    {
        // lets find best possible method...

        // search best fit
        // case 1, arguments may be less than parameters with rest of default values
        // case 2, number of arguments may match number of parameters but types may differ

        var pvalues = new object[argCount];
        var types = new Type[argCount];
        for (int i = 0; i < argCount; i++)
        {
            var a1 = args[i];
            if (a1.IsNull())
            {
                pvalues[i] = null;
                types[i] = typeof(object);
                continue;
            }
            if (a1.IsString)
            {
                pvalues[i] = a1.ToString();
                types[i] = typeof(string);
                continue;
            }
            if (a1.IsBoolean)
            {
                pvalues[i] = a1.BooleanValue;
                types[i] = typeof(bool);
                continue;
            }
            if (a1.IsNumber)
            {
                if (a1.DoubleValue % 1 == 0)
                {
                    pvalues[i] = a1.IntValue;
                    types[i] = typeof(int);
                }
                else
                {
                    pvalues[i] = a1.DoubleValue;
                    types[i] = typeof(double);
                }
                continue;
            }
            if (a1.IsDate)
            {
                pvalues[i] = a1.DateValue;
                types[i] = typeof(DateTime);
                continue;
            }
            if (a1.IsWrapped)
            {
                var t = a1.Unwrap<object>();
                pvalues[i] = t;
                types[i] = t.GetType();
                continue;
            }
            pvalues[i] = a1;
            types[i] = typeof(object);
        }
        return (pvalues, types);
    }
}
