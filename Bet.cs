using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloodBot
{
    public class bet
    {
        //public string ID;
        public long MatchID;
        public string TeamA;
        public string TeamB;
        public decimal Money;
        public string Score;
        public char Outcome;
        public long UserID;
        public long BetID;

        // TODO SQL.GetMatchTeams (can be outside instead of here?)
        // TeamA = A
        // TeamB = B

        // !Teams & Score
        public bet(long UserID, long MatchID, string Score, decimal Money)
        {
            this.UserID = UserID;
            this.MatchID = MatchID;
            this.Money = Money;
            this.Score = Score;
            // Get Outcome
            int A = Score[0];
            int B = Score[2];
            if (A > B)
            {
                Outcome = 'A';
            }
            else if (B > A)
            {
                Outcome = 'B';
            }
            else
            {
                Outcome = 'T';
            }
        }

        // With no Teams & no Score
        public bet(long UserID, long MatchID, char Outcome, decimal Money)
        {
            this.UserID = UserID;
            this.MatchID = MatchID;
            this.Money = Money;
            this.Outcome = Outcome;
        }

        // With Teams & Score
        public bet (long BetID, long UserID, long MatchID, string TeamA, string TeamB, string Score, char Outcome, decimal Money)
        {
            this.BetID = BetID;
            this.UserID = UserID;
            this.MatchID = MatchID;
            this.TeamA = TeamA;
            this.TeamB = TeamB;
            this.Score = Score;
            this.Outcome = Outcome;
            this.Money = Money;
        }

        // With Teams & no Scores
        public bet(long BetID, long UserID, long MatchID, string TeamA, string TeamB, char Outcome, decimal Money)
        {
            this.BetID = BetID;
            this.UserID = UserID;
            this.MatchID = MatchID;
            this.TeamA = TeamA;
            this.TeamB = TeamB;
            this.Outcome = Outcome;
            this.Money = Money;
        }
    }
}
