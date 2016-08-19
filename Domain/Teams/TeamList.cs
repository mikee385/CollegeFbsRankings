using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain.Teams
{
    public class TeamList<TValue> : KeyedCollection<TeamId, TValue>, ITeamList<TValue> where TValue : Team
    {
        private CovariantDictionaryWrapper<TeamId, TValue, Team> _dictionary;

        public TeamList()
        {
            _dictionary = null;
        }

        public TeamList(IEnumerable<TValue> teams)
            : this()
        {
            foreach (var team in teams)
            {
                Add(team);
            }
        }

        protected override TeamId GetKeyForItem(TValue team)
        {
            return team.Id;
        }

        public IReadOnlyDictionary<TeamId, Team> AsDictionary()
        {
            if (_dictionary == null && Dictionary != null)
            {
                _dictionary = new CovariantDictionaryWrapper<TeamId, TValue, Team>(Dictionary);
            }
            return _dictionary;
        }
    }
}
