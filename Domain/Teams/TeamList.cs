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
        private readonly CovariantDictionaryWrapper<TeamId, TValue, Team> _dictionary;

        public TeamList()
        {
            _dictionary = new CovariantDictionaryWrapper<TeamId, TValue, Team>(Dictionary);
        }

        public TeamList(IEnumerable<TValue> teams)
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
            return _dictionary;
        }
    }
}
