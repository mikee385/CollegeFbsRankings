using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain
{
    public abstract class Identifier<T> : IEquatable<Identifier<T>>
    {
        private readonly Guid _value;

        protected Identifier(Guid value)
        {
            _value = value;
        }

        public Guid Value
        {
            get { return _value; }
        }

        public bool Equals(Identifier<T> other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            var id = obj as Identifier<T>;
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

        public static bool operator ==(Identifier<T> id1, Identifier<T> id2)
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

        public static bool operator !=(Identifier<T> id1, Identifier<T> id2)
        {
            return !(id1 == id2);
        }
    }
}
