using CS_EventsNotifierSlackBot.WebSockets.DTO;

namespace CS_EventsNotifierSlackBot.WebSockets.Commands {

	public class RequestPushEvent: CommandBase {
		public static string Name { get => typeof(RequestPushEvent).Name; }

		public RequestPushEvent() : base(Name) { }

		public RequestPushEvent(EventDTO eventDTO) {
			Params = eventDTO;
		}
	}
}