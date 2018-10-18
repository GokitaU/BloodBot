using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloodBot
{
    // TODO rename to LiveMatch
    public class Match
    {
        public string MatchID;
        public string Tournament;
        public string Group;
        public string HomeCoach;
        public string HomeTeam;
        public string HomeRace;
        public string HomeTV;
        public string AwayCoach;
        public string AwayTeam;
        public string AwayRace;
        public string AwayTV;
        public string SpectatorLink;
        public bool Announced;
        

        //constructor
        public Match(string aMatchID, string aTournament, string aHomeCoach, string aHomeTeam, string aHomeRace, string aHomeTV,
            string aAwayCoach, string aAwayTeam, string aAwayRace, string aAwayTV, string aSpectatorLink, string aGroup)
        {
            MatchID = aMatchID;
            Tournament = aTournament;
            Group = aGroup;
            HomeCoach = aHomeCoach;
            HomeTeam = aHomeTeam;
            HomeRace = aHomeRace;
            HomeTV = aHomeTV;
            AwayCoach = aAwayCoach;
            AwayTeam = aAwayTeam;
            AwayRace = aAwayRace;
            AwayTV = aAwayTV;
            Announced = false;
            SpectatorLink = aSpectatorLink;
        }

        // TODO Should this really be here? 
        public string MakeAnnouncementString()
        {
            string MatchAnnouncement;
            MatchAnnouncement = "**" + HomeTeam + "** vs **" + AwayTeam+"**";
            return (MatchAnnouncement);
        }
    }
}
