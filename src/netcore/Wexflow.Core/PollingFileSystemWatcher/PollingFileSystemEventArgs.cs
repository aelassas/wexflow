// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

#pragma warning disable CS1591 // Commentaire XML manquant pour le type ou le membre visible publiquement
namespace Wexflow.Core.PollingFileSystemWatcher
{
    public class PollingFileSystemEventArgs : EventArgs
    {
        public PollingFileSystemEventArgs(FileChange[] changes)
        {
            Changes = changes;
        }

        public FileChange[] Changes { get; }
    }
}
#pragma warning restore CS1591 // Commentaire XML manquant pour le type ou le membre visible publiquement