using CS_EventsNotifierSlackBot.Global;
using Nancy;
using System.Linq;
using System.IO;
using System;
using Nancy.IO;
using Nancy.Extensions;
using CS_EventsNotifierSlackBot.RouteModules.Dialogflow.DTO;
using CS_EventsNotifierSlackBot.RouteModules.Dialogflow.Commands;

namespace CS_EventsNotifierSlackBot.RouteModules.Dialogflow {
	public class IntentsModule: NancyModule {
		public IntentsModule(): base(""){
			Post("/intents", params_ => {
				try {
					// body -> json -> RequestWebHookIntent
					RequestWebHookIntent incomeWebHook = RequestWebHookIntent.FromJson(RequestStream.FromStream(Request.Body).AsString());

					IntentType.Type intentType = IntentType.GetType(incomeWebHook?.QueryResult?.Intent?.Name);

					switch(intentType) {
						case IntentType.Type.Undefined:
							break;
						case IntentType.Type.WhereCoworker:
							return onWhereCoworker(WhereCoworkerDTO.FromObject(incomeWebHook?.QueryResult?.Parameters));
					}
				} catch(Exception e) {
					Console.Out.WriteLine(e.Message + "\n" + e.StackTrace);
				}

				return 404;
			});
		}

		private Response onWhereCoworker(WhereCoworkerDTO whereCoworkerDTO) {
			Console.Out.WriteLine($"{whereCoworkerDTO.TargetName} {whereCoworkerDTO.TargetLastname}");
			return 200;
		}
	}
}
