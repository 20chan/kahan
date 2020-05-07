using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace kahan.Hubs {
    public class MusicClient : IAsyncDisposable {
        public string Nickname;

        public event Action<string> OnRequestPlay;

        private readonly string hubUrl;
        private bool started;
        private HubConnection hub;

        public MusicClient(string siteUrl) {
            hubUrl = siteUrl.TrimEnd('/') + Messages.HUBPATH;
        }

        public async Task StartAsync() {
            if (started) {
                return;
            }

            hub = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .Build();

            hub.On(Messages.PingStatus, async () => {
                await PingStatus();
            });

            hub.On<string>(Messages.RequestPlay, parameter => {
                OnRequestPlay?.Invoke(parameter);
            });

            await hub.StartAsync();

            started = true;

            await hub.SendAsync(Messages.RegisterClient, Nickname);
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

        private async Task PingStatus() {
            await hub.SendAsync(Messages.PongStatus, Nickname);
        }
    }
}