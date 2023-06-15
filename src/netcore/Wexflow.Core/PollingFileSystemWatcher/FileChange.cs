// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.IO;

namespace Wexflow.Core.PollingFileSystemWatcher
{
#pragma warning disable CS1591 // Commentaire XML manquant pour le type ou le membre visible publiquement
    public readonly struct FileChange
#pragma warning restore CS1591 // Commentaire XML manquant pour le type ou le membre visible publiquement
    {
        internal FileChange(string directory, string path, WatcherChangeTypes type)
        {
            Debug.Assert(path != null);
            Directory = directory;
            Name = path;
            ChangeType = type;
        }

#pragma warning disable CS1591 // Commentaire XML manquant pour le type ou le membre visible publiquement
        public string Directory { get; }
#pragma warning restore CS1591 // Commentaire XML manquant pour le type ou le membre visible publiquement
#pragma warning disable CS1591 // Commentaire XML manquant pour le type ou le membre visible publiquement
        public string Name { get; }
#pragma warning restore CS1591 // Commentaire XML manquant pour le type ou le membre visible publiquement
#pragma warning disable CS1591 // Commentaire XML manquant pour le type ou le membre visible publiquement
        public WatcherChangeTypes ChangeType { get; }
#pragma warning restore CS1591 // Commentaire XML manquant pour le type ou le membre visible publiquement
    }
}
