
namespace MegaBotv2
{
    public class userinfo
    {
        public string user_id { get; set; }
        public string username { get; set; }
        public uint tokens { get; set; }
        public int level { get; set; }
        public int xp { get; set; }
        public int messages { get; set; }
        public int blacklist { get; set; }
        public string steamid { get; set; }

        public string server_id { get; set; }
        public int levelupnot { get; set; }
        public int joinnot { get; set; }
        public int leavenot { get; set; }

        public string channel_id { get; set; }
        public string channel_name { get; set; }
    }
}
