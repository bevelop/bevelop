using System;
using Bevelop.Messages;
using Microsoft.AspNet.SignalR.Client;

namespace Bevelop.VSClient.Services
{
    public class ChangeServer : IChangeServer
    {
        readonly Func<string> _getServerUrl;
        HubConnection _connection;
        IHubProxy _hub;

        public ChangeServer(Func<string> getServerUrl)
        {
            _getServerUrl = getServerUrl;
        }

        public event EventHandler<FileRemotelyChangedEventArgs> FileRemotelyChanged = delegate { };

        public void NotifyFileChange(FileChange fileChange)
        {
            GetHub().Invoke("NotifyChange", fileChange);
        }

        public void RequestChanges(string username, FileAddress fileAddress)
        {
            GetHub().Invoke("RequestChanges", username, fileAddress);
        }

        IHubProxy GetHub()
        {
            if (_hub == null)
            {
                _connection = new HubConnection(_getServerUrl());
                _hub = _connection.CreateHubProxy("ChangeHub");
                _connection.Start().Wait();

                _hub.On<FileChange>("notify", OnAnotherUserChangedFile);
            }

            return _hub;
        }

        void OnAnotherUserChangedFile(FileChange change)
        {
            FileRemotelyChanged(this, new FileRemotelyChangedEventArgs(change));
        }
    }
}