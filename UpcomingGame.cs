using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloodBot
{
    public class UpcomingGame
    {
        public DateTime Date;
        public string TeamA;
        public string CoachA;
        public string TeamB;
        public string CoachB;


        public UpcomingGame(DateTime Date, string TeamA, string CoachA, string TeamB, string CoachB)
        {
            this.Date = Date;
            this.TeamA = TeamA;
            this.CoachA = CoachA;
            this.TeamB = TeamB;
            this.CoachB = CoachB;
        }

        public UpcomingGame()
        {
            /*
            Date = DateTime.Today;
            TeamA = null;
            CoachA = null;
            TeamB = null;
            CoachB = null;
            */
        }
    }
}
