using System;

namespace CS_EventsNotifierSlackBot.WebSockets.DTO {

	public class EventDTO {

		public int? EventCode { get; set; }

		public byte? Direction { get; set; }

		public decimal? CardNumber { get; set; }

		public DateTime? EventTime { get; set; }

		//

		public string AccountNumber { get; set; }

		//

		public string StartAreaName { get; set; }

		public string TargetAreaName { get; set; }

		//

		public string ObjectType { get; set; }

		public string ObjectName { get; set; }

		//

		public string HolderType { get; set; }

		public string HolderSurname { get; set; }

		public string HolderName { get; set; }

		public string HolderMiddlename { get; set; }

		public string HolderDepartment { get; set; }

		public string HolderTabNumber { get; set; }

		public byte[] HolderPhoto { get; set; }

	}
}