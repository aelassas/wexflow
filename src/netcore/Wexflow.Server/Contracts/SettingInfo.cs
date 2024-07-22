namespace Wexflow.Server.Contracts
{
    public class SettingInfo(string name, string value, AttributeInfo[] attributes)
    {
        public string Name { get; set; } = name;

        public string Value { get; set; } = value;

        public AttributeInfo[] Attributes { get; set; } = attributes;
    }
}
