namespace Wexflow.Server.Contracts
{
    public class TaskInfo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsEnabled { get; set; }

        public SettingInfo[] Settings { get; set; }

        public TaskInfo(int id, string name, string desc, bool isEnabled, SettingInfo[] settings)
        {
            Id = id;
            Name = name;
            Description = desc;
            IsEnabled = isEnabled;
            Settings = settings;
        }
    }
}
