using System;
using Bevelop.Messages;

namespace Bevelop.VSClient.Services
{
    public class FileLocallyChangedEventArgs : EventArgs
    {
        public FileLocallyChangedEventArgs(FileChange fileChange, bool wasPreviouslyUnchanged)
        {
            FileChange = fileChange;
            WasPreviouslyUnchanged = wasPreviouslyUnchanged;
        }

        public FileChange FileChange { get; private set; }
        public bool WasPreviouslyUnchanged { get; set; }
    }
}