using CS_EventsNotifierSlackBot.Global;
using CS_EventsNotifierSlackBot.RouteModules.Dialogflow.DTO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CS_EventsNotifierSlackBot.WebSockets.DTO {

	public partial class HolderLocationDTO {
		public EventDTO HolderInfo { get; set; }

		public TimePeriodDTO TimePeriod { get; set; }

		public List<EventInfoDTO> EventsInfo { get; set; }
	}

	public partial class HolderLocationDTO {

		public static HolderLocationDTO FromObject(object obj) => FromJson(JsonConvert.SerializeObject(obj, JsonConverterSettings.Settings));

		public static HolderLocationDTO FromJson(string json) => JsonConvert.DeserializeObject<HolderLocationDTO>(json, JsonConverterSettings.Settings);
	}

	public static class SerializeCoworkerLocationDTO {

		public static string ToJson(this EventDTO self) => JsonConvert.SerializeObject(self, JsonConverterSettings.Settings);
	}
}