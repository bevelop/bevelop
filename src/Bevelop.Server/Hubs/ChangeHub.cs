using Bevelop.Messages;
using Microsoft.AspNet.SignalR;

namespace Bevelop.Server.Hubs
{
    public class ChangeHub : Hub
    {
        public void NotifyChange(FileChange message)
        {
            Clients.Others.notify(message);
        }
    }
}