using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace NeuroSpeech.Positron;


public class AtomEnumerable :
    IJSArray,
    IList
{

    IJSValue IJSArray.ArrayObject => array;

    /// <summary>
    /// Used internally, do not use it.
    /// </summary>
    public readonly IJSValue array;
    private readonly IJSContext context;

    public object Key { get; set; }

    public int Count => this.array.Length;

    public bool IsReadOnly => false;

    public IJSValue this[int index]
    {
        get => this.array[index];
        //{
        //    var x = this.array[index];
        //    System.Diagnostics.Debug.WriteLine($"{index}: {x["label"]}");
        //    return x;
        //}
        set => this.array[index] = value;
    }

    private Dictionary<NotifyCollectionChangedEventHandler, IJSValue> registrations = null;

    public AtomEnumerable(IJSValue array)
    {
        this.array = array;
        this.context = array.Context;
    }

    public IEnumerator<IJSValue> GetEnumerator()
    {
        var a = array.Length;
        // System.Diagnostics.Debug.WriteLine($"Length is {a}");
        for (var i = 0; i < a; i++)
        {
            yield return array[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public int IndexOf(IJSValue item)
    {
        return this.array.InvokeMethod("indexOf", item).IntValue;
    }

    public void Insert(int index, IJSValue item)
    {
        var c = this.array.Context;
        this.array.InvokeMethod("insert", c.CreateNumber(index), item);
    }

    public void RemoveAt(int index)
    {
        var c = this.array.Context;
        this.array.InvokeMethod("removeAt", c.CreateNumber(index));
    }

    public void Add(IJSValue item)
    {
        this.array.InvokeMethod("add", item);
    }

    public void Clear()
    {
        this.array.InvokeMethod("clear");
    }

    public bool Contains(IJSValue item)
    {
        return this.IndexOf(item) != -1;
    }

    public void CopyTo(IJSValue[] array, int arrayIndex)
    {
        int l = this.array.Length;
        for (int i = 0; i < l; i++)
        {
            array[arrayIndex + i] = this.array[i];
        }
    }

    public bool Remove(IJSValue item)
    {
        return this.array.InvokeMethod("remove", item).BooleanValue;
    }


    bool IList.IsFixedSize => false;

    bool IList.IsReadOnly => false;

    int ICollection.Count => Count;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => null;

    object IList.this[int index]
    {
        get => this[index];
        set => this[index] = context.Marshal(value);
    }

    int IList.Add(object value)
    {
        return this.array.InvokeMethod("add", context.Marshal(value)).IntValue;
    }

    void IList.Clear() => Clear();

    bool IList.Contains(object value)
        => Contains(context.Marshal(value));

    int IList.IndexOf(object value)
        => IndexOf(context.Marshal(value));

    void IList.Insert(int index, object value)
        => Insert(index, context.Marshal(value));

    void IList.Remove(object value)
        => Remove(context.Marshal(value));

    void IList.RemoveAt(int index)
        => RemoveAt(index);

    void ICollection.CopyTo(Array array, int index)
    {
        int l = this.array.Length;
        for (int i = 0; i < l; i++)
        {
            array.SetValue(this.array[i], index);
        }
    }
}

