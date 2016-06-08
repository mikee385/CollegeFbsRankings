using System;

namespace CollegeFbsRankings.Seasons
{
    public class SeasonID : IEquatable<SeasonID>
    {
        private readonly Guid _value;

        private SeasonID(Guid value)
        {
            _value = value;
        }

        public static SeasonID Create()
        {
            var id = Guid.NewGuid();
            return new SeasonID(id);
        }

        public static SeasonID FromExisting(Guid id)
        {
            return new SeasonID(id);
        }

        public Guid Value
        {
            get { return _value; }
        }

        public bool Equals(SeasonID other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            var id = obj as SeasonID;
            if (ReferenceEquals(id, null))
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

        public static bool operator ==(SeasonID id1, SeasonID id2)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(id1, id2))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (ReferenceEquals(id1, null) || ReferenceEquals(id2, null))
            {
                return false;
            }

            // Return true if the fields match.
            return id1.Equals(id2);
        }

        public static bool operator !=(SeasonID id1, SeasonID id2)
        {
            return !(id1 == id2);
        }
    }
}
