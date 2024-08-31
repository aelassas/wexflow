// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Wexflow.Core.PollingFileSystemWatcher
{
    internal struct FileChangeList
    {
        private const int DEFAULT_LIST_SIZE = 4;
        private FileChange[] _changes;
        private int _count;

        public readonly bool IsEmpty => _changes == null || _count == 0;

        internal void AddAdded(string directory, string path)
        {
            Debug.Assert(path != null);

            EnsureCapacity();
            _changes[_count++] = new FileChange(directory, path, WatcherChangeTypes.Created);
        }

        internal void AddChanged(string directory, string path)
        {
            Debug.Assert(path != null);

            EnsureCapacity();
            _changes[_count++] = new FileChange(directory, path, WatcherChangeTypes.Changed);
        }

        internal void AddRemoved(string directory, string path)
        {
            Debug.Assert(path != null);

            EnsureCapacity();
            _changes[_count++] = new FileChange(directory, path, WatcherChangeTypes.Deleted);
        }

        private void EnsureCapacity()
        {
            _changes ??= new FileChange[DEFAULT_LIST_SIZE];
            if (_count >= _changes.Length)
            {
                var larger = new FileChange[_changes.Length * 2];
                _changes.CopyTo(larger, 0);
                _changes = larger;
            }
        }

        private readonly void Sort()
        {
            Array.Sort(_changes, 0, _count, Comparer.ColumnDefault);
        }

        public override readonly string ToString()
        {
            return _count.ToString();
        }

        public readonly FileChange[] ToArray()
        {
            Sort();
            var result = new FileChange[_count];
            Array.Copy(_changes, result, _count);
            return result;
        }

        private sealed class Comparer : IComparer<FileChange>
        {
            public static readonly IComparer<FileChange> ColumnDefault = new Comparer();

            public int Compare(FileChange left, FileChange right)
            {
                var nameOrder = string.CompareOrdinal(left.Name, right.Name);
                return nameOrder != 0 ? nameOrder : left.ChangeType.CompareTo(right.ChangeType);
            }
        }
    }
}
