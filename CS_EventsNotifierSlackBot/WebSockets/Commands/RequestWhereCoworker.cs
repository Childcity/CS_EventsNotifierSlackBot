using CS_EventsNotifierSlackBot.WebSockets.DTO;

namespace CS_EventsNotifierSlackBot.WebSockets.Commands {

	public class RequestWhereCoworker: CommandBase {
		public static string Name { get => typeof(RequestWhereCoworker).Name; }

		public RequestWhereCoworker() : base(Name) { }

		public RequestWhereCoworker(HolderLocationPeriodDTO holderLocationPeriod) {
			Params = holderLocationPeriod;
		}
	}
}