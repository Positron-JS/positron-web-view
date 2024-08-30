using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace NeuroSpeech.Positron;


[DebuggerDisplay("{Value}", Name = "{Key}")]
public struct JSProperty
{

    readonly string key;
    readonly IJSValue value;
    public JSProperty(string key, IJSValue value)
    {
        this.value = value;
        this.key = key;
    }

    public string Key => this.key;
    public IJSValue Value => this.value;
}

/// <summary>
/// This is an interface that represents JavaScript value, since different JavaScript engines may have different type of classes
/// , we decided to make IJSValue that represents all operations with JavaScript type and it lets us write platform and engine independent
/// code.
/// 
/// </summary>
public interface IJSValue
{

    /// <summary>
    /// Attached JavaScript context.
    /// </summary>
    IJSContext Context { get; }

    /// <summary>
    /// Since JavaScript can return a value which may not be null in CLR but may be null in JavaScript
    /// so you can use this value to detect if the object is really null
    /// </summary>
    bool IsValueNull { get; }

    /// <summary>
    /// Check if value is undefined, for JavaScript you may receive a value in your methods that may not be null but may be undefined.
    /// </summary>
    bool IsUndefined { get; }

    /// <summary>
    /// True if underlying value is a number
    /// </summary>
    bool IsNumber { get; }

    /// <summary>
    /// True if underlying value is a boolean
    /// </summary>
    bool IsBoolean { get; }

    /// <summary>
    /// True if underlying value is a string
    /// </summary>
    bool IsString { get; }

    /// <summary>
    /// True if underlying value is an object/wrapper/function/date
    /// </summary>
    bool IsObject { get; }

    /// <summary>
    /// True if underlying value is a Date
    /// </summary>

    bool IsDate { get; }

    /// <summary>
    /// True if underlying value is an Array
    /// </summary>
    bool IsArray { get; }

    /// <summary>
    /// True if underlying value is a wrapped object, Wrapped objects are passed using Weak References and do not have any methods
    /// or properties that you can call from JavaScript. You have to expose services using classes that implements from IJSService interface
    /// </summary>
    bool IsWrapped { get; }

    bool IsSymbol { get; }

    /// <summary>
    /// Boolean value
    /// </summary>
    bool BooleanValue { get; }

    /// <summary>
    /// Double value
    /// </summary>
    double DoubleValue { get; }

    /// <summary>
    /// Int32 value
    /// </summary>
    int IntValue { get; }

    /// <summary>
    /// Int64 value, though Int64 is not available in JavaScript, this value is actually deserialized as string and parsed into Long
    /// </summary>
    long LongValue { get; }

    /// <summary>
    /// Float value
    /// </summary>
    float FloatValue { get; }

    /// <summary>
    /// Date Value
    /// </summary>
    DateTime DateValue { get; }


    IJSValue CreateNewInstance();
    IJSValue CreateNewInstance(IJSValue arg1);
    IJSValue CreateNewInstance(IJSValue arg1, IJSValue arg2);
    IJSValue CreateNewInstance(IJSValue arg1, IJSValue arg2, IJSValue arg3);

    IJSValue CreateNewInstance(IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4);

    IJSValue CreateNewInstance(IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5);

    IJSValue CreateNewInstance(IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6);

    IJSValue CreateNewInstance(IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6, IJSValue arg7);

    IJSValue CreateNewInstance(IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6, IJSValue arg7, IJSValue arg8);

    /// <summary>
    /// If this is a Function, it will create new instance from this value
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    IJSValue CreateNewInstance(params IJSValue[] args);

    /// <summary>
    /// Get/Set JavaScript properties, you may get null or IJSValue which may be null or undefined
    /// </summary>
    /// <param name="name">Case sensitive property name</param>
    /// <returns></returns>
    IJSValue this[string name] { get; set; }

