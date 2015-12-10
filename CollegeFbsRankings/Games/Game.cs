using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
        int Key { get; }

        DateTime Date { get; }

        int Week { get; }

        Team HomeTeam { get; }

        Team AwayTeam { get; }

        string TV { get; }

        string Notes { get; }

        eTeamType TeamType { get; }

        eSeasonType SeasonType { get; }
    }

    public abstract class Game : IGame
    {
        private readonly int _key;
        private readonly DateTime _date;
        private readonly int _week;
        private readonly Team _homeTeam;
        private readonly Team _awayTeam;
        private readonly string _tv;
        private readonly string _notes;
        private readonly eTeamType _teamType;
        private readonly eSeasonType _seasonType;
        
        protected Game(int key, DateTime date, int week, Team homeTeam, Team awayTeam, string tv, string notes, eSeasonType seasonType)
        {
            _key = key;
            _date = date;
            _week = week;
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
                    "Game {0} does not contain an FBS team: {1} vs. {2}",
                    _key, _homeTeam, _awayTeam));
            }

            _seasonType = seasonType;
        }

        public int Key
        {
            get { return _key; }
        }

        public DateTime Date
        {
            get { return _date; }
        }

        public int Week
        {
            get { return _week; }
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
