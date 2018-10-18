<Query Kind="SQL">
  <Connection>
    <ID>e2f89bfd-a246-4e51-9dc0-4483e6574e43</ID>
    <Persist>true</Persist>
    <Server>(LocalDB)\MSSQLLocalDB</Server>
    <AttachFile>true</AttachFile>
    <AttachFileName>C:\Users\Me\Desktop\BloodBot\BloodBot\BloodBot.mdf</AttachFileName>
  </Connection>
</Query>

CREATE TABLE Matches (
	ID bigint NOT NULL PRIMARY KEY IDENTITY(1,1),
	Live bit NOT NULL ,
	TeamA text NOT NULL,
	TeamB text NOT NULL
	);