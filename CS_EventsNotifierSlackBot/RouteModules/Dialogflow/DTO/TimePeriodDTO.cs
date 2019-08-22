namespace CS_EventsNotifierSlackBot.RouteModules.Dialogflow.DTO {
	using System;
	using Newtonsoft.Json;

	public partial class TimePeriodDTO {

		public TimePeriodDTO() { }

		public TimePeriodDTO(TimePeriodDTO timePeriod) {
			StartTime = timePeriod.StartTime;
			EndTime = timePeriod.EndTime;
		}

		[JsonProperty("startTime", NullValueHandling = NullValueHandling.Ignore)]
		public DateTimeOffset? StartTime { get; set; }

		[JsonProperty("endTime", NullValueHandling = NullValueHandling.Ignore)]
		public DateTimeOffset? EndTime { get; set; }
	}
}