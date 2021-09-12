using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace webapi
{
    public class ChatHub : Hub
    {
        #region snippet_SendMessage
        public async Task SendMessage(string user, string message)
        {            
           
            await Clients.Caller.SendAsync("ReceiveMessage", user, message);
            await Clients.Caller.SendAsync("ReceiveMessage2", user, message);
        }

        
        #endregion
    }
}
