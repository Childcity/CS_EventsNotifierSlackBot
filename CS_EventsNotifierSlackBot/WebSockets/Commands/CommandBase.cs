using Newtonsoft.Json;
using System;

namespace CS_EventsNotifierSlackBot.WebSockets.Commands {

	public class CommandBase {

		[JsonIgnore]
		private readonly string name;
		
		public string Command { get; set; }
		public Guid CommandId { get; set; }
		public DateTime TimeStamp { get; set; }

		public object Params { get; set; }

		public CommandBase(string name) {
			this.name = name;
			Command = GetType().Name;
			CommandId = Guid.NewGuid();
			TimeStamp = DateTime.Now;
		}

		public CommandBase() : this(typeof(CommandBase).Name) { }

		public override string ToString() {
			return name;
		}
	}
}