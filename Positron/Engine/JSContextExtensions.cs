using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NeuroSpeech.Positron.Delegates;

namespace NeuroSpeech.Positron;

public enum JSType
{
    Undefined,
    Object,
    Boolean,
    Number,
    BigInt,
    String,
    Symbol,
    Function,
    Unknown
}

public static class JSContextExtensions
{

    private const string classTemplate = @"var _$_extends = (this && this._$_extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();

// store all classes here..
var _$_classes = {};
";


    internal static MethodBase GetBestMatch(Type[] types, List<(MethodBase method, ParameterInfo[] parameters)> methods)
    {
        foreach(var (method, p) in methods)
        {
            bool success = true;
            for (int i = 0; i < p.Length; i++)
            {
                if (types.Length > i)
                {
                    if(!p[i].ParameterType.IsAssignableFrom(types[i]))
                    {
                        success = false;
                        break;
                    }
                } else
                {
                    if(!p[i].HasDefaultValue)
                    {
                        success = false;
                        break;
                    }
                }
            }
            if (success) return method;
        }
        throw new ArgumentException($"No parameter types match for method {methods.First().method.Name} for {string.Join(",", types.Select(t => t.Name))}");
    }

    internal static IJSValue[] Empty = new IJSValue[] { };

    private static ConcurrentDictionary<Type, ClrClassInterop> cache = new ConcurrentDictionary<Type, ClrClassInterop>();

    public static IJSValue CreateClass(this IJSContext context, Type type)
    {
        if (!context.HasProperty("_$_extends"))
        {
            context.Evaluate(classTemplate);

            // we need to add AbortController when the classes are being created first time.
            // context["AbortController"] =  context.CreateClass(typeof(AbortController));
        }

        var classes = context["_$_classes"];

        var fullName = type.AssemblyQualifiedName;

        var jsClass = classes[fullName];
        if (!jsClass.IsNull())
            return jsClass;

        var c = context.ClassFactory.Create(type);

        jsClass = c.CreateClass(context);
        classes[fullName] = jsClass;
        return jsClass;
    }

    /// <summary>
    /// Creates a class wrapper for given Type, this method creates a class along with its contructor signature
    /// </summary>
    /// <param name="type"></param>
    /// <param name="baseClass"></param>
    /// <returns></returns>
    //public static IJSValue CreateClass(this IJSContext context, Type type)
    //{
    //    if (!context.HasProperty("_$_extends"))
    //    {
    //        context.Evaluate(classTemplate);
    //    }

    //    var classes = context["_$_classes"];

    //    var fullName = type.AssemblyQualifiedName;

    //    var jsClass = classes[fullName];
    //    if (!jsClass.IsNull())
    //        return jsClass;

    //    IJSValue baseClass = null;

    //    if (type.BaseType != typeof(object))
    //    {
    //        baseClass = CreateClass(context, type.BaseType);
    //    }


    //    var propertyCache = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
    //        .Where(x => x.GetAccessors().Length > 0)
    //        .Select(x => (property: x, isStatic: x.GetAccessors().FirstOrDefault()?.IsStatic ?? false))
    //        .GroupBy(x => x.property.Name)
    //        .ToDictionary(x => x.Key.ToCamelCase(), x => x.ToList());

    //    var methodCache = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
    //        .Where(x => !x.IsSpecialName)
    //        .GroupBy(x => x.Name)
    //        .ToDictionary(x => x.Key.ToCamelCase(), x => x.Select(m => (method: (MethodBase)m, parameters: m.GetParameters())).ToList());

    //    var constructorCache = type.GetConstructors().Select(c => (method: (MethodBase)c, parameters: c.GetParameters())).ToList();

    //    var invoker = context.CreateFunction(4, (c, a) =>
    //    {
    //        var t0 = a[0];
    //        var t1 = a[1];
    //        var target = t0.IsNull() ? null : t0.Unwrap<object>();
    //        var name = t1.IsNull() ? null : t1.ToString();
    //        var memberType = a[2].DoubleValue;
    //        var args = a.Count > 3 ? a[3].ToArray().ToArray() : Empty;

    //        // is property?
    //        if (memberType == 1)
    //        {
    //            var px = propertyCache[name].FirstOrDefault(x => target == null ? x.isStatic : !x.isStatic).property;
    //            // this is set...
    //            if (args.Length > 0)
    //            {
    //                var type1 = px.PropertyType;
    //                var sv = args[0];
    //                px.SetValue(target, context.Deserialize(sv, type1));
    //                return sv;
    //            }
    //            return context.Marshal(px.GetValue(target), SerializationMode.WeakReference);
    //        }

