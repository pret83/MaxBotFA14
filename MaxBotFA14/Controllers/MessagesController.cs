using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Utilities;
using Newtonsoft.Json;
using System.Xml;
using System.ServiceModel.Syndication;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Runtime.Serialization.Json;
using MaxBotFA14.Model;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MaxBotFA14
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private List<Races> races;

        //public MessagesController()
        //{
        //    string eventJson;
        //    using (WebClient client = new WebClient())
        //    {
        //        eventJson = client.DownloadString(@"http://maxchallenge-bot.azurewebsites.net/events/2016");
        //    }

        //    races = JsonConvert.DeserializeObject<List<Races>>(eventJson);
        //}
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<Message> Post([FromBody]Message message)
        {
            if (message.Type == "Message")
            {
                // calculate something for us to return
                //int length = (message.Text ?? string.Empty).Length;

                //// return our reply to the user
                //return message.CreateReplyMessage($"You sent {length} characters");
                return HandleUserMessage(message);


            }
            else
            {
                return HandleSystemMessage(message);
            }
        }


        private Message HandleUserMessage(Message message)
        {
            if (races == null)
            {
                string eventJson;
                using (WebClient client = new WebClient())
                {
                    eventJson = client.DownloadString(@"http://maxchallenge-bot.azurewebsites.net/events/2016");
                }
                races = JsonConvert.DeserializeObject<List<Races>>(eventJson);
            }


            string gp = ExtractEvent(message.Text);
            if (!string.IsNullOrEmpty(gp))
            {

                var gpInfo = races.FirstOrDefault(r => r.Event.Equals(gp));

                if (gpInfo != null)
                {
                    GpSession session = ExtractSession(message.Text);
                    SessionEvent sevent = ExtractSessionEvent(message.Text);


                    if (message.Text.Contains("when") || message.Text.Contains("date") || message.Text.Contains("time") || message.Text.Contains("day"))
                    {
                        if (session != GpSession.Unknown && sevent != SessionEvent.Unknown)
                        {
                            switch (session)
                            {
                                case GpSession.FP1:
                                    {
                                        if (sevent == SessionEvent.Begin)
                                        {
                                            return message.CreateReplyMessage(GetNiceTimeFromString(gpInfo.Schedule.Free_0020_practice_0020_1.time_from));
                                        }
                                        else if (sevent == SessionEvent.Date)
                                        {
                                            return message.CreateReplyMessage(gpInfo.Schedule.Free_0020_practice_0020_1.date);
                                        }
                                        else if (sevent == SessionEvent.Finish)
                                        {
                                            return message.CreateReplyMessage(GetNiceTimeFromString(gpInfo.Schedule.Free_0020_practice_0020_1.time_to));
                                        }
                                        break;
                                    }
                                case GpSession.FP2:
                                    {
                                        if (sevent == SessionEvent.Begin)
                                        {
                                            return message.CreateReplyMessage(GetNiceTimeFromString(gpInfo.Schedule.Free_0020_practice_0020_2.time_from));
                                        }
                                        else if (sevent == SessionEvent.Date)
                                        {
                                            return message.CreateReplyMessage(gpInfo.Schedule.Free_0020_practice_0020_2.date);
                                        }
                                        else if (sevent == SessionEvent.Finish)
                                        {
                                            return message.CreateReplyMessage(GetNiceTimeFromString(gpInfo.Schedule.Free_0020_practice_0020_2.time_to));
                                        }
                                        break;
                                    }
                                case GpSession.FP3:
                                    {
                                        if (sevent == SessionEvent.Begin)
                                        {
                                            return message.CreateReplyMessage(GetNiceTimeFromString(gpInfo.Schedule.Free_0020_practice_0020_3.time_from));
                                        }
                                        else if (sevent == SessionEvent.Date)
                                        {
                                            return message.CreateReplyMessage(gpInfo.Schedule.Free_0020_practice_0020_3.date);
                                        }
                                        else if (sevent == SessionEvent.Finish)
                                        {
                                            return message.CreateReplyMessage(GetNiceTimeFromString(gpInfo.Schedule.Free_0020_practice_0020_3.time_to));
                                        }
                                        break;
                                    }
                                case GpSession.Qualy:
                                    {
                                        if (sevent == SessionEvent.Begin)
                                        {
                                            return message.CreateReplyMessage(GetNiceTimeFromString(gpInfo.Schedule.Qualifying.time_from));
                                        }
                                        else if (sevent == SessionEvent.Date)
                                        {
                                            return message.CreateReplyMessage(GetNiceTimeFromString(gpInfo.Schedule.Qualifying.date));
                                        }
                                        else if (sevent == SessionEvent.Finish)
                                        {
                                            return message.CreateReplyMessage(GetNiceTimeFromString(gpInfo.Schedule.Qualifying.time_to));
                                        }
                                        break;
                                    }
                                case GpSession.Race:
                                    {
                                        if (sevent == SessionEvent.Begin)
                                        {
                                            return message.CreateReplyMessage(GetNiceTimeFromString(gpInfo.Schedule.Race.time_from));
                                        }
                                        else if (sevent == SessionEvent.Date)
                                        {
                                            return message.CreateReplyMessage(GetNiceTimeFromString(gpInfo.Schedule.Race.date));
                                        }
                                        else if (sevent == SessionEvent.Finish)
                                        {
                                            return message.CreateReplyMessage(GetNiceTimeFromString(gpInfo.Schedule.Race.time_to));
                                        }
                                        break;
                                    }
                            }
                        }
                    }
                    else
                    {
                        TrackInfoPiece info = ExtractInfo(message.Text);
                        if (info == TrackInfoPiece.Description)
                        {
                            return message.CreateReplyMessage(gpInfo.Track.description);
                        }
                        else if (info == TrackInfoPiece.History)
                        {
                            return message.CreateReplyMessage(gpInfo.Track.history);
                        }
                        else if (info == TrackInfoPiece.Quote)
                        {
                            return message.CreateReplyMessage(gpInfo.Track.qoute);
                        }
                    }
                }
            }

            else
            {
                var rawwords = message.Text.Split(new char[] { ' ' });
                var words = rawwords.Select(w => w.Trim(new char[] { ' ', ',', ';', '.', '!', '?', ';', ':' })).ToList();

                using (XmlReader responseReader = XmlReader.Create(@"http://maxchallenge-bot.azurewebsites.net/news"))
                {
                    SyndicationItem bestMatchItem = null;
                    SyndicationFeed feed = SyndicationFeed.Load(responseReader);

                    int maxMatch = int.MinValue;
                    foreach (var item in feed.Items)
                    {
                        int matchnum = words.Count(w => item.Summary.Text.Contains(w));
                        if (matchnum > maxMatch)
                        {
                            bestMatchItem = item;
                            maxMatch = matchnum;
                        }
                    }
                    if (bestMatchItem != null)
                    {
                        message.CreateReplyMessage(StripHTML(bestMatchItem.Summary.Text));
                    }
                }
            }

            return message.CreateReplyMessage("Sorry, I don't know the answer.");

        }

        public string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }
        private string GetNiceTimeFromString(string text)
        {
            //TODO: implement nicer
            return text;
        }

        private TrackInfoPiece ExtractInfo(string text)
        {
            if (text.Contains("description") || text.Contains("about"))
            {
                return TrackInfoPiece.Description;
            }
            else if (text.Contains("history") || text.Contains("past"))
            {
                return TrackInfoPiece.History;
            }
            else if (text.Contains("think") || text.Contains("quote"))
            {
                return TrackInfoPiece.Quote;
            }
            else
            {
                return TrackInfoPiece.Unknown;
            }
        }

        private SessionEvent ExtractSessionEvent(string text)
        {
            if (text.Contains("begin") || text.Contains("start"))
            {
                return SessionEvent.Begin;
            }
            else if (text.Contains("end") || text.Contains("finish"))
            {
                return SessionEvent.Finish;
            }
            else if (text.Contains("day") || text.Contains("date"))
            {
                return SessionEvent.Date;
            }
            else
            {
                return SessionEvent.Unknown;
            }
        }

        private GpSession ExtractSession(string text)
        {
            if (text.Contains("practice 1") || text.Contains("first practice") || text.Contains("first free practice"))
            {
                return GpSession.FP1;
            }
            else if (text.Contains("practice 2") || text.Contains("second practice") || text.Contains("second free practice"))
            {
                return GpSession.FP2;
            }
            else if (text.Contains("practice 3") || text.Contains("third practice") || text.Contains("third free practice"))
            {
                return GpSession.FP3;
            }
            else if (text.Contains("Qualifying"))
            {
                return GpSession.Qualy;
            }
            else if (text.Contains("Race"))
            {
                return GpSession.Race;
            }
            else
            {
                return GpSession.Unknown;
            }
        }

        private string ExtractEvent(string text)
        {
            if (text.Contains("AUSTRALIA") || text.Contains("Melbourne"))
            {
                return "FORMULA 1 AUSTRALIA";
            }
            else if (text.Contains("BAHRAIN"))
            {
                return "FORMULA 1 BAHRAIN";
            }
            else if (text.Contains("CHINA") || text.Contains("chinese"))
            {
                return "FORMULA 1 CHINA";
            }
            else if (text.Contains("RUSSIA") || text.Contains("Sochi"))
            {
                return "FORMULA 1 RUSSIA";
            }
            else if (text.Contains("SPAIN") || text.Contains("Spanish") || text.Contains("Barcelona"))
            {
                return "FORMULA 1 SPAIN";
            }
            else if (text.Contains("MONACO") || text.Contains("Monte Carlo"))
            {
                return "FORMULA 1 MONACO";
            }
            else if (text.Contains("CANADA") || text.Contains("Montreal") || text.Contains("Canadian"))
            {
                return "FORMULA 1 CANADA";
            }
            else if (text.Contains("EUROPE") || text.Contains("Baku"))
            {
                return "FORMULA 1 EUROPE";
            }
            else if (text.Contains("AUSTRIA") || text.Contains("Spielberg") || text.Contains("The Red Bull Ring"))
            {
                return "FORMULA 1 AUSTRIA";
            }
            else if (text.Contains("GREAT BRITAIN") || text.Contains("Silverstone"))
            {
                return "FORMULA 1 GREAT BRITAIN";
            }
            else if (text.Contains("HUNGARY") || text.Contains("Hungaroring"))
            {
                return "FORMULA 1 HUNGARY";
            }
            else if (text.Contains("GERMANY") || text.Contains("Hockenheim"))
            {
                return "FORMULA 1 GERMANY";
            }
            else if (text.Contains("BELGIUM") || text.Contains("Spa-Francorchamps"))
            {
                return "FORMULA 1 BELGIUM";
            }
            else if (text.Contains("ITALY") || text.Contains("Monza"))
            {
                return "FORMULA 1 ITALY";
            }
            else if (text.Contains("SINGAPORE") || text.Contains("Marina Bay"))
            {
                return "FORMULA 1 SINGAPORE";
            }
            else if (text.Contains("MALAYSIA") || text.Contains("Sepang"))
            {
                return "FORMULA 1 MALAYSIA";
            }
            else if (text.Contains("JAPAN") || text.Contains("Suzuka"))
            {
                return "FORMULA 1 JAPAN";
            }
            else if (text.Contains("USA") || text.Contains("Americas"))
            {
                return "FORMULA 1 USA";
            }
            else if (text.Contains("MEXICO") || text.Contains("Mexican"))
            {
                return "FORMULA 1 MEXICO";
            }
            else if (text.Contains("BRAZIL"))
            {
                return "FORMULA 1 BRAZIL";
            }
            else if (text.Contains("ABU DHABI") || text.Contains("Yas Marina"))
            {
                return "FORMULA 1 ABU DHABI";
            }


            return string.Empty;
        }

        private Message HandleSystemMessage(Message message)
        {
            if (message.Type == "Ping")
            {
                Message reply = message.CreateReplyMessage();
                reply.Type = "Ping";
                return reply;
            }
            else if (message.Type == "DeleteUserData")
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == "BotAddedToConversation")
            {
            }
            else if (message.Type == "BotRemovedFromConversation")
            {
            }
            else if (message.Type == "UserAddedToConversation")
            {
            }
            else if (message.Type == "UserRemovedFromConversation")
            {
            }
            else if (message.Type == "EndOfConversation")
            {
            }

            return null;
        }
    }

    public enum GpSession
    {
        Unknown,
        FP1,
        FP2,
        FP3,
        Qualy,
        Race,
    }

    public enum SessionEvent
    {
        Unknown,
        Begin,
        Finish,
        Date,
    }

    public enum TrackInfoPiece
    {
        Unknown,
        Description,
        Quote,
        History,
    }
}