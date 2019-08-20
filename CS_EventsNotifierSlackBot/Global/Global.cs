using System.IO;
using System.Collections.Generic;
using CS_EventsNotifierSlackBot.WebSockets;

namespace CS_EventsNotifierSlackBot.Global {
	public static class GlobalScope {
		public static Dictionary<decimal, MemoryStream> CachedImages { get; set; }
		public static List<CS_EventsListener> CSEventsListeners;

		static GlobalScope() {
			CachedImages = new Dictionary<decimal, MemoryStream>();
			CSEventsListeners = new List<CS_EventsListener>();
		}
	}
}