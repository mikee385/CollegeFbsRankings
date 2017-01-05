using System;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Domain.Seasons;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Games
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

    public class GameId : Identifier<Game>
    {
        protected GameId(Guid id)
            : base(id)
        { }

        public static GameId Create()
        {
            var id = Guid.NewGuid();
            return new GameId(id);
        }

        public static GameId FromExisting(Guid id)
        {
            return new GameId(id);
        }
    }

    public abstract class Game
    {
        private readonly GameId _id;
        private readonly Season _season;
        private readonly int _week;
        private readonly DateTime _date;
        private readonly TeamId _homeTeamId;
        private readonly TeamId _awayTeamId;
        private readonly string _tv;
        private readonly string _notes;
        private readonly eTeamType _teamType;
        private readonly eSeasonType _seasonType;

        protected Game(GameId id, Season season, int week, DateTime date, TeamId homeTeamId, TeamId awayTeamId, string tv, string notes, eSeasonType seasonType)
        {
            _id = id;
            _season = season;
            _week = week;
            _date = date;
            _homeTeamId = homeTeamId;
            _awayTeamId = awayTeamId;
            _tv = tv;
            _notes = notes;

            if (_homeTeamId is FbsTeamId)
            {
                if (_awayTeamId is FbsTeamId)
                    _teamType = eTeamType.Fbs;
                else
                    _teamType = eTeamType.Fcs;
            }
            else if (_awayTeamId is FbsTeamId)
                _teamType = eTeamType.Fcs;
            else
            {
                throw new Exception(String.Format(
                    "Game does not contain an FBS team: {0}",
                    id));
            }

            _seasonType = seasonType;
        }

        public GameId Id
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

        public TeamId HomeTeamId
        {
            get { return _homeTeamId; }
        }

        public TeamId AwayTeamId
        {
            get { return _awayTeamId; }
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
        public static IEnumerable<T> Fbs<T>(this IEnumerable<T> games) where T : Game
        {
            return games.Where(g => g.TeamType == eTeamType.Fbs);
        }

        public static IEnumerable<T> Fcs<T>(this IEnumerable<T> games) where T : Game
        {
            return games.Where(g => g.TeamType == eTeamType.Fcs);
        }
        public static IEnumerable<T> RegularSeason<T>(this IEnumerable<T> games) where T : Game
        {
            return games.Where(g => g.SeasonType == eSeasonType.RegularSeason);
        }

        public static IEnumerable<T> Postseason<T>(this IEnumerable<T> games) where T : Game
        {
            return games.Where(g => g.SeasonType == eSeasonType.PostSeason);
        }
    }
}
