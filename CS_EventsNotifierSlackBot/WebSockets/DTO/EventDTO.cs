using CS_EventsNotifierSlackBot.Global;
using Newtonsoft.Json;
using System;

namespace CS_EventsNotifierSlackBot.WebSockets.DTO {

	public partial class EventDTO {

		public int? EventCode { get; set; }

		public byte? Direction { get; set; }

		public decimal? CardNumber { get; set; }

		public DateTime? EventTime { get; set; }

		//

		public string AccountNumber { get; set; }

		//

		public string StartAreaName { get; set; }

		public string TargetAreaName { get; set; }

		//

		public string ObjectType { get; set; }

		public string ObjectName { get; set; }

		//

		public string HolderType { get; set; }

		public string HolderSurname { get; set; }

		public string HolderName { get; set; }

		public string HolderMiddlename { get; set; }

		public string HolderDepartment { get; set; }

		public string HolderTabNumber { get; set; }

		public byte[] HolderPhoto { get; set; }

	}

	public partial class EventDTO {

		public static EventDTO FromObject(object obj) => FromJson(JsonConvert.SerializeObject(obj, JsonConverterSettings.Settings));

		public static EventDTO FromJson(string json) => JsonConvert.DeserializeObject<EventDTO>(json, JsonConverterSettings.Settings);
	}

	public static class SerializeEventDTO {

		public static string ToJson(this EventDTO self) => JsonConvert.SerializeObject(self, JsonConverterSettings.Settings);
	}
}