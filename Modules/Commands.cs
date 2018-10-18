using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace BloodBot
{
    public class Info : ModuleBase
    {
        SQL sql = new SQL();
        FumbblScrapper fs = new FumbblScrapper();

        // TODO it should be somewhere else along with all other league information?
        const int LEAGUE_MAIN = 9828;
        const int LEAGUE_SECRET = 10393;

        // !say hello -> hello
        [RequireOwner]
        [Command("say"), Summary("Echos a message.")]
        public async Task Say([Remainder, Summary("The text to echo")] string echo)
        {
            await ReplyAsync(echo);
        }

        // !register -> register user
        [Command("register"), Summary("Register yourself")]
        public async Task Register()
        {
            long ID = (long)Context.User.Id;
            string Username = Context.User.Username;
            if (!sql.UserExists(ID))
            {
                sql.RegisterUser(ID, Username);
                await ReplyAsync(MentionUtils.MentionUser(Context.User.Id) + " you have been successfully registered");
            }
            else
            {
                await ReplyAsync(MentionUtils.MentionUser(Context.User.Id) + " you are already registered");
            }
        }

        // !money -> display user money (hand money + bet money)
        [RequireContext(ContextType.Guild)]
        [Command("money"), Summary("Check your money")]
        public async Task Money()
        {
            if (sql.UserExists((long)Context.User.Id))
            {
                decimal hand = sql.GetMoney((long)Context.User.Id);
                decimal bet = sql.GetMoneyBet((long)Context.User.Id);

                EmbedBuilder eb = new EmbedBuilder();
                eb.WithTitle("Your money");
                eb.WithColor(Color.DarkPurple);
                eb.WithThumbnailUrl("https://i.imgur.com/QTzpQlD.png");
                eb.AddField("Total", hand + bet+ "★");
                eb.AddInlineField("Hand", hand + "★");
                eb.AddInlineField("Bet", bet + "★");
                Embed e = eb.Build();
                await ReplyAsync(MentionUtils.MentionUser(Context.User.Id), embed: e);
            }
            else
            {
                await ReplyAsync(MentionUtils.MentionUser(Context.User.Id) + " you have to register first");
            }
        }

        // !bet <MatchID> <Score> <Money> -> makes a bet 
        [RequireContext(ContextType.Guild)]
        [Command("bet"), Summary("Make a bet with score")]
        public async Task Bet(long MatchID, string Score, decimal Money)
        {
            //await Context.Message.DeleteAsync();  // SECRET
            long ID = (long)Context.User.Id;

            if (!sql.UserExists(ID))
            {
                await ReplyAsync(MentionUtils.MentionUser(Context.User.Id) + " you have to register before betting");
            }
            else
            {
                if (!(Score.Length == 3 && char.IsDigit(Score[0]) && char.IsDigit(Score[2]) && Score[1] == '-'))
                {
                    await ReplyAsync(MentionUtils.MentionUser(Context.User.Id) + " invalid score format");
                }
                else
                {
                    // make bet object
                    bet bet = new bet(ID, MatchID, Score, Money);

                    if (!sql.HasEnoughMoney(ID, Money))
                    {
                        await ReplyAsync(MentionUtils.MentionUser(Context.User.Id) + " you don't have enough money to make that bet");
                    }
                    else
                    {
                        if (Money <= 0)
                        {
                            await ReplyAsync(MentionUtils.MentionUser(Context.User.Id) + " invalid number");
                        }
                        else
                        {
                            if (!sql.MatchExists(MatchID))
                            {
                                await ReplyAsync(MentionUtils.MentionUser(Context.User.Id) + " invalid match");
                            }
                            else
                            {
                                if (sql.IsMatchLive(MatchID))
                                {
                                    await ReplyAsync(MentionUtils.MentionUser(Context.User.Id) + " you can't bet during a live match baka desu senpai");
                                }
                                else
                                {
                                    if (sql.BetExists(ID, MatchID))
                                    {
                                        await ReplyAsync(MentionUtils.MentionUser(Context.User.Id) + " you already bet on that match");
                                    }
                                    else
                                    {
                                        // FINALS
                                        if (MatchID == 28)
                                        {
                                            bet.Money += 125;
                                            sql.MakeBet(bet);
                                            sql.SubstractMoney(ID, Money);
                                            sql.AddMoneyBet(ID, Money);
                                            await ReplyAsync(MentionUtils.MentionUser(Context.User.Id) + " FINALS MADNESS! You succesfully placed your bet and we added 125★ to it for free!");
                                        }
                                        else
                                        {
                                            sql.MakeBet(bet);
                                            sql.SubstractMoney(ID, Money);
                                            sql.AddMoneyBet(ID, Money);
                                            ScheduledMatch sm = sql.GetScheduledMatch(MatchID);
                                            //await ReplyAsync(string.Format("{0} bet placed on **{1}** vs **{2}**", MentionUtils.MentionUser(Context.User.Id), sm.TeamA, sm.TeamB));   // Secret
                                            await ReplyAsync(string.Format("{0} bet {3}★ on **{1}** vs **{2}**", MentionUtils.MentionUser(Context.User.Id), sm.TeamA, sm.TeamB, Money));// Public
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // TODO it doesn't split them right, sometimes it goes 3/2 or 2/1 or whatever when there's just 6+1
        [Command("matches", RunMode = RunMode.Async), Summary("Check scheduled matches")]
        public async Task Matches()
        {
            Dictionary<int, List<string>> Matches = sql.GetMatches();
            EmbedBuilder eb;
            int lenght = Matches.Keys.Count;
            int count = 0;
            int embedcount = 1;
            decimal embedsmax = Math.Round((decimal)lenght / 5);
            if (embedsmax == 0)
            {
                embedsmax = 1;
            }

            eb = new EmbedBuilder();
            eb.WithTitle(string.Format("Matches ({0}/{1})", embedcount, embedsmax));
            eb.WithColor(Color.DarkPurple);
            eb.WithThumbnailUrl("https://i.imgur.com/QTzpQlD.png");

            while (count < lenght)
            {
                Logger.Log(count);
                eb.AddField(":id: " + Matches[count][0], string.Format("**{0}** *vs* **{1}**", Matches[count][1], Matches[count][2]), false);
                eb.AddInlineField(fs.GetTeamCoach(int.Parse(Matches[count][3])), fs.GetTeamRace(int.Parse(Matches[count][3])));
                eb.AddInlineField(fs.GetTeamCoach(int.Parse(Matches[count][4])), fs.GetTeamRace(int.Parse(Matches[count][4])));

                if (count != 0 && count % 5 == 0)
                {
                    Embed e = eb.Build();
                    await ReplyAsync(string.Empty, embed: e).ConfigureAwait(false);
                    eb = new EmbedBuilder();
                    embedcount++;
                    eb.WithTitle(string.Format("Matches ({0}/{1})", embedcount, (Math.Round((decimal)lenght / 5))));
                    eb.WithColor(Color.DarkPurple);
                    eb.WithThumbnailUrl("https://i.imgur.com/QTzpQlD.png");
                }
                count++;
            }
            if (eb.Fields.Count != 0)
            {
                Embed e = eb.Build();
                await ReplyAsync(string.Empty, embed: e).ConfigureAwait(false);
            }
        }

        // only needs team names in no particular order
        // runmode async for not blocking the gateway
        [RequireOwner]
        [Command("resolve", RunMode = RunMode.Async), Summary("resolve match")]
        public async Task Resolve(string A, string B)
        {
            if (Context.User.Id == DiscordConstants.ID_OWNER)
            {
                Match match = new Match("id", "tournament", "Acoach", A, "Arace", "Atv", "Bcoach", B, "Brace", "Btv", "spectatorlink", "group");
                BetResolver br = new BetResolver();
                br.ResolveMatch(match);
                await ReplyAsync("resolving mtach");
            }
        }

        [RequireContext(ContextType.Guild)]
        [Command("bets", RunMode = RunMode.Async), Summary("Get bets")]
        public async Task Bets()
        {
            long ID = (long)Context.User.Id;
            Logger.Log("getting bets");
            List<bet> bets = new List<bet>();
            bets = sql.GetBets(ID);

            string message = string.Format("{0} here are your bets: \n", MentionUtils.MentionUser(Context.User.Id));
            Logger.Log("is bets null?");
            if (bets != null)
            {
                Logger.Log("bets is not null");
                foreach (bet bet in bets)
                {
                    if (bet.Score != null)
                    {
                        message += string.Format(string.Format(" ID: **{4}** **{0}** vs **{1}** -> **{2}** *{3}*★ \n", bet.TeamA, bet.TeamB, bet.Score, bet.Money, bet.MatchID));
                    }
                    else
                    {
                        message += string.Format(" ID: **{4}** **{0}** vs **{1}** -> **{2}** *{3}*★ \n", bet.TeamA, bet.TeamB, bet.Outcome, bet.Money, bet.MatchID);
                    }
                }
            }
            else
            {
                message = string.Format("{0} you have no bets", MentionUtils.MentionUser(Context.User.Id));
            }
            //await UserExtensions.SendMessageAsync(Context.Message.Author,message);    // Secret
            await ReplyAsync(message);                                                  // Public
        }

        [RequireContext(ContextType.Guild)]
        [Command("deletebet"), Summary("deletes a bet")]
        public async Task DeleteBet(long MatchID)
        {
            long ID = (long)Context.User.Id;
            if (sql.IsMatchLive(MatchID))
            {
                await ReplyAsync(MentionUtils.MentionUser(Context.User.Id) + " lel no");
            }
            else if (!sql.BetExists(ID, MatchID))
            {
                await ReplyAsync(MentionUtils.MentionUser(Context.User.Id) + " you haven't bet on that match yet");
            }
            else if (MatchID == 1)
            {
                decimal money = sql.GetBetMoney(ID, MatchID)-125;
                sql.AddMoney(ID, money);
                sql.DeleteBet(ID, MatchID);
                sql.SubstractMoneyBet(ID, money);
                await ReplyAsync(MentionUtils.MentionUser(Context.User.Id) + string.Format(" your bet has been deleted and you have been refunded your money", money));
            }
            else
            {
                decimal money = sql.GetBetMoney(ID, MatchID);
                sql.AddMoney(ID, money);
                sql.DeleteBet(ID, MatchID);
                sql.SubstractMoneyBet(ID, money);
                await ReplyAsync(MentionUtils.MentionUser(Context.User.Id) + string.Format(" your bet has been deleted and you have been refunded your money", money));
            }
        }

        [Command("help"), Summary("help")]
        public async Task Help()
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithTitle("Help");
            eb.WithColor(Color.DarkPurple);
            eb.WithThumbnailUrl("https://i.imgur.com/QTzpQlD.png");
            eb.AddField(":exclamation: faq", "Explains how the whole thing works and FAQs");
            eb.AddField(":exclamation: help", "Shows this message");
            eb.AddField(":exclamation: register", "Register yourself to start betting");
            eb.AddField(":exclamation: matches", "Shows a list with all the scheduled matches as A-B and their ID");
            eb.AddField(":exclamation: money", "Shows your money");
            eb.AddField(":exclamation: bet <Match ID> <Score> <Money>", "Make a bet with the score on a match\nFor more information on how winners and losers are determined check !faq\n **#-#**: A-B\n**Example**: !bet 1 2-1 100");
            eb.AddField(":exclamation: bets", "Shows a list with all your bets");
            eb.AddField(":exclamation: deletebet <Match ID>", "Deletes a bet on the specified match and refunds your money");
            eb.AddField(":exclamation: today", "Shows what games are scheduled for today with their time (fumbbl time)");
            eb.AddField(":exclamation: scheduled", "Shows what games are scheduled with their date and time (fumbbl time)");
            Embed e = eb.Build();
            await ReplyAsync(string.Empty, embed: e);//.ConfigureAwait(false);
        }

        [Command("faq"), Summary("info")]
        public async Task Faq()
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithTitle("Info");
            eb.WithColor(Color.DarkPurple);
            eb.WithThumbnailUrl("https://i.imgur.com/QTzpQlD.png");
            eb.AddField("How do I bet?", Properties.Resources.HowIsBetFormed);
            eb.AddField("How are winners determined, and how does Outcome/Score matter?", Properties.Resources.HowIsWinnerBorn);
            eb.AddField("How are payouts calculated?", Properties.Resources.HowIsPayoutCalculated);
            Embed e = eb.Build();
            await ReplyAsync(string.Empty, embed: e);
        }

        [Command("scheduled"), Summary("get scheduled matches")]
        public async Task Scheduled()
        {
            FumbblScrapper fs = new FumbblScrapper();
            List<UpcomingGame> games = new List<UpcomingGame>();
            // TODO get scheduled games for all /tg/ groups
            games.AddRange(fs.GetUpcomingMatches(LEAGUE_MAIN));
            games.AddRange(fs.GetUpcomingMatches(LEAGUE_SECRET));

            if (games.Count == 0)
            {
                await ReplyAsync("There are no scheduled games");
            }
            else
            {
                EmbedBuilder eb = new EmbedBuilder();
                eb.WithTitle("Scheduled Games");
                eb.WithColor(Color.DarkPurple);
                eb.WithThumbnailUrl("https://i.imgur.com/QTzpQlD.png");
                foreach (UpcomingGame game in games)
                {
                    eb.AddField(game.Date.ToString("dd-MM HH:mm UTC+02"), string.Format("({2}) **{0}** vs **{1}**", game.TeamA, game.TeamB, sql.GetTeamMatch(game.TeamA)));
                }

                Embed e = eb.Build();
                await ReplyAsync(string.Empty, embed: e);
            }
        }

        [Command("today"), Summary("get scheduled games for today")]
        public async Task Today()
        {
            FumbblScrapper fs = new FumbblScrapper();
            // TODO get scheduled games for all /tg/ groups
            List<UpcomingGame> games = fs.GetUpcomingMatches(LEAGUE_MAIN);
            if (games.Count == 0)
            {
                await ReplyAsync("There are no scheduled games");
            }

            else
            {
                Logger.Log(games.Count);
                EmbedBuilder eb = new EmbedBuilder();
                eb.WithTitle("Today's games");
                eb.WithColor(Color.DarkPurple);
                eb.WithThumbnailUrl("https://i.imgur.com/QTzpQlD.png");

                foreach (UpcomingGame game in games)
                {
                    Logger.Log(game.Date);
                    if (DateTime.Now.Date.Equals(game.Date.Date))
                    {
                        TimeSpan left = game.Date - DateTime.Now;
                        string leftstring = string.Format("{0}h {1}m", left.Hours, left.Minutes);
                        eb.AddField(string.Format(":id: {0}", sql.GetTeamMatch(game.TeamA)), string.Format("**{0}** vs **{1}** \n  :alarm_clock: {2} :arrow_left: *{3} left*", game.TeamA, game.TeamB, game.Date.ToString("HH:mm UTC+02"), leftstring));
                    }
                }

                if (eb.Fields.Count == 0)
                {
                    await ReplyAsync("There are no games scheduled for today");
                }
                else
                {
                    Embed e = eb.Build();
                    await ReplyAsync(string.Empty, embed: e);
                }
            }
        }

        [RequireContext(ContextType.Guild)]
        [Command("matchodds"), Summary("get the odds for a match")]
        public async Task MatchOdds(long matchid)
        {
            if (sql.MatchExists(matchid))
            {
                ScheduledMatch match = sql.GetScheduledMatch(matchid);
                List<long> bettors = sql.GetMatchBettors(matchid);

                decimal potA = 0;
                decimal potT = 0;
                decimal potB = 0;

                foreach (long user in bettors)
                {
                    sql.GetBetMoney(user, matchid);
                    char Outcome = sql.GetBetOutcome(user, matchid);
                    if (Outcome == 'A')
                    {
                        potA = potA + sql.GetBetMoney(user, matchid);
                    }
                    else if (Outcome == 'B')
                    {
                        potB = potB + sql.GetBetMoney(user, matchid);
                    }
                    else
                    {
                        potT = potT + sql.GetBetMoney(user, matchid);
                    }
                }

                decimal totalpot = potA + potB + potT;

                if (totalpot == 0)
                {
                    await ReplyAsync(string.Format(".\n" +
                    " **{0}** vs **{1}** \n\n" +
                    // PUBLIC ODDS
                    ":regional_indicator_a: 0★ \n" +
                    ":regional_indicator_t: 0★ \n" +
                    ":regional_indicator_b: 0★ \n" +
                    "\n:moneybag: {2}★", match.TeamA, match.TeamB, totalpot));
                }
                else
                {
                    int oddsA = (int)(Math.Round(potA / totalpot, 2) * 100);
                    int oddsT = (int)(Math.Round(potT / totalpot, 2) * 100);
                    int oddsB = (int)(Math.Round(potB / totalpot, 2) * 100);

                    await ReplyAsync(string.Format(".\n" +
                        " **{0}** vs **{1}** \n\n" +
                        // PUBLIC ODDS
                        ":regional_indicator_a: {2}★ ({3}%) \n" +
                        ":regional_indicator_t: {4}★ ({5}%) \n" +
                        ":regional_indicator_b: {6}★ ({7}%) \n" +
                        "\n:moneybag: {8}★", match.TeamA, match.TeamB, potA, oddsA, potT, oddsT, potB, oddsB, totalpot));
                }
            }
            else
            {
                await ReplyAsync("Invalid match");
            }
        }

        [Command("top"), Summary("get the 5 richest people")]
        public async Task Top()
        {
            Dictionary<string, decimal> users = sql.GetUsersTotalMoney();
            string topgoys = "The happiest merchants: \n";
            int count = 1;
            foreach (KeyValuePair<string,decimal> pair in users)
            {
                if (count == 6)
                {
                    break;
                }
                topgoys += string.Format("{0}. {1}: {2}★ \n", count, pair.Key, pair.Value);
                count++;
            }

            await ReplyAsync(topgoys);
        }
    }
}
