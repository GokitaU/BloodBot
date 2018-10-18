CREATE PROCEDURE [dbo].[GetWinnerPot] 
    @Team text,
    @Score text
AS
BEGIN
    SELECT sum(Bets.MoneyBet)
                    FROM Bets INNER JOIN Matches on Bets.MatchID = Matches.ID
                    WHERE (Matches.TeamA like @Team OR Matches.TeamB like @Team) AND Bets.Bet LIKE @Score
END