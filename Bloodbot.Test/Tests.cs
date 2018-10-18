using System;
using NUnit.Framework;
using BloodBot;
using System.Collections.Generic;
using System.Xml;

namespace Bloodbot.Test
{
    [TestFixture]
    public class Class1
    {
        [Test]
        public void When_Log_Expect_Time()
        {
            Logger.Log("test");
        }

        [Test]
        public void When_TestConnection_Expect_True()
        {
            SQL sql = new SQL();
            sql.TestConnection();
        }

        [Test]
        public void When_GetBettors_Expect_Something()
        {
            SQL sql = new SQL();
            List<long> Bettors = sql.GetBettors(1);
            int i = 0;
            foreach (long Bettor in Bettors)
            {
                i++;
            }
            Assert.Warn(i.ToString());
        }

        [Test]
        public void When_ScrapperGetScore_Expect_Score()
        {
            FumbblScraper fs = new FumbblScraper();
            string score = fs.GetScore("Ratattackattack!!!", "Underdog United");
            Assert.Warn(score);
        }

        [Test]
        public void When_FabulousElfBois_Expect_True()
        {
            Match match = new Match("id", "tournament", "homecoach", "Fabulous♂ Elf-Bois", "homerace", "hometv", "awaycoach",
                "Basketball Boys", "awayrace", "awaytv", "spectatorlink", "group");
            SQL sql = new SQL();
            bool result = false;
            sql.SetMatchLive(1);
            result = sql.IsMatchLive(1);
            Assert.That(result == true);
        }

        [Test]
        public void Assert_that_basketballbois_teamA()
        {
            SQL sql = new SQL();
            Assert.That(sql.IsTeamA("Basketball Boys"));

        }

        [Test]
        public void When_GetScore_Expect_Score()
        {
            FumbblScraper fs = new FumbblScraper();
            string TeamA = "Quickslithers";
            string TeamB = "Teenage Mutant Ninja Hobos";
            string score;

            score = fs.GetScore(TeamA, TeamB);

            Assert.Warn(score);
        }
    }
}
