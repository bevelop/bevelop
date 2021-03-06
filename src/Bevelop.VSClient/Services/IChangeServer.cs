﻿using System;
using Bevelop.Messages;

namespace Bevelop.VSClient.Services
{
    public interface IChangeServer
    {
        event EventHandler<FileRemotelyChangedEventArgs> FileRemotelyChanged;
        void NotifyFileChange(FileChange fileChange);
        void RequestChanges(string username, FileAddress fileAddress);
    }
}