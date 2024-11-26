namespace Wexflow.Server.Contracts
{
    public class SaveResult
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string FileSize { get; set; }
        public bool Result { get; set; }
        public bool WrongWorkflowId { get; set; }
    }
}
