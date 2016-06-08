using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Games
{
    public interface ITeamFutureGame : ITeamGame, IFutureGame
    { }

    public class TeamFutureGame : TeamGame, ITeamFutureGame
    {
        protected TeamFutureGame(Team team, IFutureGame game)
            : base(team, game)
        { }

        public static ITeamFutureGame Create(Team team, IFutureGame game)
        {
            return new TeamFutureGame(team, game);
        }
    }

    public static class TeamFutureGameExtensions
    {
        public static IEnumerable<ITeamFutureGame> Future(this IEnumerable<ITeamGame> games)
        {
            return games.OfType<ITeamFutureGame>();
        }
    }
}
