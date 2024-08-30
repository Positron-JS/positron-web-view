namespace NeuroSpeech.Positron.Core;

public class AssemblyInfo: IJSProxy
{
    public AssemblyInfo(string name,
        AssemblyInfo? parent = null,
        Type? type = null
        ) {
        this.Parent = parent;
        this.Name = name;
        this.Fullname = parent == null
            ? this.Name
            : $"{parent.Fullname}.{this.Name}";
        this.Type = type;

        //if (this.Parent != null)
        //{
        //    var c = this.Parent.Children ??= new Dictionary<string, AssemblyInfo>();
        //    // c[this.Name] = this;
        //}
    }

    public AssemblyInfo? Parent { get; }
    public string Name { get; }
    public string Fullname { get; }
    public Type? Type { get; }
    
    public Dictionary<string, AssemblyInfo>? Children { get; private set; }

    public Type Resolve(string name)
    {
        if (Children == null)
        {
            throw new InvalidOperationException($"Type {name} not found in {this.Fullname}");
        }
        if(Children.TryGetValue(name, out var value))
        {
            if (value.Type != null)
            {
                return value.Type;
            }
        }
        throw new InvalidOperationException($"Type {name} not found in {this.Fullname}");
    }

    internal AssemblyInfo GetOrCreate(string token)
    {
        this.Children ??= new();
        return this.Children.GetOrCreate(token, (x) => new AssemblyInfo(x, this));
    }

    internal void AddType(Type type)
    {
        this.Children ??= new();
        this.Children.Add(type.Name, new AssemblyInfo(type.Name, this, type));
    }

    IJSValue IJSProxy.Get(IJSContext context, IJSValue @this, IList<IJSValue> args)
    {

        var name = args[1].ToString()!;

        if (this.Children?.TryGetValue(name, out var av) ?? false)
        {
            return context.Marshal(av);
        }


        throw new ArgumentException($"{name} not found in ${this.Fullname}");
    }

    IJSValue IJSProxy.Set(IJSContext context, IJSValue @this, IList<IJSValue> args)
    {
        return context.Undefined;
    }
}
