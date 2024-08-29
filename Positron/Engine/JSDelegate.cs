using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NeuroSpeech.Positron.Delegates;


public class JSDelegate<TReturn>
{
    private readonly IJSContext context;
    private readonly IJSValue value;

    public JSDelegate(IJSContext context, IJSValue value)
    {
        this.context = context;
        this.value = value;
    }

    public TReturn Func()
    {
        return context.Deserialize<TReturn>(
            value.InvokeFunction(null));
    }

    public void Action()
    {
        value.InvokeFunction(null);
    }
}
public class JSDelegate<T1, TReturn>
{
    private readonly IJSContext context;
    private readonly IJSValue value;

    public JSDelegate(IJSContext context, IJSValue value)
    {
        this.context = context;
        this.value = value;
    }

    public TReturn Func(T1 input)
    {
        return context.Deserialize<TReturn>(value.InvokeFunction(null, 
            context.Marshal(input) 
        ));
    }

    public void Action(T1 input)
    {
        value.InvokeFunction(null,
            context.Marshal(input) 
        );
    }
}

public class JSDelegate<T1, T2, TReturn>
{
    private readonly IJSContext context;
    private readonly IJSValue value;

    public JSDelegate(IJSContext context, IJSValue value)
    {
        this.context = context;
        this.value = value;
    }

    public TReturn Func(T1 input1, T2 input2)
    {
        return context.Deserialize<TReturn>(value.InvokeFunction(null,
            context.Marshal(input1),
            context.Marshal(input2)
        ));
    }

    public void Action(T1 input1, T2 input2)
    {
        value.InvokeFunction(null,
            context.Marshal(input1),
            context.Marshal(input2)
        );
    }
}

public class JSDelegate<T1, T2, T3, TReturn>
{
    private readonly IJSContext context;
    private readonly IJSValue value;

    public JSDelegate(IJSContext context, IJSValue value)
    {
        this.context = context;
        this.value = value;
    }

    public TReturn Func(T1 input1, T2 input2, T3 input3)
    {
        return context.Deserialize<TReturn>(value.InvokeFunction(null, 
            context.Marshal(input1),
            context.Marshal(input2),
            context.Marshal(input3)
        ));
    }

    public void Action(T1 input1, T2 input2, T3 input3)
    {
        value.InvokeFunction(null,
            context.Marshal(input1),
            context.Marshal(input2),
            context.Marshal(input3)
        );
    }
}

public class JSDelegate<T1, T2, T3, T4, TReturn>
{
    private readonly IJSContext context;
    private readonly IJSValue value;

    public JSDelegate(IJSContext context, IJSValue value)
    {
        this.context = context;
        this.value = value;
    }

    public TReturn Func(T1 input1, T2 input2, T3 input3, T4 input4)
    {
        return context.Deserialize<TReturn>(value.InvokeFunction(null,
            context.Marshal(input1),
            context.Marshal(input2),
            context.Marshal(input3),
            context.Marshal(input4)
        ));
    }

    public void Action(T1 input1, T2 input2, T3 input3, T4 input4)
    {
        value.InvokeFunction(null, 
            context.Marshal(input1),
            context.Marshal(input2),
            context.Marshal(input3),
            context.Marshal(input4)
        );
    }
}

public class JSDelegate<T1, T2, T3, T4, T5, TReturn>
{
    private readonly IJSContext context;
    private readonly IJSValue value;

    public JSDelegate(IJSContext context, IJSValue value)
    {
        this.context = context;
        this.value = value;
    }

    public TReturn Func(T1 input1, T2 input2, T3 input3, T4 input4, T5 input5)
    {
        return context.Deserialize<TReturn>(value.InvokeFunction(null,
            context.Marshal(input1),
            context.Marshal(input2),
            context.Marshal(input3),
            context.Marshal(input4),
            context.Marshal(input5)
        ));
    }

    public void Action(T1 input1, T2 input2, T3 input3, T4 input4, T5 input5)
    {
        value.InvokeFunction(null, 
            context.Marshal(input1),
            context.Marshal(input2),
            context.Marshal(input3),
            context.Marshal(input4),
            context.Marshal(input5)
        );
    }
}

public class JSDelegate<T1, T2, T3, T4, T5, T6, TReturn>
{
    private readonly IJSContext context;
    private readonly IJSValue value;

    public JSDelegate(IJSContext context, IJSValue value)
    {
        this.context = context;
        this.value = value;
    }

    public TReturn Func(T1 input1, T2 input2, T3 input3, T4 input4, T5 input5, T6 input6)
    {
        return context.Deserialize<TReturn>(value.InvokeFunction(null, 
            context.Marshal(input1),
            context.Marshal(input2),
            context.Marshal(input3),
            context.Marshal(input4),
            context.Marshal(input5),
            context.Marshal(input6)
        ));
    }

    public void Action(T1 input1, T2 input2, T3 input3, T4 input4, T5 input5, T6 input6)
    {
        value.InvokeFunction(null,
            context.Marshal(input1),
            context.Marshal(input2),
            context.Marshal(input3),
            context.Marshal(input4),
            context.Marshal(input5),
            context.Marshal(input6)
        );
    }
}

public class JSDelegate<T1, T2, T3, T4, T5, T6, T7, TReturn>
{
    private readonly IJSContext context;
    private readonly IJSValue value;

