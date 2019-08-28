using System;

namespace CS_EventsNotifierSlackBot.WebSockets.DTO {

	public static class QueryType {

		public enum Type {
			Empty,
			InWhatTime,
			Where,
			When,
			WhatPlace
		}

		public static Type GetType(string queryType) {
			if(queryType != null && queryType.Length > 2) {
				switch(queryType.Trim().ToLower()) {
					case "во сколько":
						return Type.InWhatTime;
					case "где":
						return Type.Where;
					case "когда":
						return Type.When;
					case "куда":
						return Type.WhatPlace;
				}
			}

			return Type.Empty;
		}
	}
}