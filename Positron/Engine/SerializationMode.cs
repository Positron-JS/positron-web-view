using System;
using System.Linq;

namespace Positron
{
    /// <summary>
    /// Serialization modes for CLR to JavaScript and back.
    /// </summary>
    public enum SerializationMode
    {
        /// <summary>
        /// Deep copy entire object as Dictionary, all values are converted
        /// to native JavaScript type, two copy serialized values will be two different objects.
        /// 
        /// This method will fail with self referencing objects.
        /// 
        /// This method is also very slow as deep copy operation will take more time.
        /// 
        /// You cannot call any methods on this object from JavaScript.
        /// </summary>
        Copy,

        /// <summary>
        /// Keeps reference along with serialization, every property is serialized as getter/setter,
        /// upon deserialization, same object will be returned.
        /// 
        /// This method is useful for self referencing objects. The only problem with this method is,
        /// reference is weakly referenced, so it may give error if reference is destroyed in CLR. 
        /// 
        /// This method is faster at time of deserialization as it simply returns referenced object.
        /// 
        /// You can call methods of this object from JavaScript (with Camel Case in JavaScript)
        /// </summary>
        Reference,

        /// <summary>
        /// Keeps weak reference along with serialization, every property is serialized as getter/setter,
        /// upon deserialization, same object will be returned.
        /// 
        /// This method is useful for self referencing objects. The only problem with this method is,
        /// reference is weakly referenced, so it may give error if reference is destroyed in CLR. 
        /// 
        /// This method is faster at time of deserialization as it simply returns referenced object.
        /// 
        /// You can call methods of this object from JavaScript (with Camel Case in JavaScript)
        /// </summary>
        WeakReference,

    }
}
