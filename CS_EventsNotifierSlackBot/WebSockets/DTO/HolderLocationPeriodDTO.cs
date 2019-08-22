using CS_EventsNotifierSlackBot.Global;
using CS_EventsNotifierSlackBot.RouteModules.Dialogflow.DTO;
using Newtonsoft.Json;

namespace CS_EventsNotifierSlackBot.WebSockets.DTO {

	public partial class HolderLocationPeriodDTO {

		public string HolderSurname { get; set; }

		public string HolderName { get; set; }

		public string HolderMiddlename { get; set; }

		public TimePeriodDTO TimePeriod { get; set; }
	}

	public partial class HolderLocationPeriodDTO {

		public static HolderLocationPeriodDTO FromObject(object obj) => FromJson(JsonConvert.SerializeObject(obj, JsonConverterSettings.Settings));

		public static HolderLocationPeriodDTO FromJson(string json) => JsonConvert.DeserializeObject<HolderLocationPeriodDTO>(json, JsonConverterSettings.Settings);
	}

	public static class SerializeHolderLocationPeriod {

		public static string ToJson(this HolderLocationPeriodDTO self, bool indented = false) => JsonConvert.SerializeObject(self, indented ? Formatting.Indented : Formatting.None, JsonConverterSettings.Settings);
	}
}