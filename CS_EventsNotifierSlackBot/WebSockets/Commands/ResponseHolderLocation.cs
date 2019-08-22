using CS_EventsNotifierSlackBot.WebSockets.DTO;

namespace CS_EventsNotifierSlackBot.WebSockets.Commands {

	public class ResponseHolderLocation: CommandBase {
		public static string Name { get => typeof(ResponseHolderLocation).Name; }

		public ResponseHolderLocation() : base(Name) {}

		public ResponseHolderLocation(HolderLocationDTO coworkerLocation) {
			Params = coworkerLocation;
		}
	}
}