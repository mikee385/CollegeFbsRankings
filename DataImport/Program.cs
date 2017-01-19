using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain;
using CollegeFbsRankings.Domain.Repositories;

using CollegeFbsRankings.Infrastructure.Csv;
using CollegeFbsRankings.Infrastructure.Sql;
using CollegeFbsRankings.Infrastructure.Sql.EntityFramework;

namespace CollegeFbsRankings.Application.DataImport
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var inputData = ConfigurationManager.GetSection("input") as CsvRepositoryConfiguration;
            if (inputData == null)
            {
                throw ThrowHelper.ArgumentError("Unable to find the input information for the CSV data (tried to find a section called 'input' in 'app.config'");
            }

            var outputData = ConfigurationManager.GetSection("output") as SqlRepositoryConfiguration;
            if (outputData == null)
            {
                throw ThrowHelper.ArgumentError("Unable to find the SQL connection information for the output data (tried to find a section called 'output' in 'app.config'");
            }

            foreach (var season in inputData.Seasons)
            {
                Console.WriteLine("Reading input data for {0}...", season.Year);
                var repository = ReadDataForYear(season.Year, season.NumWeeksInRegularSeason, season.FbsTeamFile, season.FbsGameFile);

                Console.WriteLine("Writing data to database for {0}...", season.Year);
                WriteToDatabase(repository, outputData.ConnectionStringName);
            }
        }

        private static ICollegeFbsRepository ReadDataForYear(int year, int numWeeksInRegularSeason, string fbsTeamFileName, string fbsGameFileName)
        {
            var fbsTeamFile = new StreamReader(fbsTeamFileName);
            var fbsGameFile = new StreamReader(fbsGameFileName);

            var repository = new CsvRepository();
            repository.AddCsvData(year, numWeeksInRegularSeason, fbsTeamFile, fbsGameFile);
            return repository;
        }

        private static void WriteToDatabase(ICollegeFbsRepository repository, string connectionStringName)
        {
            var configuration = ConfigurationManager.ConnectionStrings[connectionStringName];
            var connectionString = configuration.ConnectionString;
            var providerFactory = DbProviderFactories.GetFactory(configuration.ProviderName);

            using (var connection = providerFactory.CreateConnection())
            {
                if (connection == null)
                {
                    throw ThrowHelper.ArgumentError(String.Format("Unable to establish a connection to '{0}' using '{1}'",
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
