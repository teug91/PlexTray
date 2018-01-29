using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PlexTray
{
    public class PlexMedia
    {
        [XmlAttribute("ratingKey")]
        public string Key { get; set; }

        [XmlAttribute("title")]
        public string Name { get; set; }
    }

    [XmlRoot("MediaContainer")]
    public class PlexMediaList
    {
        [XmlElement("Video")]
        public List<PlexMedia> AllePlexMedia { get; set; }
    }
}