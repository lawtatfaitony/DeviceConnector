﻿using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VxClient
{ 
    public class ServerHub : Hub
    { 
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendMdeiaMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMediaMessage", user, message);
        }
    }
}
