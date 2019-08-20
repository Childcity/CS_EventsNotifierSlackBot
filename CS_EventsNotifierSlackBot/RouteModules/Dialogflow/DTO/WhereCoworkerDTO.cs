using CS_EventsNotifierSlackBot.Global;
using Newtonsoft.Json;
using System;

namespace CS_EventsNotifierSlackBot.RouteModules.Dialogflow.DTO {

	public partial class WhereCoworkerDTO {

		[JsonProperty("target-name", NullValueHandling = NullValueHandling.Ignore)]
		public string TargetName { get; set; }

		[JsonProperty("target-lastname", NullValueHandling = NullValueHandling.Ignore)]
		public string TargetLastname { get; set; }

		[JsonProperty("date", NullValueHandling = NullValueHandling.Ignore)]
		public DateTimeOffset? Date { get; set; }

		[JsonProperty("time-period", NullValueHandling = NullValueHandling.Ignore)]
		public TimePeriodDTO TimePeriod { get; set; }

		[JsonProperty("target-middlename", NullValueHandling = NullValueHandling.Ignore)]
		public string TargetMiddlename { get; set; }

		[JsonProperty("time", NullValueHandling = NullValueHandling.Ignore)]
		public string Time { get; set; }

		[JsonProperty("date-time", NullValueHandling = NullValueHandling.Ignore)]
		public string DateTime { get; set; }
	}

	public partial class WhereCoworkerDTO {

		public static WhereCoworkerDTO FromObject(object obj) => FromJson(JsonConvert.SerializeObject(obj, JsonConverterSettings.Settings));

		public static WhereCoworkerDTO FromJson(string json) => JsonConvert.DeserializeObject<WhereCoworkerDTO>(json, JsonConverterSettings.Settings);
	}

	public static class SerializeWhereCoworkerDTO {

		public static string ToJson(this WhereCoworkerDTO self) => JsonConvert.SerializeObject(self, JsonConverterSettings.Settings);
	}
}