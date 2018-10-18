using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloodBot
{
    class BloodBot
    {
        MatchParser matchparser = new MatchParser();
        SQL sql = new SQL();
        BetResolver betresolver = new BetResolver();
        FumbblScraper fumbblscrapper = new FumbblScraper();

        // TODO make these lists
        public Dictionary<string, Match> LiveMatches = new Dictionary<string, Match>();
        public Dictionary<string, Match> NewMatches = new Dictionary<string, Match>();

        public void Run()
        {
            System.Threading.Thread.Sleep(25000);
            while (true)
            {
                // TODO wrap in MatchParser or MatchAnnouncer?
                Logger.Log("Checking live matches");
                LiveMatches = NewMatches;
                NewMatches = matchparser.GetMatches();

                // Announce new matches
                foreach (KeyValuePair<string,Match> newmatch in NewMatches)
                {
                    if (newmatch.Value.Announced != true)
                    {
                        if (sql.IsMatchLive(sql.GetTeamMatch(newmatch.Value.HomeTeam)) != true)
                        {
                            AnnounceMatch(newmatch.Value);
                            sql.SetMatchLive(sql.GetTeamMatch(newmatch.Value.HomeTeam));
                        }
                        newmatch.Value.Announced = true;
                    }
                }

                // Resolve finished matches
                foreach (KeyValuePair<string,Match> livematch in LiveMatches)
                {
                    if (!NewMatches.ContainsKey(livematch.Key))
                    {
                        Logger.Log("resolving match");
                        // TODO make async because this can take a while to get resolved
                        betresolver.ResolveMatch(livematch.Value);
                    }
                }

                // TODO wrap in TournamentParser(?) class
                // Get scheduled matches
                Logger.Log("Parsing main league matches");
                sql.SubmitTeams(fumbblscrapper.GetScheduledMatches(fumbblscrapper.GetTournamentsInProgress("9828")));
                Logger.Log("Parsing secret league matches");
                sql.SubmitTeams(fumbblscrapper.GetScheduledMatches(fumbblscrapper.GetTournamentsInProgress("10393")));

                // Info
                Logger.Log("LiveMatches count: " + LiveMatches.Count());
                Logger.Log("NewMatches count: " + NewMatches.Count());
                Logger.Log("Finished cycle");

                System.Threading.Thread.Sleep(60000);
            }
        }

        private async void AnnounceMatch(Match match)
        {
            Logger.Log("Announcing match");
            var channel = Program.client.GetChannel(DiscordConstants.CHANNEL_MATCH_ANNOUNCEMENTS) as SocketTextChannel;
            await channel.SendMessageAsync(MentionUtils.MentionRole(DiscordConstants.ROLE_LAZYFAN) + " \n" + "*" + match.Group + "* \n" + match.MakeAnnouncementString() + "\n" + "https://fumbbl.com" + match.SpectatorLink);
        }
    }
}
