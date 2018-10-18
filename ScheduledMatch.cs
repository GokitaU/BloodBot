using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloodBot
{
    public class ScheduledMatch
    {
        public string TeamA;
        public string TeamB;
        public bool Live;

        public ScheduledMatch()
        {

        }

        public ScheduledMatch(string TeamA, string TeamB, bool Live)
        {
            this.TeamA = TeamA;
            this.TeamB = TeamB;
            this.Live = Live;
        }

        public ScheduledMatch(string TeamA, string TeamB)
        {
            this.TeamA = TeamA;
            this.TeamB = TeamB;
        }
    }
}
