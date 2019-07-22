using System;

namespace CS_EventsNotifierSlackBot.WebSockets.DTO {
	public class EventDTO {
		public decimal EventNumber { get; set; }
		
		public int EventCode { get; set; }
		
		public int GeneratorID { get; set; }
		
		public int InitiatorID { get; set; }
		
		public int? AccountID { get; set; }
		
		public int? HolderID { get; set; }
		
		public int? ControlPointID { get; set; }
		
		public int? StartZoneID { get; set; }
		
		public int? TargetZoneID { get; set; }
		
		public byte? Direction { get; set; }
		
		public decimal? CardNumber { get; set; }
		
		public int? GroupID { get; set; }
		
		public DateTime EventTime { get; set; }
	}
}
