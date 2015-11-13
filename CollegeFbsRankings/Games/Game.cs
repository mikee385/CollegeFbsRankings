using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Games
{
    public enum eGameType
    {
        Fbs,
        Fcs
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

        eGameType Type { get; }
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
        private readonly eGameType _type;
        
        protected Game(int key, DateTime date, int week, Team homeTeam, Team awayTeam, string tv, string notes)
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
                    _type = eGameType.Fbs;
                else
                    _type = eGameType.Fcs;
            }
            else if (_awayTeam is FbsTeam)
                _type = eGameType.Fcs;
            else
            {
                throw new Exception(String.Format(
                    "Game {0} does not contain an FBS team: {1} vs. {2}",
                    _key, _homeTeam, _awayTeam));
            }
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

        public eGameType Type
        {
            get { return _type; }
        }
    }

    public static class GameExtensions
    {
        public static IEnumerable<T> Fbs<T>(this IEnumerable<T> games) where T : IGame
        {
            return games.Where(g => g.Type == eGameType.Fbs);
        }

        public static IEnumerable<T> Fcs<T>(this IEnumerable<T> games) where T : IGame
        {
            return games.Where(g => g.Type == eGameType.Fcs);
        }
    }
}
