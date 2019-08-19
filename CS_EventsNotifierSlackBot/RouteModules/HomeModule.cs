using Nancy;

namespace CS_EventsNotifierSlackBot.RouteModules {
	public class HomeModule: NancyModule {
		public HomeModule(): base(""){
			Get("/", params_ => {
				return View["wwwroot/Home.html"];
			});
		}
	}
}
