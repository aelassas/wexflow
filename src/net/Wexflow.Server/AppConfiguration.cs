namespace Wexflow.Server
{
    public class AppConfiguration : IAppConfiguration
    {
        public Logging Logging { get; set; }
        public Smtp Smtp { get; set; }
    }
}
