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
        private SyndicationFeed feed;
        //private StringComparison comp = StringComparison.InvariantCultureIgnoreCase;
        //private StringComparer scomp = new StringComparer(;


        private bool MyContains(string string1, string string2)
        {
            return string1.IndexOf(string2, StringComparison.InvariantCultureIgnoreCase) != -1;
        }

        public MessagesController()
        {
            FetchGps();
            FetchNews();
        }

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


        private async Task FetchNews()
        {
            using (XmlReader responseReader = XmlReader.Create(@"http://maxchallenge-bot.azurewebsites.net/news"))
            {
                //SyndicationItem bestMatchItem = null;
                feed = SyndicationFeed.Load(responseReader);

                //int maxMatch = int.MinValue;
                //foreach (var item in feed.Items)
                //{
                //    int matchnum = words.Count(w => item.Summary.Text.Contains(w));
                //    if (matchnum > maxMatch)
                //    {
                //        bestMatchItem = item;
                //        maxMatch = matchnum;
                //    }
                //}
                //if (bestMatchItem != null)
                //{
                //    message.CreateReplyMessage(StripHTML(bestMatchItem.Summary.Text));
                //}
            }
        }

        private async Task FetchGps()
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
        }

        private Message HandleUserMessage(Message message)
        {
            try
            {
                if (string.Equals(message.Text, "hi", StringComparison.InvariantCultureIgnoreCase))
                {
                    return message.CreateReplyMessage("Hi. I'm ready to answer your questions.");
                }

                if (races != null)
                {

                    string gp = ExtractEvent(message.Text);
                    if (!string.IsNullOrEmpty(gp))
                    {

                        var gpInfo = races.FirstOrDefault(r => r.Event.Equals(gp));

                        if (gpInfo != null)
                        {
                            GpSession session = ExtractSession(message.Text);
                            SessionEvent sevent = ExtractSessionEvent(message.Text);


                            if (MyContains(message.Text, "when") || MyContains(message.Text, "date") || MyContains(message.Text, "time") || MyContains(message.Text, "day"))
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
                }

                if (feed != null)
                {
                    var rawwords = message.Text.Split(new char[] { ' ' });
                    var words = rawwords.Select(w => w.Trim(new char[] { ' ', ',', ';', '.', '!', '?', ';', ':' })).ToList();

                    //using (XmlReader responseReader = XmlReader.Create(@"http://maxchallenge-bot.azurewebsites.net/news"))
                    //{
                    SyndicationItem bestMatchItem = null;
                    //    SyndicationFeed feed = SyndicationFeed.Load(responseReader);

                    int maxMatch = int.MinValue;
                    foreach (var item in feed.Items)
                    {
                        int matchnum = words.Count(w => w.Length > 4 && MyContains(item.Summary.Text, w));
                        if (matchnum > maxMatch)
                        {
                            bestMatchItem = item;
                            maxMatch = matchnum;
                        }
                    }
                    if (bestMatchItem != null)
                    {
                        return message.CreateReplyMessage(StripHTML(bestMatchItem.Summary.Text));
                    }
                    //}
                }
            }
            catch (Exception ex)
            {
                return message.CreateReplyMessage("Sorry, I don't know the answer.");
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
            if (MyContains(text, "description") || MyContains(text, "about"))
            {
                return TrackInfoPiece.Description;
            }
            else if (MyContains(text, "history") || MyContains(text, "past"))
            {
                return TrackInfoPiece.History;
            }
            else if (MyContains(text, "think") || MyContains(text, "quote"))
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
            if (MyContains(text, "begin") || MyContains(text, "start"))
            {
                return SessionEvent.Begin;
            }
            else if (MyContains(text, "end") || MyContains(text, "finish"))
            {
                return SessionEvent.Finish;
            }
            else if (MyContains(text, "day") || MyContains(text, "date"))
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
            if (MyContains(text, "practice 1") || MyContains(text, "first practice") || MyContains(text, "first free practice"))
            {
                return GpSession.FP1;
            }
            else if (MyContains(text, "practice 2") || MyContains(text, "second practice") || MyContains(text, "second free practice"))
            {
                return GpSession.FP2;
            }
            else if (MyContains(text, "practice 3") || MyContains(text, "third practice") || MyContains(text, "third free practice"))
            {
                return GpSession.FP3;
            }
            else if (MyContains(text, "Qualifying") || MyContains(text, "quali"))
            {
                return GpSession.Qualy;
            }
            else if (MyContains(text, "Race"))
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
            if (MyContains(text, "AUSTRALIA") || MyContains(text, "Melbourne"))
            {
                return "FORMULA 1 AUSTRALIA";
            }
            else if (MyContains(text, "BAHRAIN"))
            {
                return "FORMULA 1 BAHRAIN";
            }
            else if (MyContains(text, "CHINA") || MyContains(text, "chinese"))
            {
                return "FORMULA 1 CHINA";
            }
            else if (MyContains(text, "RUSSIA") || MyContains(text, "Sochi"))
            {
                return "FORMULA 1 RUSSIA";
            }
            else if (MyContains(text, "SPAIN") || MyContains(text, "Spanish") || MyContains(text, "Barcelona"))
            {
                return "FORMULA 1 SPAIN";
            }
            else if (MyContains(text, "MONACO") || MyContains(text, "Monte Carlo"))
            {
                return "FORMULA 1 MONACO";
            }
            else if (MyContains(text, "CANADA") || MyContains(text, "Montreal") || MyContains(text, "Canadian"))
            {
                return "FORMULA 1 CANADA";
            }
            else if (MyContains(text, "EUROPE") || MyContains(text, "Baku"))
            {
                return "FORMULA 1 EUROPE";
            }
            else if (MyContains(text, "AUSTRIA") || MyContains(text, "Spielberg") || MyContains(text, "The Red Bull Ring"))
            {
                return "FORMULA 1 AUSTRIA";
            }
            else if (MyContains(text, "GREAT BRITAIN") || MyContains(text, "Silverstone"))
            {
                return "FORMULA 1 GREAT BRITAIN";
            }
            else if (MyContains(text, "HUNGARY") || MyContains(text, "Hungaroring"))
            {
                return "FORMULA 1 HUNGARY";
            }
            else if (MyContains(text, "GERMANY") || MyContains(text, "Hockenheim"))
            {
                return "FORMULA 1 GERMANY";
            }
            else if (MyContains(text, "BELGIUM") || MyContains(text, "Spa-Francorchamps"))
            {
                return "FORMULA 1 BELGIUM";
            }
            else if (MyContains(text, "ITALY") || MyContains(text, "Monza"))
            {
                return "FORMULA 1 ITALY";
            }
            else if (MyContains(text, "SINGAPORE") || MyContains(text, "Marina Bay"))
            {
                return "FORMULA 1 SINGAPORE";
            }
            else if (MyContains(text, "MALAYSIA") || MyContains(text, "Sepang"))
            {
                return "FORMULA 1 MALAYSIA";
            }
            else if (MyContains(text, "JAPAN") || MyContains(text, "Suzuka"))
            {
                return "FORMULA 1 JAPAN";
            }
            else if (MyContains(text, "USA") || MyContains(text, "Americas"))
            {
                return "FORMULA 1 USA";
            }
            else if (MyContains(text, "MEXICO") || MyContains(text, "Mexican"))
            {
                return "FORMULA 1 MEXICO";
            }
            else if (MyContains(text, "BRAZIL"))
            {
                return "FORMULA 1 BRAZIL";
            }
            else if (MyContains(text, "ABU DHABI") || MyContains(text, "Yas Marina"))
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