using System;

namespace CollegeFbsRankings.Teams
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
