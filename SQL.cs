using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace BloodBot
{
    public class SQL
    {

        readonly string ConnectionString;
        public SQL()
        {

            ConnectionString = Config.Data[Config.ConfigKeys.ConnectionString];
        }

        // TODO 0: Wrap all SqlCommand commands in using statements
        // SqlConnection sql = new SqlConnection(ConnectionString)
        // https://stackoverflow.com/questions/23185990/sqlcommand-with-using-statement/23186013

        public bool TestConnection()
        {
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                try
                {
                    sql.Open();
                    return true;
                }
                catch (Exception e)
                {
                    throw (e);
                }
            }
        }

        //TODO update this to take a list of scheduledmatches objects once I make that, fumbblscraper should be changed for that too
        public void SubmitTeams(List<string> Teams)
        {
            SqlConnection sql = new SqlConnection(@ConnectionString);

            int counter = 0;
            while (counter <= Teams.Count - 1)
            {
                if (Teams[counter] != "&nbsp;" && Teams[counter + 1] != "&nbsp;")
                {
                    SqlCommand command = new SqlCommand("IF NOT EXISTS " +
                        "(SELECT * FROM Matches WHERE TeamA LIKE @TeamA AND TeamB LIKE @TeamB)" +
                        "BEGIN " +
                        "INSERT INTO Matches (TeamA, TeamB, Live, AID, BID) VALUES (@TeamA, @TeamB, @Live, @AID, @BID)" +
                        "END", sql);
                    command.Parameters.AddWithValue("@TeamA", Teams[counter]);
                    command.Parameters.AddWithValue("@TEAMB", Teams[counter + 1]);
                    command.Parameters.AddWithValue("@Live", 0);
                    command.Parameters.AddWithValue("@AID", int.Parse(Teams[counter + 2]));
                    command.Parameters.AddWithValue("@BID", int.Parse(Teams[counter + 3]));
                    sql.Open();
                    command.ExecuteNonQuery();
                    sql.Close();
                }
                counter += 4;
            }
            sql.Dispose();
        }

        public void RegisterUser(long UserID, string Username)
        {
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("INSERT INTO Users (ID, Username, BBucks) VALUES (@UserID, @Username, @BBucks);", sql);
                command.Parameters.AddWithValue("@UserID", UserID);
                command.Parameters.AddWithValue("@Username", Username);
                command.Parameters.AddWithValue("@BBucks", 100);
                sql.Open();
                command.ExecuteNonQuery();
            }
        }

        public bool BetExists(long UserID, long MatchID)
        {
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("SELECT * FROM Bets WHERE UserID = @UserID AND MatchID = @MatchID", sql);
                command.Parameters.AddWithValue("@UserID", UserID);
                command.Parameters.AddWithValue("@MatchID", MatchID);
                sql.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }



        //TODO pass bet object
        public void MakeBet(bet bet)
        {
            SqlConnection sql = new SqlConnection(ConnectionString);
            if (bet.Score != null)
            {
                
                using (sql)
                {
                    SqlCommand Command = new SqlCommand("INSERT INTO Bets (UserID, MatchID, BetMoney, Outcome, Score) VALUES (@UserID, @MatchID, @BetMoney, @Outcome, @Score)", sql);
                    Command.Parameters.AddWithValue("@UserID", bet.UserID);
                    Command.Parameters.AddWithValue("@MatchID", bet.MatchID);
                    Command.Parameters.AddWithValue("@BetMoney", bet.Money);
                    Command.Parameters.AddWithValue("@Score", bet.Score);
                    Command.Parameters.AddWithValue("@Outcome", bet.Outcome);
                    sql.Open();
                    Command.ExecuteNonQuery();
                }
            }
            else
            {
                using (sql)
                {
                    SqlCommand Command = new SqlCommand("INSERT INTO Bets (UserID, MatchID, BetMoney, Outcome) VALUES (@UserID, @MatchID, @BetMoney, @Outcome)", sql);
                    Command.Parameters.AddWithValue("@UserID", bet.UserID);
                    Command.Parameters.AddWithValue("@MatchID", bet.MatchID);
                    Command.Parameters.AddWithValue("@BetMoney", bet.Money);
                    Command.Parameters.AddWithValue("@Outcome", bet.Outcome);
                    sql.Open();
                    Command.ExecuteNonQuery();
                }
            }
        }

        public bool HasEnoughMoney(long UserID, decimal Money)
        {
            decimal CurrentMoney = 0;
            using (SqlConnection sql = new SqlConnection(ConnectionString))
            {
                sql.Open();
                SqlCommand command = new SqlCommand("SELECT BBucks FROM Users WHERE ID = @UserID;", sql);
                command.Parameters.AddWithValue("@UserID", UserID);
                SqlDataReader Reader = command.ExecuteReader();
                while (Reader.Read())
                {
                    CurrentMoney = Reader.GetDecimal(0);
                }
                sql.Close();
                sql.Dispose();
            }
            if (CurrentMoney >= Money && (CurrentMoney - Money) >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //shouldn't return null because then user doesn't exist, and minimum cash is always 0
        public decimal GetMoney(long UserID)
        {
            decimal Money = 0;
            using (SqlConnection sql = new SqlConnection(ConnectionString))
            {
                sql.Open();
                SqlCommand command = new SqlCommand("SELECT BBucks FROM Users WHERE ID = @UserID;", sql);
                command.Parameters.AddWithValue("@UserID", UserID);
                SqlDataReader Reader = command.ExecuteReader();
                while (Reader.Read())
                {
                    Money = Reader.GetDecimal(0);
                }
                sql.Close();
                sql.Dispose();
            }
            return Money;
        }

        public bool SubstractMoney(long UserID, decimal Money)
        {
            if (HasEnoughMoney(UserID, Money))
            {
                SqlConnection sql = new SqlConnection(ConnectionString);
                using (sql)
                {
                    SqlCommand command = new SqlCommand("UPDATE Users SET BBucks = BBucks - @Money WHERE ID = @UserID", sql);
                    command.Parameters.AddWithValue("@Money", Money);
                    command.Parameters.AddWithValue("@UserID", UserID);
                    sql.Open();
                    command.ExecuteNonQuery();
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public void AddMoney(long UserID, decimal Money)
        {
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("UPDATE Users SET BBucks = BBucks + @Money WHERE ID = @UserID", sql);
                command.Parameters.AddWithValue("@Money", Money);
                command.Parameters.AddWithValue("@UserID", UserID);
                sql.Open();
                command.ExecuteNonQuery();
            }
        }

        public bool UserExists(long UserID)
        {
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("SELECT * FROM Users WHERE ID = @UserID", sql);
                command.Parameters.AddWithValue("@UserID", UserID);
                sql.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        public bool MatchExists(long MatchID)
        {
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("SELECT * FROM Matches WHERE ID = @MatchID", sql);
                command.Parameters.AddWithValue("@MatchID", MatchID);
                sql.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        //TODO show bet odds
        public List<bet> GetBets(long UserID)
        {
            List<bet> bets = new List<bet>();
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("SELECT Matches.ID, Matches.TeamA, Matches.TeamB, Bets.Score, Bets.Outcome, Bets.BetMoney, Bets.ID " +
                                                    "FROM Bets INNER JOIN Matches ON Bets.MatchID = Matches.ID " +
                                                    "WHERE Bets.UserID = @UserID", sql);
                command.Parameters.AddWithValue("@UserID", UserID);
                sql.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (!reader.HasRows)
                {
                    return null;
                }
                else
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(3))
                        {
                            bets.Add(new bet(reader.GetInt64(6), UserID, reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4)[0], reader.GetDecimal(5)));
                        }
                        else
                        {
                            bets.Add(new bet(reader.GetInt64(6), UserID, reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(4)[0], reader.GetDecimal(5)));
                        }
                    }
                }
            }
            return (bets);
        }

        public Dictionary<int, List<string>> GetMatches()
        {
            Dictionary<int, List<string>> Matches = new Dictionary<int, List<string>>();
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("SELECT ID, TeamA, TeamB, AID, BID FROM Matches", sql);
                sql.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (!reader.HasRows)
                {
                    return null;
                }
                else
                {
                    int i = 0;
                    while (reader.Read())
                    {
                        // Matches[(int)reader.GetInt64(0)] <---- old one if new one fucks it up
                        Matches[i] = new List<string>
                        {
                            Convert.ToString(reader.GetInt64(0)),
                            reader.GetString(1),
                            reader.GetString(2),
                            reader.GetInt32(3).ToString(),
                            reader.GetInt32(4).ToString()
                        };
                        i++;
                    }
                }
            }
            return (Matches);
        }

        // TODO fix types
        public Dictionary<int, List<string>> GetMatchesWithPot()
        {
            Dictionary<int, List<string>> MatchesWithPot = new Dictionary<int, List<string>>();
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("SELECT Matches.ID, CONVERT(VARCHAR(255),Matches.TeamA), CONVERT(VARCHAR(255),Matches.TeamB), " +
                                                    "SUM(Bets.BetMoney) FROM Matches INNER JOIN Bets ON Matches.ID = Bets.MatchID GROUP BY Matches.ID, " +
                                                    "CONVERT(VARCHAR(255), Matches.TeamA), CONVERT(VARCHAR(255), Matches.TeamB); ", sql);
                sql.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (!reader.HasRows)
                {
                    return null;
                }
                else
                {
                    while (reader.Read())
                    {
                        MatchesWithPot[reader.GetInt32(0)] = new List<string>
                        {
                            Convert.ToString(reader.GetInt32(0)),
                            reader.GetString(1),
                            reader.GetString(2),
                            Convert.ToString(reader.GetInt32(3))
                        };
                    }
                }
            }
            return (MatchesWithPot);
        }

        public bool IsTeamA(string team)
        {
            bool IsTeamA = false;
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("SELECT TeamA FROM Matches WHERE TeamA = @TeamA", sql);
                command.Parameters.AddWithValue("@TeamA", team);
                sql.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    IsTeamA = true;
                }
                else
                {
                    IsTeamA = false;
                }

            }
            return IsTeamA;
        }

        public bool IsTeamB(string team)
        {
            bool IsTeamB = false;
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("SELECT TeamB FROM Matches WHERE TeamB = @TeamB", sql);
                command.Parameters.AddWithValue("@TeamB", team);
                sql.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    IsTeamB = true;
                }
                else
                {
                    IsTeamB = false;
                }

            }
            return IsTeamB;
        }

        public decimal GetLoserPot(string MatchTeam, char Outcome)
        {
            decimal LoserPot = 0;
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("SELECT sum(Bets.BetMoney) " +
                    "FROM Bets INNER JOIN Matches on Bets.MatchID = Matches.ID " +
                    "WHERE (Matches.TeamA like @Team OR Matches.TeamB like @Team) AND Bets.Outcome NOT LIKE @Outcome", sql);
                command.Parameters.AddWithValue("@Team", MatchTeam);
                command.Parameters.AddWithValue("@Outcome", Outcome);
                sql.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        LoserPot = reader.GetDecimal(0);
                    }
                }
            }
            return LoserPot;
        }

        public decimal GetWinnerPot(string MatchTeam, char Outcome)
        {
            decimal WinnerPot = 0;
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("SELECT sum(Bets.BetMoney)" +
                    "FROM Bets INNER JOIN Matches on Bets.MatchID = Matches.ID " +
                    "WHERE (Matches.TeamA = @Team OR Matches.TeamB = @Team) AND Bets.Outcome = @Outcome", sql);
                command.Parameters.AddWithValue("@Team", MatchTeam);
                command.Parameters.AddWithValue("@Outcome", Outcome);
                sql.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        WinnerPot = reader.GetDecimal(0);
                    }
                    
                }
            }
            return WinnerPot;
        }

        public long GetTeamMatch(string MatchTeam)
        {
            long MatchID = 0;
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("SELECT ID FROM Matches WHERE TeamA LIKE @Team OR TeamB LIKE @Team", sql);
                command.Parameters.AddWithValue("@Team", MatchTeam);
                sql.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    MatchID = reader.GetInt64(0);
                }
            }
            return MatchID;
        }

        public List<long> GetMatchBettors(long MatchID)
        {
            List<long> Bettors = new List<long>();
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("SELECT UserID FROM Bets WHERE MatchID LIKE @MatchID", sql);
                command.Parameters.AddWithValue("@MatchID", MatchID);
                sql.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Bettors.Add(reader.GetInt64(0));
                }
            }
            return Bettors;
        }

        public List<long> GetBettors(long MatchID)
        {
            List<long> Bettors = new List<long>();
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("SELECT UserID FROM Bets WHERE MatchID LIKE @MatchID", sql);
                command.Parameters.AddWithValue("@MatchID", MatchID);
                sql.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Bettors.Add(reader.GetInt64(0));
                }
            }
            return Bettors;
        }

        public decimal GetBetMoney(long UserID, long MatchID)
        {
            decimal BetMoney = 0;
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("SELECT BetMoney " +
                    "FROM Bets " +
                    "WHERE MatchID = @MatchID AND UserID = @UserID", sql);
                command.Parameters.AddWithValue("@MatchID", MatchID);
                command.Parameters.AddWithValue("@UserID", UserID);
                sql.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    BetMoney = reader.GetDecimal(0);
                }
            }
            return BetMoney;
        }

        public string GetBetScore(long UserID, long MatchID)
        {
            string BetScore = null;
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("SELECT Score " +
                    "FROM Bets " +
                    "WHERE MatchID = @MatchID AND UserID = @UserID", sql);
                command.Parameters.AddWithValue("@MatchID", MatchID);
                command.Parameters.AddWithValue("@UserID", UserID);
                sql.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        BetScore = reader.GetString(0);
                    }
                }
            }
            Logger.Log(BetScore);
            return BetScore;
        }

        public char GetBetOutcome(long UserID, long MatchID)
        {
            char BetOutcome = ' ';
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("SELECT Outcome " +
                    "FROM Bets " +
                    "WHERE MatchID = @MatchID AND UserID = @UserID", sql);
                command.Parameters.AddWithValue("@MatchID", MatchID);
                command.Parameters.AddWithValue("@UserID", UserID);
                sql.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    BetOutcome = reader.GetString(0)[0];
                }
            }
            Logger.Log(BetOutcome);
            return BetOutcome;
        }

        public void DeleteBet(long UserID, long MatchID)
        {
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("DELETE FROM Bets WHERE UserID = @UserID AND MatchID = @MatchID", sql);
                command.Parameters.AddWithValue("@MatchID", MatchID);
                command.Parameters.AddWithValue("@UserID", UserID);
                sql.Open();
                // BeginExecuteNonQuery is async, so we assign its result to IAsyncResult so we know when it's over
                IAsyncResult result = command.BeginExecuteNonQuery();
                // Therefore we must manually end the query once it's complete, otherwise we end it before it's complete
                command.EndExecuteNonQuery(result);
            }
        }

        public void DeleteMatch(long MatchID)
        {
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("DELETE FROM Matches WHERE ID LIKE @MatchID", sql);
                command.Parameters.AddWithValue("@MatchID", MatchID);
                sql.Open();
                IAsyncResult result = command.BeginExecuteNonQuery();
                command.EndExecuteNonQuery(result);
            }
        }

        public void SetMatchLive(long MatchID)
        {
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("UPDATE Matches SET Live = 'True' WHERE ID LIKE @MatchID", sql);
                command.Parameters.AddWithValue("@MatchID", MatchID);
                sql.Open();
                IAsyncResult result = command.BeginExecuteNonQuery();
                command.EndExecuteNonQuery(result);
            }
        }

        public bool IsMatchLive(long MatchID)
        {
            bool Live = false;
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("SELECT Live FROM Matches WHERE ID = @MatchID", sql);
                command.Parameters.AddWithValue("@MatchID", MatchID);
                sql.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Live = reader.GetBoolean(0);
                }
            }
            return Live;
        }

        public decimal GetTeamID(string Team)
        {
            int ID = 0;
            if (IsTeamA(Team))
            {
                using (SqlConnection sql = new SqlConnection(ConnectionString))
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand("SELECT AID FROM Matches WHERE TeamA = @Team;", sql);
                    command.Parameters.AddWithValue("@Team", Team);
                    SqlDataReader Reader = command.ExecuteReader();
                    while (Reader.Read())
                    {
                        ID = Reader.GetInt32(0);
                    }
                    sql.Close();
                    sql.Dispose();
                }
            }
            else
            {
                using (SqlConnection sql = new SqlConnection(ConnectionString))
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand("SELECT BID FROM Matches WHERE TeamB = @Team;", sql);
                    command.Parameters.AddWithValue("@Team", Team);
                    SqlDataReader Reader = command.ExecuteReader();
                    while (Reader.Read())
                    {
                        ID = Reader.GetInt32(0);
                    }
                    sql.Close();
                    sql.Dispose();
                }
            }
            return ID;
        }

        public ScheduledMatch GetScheduledMatch(long MatchID)
        {
            ScheduledMatch Match = new ScheduledMatch();
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("SELECT TeamA, TeamB FROM Matches WHERE ID LIKE @MatchID", sql);
                command.Parameters.AddWithValue("@MatchID", MatchID);
                sql.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Match.TeamA = reader.GetString(0);
                    Match.TeamB = reader.GetString(1);
                }
            }
            return Match;
        }

        public void AddMoneyBet(long UserID, decimal Money)
        {
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("UPDATE Users SET MoneyBet = MoneyBet + @Money WHERE ID = @UserID", sql);
                command.Parameters.AddWithValue("@Money", Money);
                command.Parameters.AddWithValue("@UserID", UserID);
                sql.Open();
                command.ExecuteNonQuery();
            }
        }

        public void SubstractMoneyBet(long UserID, decimal Money)
        {
                SqlConnection sql = new SqlConnection(ConnectionString);
                using (sql)
                {
                    SqlCommand command = new SqlCommand("UPDATE Users SET MoneyBet = MoneyBet - @Money WHERE ID = @UserID", sql);
                    command.Parameters.AddWithValue("@Money", Money);
                    command.Parameters.AddWithValue("@UserID", UserID);
                    sql.Open();
                    command.ExecuteNonQuery();
                }
        }

        public List<long> GetUsersID()
        {
            List<long> Users = new List<long>();
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("SELECT ID FROM Users", sql);
                sql.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Users.Add(reader.GetInt64(0));
                }
            }
            return Users;
        }

        public decimal GetUserBetsBetMoney(long UserID)
        {
            decimal BetMoney = 0;
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("SELECT sum(BetMoney)  " +
                    "FROM Bets " +
                    "WHERE UserID = @UserID", sql);
                command.Parameters.AddWithValue("@UserID", UserID);
                sql.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        BetMoney = reader.GetDecimal(0);
                    }
                }
            }
            return BetMoney;
        }

        public decimal GetMoneyBet(long UserID)
        {
            decimal Money = 0;
            using (SqlConnection sql = new SqlConnection(ConnectionString))
            {
                sql.Open();
                SqlCommand command = new SqlCommand("SELECT MoneyBet FROM Users WHERE ID = @UserID;", sql);
                command.Parameters.AddWithValue("@UserID", UserID);
                SqlDataReader Reader = command.ExecuteReader();
                while (Reader.Read())
                {
                    Money = Reader.GetDecimal(0);
                }
                sql.Close();
                sql.Dispose();
            }
            return Money;
        }

        public Dictionary<string,decimal> GetUsersTotalMoney()
        {
            Dictionary<string, decimal> users = new Dictionary<string, decimal>();
            SqlConnection sql = new SqlConnection(ConnectionString);
            using (sql)
            {
                SqlCommand command = new SqlCommand("SELECT Username, sum(BBucks + MoneyBet) as bux " +
                    "FROM Users " +
                    "GROUP BY Username " +
                    "ORDER BY bux DESC;", sql);
                sql.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(reader.GetString(0),reader.GetDecimal(1));
                }
            }
            return users;
        }


    }
}
