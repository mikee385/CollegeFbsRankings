--
-- File generated with SQLiteStudio v3.1.0 on Mon Jul 25 09:23:13 2016
--
-- Text encoding used: System
--
PRAGMA foreign_keys = off;
BEGIN TRANSACTION;


-- Table: GameStatus
CREATE TABLE GameStatus (
    ID    INTEGER NOT NULL
                  PRIMARY KEY AUTOINCREMENT
                  UNIQUE,
    Value TEXT    NOT NULL
                  UNIQUE
);

INSERT INTO GameStatus (
                           ID,
                           Value
                       )
                       VALUES (
                           1,
                           'COMPLETED'
                       );

INSERT INTO GameStatus (
                           ID,
                           Value
                       )
                       VALUES (
                           2,
                           'FUTURE'
                       );

INSERT INTO GameStatus (
                           ID,
                           Value
                       )
                       VALUES (
                           3,
                           'CANCELLED'
                       );

-- Table: Level
CREATE TABLE Level (
    ID    INTEGER NOT NULL
                  PRIMARY KEY AUTOINCREMENT
                  UNIQUE,
    Value TEXT    NOT NULL
                  UNIQUE
);

INSERT INTO Level (
                      ID,
                      Value
                  )
                  VALUES (
                      1,
                      'FBS'
                  );

INSERT INTO Level (
                      ID,
                      Value
                  )
                  VALUES (
                      2,
                      'FCS'
                  );


-- Table: SeasonType
CREATE TABLE SeasonType (
    ID    INTEGER NOT NULL
                  PRIMARY KEY AUTOINCREMENT
                  UNIQUE,
    Value TEXT    NOT NULL
                  UNIQUE
);

INSERT INTO SeasonType (
                           ID,
                           Value
                       )
                       VALUES (
                           1,
                           'REGULAR_SEASON'
                       );

INSERT INTO SeasonType (
                           ID,
                           Value
                       )
                       VALUES (
                           2,
                           'POSTSEASON'
                       );


-- Table: Season
CREATE TABLE Season (
    ID                      INTEGER NOT NULL
                                    PRIMARY KEY AUTOINCREMENT
                                    UNIQUE,
    GUID                    TEXT    NOT NULL
                                    UNIQUE,
    Year                    INTEGER NOT NULL,
    NumWeeksInRegularSeason INTEGER NOT NULL
);


-- Table: Conference
CREATE TABLE Conference (
    ID   INTEGER NOT NULL
                 PRIMARY KEY AUTOINCREMENT
                 UNIQUE,
    GUID TEXT    NOT NULL
                 UNIQUE,
    Name TEXT    NOT NULL
);


-- Table: Division
CREATE TABLE Division (
    ID             INTEGER NOT NULL
                           PRIMARY KEY AUTOINCREMENT
                           UNIQUE,
    GUID           TEXT    NOT NULL
                           UNIQUE,
    ConferenceGUID TEXT    REFERENCES Conference (GUID) 
                           NOT NULL,
    Name           TEXT    NOT NULL
);


-- Table: Team
CREATE TABLE Team (
    ID   INTEGER NOT NULL
                 PRIMARY KEY AUTOINCREMENT
                 UNIQUE,
    GUID TEXT    NOT NULL
                 UNIQUE,
    Name TEXT    NOT NULL
);


-- Table: Game
CREATE TABLE Game (
    ID            INTEGER  NOT NULL
                           PRIMARY KEY AUTOINCREMENT
                           UNIQUE,
    GUID          TEXT     NOT NULL
                           UNIQUE,
    SeasonGUID    TEXT     REFERENCES Season (GUID) 
                           NOT NULL,
    Week          INTEGER  NOT NULL,
    Level         TEXT     NOT NULL
                           REFERENCES Level (Value),
    SeasonType    TEXT     NOT NULL
                           REFERENCES SeasonType (Value),
    Status        TEXT     NOT NULL
                           REFERENCES GameStatus (Value),
    Date          DATETIME NOT NULL,
    HomeTeamGUID  TEXT     NOT NULL
                           REFERENCES Team (GUID),
    HomeTeamScore INTEGER,
    AwayTeamGUID  TEXT     NOT NULL
                           REFERENCES Team (GUID),
    AwayTeamScore INTEGER,
    TV            TEXT     NOT NULL,
    Notes         TEXT     NOT NULL,
    FOREIGN KEY (
        Level
    )
    REFERENCES Level (Value),
    FOREIGN KEY (
        SeasonType
    )
    REFERENCES SeasonType (Value),
    FOREIGN KEY (
        Status
    )
    REFERENCES GameStatus (Value),
    FOREIGN KEY (
        HomeTeamGUID
    )
    REFERENCES Team (GUID),
    FOREIGN KEY (
        AwayTeamGUID
    )
    REFERENCES Team (GUID) 
);


