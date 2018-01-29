using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PlexTray
{
    public class PlexLibrary
    {
        [XmlAttribute("key")]
        public string Key { get; set; }

        [XmlAttribute("title")]
        public string Name { get; set; }
    }

    [XmlRoot("MediaContainer")]
    public class PlexLibraryList
    {
        [XmlElement("Directory")]
        public List<PlexLibrary> PlexLibraries { get; set; }
    }
}