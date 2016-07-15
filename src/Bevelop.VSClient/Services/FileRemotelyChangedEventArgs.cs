using System;
using Bevelop.Messages;

namespace Bevelop.VSClient.Services
{
    public class FileRemotelyChangedEventArgs : EventArgs
    {
        public FileRemotelyChangedEventArgs(FileChange fileChange)
        {
            FileChange = fileChange;
        }

        public FileChange FileChange { get; private set; }
    }
}