namespace sodoffmmo.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
class ExtensionCommandHandlerAttribute : Attribute {
    public string Name { get; }

    public ExtensionCommandHandlerAttribute(string name) {
        Name = name;
    }
}