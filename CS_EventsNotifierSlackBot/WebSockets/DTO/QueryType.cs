﻿using System;

namespace CS_EventsNotifierSlackBot.WebSockets.DTO {

	public static class QueryType {

		public enum Type {
			Empty,
			InWhatTime,
			Where,
			WhereNow,
			When,
			WhatPlace,
			HowLong
		}

		public static Type GetType(string queryType) {
			if(queryType != null && queryType.Length > 2) {
				switch(queryType.Replace("?", "").Trim().ToLower()) {
					case "во сколько":
						return Type.InWhatTime;
					case "где":
						return Type.Where;
					case "где сейчас":
						return Type.WhereNow;
					case "когда":
						return Type.When;
					case "куда":
						return Type.WhatPlace;
					case "сколько":
						return Type.HowLong;
				}
			}

			return Type.Empty;
		}
	}
}