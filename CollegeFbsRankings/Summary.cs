using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CollegeFbsRankings.Games;
using CollegeFbsRankings.Rankings;
using CollegeFbsRankings.Teams;
using CollegeFbsRankings.Validations;

namespace CollegeFbsRankings
{
    public class Summary
    {
        private readonly IReadOnlyList<FbsTeam> _fbsTeams;
        private readonly IReadOnlyList<FcsTeam> _fcsTeams;
        private readonly IReadOnlyList<IGame> _games;
        private readonly IReadOnlyList<IGame> _cancelledGames;
        private readonly Dictionary<string, Method> _methodSummaries;

        public class Method
        {
            private readonly Ranking<TeamRankingValue> _ranking;
            private readonly IReadOnlyList<Validation.GameValue> _validation;
            private readonly IReadOnlyList<Validation.GameValue> _prediction;

            public Method(Ranking<TeamRankingValue> ranking, IReadOnlyList<Validation.GameValue> validation, IReadOnlyList<Validation.GameValue> prediction)
            {
                _ranking = ranking;
                _validation = validation;
                _prediction = prediction;
            }

            public Ranking<TeamRankingValue> Ranking
            {
                get { return _ranking; }
            }

            public IReadOnlyList<Validation.GameValue> Validation
            {
                get { return _validation; }
            }

            public IReadOnlyList<Validation.GameValue> Prediction
            {
                get { return _prediction; }
            }
        }

        public Summary(IReadOnlyList<FbsTeam> fbsTeams, IReadOnlyList<FcsTeam> fcsTeams, IReadOnlyList<IGame> games, IReadOnlyList<IGame> cancelledGames)
        {
            _fbsTeams = fbsTeams;
            _fcsTeams = fcsTeams;
            _games = games;
            _cancelledGames = cancelledGames;

            _methodSummaries = new Dictionary<string, Method>();
        }

        public IReadOnlyList<FbsTeam> FbsTeams
        {
            get { return _fbsTeams; }
        }

        public IReadOnlyList<FcsTeam> FcsTeams
        {
            get { return _fcsTeams; }
        }

        public IReadOnlyList<IGame> Games
        {
            get { return _games; }
        }

        public IReadOnlyList<IGame> CancelledGames
        {
            get { return _cancelledGames; }
        }

        public IReadOnlyDictionary<string, Method> MethodSummaries
        {
            get { return _methodSummaries; }
        }

        public void AddMethodSummary(string name, Ranking<TeamRankingValue> ranking, 
            IReadOnlyList<Validation.GameValue> validation, IReadOnlyList<Validation.GameValue> prediction)
        {
            _methodSummaries.Add(name, new Method(ranking, validation, prediction));
        }

        public static string Format(int year, Summary summary)
        {
            var writer = new StringWriter();

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
                    writer.WriteLine("    Week {0,-2} {1} vs. {2} - {3}",
                        game.Week,
                        game.HomeTeam.Name,
                        game.AwayTeam.Name,
                        game.Notes);
                }
                writer.WriteLine();
            }

            if (summary.MethodSummaries.Any())
            {
                var maxTitleLength = summary.MethodSummaries.Max(s => s.Key.Length);

                foreach (var method in summary.MethodSummaries)
                {
                    var validationCorrect = method.Value.Validation.Count(r => r.Result == eValidationResult.Correct);
                    var validationIncorrect = method.Value.Validation.Count(r => r.Result == eValidationResult.Incorrect);
                    var validationPercent = (double)validationCorrect / (validationCorrect + validationIncorrect);
                    writer.WriteLine("Regular Season Retrodiction for {0,-" + maxTitleLength + "} = {1:F8} %", method.Key, validationPercent);
                }
                writer.WriteLine();

                foreach (var method in summary.MethodSummaries)
                {
                    var predictionCorrect = method.Value.Prediction.Count(r => r.Result == eValidationResult.Correct);
                    var predictionIncorrect = method.Value.Prediction.Count(r => r.Result == eValidationResult.Incorrect);
                    var predictionPercent = (double)predictionCorrect / (predictionCorrect + predictionIncorrect);
                    writer.WriteLine("Postseason Prediction for {0,-" + maxTitleLength + "} = {1:F8} %", method.Key, predictionPercent);
                }
                writer.WriteLine();

                foreach (var method in summary.MethodSummaries)
                {
                    var title = String.Format("Top 5 for {0}:", method.Key);
                    var top5 = method.Value.Ranking.Top(5);

                    writer.WriteLine(top5.Format(title));
                    writer.WriteLine();
                }
                writer.WriteLine();
            }
            
            return writer.ToString();
        }
    }
}
