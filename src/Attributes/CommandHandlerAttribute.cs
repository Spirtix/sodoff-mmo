namespace sodoffmmo.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
class CommandHandlerAttribute : Attribute {
    public int ID { get; }

    public CommandHandlerAttribute(int id) {
        ID = id;
    }
}