    public JSDelegate(IJSContext context, IJSValue value)
    {
        this.context = context;
        this.value = value;
    }

    public TReturn Func(T1 input1, T2 input2, T3 input3, T4 input4, T5 input5, T6 input6, T7 input7)
    {
        return context.Deserialize<TReturn>(value.InvokeFunction(null, 
            context.Marshal(input1),
            context.Marshal(input2),
            context.Marshal(input3),
            context.Marshal(input4),
            context.Marshal(input5),
            context.Marshal(input6),
            context.Marshal(input7)
        ));
    }

    public void Action(T1 input1, T2 input2, T3 input3, T4 input4, T5 input5, T6 input6, T7 input7)
    {
        value.InvokeFunction(null, 
            context.Marshal(input1),
            context.Marshal(input2),
            context.Marshal(input3),
            context.Marshal(input4),
            context.Marshal(input5),
            context.Marshal(input6),
            context.Marshal(input7)
        );
    }
}

public class JSDelegate<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>
{
    private readonly IJSContext context;
    private readonly IJSValue value;

    public JSDelegate(IJSContext context, IJSValue value)
    {
        this.context = context;
        this.value = value;
    }

    public TReturn Func(T1 input1, T2 input2, T3 input3, T4 input4, T5 input5, T6 input6, T7 input7, T8 input8)
    {
        return context.Deserialize<TReturn>(value.InvokeFunction(null, 
            context.Marshal(input1),
            context.Marshal(input2),
            context.Marshal(input3),
            context.Marshal(input4),
            context.Marshal(input5),
            context.Marshal(input6),
            context.Marshal(input7),
            context.Marshal(input8)
        ));
    }

    public void Action(T1 input1, T2 input2, T3 input3, T4 input4, T5 input5, T6 input6, T7 input7, T8 input8)
    {
        value.InvokeFunction(null, 
            context.Marshal(input1),
            context.Marshal(input2),
            context.Marshal(input3),
            context.Marshal(input4),
            context.Marshal(input5),
            context.Marshal(input6),
            context.Marshal(input7),
            context.Marshal(input8)
        );
    }
}

public class JSDelegate
{


    public static Delegate Create(Type delegateType, IJSContext context, IJSValue value)
    {
        var method = delegateType.GetMethod("Invoke");
        var args = method.GetParameters();
        int len = args.Length;
        bool isVoid = method.ReturnType == typeof(void);
        var returnType = isVoid ? typeof(int) : method.ReturnType;
        Type jsType;
        Type[] argTypes;
        switch (len)
        {
            case 0:
                jsType = typeof(JSDelegate<>);
                argTypes =  new Type[] { returnType };
                break;
            case 1:
                jsType = typeof(JSDelegate<,>);
                argTypes = new Type[] {
                    args[0].ParameterType,
                    returnType
                };
                break;
            case 2:
                jsType = typeof(JSDelegate<,,>);
                argTypes = new Type[] {
                    args[0].ParameterType,
                    args[1].ParameterType,
                    returnType
                };
                break;
            case 3:
                jsType = typeof(JSDelegate<,,,>);
                argTypes = new Type[] {
                    args[0].ParameterType,
                    args[1].ParameterType,
                    args[2].ParameterType,
                    returnType
                };
                break;
            case 4:
                jsType = typeof(JSDelegate<,,,,>);
                argTypes = new Type[] {
                    args[0].ParameterType,
                    args[1].ParameterType,
                    args[2].ParameterType,
                    args[3].ParameterType,
                    returnType
                };
                break;
            case 5:
                jsType = typeof(JSDelegate<,,,,,>);
                argTypes = new Type[] {
                    args[0].ParameterType,
                    args[1].ParameterType,
                    args[2].ParameterType,
                    args[3].ParameterType,
                    args[4].ParameterType,
                    returnType
                };
                break;
            case 6:
                jsType = typeof(JSDelegate<,,,,,,>);
                argTypes = new Type[] {
                    args[0].ParameterType,
                    args[1].ParameterType,
                    args[2].ParameterType,
                    args[3].ParameterType,
                    args[4].ParameterType,
                    args[5].ParameterType,
                    returnType
                };
                break;
            case 7:
                jsType = typeof(JSDelegate<,,,,,,,>);
                argTypes = new Type[] {
                    args[0].ParameterType,
                    args[1].ParameterType,
                    args[2].ParameterType,
                    args[3].ParameterType,
                    args[4].ParameterType,
                    args[5].ParameterType,
                    args[6].ParameterType,
                    returnType
                };
                break;
            case 8:
                jsType = typeof(JSDelegate<,,,,,,,,>);
                argTypes = new Type[] {
                    args[0].ParameterType,
                    args[1].ParameterType,
                    args[2].ParameterType,
                    args[3].ParameterType,
                    args[4].ParameterType,
                    args[5].ParameterType,
                    args[6].ParameterType,
                    args[7].ParameterType,
                    returnType
                };
                break;
            default:
                throw new ArgumentOutOfRangeException($"Too many method parameters, max is 8");
        }
        jsType = jsType.MakeGenericType(argTypes);
        var target = Activator.CreateInstance(jsType, context, value);

        var m = jsType.GetMethod(isVoid ? "Action" : "Func");

        return Delegate.CreateDelegate(delegateType, target, m);
    }


}
