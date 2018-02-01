using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WebSocketSharp;
using Newtonsoft.Json;
using System.Net;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Globalization;

namespace PlexTray.Pushbullet
{
    internal static class Communicator
    {
        private static HttpClient client = new HttpClient();

        public static void Init()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("Access-Token", SettingsManager.GetPushbulletToken());
            client.Timeout = new TimeSpan(0, 0, 30);
			GetPushes();

            var ws = new WebSocket("wss://stream.pushbullet.com/websocket/" + SettingsManager.GetPushbulletToken());
            ws.OnMessage += (sender, e) =>
                ReadMesssage(e);

            ws.Connect();
        }

        private static void ReadMesssage(MessageEventArgs e)
        {
            if (e.IsText)
            {
                var message = JsonConvert.DeserializeObject<PushbulletMesssage>(e.Data.ToString());
                if (message.Type == "nop")
                {
                    Debug.WriteLine("Connection is still active...");
                }
                else if (message.Type == "tickle")
                {
                    GetPushes();
                }
            }
        }

        private static async void GetPushes()
        {
            var content = new[]
            {
                new KeyValuePair<string, string>("Access-Token:", SettingsManager.GetPushbulletToken())
            };

			var push = await Get("https://api.pushbullet.com/v2/pushes?modified_after=" + SettingsManager.GetTimestamp().ToString(CultureInfo.InvariantCulture));//SettingsManager.GetTimestamp().ToString(CultureInfo.InvariantCulture));
			//var push = await Get("https://api.pushbullet.com/v2/pushes?limit=1");

			dynamic deserializedValue = JsonConvert.DeserializeObject(push);
            var values = deserializedValue["pushes"];

            if(values != null)
                foreach (var value in values)
                {
                    string url = (string)value["file_url"];
                    string name = GetHTML(url);

                    var timestamp = (decimal)value["created"];

					if (timestamp > SettingsManager.GetTimestamp())
						 SettingsManager.SetTimestamp(timestamp + 1);

                    PushRecieved?.Invoke(name, null);
                    await Task.Delay(5000);
                }
        }

        private static async Task<string> Get(string requestUri, IEnumerable<KeyValuePair<string, string>> nameValueCollection = null)
        {
            try
            {
                var result = await client.GetAsync(requestUri);
                return await result.Content.ReadAsStringAsync();
            }

            catch (HttpRequestException)
            {
                return "";
            }
        }

        public static string GetHTML(string url)
        {
            string htmlCode;

            using (WebClient client = new WebClient())
            {
                htmlCode = client.DownloadString(url);
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlCode);
            foreach (HtmlNode table in doc.DocumentNode.SelectNodes("//table"))
            {
				if (table.HasChildNodes)
                foreach (HtmlNode row in table.SelectNodes(".//tr"))
                {
                    foreach (HtmlNode cell in row.SelectNodes("th|td[2]"))
                    {
                        if (cell.Name == "td")
                        {
                            string name = cell.InnerText.Substring(0, cell.InnerText.IndexOf("."));
                            return name;
                        }
                    }
                }
            }
            return "";
        }

        public static EventHandler<EventArgs> PushRecieved;
    }
}