    /// <summary>
    /// Store/retrive objects via IJSValue key, it is recommended to use string/int indexer, this indexer
    /// should only be used for Symbols.
    /// 
    /// If you have already received IJSValue from somewhere else, you can pass it on as it will not require creation of strings.
    /// Creating IJSValue from string/int will be an expensive operation.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    IJSValue this[IJSValue key] { get;set; }

    /// <summary>
    /// Returns true if specified property exists on current value, this method is case sensitive
    /// </summary>
    /// <param name="name">Case sensitive property name</param>
    /// <returns></returns>
    bool HasProperty(string name);

    /// <summary>
    /// Returns true if property was deleted successfully
    /// </summary>
    /// <param name="name">Case sensitive property name</param>
    /// <returns></returns>
    bool DeleteProperty(string name);

    IJSValue InvokeMethod(string name);
    IJSValue InvokeMethod(string name, IJSValue arg1);
    IJSValue InvokeMethod(string name, IJSValue arg1, IJSValue arg2);
    IJSValue InvokeMethod(string name, IJSValue arg1, IJSValue arg2, IJSValue arg3);
    IJSValue InvokeMethod(string name, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4);
    IJSValue InvokeMethod(string name, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5);

    IJSValue InvokeMethod(string name, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6);

    IJSValue InvokeMethod(string name, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6, IJSValue arg7);

    IJSValue InvokeMethod(string name, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6, IJSValue arg7, IJSValue arg8);


    /// <summary>
    /// Invokes method on the object
    /// </summary>
    /// <param name="name">Case sensitive property name</param>
    /// <param name="args"></param>
    /// <returns></returns>
    IJSValue InvokeMethod(string name, params IJSValue[] args);


    IJSValue InvokeFunction(IJSValue thisValue);

    IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1);

    IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1, IJSValue arg2);

    IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1, IJSValue arg2, IJSValue arg3);

    IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4);

    IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5);

    IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6);

    IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6, IJSValue arg7);

    IJSValue InvokeFunction(IJSValue thisValue, IJSValue arg1, IJSValue arg2, IJSValue arg3, IJSValue arg4, IJSValue arg5, IJSValue arg6, IJSValue arg7, IJSValue arg8);

    IJSValue InvokeFunction(IJSValue thisValue, IList<IJSValue> args);

    /// <summary>
    /// Calls current value as a function
    /// </summary>
    /// <param name="thisValue"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    IJSValue InvokeFunction(IJSValue thisValue, params IJSValue[] args);


    /// <summary>
    /// Equivalent of ` instanceof x ` in JavaScript
    /// </summary>
    /// <param name="jsClass"></param>
    /// <returns></returns>
    bool InstanceOf(IJSValue jsClass);

    /// <summary>
    /// Unwraps wrapped value and returns underlying object, this object is weakly referenced
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T Unwrap<T>();

    ///// <summary>
    ///// Try unwrapping the value, this is important in some case if the object is either null or disposed
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="value"></param>
    ///// <returns></returns>
    //bool TryUnwrap<T>(out T value);

    /// <summary>
    /// If this is an array, it returns `IList<IJSValue>`.
    /// IList<IJSValue> supports Collection Notifications
    /// </summary>
    /// <returns></returns>
    IList<IJSValue> ToArray();

    /// <summary>
    /// Returns number of items of this Array, only if this IJSValue represents an array
    /// </summary>
    int Length { get; set; }

    /// <summary>
    /// Get/Set value at specified index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    IJSValue this[int index] { get;set; }

    /// <summary>
    /// Defines Property on the object
    /// </summary>
    /// <param name="name"></param>
    /// <param name="descriptor"></param>
    void DefineProperty(string name, JSPropertyDescriptor descriptor);


    // [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    IEnumerable<JSProperty> Entries { get; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    string DebugView { get; }
}

public struct JSPropertyDescriptor
{
    public IJSValue Get;
    public IJSValue Set;
    public IJSValue Value;
    public bool? Configurable;
    public bool? Writable;
    public bool? Enumerable;
}
