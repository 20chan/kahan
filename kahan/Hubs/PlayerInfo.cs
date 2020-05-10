namespace kahan.Hubs {
    public class PlayerInfo {
        public string Nickname { get; set; }

        public string SongId { get; set; }
        public float Current { get; set; }
        public float Duration { get; set; }
        public bool IsPlaying { get; set; }
    }
}