using System;

namespace CollegeFbsRankings.Games
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
