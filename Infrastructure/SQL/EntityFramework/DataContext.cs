using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Conferences;
using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Repositories;
using CollegeFbsRankings.Domain.Seasons;
using CollegeFbsRankings.Domain.Teams;

using CollegeFbsRankings.Infrastructure.Models;

using Conference = CollegeFbsRankings.Infrastructure.Models.Conference;
using Division = CollegeFbsRankings.Infrastructure.Models.Division;
using Game = CollegeFbsRankings.Infrastructure.Models.Game;
using Season = CollegeFbsRankings.Infrastructure.Models.Season;
using Team = CollegeFbsRankings.Infrastructure.Models.Team;

namespace CollegeFbsRankings.Infrastructure.Sql.EntityFramework
{
    public class DataContext : DbContext
    {
        public DataContext(DbConnection connection)
            : base(connection, true)
        {
            Database.SetInitializer<DataContext>(null);

            Validate();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Level>()
                .ToTable("Level")
                .HasKey(e => e.ID)
                .Property(e => e.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<GameStatus>()
                .ToTable("GameStatus")
                .HasKey(e => e.ID)
                .Property(e => e.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<SeasonType>()
                .ToTable("SeasonType")
                .HasKey(e => e.ID)
                .Property(e => e.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);


            modelBuilder.Entity<Season>()
                .ToTable("Season")
                .HasKey(e => e.ID)
                .Property(e => e.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<Conference>()
                .ToTable("Conference")
                .HasKey(e => e.ID)
                .Property(e => e.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<Division>()
                .ToTable("Division")
                .HasKey(e => e.ID)
                .Property(e => e.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<Team>()
                .ToTable("Team")
                .HasKey(e => e.ID)
                .Property(e => e.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<Game>()
                .ToTable("Game")
                .HasKey(e => e.ID)
                .Property(e => e.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);


            modelBuilder.Entity<TeamBySeason>()
                .ToTable("TeamBySeason")
                .HasKey(e => e.ID)
                .Property(e => e.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }

        public DbSet<Level> Levels { get; set; }

        public DbSet<GameStatus> GameStatuses { get; set; }

        public DbSet<SeasonType> SeasonTypes { get; set; }


        public DbSet<Season> Seasons { get; set; }

        public DbSet<Conference> Conferences { get; set; }

        public DbSet<Division> Divisions { get; set; }

        public DbSet<Team> Teams { get; set; }

        public DbSet<Game> Games { get; set; }


        public DbSet<TeamBySeason> TeamsBySeason { get; set; }

        private void Validate()
        {
            var invalidMessages = new List<string>();

            var validLevels = new[]
            {
                "FBS",
                "FCS"
            };
            foreach (var level in validLevels)
            {
                var dbLevels = Levels.Where(e => e.Value.Equals(level)).ToList();
                if (dbLevels.Count < 1)
                {
                    invalidMessages.Add(String.Format("The Level table is missing {0}", level));
                }
                else if (dbLevels.Count > 1)
                {
                    invalidMessages.Add(String.Format("The Level table contains multiple {0}", level));
                }
            }
            foreach (var dbLevel in Levels)
            {
                if (!validLevels.Contains(dbLevel.Value))
                {
                    invalidMessages.Add(String.Format("The Level table contains unexpected {0}", dbLevel.Value));
                }
            }

            var validGameStatuses = new[]
            {
                "COMPLETED",
                "FUTURE",
                "CANCELLED"
            };
            foreach (var gameStatus in validGameStatuses)
            {
                var dbGameStatuses = GameStatuses.Where(e => e.Value.Equals(gameStatus)).ToList();
                if (dbGameStatuses.Count < 1)
                {
                    invalidMessages.Add(String.Format("The GameStatus table is missing {0}", gameStatus));
                }
                else if (dbGameStatuses.Count > 1)
                {
                    invalidMessages.Add(String.Format("The GameStatus table contains multiple {0}", gameStatus));
                }
            }
            foreach (var dbGameStatus in GameStatuses)
            {
                if (!validGameStatuses.Contains(dbGameStatus.Value))
                {
                    invalidMessages.Add(String.Format("The GameStatus table contains unexpected {0}", dbGameStatus.Value));
                }
            }

            var validSeasonTypes = new[]
            {
                "REGULAR_SEASON",
                "POSTSEASON"
            };
            foreach (var seasonType in validSeasonTypes)
            {
                var dbSeasonTypes = SeasonTypes.Where(e => e.Value.Equals(seasonType)).ToList();
                if (dbSeasonTypes.Count < 1)
                {
                    invalidMessages.Add(String.Format("The SeasonType table is missing {0}", seasonType));
                }
                else if (dbSeasonTypes.Count > 1)
                {
                    invalidMessages.Add(String.Format("The SeasonType table contains multiple {0}", seasonType));
                }
            }
            foreach (var dbSeasonType in SeasonTypes)
            {
                if (!validSeasonTypes.Contains(dbSeasonType.Value))
                {
                    invalidMessages.Add(String.Format("The SeasonType table contains unexpected {0}", dbSeasonType.Value));
                }
            }

            if (invalidMessages.Any())
            {
                throw new InvalidOperationException(String.Join(Environment.NewLine, invalidMessages));
            }
        }

        public void ImportData(ICollegeFbsRepository repository)
        {
            // Seasons
            var dbSeasons = new Dictionary<SeasonID, Season>();
            foreach (var season in repository.Seasons.Execute())
            {
                var dbSeason = AddOrUpdateSeason(season);
                dbSeasons.Add(season.ID, dbSeason);

                var seasonRepository = repository.ForSeason(season);

                // Conferences
                var dbConferences = new Dictionary<ConferenceID, Conference>();
                foreach (var conference in seasonRepository.Conferences.Execute())
                {
                    var dbConference = AddOrUpdateConference(conference);
                    dbConferences.Add(conference.ID, dbConference);
                }

                // Divisions
                var dbDivisions = new Dictionary<DivisionID, Division>();
                foreach (var division in seasonRepository.Divisions.Execute())
                {
                    var dbConference = dbConferences[division.Conference.ID];
                    var dbDivision = AddOrUpdateDivision(division, dbConference);
                    dbDivisions.Add(division.ID, dbDivision);
                }

                // Teams
                var dbTeams = new Dictionary<TeamID, Team>();
                foreach (var team in seasonRepository.Teams.Execute())
                {
                    var dbTeam = AddOrUpdateTeam(team);
                    dbTeams.Add(team.ID, dbTeam);
                }

                // Games
                foreach (var game in seasonRepository.Games.Execute())
                {
                    var dbHomeTeam = dbTeams[game.HomeTeam.ID];
                    var dbAwayTeam = dbTeams[game.AwayTeam.ID];
                    AddOrUpdateGame(game, dbSeason, dbHomeTeam, dbAwayTeam);
                }

                //foreach (var game in seasonRepository.CancelledGames.Execute())
                //{
                //    var dbHomeTeam = dbTeams[game.HomeTeam.ID];
                //    var dbAwayTeam = dbTeams[game.AwayTeam.ID];
                //    AddOrUpdateGame(game, dbSeason, dbHomeTeam, dbAwayTeam);
                //}

                // Teams By Season
                var fbsConferences = seasonRepository.Conferences.Fbs().Execute();
                foreach (var conference in fbsConferences)
                {
                    var fbsDivisions = seasonRepository.Divisions.ForConference(conference.ID).Execute().ToList();
                    if (fbsDivisions.Any())
                    {
                        foreach (var division in fbsDivisions)
                        {
                            var fbsTeams = seasonRepository.Teams.ForDivision(division.ID).Execute();
                            foreach (var team in fbsTeams)
                            {
                                var dbConference = dbConferences[conference.ID];
                                var dbDivision = dbDivisions[division.ID];
                                var dbTeam = dbTeams[team.ID];

                                AddOrUpdateTeamBySeason("FBS", dbSeason, dbConference, dbDivision, dbTeam);
                            }
                        }
                    }
                    else
                    {
                        var fbsTeams = seasonRepository.Teams.ForConference(conference.ID).Execute();
                        foreach (var team in fbsTeams)
                        {
                            var dbConference = dbConferences[conference.ID];
                            var dbTeam = dbTeams[team.ID];

                            AddOrUpdateTeamBySeason("FBS", dbSeason, dbConference, null, dbTeam);
                        }
                    }
                }

                var fcsTeams = seasonRepository.Teams.Fcs().Execute();
                foreach (var team in fcsTeams)
                {
                    var dbTeam = dbTeams[team.ID];

                    AddOrUpdateTeamBySeason("FCS", dbSeason, null, null, dbTeam);
                }
            }

            SaveChanges();
        }

        private Season AddOrUpdateSeason(Domain.Seasons.Season season)
        {
            var dbSeasonList = Seasons.Where(e => e.Year == season.Year).ToList();
            if (dbSeasonList.Count > 1)
            {
                throw new InvalidOperationException(
                    String.Format("Found multiple seasons in the database for year: '{0}'", season.Year));
            }

            Season dbSeason;
            if (dbSeasonList.Count == 1)
            {
                dbSeason = dbSeasonList[0];
                UpdateSeason(dbSeason, season);
            }
            else
            {
                dbSeason = AddSeason(season);
            }

            return dbSeason;
        }

        private Season AddSeason(Domain.Seasons.Season season)
        {
            var dbSeason = new Season
            {
                GUID = season.ID.ToString(),
                Year = season.Year,
                NumWeeksInRegularSeason = season.NumWeeksInRegularSeason
            };
            Seasons.Add(dbSeason);

            return dbSeason;
        }

        private void UpdateSeason(Season existing, Domain.Seasons.Season updated)
        {
            existing.Year = updated.Year;
            existing.NumWeeksInRegularSeason = updated.NumWeeksInRegularSeason;
        }

        private Conference AddOrUpdateConference(Domain.Conferences.Conference conference)
        {
            var dbConferenceList = Conferences.Where(e => e.Name == conference.Name).ToList();
            if (dbConferenceList.Count > 1)
            {
                throw new InvalidOperationException(
                    String.Format("Found multiple conferences in the database with name: '{0}'", conference.Name));
            }

            Conference dbConference;
            if (dbConferenceList.Count == 1)
            {
                dbConference = dbConferenceList[0];
                UpdateConference(dbConference, conference);
            }
            else
            {
                dbConference = AddConference(conference);
            }

            return dbConference;
        }

        private Conference AddConference(Domain.Conferences.Conference conference)
        {
            var dbConference = new Conference
            {
                GUID = conference.ID.ToString(),
                Name = conference.Name
            };
            Conferences.Add(dbConference);

            return dbConference;
        }

        private void UpdateConference(Conference existing, Domain.Conferences.Conference updated)
        {
            existing.Name = updated.Name;
        }

        private Division AddOrUpdateDivision(Domain.Conferences.Division division, Conference dbConference)
        {
            var dbDivisionList = Divisions.Where(e => e.ConferenceGUID == dbConference.GUID && e.Name == division.Name).ToList();
            if (dbDivisionList.Count > 1)
            {
                throw new InvalidOperationException(
                    String.Format("Found multiple divisions for conference '{0}' in the database with name: '{1}'",
                    dbConference.Name, division.Name));
            }

            Division dbDivision;
            if (dbDivisionList.Count == 1)
            {
                dbDivision = dbDivisionList[0];
                UpdateDivision(dbDivision, division, dbConference);
            }
            else
            {
                dbDivision = AddDivision(division, dbConference);
            }

            return dbDivision;
        }

        private Division AddDivision(Domain.Conferences.Division division, Conference dbConference)
        {
            var dbDivision = new Division
            {
                GUID = division.ID.ToString(),
                ConferenceGUID = dbConference.GUID,
                Name = division.Name
            };
            Divisions.Add(dbDivision);

            return dbDivision;
        }

        private void UpdateDivision(Division existing, Domain.Conferences.Division updated, Conference dbConference)
        {
            existing.ConferenceGUID = dbConference.GUID;
            existing.Name = updated.Name;
        }

        private Team AddOrUpdateTeam(Domain.Teams.Team team)
        {
            var dbTeamList = Teams.Where(e => e.Name == team.Name).ToList();
            if (dbTeamList.Count > 1)
            {
                throw new InvalidOperationException(
                    String.Format("Found multiple teams in the database with name: '{0}'", team.Name));
            }

            Team dbTeam;
            if (dbTeamList.Count == 1)
            {
                dbTeam = dbTeamList[0];
                UpdateTeam(dbTeam, team);
            }
            else
            {
                dbTeam = AddTeam(team);
            }

            return dbTeam;
        }

        private Team AddTeam(Domain.Teams.Team team)
        {
            var dbTeam = new Team
            {
                GUID = team.ID.ToString(),
                Name = team.Name
            };
            Teams.Add(dbTeam);

            return dbTeam;
        }

        private void UpdateTeam(Team existing, Domain.Teams.Team updated)
        {
            existing.Name = updated.Name;
        }

        private Game AddOrUpdateGame(IGame game, Season dbSeason, Team dbHomeTeam, Team dbAwayTeam)
        {
            var dbGameList = Games.Where(e => e.SeasonGUID == dbSeason.GUID && e.Week == game.Week && e.HomeTeamGUID == dbHomeTeam.GUID && e.AwayTeamGUID == dbAwayTeam.GUID).ToList();
            if (dbGameList.Count > 1)
            {
                throw new InvalidOperationException(
                    String.Format("Found multiple games in the database for season '{0}' and week '{1}' between '{2}' and '{3}'",
                    dbSeason.Year, game.Week, dbHomeTeam.Name, dbAwayTeam.Name));
            }

            Game dbGame;
            if (dbGameList.Count == 1)
            {
                dbGame = dbGameList[0];
                UpdateGame(dbGame, game, dbSeason, dbHomeTeam, dbAwayTeam);
            }
            else
            {
                dbGame = AddGame(game, dbSeason, dbHomeTeam, dbAwayTeam);
            }

            return dbGame;
        }

        private Game AddGame(IGame game, Season dbSeason, Team dbHomeTeam, Team dbAwayTeam)
        {
            var dbGame = new Game
            {
                GUID = game.ID.ToString(),
                SeasonGUID = dbSeason.GUID,
                Week = game.Week,
                Level = (game.TeamType == eTeamType.Fbs) ? "FBS" : "FCS",
                SeasonType = (game.SeasonType == eSeasonType.RegularSeason) ? "REGULAR_SEASON" : "POSTSEASON",
                Status = (game is ICompletedGame) ? "COMPLETED" : "FUTURE",
                Date = game.Date,
                HomeTeamGUID = dbHomeTeam.GUID,
                AwayTeamGUID = dbAwayTeam.GUID,
                TV = game.TV,
                Notes = game.Notes
            };

            var completedGame = game as ICompletedGame;
            if (completedGame != null)
            {
                dbGame.HomeTeamScore = completedGame.HomeTeamScore;
                dbGame.AwayTeamScore = completedGame.AwayTeamScore;
            }
            else
            {
                dbGame.HomeTeamScore = null;
                dbGame.AwayTeamScore = null;
            }

            Games.Add(dbGame);

            return dbGame;
        }

        private void UpdateGame(Game existing, IGame updated, Season dbSeason, Team dbHomeTeam, Team dbAwayTeam)
        {
            existing.SeasonGUID = dbSeason.GUID;
            existing.Week = updated.Week;
            existing.Level = (updated.TeamType == eTeamType.Fbs) ? "FBS" : "FCS";
            existing.SeasonType = (updated.SeasonType == eSeasonType.RegularSeason) ? "REGULAR_SEASON" : "POSTSEASON";
            existing.Status = (updated is ICompletedGame) ? "COMPLETED" : "FUTURE";
            existing.Date = updated.Date;
            existing.HomeTeamGUID = dbHomeTeam.GUID;
            existing.AwayTeamGUID = dbAwayTeam.GUID;
            existing.TV = updated.TV;
            existing.Notes = updated.Notes;

            var completedGame = updated as ICompletedGame;
            if (completedGame != null)
            {
                existing.HomeTeamScore = completedGame.HomeTeamScore;
                existing.AwayTeamScore = completedGame.AwayTeamScore;
            }
            else
            {
                existing.HomeTeamScore = null;
                existing.AwayTeamScore = null;
            }
        }

        private TeamBySeason AddOrUpdateTeamBySeason(string level, Season dbSeason, Conference dbConference, Division dbDivision, Team dbTeam)
        {
            var dbTeamBySeasonList = TeamsBySeason.Where(e => e.Level == level && e.SeasonGUID == dbSeason.GUID && e.TeamGUID == dbTeam.GUID).ToList();
            if (dbTeamBySeasonList.Count > 1)
            {
                throw new InvalidOperationException(
                    String.Format("Found multiple teams '{1}' in the database for season '{0}'",
                    dbSeason.Year, dbTeam.Name));
            }

            TeamBySeason dbTeamBySeason;
            if (dbTeamBySeasonList.Count == 1)
            {
                dbTeamBySeason = dbTeamBySeasonList[0];
                UpdateTeamBySeason(dbTeamBySeason, level, dbSeason, dbConference, dbDivision, dbTeam);
            }
            else
            {
                dbTeamBySeason = AddTeamBySeason(level, dbSeason, dbConference, dbDivision, dbTeam);
            }

            return dbTeamBySeason;
        }

        private TeamBySeason AddTeamBySeason(string level, Season dbSeason, Conference dbConference, Division dbDivision, Team dbTeam)
        {
            var dbTeamBySeason = new TeamBySeason
            {
                Level = level,
                SeasonGUID = dbSeason.GUID,
                ConferenceGUID = (dbConference != null) ? dbConference.GUID : null,
                DivisionGUID = (dbDivision != null) ? dbDivision.GUID : null,
                TeamGUID = dbTeam.GUID
            };
            TeamsBySeason.Add(dbTeamBySeason);

            return dbTeamBySeason;
        }

        private void UpdateTeamBySeason(TeamBySeason existing, string level, Season dbSeason, Conference dbConference, Division dbDivision, Team dbTeam)
        {
            existing.Level = level;
            existing.SeasonGUID = dbSeason.GUID;
            existing.ConferenceGUID = (dbConference != null) ? dbConference.GUID : null;
            existing.DivisionGUID = (dbDivision != null) ? dbDivision.GUID : null;
            existing.TeamGUID = dbTeam.GUID;
        }
    }
}
