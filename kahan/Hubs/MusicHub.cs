using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace kahan.Hubs {
    public class MusicHub : Hub {
        private const string GroupClient = "groupClient";
        private const string GroupAdmin = "groupAdmin";

        private static readonly Dictionary<string, string> clients = new Dictionary<string, string>();

        public async Task RegisterClient(string nickname) {
            var id = Context.ConnectionId;
            clients[id] = nickname;
            await Groups.AddToGroupAsync(id, GroupClient);
            await QueryStatusTo(Clients.Group(GroupAdmin));
        }

        public async Task RegisterAdmin() {
            var id = Context.ConnectionId;
            await Groups.AddToGroupAsync(id, GroupAdmin);
            await QueryStatusTo(Clients.Client(id));
        }

        public override async Task OnDisconnectedAsync(Exception exception) {
            var id = Context.ConnectionId;
            Console.WriteLine($"disconneted {exception?.Message} {id}");

            if (clients.Remove(id)) {
                await QueryStatusTo(Clients.Group(GroupAdmin));
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task PongStatus(string id, string nickname) {
            clients[id] = nickname;
        }

        public async Task RequestPlay(string user, string parameter) {
            await Clients.Client(user).SendAsync(Messages.RequestPlay, parameter);
        }

        private async Task QueryStatusTo(IClientProxy targets) {
            await targets.SendAsync(Messages.QueryStatus, clients);
        }
    }
}