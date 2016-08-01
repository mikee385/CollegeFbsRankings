using System;

namespace CollegeFbsRankings.Domain.Games
{
    public class GameID : IEquatable<GameID>
    {
        private readonly Guid _value;

        private GameID(Guid value)
        {
            _value = value;
        }

        public static GameID Create()
        {
            var id = Guid.NewGuid();
            return new GameID(id);
        }

        public static GameID FromExisting(Guid id)
        {
            return new GameID(id);
        }

        public Guid Value
        {
            get { return _value; }
        }

        public bool Equals(GameID other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            var id = obj as GameID;
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

        public static bool operator ==(GameID id1, GameID id2)
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

        public static bool operator !=(GameID id1, GameID id2)
        {
            return !(id1 == id2);
        }
    }
}
