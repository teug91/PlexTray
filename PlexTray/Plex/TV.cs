using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PlexTray
{
	public class PlexTV
	{
		[XmlAttribute("ratingKey")]
		public string Key { get; set; }

		[XmlAttribute("title")]
		public string Name { get; set; }
	}

	[XmlRoot("MediaContainer")]
	public class PlexTVList
	{
		[XmlElement("Directory")]
		public List<PlexTV> AllPlexTV { get; set; }
	}
}