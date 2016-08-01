using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Repositories;

using CollegeFbsRankings.Infrastructure.Csv;
using CollegeFbsRankings.Infrastructure.SQL.EntityFramework;

namespace CollegeFbsRankings.Application.DataImport
{
    public class Program
    {
        private static readonly Dictionary<int, int> Seasons = new Dictionary<int, int>
        {
            {1998, 15},
            {1999, 15},
            {2000, 15},
            {2001, 15},
            {2002, 16},
            {2003, 16},
            {2004, 15},
            {2005, 14},
            {2006, 14},
            {2007, 14},
            {2008, 15},
            {2009, 15},
            {2010, 15},
            {2011, 15},
            {2012, 15},
            {2013, 16},
            {2014, 16},
            {2015, 15}
        };

        private const string DataFolder = @"..\..\..\Data";

        private const string ConnectionStringKey = "CollegeFbsData";

        public static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                throw new ArgumentException("Expected a single value for the year, but found: '{0}'", String.Join(", ", args));
            }

            if (args.Length == 1)
            {
                int year;
                if (!Int32.TryParse(args[0], out year))
                {
                    throw new ArgumentException(String.Format("Invalid value for year: '{0}'", args[0]));
                }
                
                Console.WriteLine("Reading input data for {0}...", year);
                var repository = ReadDataForYear(year);
                
                Console.WriteLine("Writing data to database for {0}...", year);
                WriteToDatabase(repository);
            }
            else
            {
                foreach (var year in Seasons.Keys)
                {
                    Console.WriteLine("Reading input data for {0}...", year);
                    var repository = ReadDataForYear(year);

                    Console.WriteLine("Writing data to database for {0}...", year);
                    WriteToDatabase(repository);
                }
            }
        }

        private static ICollegeFbsRepository ReadDataForYear(int year)
        {
            int numWeeksInRegularSeason;
            if (!Seasons.TryGetValue(year, out numWeeksInRegularSeason))
            {
                throw new ArgumentException(String.Format("Data is not available for year '{0}'", year), "year");
            }

            var yearString = Convert.ToString(year);
            var fbsTeamFileName = Path.Combine(DataFolder, yearString, "FBS Teams.txt");
            var gameFileName = Path.Combine(DataFolder, yearString, "FBS Scores.txt");

            var fbsTeamFile = new StreamReader(fbsTeamFileName);
            var gameFile = new StreamReader(gameFileName);

            var repository = new CsvRepository();
            repository.AddCsvData(year, numWeeksInRegularSeason, fbsTeamFile, gameFile);
            return repository;
        }

        private static void WriteToDatabase(ICollegeFbsRepository repository)
        {
            var configuration = ConfigurationManager.ConnectionStrings[ConnectionStringKey];
            var connectionString = configuration.ConnectionString;
            var providerFactory = DbProviderFactories.GetFactory(configuration.ProviderName);

            using (var connection = providerFactory.CreateConnection())
            {
                if (connection == null)
                {
                    throw new Exception(String.Format("Unable to establish a connection to '{0}' using '{1}'",
                        configuration.ProviderName, configuration.ConnectionString));
                }

                connection.ConnectionString = connectionString;
                connection.Open();

                using (var db = new DataContext(connection))
                {
                    db.ImportData(repository);
                }
            }
        }
    }
}
