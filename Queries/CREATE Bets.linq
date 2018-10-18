<Query Kind="SQL">
  <Connection>
    <ID>e2f89bfd-a246-4e51-9dc0-4483e6574e43</ID>
    <Persist>true</Persist>
    <Server>(LocalDB)\MSSQLLocalDB</Server>
    <AttachFile>true</AttachFile>
    <AttachFileName>C:\Users\Me\Desktop\BloodBot\BloodBot\BloodBot.mdf</AttachFileName>
  </Connection>
</Query>

CREATE TABLE Bets (
	ID bigint NOT NULL PRIMARY KEY IDENTITY(1,1),
	UserID bigint NOT NULL FOREIGN KEY REFERENCES Users(ID),
	MatchID bigint NOT NULL FOREIGN KEY REFERENCES Matches(ID),
	MoneyBet decimal NOT NULL,
	Bet text NOT NULL
	);