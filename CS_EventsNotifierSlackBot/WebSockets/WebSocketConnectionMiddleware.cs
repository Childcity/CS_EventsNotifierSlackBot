using CS_EventsNotifierSlackBot.Global;
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
			WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
			Console.WriteLine("/ws connected");


			CS_EventsListener csEventsListener = null;
			try {
				csEventsListener = new CS_EventsListener(context, webSocket);
				GlobalScope.CSEventsListeners.Add(csEventsListener);
				await csEventsListener.Listen();
			} finally {
				if(csEventsListener != null) {
					GlobalScope.CSEventsListeners.Remove(csEventsListener);
				}
			}
		}
	}
}
