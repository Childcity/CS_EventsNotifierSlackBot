using CS_EventsNotifierSlackBot.Global;
using CS_EventsNotifierSlackBot.RouteModules.Dialogflow.Commands;
using CS_EventsNotifierSlackBot.RouteModules.Dialogflow.DTO;
using CS_EventsNotifierSlackBot.WebSockets.Commands;
using CS_EventsNotifierSlackBot.WebSockets.DTO;
using Nancy;
using Nancy.Extensions;
using Nancy.IO;
using System;

namespace CS_EventsNotifierSlackBot.RouteModules.Dialogflow {

	public class IntentsModule: NancyModule {

		public IntentsModule() : base("") {
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
					Console.WriteLine("Exception: " + e.Message + "\n" + e.StackTrace);
				}

				return 404;
			});
		}

		private Response onWhereCoworker(WhereCoworkerDTO userQuery) {
			Console.WriteLine(userQuery.ToJson(true));

			var tp = new TimePeriodDTO() {
				StartTime = DateTimeOffset.Now.Date.Add(TimeSpan.Zero),
				EndTime = DateTimeOffset.Now.Date.Add(new TimeSpan(23, 59, 59))
			};

			Console.WriteLine("Default: " + tp.StartTime.ToString() + " " + tp.EndTime.ToString());

			// if user hasn't entered any data and time -> set to current date and time
			if(userQuery.TimePeriod == null 
				&& userQuery.Date == null 
				&& userQuery.DateTimeObject == null 
				&& userQuery.Time == null)
			{
				userQuery.DateTimeObject = new DateTimeWraper(DateTimeOffset.Now);
			}

			// recognize any possible time and data
			tp = RecognizeDate(userQuery, oldTp: tp);
			tp = RecognizeTime(userQuery, oldTp: tp);
			tp = RecognizeDateTime(userQuery, oldTp: tp);

			// create params for Event Server request
			var whereHolderRequest = new HolderLocationPeriodDTO() {
				HolderName = userQuery.TargetName.Replace("?", ""),
				HolderMiddlename = userQuery.TargetMiddlename.Replace("?", ""),
				HolderSurname = userQuery.TargetLastname.Replace("?", ""),
				TimePeriod = tp
			};

			Console.WriteLine(whereHolderRequest.ToJson(true));

			foreach(var eventsListener in GlobalScope.CSEventsListeners) {
				eventsListener.PostCommand(new RequestWhereCoworker(whereHolderRequest));
			}

			// send response to Dialogflow (Dialogflow automatically resend it to Slack, Telegram, Skype, etc.)
			return new ResponseWebHookIntent() {
				FulfillmentText = $"*Буду искать:* '{whereHolderRequest.HolderName} {whereHolderRequest.HolderMiddlename} {whereHolderRequest.HolderSurname}'\n" +
							      $"*c :* {whereHolderRequest.TimePeriod.StartTime.ToString()}\n" +
								  $"*по:* {whereHolderRequest.TimePeriod.EndTime.ToString()}"
			}.ToJson();
		}

		private static TimePeriodDTO RecognizeDate(WhereCoworkerDTO userQuery, TimePeriodDTO oldTp) {
			TimePeriodDTO newTp = new TimePeriodDTO(oldTp);

			// if user input date
			if(userQuery.Date.HasValue) {
				DateTimeOffset newDate = userQuery.Date.Value.Date.Add(TimeSpan.Zero); //extract date with zero time
				TimeSpan? delta = oldTp.StartTime?.Subtract(newDate);
				newTp.StartTime = oldTp.StartTime - delta;
				newTp.EndTime = oldTp.EndTime - delta; //delta the same as for tp.StartTime

				Console.WriteLine("\nNew Date:");
				Console.WriteLine("newDate: " + newDate.ToString());
				Console.WriteLine("delta: " + delta.ToString());
				Console.WriteLine("DateExist: " + newTp.StartTime.ToString() + " " + newTp.EndTime.ToString());
			}

			return newTp;
		}

		private static TimePeriodDTO RecognizeTime(WhereCoworkerDTO userQuery, TimePeriodDTO oldTp) {
			TimePeriodDTO newTp = new TimePeriodDTO(oldTp);

			// if time period exist
			if(userQuery.TimePeriod != null && userQuery.TimePeriod.StartTime.HasValue) {
				newTp.StartTime = oldTp.StartTime?.Date.Add(userQuery.TimePeriod.StartTime.Value.TimeOfDay);
				newTp.EndTime = oldTp.EndTime?.Date.Add(userQuery.TimePeriod.EndTime.Value.TimeOfDay);
				Console.WriteLine("\nNew TimePeriod:");
				Console.WriteLine("DateTimePeriod: " + newTp.StartTime.ToString() + " " + newTp.EndTime.ToString());
			} else if(userQuery.Time.HasValue) { // if time exist 
				TimeSpan userQueryTime = userQuery.Time.Value.DateTime.TimeOfDay;
				newTp.StartTime = oldTp.StartTime?.Date.Add(userQueryTime).Add(TimeSpan.FromMinutes(-10));
				newTp.EndTime = oldTp.EndTime?.Date.Add(userQueryTime).Add(TimeSpan.FromMinutes(+10));
				Console.WriteLine("\nNew Time:");
				Console.WriteLine("DateTimePeriod: " + newTp.StartTime.ToString() + " " + newTp.EndTime.ToString());
			}

			return newTp;
		}

		private static TimePeriodDTO RecognizeDateTime(WhereCoworkerDTO userQuery, TimePeriodDTO oldTp) {
			TimePeriodDTO newTp = new TimePeriodDTO(oldTp);

			// if user input date
			if(userQuery.DateTimeObject != null && userQuery.DateTimeObject.DateTime.HasValue) {
				DateTimeOffset newDateTime = userQuery.DateTimeObject.DateTime.Value; //extract date and time
				
				newTp.StartTime = newDateTime.Add(TimeSpan.FromMinutes(-10));
				newTp.EndTime = newDateTime.Add(TimeSpan.FromMinutes(+10));

				Console.WriteLine("\nNew DateTime:");
				Console.WriteLine("newDateTime: " + newDateTime.ToString());
				Console.WriteLine("DateTimePeriod: " + newTp.StartTime.ToString() + " " + newTp.EndTime.ToString());
			}

			return newTp;
		}
	}
}