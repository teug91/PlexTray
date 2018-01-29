using Newtonsoft.Json;

namespace PlexTray.Pushbullet
{
    class HtmlUrl
    {
        [JsonProperty("pushes")]
        public string Url { get; set; }
    }
}
