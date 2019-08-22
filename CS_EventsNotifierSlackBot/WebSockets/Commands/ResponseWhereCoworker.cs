using CS_EventsNotifierSlackBot.WebSockets.DTO;

namespace CS_EventsNotifierSlackBot.WebSockets.Commands {

	public class ResponseWhereCoworker: CommandBase {
		public static string Name { get => typeof(ResponseWhereCoworker).Name; }

		public ResponseWhereCoworker() : base(Name) {}

		public ResponseWhereCoworker(HolderLocationDTO coworkerLocation) {
			Params = coworkerLocation;
		}
	}
}