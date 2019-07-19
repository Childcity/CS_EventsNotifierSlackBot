using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CS_EventsNotifierSlackBot.IntegrationApi {
	public class HomeModule: NancyModule {
		public HomeModule(): base(""){
			Get("/", args => {
				return View["wwwroot/Home.html"];
			});
		}
	}
}
