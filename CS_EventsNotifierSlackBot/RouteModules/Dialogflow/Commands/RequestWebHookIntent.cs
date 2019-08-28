using CS_EventsNotifierSlackBot.Global;
using CS_EventsNotifierSlackBot.RouteModules.Dialogflow.DTO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CS_EventsNotifierSlackBot.RouteModules.Dialogflow.Commands {

	public partial class RequestWebHookIntent {

		[JsonProperty("responseId", NullValueHandling = NullValueHandling.Ignore)]
		public string ResponseId { get; set; }

		[JsonProperty("queryResult", NullValueHandling = NullValueHandling.Ignore)]
		public QueryResult QueryResult { get; set; }

		[JsonProperty("originalDetectIntentRequest", NullValueHandling = NullValueHandling.Ignore)]
		public OriginalDetectIntentRequest OriginalDetectIntentRequest { get; set; }

		[JsonProperty("session", NullValueHandling = NullValueHandling.Ignore)]
		public string Session { get; set; }
	}

	public partial class OriginalDetectIntentRequest {

		[JsonProperty("payload", NullValueHandling = NullValueHandling.Ignore)]
		public Payload Payload { get; set; }
	}

	public partial class Payload {
		// Not needed now. For feauture use
	}

	public partial class QueryResult {

		[JsonProperty("queryText", NullValueHandling = NullValueHandling.Ignore)]
		public string QueryText { get; set; }

		[JsonProperty("parameters", NullValueHandling = NullValueHandling.Ignore)]
		public object Parameters { get; set; }

		[JsonProperty("allRequiredParamsPresent", NullValueHandling = NullValueHandling.Ignore)]
		public bool? AllRequiredParamsPresent { get; set; }

		[JsonProperty("fulfillmentText", NullValueHandling = NullValueHandling.Ignore)]
		public string FulfillmentText { get; set; }

		[JsonProperty("fulfillmentMessages", NullValueHandling = NullValueHandling.Ignore)]
		public List<FulfillmentMessage> FulfillmentMessages { get; set; }

		[JsonProperty("outputContexts", NullValueHandling = NullValueHandling.Ignore)]
		public List<OutputContext> OutputContexts { get; set; }

		[JsonProperty("intent", NullValueHandling = NullValueHandling.Ignore)]
		public Intent Intent { get; set; }

		[JsonProperty("intentDetectionConfidence", NullValueHandling = NullValueHandling.Ignore)]
		public long? IntentDetectionConfidence { get; set; }

		[JsonProperty("languageCode", NullValueHandling = NullValueHandling.Ignore)]
		public string LanguageCode { get; set; }
	}

	public partial class FulfillmentMessage {

		[JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
		public Text Text { get; set; }
	}

	public partial class Text {

		[JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> TextText { get; set; }
	}

	public partial class Intent {

		[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
		public string Name { get; set; }

		[JsonProperty("displayName", NullValueHandling = NullValueHandling.Ignore)]
		public string DisplayName { get; set; }

		[JsonProperty("endInteraction", NullValueHandling = NullValueHandling.Ignore)]
		public bool? EndInteraction { get; set; }
	}

	public partial class OutputContext {

		[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
		public string Name { get; set; }

		[JsonProperty("lifespanCount", NullValueHandling = NullValueHandling.Ignore)]
		public long? LifespanCount { get; set; }

		[JsonProperty("parameters", NullValueHandling = NullValueHandling.Ignore)]
		public object OutputContextParameters { get; set; }
	}

	public partial class RequestWebHookIntent {

		public static RequestWebHookIntent FromJson(string json) => JsonConvert.DeserializeObject<RequestWebHookIntent>(json, JsonConverterSettings.Settings);
	}

	public static class SerializeRequestWebHookIntent {

		public static string ToJson(this RequestWebHookIntent self) => JsonConvert.SerializeObject(self, JsonConverterSettings.Settings);
	}
}