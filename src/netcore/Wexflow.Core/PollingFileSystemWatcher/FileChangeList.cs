// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;

namespace System.IO
{
    internal struct FileChangeList
    {
        const int DefaultListSize = 4;

        FileChange[] _changes;
        int _count;

        public readonly bool IsEmpty { get { return _changes == null || _count == 0; } }

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

        void EnsureCapacity()
        {
            _changes ??= new FileChange[DefaultListSize];
            if (_count >= _changes.Length)
            {
                FileChange[] larger = new FileChange[_changes.Length * 2];
                _changes.CopyTo(larger, 0);
                _changes = larger;
            }
        }

        readonly void Sort()
        {
            Array.Sort(_changes, 0, _count, Comparer.Default);
        }

        public override readonly string ToString()
        {
            return _count.ToString();
        }

        public readonly FileChange[] ToArray()
        {
            Sort();
            FileChange[] result = new FileChange[_count];
            Array.Copy(_changes, result, _count);
            return result;
        }

        class Comparer : IComparer<FileChange>
        {
            public static IComparer<FileChange> Default = new Comparer();

            public int Compare(FileChange left, FileChange right)
            {
                int nameOrder = String.CompareOrdinal(left.Name, right.Name);
                return nameOrder != 0 ? nameOrder : left.ChangeType.CompareTo(right.ChangeType);
            }
        }
    }
}
