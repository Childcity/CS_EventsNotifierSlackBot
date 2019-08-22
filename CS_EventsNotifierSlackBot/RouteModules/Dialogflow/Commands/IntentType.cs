namespace CS_EventsNotifierSlackBot.RouteModules.Dialogflow.Commands {

	public static class IntentType {
		public static readonly string WhereCoworker = "abeee50f-81ed-44d5-9461-4ebd500a9e29";

		public enum Type {
			Undefined,
			WhereCoworker
		}

		public static Type GetType(string intentName) {
			if(intentName != null) {
				if(intentName.Contains(WhereCoworker))
					return Type.WhereCoworker;
			}

			return Type.Undefined;
		}
	}
}