using Newtonsoft.Json;

namespace PlexTray.Pushbullet
{
    class PushbulletMesssage
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
