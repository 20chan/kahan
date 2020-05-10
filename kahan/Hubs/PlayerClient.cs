using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace kahan.Hubs {
    public class PlayerClient : IAsyncDisposable {
        public PlayerInfo Info;

        public event Action<PlayerInfo> OnInfoUpdated;

        private readonly string hubUrl;
        private bool started;
        private HubConnection hub;

        public PlayerClient(string siteUrl) {
            Info = new PlayerInfo();
            hubUrl = siteUrl.TrimEnd('/') + Messages.HUBPATH;
        }

        public async Task StartAsync() {
            if (started) {
                return;
            }

            hub = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .Build();

            hub.On<PlayerInfo>(Messages.UpdatePlayerInfo, info => {
                OnInfoUpdated?.Invoke(info);
            });

            await hub.StartAsync();

            started = true;

            await Register();
        }

        public async Task StopAsync() {
            if (started) {
                return;
            }

            await hub.StopAsync();
            await hub.DisposeAsync();
            hub = null;
            started = false;
        }

        public async ValueTask DisposeAsync() {
            await StopAsync();
        }

        public async Task Register() {
            await hub.SendAsync(Messages.RegisterPlayer, Info.Nickname);
        }

        public async Task UpdateInfo() {
            await hub.SendAsync(Messages.UpdatePlayerInfo, Info);
        }
    }
}