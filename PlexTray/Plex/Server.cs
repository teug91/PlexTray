using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PlexTray
{
    [XmlRoot("MediaContainer")]
    public class PlexServer
    {
        [XmlAttribute("machineIdentifier")]
        public string Key { get; set; }
    }
}