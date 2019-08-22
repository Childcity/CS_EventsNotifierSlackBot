using CS_EventsNotifierSlackBot.Global;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CS_EventsNotifierSlackBot.RouteModules.Dialogflow.Commands {

	public partial class ResponseWebHookIntent {

		[JsonProperty("fulfillmentText", NullValueHandling = NullValueHandling.Ignore)]
		public string FulfillmentText { get; set; }

		[JsonProperty("fulfillmentMessages", NullValueHandling = NullValueHandling.Ignore)]
		public List<FulfillmentMessage> FulfillmentMessages { get; set; }
	}

	public partial class FulfillmentMessage {

		[JsonProperty("type", Required = Required.DisallowNull)]
		public long Type { get; set; }

		[JsonProperty("platform", NullValueHandling = NullValueHandling.Ignore)]
		public string Platform { get; set; }

		[JsonProperty("speech", NullValueHandling = NullValueHandling.Ignore)]
		public string Speech { get; set; }

		[JsonProperty("imageUrl", NullValueHandling = NullValueHandling.Ignore)]
		public Uri ImageUrl { get; set; }
	}

	public partial class ResponseWebHookIntent {

		public static ResponseWebHookIntent FromJson(string json) => JsonConvert.DeserializeObject<ResponseWebHookIntent>(json, JsonConverterSettings.Settings);
	}

	public static class SerializeResponseWebHookIntent {

		public static string ToJson(this ResponseWebHookIntent self) => JsonConvert.SerializeObject(self, JsonConverterSettings.Settings);
	}
}