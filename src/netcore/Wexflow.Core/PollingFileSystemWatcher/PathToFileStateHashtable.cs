// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Wexflow.Core.PollingFileSystemWatcher
{
    // this is a quick an dirty hashtable optimized for the PollingFileSystemWatcher
    // It allows mutating struct values (FileState) contained in the hashtable
    // It has optimized Equals and GetHasCode
    // It implements removals by marking values as "removed" (Path==null) and then garbage collecting them when table is resized
    [Serializable]
    internal sealed class PathToFileStateHashtable(int capacity = 4)
    {
        private int _nextValuesIndex = 1; // the first Values slot is reserved so that default(Bucket) knows that it is not pointing to any value.
        public FileState[] Values { get; private set; } = new FileState[capacity];
        private Bucket[] _buckets = new Bucket[GetPrime(capacity + 1)];

        public int Count { get; private set; }

        public void Add(string directory, string file, FileState value)
        {
            if (_nextValuesIndex >= Values.Length) // Resize
            {
                Resize();
            }

            Values[_nextValuesIndex] = value;
            var bucket = ComputeBucket(file);

            while (true)
            {
                if (_buckets[bucket].IsEmpty)
                {
                    _buckets[bucket] = new Bucket(directory, file, _nextValuesIndex);
                    Count++;
                    _nextValuesIndex++;
                    return;
                }
                bucket = NextCandidateBucket(bucket);
            }
        }

        public void Remove(string directory, string file)
        {
            var index = IndexOf(directory, file);
            Debug.Assert(index != -1, "this should never happen");

            Values[index].Path = null;
            Values[index].Directory = null;
            Count--;
        }

        public int IndexOf(string directory, ReadOnlySpan<char> file)
        {
            var bucket = ComputeBucket(file);
            while (true)
            {
                var valueIndex = _buckets[bucket].ValuesIndex;
                if (valueIndex == 0)
                {
                    return -1; // not found
                }

                if (Equal(_buckets[bucket].Key, directory, file))
                {
                    if (Values[valueIndex].Path != null)
                    {
                        return valueIndex;
                    }
                }
                bucket = NextCandidateBucket(bucket);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int NextCandidateBucket(int bucket)
        {
            bucket++;
            if (bucket >= _buckets.Length)
            {
                bucket = 0;
            }
            return bucket;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once RedundantUnsafeContext
        private static unsafe bool Equal(FullPath fullPath, string directory, ReadOnlySpan<char> file)
        {
            return string.Equals(fullPath.Directory, directory, StringComparison.Ordinal)
&& file.Equals((ReadOnlySpan<char>)fullPath.File, StringComparison.Ordinal);
        }

        // ReSharper disable once RedundantUnsafeContext
        private static unsafe int GetHashCode(ReadOnlySpan<char> path)
        {
            var code = 0;
            for (var index = 0; index < path.Length; index++)
            {
                var next = path[index];
                code |= next;
                code <<= 8;
                if (index > 8)
                {
                    break;
                }
            }
            return code;
        }

        private int ComputeBucket(ReadOnlySpan<char> file)
        {
            var hash = GetHashCode(file);
            if (hash == int.MinValue)
            {
                hash = int.MaxValue;
            }

            var bucket = Math.Abs(hash) % _buckets.Length;
            return bucket;
        }

        private void Resize()
        {
            // this is because sometimes we just need to garbade collect instead of increase size
            var newSize = Math.Max(Count * 2, 4);

            PathToFileStateHashtable bigger = new(newSize);

            foreach (var existingValue in this)
            {
                bigger.Add(existingValue.Directory, existingValue.Path, existingValue);
            }
            Values = bigger.Values;
            _buckets = bigger._buckets;
            _nextValuesIndex = bigger._nextValuesIndex;
            Count = bigger.Count;
        }

        private static readonly int[] Primes = [
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369];

        private static bool IsPrime(int candidate)
        {
            if ((candidate & 1) != 0)
            {
                var limit = (int)Math.Sqrt(candidate);
                for (var divisor = 3; divisor <= limit; divisor += 2)
                {
                    if (candidate % divisor == 0)
                    {
                        return false;
                    }
                }
                return true;
            }
            return candidate == 2;
        }

        private static int GetPrime(int min)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Primes.Length; i++)
            {
                var prime = Primes[i];
                if (prime >= min)
                {
                    return prime;
                }
            }

            //outside of our predefined table. 
            //compute the hard way. 
            for (var i = min | 1; i < int.MaxValue; i += 2)
            {
                if (IsPrime(i))
                {
                    return i;
                }
            }
            return min;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator(PathToFileStateHashtable table)
        {
            private readonly PathToFileStateHashtable _table = table;
            private int _index = 0;

            public bool MoveNext()
            {
                do
                {
                    _index++;
                    if (_index > _table._nextValuesIndex || _index >= _table.Values.Length) { return false; }
                }
                while (_table.Values[_index].Path == null);

                return true;
            }

            public readonly FileState Current => _table.Values[_index];
        }

        public override string ToString()
        {
            return Count.ToString();
        }

        [Serializable]
        private struct Bucket
        {
            public FullPath Key;
            public int ValuesIndex;

            public Bucket(string directory, string file, int valueIndex)
            {
                Key.Directory = directory;
                Key.File = file;
                ValuesIndex = valueIndex;
            }
            public readonly bool IsEmpty => ValuesIndex == 0;

            public override readonly string ToString()
            {
                return IsEmpty ? "empty" : Key.ToString();
            }
        }

        [Serializable]
        private struct FullPath
        {
            public string Directory;
            public string File;

            public override readonly string ToString()
            {
                return File;
            }
        }
    }
}
