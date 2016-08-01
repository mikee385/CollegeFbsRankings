using System;

namespace CollegeFbsRankings.Domain.Teams
{
    public class TeamID : IEquatable<TeamID>
    {
        private readonly Guid _value;

        private TeamID(Guid value)
        {
            _value = value;
        }

        public static TeamID Create()
        {
            var id = Guid.NewGuid();
            return new TeamID(id);
        }

        public static TeamID FromExisting(Guid id)
        {
            return new TeamID(id);
        }

        public Guid Value
        {
            get { return _value; }
        }

        public bool Equals(TeamID other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            var id = obj as TeamID;
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

        public static bool operator ==(TeamID id1, TeamID id2)
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

        public static bool operator !=(TeamID id1, TeamID id2)
        {
            return !(id1 == id2);
        }
    }
}
