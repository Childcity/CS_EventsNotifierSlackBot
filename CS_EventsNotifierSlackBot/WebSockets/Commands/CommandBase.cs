using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_EventsNotifierSlackBot.WebSockets.Commands {
	public class CommandBase {
		public string Command { get; set; }
		public Guid CommandId { get; set; }
		public DateTime TimeStamp { get; set; }

		public object Params { get; set; }

		public CommandBase() {
			Command = GetType().Name;
			CommandId = Guid.NewGuid();
			TimeStamp = DateTime.Now;
		}
	}
}
