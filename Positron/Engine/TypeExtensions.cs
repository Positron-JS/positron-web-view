using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Positron
{
    public class WAMemberInfo<T>
        where T :MemberInfo
    {
        public readonly T Member;
        public readonly string Name;
        public readonly string FullName;
        public readonly string FullNameSet;
        public readonly string FullNameGet;
        public WAMemberInfo(T propertyInfo, Type type)
        {
            this.Member = propertyInfo;
            this.Name = propertyInfo.Name.ToCamelCase();
            this.FullName = $"{type.FullName}.{propertyInfo.Name}";
            this.FullNameGet = $"{FullName}.get";
            this.FullNameSet = $"{FullName}.set";
        }

    }

    public static class TypeExtensions
    {

        public static string GetJSTypeName(this Type type, bool fullName = false)
        {
            
            if (fullName)
            {
                var name = GetJSTypeName(type, false);
                if (!string.IsNullOrWhiteSpace(type.Namespace))
                    return type.Namespace + "." + name;
                return name;
            }
            if (type.IsConstructedGenericType)
            {                
                var g = GetJSTypeName(type.GetGenericTypeDefinition(), true).Replace(".","_");
                var args = type.GenericTypeArguments.Select(x => GetJSTypeName(x, true).Replace(".", "_"));
                return $"{g}${string.Join("$", args)}";
            }
            if (type.IsGenericTypeDefinition)
            {
                return type.Name.Split('`')[0];
            }
            return type.Name;
        }


        private static ConcurrentDictionary<Type, List<WAMemberInfo<PropertyInfo>>> propertyCache
            = new ConcurrentDictionary<Type, List<WAMemberInfo<PropertyInfo>>>();

        public static List<WAMemberInfo<PropertyInfo>> GetCachedProperties(this Type type)
        {
            return propertyCache.GetOrAdd(type, (k) => {
                return k.GetRuntimeProperties()
                    .Where((x) => x.CanRead && (x.GetIndexParameters() == null || x.GetIndexParameters().Length == 0))
                    .Select((x) => new WAMemberInfo<PropertyInfo>(x, type))
                    .ToList();
            });
        }


        private static ConcurrentDictionary<Type, List<WAMemberInfo<MethodInfo>>> methodCache
            = new ConcurrentDictionary<Type, List<WAMemberInfo<MethodInfo>>>();

        public static List<WAMemberInfo<MethodInfo>> GetCachedMethods(this Type type)
        {
            return methodCache.GetOrAdd(type, (k) => {
                return k.GetMethods(System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.InvokeMethod)
                    .Where((x) => !x.IsSpecialName)
                    .Select((x) => new WAMemberInfo<MethodInfo>(x, type))
                    .ToList();
            });
        }

    }
}
