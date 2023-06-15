// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
#pragma warning disable 1591
namespace Wexflow.Core.PollingFileSystemWatcher
{
#pragma warning disable CS1591 // Commentaire XML manquant pour le type ou le membre visible publiquement
    public class PollingFileSystemEventArgs : EventArgs
#pragma warning restore CS1591 // Commentaire XML manquant pour le type ou le membre visible publiquement
    {
#pragma warning disable CS1591 // Commentaire XML manquant pour le type ou le membre visible publiquement
        public PollingFileSystemEventArgs(FileChange[] changes)
#pragma warning restore CS1591 // Commentaire XML manquant pour le type ou le membre visible publiquement
        {
            Changes = changes;
        }

#pragma warning disable CS1591 // Commentaire XML manquant pour le type ou le membre visible publiquement
        public FileChange[] Changes { get; }
#pragma warning restore CS1591 // Commentaire XML manquant pour le type ou le membre visible publiquement
    }
}
#pragma warning restore 1591
