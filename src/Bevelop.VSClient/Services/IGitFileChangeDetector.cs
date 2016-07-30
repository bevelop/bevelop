using System;
using Bevelop.Messages;

namespace Bevelop.VSClient.Services
{
    public interface IGitFileChangeDetector : IDisposable
    {
        event EventHandler<FileLocallyChangedEventArgs> FileLocallyChanged;
        event EventHandler<EventArgs> FileReset;

        FileAddress FileAddress { get; }
        string FullPath { get; }
        string Username { get; }
        bool HasChanges { get; }
    }
}