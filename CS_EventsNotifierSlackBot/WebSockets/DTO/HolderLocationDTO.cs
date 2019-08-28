using CS_EventsNotifierSlackBot.Global;
using CS_EventsNotifierSlackBot.RouteModules.Dialogflow.DTO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CS_EventsNotifierSlackBot.WebSockets.DTO {

	public partial class HolderLocationDTO {
		public EventDTO HolderInfo { get; set; }

		public TimePeriodDTO TimePeriod { get; set; }

		public List<EventInfoDTO> EventsInfo { get; set; }

		public QueryType.Type QueryType { get; set; }

		public bool? IsHolderIn { get; set; }
	}

	public partial class HolderLocationDTO {

		public static HolderLocationDTO FromObject(object obj) => FromJson(JsonConvert.SerializeObject(obj, JsonConverterSettings.Settings));

		public static HolderLocationDTO FromJson(string json) => JsonConvert.DeserializeObject<HolderLocationDTO>(json, JsonConverterSettings.Settings);
	}

	public static class SerializeHolderLocationDTO {

		public static string ToJson(this HolderLocationDTO self, bool indented = false) => JsonConvert.SerializeObject(self, indented ? Formatting.Indented : Formatting.None, JsonConverterSettings.Settings);
	}
}