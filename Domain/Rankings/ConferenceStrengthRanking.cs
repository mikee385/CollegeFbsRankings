using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Conferences;

namespace CollegeFbsRankings.Domain.Rankings
{
    public class ConferenceStrengthRankingValue : ConferenceRankingValue
    {
        private readonly int _gametotal;
        private readonly int _wintotal;

        private readonly double _conferenceStrength;
        private readonly double _winPercentage;
        private readonly double _winStrength;

        private readonly IEnumerable<double> _values;

        public ConferenceStrengthRankingValue(ConferenceId id, string name, int gameTotal, int winTotal, double conferenceStrength, double winStrength)
            : base(id, name)
        {
            _gametotal = gameTotal;
            _wintotal = winTotal;

            _conferenceStrength = conferenceStrength;
            _winPercentage = gameTotal > 0 ? (double) winTotal / gameTotal : 0.0;
            _winStrength = winStrength;

            _values = new[] { _conferenceStrength, _winPercentage, _winStrength };
        }

        public int GameTotal
        {
            get { return _gametotal; }
        }

        public int WinTotal
        {
            get { return _wintotal; }
        }

        public double ConferenceStrength
        {
            get { return _conferenceStrength; }
        }

        public double WinPercentage
        {
            get { return _winPercentage; }
        }

        public double WinStrength
        {
            get { return _winStrength; }
        }

        public override IEnumerable<double> Values
        {
            get { return _values; }
        }
    }

    public class ConferenceStrengthRanking : ConferenceRanking<ConferenceStrengthRankingValue>
    {
        public ConferenceStrengthRanking(IEnumerable<KeyValuePair<ConferenceId, ConferenceStrengthRankingValue>> data)
            : base(data)
        { }

        public ConferenceStrengthRanking ForConferences(ICollection<ConferenceId> conferenceIds)
        {
            return new ConferenceStrengthRanking(this.Where(rank => conferenceIds.Contains(rank.Key)));
        }
    }
}
