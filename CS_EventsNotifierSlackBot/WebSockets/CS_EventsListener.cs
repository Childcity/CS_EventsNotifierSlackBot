using CS_EventsNotifierSlackBot.Global;
using CS_EventsNotifierSlackBot.WebSockets.Commands;
using CS_EventsNotifierSlackBot.WebSockets.DTO;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SlackBotMessages;
using SlackBotMessages.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CS_EventsNotifierSlackBot.WebSockets {

	public class CS_EventsListener {
		private readonly HttpContext wsContext;
		private readonly WebSocket webSocket;
		private readonly SbmClient slackClient;
		private readonly CancellationTokenSource tokenSource;
		private BlockingCollection<CommandBase> messageQueue;

		public CS_EventsListener(HttpContext context, WebSocket socket) {
			webSocket = socket;
			wsContext = context;

			string slackWebHookUrl = Environment.GetEnvironmentVariable("SLACK_WEBHOOK_URL")
				?? throw new ArgumentNullException("slackWebHookUrl", "EnvironmentVariable 'SLACK_WEBHOOK_URL' doesn't set!");

			slackClient = new SbmClient(slackWebHookUrl);
			messageQueue = new BlockingCollection<CommandBase>();
			tokenSource = new CancellationTokenSource();
		}

		public void PostCommand(CommandBase command) {
			messageQueue.Add(command, tokenSource.Token);
		}

		public async Task Listen() {
			byte[] buffer = new byte[1024];
			WebSocketReceiveResult result = null;
			MemoryStream ms = null;

			Task commandSenderTask = Task.Run(async () => await commandSender(tokenSource.Token));

			try {
				ms = new MemoryStream();

				do {
					result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), tokenSource.Token);
					await ms.WriteAsync(buffer, 0, result.Count, tokenSource.Token);

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
				} while (!result.CloseStatus.HasValue);

				Console.WriteLine("Closing... ");

				await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, tokenSource.Token);

				// will cancel commandSenderTask
				tokenSource.Cancel();

				await commandSenderTask;
			} catch (Exception e) {
				Console.WriteLine("Exception: " + e.Message + "\n" + e.StackTrace);
			} finally {
				ms?.Dispose();
				messageQueue?.Dispose();
				webSocket?.Abort();
				tokenSource?.Dispose();
			}
		}

		private async Task commandSender(CancellationToken cancellationToken) {
			foreach (CommandBase item in messageQueue.GetConsumingEnumerable(cancellationToken)) {
				if (cancellationToken.IsCancellationRequested) {
					return;
				}

				byte[] buffer = Encoding.UTF8.GetBytes(item.ToJson());
				await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancellationToken);
			}
		}

		private async Task processResult(string message) {
			try {
				//Console.WriteLine(message);

				CommandBase command = CommandBase.FromJson(message);

				string cmdType = command.Command;

				if (cmdType == RequestPushEvent.Name) {
					await onPushEvent(EventDTO.FromObject(command.Params));
				} else if (cmdType == ResponseHolderLocation.Name) {
					await onResponseWhereCoworker(HolderLocationDTO.FromObject(command.Params));
				}
			} catch (Exception e) {
				Console.WriteLine(e.ToString());
			}
		}

		private async Task onResponseWhereCoworker(HolderLocationDTO holderLocation) {
			Console.WriteLine(holderLocation.ToJson(true));

			if (holderLocation?.HolderInfo?.HolderName == null) {
				await sendResponse($"Не нашел событий с указанным сотрудником. " +
					$"Возможно его небыло {holderLocation?.TimePeriod?.EndTime?.Date.ToString("dd MMMM yyyy", CultureInfo.CreateSpecificCulture("ru-RU"))}");
				return;
			}


			string holderNameStr = $"{holderLocation.HolderInfo.HolderSurname ?? ""} {holderLocation.HolderInfo.HolderName ?? ""} {holderLocation.HolderInfo.HolderMiddlename ?? ""}";

			var queryType = holderLocation.QueryType;

			if (queryType == QueryType.Type.Where) { // юзер хочет знать где был(а) сотрудник
				await sendResponse($"*Сотрудник* {holderNameStr}\n" +
						   $"{buildEventsInfo(holderLocation.EventsInfo)}");
				return;
			}

			//
			// юзер хочет узнать зашёл/вышел сотрудник
			//

			Debug.Assert(holderLocation.IsHolderIn.HasValue); // should be always true here!

			// найдем событие входа/выхода на/из територию
			var foundedEventInf = findInOrOutEvent(holderLocation.EventsInfo);
			string eventObjNameInStr = $"{foundedEventInf?[0]?.ObjectName ?? "\"Контрольная точка не задана\""}";
			string eventObjNameOutStr = $"{foundedEventInf?[0]?.ObjectName ?? "\"Контрольная точка не задана\""}";

			// юзер хочет узнать сколько по времени был(а) сотрудник
			if (queryType == QueryType.Type.HowLong) {
				if (foundedEventInf?[0] != null) {
					var inEventTime = foundedEventInf[0]?.EventTime;
					var outEventTime = foundedEventInf[1]?.EventTime;

					if(! outEventTime.HasValue) { // последнее событие не выход на Улицу
						try { // попытка найти любое последнее событие прохода
							outEventTime = holderLocation.EventsInfo.First(evInf => evInf.EventCode == 105).EventTime;
						} catch (Exception) { }
					}

					await sendResponse($"{holderNameStr}\n" +
								$"был(а) на территории {(outEventTime - inEventTime)?.ToString(@"%h'ч. '%m'мин.'")}");
				} else {
					await sendResponse($"Сотрудника \n{holderNameStr}\n" +
								$"*не было*");
				}

				return;
			}

			string eventInTimeStr = $"{foundedEventInf?[0]?.EventTime?.ToString("T", CultureInfo.CreateSpecificCulture("ru-RU"))}";
			string eventOutTimeStr = $"{foundedEventInf?[1]?.EventTime?.ToString("T", CultureInfo.CreateSpecificCulture("ru-RU"))}";

			// зашёл?
			if (holderLocation.IsHolderIn.Value) {
				if (queryType == QueryType.Type.Empty) { // юзер хочет ответ (Да, зашёл)/(Нет, не зашёл)
					if (foundedEventInf?[0] != null) {
						await sendResponse($"Да, {holderNameStr}\n" +
									$"*зашёл* в {eventInTimeStr} через {eventObjNameInStr}\n" +
									((eventOutTimeStr == "") ? getNotFoundLastEventMessage(holderLocation, holderNameStr) :
										$"*вышел* в {eventOutTimeStr} через {eventOutTimeStr}\n"));
					} else {
						await sendResponse($"Нет, {holderNameStr}\n" +
									$"*не было*");
					}
				} else { // юзер хочет узнать (Во сколько)/(когда)/(в котором часу) сотрудник зашёл
					if (foundedEventInf?[0] != null) {
						await sendResponse($"{holderNameStr}\n" +
									$"*зашёл* в {eventInTimeStr} через {eventObjNameInStr}\n");
					} else {
						await sendResponse($"Сотрудника \n{holderNameStr}\n" +
									$"*не было*");
					}
				}

			} else { // вышел?

				if (foundedEventInf?[1] == null) {

					await sendResponse(getNotFoundLastEventMessage(holderLocation, holderNameStr));

				} else {
					if (queryType == QueryType.Type.Empty) {
						await sendResponse($"Да, {holderNameStr}\n" +
								   $"*вышел* в {eventOutTimeStr} через {eventObjNameOutStr}\n");
					} else { // юзер хочет узнать Во сколько/когда сотрудник вышел
						await sendResponse($"{holderNameStr}\n" +
									$"*вышел* в {eventOutTimeStr} через {eventObjNameOutStr}");
					}
				}
			}
		}

		private async Task onPushEvent(EventDTO eventDTO) {
			string cardNamber = string.Empty;

			if (GlobalScope.CachedImages.Count > 500) {
				foreach (var item in GlobalScope.CachedImages) {
					item.Value.Dispose();
				}
				GlobalScope.CachedImages.Clear();
			}

			if (eventDTO.CardNumber.HasValue) {
				if (!GlobalScope.CachedImages.ContainsKey(eventDTO.CardNumber.Value)) {
					if (eventDTO?.HolderPhoto != null) {
						using (var image = Image.Load<Rgb24>(eventDTO.HolderPhoto)) {
							prepareImage(image);
							GlobalScope.CachedImages[eventDTO.CardNumber.Value] = new MemoryStream();
							image.SaveAsPng(GlobalScope.CachedImages[eventDTO.CardNumber.Value]);

							Console.WriteLine("Cached image Length (bytes): " + GlobalScope.CachedImages[eventDTO.CardNumber.Value].Length);
							Console.WriteLine("GlobalScope.CachedImages.Count: " + GlobalScope.CachedImages.Count);
						}
					}
				}

				// remove '.0' from the end of eventDTO.CardNumber
				cardNamber = eventDTO.CardNumber.Value.ToString().Split(new char[] { ',', '.' })[0];
			}

			string imagePath = $"{wsContext.Request.Scheme}://{wsContext.Request.Host}/image/{cardNamber}";
			//imagePath = $"http://307137cf.ngrok.io/image/{cardNamber}";

			Console.WriteLine(imagePath);

			await sendResponse(new Message() {
				Text = $"*Сотрудник*\n{eventDTO?.HolderSurname ?? ""} {eventDTO?.HolderName ?? ""} {eventDTO?.HolderMiddlename ?? ""}\n\n" +
						$"*{eventDTO?.ObjectName ?? "Контрольная точка не задана"}*\n" +
						$"{eventDTO?.EventTime?.DateTime.ToString("T", CultureInfo.CreateSpecificCulture("ru-RU"))} | " +
						$"{((eventDTO?.Direction ?? 0) == 0 ? "Вход" : "Выход")} | " +
						$"Осуществление прохода по пропуску",
				Attachments = new List<Attachment> {
						new Attachment {
							Fallback = $"Enable Attachment in settings for more info!",
							ImageUrl = imagePath,
							Color = "#4081F5"
						}
					}
			});
		}

		private async Task sendResponse(string msgStr) => await sendResponse(new Message(msgStr));

		private async Task sendResponse(Message message) {
			Console.WriteLine(message.Text);
			await slackClient.Send(message);
		}

		private static string getNotFoundLastEventMessage(HolderLocationDTO holderLocation, string holderNameStr) {
			string msg = $"Не нашел события *выхода* из территории";

			try { // попытка найти любое последнее событие прохода
				var lastEvent = holderLocation.EventsInfo.First(evInf => evInf.EventCode == 105);
				msg += $"\nПоследнее событие прохода:\n{holderNameStr}\n" +
					   $"*{((lastEvent.Direction ?? 0) == 0 ? "Вход" : "Выход")} " +
					   $"с территории '{lastEvent.StartAreaName}' на территорию '{lastEvent.TargetAreaName}' " +
					   $"через {lastEvent.ObjectName ?? "\"Контрольная точка не задана\""}*\n" +
					   $"в {lastEvent.EventTime?.ToString("T", CultureInfo.CreateSpecificCulture("ru-RU"))}";
			} catch (Exception) { }

			return msg;
		}

		private static EventInfoDTO[] findInOrOutEvent(List<EventInfoDTO> events) {
			EventInfoDTO[] eventInfo = null;

			if (events?.Count > 0) {
				eventInfo = new EventInfoDTO[2];

				eventInfo[0] = events.LastOrDefault(evInf => evInf.EventCode == 105); // Holder In
				eventInfo[1] = events.FirstOrDefault(//evInf => evInf.EventCode == 105
													//		&& evInf.TargetAreaName.Trim().ToLower().Equals("улица")
															); // Holder Out

				string outTargetName = "" + eventInfo[1]?.TargetAreaName?.Trim()?.ToLower();
				if (! outTargetName.Equals("улица")) {
					eventInfo[1] = null;
				}
			}

			return eventInfo;
		}

		private static string buildEventsInfo(List<EventInfoDTO> events) {
			string info = string.Empty;

			int eventNumber = 1;
			foreach (var eventDTO in events) {
				info += $"*{eventNumber++}. {eventDTO.ObjectName ?? "Контрольная точка не задана"} *\n" +
						$"{eventDTO.EventTime?.ToString("T", CultureInfo.CreateSpecificCulture("ru-RU"))} | " +
						$"{((eventDTO.Direction ?? 0) == 0 ? "Вход" : "Выход")} | " +
						$"Осуществление прохода по пропуску\n";
			}

			return info;
		}

		private static void prepareImage(Image<Rgb24> image) {
			int heightBeforeEntropyCrop = image.Height;
			image.Mutate(im => im.EntropyCrop());
			int heightAfterEntropyCrop = image.Height;

			if (image.Height > 250) {
				image.Mutate(im => im.Resize(250 * image.Width / image.Height, 250));

				bool isHeightCroped = MathF.Abs(heightAfterEntropyCrop - heightBeforeEntropyCrop) > 5;

				// if only image.Height > 250 -> Crop only image.Height
				int xStartCropPoint = 0;
				int newImageWidth = image.Width;
				int newImageHeight = isHeightCroped ? image.Height - 10 : image.Height;

				if (image.Width > 250) {
					xStartCropPoint = 10;
					newImageWidth = image.Width - 10 - 10;
				}

				image.Mutate(im => im.Crop(new Rectangle(xStartCropPoint, 0, newImageWidth, newImageHeight)));
			}
		}
	}
}