    //        // find best possible method..
    //        var methods = name == null ? constructorCache : methodCache[name];
    //        MethodBase method = null;
    //        object[] pvalues = null;
    //        ParameterInfo[] parameters = null;
    //        if (methods.Count == 1)
    //        {
    //            (method, parameters) = methods[0];
    //            pvalues = parameters
    //            .Select((p, i) => i < args.Length ? c.Deserialize(args[i], p.ParameterType) : (p.HasDefaultValue ? p.DefaultValue : null)).ToArray();
    //        }
    //        else
    //        {
    //            // lets find best possible method...

    //            // search best fit
    //            // case 1, arguments may be less than parameters with rest of default values
    //            // case 2, number of arguments may match number of parameters but types may differ

    //            pvalues = new object[args.Length];
    //            var types = new Type[args.Length];

    //            for (int i = 0; i < args.Length; i++)
    //            {
    //                var a1 = args[i];
    //                if (a1.IsNull())
    //                {
    //                    pvalues[i] = null;
    //                    types[i] = typeof(object);
    //                    continue;
    //                }
    //                if (a1.IsString)
    //                {
    //                    pvalues[i] = a1.ToString();
    //                    types[i] = typeof(string);
    //                    continue;
    //                }
    //                if (a1.IsBoolean)
    //                {
    //                    pvalues[i] = a1.BooleanValue;
    //                    types[i] = typeof(bool);
    //                    continue;
    //                }
    //                if (a1.IsNumber)
    //                {
    //                    if (a1.DoubleValue % 1 == 0)
    //                    {
    //                        pvalues[i] = a1.IntValue;
    //                        types[i] = typeof(int);
    //                    }
    //                    else
    //                    {
    //                        pvalues[i] = a1.DoubleValue;
    //                        types[i] = typeof(double);
    //                    }
    //                    continue;
    //                }
    //                if (a1.IsDate)
    //                {
    //                    pvalues[i] = a1.DateValue;
    //                    types[i] = typeof(DateTime);
    //                    continue;
    //                }
    //                if (a1.IsWrapped)
    //                {
    //                    var t = a1.Unwrap<object>();
    //                    pvalues[i] = t;
    //                    types[i] = t.GetType();
    //                    continue;
    //                }
    //                pvalues[i] = a1;
    //                types[i] = typeof(object);
    //            }

    //            method = GetBestMatch(types, methods);


    //        }
    //        object retVal;
    //        if (method is ConstructorInfo ci)
    //        {
    //            retVal = ci.Invoke(pvalues);
    //        }
    //        else
    //        {
    //            retVal = method.Invoke(target, pvalues);
    //        }
    //        if (method is MethodInfo mi && mi.ReturnType == typeof(void))
    //            return c.Undefined;
    //        return c.Marshal(retVal, SerializationMode.WeakReference);

    //    }, $"{type.Name}_Invoke");

    //    var sb = TypeRegistry.GenerateScript(type);
    //    var eval = $"(function(_super, _invoker) {{ {sb.script } }})";

    //    try
    //    {
    //        var fx = context.Evaluate(eval, sb.name);

    //        // creates the given class
    //        jsClass = fx.InvokeFunction(fx, baseClass ?? context.Null, invoker);
    //        classes[fullName] = jsClass;
    //        return jsClass;
    //    }
    //    catch (Exception ex)
    //    {
    //        System.Diagnostics.Debug.WriteLine(ex);
    //        throw;
    //    }
    //}


    private static long id = 1;

