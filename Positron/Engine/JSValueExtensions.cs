using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Positron.Delegates;

namespace Positron;

public static class JSValueExtensions
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double AsDouble(this IJSValue value)
    {
        if (value.IsNumber) return value.DoubleValue;
        if (double.TryParse(value.ToString(), out var v)) return v;
        return double.NaN;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float AsFloat(this IJSValue value)
    {
        if (value.IsNumber) return value.FloatValue;
        if (float.TryParse(value.ToString(), out var v)) return v;
        return float.NaN;
    }

    /// <summary>
    /// Use this method to test if value is null or not, sometimes JavaScript object returned
    /// may not be null in CLR but it's value may be null
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNull(this IJSValue value)
    {
        return value == null || value.IsValueNull || value.IsUndefined;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IJSValue GetAt(this IList<IJSValue> list, int i)
    {
        return i < list.Count ? list[i] : null;
    }

    public static bool CanConvertTo(this IJSValue value, Type type, out object result)
    {
        if (value.IsNull())
        {
            result = null;
            if (type.IsValueType)
            {
                return false;
            }
            return true;
        }
        if (type == typeof(IJSValue))
        {
            result = value;
            return true;
        }
        if (value.IsWrapped)
        {
            var v = value.Unwrap<object>();
            if (type.IsAssignableFrom(v.GetType()))
            {
                result = v;
                return true;
            }
            result = null;
            return false;
        }
        if (type.IsEnum && (value.IsString || value.IsObject))
        {
            var names = Enum.GetNames(type);
            var vs = value.ToString();
            var name = names.FirstOrDefault(x => string.Equals(x, vs, StringComparison.OrdinalIgnoreCase));
            if (name != null)
            {
                result = Enum.Parse(type, vs);
                return true;
            }
        }

        var any = type == typeof(object);

        if (any)
        {
            if (value.IsWrapped)
            {
                result = value.Unwrap<object>();
                return true;
            }
            if (value.IsNumber)
            {
                var d = value.DoubleValue;
                if (d % 1 == 0)
                {
                    long l = (long)d;
                    if (int.MinValue <= l && l <= int.MaxValue)
                    {
                        result = (int)d;
                    }
                    else
                    {
                        result = (long)d;
                    }
                }
                else
                {
                    result = d;
                }

                return true;
            }
            if (value.IsBoolean)
            {
                result = value.BooleanValue;
                return true;
            }
            if (value.IsDate)
            {
                result = value.DateValue;
                return true;
            }
            result = value.ToString();
            return true;
        }

        var tc = Type.GetTypeCode(type);
        switch (tc)
        {
            case TypeCode.Boolean:
                if (value.IsBoolean)
                {
                    result = value.BooleanValue;
                    return true;
                }
                break;
            case TypeCode.Byte:
                if (value.IsNumber)
                {
                    result = (byte)value.DoubleValue;
                    return true;
                }
                break;
            case TypeCode.Char:
                if (value.IsString)
                {
                    result = (char)value.ToString().FirstOrDefault();
                    return true;
                }
                if (value.IsNumber)
                {
                    result = (char)value.IntValue;
                    return true;
                }
                break;
            case TypeCode.DateTime:
                if (value.IsDate)
                {
                    result = (DateTime)value.DateValue;
                    return true;
                }
                break;
            case TypeCode.Decimal:
                if (value.IsNumber)
                {
                    result = (decimal)value.DoubleValue;
                    return true;
                }
                break;
            case TypeCode.Double:
                if (value.IsNumber)
                {
                    result = value.DoubleValue;
                    return true;
                }
                break;
            case TypeCode.Int16:
                if (value.IsNumber)
                {
                    result = (short)value.IntValue;
                    return true;
                }
                break;
            case TypeCode.Int32:
                if (value.IsNumber)
                {
                    result = value.IntValue;
                    return true;
                }
                break;
            case TypeCode.Int64:
                if (value.IsNumber)
                {
                    result = value.LongValue;
                    return true;
                }
                break;
            case TypeCode.SByte:
                if (value.IsNumber)
                {
                    result = (sbyte)value.IntValue;
                    return true;
                }
                break;
            case TypeCode.Single:
                if (value.IsNumber)
                {
                    result = (float)value.DoubleValue;
                    return true;
                }
                break;
            case TypeCode.UInt16:
                if (value.IsNumber)
                {
                    result = (ushort)value.IntValue;
                    return true;
                }
                break;
            case TypeCode.UInt32:
                if (value.IsNumber)
                {
                    result = (uint)value.IntValue;
                    return true;
                }
                break;
            case TypeCode.UInt64:
                if (value.IsNumber)
                {
                    result = (ulong)value.LongValue;
                    return true;
                }
                break;
            case TypeCode.String:
                if (value.IsString || value.IsObject)
                {
                    result = value.ToString();
                    return true;
                }
                break;
        }

        if (typeof(Delegate).IsAssignableFrom(type))
        {
            result = JSDelegate.Create(type, value.Context, value);
            return true;
        }

        var (nativeType, nativeValue) = value.GetNativeType();

        var tdc = System.ComponentModel.TypeDescriptor.GetConverter(type);
        if (tdc?.CanConvertFrom(nativeType) ?? false)
        {
            result = tdc.ConvertFrom(nativeValue)!;
            return true;
        }
        tdc = System.ComponentModel.TypeDescriptor.GetConverter(nativeType);
        if (tdc?.CanConvertTo(type) ?? false)
        {
            result = tdc.ConvertTo(nativeValue, type)!;
            return true;
        }


        result = null;
        return false;
    }

    private static (Type, object?) GetNativeType(this IJSValue value)
    {
        if (value.IsValueNull || value.IsUndefined)
        {
            return (typeof(object), null);
        }
        if (value.IsWrapped)
        {
            var v = value.Unwrap<object>();
            return (v.GetType(), v);
        }

        if (value.IsString)
        {
            return (typeof(string), value.ToString());
        }

        if (value.IsNumber)
        {
            var vl = (long)value.DoubleValue;
            if (value.LongValue == vl)
            {
                return (typeof(long), vl);
            }
            var vi = (int)value.IntValue;
            if (value.IntValue == vi)
            {
                return (typeof(int), vi);
            }
            return (typeof(double), value.DoubleValue);
        }
        if (value.IsBoolean)
        {
            return (typeof(bool), value.BooleanValue);
        }
        return (typeof(object), null);
    }

}
