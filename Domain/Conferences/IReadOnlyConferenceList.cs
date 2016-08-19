using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain.Conferences
{
    public interface IReadOnlyConferenceList<out TValue> : IReadOnlyList<TValue> where TValue : Conference
    {
        IReadOnlyDictionary<ConferenceId, Conference> AsDictionary();
    }
}
