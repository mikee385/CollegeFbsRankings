using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Rankings;
using CollegeFbsRankings.Domain.Teams;
using CollegeFbsRankings.Domain.Validations;

namespace CollegeFbsRankings.UI.Formatters
{
    public class YearSummary
    {
        private readonly IReadOnlyList<FbsTeam> _fbsTeams;
        private readonly IReadOnlyList<FcsTeam> _fcsTeams;
        private readonly IReadOnlyList<Game> _games;
        private readonly IReadOnlyList<Game> _cancelledGames;

        private readonly List<RankingSummary> _rankingSummaries;

        public class RankingSummary
        {
            private readonly string _name;
            private readonly PerformanceRanking _performance;
            private readonly Validation<GameId> _validation;
            private readonly Validation<GameId> _prediction;

            public RankingSummary(
                string name,
                PerformanceRanking performance,
                Validation<GameId> validation,
                Validation<GameId> prediction)
            {
                _name = name;
                _performance = performance;
                _validation = validation;
                _prediction = prediction;
            }

            public string Name
            {
                get { return _name; }
            }

            public PerformanceRanking Performance
            {
                get { return _performance; }
            }

            public Validation<GameId> Validation
            {
                get { return _validation; }
            }

            public Validation<GameId> Prediction
            {
                get { return _prediction; }
            }
        }

        public YearSummary(
            IReadOnlyList<FbsTeam> fbsTeams, 
            IReadOnlyList<FcsTeam> fcsTeams, 
            IReadOnlyList<Game> games, 
            IReadOnlyList<Game> cancelledGames)
        {
            _fbsTeams = fbsTeams;
            _fcsTeams = fcsTeams;
            _games = games;
            _cancelledGames = cancelledGames;

            _rankingSummaries = new List<RankingSummary>();
        }

        public IReadOnlyList<FbsTeam> FbsTeams
        {
            get { return _fbsTeams; }
        }

        public IReadOnlyList<FcsTeam> FcsTeams
        {
            get { return _fcsTeams; }
        }

        public IReadOnlyList<Game> Games
        {
            get { return _games; }
        }

        public IReadOnlyList<Game> CancelledGames
        {
            get { return _cancelledGames; }
        }

        public IReadOnlyList<RankingSummary> RankingSummaries
        {
            get { return _rankingSummaries; }
        }

        public void AddRankingSummary(
            string name,
            PerformanceRanking performance,
            Validation<GameId> validation,
            Validation<GameId> prediction)
        {
            _rankingSummaries.Add(new RankingSummary(name, performance, validation, prediction));
        }

        public static void Format(TextWriter writer, int year, IReadOnlyDictionary<TeamId, Team> teamMap, YearSummary summary)
        {
            writer.WriteLine("{0} Results", year);
            writer.WriteLine("---------------------------------");
            writer.WriteLine();

            writer.WriteLine("Number of FBS Teams = {0}", summary.FbsTeams.Count);
            writer.WriteLine("Number of FCS Teams = {0}", summary.FcsTeams.Count);
            writer.WriteLine();

            var completedGames = summary.Games.Completed().ToList();
            var futureGames = summary.Games.Future().ToList();

            writer.WriteLine("Number of Completed Games = {0}", completedGames.Count);
            writer.WriteLine("Number of Future Games = {0}", futureGames.Count);
            writer.WriteLine();

            var regularSeasonGames = completedGames.RegularSeason().ToList();
            var postseasonGames = summary.Games.Postseason().ToList();

            writer.WriteLine("Number of FBS  Games = {0}", regularSeasonGames.Fbs().Count());
            writer.WriteLine("Number of FCS  Games = {0}", regularSeasonGames.Fcs().Count());
            writer.WriteLine("Number of Bowl Games = {0}", postseasonGames.Count);
            writer.WriteLine();

            if (summary.CancelledGames.Any())
            {
                writer.WriteLine("WARNING: Potentially cancelled games were removed:");
                foreach (var game in summary.CancelledGames)
                {
                    var homeTeam = teamMap[game.HomeTeamId];
                    var awayTeam = teamMap[game.AwayTeamId];

                    writer.WriteLine("    Week {0,-2} {1} vs. {2} - {3}",
                        game.Week,
                        homeTeam.Name,
                        awayTeam.Name,
                        game.Notes);
                }
                writer.WriteLine();
            }

            var maxTitleLength = summary.RankingSummaries.Any() ? summary.RankingSummaries.Max(s => s.Name.Length) : 0;

            foreach (var method in summary.RankingSummaries)
            {
                var validationCorrect = method.Validation.Count(r => r.Value == eValidationResult.Correct);
                var validationIncorrect = method.Validation.Count(r => r.Value == eValidationResult.Incorrect);
                var validationPercent = (double)validationCorrect / (validationCorrect + validationIncorrect);
                writer.WriteLine("Regular Season Retrodiction for {0,-" + maxTitleLength + "} = {1:F8} %", method.Name, validationPercent);
            }
            writer.WriteLine();

            foreach (var method in summary.RankingSummaries)
            {
                var predictionCorrect = method.Prediction.Count(r => r.Value == eValidationResult.Correct);
                var predictionIncorrect = method.Prediction.Count(r => r.Value == eValidationResult.Incorrect);
                var predictionPercent = (double)predictionCorrect / (predictionCorrect + predictionIncorrect);
                writer.WriteLine("Postseason Prediction for {0,-" + maxTitleLength + "} = {1:F8} %", method.Name, predictionPercent);
            }
            writer.WriteLine();

            foreach (var method in summary.RankingSummaries)
            {
                var title = String.Format("Top 5 for {0}:", method.Name);

                var top5Teams = method.Performance
                    .Where(rank => rank.Key is FbsTeamId)
                    .Take(5)
                    .Select(rank => rank.Key)
                    .ToList();
                var top5PerformanceRanking = method.Performance.ForTeams(top5Teams);

                FormatRanking(writer, title, teamMap, top5PerformanceRanking);
                writer.WriteLine();
            }
            writer.WriteLine();
        }

        private static void FormatRanking(
            TextWriter writer,
            string title, 
            IReadOnlyDictionary<TeamId, Team> teamMap,
            PerformanceRanking ranking)
        {
            writer.WriteLine(title);
            writer.WriteLine("--------------------");

            // Calculate the formatting information for the titles.
            var maxTitleLength = ranking.Max(rank => teamMap[rank.Key].Name.Length);

            // Output the rankings.
            int index = 1, outputIndex = 1;
            List<double> previousValues = null;

            foreach (var rank in ranking)
            {
                var currentValues = rank.Value.Values.ToList();
                if (index != 1)
                {
                    if (!currentValues.SequenceEqual(previousValues))
                        outputIndex = index;
                }

                var teamName = teamMap[rank.Key].Name;
                var titleInfo = String.Format("{0,-4} {1,-" + (maxTitleLength + 3) + "}", outputIndex, teamName);
                var rankingInfo = String.Join("   ", currentValues.Select(value => String.Format("{0:F8}", value)));

                writer.WriteLine(String.Join(" ", titleInfo, rankingInfo));

                ++index;
                previousValues = currentValues;
            }
        }
    }
}
