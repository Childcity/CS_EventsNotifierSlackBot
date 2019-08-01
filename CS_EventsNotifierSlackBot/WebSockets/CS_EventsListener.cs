using CS_EventsNotifierSlackBot.WebSockets.Commands;
using CS_EventsNotifierSlackBot.WebSockets.DTO;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SlackBotMessages;
using SlackBotMessages.Models;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using CS_EventsNotifierSlackBot.Global;
using System.Collections.Generic;
using System.Globalization;

namespace CS_EventsNotifierSlackBot.WebSockets {

	public class CS_EventsListener {
		private readonly SbmClient slackClient;
		public readonly HttpContext wsContext;

		public WebSocket WebSocket { get; set; }

		public CS_EventsListener(HttpContext context, WebSocket webSocket) {
			WebSocket = webSocket;
			wsContext = context;

			string slackWebHookUrl = Environment.GetEnvironmentVariable("SLACK_WEBHOOK_URL")
				?? throw new ArgumentNullException("slackWebHookUrl", "EnvironmentVariable 'SLACK_WEBHOOK_URL' doesn't set!");

			slackClient = new SbmClient(slackWebHookUrl);
		}

		public async Task Listen() {

			byte[] buffer = new byte[1024];
			WebSocketReceiveResult result = null;

			MemoryStream ms = null;
			try {
				ms = new MemoryStream();

				do {
					result = await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
					await ms.WriteAsync(buffer, 0, result.Count, CancellationToken.None);

					// if total message received -> process it
					if(result.EndOfMessage && (! result.CloseStatus.HasValue)) {
						ms.Seek(0, SeekOrigin.Begin);

						if(result.MessageType == WebSocketMessageType.Text) {
							using(var reader = new StreamReader(ms, Encoding.UTF8)) {
								processResult(await reader.ReadToEndAsync()); // start task without wait
							}
						}

						ms.Dispose();
						ms = new MemoryStream();
					}
				} while(! result.CloseStatus.HasValue);

				Console.WriteLine("Closing... ");

				await WebSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
			} catch(Exception e) {
				Console.WriteLine("Exception: " + e.Message + "\n" + e.StackTrace);
			} finally {
				ms?.Dispose();
			}
		}

		private async Task processResult(string message) {
			try {
				//Console.WriteLine(message);

				CommandBase command = JsonConvert.DeserializeObject<CommandBase>(message);

				switch(command.Command) {
					case "RequestPushEvent": {
							EventDTO eventDTO = JsonConvert.DeserializeObject<EventDTO>(
								JsonConvert.SerializeObject(command.Params, Formatting.Indented));
							await onPushEvent(eventDTO);
							break;
						}
						//default:
						//	{
						//      Encoding.UTF8.GetBytes("{Command: \"" + { command.Command}+"\"}").CopyTo(buffer, 0);
						//		await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
						//		break;
						//	}
				}
			} catch(Exception e) {
				Console.WriteLine(e.ToString());
			}
		}

		private async Task onPushEvent(EventDTO eventDTO) {
			string cardNamber = string.Empty;

			if(GlobalScope.CachedImages.Count > 500) {
				foreach(var item in GlobalScope.CachedImages) {
					item.Value.Dispose();
				}
				GlobalScope.CachedImages.Clear();
			}

			if(eventDTO.CardNumber.HasValue) {
				if(! GlobalScope.CachedImages.ContainsKey(eventDTO.CardNumber.Value)) {
					using(var image = Image.Load<Rgb24>(eventDTO?.HolderPhoto)) {
						if(image.Height > 250 && image.Width > 250) {
							image.Mutate(im => im.Resize(250 * image.Width / image.Height, 250).Crop(image.Width - 30, image.Height - 30));
						}else if(image.Height > 250) {
							// if only image.Height > 250 -> Crop only image.Height
							image.Mutate(im => im.Resize(250 * image.Width / image.Height, 250).Crop(image.Width, image.Height - 30));
						}

						GlobalScope.CachedImages[eventDTO.CardNumber.Value] = new MemoryStream();
						image.SaveAsPng(GlobalScope.CachedImages[eventDTO.CardNumber.Value]);
					}
				}

				// remove '.0' from the end of eventDTO.CardNumber
				cardNamber = eventDTO.CardNumber.Value.ToString().Split(new char[] { ',', '.' })[0];
			}

			string imagePath = $"{wsContext.Request.Scheme}://{wsContext.Request.Host}/image/{cardNamber}";
			//imagePath = $"http://0cec647b.ngrok.io/image/{cardNamber}";

			Console.WriteLine("Cached image Length (bytes): " + GlobalScope.CachedImages[eventDTO.CardNumber.Value].Length);
			Console.WriteLine("GlobalScope.CachedImages.Count: " + GlobalScope.CachedImages.Count);
			Console.WriteLine(imagePath);

			var message = new Message() {
				Text = $"*Сотрудник*\n{eventDTO.HolderSurname ?? ""} {eventDTO.HolderName ?? ""} {eventDTO.HolderMiddlename ?? ""}\n\n" +
						$"*{eventDTO.ObjectName ?? "Контрольная точка не задана"}*\n" +
						$"{eventDTO.EventTime?.ToString("T", CultureInfo.CreateSpecificCulture("ru-RU"))} | " +
						$"{((eventDTO.Direction ?? 0) == 0 ? "Вход" : "Выход")} | " +
						$"Осуществление прохода по пропуску",
				Attachments = new List<Attachment> {
						new Attachment {
							Fallback = $"Enable Attachment in settings for more info!",
							ImageUrl = imagePath,
							Color = "#4081F5"
						}
					}
			};

			var resp = await slackClient.Send(message);
		}
	}
}