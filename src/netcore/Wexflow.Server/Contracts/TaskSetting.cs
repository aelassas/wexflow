namespace Wexflow.Server.Contracts
{
    public class TaskSetting
    {
        public string Name { get; set; }
        public bool Required { get; set; }
        public string Type { get; set; }
        public string[] List { get; set; }
        public string DefaultValue { get; set; }
    }
}
