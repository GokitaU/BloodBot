using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace BloodBot
{
    public class BetResolver
    {
        FumbblScrapper scraper = new FumbblScrapper();
        SQL sql = new SQL();

        public void ResolveMatch(Match match)
        {
            Logger.Log("Resolving Match");
            Logger.Log("Getting Score");
            string score = scraper.GetScore(match.HomeTeam, match.AwayTeam);
            string TeamA;
            string TeamB;
            Logger.Log(score);

            // Sort the teams according to the DB
            if (sql.IsTeamA(match.HomeTeam))
            {
                TeamA = match.HomeTeam;
                TeamB = match.AwayTeam;
            }
            else
            {
                TeamA = match.AwayTeam;
                TeamB = match.HomeTeam;
                score = score[2] + "-" + score[0];
            }
            Logger.Log(score);

            int ScoreA = Convert.ToInt32(score[0]);
            int ScoreB = Convert.ToInt32(score[2]);

            char Outcome;
            if (ScoreA > ScoreB)
            {
                Outcome = 'A';
            }
            else if (ScoreB > ScoreA)
            {
                Outcome = 'B';
            }
            else
            {
                Outcome = 'T';
            }
            Logger.Log(Outcome);
            long MatchID = sql.GetTeamMatch(TeamA);

            // check if anyone bet at all, if not end
            // check if only losers bet and fuck them in the ass
            // check if only winners bet and reward them

            Logger.Log("Getting winner pot");
            decimal WinnerPot = sql.GetWinnerPot(TeamA, Outcome);
            Logger.Log(WinnerPot);
            Logger.Log("Getting loser pot");
            decimal Loserpot = sql.GetLoserPot(TeamA, Outcome);
            Logger.Log(Loserpot);
            decimal TotalPot = WinnerPot + Loserpot;
            Logger.Log(TotalPot);

            List<long> Users = new List<long>();
            Logger.Log("Getting Bettors");
            Users = sql.GetBettors(MatchID);
            Logger.Log("got bettors");

            string message = string.Format("Match finished \n **{0}** vs **{1}** \n Score: {2} \n",TeamA,TeamB,score);

            if (WinnerPot == 0 && Loserpot == 0)
            {
                // no one cared
                Logger.Log("no one cared");
                // TODO send message saying no one cared so nothing happened
            }
            else if (WinnerPot == 0)
            {
                // only losers bet
                Logger.Log("no one won");
                foreach (long User in Users)
                {
                    decimal BetMoney = sql.GetBetMoney(User, MatchID);
                    sql.DeleteBet(User, MatchID);
                    sql.SubstractMoneyBet(User, BetMoney);

                    bool broke = false;
                    if (sql.GetMoney(User) <= 0 && sql.GetBets(User) == null)
                    {
                        sql.AddMoney(User, 50);
                        broke = true;
                    }
                    if (broke)
                    {
                        message += string.Format(" {0} :skull_crossbones: {1}★ and broke! Here's 50★ \n", MentionUtils.MentionUser((ulong)User), BetMoney);
                    }
                    else
                    {
                        message += string.Format(" {0} :skull: {1}★ \n", MentionUtils.MentionUser((ulong)User), BetMoney);
                    }
                }
            }
            else if (Loserpot == 0)
            {
                // only winners bet
                Logger.Log("no one lost");
                foreach (long User in Users)
                {
                    decimal BetMoney = sql.GetBetMoney(User, MatchID);
                    sql.AddMoney(User, BetMoney);
                    sql.SubstractMoneyBet(User, BetMoney);
                    sql.DeleteBet(User, MatchID);
                    message += string.Format(" {0} Everyone won so no one wins anything, you get your bet back {1}★ \n", MentionUtils.MentionUser((ulong)User), BetMoney);
                }
            }
            else
            {
                // there were winners and losers
                Logger.Log("solving winners and losers");
                foreach (long User in Users)
                {
                    Logger.Log(User);
                    Logger.Log("getting bet money");
                    decimal BetMoney = sql.GetBetMoney(User, MatchID);
                    Logger.Log("if betoutcome == outcome");
                    if (sql.GetBetOutcome(User, MatchID) == Outcome)
                    {
                        Logger.Log("calculating percentpot");
                        Logger.Log("calculating moneywon");
                        decimal PercentPot = (int)Math.Round(((double)BetMoney / (double)WinnerPot) * 100);
                        decimal MoneyWon = (int)Math.Round(((double)PercentPot * 0.01) * (double)Loserpot);
                        Logger.Log("if betscore != null");
                        if (sql.GetBetScore(User, MatchID) != null && sql.GetBetScore(User, MatchID) == score)
                        {
                            sql.AddMoney(User, MoneyWon + BetMoney*2);
                            sql.SubstractMoneyBet(User, BetMoney);
                            sql.DeleteBet(User, MatchID);
                            message += string.Format(" {0} :star2: {1}★*2 + {2}★ → **{3}**★ \n", MentionUtils.MentionUser((ulong)User), BetMoney, MoneyWon, BetMoney * 2 + MoneyWon);
                        }
                        else
                        {
                            sql.AddMoney(User, MoneyWon + BetMoney);
                            sql.SubstractMoneyBet(User, BetMoney);
                            sql.DeleteBet(User, MatchID);
                            message += string.Format(" {0} :star: {1}★ + {2}★ → **{3}**★ \n", MentionUtils.MentionUser((ulong)User), BetMoney, MoneyWon, BetMoney + MoneyWon);
                        }
                    }
                    else
                    {
                        sql.SubstractMoneyBet(User, BetMoney);
                        sql.DeleteBet(User, MatchID);

                        bool broke = false;
                        if (sql.GetMoney(User) <= 0 && sql.GetBets(User) == null)
                        {
                            sql.AddMoney(User, 50);
                            broke = true;
                        }
                        if (broke)
                        {
                            message += string.Format(" {0} :skull_crossbones: {1}★ and broke! Here's 50★ \n", MentionUtils.MentionUser((ulong)User), BetMoney);
                        }
                        else
                        {
                            message += string.Format(" {0} :skull: {1}★ \n", MentionUtils.MentionUser((ulong)User), BetMoney);
                        }
                    }
                }
            }
            Logger.Log("Finished resolving, deleting match");
            sql.DeleteMatch(MatchID);
            Logger.Log("match deleted, sending message");
            AnnounceResults(message);
        }

        private async void AnnounceResults(string message)
        {
            Logger.Log("Announcing bet results");
            var channel = Program.client.GetChannel(DiscordConstants.CHANNEL_CASINO) as SocketTextChannel;
            await channel.SendMessageAsync(message);
        }
    }
}
