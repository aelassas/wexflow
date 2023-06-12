using System.Collections.Generic;
using Wexflow.Core;

namespace Wexflow.Tasks.FilesJoiner
{
    internal class GroupedFile
    {
        public string FileName { get; set; }
        public List<FileInf> Files { get; set; }
    }
}