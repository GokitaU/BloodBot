using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace BloodBot
{
    public class MatchParser
    {
        /// <summary>
        ///     Helper function that checks if the class of the node is equal to the given string
        /// </summary>
        bool CheckNodeClass(HtmlNode node, string text)
        {
            if (node.GetAttributeValue("class", "").Equals(text))
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }

        /// <summary>
        ///     Helper function that returns the numbers in a given string
        /// </summary>
        string GetNumbers(string text)
        {
            string numbers = "";
            foreach (char c in text)
            {
                if (char.IsDigit(c))
                {
                    numbers += c;
                }
            }

            return numbers;
        }

        /// <summary>
        ///     Looks through the current matches on FUMBBL and gets all the matches in the desired section
        /// </summary>
        public Dictionary<string, Match> GetMatches()
        {
            Dictionary<string, Match> Matches = new Dictionary<string, Match>();
            HtmlWeb web = new HtmlWeb();
            HtmlDocument document = null;

            int tries = 0;
            bool success = false;
            while (success == false)
            {
                try
                {
                    document = web.Load("https://fumbbl.com/p/games");
                    success = true;
                }
                catch (Exception e)
                {
                    tries++;
                    Logger.Log(DateTime.UtcNow + " fumbbl is kill, TRIES: " + tries);
                    System.Threading.Thread.Sleep(60000);
                }
            }

            // div = navigator and where we start looking, Gamesfooter = end
            HtmlNode div = document.DocumentNode.SelectSingleNode("//div[.='League tournaments']");
            HtmlNode Gamesfooter = document.DocumentNode.SelectSingleNode("//div[@class='gamesfooter']");

            // FUMBBL divisions
            HtmlNode League = document.DocumentNode.SelectSingleNode("//div[.='League']");
            HtmlNode Blackbox = document.DocumentNode.SelectSingleNode("//div[.='Blackbox']");

            // storage variables to make new matches
            string MatchID = "null";
            string Tournament = "";
            string Group = "";
            string HomeCoach = "";
            string HomeTeam = "";
            string HomeRace = "";
            string HomeTV = "";
            string AwayCoach = "";
            string AwayTeam = "";
            string AwayRace = "";
            string AwayTV = "";
            string SpectatorLink = "";

            while (/*(div != League) && (div != Blackbox) &&*/ (div != Gamesfooter) && (div != null))
            {
                // get tournament name
                if (CheckNodeClass(div, "tournament"))
                {
                    Tournament = div.ChildNodes["a"].InnerText;

                    Group = div.ChildNodes["a"].GetAttributeValue("href", "");
                    if (Group == "/p/group?op=view&amp;group=9828") // main
                    {
                        Group = "/tg/ FUMBBL League";
                    }
                    else if (Group == "/p/group?op=view&amp;group=10667") // monkey
                    {
                        Group = "/tg/ Monkey League";
                    }
                    else if (Group == "/p/group?op=view&amp;group=10393") // secret
                    {
                        Group = "/tg/ Secret League";
                    }
                    else if (Group == "/p/group?op=view&amp;group=10178") // stunty
                    {
                        Group = "/tg/ Stunty Leeg";
                    }
                    else if (Group == "/p/group?op=view&amp;group=10591") // stall
                    {
                        Group = "/tg/ STALL";
                    }
                    else if (Group == "/p/group?op=view&amp;group=10948") // gacha
                    {
                        Group = "Cuckrim Gacha Draft Paradise";
                    }
                    else if (Group == "/p/group?op=view&amp;group=11066") // speed
                    {
                        Group = "/tg/ SPEED Bowl";
                    }
                }

                // get Match info
                if (CheckNodeClass(div, "matchrecord withtournament"))
                {
                    foreach (HtmlNode node in div.ChildNodes)
                    {

                        if (CheckNodeClass(node, "home"))
                        {
                            foreach (HtmlNode node2 in node.ChildNodes)
                            {
                                if (CheckNodeClass(node2, "team"))
                                {
                                    HomeTeam = node2.InnerText;
                                }

                                else if (CheckNodeClass(node2, "race"))
                                {
                                    HomeCoach = node2.ChildNodes["a"].InnerText;
                                    HomeRace = node2.LastChild.InnerText;
                                }
                            }
                        }

                        else if (CheckNodeClass(node, "divider"))
                        {
                            foreach (HtmlNode node2 in node.ChildNodes)
                            {
                                //if (CheckNodeClass(node2, "progress"))
                                //{
                                //    // time
                                //    Logger.Log("Time: " + node2.FirstChild.InnerText);
                                //}
                                if (CheckNodeClass(node2, "spectate"))
                                {
                                    // spectator link
                                    //Logger.Log("Spectate: " + node2.ChildNodes["a"].GetAttributeValue("href", ""));
                                    SpectatorLink = node2.ChildNodes["a"].GetAttributeValue("href","");
                                    MatchID = GetNumbers(node2.ChildNodes["a"].GetAttributeValue("href", ""));
                                }
                            }
                        }

                        else if ((CheckNodeClass(node, "away")))
                        {
                            foreach (HtmlNode node2 in node.ChildNodes)
                            {
                                if (CheckNodeClass(node2, "team"))
                                {
                                    AwayTeam = node2.InnerText;
                                }

                                else if (CheckNodeClass(node2, "race"))
                                {
                                    AwayCoach = node2.ChildNodes["a"].InnerText;
                                    AwayRace = node2.FirstChild.InnerText;
                                }
                            }
                        }
                    }
                }

                //update or create match
                if (CheckNodeClass(div, "matchrecord withtournament"))
                {
                    if (Group.StartsWith("/tg/") || Group.StartsWith("Cuckrim"))
                    {
                        Matches[MatchID] = new Match(MatchID, Tournament, HomeCoach, HomeTeam, HomeRace, HomeTV, AwayCoach, AwayTeam, AwayRace, AwayTV, SpectatorLink, Group);
                    }
                    MatchID = "null";
                }
                div = div.NextSibling;
            }
            if (div == null) Logger.Log(DateTimeOffset.UtcNow +" no matches being played");

            return Matches;
        }
    }
}
