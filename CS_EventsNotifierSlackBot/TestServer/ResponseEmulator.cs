namespace TestServer {
	using CS_EventsNotifierSlackBot.RouteModules.Dialogflow.DTO;
	using CS_EventsNotifierSlackBot.WebSockets.Commands;
	using CS_EventsNotifierSlackBot.WebSockets.DTO;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net.WebSockets;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;

	public class ResponseEmulator {
		private ClientWebSocket webSocket;
		private CancellationToken token;
		private Uri clientUri;

		private static Dictionary<string, List<EventInfoDTO>> lastGeneratedEvents = new Dictionary<string, List<EventInfoDTO>>();

		public ResponseEmulator(Uri uri, CancellationToken cancellationToken) {
			webSocket = new ClientWebSocket();
			clientUri = uri;
			token = cancellationToken;
		}

		public async Task Listen() {
			Console.WriteLine("Emulator connecting to: " + clientUri);
			await webSocket.ConnectAsync(clientUri, token);
			Console.WriteLine("Emulator connected!");

			byte[] buffer = new byte[1024];
			WebSocketReceiveResult result = null;
			MemoryStream ms = null;

			try {
				ms = new MemoryStream();

				do {
					result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
					await ms.WriteAsync(buffer, 0, result.Count, token);

					// if total message received -> process it
					if (result.EndOfMessage && (!result.CloseStatus.HasValue)) {
						ms.Seek(0, SeekOrigin.Begin);

						if (result.MessageType == WebSocketMessageType.Text) {
							using (var reader = new StreamReader(ms, Encoding.UTF8)) {
								await processResult(await reader.ReadToEndAsync()); // start task without wait
							}
						}

						ms.Dispose();
						ms = new MemoryStream();
					}
				} while ((!result.CloseStatus.HasValue) && (!token.IsCancellationRequested));

				Console.WriteLine("Closing server emulator... ");

				await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, token);
			} catch (Exception e) {
				Console.WriteLine("Exception: " + e.Message + "\n" + e.StackTrace);
			} finally {
				ms?.Dispose();
				webSocket?.Dispose();
			}
		}

		private async Task processResult(string message) {
			if(CS_EventsNotifierSlackBot.Global.GlobalScope.CSEventsListeners.Count > 1)
				return;

			await Task.Delay(1000);

			try {
				Console.WriteLine("Emulator received: " + message);

				CommandBase command = CommandBase.FromJson(message);

				string cmdType = command.Command;

				if (cmdType == RequestHolderLocation.Name) {
					HolderLocationPeriodDTO locationPeriod = HolderLocationPeriodDTO.FromObject(command.Params);

					string keyToFindLast = locationPeriod.HolderName.Trim().ToLower() 
						+ locationPeriod.HolderMiddlename.Trim().ToLower() 
						+ locationPeriod.HolderSurname.Trim().ToLower() 
						+ locationPeriod.TimePeriod.EndTime?.Date;

					List<EventInfoDTO> eventsInfo;
					if (lastGeneratedEvents.ContainsKey(keyToFindLast)) {
						lastGeneratedEvents.TryGetValue(keyToFindLast, out eventsInfo);
						eventsInfo = eventsInfo.Where(ev => ev.EventTime >= locationPeriod.TimePeriod.StartTime && ev.EventTime <= locationPeriod.TimePeriod.EndTime).ToList();
					} else {
						eventsInfo = generateEvents(locationPeriod.TimePeriod);
						lastGeneratedEvents.Add(keyToFindLast, eventsInfo);
					}


					// Emulatig Response from Database
					var holderLocationDTO = new HolderLocationDTO() {
						QueryType = locationPeriod.QueryType,
						IsHolderIn = locationPeriod.IsHolderIn,
						TimePeriod = locationPeriod.TimePeriod,
						HolderInfo = new EventDTO() {
							CardNumber = 421449585,
							HolderType = "\u041F\u043E\u0441\u0435\u0442\u0438\u0442\u0435\u043B\u044C", //Посетитель
							HolderName = locationPeriod.HolderName,
							HolderMiddlename = locationPeriod.HolderMiddlename,
							HolderSurname = locationPeriod.HolderSurname,
							HolderDepartment = "\u041E\u0442\u0434\u0435\u043B \u043A\u0430\u0434\u0440\u043E\u0432", //Отдел кадров
							HolderTabNumber = "\u0422\u0430\u0431\u0435\u043B\u044C 845" //Табель 845
						},
						EventsInfo = eventsInfo
					};

					var response = new ResponseHolderLocation(holderLocationDTO);

					byte[] buffer = Encoding.UTF8.GetBytes(response.ToJson());
					await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, token);
				}
			} catch (Exception e) {
				Console.WriteLine(e.ToString());
			}
		}

		private static List<EventInfoDTO> generateEvents(TimePeriodDTO timePeriod) {
			Random rnd = new Random((int)DateTime.Now.ToFileTime());
			int capacity = rnd.Next(3, 7);
			var eventsInfos = new List<EventInfoDTO>(capacity);

			var endTime = timePeriod.EndTime;
			var timeDelta = (timePeriod.EndTime - timePeriod.StartTime) / (capacity + 2);

			for (int i = 0; i < capacity; i++) {
				eventsInfos.Add(new EventInfoDTO() {
					// if it first event => should be 'Выход' if it last event => should be 'Вход' else random 
					//Direction = (((rnd.Next() % 2) == 0 || i == 0) && (i != (capacity - 1))) ? new byte?(1) : new byte?(0),
					Direction = (rnd.Next() % 2) == 0 ? new byte?(1) : new byte?(0),
					EventCode = 105,
					EventTime = (endTime -= timeDelta + TimeSpan.FromMinutes(rnd.Next(59)))?.DateTime,

					StartAreaName = ((rnd.Next() % 2) == 0 || i == 0) ? "\u0423\u043B\u0438\u0446\u0430" : "\u041E\u0444\u0438\u0441", //улица/офис
					TargetAreaName = ((rnd.Next() % 2) == 0 || i == (capacity - 1)) ? "\u0423\u043B\u0438\u0446\u0430" : "\u041E\u0444\u0438\u0441",

					ObjectType = "\u0414\u0432\u0435\u0440\u0438 \u0441 \u0434\u0432\u043E\u0439\u043D\u044B\u043C \u0437\u0430\u043C\u043A\u043E\u043C", //Двери с двойным замком
					ObjectName = "\u0414\u0432\u0435\u0440\u0438 #" + rnd.Next(10) //Двери #
				});
			}

			return eventsInfos;
		}
	}
}