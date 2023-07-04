using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Wexflow.Core
{
    /// <summary>
    /// FileInf.
    /// </summary>
    public class FileInf
    {
        private string _path;
        private string _renameTo;
        private FileInfo _fileInfo;

        /// <summary>
        /// File path.
        /// </summary>
        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                FileName = System.IO.Path.GetFileName(value);
            }
        }
        /// <summary>
        /// File name.
        /// </summary>
        public string FileName { get; private set; }
        /// <summary>
        /// Task Id.
        /// </summary>
        public int TaskId { get; }
        /// <summary>
        /// RenameTo.
        /// </summary>
        public string RenameTo
        {
            get => _renameTo;
            set
            {
                _renameTo = value;

                if (!string.IsNullOrEmpty(value))
                {
                    RenameToOrName = value;
                }
            }
        }
        /// <summary>
        /// RenameToOrName.
        /// </summary>
        public string RenameToOrName { get; private set; }
        /// <summary>
        /// List of tags.
        /// </summary>
        public List<Tag> Tags { get; }

        /// <summary>
        /// File system info from <see cref="Path"/>.
        /// </summary>
        public FileInfo FileInfo => _fileInfo ?? (_fileInfo = new FileInfo(Path));

        /// <summary>
        /// Creates a new instance of FileInf.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <param name="taskId">Task Id.</param>
        public FileInf(string path, int taskId)
        {
            Path = path;
            TaskId = taskId;
            RenameToOrName = System.IO.Path.GetFileName(path);
            Tags = new List<Tag>();
        }

        /// <summary>
        /// FileInfo to string.
        /// </summary>
        /// <returns>FileInf as an XElement.ToString().</returns>
        public override string ToString()
        {
            return ToXElement().ToString();
        }

        /// <summary>
        /// FileInf To XElement.
        /// </summary>
        /// <returns>FileInf as an XElement.</returns>
        public XElement ToXElement()
        {
            return new XElement("File",
                new XAttribute("taskId", TaskId),
                new XAttribute("path", Path),
                new XAttribute("name", FileName),
                new XAttribute("renameTo", RenameTo ?? string.Empty),
                new XAttribute("renameToOrName", RenameToOrName),
                from tag in Tags
                select new XAttribute(tag.Key, tag.Value));
        }
    }
}