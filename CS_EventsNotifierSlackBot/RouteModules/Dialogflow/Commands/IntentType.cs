namespace CS_EventsNotifierSlackBot.RouteModules.Dialogflow.Commands {

	public static class IntentType {
		public static readonly string WhereCoworker = "abeee50f-81ed-44d5-9461-4ebd500a9e29";
		public static readonly string WhenCoworker = "de815e64-7e83-4960-a529-35ce82177b7d";

		public enum Type {
			Undefined,
			WhereCoworker,
			WhenCoworker
		}

		public static Type GetType(string intentName) {
			if(intentName != null) {
				if(intentName.Contains(WhereCoworker))
					return Type.WhereCoworker;
				else if(intentName.Contains(WhenCoworker))
					return Type.WhenCoworker;
			}

			return Type.Undefined;
		}
	}
}