    /// <summary>
    /// Evaluates given script with arguments. All arguments are stored in global storage before execution. After execution, they are removed from global storage.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="text"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static IJSValue EvaluateTemplate(
        this IJSContext context,
        FormattableString text,
        SerializationMode mode = SerializationMode.WeakReference)
    {
        var args = text.GetArguments().Select((o, i) =>
        {
            var n = id;
            do
            {
                var p = $"_$_etp_{n}_{i}";
                if (!context.HasProperty(p))
                {
                    context[p] = context.Marshal(o, mode);
                    return p;
                }
                n = System.Threading.Interlocked.Increment(ref id);

            } while (true);
            
        }).ToArray();

        string script = string.Format(text.Format, args);
        try
        {
            return context.Evaluate(script);
        } finally
        {
            foreach(var a in args)
            {
                context.DeleteProperty(a);
            }
        }
    }

    public static IJSValue GetOrCreate(this IJSContext context, string name, Func<IJSValue> factory)
    {
        return GetOrCreate(context, context.CreateString(name), factory);
    }

    public static IJSValue GetOrCreate(this IJSContext context, IJSValue keyOrSymbol, Func<IJSValue> factory)
    {
        var v = context[keyOrSymbol];
        if (v.IsNull())
        {
            v = factory();
            context[keyOrSymbol] = v;
        }
        return v;
    }

    public static IJSValue GetOrCreate(this IJSValue context, string name, Func<IJSValue> factory)
    {
        var v = context[name];
        if (v.IsNull())
        {
            v = factory();
            context[name] = v;
        }
        return v;
    }

    public static IJSValue GetOrCreate(this IJSValue context, IJSValue keyOrSymbol, Func<IJSValue> factory)
    {
        var v = context[keyOrSymbol];
        if (v.IsNull())
        {
            v = factory();
            context[keyOrSymbol] = v;
        }
        return v;
    }


    /// <summary>
    /// Evaluates `typeof obj` for this and returns appropriate value.
    /// 
    /// You must use this only if object's inbuilt IsObject, IsArray etc are not useful.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static JSType GetTypeOf(IJSValue value)
    {
        var type = value.Context.EvaluateTemplate($"typeof {value}").ToString();
        if(Enum.TryParse<JSType>(type, true, out var result))
        {
            return result;
        }
        return JSType.Unknown;
    }

    // static readonly MethodInfo awaitMethod = typeof(JSContextExtensions).GetMethod("Await");

    static readonly MethodInfo awaitPromiseMethod = typeof(JSContextExtensions).GetMethod("AwaitPromise");

    /// <summary>
    /// Broadcast message on channel to receive it inside ViewModel in JavaScript
    /// </summary>
    /// <param name="context"></param>
    /// <param name="channel"></param>
    /// <param name="data"></param>
    public static void Broadcast(this IJSContext context, string channel, IJSValue data)
    {
        context["bridge"].InvokeMethod("broadcast", context.CreateString(channel), data);
    }

    public static IJSValue CreateDisposableAction(this IJSContext context, Action dispose)
    {
        var obj = context.CreateObject();
        obj["dispose"] = context.CreateFunction(0, (c, a) => {
            dispose();
            return c.Undefined;
        }, "DisposableAction");
        return obj;
    }


    /// <summary>
    /// Retreives current Stack Position, useful for debugging.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetCurrentStack(this IJSContext context)
    {
        return context.Evaluate("(new Error()).stack").ToString();
    }

    public static async Task<IJSValue> Await<T>(IJSContext context, Task<T> task, SerializationMode mode) {
        var r = await task;
        return context.Marshal(r, mode);
    }

    public static Task<T> AwaitPromise<T>(IJSContext  context, IJSValue value)
    {
        TaskCompletionSource<T> source = new TaskCompletionSource<T>();
        value.InvokeMethod("then", context.CreateFunction(1, (c, a) => {
            var r = a[0];
            source.SetResult(r.IsNull() ? default : context.Deserialize<T>(r));
            return c.Undefined;
        }, "Task.then"));
        value.InvokeMethod("catch", context.CreateFunction(1, (c, a) => {
            var e = a[0];
            var error = "Unknown error";
            if (e != null) {
                if (e.HasProperty("stack"))
                {
                    error = e.ToString() + "\r\n" + e["stack"].ToString();
                } else {
                    error = e.ToString();
                }
            }
            source.SetException(new Exception(error));
            return c.Undefined;
        }, "Task.catch"));
        return source.Task;
    }

    /// <summary>
    /// Creates a JSON Style dictionary that you can access inside JavaScript.
    /// By default, As all objects are only passed as a wrapper, this is done to improve speed,
    /// as setting up property getter/setter for every property might be very slow
    /// </summary>
    /// <param name="context"></param>
    /// <param name="valueToCopy"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static IJSValue Marshal(this IJSContext context, object valueToCopy, SerializationMode mode = SerializationMode.WeakReference)
    {
        if (valueToCopy == null)
        {
            return context.Null;
        }
        if (valueToCopy is IJSValue jv) return jv;

        if (valueToCopy is Type type)
        {
            return context.CreateClass(type);
        }

        type = valueToCopy.GetType();

        type = Nullable.GetUnderlyingType(type) ?? type;
        if (type.IsEnum)
        {
            // if enums are wrapped, they are easily converted without string translation..
            return context.Wrap(valueToCopy);
            // return context.CreateString(valueToCopy.ToString());
        }
        var typeCode = Type.GetTypeCode(type);
        switch (typeCode)
        {
            case TypeCode.Byte:
                return context.CreateNumber((byte)valueToCopy);
            case TypeCode.SByte:
                return context.CreateNumber((sbyte)valueToCopy);
            case TypeCode.Char:
            case TypeCode.String:
                return context.CreateString(valueToCopy.ToString());
            case TypeCode.Int16:
                return context.CreateNumber((short)valueToCopy);
            case TypeCode.Int32:
                return context.CreateNumber((int)valueToCopy);
            case TypeCode.Int64:
                return context.CreateNumber((long)valueToCopy);
            case TypeCode.UInt16:
                return context.CreateNumber((ushort)valueToCopy);
            case TypeCode.UInt32:
                return context.CreateNumber((uint)valueToCopy);
            case TypeCode.UInt64:
                return context.CreateNumber((ulong)valueToCopy);
            case TypeCode.Double:
                return context.CreateNumber((double)valueToCopy);
            case TypeCode.Decimal:
                return context.CreateNumber((double)(decimal)valueToCopy);
            case TypeCode.Single:
                return context.CreateNumber((float)valueToCopy);
            case TypeCode.Boolean:
                return (bool)valueToCopy ? context.True : context.False;
            case TypeCode.DateTime:
                return context.CreateDate((DateTime)valueToCopy);
        }
        if (valueToCopy is IJSArray jsList) return jsList.ArrayObject;
        if (valueToCopy is System.Collections.IList || type.IsArray)
        {
            var array = context.CreateArray();
            foreach (var value in (System.Collections.IEnumerable)valueToCopy)
            {
                array.Add(context.Marshal(value, mode));
            }
            return array.ArrayObject;
        }
        if (valueToCopy is Task t)
        {
            if (type == typeof(Task))
            {
                return context.CreatePromise(t);
            }

            // convert to typed result...
            Type resultType = type.GetGenericArguments()[0];
            var createMethod = createPromiseMethod.MakeGenericMethod(resultType);
            return (IJSValue)createMethod.Invoke(null, new object[] { context, t });
        }
        if (valueToCopy is System.Collections.IDictionary || mode == SerializationMode.Copy)
        {
            return CopySerialize(context, valueToCopy, new List<object>());
        }

        if (valueToCopy is IJSService jss)
        {
            var js = JSService.Create(context, jss);
            return js;
        }

        var obj = context.Wrap(valueToCopy);
        obj["__proto__"] = context.CreateClass(type)["prototype"];
        return obj;
    }

    private static IJSValue CopySerialize(this IJSContext context, object valueToCopy, List<object> serialized)
    {
        if (valueToCopy == null)
        {
            return null;
        }
        if (valueToCopy is IJSValue jv) return jv;
        var type = valueToCopy.GetType();
        type = Nullable.GetUnderlyingType(type) ?? type;
        var typeCode = Type.GetTypeCode(type);
        switch (typeCode)
        {
            case TypeCode.Byte:
                return context.CreateNumber((byte)valueToCopy);
            case TypeCode.SByte:
                return context.CreateNumber((sbyte)valueToCopy);
            case TypeCode.Char:
            case TypeCode.String:
                return context.CreateString(valueToCopy.ToString());
            case TypeCode.Int16:
                return context.CreateNumber((short)valueToCopy);
            case TypeCode.Int32:
                return context.CreateNumber((int)valueToCopy);
            case TypeCode.Int64:
                return context.CreateNumber((long)valueToCopy);
            case TypeCode.UInt16:
                return context.CreateNumber((ushort)valueToCopy);
            case TypeCode.UInt32:
                return context.CreateNumber((uint)valueToCopy);
            case TypeCode.UInt64:
                return context.CreateNumber((ulong)valueToCopy);
            case TypeCode.Double:
                return context.CreateNumber((double)valueToCopy);
            case TypeCode.Decimal:
                return context.CreateNumber((double)(decimal)valueToCopy);
            case TypeCode.Single:
                return context.CreateNumber((float)valueToCopy);
            case TypeCode.Boolean:
                return (bool)valueToCopy ? context.True : context.False;
            case TypeCode.DateTime:
                return context.CreateDate((DateTime)valueToCopy);
        }
        if (valueToCopy is IJSArray jsList) return jsList.ArrayObject;

        if (serialized.Contains(valueToCopy))
            throw new NotSupportedException($"Serializing self referencing object via copy is not supported.");
        serialized.Add(valueToCopy);
        if (valueToCopy is System.Collections.IDictionary d)
        {
            var dobj = context.CreateObject();
            var de = d.GetEnumerator();
            while (de.MoveNext())
            {
                dobj[de.Key.ToString()] = context.Marshal(de.Value, SerializationMode.WeakReference);
            }
            return dobj;
        }
        if (valueToCopy is System.Collections.IList list) {
            var array = context.CreateArray();
            foreach(var value in list)
            {
                array.Add(context.CopySerialize(value, serialized));
            }
            return array.ArrayObject;
        }
        var obj = context.CreateObject();
        foreach(var p in valueToCopy.GetType().GetProperties()) {
            if (!p.CanRead) continue;
            obj[p.Name.ToCamelCase()] = context.CopySerialize(p.GetValue(valueToCopy), serialized);
        }
        return obj;
    }

    public static T Deserialize<T>(this IJSContext  context, IJSValue value)
    {
        return (T)Deserialize(context, value, typeof(T));
    }

    /// <summary>
    /// Deserialize given JavaScript value to CLR Object type, if the object was serialized using reference,
    /// it will be deseralized quickly, otherwise new copy of CLR object will be created with deep copy
    /// </summary>
    /// <param name="context"></param>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <returns></returns>
    public static object Deserialize(this IJSContext context, IJSValue  value, Type targetType)
    {
        if (value.IsNull())
        {
            if (targetType.IsValueType)
                return Activator.CreateInstance(targetType);
            return null;
        }
        if (targetType == typeof(IJSValue))
        {
            return value;
        }
        if (value.IsWrapped)
        {
            return value.Unwrap<object>();
        }
        if (targetType == typeof(object))
        {
            return value;
        }
        var t = Nullable.GetUnderlyingType(targetType);
        if (t != null)
        {
            targetType = t;
        }
        var code = Type.GetTypeCode(targetType);
        if (value.IsString  || targetType == typeof(string))
        {
            return value.ToString();
        }
        switch (code)
        {
            case TypeCode.Boolean:
                return value.IsBoolean ? value.BooleanValue : bool.Parse(value.ToString());
            case TypeCode.Single:
                return value.AsFloat();
            case TypeCode.Double:
                return value.AsDouble();
            case TypeCode.Decimal:
                return value.IsNumber ? (decimal)value.DoubleValue : decimal.Parse(value.ToString());
            case TypeCode.DateTime:
                return value.IsDate ? value.DateValue : DateTime.Parse(value.ToString());
            case TypeCode.Int16:
                return value.IsNumber ? (short)value.IntValue : short.Parse(value.ToString());
            case TypeCode.Int32:
                return value.IsNumber ? (int)value.IntValue : int.Parse(value.ToString()); 
            case TypeCode.Int64:
                return value.IsNumber ? (long)value.IntValue : long.Parse(value.ToString());
            case TypeCode.UInt16:
                return value.IsNumber ? (ushort)value.IntValue : ushort.Parse(value.ToString());
            case TypeCode.UInt32:
                return value.IsNumber ? (uint)value.IntValue : uint.Parse(value.ToString());
            case TypeCode.UInt64:
                return value.IsNumber ? (ulong)value.IntValue : ulong.Parse(value.ToString());
        }

        if (typeof(Delegate).IsAssignableFrom(targetType))
        {
            return JSDelegate.Create(targetType, context, value);
        }

        //if (targetType == typeof(bool))
        //{
        //    return value.IsBoolean? value.BooleanValue : bool.Parse(value.ToString());
        //}
        //if (targetType == typeof(float))
        //{
        //    return value.AsFloat();
        //}
        //if (targetType == typeof(double))
        //{
        //    return value.AsDouble();
        //}
        //if (targetType == typeof(decimal))
        //{
        //    return value.IsNumber ? (decimal)value.DoubleValue : decimal.Parse(value.ToString());
        //}
        //if (targetType == typeof(DateTime))
        //{
        //    return value.IsDate ? value.DateValue : DateTime.Parse(value.ToString());
        //}
        if (targetType == typeof(DateTimeOffset))
        {
            return value.IsDate ? new DateTimeOffset(value.DateValue, TimeSpan.FromSeconds(0)) : DateTimeOffset.Parse(value.ToString());
        }

        //if (targetType == typeof(int))
        //{
        //    return value.IsNumber ? value.IntValue : int.Parse(value.ToString());
        //}
        //if (targetType == typeof(long))
        //{
        //    return value.IsNumber ? (long)value.DoubleValue : long.Parse(value.ToString());
        //}

        //if (targetType == typeof(short))
        //{
        //    return (short)(value.IsNumber ? value.IntValue : int.Parse(value.ToString()));
        //}

        if (typeof(Task).IsAssignableFrom(targetType))
        {
            Type resultType = targetType == typeof(Task) ? typeof(object) : targetType.GetGenericArguments()[0];
            var rt = awaitPromiseMethod.MakeGenericMethod(resultType).Invoke(null, new object[] { context, value });
            return rt;
        }

        if (value.IsArray)
        {

            // this is case of AtomChips control binding
            if(targetType == typeof(System.Collections.IList) || targetType == typeof(System.Collections.IEnumerable))
            {
                return new AtomEnumerable(value);
            }

            if (targetType.IsArray)
            {
                // create new array...
                var et = targetType.GetElementType();
                var array = Array.CreateInstance(et, value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    array.SetValue( Deserialize(context, value[i], et),i);
                }
                return array;
            }

            // find type...
            if(!typeof(System.Collections.ICollection).IsAssignableFrom(targetType))
            {
                throw new InvalidCastException($"Unable to create type of {targetType} from Array");
            }

            var list = Activator.CreateInstance(targetType) as System.Collections.IList;

            var ice = targetType.GetInterfaces().FirstOrDefault(x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>));
            Type itemType = ice?.GetGenericArguments()?[0] ?? typeof(object);

            for (int i = 0; i < value.Length; i++)
            {
                list.Add(context.Deserialize(value[i], itemType));
            }
            return list;
        }
        if (value.IsObject)
        {

            var target = Activator.CreateInstance(targetType);

            if (typeof(System.Collections.IDictionary).IsAssignableFrom(targetType))
            {
                var d = target as System.Collections.IDictionary;
                var ice = targetType.GetInterfaces().FirstOrDefault(x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>));
                Type itemType = ice?.GetGenericArguments()?[1] ?? typeof(object);
                foreach (var kvp in value.Entries)
                {
                    d[kvp.Key] = context.Deserialize(kvp.Value, itemType);
                }
                return d;
            }
            else
            {
                foreach (var kvp in value.Entries)
                {
                    var px = targetType.GetProperty(kvp.Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreCase);
                    if (px == null) continue;
                    px.SetValue(target, context.Deserialize(kvp.Value, px.PropertyType));
                }
            }
            return target;
        }

        throw new NotImplementedException();

    }

    /// <summary>
    /// Converts given Task to JavaScript Promise
    /// </summary>
    /// <param name="context"></param>
    /// <param name="task"></param>
    /// <returns></returns>
    public static IJSValue CreatePromise(this IJSContext context, Task task)
    {
        var cp = context["__web_atoms_create_promise"];
        var promise = cp.InvokeFunction(cp);
        context.RunOnUIThread(async () => {
            try
            {
                await task;
                promise.InvokeMethod("r", context.Undefined);
            }
            catch (Exception ex)
            {
                promise.InvokeMethod("e", context.CreateString(ex.ToString()));
            }
        });
        return promise["promise"];
    }

    internal static System.Reflection.MethodInfo createPromiseMethod =
        typeof(JSContextExtensions).GetMethod("CreatePromiseWithResult");

    /// <summary>
    /// Converts given Task with Value to JavaScript Promise
    /// </summary>
    /// <param name="context"></param>
    /// <param name="task"></param>
    /// <returns></returns>
    public static IJSValue CreatePromiseWithResult<T>(this IJSContext context, Task<T> task)
    {
        var cp = context["__web_atoms_create_promise"];
        var promise = cp.InvokeFunction(cp);
        context.RunOnUIThread(async () => {
            try
            {
                var result = await task;
                promise.InvokeMethod("r", context.Marshal(result));
            }
            catch (Exception ex)
            {
                promise.InvokeMethod("e", context.CreateString(ex.ToString()));
            }
        });
        return promise["promise"];
    }


}
