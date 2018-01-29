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
                    // New push available.
                    //GetPush();
                    GetPushes();
                }
            }
        }

        private static async void GetPush()
        {
            var content = new[]
            {
                new KeyValuePair<string, string>("Access-Token:", SettingsManager.GetPushbulletToken())
            };

            var push = await Get("https://api.pushbullet.com/v2/pushes?limit=1");
            //Debug.WriteLine(push);
            //string url = JsonConvert.DeserializeObject<HtmlUrl>(push).Url;

            dynamic deserializedValue = JsonConvert.DeserializeObject(push);
            var values = deserializedValue["pushes"];
            string url = (string) values[0]["file_url"];

            Debug.WriteLine("URL: " + url);
            string name = GetHTML(url);

            PushRecieved?.Invoke(name, null);
            //Debug.WriteLine("Push: " + push);
        }

        private static async void GetPushes()
        {
            var content = new[]
            {
                new KeyValuePair<string, string>("Access-Token:", SettingsManager.GetPushbulletToken())
            };

			//var push = await Get("https://api.pushbullet.com/v2/pushes?modified_after=" + (decimal)SettingsManager.GetTimestamp());//SettingsManager.GetTimestamp().ToString(CultureInfo.InvariantCulture));
			var push = await Get("https://api.pushbullet.com/v2/pushes?limit=1");
			//Debug.WriteLine(push);
			//string url = JsonConvert.DeserializeObject<HtmlUrl>(push).Url;

			dynamic deserializedValue = JsonConvert.DeserializeObject(push);
            var values = deserializedValue["pushes"];

            if(values != null)
                foreach (var value in values)
                {
                    string url = (string)value["file_url"];
                    Debug.WriteLine("URL: " + url);
                    string name = GetHTML(url);

                    float timestamp = (float)value["created"];
                    //float.TryParse((string)value["created"], out timestamp);

                    SettingsManager.SetTimestamp(timestamp);
                    Debug.WriteLine("Timestamp: " + SettingsManager.GetTimestamp().ToString());

                    PushRecieved?.Invoke(name, null);
                    await Task.Delay(5000);
                }

            else
            {
                Debug.WriteLine("Values null");
                Debug.WriteLine(push);
				Debug.WriteLine("https://api.pushbullet.com/v2/pushes?modified_after=" + (decimal)SettingsManager.GetTimestamp());//SettingsManager.GetTimestamp().ToString(CultureInfo.InvariantCulture));
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
                //Console.WriteLine("Found: " + table.Id);
                foreach (HtmlNode row in table.SelectNodes("tr"))
                {
                    int i = 0;
                    foreach (HtmlNode cell in row.SelectNodes("th|td"))
                    {
                        if (cell.Name == "td" && i == 1)
                        {
                            string name = cell.InnerText.Substring(0, cell.InnerText.IndexOf("."));
                            return name;
                            
                        }
                        i++;
                    }
                }
            }
            return "";
        }

        public static EventHandler<EventArgs> PushRecieved;
    }
}
