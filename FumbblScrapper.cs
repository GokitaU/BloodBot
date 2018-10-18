
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using HtmlAgilityPack;

namespace BloodBot
{
    public class FumbblScrapper
    {
        public string GetScore(string TeamA, string TeamB)
        {
            string score;
            XmlDocument MatchList = new XmlDocument();
            int page = 0;
            while (page != 50)
            {
                int tries = 0;
                bool success = false;
                while (success == false)
                {
                    try
                    {
                        MatchList.Load(string.Format("https://fumbbl.com/xml:matches?p={0}",page));
                        success = true;
                    }
                    catch (Exception e)
                    {
                        tries++;
                        Logger.Log(DateTime.UtcNow + " fumbbl is down, TRIES: " + tries);
                        System.Threading.Thread.Sleep(60000);
                    }

                    foreach (XmlNode node in MatchList.SelectNodes("//match"))
                    {
                        if (node.SelectSingleNode("home/name/text()").Value == TeamA ||
                            node.SelectSingleNode("away/name/text()").Value == TeamB)
                        {
                            score = (node.SelectSingleNode("home/touchdowns/text()").Value + "-" + node.SelectSingleNode("away/touchdowns/text()").Value);
                            return score;
                        }
                    }
                    page++;
                    System.Threading.Thread.Sleep(1000);
                }
            }
            throw new ArgumentException("Couldn't find the score for the given match");
        }

        public List<string> GetTournamentsInProgress(string GroupID)
        {
            string link = string.Format("https://fumbbl.com/api/group/tournaments/{0}/xml", GroupID);
            Logger.Log("Parsing tournaments in progress");
            List<string> list = new List<string>();

            Logger.Log("Loading tournament page");
            XmlDocument Tournaments = new XmlDocument();

            int tries = 0;
            bool success = false;
            while (success == false)
            {
                try
                {
                    Tournaments.Load(link);
                    success = true;
                }
                catch (Exception e)
                {
                    tries++;
                    Logger.Log(DateTime.UtcNow + " fumbbl is down, TRIES: " + tries);
                    System.Threading.Thread.Sleep(60000);
                }
            }

            foreach (XmlNode node in Tournaments.SelectNodes("//group"))
            {
                if (node.SelectSingleNode("status/text()").Value == "In Progress")
                {
                    list.Add(node.SelectSingleNode("@id").InnerText);
                }
            }
            return list;
        }

        public List<string> GetScheduledMatches(List<string> tournaments)
        {
            Logger.Log("Parsing scheduled matches");
            List<string> ScheduledGames = new List<string>();
            XmlDocument TournamentPage;
            foreach (string tournament in tournaments)
            {
                TournamentPage = new XmlDocument();

                int tries = 0;
                bool success = false;
                while (success == false)
                {
                    try
                    {
                        TournamentPage.Load(string.Format("https://fumbbl.com/api/tournament/schedule/{0}/xml", tournament));
                        success = true;
                    }
                    catch (Exception e)
                    {
                        tries++;
                        Logger.Log(DateTime.UtcNow + " fumbbl is down, TRIES: " + tries);
                        System.Threading.Thread.Sleep(60000);
                    }
                }

                foreach (XmlNode match in TournamentPage.SelectNodes("//match"))
                {
                    if (match.SelectSingleNode("result") == null)
                    {
                        ScheduledGames.Add(match.SelectSingleNode("teams/team[1]/name").InnerText);
                        ScheduledGames.Add(match.SelectSingleNode("teams/team[2]/name").InnerText);
                        ScheduledGames.Add(match.SelectSingleNode("teams/team[1]/@id").InnerText);
                        ScheduledGames.Add(match.SelectSingleNode("teams/team[2]/@id").InnerText);
                    }
                    
                }
            }
            return ScheduledGames;
        }

        public string GetTeamCoach(int id)
        {
            Logger.Log("getting team coach");
            string coach;
            XmlDocument TeamPage = new XmlDocument();
            int tries = 0;
            bool success = false;
            while (success == false)
            {
                try
                {
                    TeamPage.Load(string.Format("https://fumbbl.com/api/team/get/{0}/xml", id));
                    success = true;
                }
                catch (Exception e)
                {
                    tries++;
                    Logger.Log(DateTime.UtcNow + " fumbbl is down, TRIES: " + tries);
                    System.Threading.Thread.Sleep(60000);
                }
            }

            coach = TeamPage.SelectSingleNode("/team/coach/name").InnerText;
            return coach;
        }

        public string GetTeamRace(int id)
        {
            Logger.Log("getting team race");
            string race;
            XmlDocument TeamPage = new XmlDocument();
            int tries = 0;
            bool success = false;
            while (success == false)
            {
                try
                {
                    TeamPage.Load(string.Format("https://fumbbl.com/api/team/get/{0}/xml", id));
                    success = true;
                }
                catch (Exception e)
                {
                    tries++;
                    Logger.Log(DateTime.UtcNow + " fumbbl is down, TRIES: " + tries);
                    System.Threading.Thread.Sleep(60000);
                }
            }
            race = TeamPage.SelectSingleNode("/team/roster/name").InnerText;
            return race;
        }

        public List<UpcomingGame> GetUpcomingMatches(int id)
        {
            Logger.Log("getting upcoming matches");

            List<UpcomingGame> games = new List<UpcomingGame>();
            XmlDocument UpcomingMatchesPage = new XmlDocument();
            int tries = 0;
            bool success = false;
            while (success == false)
            {
                try
                {
                    UpcomingMatchesPage.Load(string.Format("https://fumbbl.com/api/group/upcoming/{0}/xml", id));
                    success = true;
                }
                catch (Exception e)
                {
                    tries++;
                    Logger.Log(DateTime.UtcNow + " fumbbl is down, TRIES: " + tries);
                    System.Threading.Thread.Sleep(60000);
                }
            }
            if (UpcomingMatchesPage.SelectNodes("//group") != null)
            {
                foreach (XmlNode match in UpcomingMatchesPage.SelectNodes("//group"))
                {
                    UpcomingGame game = new UpcomingGame();
                    Logger.Log(match.SelectSingleNode("date").InnerText);
                    game.Date = DateTime.Parse(match.SelectSingleNode("date").InnerText.Replace("CEST", "+2"));
                    game.TeamA = match.SelectSingleNode("home/name").InnerText;
                    game.CoachA = match.SelectSingleNode("home/coach").InnerText;
                    game.TeamB = match.SelectSingleNode("away/name").InnerText;
                    game.CoachB = match.SelectSingleNode("away/coach").InnerText;
                    games.Add(game);
                }
            }
            return games;
        }
    }
}
