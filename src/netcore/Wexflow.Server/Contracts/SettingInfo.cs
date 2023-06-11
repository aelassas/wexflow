namespace Wexflow.Server.Contracts
{
    public class SettingInfo
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public AttributeInfo[] Attributes { get; set; }

        public SettingInfo(string name, string value, AttributeInfo[] attributes)
        {
            Name = name;
            Value = value;
            Attributes = attributes;
        }
    }
}
