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
