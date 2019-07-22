using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace CS_EventsNotifierSlackBot.WebSockets {

	// extension method exposes the middleware through IApplicationBuilder
	public static class WebSocketConnectionMiddlewareExtensions {
		public static IApplicationBuilder UseWebSocketConnection(this IApplicationBuilder builder) {
			return builder.UseMiddleware<WebSocketConnectionMiddleware>();
		}
	}

	public class WebSocketConnectionMiddleware {
		private readonly RequestDelegate next;
		private CS_EventsListener csEventsListener;

		public WebSocketConnectionMiddleware(RequestDelegate next) {
			this.next = next;
		}

		public async Task InvokeAsync(HttpContext context) {
			if(context.Request.Path != "/ws") {
				// Call the next delegate/middleware in the pipeline
				await next(context);
				return;
			}

			if(! context.WebSockets.IsWebSocketRequest) {
				context.Response.StatusCode = 400;	
				return;
			}

			Console.WriteLine("/ws");

			if(csEventsListener != null) {
				csEventsListener.WebSocket.Abort();
				csEventsListener = null;
			}

			Console.WriteLine("/ws connected");
			WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();


			csEventsListener = new CS_EventsListener();
			await csEventsListener.Listen(context, webSocket);
		}
	}
}
