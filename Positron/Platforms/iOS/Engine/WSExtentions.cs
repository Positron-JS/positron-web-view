using JavaScriptCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Positron
{
    internal static class WSExtentions
    {
        static JSValue[] EmptyJSValue = new JSValue[] { };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static JSValue[] ToJSArray(this IJSValue[] values)
        {
            int n = values.Length;
            if (n == 0)
            {
                return EmptyJSValue;
            }
            var v = new JSValue[values.Length];
            int i = 0;
            foreach (var item in values)
            {
                v[i++] = item == null ? null : ((WJSValue)item).value;
            }
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static JSValue[] ToJSArray(this IList<IJSValue> values)
        {
            int n = values.Count;
            if (n == 0)
            {
                return EmptyJSValue;
            }
            var v = new JSValue[n];
            int i = 0;
            foreach (var item in values)
            {
                v[i++] = item == null ? null : ((WJSValue)item).value;
            }
            return v;
        }


        static IJSValue[] Empty = new IJSValue[] { };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IJSValue[] ToWJSArray(this JSValue[] values, WJSContext context)
        {
            int n = values.Length;
            if (n == 0) return Empty;
            var v = new IJSValue[n];
            int i = 0;
            foreach (var item in values)
            {
                v[i++] = item == null || item.IsNull ? (IJSValue)null : new WJSValue(item);
            }
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue ToJSValue(this IJSValue value, JSContext context)
        {
            if (value == null) return JSValue.Null(context);
            return ((WJSValue)value).value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IJSValue ToIJSValue(this JSValue value) {
            return value == null || value.IsNull ? (IJSValue)null : new WJSValue(value);
        }

    }
}