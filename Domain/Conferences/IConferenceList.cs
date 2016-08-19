using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain.Conferences
{
    public interface IConferenceList<TValue> : IList<TValue>, IReadOnlyConferenceList<TValue> where TValue : Conference
    { }
}
