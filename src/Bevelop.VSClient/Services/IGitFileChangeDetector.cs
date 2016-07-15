using System;

namespace Bevelop.VSClient.Services
{
    public interface IGitFileChangeDetector : IDisposable
    {
        event EventHandler<FileLocallyChangedEventArgs> FileLocallyChanged;
        event EventHandler<EventArgs> FileReset;

        string FullPath { get; }
        string RelativePath { get; }
    }
}