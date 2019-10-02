using CS_EventsNotifierSlackBot.Global;
using CS_EventsNotifierSlackBot.RouteModules.Dialogflow.Commands;
using CS_EventsNotifierSlackBot.RouteModules.Dialogflow.DTO;
using CS_EventsNotifierSlackBot.WebSockets.Commands;
using CS_EventsNotifierSlackBot.WebSockets.DTO;
using Nancy;
using Nancy.Extensions;
using Nancy.IO;
using Newtonsoft.Json;
using SlackBotMessages;
using System;
using System.Linq;

namespace CS_EventsNotifierSlackBot.RouteModules.Dialogflow {

	public class IntentsModule: NancyModule {

		public IntentsModule() : base("") {
			base.Post("/intents", params_ => {
				try {
					// body -> json -> RequestWebHookIntent
					RequestWebHookIntent incomeWebHook = RequestWebHookIntent.FromJson(RequestStream.FromStream(Request.Body).AsString());
					IntentType.Type intentType = IntentType.GetType(incomeWebHook?.QueryResult?.Intent?.Name);
					object Parameters = incomeWebHook?.QueryResult?.Parameters;

					// Here is a temporary fix of bug in Dialogflow. 
					// Slack have changed api and today (12.09.2019) Dialogflow not correct works with unswers to slack
					if (intentType == IntentType.Type.Undefined || (incomeWebHook?.QueryResult?.AllRequiredParamsPresent ?? false) == false) { 
						return buildFulfillmentMessage(incomeWebHook);
					}

					switch (intentType) {
						case IntentType.Type.Undefined:
							break;

						case IntentType.Type.WhereCoworker:
							//return onWhereCoworker(WhereCoworkerDTO.FromObject(Parameters));
							onWhereCoworker(WhereCoworkerDTO.FromObject(Parameters));
							break;

						case IntentType.Type.WhenCoworker:
							//return onWhenCoworker(WhenCoworkerDTO.FromObject(Parameters));
							onWhenCoworker(WhenCoworkerDTO.FromObject(Parameters));
							break;
					}

					return buildFulfillmentMessage(incomeWebHook);

				} catch(Exception e) {
					Console.WriteLine("Exception: " + e.Message + "\n" + e.StackTrace);
				}

				return 404;
			});
		}

		private static string buildFulfillmentMessage(RequestWebHookIntent incomeWebHook) {
			string msg = string.Empty;
			incomeWebHook.QueryResult.FulfillmentMessages.ForEach(fulFlMsg => msg += fulFlMsg.Text.TextText.FirstOrDefault() + "\n");
			string slackWebHookUrl = Environment.GetEnvironmentVariable("SLACK_WEBHOOK_URL")
				?? throw new ArgumentNullException("slackWebHookUrl", "EnvironmentVariable 'SLACK_WEBHOOK_URL' doesn't set!");

			new SbmClient(slackWebHookUrl).Send(new SlackBotMessages.Models.Message(msg)).Wait();
			return msg;
		}

		private Response onWhenCoworker(WhenCoworkerDTO userQuery) {
			//Console.WriteLine(userQuery.ToJson(true));

			var tp = new TimePeriodDTO() {
				StartTime = DateTimeOffset.UtcNow.Date.Add(TimeSpan.Zero),
				EndTime = DateTimeOffset.UtcNow.Date.Add(new TimeSpan(23, 59, 59))
			};

			// if user hasn't entered any data and time -> set to current date and time
			if (! userQuery.Date.HasValue) {
				userQuery.Date = DateTimeOffset.UtcNow;
			}

			// recognize any possible time and data
			tp = RecognizeDate(date: userQuery.Date, oldTp: tp);

			// create params for Event Server request
			var holderRequest = new HolderLocationPeriodDTO() {
				QueryType = QueryType.GetType(userQuery.QueryType),
				HolderName = userQuery.TargetName.Replace("?", ""),
				HolderMiddlename = userQuery.TargetMiddlename.Replace("?", ""),
				HolderSurname = userQuery.TargetLastname.Replace("?", ""),
				TimePeriod = tp,
				IsHolderIn = userQuery.InOrOut == null ? new bool?() : 
								userQuery.InOrOut.Trim().Equals("пришла", StringComparison.OrdinalIgnoreCase) ? true : false
			};

			Console.WriteLine(holderRequest.ToJson(true));

			foreach(var eventsListener in GlobalScope.CSEventsListeners) {
				eventsListener.PostCommand(new RequestHolderLocation(holderRequest));
			}

			return 200;
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
				&& (! userQuery.Date.HasValue)
				&& userQuery.DateTimeObject == null 
				&& (! userQuery.Time.HasValue))
			{
				userQuery.DateTimeObject = new DateTimeWraper(DateTimeOffset.UtcNow);
			}

