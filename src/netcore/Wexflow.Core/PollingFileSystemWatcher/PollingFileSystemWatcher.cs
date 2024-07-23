// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.IO.Enumeration;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;

#pragma warning disable CS1591 // Commentaire XML manquant pour le type ou le membre visible publiquement
namespace Wexflow.Core.PollingFileSystemWatcher
{
    /// <summary>
    /// PollingFileSystemWatcher can be used to monitor changes to a file system directory
    /// </summary>
    /// <remarks>
    /// This type is similar to FileSystemWatcher, but unlike FileSystemWatcher it is fully reliable,
    /// at the cost of some performance overhead. 
    /// Instead of relying on Win32 file notification APIs, it periodically scans the watched directory to discover changes.
    /// This means that sooner or later it will discover every change.
    /// FileSystemWatcher's Win32 APIs can drop some events in rare circumstances, which is often an acceptable compromise.
    /// In scenarios where events cannot be missed, PollingFileSystemWatcher should be used.
    /// Note: When a watched file is renamed, one or two notifications will be made.
    /// Note: When no changes are detected, PollingFileSystemWatcher will not allocate memory on the GC heap.
    /// </remarks>
    [Serializable]
    public class PollingFileSystemWatcher : IDisposable, ISerializable
    {
        private Timer _timer;
        private readonly PathToFileStateHashtable _state; // stores state of the directory
        private long _version; // this is used to keep track of removals.
        private bool _started;
        private bool _disposed;
        private long _pollingInterval = 1000;

        /// <summary>
        /// Creates an instance of a watcher
        /// </summary>
        /// <param name="path">The path to watch.</param>
        /// <param name="filter">The type of files to watch. For example, "*.txt" watches for changes to all text files.</param>
        /// <param name="options">Options.</param>
        public PollingFileSystemWatcher(string path, string filter = "*", EnumerationOptions options = null)
        {
            ArgumentNullException.ThrowIfNull(path);

            if (!Directory.Exists(path))
            {
                throw new ArgumentException("Path not found.", nameof(path));
            }

            _state = new PathToFileStateHashtable();
            Path = path;
            Filter = filter ?? throw new ArgumentNullException(nameof(filter));
            EnumerationOptions = options ?? new EnumerationOptions();
            _timer = new Timer(TimerHandler);
        }

        private void Initialize()
        {
            _timer = new Timer(TimerHandler);
            if (!_started)
            {
                return;
            }

            _ = _timer.Change(PollingInterval, Timeout.Infinite);
        }

        public EnumerationOptions EnumerationOptions { get; set; }
        public string Filter { get; set; }
        public string Path { get; set; }

        /// <summary>
        /// The number of milliseconds to wait until checking the file system again
        /// </summary>
        public long PollingInterval
        {
            get => _pollingInterval;
            set
            {
                _pollingInterval = value;
                if (_started)
                {
                    _ = _timer.Change(PollingInterval, Timeout.Infinite);
                }
            }
        }

        public void Start()
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(PollingFileSystemWatcher));

            if (_started)
            {
                return;
            }

