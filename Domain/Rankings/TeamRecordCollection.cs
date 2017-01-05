using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Rankings
{
    public class TeamRecord
    {
        public TeamRecord()
        {
            GameTotal = 0;
            WinTotal = 0;
        }
        
        public int GameTotal { get; private set; }
        public int WinTotal { get; private set; }

        public double WinPercentage
        {
            get { return (GameTotal > 0) ? (double)WinTotal / GameTotal : 0.0; }
        }

        internal void AddWin()
        {
            GameTotal += 1;
            WinTotal += 1;
        }

        internal void AddLoss()
        {
            GameTotal += 1;
        }
    }

    public class TeamRecordCollection : IReadOnlyDictionary<TeamId, TeamRecord>
    {
        private readonly IReadOnlyDictionary<TeamId, TeamRecord> _dict;

        public TeamRecordCollection(
            IReadOnlyDictionary<TeamId, Team> teamMap,
            IReadOnlyCollection<CompletedGame> games)
        {
            var teamRecord = teamMap.ToDictionary(team => team.Key, team => new TeamRecord());
            foreach (var game in games)
            {
                var winningTeamRecord = teamRecord[game.WinningTeamId];
                var losingTeamRecord = teamRecord[game.LosingTeamId];

                winningTeamRecord.AddWin();
                losingTeamRecord.AddLoss();
            }
            _dict = teamRecord;
        }

        public TeamRecord this[TeamId key]
        {
            get { return _dict[key]; }
        }

        public int Count
        {
            get { return _dict.Count; }
        }

        public IEnumerable<TeamId> Keys
        {
            get { return _dict.Keys; }
        }

        public IEnumerable<TeamRecord> Values
        {
            get { return _dict.Values; }
        }

        public bool ContainsKey(TeamId key)
        {
            return _dict.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<TeamId, TeamRecord>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        public bool TryGetValue(TeamId key, out TeamRecord value)
        {
            return _dict.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_dict).GetEnumerator();
        }
    }
}