			// recognize any possible time and data
			tp = RecognizeDate(userQuery.Date, oldTp: tp);
			tp = RecognizeTime(userQuery, oldTp: tp);
			tp = RecognizeDateTime(userQuery, oldTp: tp);

			// create params for Event Server request
			var whereHolderRequest = new HolderLocationPeriodDTO() {
				QueryType = ((DateTime.UtcNow - tp.StartTime)?.TotalMinutes < 15) ? QueryType.Type.WhereNow : QueryType.Type.Where,
				HolderName = userQuery.TargetName.Replace("?", ""),
				HolderMiddlename = userQuery.TargetMiddlename.Replace("?", ""),
				HolderSurname = userQuery.TargetLastname.Replace("?", ""),
				TimePeriod = ((DateTime.UtcNow - tp.StartTime)?.TotalMinutes < 15) 
					? new TimePeriodDTO() { StartTime = DateTimeOffset.Now.Date.Add(TimeSpan.Zero), EndTime = DateTimeOffset.Now.Date.Add(new TimeSpan(23, 59, 59))} 
					: tp
			};

			Console.WriteLine(whereHolderRequest.ToJson(true));

			foreach(var eventsListener in GlobalScope.CSEventsListeners) {
				eventsListener.PostCommand(new RequestHolderLocation(whereHolderRequest));
			}

			// send response to Dialogflow (Dialogflow automatically resend it to Slack, Telegram, Skype, etc.)
			//return new ResponseWebHookIntent() {
			//	FulfillmentText = $"*Буду искать:* '{whereHolderRequest.HolderName} {whereHolderRequest.HolderMiddlename} {whereHolderRequest.HolderSurname}'\n" +
			//				      $"*c :* {whereHolderRequest.TimePeriod.StartTime.ToString()}\n" +
			//					  $"*по:* {whereHolderRequest.TimePeriod.EndTime.ToString()}"
			//}.ToJson();
			return 200;
		}

		private static TimePeriodDTO RecognizeDate(DateTimeOffset? date, TimePeriodDTO oldTp) {
			TimePeriodDTO newTp = new TimePeriodDTO(oldTp);

			// if user input date
			if(date.HasValue) {
				DateTimeOffset newDateTimeUtc = date.Value.UtcDateTime;
				DateTimeOffset newDate = newDateTimeUtc.Date.Add(TimeSpan.Zero); //extract date with zero time

				TimeSpan? deltaStart = oldTp.StartTime?.Date.Subtract(newDate.Date);
				TimeSpan? deltaEnd = oldTp.EndTime?.Date.Subtract(newDate.Date);

				newTp.StartTime = oldTp.StartTime.Value.DateTime - deltaStart;
				newTp.EndTime = oldTp.EndTime.Value.DateTime - deltaEnd; //delta the same as for tp.StartTime

				Console.WriteLine("\nNew Date:");
				Console.WriteLine("newDate: " + newDate.ToString());
				Console.WriteLine("delta: " + deltaStart.ToString() + "   " + deltaEnd.ToString());
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