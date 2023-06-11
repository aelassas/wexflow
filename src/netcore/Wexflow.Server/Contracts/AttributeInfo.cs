namespace Wexflow.Server.Contracts
{
    public class AttributeInfo
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public AttributeInfo(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
