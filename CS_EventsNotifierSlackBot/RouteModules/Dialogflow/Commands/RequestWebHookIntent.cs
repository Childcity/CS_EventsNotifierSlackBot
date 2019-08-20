using CS_EventsNotifierSlackBot.Global;
using CS_EventsNotifierSlackBot.RouteModules.Dialogflow.DTO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CS_EventsNotifierSlackBot.RouteModules.Dialogflow.Commands {

	public static class IntentType {
		public static readonly string WhereCoworker = "abeee50f-81ed-44d5-9461-4ebd500a9e29";

		public enum Type {
			Undefined,
			WhereCoworker
		}

		public static Type GetType(string intentName) {
			if(intentName != null) {
				if(intentName.Contains(WhereCoworker))
					return Type.WhereCoworker;
			}

			return Type.Undefined;
		}
	}

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
		public OutputContextParameters Parameters { get; set; }
	}

	public partial class OutputContextParameters {

		[JsonProperty("target-name", NullValueHandling = NullValueHandling.Ignore)]
		public string TargetName { get; set; }

		[JsonProperty("target-name.original", NullValueHandling = NullValueHandling.Ignore)]
		public string TargetNameOriginal { get; set; }

		[JsonProperty("target-lastname", NullValueHandling = NullValueHandling.Ignore)]
		public string TargetLastname { get; set; }

		[JsonProperty("target-lastname.original", NullValueHandling = NullValueHandling.Ignore)]
		public string TargetLastnameOriginal { get; set; }

		[JsonProperty("date", NullValueHandling = NullValueHandling.Ignore)]
		public DateTimeOffset? Date { get; set; }

		[JsonProperty("date.original", NullValueHandling = NullValueHandling.Ignore)]
		public string DateOriginal { get; set; }

		[JsonProperty("time-period", NullValueHandling = NullValueHandling.Ignore)]
		public TimePeriodDTO TimePeriod { get; set; }

		[JsonProperty("time-period.original", NullValueHandling = NullValueHandling.Ignore)]
		public string TimePeriodOriginal { get; set; }

		[JsonProperty("target-middlename", NullValueHandling = NullValueHandling.Ignore)]
		public string TargetMiddlename { get; set; }

		[JsonProperty("target-middlename.original", NullValueHandling = NullValueHandling.Ignore)]
		public string TargetMiddlenameOriginal { get; set; }

		[JsonProperty("time", NullValueHandling = NullValueHandling.Ignore)]
		public string Time { get; set; }

		[JsonProperty("time.original", NullValueHandling = NullValueHandling.Ignore)]
		public string TimeOriginal { get; set; }

		[JsonProperty("date-time", NullValueHandling = NullValueHandling.Ignore)]
		public string DateTime { get; set; }

		[JsonProperty("date-time.original", NullValueHandling = NullValueHandling.Ignore)]
		public string DateTimeOriginal { get; set; }
	}

	public partial class RequestWebHookIntent {

		public static RequestWebHookIntent FromJson(string json) => JsonConvert.DeserializeObject<RequestWebHookIntent>(json, JsonConverterSettings.Settings);
	}

	public static class RequestWebHookIntentSerialize {

		public static string ToJson(this RequestWebHookIntent self) => JsonConvert.SerializeObject(self, JsonConverterSettings.Settings);
	}
}