            _started = true;
            _ = ComputeChangesAndUpdateState(); // captures the initial state
            _ = _timer.Change(PollingInterval, Timeout.Infinite);
        }

        // This function walks all watched files, collects changes, and updates state
        private FileChangeList ComputeChangesAndUpdateState()
        {
            _version++;

            FileSystemChangeEnumerator enumerator = new(this, Path, EnumerationOptions);
            while (enumerator.MoveNext())
            {
                // Ignore `.Current`
            }
            var changes = enumerator.Changes;

            foreach (var value in _state)
            {
                if (value.Version != _version)
                {
                    changes.AddRemoved(value.Directory, value.Path);
                    _state.Remove(value.Directory, value.Path);
                }
            }

            return changes;
        }

        protected internal virtual bool ShouldIncludeEntry(ref FileSystemEntry entry)
        {
            if (entry.IsDirectory)
            {
                return false;
            }

            if (Filter == null)
            {
                return true;
            }

            var ignoreCase = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            return FileSystemName.MatchesSimpleExpression(Filter, entry.FileName, ignoreCase: ignoreCase);
        }

        protected internal virtual bool ShouldRecurseIntoEntry(ref FileSystemEntry entry)
        {
            return true;
        }

        internal void UpdateState(string directory, ref FileChangeList changes, ref FileSystemEntry file)
        {
            var index = _state.IndexOf(directory, file.FileName);
            if (index == -1) // file added
            {
                var path = file.FileName.ToString();

                changes.AddAdded(directory, path);

                FileState newFileState = new(directory, path)
                {
                    LastWriteTimeUtc = file.LastWriteTimeUtc,
                    Length = file.Length,
                    Version = _version
                };
                _state.Add(directory, path, newFileState);
                return;
            }

            _state.Values[index].Version = _version;

            var previousState = _state.Values[index];
            if (file.LastWriteTimeUtc != previousState.LastWriteTimeUtc || file.Length != previousState.Length)
            {
                changes.AddChanged(directory, previousState.Path);
                _state.Values[index].LastWriteTimeUtc = file.LastWriteTimeUtc;
                _state.Values[index].Length = file.Length;
            }
        }

        /// <summary>
        /// This callback is called when any change (Created, Deleted, Changed) is detected in any watched file.
        /// </summary>
        public event EventHandler Changed;

        public event PollingFileSystemEventHandler ChangedDetailed;

        public event ErrorEventHandler Error;

        /// <summary>
        /// Disposes the timer used for polling.
        /// </summary>
        public void Dispose()
        {
            _disposed = true;
            _timer.Dispose();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool Dispose(WaitHandle notifyObject)
        {
            _disposed = true;
            var isSuccess = _timer.Dispose(notifyObject);
            Dispose(true);
#pragma warning disable CA1816 // Les méthodes Dispose doivent appeler SuppressFinalize
            GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Les méthodes Dispose doivent appeler SuppressFinalize

            return isSuccess;
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        private void TimerHandler(object context)
        {
            try
            {
                var changes = ComputeChangesAndUpdateState();

                if (!changes.IsEmpty)
                {
                    Changed?.Invoke(this, EventArgs.Empty);
                    ChangedDetailed?.Invoke(this, new PollingFileSystemEventArgs(changes.ToArray()));
                }
            }
            catch (Exception e)
            {
                Error?.Invoke(this, new ErrorEventArgs(e));
            }

            if (!_disposed)
            {
                _ = _timer.Change(PollingInterval, Timeout.Infinite);
            }
        }

        #region Serializable

        protected PollingFileSystemWatcher(SerializationInfo info, StreamingContext context)
        {
            _state = (PathToFileStateHashtable)info.GetValue(nameof(_state), typeof(PathToFileStateHashtable));
            _version = info.GetInt64(nameof(_version));
            _started = info.GetBoolean(nameof(_started));
            _disposed = info.GetBoolean(nameof(_disposed));

            Path = info.GetString(nameof(Path));
            Filter = info.GetString(nameof(Filter));
            EnumerationOptions = new EnumerationOptions { RecurseSubdirectories = info.GetBoolean(nameof(EnumerationOptions.RecurseSubdirectories)) };

            _timer = new Timer(TimerHandler);
            PollingInterval = info.GetInt32(nameof(PollingInterval));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(_state), _state);
            info.AddValue(nameof(_version), _version);
            info.AddValue(nameof(_started), _started);
            info.AddValue(nameof(_disposed), _disposed);

            info.AddValue(nameof(Path), Path);
            info.AddValue(nameof(Filter), Filter);
            info.AddValue(nameof(EnumerationOptions.RecurseSubdirectories), EnumerationOptions.RecurseSubdirectories);
            info.AddValue(nameof(PollingInterval), PollingInterval);
        }

        #endregion
    }
}
#pragma warning restore CS1591 // Commentaire XML manquant pour le type ou le membre visible publiquement
