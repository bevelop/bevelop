using System;
using Bevelop.Messages;
using Microsoft.AspNet.SignalR.Client;

namespace Bevelop.VSClient.Services
{
    public class ChangeServer : IChangeServer
    {
        HubConnection _connection;
        IHubProxy _hub;

        public event EventHandler<FileRemotelyChangedEventArgs> FileRemotelyChanged = delegate { };

        public void NotifyFileChange(FileChange fileChange)
        {
            GetHub().Invoke("NotifyChange", fileChange);
        }

        IHubProxy GetHub()
        {
            if (_hub == null)
            {
                _connection = new HubConnection("http://localhost:8080/");
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