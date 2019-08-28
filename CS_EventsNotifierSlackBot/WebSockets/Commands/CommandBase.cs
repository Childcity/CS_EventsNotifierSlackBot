using CS_EventsNotifierSlackBot.Global;
using Newtonsoft.Json;
using System;

namespace CS_EventsNotifierSlackBot.WebSockets.Commands {

	public partial class CommandBase {

		[JsonIgnore]
		private readonly string name;
		
		public string Command { get; set; }

		public Guid CommandId { get; set; }

		public DateTimeOffset TimeStamp { get; set; }

		// Field contain parameters from each command
		public object Params { get; set; }

		public CommandBase(string name) {
			this.name = name;
			Command = GetType().Name;
			CommandId = Guid.NewGuid();
			TimeStamp = DateTimeOffset.Now;
		}

		public CommandBase() : this(typeof(CommandBase).Name) { }

		public override string ToString() {
			return name;
		}
	}

	public partial class CommandBase {
		
		public static CommandBase FromJson(string json) => JsonConvert.DeserializeObject<CommandBase>(json, JsonConverterSettings.Settings);
	}

	public static class SerializeCommandBase {

		public static string ToJson(this CommandBase self, bool indented = false) => JsonConvert.SerializeObject(self, indented ? Formatting.Indented : Formatting.None, JsonConverterSettings.Settings);
	}
}