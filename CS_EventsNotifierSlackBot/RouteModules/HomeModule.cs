using CS_EventsNotifierSlackBot.Global;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace CS_EventsNotifierSlackBot.RouteModules {
	public class HomeModule: NancyModule {
		public HomeModule(): base(""){
			Get("/", args => {
				return View["wwwroot/Home.html"];
			});

			Get("/image/{id:decimal}", params_ => {
				var imageStream = new MemoryStream();

				GlobalScope.CachedImages[params_.id].Seek(0, SeekOrigin.Begin);
				GlobalScope.CachedImages[params_.id].CopyTo(imageStream);

				imageStream.Seek(0, SeekOrigin.Begin);
				return Response.FromStream(imageStream, "image/jpg");

			}, ctx => {
				string idStr = ctx.Request.Url.Path.Split("/").LastOrDefault();
				return decimal.TryParse(idStr, out decimal id) && GlobalScope.CachedImages.ContainsKey(id);
			});
		}
	}
}
