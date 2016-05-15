using System;

namespace CollegeFbsRankings.Conferences
{
    public class ConferenceID : IEquatable<ConferenceID>
    {
        private readonly Guid _value;

        private ConferenceID(Guid value)
        {
            _value = value;
        }

        public static ConferenceID Create()
        {
            var id = Guid.NewGuid();
            return new ConferenceID(id);
        }

        public static ConferenceID FromExisting(Guid id)
        {
            return new ConferenceID(id);
        }

        public Guid Value
        {
            get { return _value; }
        }

        public bool Equals(ConferenceID other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            var id = obj as ConferenceID;
            if (id == null)
                return false;

            return Equals(id);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
