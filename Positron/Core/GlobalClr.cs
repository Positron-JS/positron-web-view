using System.Runtime.CompilerServices;

namespace Positron.Core
{
    public class GlobalClr
    {

        private static long rID = 1;

        private ConditionalWeakTable<object, string> references = new ConditionalWeakTable<object, string>();

        public GlobalClr()
        {
        }


        public Type? ResolveType(string typeName)
        {
            return Type.GetType(typeName);
        }

        public string Serialize(IJSValue value)
        {
            if (value.IsValueNull || value.IsUndefined)
            {
                return "null";
            }
            if (value.IsString)
            {
                return Serialize(value.ToString()!);
            }
            if (value.IsNumber)
            {
                return Serialize(value.DoubleValue!);
            }
            if (value.IsDate)
            {
                return Serialize(value.DateValue!);
            }
            if (value.IsBoolean)
            {
                return Serialize(value.BooleanValue!);
            }
            if (value.IsArray)
            {
                return Serialize(value.ToArray().Select((x) => SerializeAsync(x)).ToList());
            }
            if (value.IsObject)
            {
                var list = new List<string>();
                foreach (var item in value.Entries)
                {
                    list.Add($"\"{item.Key}\": {SerializeAsync(item.Value)}");
                }
                return "{" + string.Join(",", list) + "}";
            }
            if (value.IsWrapped)
            {
                var v = value.Unwrap<object>();
                throw new NotSupportedException($"You cannot transfer clr object to JavaScript");

            }
            return Serialize(value.ToString());
        }

        public async Task<string> SerializeAsync(IJSValue value)
        {
            if (value.IsValueNull || value.IsUndefined)
            {
                return "null";
            }
            if (value.IsString)
            {
                return Serialize(value.ToString()!);
            }
            if (value.IsNumber)
            {
                return Serialize(value.DoubleValue!);
            }
            if (value.IsDate)
            {
                return Serialize(value.DateValue!);
            }
            if(value.IsBoolean)
            {
                return Serialize(value.BooleanValue!);
            }
            if(value.IsArray)
            {
                return Serialize(value.ToArray().Select((x) => SerializeAsync(x)).ToList());
            }
            if(value.IsObject)
            {
                var list = new List<string>();
                foreach (var item in value.Entries)
                {
                    list.Add($"\"{item.Key}\": {SerializeAsync(item.Value)}");
                }
                return "{" + string.Join(",", list) + "}";
            }
            if (value.IsWrapped)
            {
                var v = value.Unwrap<object>();
                if (v is Task task)
                {
                    return await SerializeTask(task);
                }

                throw new NotSupportedException($"You cannot transfer clr object to JavaScript");

            }
            return Serialize(value.ToString());
        }

        private async Task<string> SerializeTask(Task task)
        {
            var t = task.GetType();
            if (t == typeof(Task))
            {
                await task;
                return "null";
            }

            var type = t.GetGenericArguments()[0];

            var r = await this.InvokeAs(type, InternalSerializeTask<object>, task);
            return r!;
        }

        public async Task<string> InternalSerializeTask<T>(Task t)
        {
            var task = (Task<T>)t;
            return this.Serialize(await task);
        }

        public string Serialize<T>(T item)
        {
            return System.Text.Json.JsonSerializer.Serialize<T>(item);
        }

        public string Serialize(object? obj)
        {
            if (obj == null)
            {
                return "null";
            }
            var type = obj.GetType();
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return this.Serialize((bool)obj);
                case TypeCode.Char:
                    return this.Serialize(new string((char)obj, 1));
                case TypeCode.SByte:
                    return this.Serialize((sbyte)obj);
                case TypeCode.Byte:
                    return this.Serialize((byte)obj);
                case TypeCode.Int16:
                    return this.Serialize((short)obj);
                case TypeCode.UInt16:
                    return this.Serialize((ushort)obj);
                case TypeCode.Int32:
                    return this.Serialize((int)obj);
                case TypeCode.UInt32:
                    return this.Serialize((uint)obj);
                case TypeCode.Int64:
                    return this.Serialize((long)obj);
                case TypeCode.UInt64:
                    return this.Serialize((ulong)obj);
                case TypeCode.Single:
                    return this.Serialize((Single)obj);
                case TypeCode.Double:
                    return this.Serialize((double)obj);
                case TypeCode.Decimal:
                    return this.Serialize((decimal)obj);
                case TypeCode.DateTime:
                    return this.Serialize((DateTime)obj);
                case TypeCode.String:
                    return this.Serialize((string)obj);
                default:
                    break;
            }
            if (obj is DateTimeOffset dt) {
                return this.Serialize((DateTimeOffset)obj);
            }
            if (obj is Exception ex)
            {
                return System.Text.Json.JsonSerializer.Serialize(new { error = ex.ToString() });
            }
            return Serialize<object>(obj);
        }
    }
}
