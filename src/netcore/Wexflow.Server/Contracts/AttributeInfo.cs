namespace Wexflow.Server.Contracts
{
    public class AttributeInfo(string name, string value)
    {
        public string Name { get; set; } = name;

        public string Value { get; set; } = value;
    }
}
