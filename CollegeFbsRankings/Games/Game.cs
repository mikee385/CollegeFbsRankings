using System;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Seasons;
using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Games
{
    public enum eTeamType
    {
        Fbs,
        Fcs
    }

    public enum eSeasonType
    {
        RegularSeason,
        PostSeason
    }

    public interface IGame
    {
        GameID ID { get; }

        Season Season { get; }

        int Week { get; }

        DateTime Date { get; }

        Team HomeTeam { get; }

        Team AwayTeam { get; }

        string TV { get; }

        string Notes { get; }

        eTeamType TeamType { get; }

        eSeasonType SeasonType { get; }
    }

    public abstract class Game : IGame
    {
        private readonly GameID _id;
        private readonly Season _season;
        private readonly int _week;
        private readonly DateTime _date;
        private readonly Team _homeTeam;
        private readonly Team _awayTeam;
        private readonly string _tv;
        private readonly string _notes;
        private readonly eTeamType _teamType;
        private readonly eSeasonType _seasonType;

        protected Game(GameID id, Season season, int week, DateTime date, Team homeTeam, Team awayTeam, string tv, string notes, eSeasonType seasonType)
        {
            _id = id;
            _season = season;
            _week = week;
            _date = date;
            _homeTeam = homeTeam;
            _awayTeam = awayTeam;
            _tv = tv;
            _notes = notes;

            if (_homeTeam is FbsTeam)
            {
                if (_awayTeam is FbsTeam)
                    _teamType = eTeamType.Fbs;
                else
                    _teamType = eTeamType.Fcs;
            }
            else if (_awayTeam is FbsTeam)
                _teamType = eTeamType.Fcs;
            else
            {
                throw new Exception(String.Format(
                    "Game does not contain an FBS team: {0} vs. {1}",
                    _homeTeam.Name, _awayTeam.Name));
            }

            _seasonType = seasonType;
        }

        public GameID ID
        {
            get { return _id; }
        }

        public Season Season
        {
            get { return _season; }
        }

        public int Week
        {
            get { return _week; }
        }

        public DateTime Date
        {
            get { return _date; }
        }

        public Team HomeTeam
        {
            get { return _homeTeam; }
        }

        public Team AwayTeam
        {
            get { return _awayTeam; }
        }

        public string TV
        {
            get { return _tv; }
        }

        public string Notes
        {
            get { return _notes; }
        }

        public eTeamType TeamType
        {
            get { return _teamType; }
        }

        public eSeasonType SeasonType
        {
            get { return _seasonType; }
        }
    }

    public static class GameExtensions
    {
        public static IEnumerable<T> Fbs<T>(this IEnumerable<T> games) where T : IGame
        {
            return games.Where(g => g.TeamType == eTeamType.Fbs);
        }

        public static IEnumerable<T> Fcs<T>(this IEnumerable<T> games) where T : IGame
        {
            return games.Where(g => g.TeamType == eTeamType.Fcs);
        }
        public static IEnumerable<T> RegularSeason<T>(this IEnumerable<T> games) where T : IGame
        {
            return games.Where(g => g.SeasonType == eSeasonType.RegularSeason);
        }

        public static IEnumerable<T> PostSeason<T>(this IEnumerable<T> games) where T : IGame
        {
            return games.Where(g => g.SeasonType == eSeasonType.PostSeason);
        }
    }
}
