using Nancy;
using SlackBotMessages;
using SlackBotMessages.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CS_EventsNotifierSlackBot.RouteModules.SlashCommands {
	public class AboutModul: NancyModule {
		public AboutModul(): base("/slash-cmd") {
			Post("/about", _ => {
				var message = new Message {
					Text = "Hello, I can notify you about new events, that ocure on StopNet4 system.",
					Attachments = new List<Attachment> {
						new Attachment {
							Fallback = "This is for slack clients which choose not to display the attachment.",
							Pretext = "Below you can see my commands:",
							Color = "good",
							Fields = new List<Field> {
								new Field {
									Title = Emoji.Question + " /about",
									Value = "Explain, what can this bot do. (Send this message)"
								}
							}
						}
					}
				};
				return message;
			}, ctx => ctx.Request.Body.Length > 0);
		}
	}
}
