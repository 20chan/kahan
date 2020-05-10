using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace kahan.Hubs {
    public class AdminClient : IAsyncDisposable {
        public string Nickname;

        public event Action<Dictionary<string, PlayerInfo>> OnAllPlayerInfoUpdate;

        private readonly string hubUrl;
        private bool started;
        private HubConnection hub;

        public AdminClient(string siteUrl) {
            hubUrl = siteUrl.TrimEnd('/') + Messages.HUBPATH;
        }

        public async Task StartAsync() {
            if (started) {
                return;
            }

            hub = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .Build();

            hub.On<Dictionary<string, PlayerInfo>>(Messages.UpdateAllPlayerInfo, parameter => {
                OnAllPlayerInfoUpdate?.Invoke(parameter);
            });

            await hub.StartAsync();

            started = true;

            await hub.SendAsync(Messages.RegisterAdmin);
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

        public async Task RequestAllPlayerInfo() {
            await hub.SendAsync(Messages.RequestAllPlayerInfo);
        }

        public async Task UploadPlayerInfo(string userId, PlayerInfo parameter) {
            await hub.SendAsync(Messages.UploadPlayerInfo, userId, parameter);
        }
    }
}