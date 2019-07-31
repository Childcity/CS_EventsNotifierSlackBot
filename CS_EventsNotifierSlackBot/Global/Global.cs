using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Collections.Generic;

namespace CS_EventsNotifierSlackBot.Global {
	public static class GlobalScope {
		//public static MemoryStream LastImage { get; set; }
		public static Dictionary<decimal, MemoryStream> CachedImages { get; set; }
		static GlobalScope() {
			//LastImage = new MemoryStream();
			CachedImages = new Dictionary<decimal, MemoryStream>();
		}
	}
}