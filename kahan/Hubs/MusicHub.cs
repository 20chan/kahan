using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace kahan.Hubs {
    public class MusicHub : Hub {
        private const string GroupPlayer = "groupPlayer";
        private const string GroupAdmin = "groupAdmin";

        private static readonly Dictionary<string, PlayerInfo> players = new Dictionary<string, PlayerInfo>();

        public async Task RegisterPlayer(string nickname) {
            var id = Context.ConnectionId;
            players[id] = new PlayerInfo {
                Nickname = nickname,
            };
            await Groups.AddToGroupAsync(id, GroupPlayer);
            await SendAllPlayerInfoTo(Clients.Group(GroupAdmin));
        }

        public async Task RegisterAdmin() {
            var id = Context.ConnectionId;
            await Groups.AddToGroupAsync(id, GroupAdmin);
            await SendAllPlayerInfoTo(Clients.Client(id));
        }

        public override async Task OnDisconnectedAsync(Exception exception) {
            var id = Context.ConnectionId;
            Console.WriteLine($"disconneted {exception?.Message} {id}");

            if (players.Remove(id)) {
                await SendAllPlayerInfoTo(Clients.Group(GroupAdmin));
            }
            await base.OnDisconnectedAsync(exception);
        }

        public void UpdatePlayerInfo(PlayerInfo info) {
            var id = Context.ConnectionId;
            players[id] = info;
        }

        public async Task UploadPlayerInfo(string id, PlayerInfo info) {
            await Clients.Client(id).SendAsync(Messages.UpdatePlayerInfo, info);
        }

        public async Task RequestAllPlayerInfo() {
            var id = Context.ConnectionId;
            await SendAllPlayerInfoTo(Clients.Client(id));
        }

        private async Task SendAllPlayerInfoTo(IClientProxy targets) {
            await targets.SendAsync(Messages.UpdateAllPlayerInfo, players);
        }
    }
}