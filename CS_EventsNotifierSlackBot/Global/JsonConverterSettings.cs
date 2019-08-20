using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;

namespace CS_EventsNotifierSlackBot.Global {
	internal static class JsonConverterSettings {

		public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings {
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
			DateParseHandling = DateParseHandling.None,
			Converters = {
				new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
			},
		};
	}
}