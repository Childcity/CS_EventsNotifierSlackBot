using CS_EventsNotifierSlackBot.WebSockets.DTO;

namespace CS_EventsNotifierSlackBot.WebSockets.Commands {

	public class RequestHolderLocation: CommandBase {
		public static string Name { get => typeof(RequestHolderLocation).Name; }

		public RequestHolderLocation() : base(Name) { }

		public RequestHolderLocation(HolderLocationPeriodDTO holderLocationPeriod) {
			Params = holderLocationPeriod;
		}
	}
}