-- Table: TeamBySeason
CREATE TABLE TeamBySeason (
    ID             INTEGER NOT NULL
                           PRIMARY KEY AUTOINCREMENT
                           UNIQUE,
    Level          TEXT    NOT NULL
                           REFERENCES Level (Value),
    SeasonGUID     TEXT    NOT NULL
                           REFERENCES Season (GUID),
    ConferenceGUID TEXT    REFERENCES Conference (GUID),
    DivisionGUID   TEXT    REFERENCES Division (GUID),
    TeamGUID       TEXT    NOT NULL
                           REFERENCES Team (GUID),
    FOREIGN KEY (
        Level
    )
    REFERENCES Level (Value),
    FOREIGN KEY (
        SeasonGUID
    )
    REFERENCES Season (GUID),
    FOREIGN KEY (
        TeamGUID
    )
    REFERENCES Team (GUID) 
);


-- View: ConferenceBySeason
CREATE VIEW ConferenceBySeason AS
    SELECT Level,
           SeasonGUID,
           ConferenceGUID
      FROM TeamBySeason
     WHERE ConferenceGUID IS NOT NULL
     GROUP BY Level,
              SeasonGUID,
              ConferenceGUID;


-- View: DivisionBySeason
CREATE VIEW DivisionBySeason AS
    SELECT Level,
           SeasonGUID,
           ConferenceGUID,
           DivisionGUID
      FROM TeamBySeason
     WHERE ConferenceGUID IS NOT NULL
     GROUP BY Level,
              SeasonGUID,
              ConferenceGUID,
              DivisionGUID;


-- View: DivisionDetails
CREATE VIEW DivisionDetails AS
    SELECT Division.ID,
           Division.GUID,
           Conference.Name AS ConferenceName,
           Division.Name AS DivisionName
      FROM Division
           INNER JOIN
           Conference ON Conference.GUID = Division.ConferenceGUID
     ORDER BY Conference.Name,
              Division.Name;


-- View: GameDetails
CREATE VIEW GameDetails AS
    SELECT Game.ID,
           Game.GUID,
           Season.Year AS Year,
           Game.Week,
           Game.Level,
           Game.SeasonType,
           Game.Status,
           Game.Date,
           HomeTeam.Name AS HomeTeamName,
           Game.HomeTeamScore,
           AwayTeam.Name AS AwayTeamName,
           Game.AwayTeamScore,
           Game.TV,
           Game.Notes
      FROM Game
           INNER JOIN
           Season ON Season.GUID = Game.SeasonGUID
           INNER JOIN
           Team AS HomeTeam ON HomeTeam.GUID = Game.HomeTeamGUID
           INNER JOIN
           Team AS AwayTeam ON AwayTeam.GUID = Game.AwayTeamGUID
     ORDER BY Season.Year,
              Game.Week,
              Game.Date;


-- View: ConferenceBySeasonDetails
CREATE VIEW ConferenceBySeasonDetails AS
    SELECT ConferenceBySeason.Level,
           Season.Year AS Year,
           Conference.Name AS ConferenceName
      FROM ConferenceBySeason
           INNER JOIN
           Season ON Season.GUID = ConferenceBySeason.SeasonGUID
           INNER JOIN
           Conference ON Conference.GUID = ConferenceBySeason.ConferenceGUID
     ORDER BY Season.Year,
              ConferenceBySeason.Level,
              Conference.Name;


-- View: DivisionBySeasonDetails
CREATE VIEW DivisionBySeasonDetails AS
    SELECT DivisionBySeason.Level,
           Season.Year AS Year,
           Conference.Name AS ConferenceName,
           Division.Name AS DivisionName
      FROM DivisionBySeason
           INNER JOIN
           Season ON Season.GUID = DivisionBySeason.SeasonGUID
           LEFT OUTER JOIN
           Conference ON Conference.GUID = DivisionBySeason.ConferenceGUID
           LEFT OUTER JOIN
           Division ON Division.GUID = DivisionBySeason.DivisionGUID
     ORDER BY Season.Year,
              DivisionBySeason.Level,
              Conference.Name,
              Division.Name;


-- View: TeamBySeasonDetails
CREATE VIEW TeamBySeasonDetails AS
    SELECT TeamBySeason.ID,
           TeamBySeason.Level,
           Season.Year AS Year,
           Conference.Name AS ConferenceName,
           Division.Name AS DivisionName,
           Team.Name AS TeamName
      FROM TeamBySeason
           INNER JOIN
           Season ON Season.GUID = TeamBySeason.SeasonGUID
           LEFT OUTER JOIN
           Conference ON Conference.GUID = TeamBySeason.ConferenceGUID
           LEFT OUTER JOIN
           Division ON Division.GUID = TeamBySeason.DivisionGUID
           INNER JOIN
           Team ON Team.GUID = TeamBySeason.TeamGUID
     ORDER BY Season.Year,
              TeamBySeason.Level,
              Conference.Name,
              Division.Name,
              Team.Name;


COMMIT TRANSACTION;
PRAGMA foreign_keys = on;
