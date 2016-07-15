using System.Collections.Generic;
using System.Threading.Tasks;
using Bevelop.Messages;
using Microsoft.AspNet.SignalR;

namespace Bevelop.Server
{
    public class ChangeHub : Hub
    {
        static IList<string> _connectionIds = new List<string>();

        public void NotifyChange(FileChange message)
        {
            Clients.Others.notify(message);
        }

        public override Task OnConnected()
        {
            _connectionIds.Add(Context.ConnectionId);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _connectionIds.Remove(Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }
    }
}