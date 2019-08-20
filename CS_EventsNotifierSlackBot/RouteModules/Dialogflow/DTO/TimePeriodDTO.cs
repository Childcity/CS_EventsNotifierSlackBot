namespace CS_EventsNotifierSlackBot.RouteModules.Dialogflow.DTO {
	using System;
	using Newtonsoft.Json;

	public partial class TimePeriodDTO {
		[JsonProperty("startTime", NullValueHandling = NullValueHandling.Ignore)]
		public DateTimeOffset? StartTime { get; set; }

		[JsonProperty("endTime", NullValueHandling = NullValueHandling.Ignore)]
		public DateTimeOffset? EndTime { get; set; }
	}
}