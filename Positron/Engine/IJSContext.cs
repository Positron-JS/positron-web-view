using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Positron
{
    public delegate void PromiseCallBack(IJSValue result, IJSValue error);

    public delegate IJSValue WJSBoundFunction(IJSContext context, IJSValue @this, IList<IJSValue> @params);

    public interface IJSArray: IList<IJSValue>
    {
        IJSValue ArrayObject { get; }
    }

    /// <summary>
    /// JavaScript Context Interface
    /// </summary>
    public interface IJSContext: IDisposable
    {

        ClrClassFactory ClassFactory { set;  get; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        void RunOnUIThread(Func<Task> task);

        ///// <summary>
        ///// Creates Promise from Given Task, the Promise will and should resolve only on the UI Thread
        ///// </summary>
        ///// <param name="task"></param>
        ///// <returns></returns>
        //IJSValue CreatePromise(Task task);

        /// <summary>
        /// Evaluates given script and returns the value
        /// </summary>
        /// <param name="script">Text</param>
        /// <param name="location">Location, used for Debugging</param>
        /// <returns></returns>
        IJSValue Evaluate(string script, string location = null);

        /// <summary>
        /// Asynchronously evaluate script on different thread
        /// </summary>
        /// <param name="script"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        Task EvaluateAsync(string script, string location = null);

        /// <summary>
        /// Creates JavaScript string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        IJSValue CreateString(string text);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        IJSValue CreateNumber(double number);

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="v"></param>
        ///// <returns></returns>
        //IJSValue CreateBoolean(bool v);

        /// <summary>
        /// Creates Symbol in JavaScript if underlying engine supports, otherwise it creats a
        /// random string with prefix `_$_`
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IJSValue CreateSymbol(string name);

        /// <summary>
        /// Returns an Undefined value for this context, this value is always newly created so you must store reference.
        /// </summary>
        IJSValue Undefined { get; }

        /// <summary>
        /// Returns JavaScript's native null
        /// </summary>
        IJSValue Null { get; }

        /// <summary>
        /// Returns JavaScript's native true
        /// </summary>
        IJSValue True { get; }

        /// <summary>
        /// Returns JavaScript's native false
        /// </summary>
        IJSValue False { get; }

        /// <summary>
        /// Creates an empty JavaScript object
        /// </summary>
        /// <returns></returns>
        IJSValue CreateObject();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IJSValue CreateDate(DateTime value);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemsToAdd"></param>
        /// <returns></returns>
        IJSArray CreateArray();

        ///// <summary>
        ///// Converts given value to appropriate JavaScript representation. Value types ans string are converted to Native JavaScript types.
        ///// Objects are wrapped.
        ///// </summary>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //IJSValue Convert(object value);

        /// <summary>
        /// Error Event to inspect JavaScript errors
        /// </summary>
        event EventHandler<ErrorEventArgs> ErrorEvent;

        /// <summary>
        /// Get/Set global values on JavaScript context
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IJSValue this[string name] { get; set; }

        /// <summary>
        /// Get/Set global values on JavaScript context
        /// </summary>
        /// <param name="keyOrSymbol"></param>
        /// <returns></returns>
        IJSValue this[IJSValue keyOrSymbol] { get; set; }

        /// <summary>
        /// Returns true if property exists on the Engine (ie global object)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool HasProperty(string name);

        /// <summary>
        /// Deletes the global variable
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool DeleteProperty(string name);

        /// <summary>
        /// Creates a JavaScript function by wrapping given Lambda function, this function can be set
        /// as a property to any JavaScript object and you can call it inside JavaScript.
        /// 
        /// Specifiying correct number of expected arguments will speed up call, unerlying implementation
        /// will choose correct method based on platfrom limitations.
        /// 
        /// Example,
        /// <code>
        ///    // c#
        ///    context["add"] = context.CreateFunction(2, (a) =&gt; a[0].DoubleValue + a[1].DoubleValue);
        ///    
        ///    // JavaScript
        ///    const c = global.add(2,3);
        ///    
        /// </code>
        /// </summary>
        /// <param name="numberOfParameters"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        IJSValue CreateFunction(int numberOfParameters, Func<IJSContext, IList<IJSValue>, IJSValue> func, string debugDescription);

        /// <summary>
        /// Creates a new constructor function, in iOS this implementation differs little as native functions cannot be used as constructors,
        /// on other platforms it remains same.
        /// </summary>
        /// <param name="numberOfParameters"></param>
        /// <param name=""></param>
        /// <param name="func"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        IJSValue CreateConstructor(int numberOfParameters, Func<IJSContext, IList<IJSValue>, IJSValue> func, string name);

        /// <summary>
        /// Creates a JavaScript bound function by wrapping given Lambda function, this function can be set
        /// as a property to any JavaScript object and you can call it inside JavaScript.
        /// 
        /// Specifiying correct number of expected arguments will speed up call, unerlying implementation
        /// will choose correct method based on platfrom limitations.
        /// 
        /// Example,
        /// <code>
        ///    // c#
        ///    context["add"] = context.CreateBoundFunction(2, (a) =&gt; a[0].DoubleValue + a[1].DoubleValue);
        ///    
        ///    // JavaScript
        ///    const c = global.add(2,3);
        ///    
        /// </code>
        /// </summary>
        /// <param name="numberOfParameters"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        IJSValue CreateBoundFunction(int numberOfParameters, WJSBoundFunction func, string debugDescription);


        /// <summary>
        /// Returns current Stack if there is any...
        /// </summary>
        string Stack { get; }

        /// <summary>
        /// Simply wraps an object...
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IJSValue Wrap(object value);

    }
}
