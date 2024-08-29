using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Positron
{

    public static class DictionaryExtensions4
    {
        public static TValue GetOrCreate<TKey, TValue>(
            this Dictionary<TKey, TValue> d,
            TKey key,
            Func<TKey, TValue> factory)
        {
            if (d.TryGetValue(key, out TValue value))
                return value;
            TValue v = factory(key);
            d[key] = v;
            return v;
        }
    }


    public readonly struct XamlMemberInfo
    {
        public readonly Type Type;
        public readonly MemberInfo MemberInfo;
        public readonly string Name;

        public XamlMemberInfo(Type type, MemberInfo member, string name)
        {
            this.Type = type;
            this.MemberInfo = member;
            this.Name = name;
        }

        public bool IsEmpty => Type == null && MemberInfo == null;
    }

    public static class TypeRegistry
    {

        private static Dictionary<Type, (string name, string script)> scriptCache = new Dictionary<Type, (string name, string script)>();

        public static Action<JSTypeContext> AdditionalClassCodeGenerator = null;
//        private static (string name, string script) Generate(Type type)
//        {
//            var memberProperties =
//                type.GetRuntimeProperties()
//                .Where(x => x.DeclaringType == type && x.GetAccessors().Length > 0)
//                .Select(x => (name: x.Name.ToCamelCase(), canRead: x.CanRead, canWrite: x.CanWrite, isStatic: x.GetAccessors().FirstOrDefault()?.IsStatic ?? false));

//            var memberMethods =
//                type.GetRuntimeMethods()
//                .Where(x => x.DeclaringType == type && !x.IsSpecialName && x.IsPublic)
//                .Select(x => (name: x.Name.ToCamelCase(), isStatic: x.IsStatic)).Distinct();

//            string typeName = type.Name;
//            int index = typeName.LastIndexOf('`');
//            if (index != -1)
//                typeName = typeName.Substring(0, index);

//            var sb = new StringBuilder();
//            if (type.BaseType != null && type.BaseType != typeof(object))
//            {
//                sb.AppendLine($"_$_extends({typeName}, _super);");
//            }
//            sb.AppendLine($@"function {typeName}() {{
//    return _invoker(null, null, 0, arguments);
//}};");

//            sb.AppendLine($"var prototype = {typeName}.prototype;");

//            JSTypeContext typeContext = new JSTypeContext()
//            {
//                type = type,
//                className = typeName,
//                writer = sb
//            };

//            foreach (var (name, canRead, canWrite, isStatic) in memberProperties)
//            {
//                string target;
//                string get;
//                string set;
//                if (!canRead && !canWrite) continue;
//                if (isStatic)
//                {
//                    target = typeName;
//                    get = canRead ? $"get: function() {{ return _invoker(null, '{name}', 1); }}," : "";
//                    set = canWrite ? $"set: function(v) {{ return _invoker(null, '{name}', 1, arguments); }}," : "";
//                }
//                else
//                {
//                    target = "prototype";
//                    get = canRead ? $"get: function() {{ return _invoker(this, '{name}', 1); }}," : "";
//                    set = canWrite ? $"set: function(v) {{ return _invoker(this, '{name}', 1, arguments); }}," : "";
//                }
//                var property = $@"Object.defineProperty({target}, '{name}', {{
//                    {get}
//                    {set}
//                    enumerable: true, configurable: true
//                }});";

//                (isStatic ? typeContext.StaticProperties : typeContext.Properties)[name] = property;
//            }

//            foreach (var (name, isStatic) in memberMethods)
//            {
//                if (isStatic)
//                {
//                    typeContext.StaticMethods[name] = $@"{typeName}.{name} = function() {{ return _invoker(null, '{name}', 0, arguments); }};";
//                }
//                else
//                {
//                    typeContext.Methods[name] = $@"prototype.{name} = function() {{ return _invoker(this, '{name}', 0, arguments); }};";
//                }
//            }

//            AdditionalClassCodeGenerator?.Invoke(typeContext);

//            foreach (var p in typeContext.Properties) sb.AppendLine(p.Value);

//            foreach (var p in typeContext.Methods) sb.AppendLine(p.Value);

//            foreach (var p in typeContext.StaticProperties) sb.AppendLine(p.Value);
//            foreach (var p in typeContext.StaticMethods) sb.AppendLine(p.Value);

//            sb.AppendLine($"return {typeName};");

//            return ("/clr/" + type.Namespace.Replace('.', '/') + "/" + typeName + ".js", sb.ToString());
//        }

        //public static (string name, string script) GenerateScript(Type type)
        //{
        //    return scriptCache.GetOrCreate(type, (k) => Generate(type));
        //}

        public static Dictionary<string, Assembly> Assemblies =>
            AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => !x.IsDynamic)
                    .ToDictionary(x => x.GetName().Name, x => x);


        public static bool IsOfType(this Type type, Type baseType)
        {
            return type == baseType || baseType.IsAssignableFrom(type);
        }

        private static Dictionary<string, XamlMemberInfo> cache = new Dictionary<string, XamlMemberInfo>();

        public static void Register(string name, Type type)
        {
            cache[name] = new XamlMemberInfo(type, null, name);
        }

        public static XamlMemberInfo Get(string name, bool throwIfNotFound = true)
        {
            var member = cache.GetOrCreate(name, k =>
            {
                var tokens = name.Split(':');
                if (tokens.Length > 1)
                {
                    var type = Get(tokens[1]).Type;

                    name = tokens[0];

                    var n = name + "Property";

                    MemberInfo ms = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                        .FirstOrDefault(x => x.Name.EqualsIgnoreCase(n));

                    if (ms == null)
                    {
                        ms = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .FirstOrDefault(x => x.Name.EqualsIgnoreCase(name));
                    }

                    if (ms == null)
                    {
                        if (throwIfNotFound)
                        {
                            throw new InvalidOperationException($"Property not found {name} on {type.Name}");
                        }
                        return default;
                    }

                    return new XamlMemberInfo(type, ms, name);
                }

                Type t1 = Type.GetType(name, false);
                if (throwIfNotFound)
                {
                    if (t1 == null)
                    {
                        throw new InvalidOperationException($"Type {name} not found");
                    }
                }
                return new XamlMemberInfo(t1 as Type, t1, name);
            });

            if (member.IsEmpty && throwIfNotFound)
                throw new NotImplementedException($"No type found for {name}");
            return member;
        }
    }
}
