using System;

namespace CollegeFbsRankings.Conferences
{
    public class DivisionID : IEquatable<DivisionID>
    {
        private readonly Guid _value;

        private DivisionID(Guid value)
        {
            _value = value;
        }

        public static DivisionID Create()
        {
            var id = Guid.NewGuid();
            return new DivisionID(id);
        }

        public static DivisionID FromExisting(Guid id)
        {
            return new DivisionID(id);
        }

        public Guid Value
        {
            get { return _value; }
        }

        public bool Equals(DivisionID other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            var id = obj as DivisionID;
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

        public static bool operator ==(DivisionID id1, DivisionID id2)
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

        public static bool operator !=(DivisionID id1, DivisionID id2)
        {
            return !(id1 == id2);
        }
    }
}
