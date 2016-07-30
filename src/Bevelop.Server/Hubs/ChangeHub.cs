using System;
using System.Linq;
using Bevelop.Messages;
using Bevelop.Server.Services;
using Microsoft.AspNet.SignalR;

namespace Bevelop.Server.Hubs
{
    public class ChangeHub : Hub
    {
        static readonly IFileChangeStore FileChangeStore;
        static readonly IClock Clock;

        static ChangeHub()
        {
            Clock = new Clock();
            FileChangeStore = new FileChangeStore(Clock);
        }

        public void NotifyChange(FileChange fileChange)
        {
            fileChange.Date = Clock.UtcNow;
            FileChangeStore.Save(fileChange);

            Clients.Others.notify(fileChange);
        }

        public void RequestChanges(string username, FileAddress fileAddress)
        {
            var changes = FileChangeStore.GetByAddress(fileAddress);

            foreach (var change in changes.Where(change => !change.User.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                Clients.Caller.notify(change);
            }
        }
    }
}