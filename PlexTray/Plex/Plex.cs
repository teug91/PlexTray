using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PlexTray
{
    internal static class Plex
    {
        private static HttpClient client = new HttpClient();

        public static void Init()
        {
            client = new HttpClient();
            client.BaseAddress = SettingsManager.GetHost();
            client.Timeout = new TimeSpan(0, 0, 5);
        }

        public static bool IsAvailable()
        {
            // TODO

            return true;
        }

        /// <summary>
        /// Opens Plex in standard browser.
        /// </summary>
        public static void OpenPlex()
        {
            Process.Start(SettingsManager.GetHost().ToString());
        }

        /// <summary>
        /// Updates all libraries.
        /// </summary>
        public static async void UpdateLibraries()
        {
            string plexHost = SettingsManager.GetHost().ToString();
            string plexToken = SettingsManager.GetPlexToken();

            var content = new[]
            {
                new KeyValuePair<string, string>("X-Plex-Token", plexToken)
            };

            var url = plexHost + "library/sections/?X-Plex-Token=" + plexToken;

            var result = await Get(url);

            if (result != "")
            {
                PlexLibraryList plexLibraries;

                XmlSerializer serializer = new XmlSerializer(typeof(PlexLibraryList));
                using (TextReader reader = new StringReader(result))
                {
                    plexLibraries = (PlexLibraryList)serializer.Deserialize(reader);
                }



                foreach (var item in plexLibraries.PlexLibraries)
                {
                    await Get(plexHost + "library/sections/" + item.Key + "/refresh?force=1&X-Plex-Token=" + plexToken);
                }
            }
        }

        private static async Task<string> Get(string requestUri)
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

        // EXPERIMENTAL PLEX STUFF

        public static async void OpenPlexMedia(string name)
        {
			bool isTV = false;

			if (name.Contains("- S"))
				isTV = true;

			if (isTV)
				name = name.Substring(0, name.LastIndexOf("- S") - 1);
			else if (name.Contains('('))
				name = name.Substring(0, name.LastIndexOf('(') - 1);

			PlexLibraryList plexLibraries = await GetLibraries();
            string baseMediaUrl = await GetBaseMediaUrl();

            if (plexLibraries != null)
            {
                // PMS needs time to add new media to library.
                int i = 0;
                while (i < 10)
                {
                    foreach (PlexLibrary library in plexLibraries.PlexLibraries)
                    {
                        string mediaKey = await FindItem(Int32.Parse(library.Key), name, isTV = true);
                        if (mediaKey != "")
                        {
                            Process.Start(baseMediaUrl + mediaKey);
                            return;
                        }
                    }

                    i++;
                    await Task.Delay(1000);
                }
            }
        }

        private static async Task<PlexLibraryList> GetLibraries()
        {
            PlexLibraryList plexLibraries;

            string plexHost = SettingsManager.GetHost().ToString();
            string plexToken = SettingsManager.GetPlexToken();

            var url = plexHost + "library/sections/?X-Plex-Token=" + plexToken;

            var result = await Get(url);

            if (result != "")
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PlexLibraryList));
                using (TextReader reader = new StringReader(result))
                {
                    plexLibraries = (PlexLibraryList)serializer.Deserialize(reader);
                }

                return plexLibraries;
            }

            return null;
        }

        private static async Task<string> FindItem(int libraryKey, string name, bool isTV)
        {
			string plexHost = SettingsManager.GetHost().ToString();
            string plexToken = SettingsManager.GetPlexToken();

            var url = plexHost + "library/sections/" + libraryKey + "/all?X-Plex-Token=" + plexToken;

            var result = await Get(url);

			PlexMediaList mediaList = new PlexMediaList();
			PlexTVList tvList = new PlexTVList();

			if (result != "")
            {
                using (TextReader reader = new StringReader(result))
                {
					if(isTV)
						tvList = (PlexTVList)new XmlSerializer(typeof(PlexTVList)).Deserialize(reader);
					else
						mediaList = (PlexMediaList)new XmlSerializer(typeof(PlexMediaList)).Deserialize(reader);
				}

				if (isTV)
					foreach (PlexTV media in tvList.AllPlexTV)
					{
					    if (media.Name == name)
					        return media.Key;
					}

				else
					foreach (PlexMedia media in mediaList.AllePlexMedia)
					{
						if (media.Name == name)
							return media.Key;
					}
			}

            return "";
        }

        private static async Task<string> GetBaseMediaUrl()
        {
            string plexHost = SettingsManager.GetHost().ToString();
            string plexToken = SettingsManager.GetPlexToken();

            var url = plexHost + "?X-Plex-Token=" + plexToken;

            var result = await Get(url);

            PlexServer plexServer;

            if (result != "")
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PlexServer));
                using (TextReader reader = new StringReader(result))
                {
                    plexServer = (PlexServer)serializer.Deserialize(reader);
                }

                return plexHost + "web/index.html#!/server/" + plexServer.Key + "/details?key=%2Flibrary%2Fmetadata%2F";
            }

            return "";
        }
    }
}
