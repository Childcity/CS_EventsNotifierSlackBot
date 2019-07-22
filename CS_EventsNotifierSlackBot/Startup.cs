using CS_EventsNotifierSlackBot.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Nancy.Owin;
using System;

namespace CS_EventsNotifierSlackBot {

	public class Startup {

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services) {
		}
		
		public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
			if(env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			// enable static content
			app.UseStaticFiles();

			// setup WebSockets 
			app.UseWebSockets(new WebSocketOptions() {
				KeepAliveInterval = TimeSpan.FromSeconds(120),
				ReceiveBufferSize = 4 * 1024
			});
			app.UseWebSocketConnection();

			// use NancyFX by Owin
			app.UseOwin(x => x.UseNancy());
		}	
	}
}