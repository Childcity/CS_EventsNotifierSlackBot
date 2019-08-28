using CS_EventsNotifierSlackBot.Global;
using Newtonsoft.Json;
using System;

namespace CS_EventsNotifierSlackBot.RouteModules.Dialogflow.DTO {

	public partial class WhenCoworkerDTO {
		[JsonProperty("query-type", NullValueHandling = NullValueHandling.Ignore)]
		public string QueryType { get; set; }

		[JsonProperty("target-name", NullValueHandling = NullValueHandling.Ignore)]
		public string TargetName { get; set; }

		[JsonProperty("target-middlename", NullValueHandling = NullValueHandling.Ignore)]
		public string TargetMiddlename { get; set; }

		[JsonProperty("target-lastname", NullValueHandling = NullValueHandling.Ignore)]
		public string TargetLastname { get; set; }

		[JsonProperty("date", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
		public DateTimeOffset? Date { get; set; }

		[JsonProperty("in-or-out", NullValueHandling = NullValueHandling.Ignore)]
		public string InOrOut { get; set; }

	}

	public partial class WhenCoworkerDTO {

		public static WhenCoworkerDTO FromObject(object obj) => FromJson(JsonConvert.SerializeObject(obj, JsonConverterSettings.Settings));

		public static WhenCoworkerDTO FromJson(string json) => JsonConvert.DeserializeObject<WhenCoworkerDTO>(json, JsonConverterSettings.Settings);
	}

	public static class SerializeWhenCoworkerDTO {

		public static string ToJson(this WhenCoworkerDTO self, bool indented = false) => JsonConvert.SerializeObject(self, indented ? Formatting.Indented : Formatting.None, JsonConverterSettings.Settings);
	}
}