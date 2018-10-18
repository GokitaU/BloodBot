using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloodBot
{
    // TODO use this everywhere
    public class Score
    {
        string score;
        int TeamA;
        int TeamB;

        public char FindWinner()
        {
            TeamA = Convert.ToInt32(score[0]);
            TeamB = Convert.ToInt32(score[2]);
            if (TeamA > TeamB)
            {
                return 'A';
            }
            else
            {
                return 'B';
            }
        }
    }
}
