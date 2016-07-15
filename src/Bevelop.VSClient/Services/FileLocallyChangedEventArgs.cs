using System;
using Bevelop.Messages;

namespace Bevelop.VSClient.Services
{
    public class FileLocallyChangedEventArgs : EventArgs
    {
        public FileLocallyChangedEventArgs(FileChange fileChange)
        {
            FileChange = fileChange;
        }

        public FileChange FileChange { get; private set; }
    }
}