using CS_EventsNotifierSlackBot.WebSockets.Commands;
using CS_EventsNotifierSlackBot.WebSockets.DTO;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SlackBotMessages;
using SlackBotMessages.Models;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CS_EventsNotifierSlackBot.WebSockets {

	public class CS_EventsListener {
		private readonly byte[] buffer;
		private readonly SbmClient slackClient;

		public WebSocket WebSocket { get; set; }

		public CS_EventsListener() {
			buffer = new byte[1024 * 4];

			string slackWebHookUrl = Environment.GetEnvironmentVariable("SLACK_WEBHOOK_URL")
				?? throw new ArgumentNullException("slackWebHookUrl", "EnvironmentVariable 'SLACK_WEBHOOK_URL' doesn't set!");

			slackClient = new SbmClient(slackWebHookUrl);
		}

		public async Task Listen(HttpContext context, WebSocket webSocket) {
			WebSocket = webSocket;
			WebSocketReceiveResult result;

			do {
				result = await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
				await processResult(result);
			} while(! result.CloseStatus.HasValue);

			await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
		}

		private async Task processResult(WebSocketReceiveResult result) {
			try {
				if(!result.EndOfMessage)
					Console.WriteLine("WARN: Message wasn't receive completely!");

				string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
				Console.WriteLine(message);

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
			var message = new Message() {
				Text = JsonConvert.SerializeObject(eventDTO, Formatting.Indented)
			};

			Console.WriteLine("Slack : \n"+ message);
			var resp = await slackClient.Send(message);
			Console.WriteLine("Slack resp:\n" + resp);
		}
	}
}