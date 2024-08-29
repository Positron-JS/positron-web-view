using Foundation;
using JavaScriptCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Positron
{
    public class WJSValue : IJSValue
    {

        internal readonly JSValue value;
        public WJSValue(JSValue value)
        {
            this.value = value;
        }
        public IJSValue this[string name]
        {
            get => this.value.GetProperty(name).ToIJSValue();
            set => this.value.SetProperty(value.ToJSValue(this.value.Context), name);
        }

        public IJSContext Context => new WJSContext(this.value.Context);

        public bool IsValueNull => this.value.IsNull;

        public bool IsUndefined => this.value.IsUndefined;

        public bool IsNumber => this.value.IsNumber;

        public bool IsBoolean => this.value.IsBoolean;

        public bool IsString => this.value.IsString;

        public bool IsObject => this.value.IsObject;

        public bool IsDate => this.value.IsDate;

        public bool IsArray => this.value.IsArray;

        public bool IsWrapped {
            get {
                if (!this.value.IsObject)
                    return false;
                return this.value.HasProperty(WJSContext.SymbolName);
                //try
                //{
                //    return this.value.ToObject(JSWrapper.SelfClass) != null;
                //}catch (Exception ex)
                //{
                //    System.Diagnostics.Debug.WriteLine(ex);
                //    return false;
                //}
            }
        }

        public bool BooleanValue => IsBoolean ? this.value.ToBool() : false;

        public double DoubleValue => IsNumber ? this.value.ToDouble() : 0;

        public int IntValue => IsNumber ? this.value.ToInt32() : 0;

        public long LongValue => IsNumber ? (long)this.value.ToDouble() : 0L;

        public float FloatValue => IsNumber ? (float)this.value.ToDouble() : 0;

        public DateTime DateValue => IsDate ? (DateTime)this.value.ToDate() :DateTime.MinValue;

        public int Length {
            get => IsArray || IsObject ? this.value.GetProperty("length").ToInt32() : 0;
            set => this.value.SetProperty(JSValue.From(value, this.value.Context), "length");
        }

        public IEnumerable<JSProperty> Entries {
        
            get
            {
                var g = this.value.Context.GlobalObject;
                var globalObject = g.GetProperty("Object");
                var list = globalObject.Invoke("keys", this.value);
                int n = list.GetProperty("length").ToInt32();
                for (uint i = 0; i < n; i++)
                {
                    var key = list.GetValueAt(i);
                    var v = this.value.GetProperty(key.ToString()).ToIJSValue();
                    yield return new JSProperty(key.ToString(), v);
                }
            }
        }

        public string DebugView => this.value.ToString();

        public IJSValue this[int index] {
            get => this.value.GetValueAt((uint)index).ToIJSValue();
            set => this.value.SetValue( value.ToJSValue(this.value.Context), (uint)index);
        }

        public IJSValue this[IJSValue key]
        {
            get => this.value[((WJSValue)key).value].ToIJSValue();
            set => this.value[((WJSValue)key).value] = value.ToJSValue(this.value.Context);
        }

        public IJSValue CreateNewInstance(params IJSValue[] args)
        {
            var v = this.value.Construct(args.ToJSArray());
            return v.ToIJSValue();
        }

        public bool DeleteProperty(string name)
        {
            return this.value.DeleteProperty(name);
        }

        public IJSValue GetValueAt(int i)
        {
            return this.value.GetValueAt((uint)i).ToIJSValue();
        }

        public bool HasProperty(string name)
        {
            return this.value.HasProperty(name);
        }

        public IJSValue InvokeFunction(IJSValue thisValue, params IJSValue[] args)
        {
            var fx =  thisValue != null 
                ? this.value.Invoke("bind", thisValue.ToJSValue(this.value.Context))
                : this.value;
           
            var v = fx.Call(args.ToJSArray());
            return v.ToIJSValue();
        }

        public IJSValue InvokeMethod(string name, params IJSValue[] args)
        {
            var v = this.value.Invoke(name, args.ToJSArray());
            return v.ToIJSValue();
        }

        public bool InstanceOf(IJSValue value)
        {
            return this.value.IsInstanceOf(value.ToJSValue(this.value.Context));
        }

        public IList<IJSValue> ToArray()
        {
            return new AtomEnumerable(this);
        }

        public T Unwrap<T>()
        {
            if (this.IsValueNull)
                return default;
            if (!this.IsWrapped)
                return default;
            var v = (this.value.ToObject(JSWrapper.SelfClass) as JSWrapper);
            //if (!v.Target.IsAlive)
            //{
            //    return default;
            //}
            // throw new ObjectDisposedException("Wrapped object is disposed");
            // return (T)v.Target.Target;
            return v.As<T>();
        }

        public override string ToString()
        {
            if (this.IsWrapped)
            {
                return this.Unwrap<object>()?.ToString();
            }
            return this.value.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is WJSValue v)
            {
                if (this.IsUndefined)
                    return v.IsUndefined;
                if (this.IsValueNull)
                    return v.IsValueNull;
                if (this.IsWrapped)
                    return v.IsWrapped && this.Unwrap<object>() == v.Unwrap<object>();
                return value.IsEqualTo(v.value);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public void DefineProperty(string name, JSPropertyDescriptor descriptor)
        {
            var c = this.value.Context;
            var obj = JSValue.CreateObject(c);
            if (descriptor.Configurable != null) {
                obj.SetProperty(NSNumber.FromBoolean(descriptor.Configurable.Value), JSPropertyDescriptorKeys.Configurable);
            }
            if (descriptor.Enumerable != null)
            {
                obj.SetProperty(NSNumber.FromBoolean(descriptor.Enumerable.Value), JSPropertyDescriptorKeys.Enumerable);
            }
            if (descriptor.Get != null)
            {
                obj.SetProperty(descriptor.Get.ToJSValue(c), JSPropertyDescriptorKeys.Get);
            }
            if (descriptor.Set != null)
            {
                obj.SetProperty(descriptor.Set.ToJSValue(c), JSPropertyDescriptorKeys.Set);
            }
            if (descriptor.Value != null)
            {
                obj.SetProperty(descriptor.Value.ToJSValue(c), JSPropertyDescriptorKeys.Value);
                if (descriptor.Writable != null)
                {
                    obj.SetProperty(NSNumber.FromBoolean(descriptor.Writable.Value), JSPropertyDescriptorKeys.Writable);
                }
            }
            this.value.DefineProperty(name, obj);
        }

        private static JSValue[] Empty = new JSValue[] { };

        public IJSValue InvokeMethod(string name)
        {
            var v = this.value.Invoke(name, Empty);
            return v.ToIJSValue();
        }

        public IJSValue InvokeMethod(string name, IJSValue arg1)
        {
            var c = value.Context;
            var v = this.value.Invoke(name, 
                arg1.ToJSValue(c)
            );
            return v.ToIJSValue();
        }

        public IJSValue InvokeMethod(string name, IJSValue arg1, IJSValue arg2)
        {
            var c = value.Context;
            var v = this.value.Invoke(name,
                arg1.ToJSValue(c),
                arg2.ToJSValue(c)
            );
            return v.ToIJSValue();
        }

        public IJSValue InvokeMethod(string name, IJSValue arg1, IJSValue arg2, IJSValue arg3)
        {
            var c = value.Context;
            var v = this.value.Invoke(name,
                arg1.ToJSValue(c),
                arg2.ToJSValue(c),
                arg3.ToJSValue(c)
            );
            return v.ToIJSValue();
        }

        public IJSValue InvokeMethod(string name, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4)
        {
            var c = value.Context;
            var v = this.value.Invoke(name,
                arg1.ToJSValue(c),
                arg2.ToJSValue(c),
                arg3.ToJSValue(c),
                arg4.ToJSValue(c)
            );
            return v.ToIJSValue();
        }

        public IJSValue InvokeMethod(string name, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5)
        {
            var c = value.Context;
            var v = this.value.Invoke(name,
                arg1.ToJSValue(c),
                arg2.ToJSValue(c),
                arg3.ToJSValue(c),
                arg4.ToJSValue(c),
                arg5.ToJSValue(c)
            );
            return v.ToIJSValue();
        }

        public IJSValue InvokeMethod(string name, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6)
        {
            var c = value.Context;
            var v = this.value.Invoke(name,
                arg1.ToJSValue(c),
                arg2.ToJSValue(c),
                arg3.ToJSValue(c),
                arg4.ToJSValue(c),
                arg5.ToJSValue(c),
                arg6.ToJSValue(c)
            );
            return v.ToIJSValue();
        }

        public IJSValue InvokeMethod(string name, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6, IJSValue arg7)
        {
            var c = value.Context;
            var v = this.value.Invoke(name,
                arg1.ToJSValue(c),
                arg2.ToJSValue(c),
                arg3.ToJSValue(c),
                arg4.ToJSValue(c),
                arg5.ToJSValue(c),
                arg6.ToJSValue(c),
                arg7.ToJSValue(c)
            );
            return v.ToIJSValue();
        }

        public IJSValue InvokeMethod(string name, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6, IJSValue arg7, IJSValue arg8)
        {
            var c = value.Context;
            var v = this.value.Invoke(name,
                arg1.ToJSValue(c),
                arg2.ToJSValue(c),
                arg3.ToJSValue(c),
                arg4.ToJSValue(c),
                arg5.ToJSValue(c),
                arg6.ToJSValue(c),
                arg7.ToJSValue(c),
                arg8.ToJSValue(c)
            );
            return v.ToIJSValue();
        }

        public IJSValue InvokeFunction(IJSValue thisValue)
        {
            if(thisValue != null)
            {
                return this.value.Invoke("call", thisValue.ToJSValue(this.value.Context)).ToIJSValue();
            }
            //var fx = thisValue != null
            //    ? this.value.Invoke("bind", thisValue.ToJSValue(this.value.Context))
            //    : this.value;

            var v = value.Call(Empty);
            return v.ToIJSValue();
        }

        public IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1)
        {
            var c = value.Context;
            if (thisValue != null)
            {
                return this.value.Invoke("call",
                    thisValue.ToJSValue(this.value.Context),
                    arg1.ToJSValue(c)
                    ).ToIJSValue();
            }
            //var fx = thisValue != null
            //    ? this.value.Invoke("bind", thisValue.ToJSValue(this.value.Context))
            //    : this.value;

            var v = value.Call(
                arg1.ToJSValue(c)
            );
            return v.ToIJSValue();
        }

        public IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1, IJSValue arg2)
        {
            var c = value.Context;
            if (thisValue != null)
            {
                return this.value.Invoke("call",
                    thisValue.ToJSValue(this.value.Context),
                    arg1.ToJSValue(c),
                    arg2.ToJSValue(c)
                    ).ToIJSValue();
            }
            //var fx = thisValue != null
            //    ? this.value.Invoke("bind", thisValue.ToJSValue(this.value.Context))
            //    : this.value;

            var v = value.Call(
                arg1.ToJSValue(c),
                arg2.ToJSValue(c)
            );
            return v.ToIJSValue();
        }

        public IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1, IJSValue arg2, IJSValue arg3)
        {
            var c = value.Context;
            if (thisValue != null)
            {
                return this.value.Invoke("call",
                    thisValue.ToJSValue(this.value.Context),
                    arg1.ToJSValue(c),
                    arg2.ToJSValue(c),
                    arg3.ToJSValue(c)
                    ).ToIJSValue();
            }
            //var fx = thisValue != null
            //    ? this.value.Invoke("bind", thisValue.ToJSValue(this.value.Context))
            //    : this.value;

            var v = value.Call(
                arg1.ToJSValue(c),
                arg2.ToJSValue(c),
                arg3.ToJSValue(c)
            );
            return v.ToIJSValue();
        }

        public IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4)
        {
            var c = value.Context;
            if (thisValue != null)
            {
                return this.value.Invoke("call",
                    thisValue.ToJSValue(this.value.Context),
                    arg1.ToJSValue(c),
                    arg2.ToJSValue(c),
                    arg3.ToJSValue(c),
                    arg4.ToJSValue(c)
                    ).ToIJSValue();
            }
            //var fx = thisValue != null
            //    ? this.value.Invoke("bind", thisValue.ToJSValue(this.value.Context))
            //    : this.value;

            var v = value.Call(
                arg1.ToJSValue(c),
                arg2.ToJSValue(c),
                arg3.ToJSValue(c),
                arg4.ToJSValue(c)
            );
            return v.ToIJSValue();
        }

        public IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5)
        {
            var c = value.Context;
            if (thisValue != null)
            {
                return this.value.Invoke("call",
                    thisValue.ToJSValue(this.value.Context),
                    arg1.ToJSValue(c),
                    arg2.ToJSValue(c),
                    arg3.ToJSValue(c),
                    arg4.ToJSValue(c),
                    arg5.ToJSValue(c)
                    ).ToIJSValue();
            }
            //var fx = thisValue != null
            //    ? this.value.Invoke("bind", thisValue.ToJSValue(this.value.Context))
            //    : this.value;

            var v = value.Call(
                arg1.ToJSValue(c),
                arg2.ToJSValue(c),
                arg3.ToJSValue(c),
                arg4.ToJSValue(c),
                arg5.ToJSValue(c)
            );
            return v.ToIJSValue();
        }

        public IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6)
        {
            var c = value.Context;
            if (thisValue != null)
            {
                return this.value.Invoke("call",
                    thisValue.ToJSValue(this.value.Context),
                    arg1.ToJSValue(c),
                    arg2.ToJSValue(c),
                    arg3.ToJSValue(c),
                    arg4.ToJSValue(c),
                    arg5.ToJSValue(c),
                    arg6.ToJSValue(c)
                    ).ToIJSValue();
            }
            //var fx = thisValue != null
            //    ? this.value.Invoke("bind", thisValue.ToJSValue(this.value.Context))
            //    : this.value;

            var v = value.Call(
                arg1.ToJSValue(c),
                arg2.ToJSValue(c),
                arg3.ToJSValue(c),
                arg4.ToJSValue(c),
                arg5.ToJSValue(c),
                arg6.ToJSValue(c)
            );
            return v.ToIJSValue();
        }

        public IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6, IJSValue arg7)
        {
            var c = value.Context;
            if (thisValue != null)
            {
                return this.value.Invoke("call",
                    thisValue.ToJSValue(this.value.Context),
                    arg1.ToJSValue(c),
                    arg2.ToJSValue(c),
                    arg3.ToJSValue(c),
                    arg4.ToJSValue(c),
                    arg5.ToJSValue(c),
                    arg6.ToJSValue(c),
                    arg7.ToJSValue(c)
                    ).ToIJSValue();
            }
            //var fx = thisValue != null
            //    ? this.value.Invoke("bind", thisValue.ToJSValue(this.value.Context))
            //    : this.value;

            var v = value.Call(
                arg1.ToJSValue(c),
                arg2.ToJSValue(c),
                arg3.ToJSValue(c),
                arg4.ToJSValue(c),
                arg5.ToJSValue(c),
                arg6.ToJSValue(c),
                arg7.ToJSValue(c)
            );
            return v.ToIJSValue();
        }

        public IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6, IJSValue arg7, IJSValue arg8)
        {
            var c = value.Context;
            if (thisValue != null)
            {
                return this.value.Invoke("call",
                    thisValue.ToJSValue(this.value.Context),
                    arg1.ToJSValue(c),
                    arg2.ToJSValue(c),
                    arg3.ToJSValue(c),
                    arg4.ToJSValue(c),
                    arg5.ToJSValue(c),
                    arg6.ToJSValue(c),
                    arg7.ToJSValue(c),
                    arg8.ToJSValue(c)
                    ).ToIJSValue();
            }
            //var fx = thisValue != null
            //    ? this.value.Invoke("bind", thisValue.ToJSValue(this.value.Context))
            //    : this.value;

            var v = value.Call(
                arg1.ToJSValue(c),
                arg2.ToJSValue(c),
                arg3.ToJSValue(c),
                arg4.ToJSValue(c),
                arg5.ToJSValue(c),
                arg6.ToJSValue(c),
                arg7.ToJSValue(c),
                arg8.ToJSValue(c)
            );
            return v.ToIJSValue();
        }

        public IJSValue InvokeFunction(IJSValue thisValue, IList<IJSValue> args)
        {
            var c = value.Context;
            if(thisValue != null)
            {
                var count = args.Count;
                var jargs = new JSValue[count + 1];
                jargs[0] = thisValue.ToJSValue(c);
                for (int i = 0; i < count; i++)
                {
                    jargs[i + 1] = args[i].ToJSValue(c);
                }
                return value.Invoke("call", jargs).ToIJSValue();
            }
            var v = value.Call(args.ToJSArray());
            return v.ToIJSValue();

        }

        public IJSValue CreateNewInstance()
        {
            var v = this.value.Construct(Empty);
            return v.ToIJSValue();

        }

        public IJSValue CreateNewInstance(IJSValue arg1)
        {
            var c = value.Context;
            var v = this.value.Construct(
                arg1.ToJSValue(c)
                );
            return v.ToIJSValue();
        }

        public IJSValue CreateNewInstance(IJSValue arg1, IJSValue arg2)
        {
            var c = value.Context;
            var v = this.value.Construct(
                arg1.ToJSValue(c),
                arg2.ToJSValue(c)
                );
            return v.ToIJSValue();
        }

        public IJSValue CreateNewInstance(IJSValue arg1, IJSValue arg2, IJSValue arg3)
        {
            var c = value.Context;
            var v = this.value.Construct(
                arg1.ToJSValue(c),
                arg2.ToJSValue(c),
                arg3.ToJSValue(c)
                );
            return v.ToIJSValue();
        }

        public IJSValue CreateNewInstance(IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4)
        {
            var c = value.Context;
            var v = this.value.Construct(
                arg1.ToJSValue(c),
                arg2.ToJSValue(c),
                arg3.ToJSValue(c),
                arg4.ToJSValue(c)
                );
            return v.ToIJSValue();
        }

        public IJSValue CreateNewInstance(IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5)
        {
            var c = value.Context;
            var v = this.value.Construct(
                arg1.ToJSValue(c),
                arg2.ToJSValue(c),
                arg3.ToJSValue(c),
                arg4.ToJSValue(c),
                arg5.ToJSValue(c)
                );
            return v.ToIJSValue();
        }

        public IJSValue CreateNewInstance(IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6)
        {
            var c = value.Context;
            var v = this.value.Construct(
                arg1.ToJSValue(c),
                arg2.ToJSValue(c),
                arg3.ToJSValue(c),
                arg4.ToJSValue(c),
                arg5.ToJSValue(c),
                arg6.ToJSValue(c)
                );
            return v.ToIJSValue();
        }

        public IJSValue CreateNewInstance(IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6, IJSValue arg7)
        {
            var c = value.Context;
            var v = this.value.Construct(
                arg1.ToJSValue(c),
                arg2.ToJSValue(c),
                arg3.ToJSValue(c),
                arg4.ToJSValue(c),
                arg5.ToJSValue(c),
                arg6.ToJSValue(c),
                arg7.ToJSValue(c)
                );
            return v.ToIJSValue();
        }

        public IJSValue CreateNewInstance(IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6, IJSValue arg7, IJSValue arg8)
        {
            var c = value.Context;
            var v = this.value.Construct(
                arg1.ToJSValue(c),
                arg2.ToJSValue(c),
                arg3.ToJSValue(c),
                arg4.ToJSValue(c),
                arg5.ToJSValue(c),
                arg6.ToJSValue(c),
                arg7.ToJSValue(c),
                arg8.ToJSValue(c)
                );
            return v.ToIJSValue();
        }
    }
}