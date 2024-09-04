using Microsoft.Data.Sqlite;
using System.Data;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace DbUtil
{
    /// <summary>
    /// Class used to setup and use an SQLite Database. Solution is to be added as a dependency in a desired project.
    /// </summary>
    public class Database
    {
        private string databaseName;

        /// <summary>
        /// Constructor is provided with a database name to build a database in the project folder.
        /// </summary>
        /// <param name="databaseName"></param>
        public Database(string databaseName) 
        {
            this.databaseName = databaseName;
            var connection = new SqliteConnection($"Data Source={this.databaseName}.db");
        }

        /// <summary>
        /// Function that takes and performs a nonquery SQL statement on the instances database.
        /// </summary>
        /// <param name="sqlStatement"></param>
        public void PerformNonQuery (string sqlStatement)
        {
                using (var connection = new SqliteConnection($"Data Source={this.databaseName}.db"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = $"{sqlStatement}";
                    command.ExecuteNonQuery();
                }
        }

        /// <summary>
        /// Function that takes a query statement and returns a dataset (list of objects containing lists of objects) with the queried data of the instances database. 
        /// To specify the size of the dataset, the function uses the DetermineNumberOfOrdinals function to determine how many different values the query should return.
        /// </summary>
        /// <param name="queryStatement"></param>
        /// <returns>"Dataset"</returns>
        public List<object> PerformQuery(string queryStatement)
        {
            try
            {
                // Creates:
                // List<object> dataset {
                //      List<object> row { value1, value2, etc..},
                //      List<object> row { value1, value2, etc..},
                //      etc.. }
                List<object> dataSet = new List<object>();
                int numberOfReturnValues = DetermineNumberOfOrdinals(queryStatement);
                if (numberOfReturnValues > 0)
                {
                    using (var connection = new SqliteConnection($"Data Source={this.databaseName}.db"))
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = $"{queryStatement}";
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    List<object> row = new List<object>();
                                    for (int i = 0; i < numberOfReturnValues; i++)
                                    {
                                        row.Add(reader.GetValue(i));
                                    }
                                    dataSet.Add(row);
                                }
                                return dataSet;
                            }
                            else return dataSet;
                        }
                    }
                }
                else
                {
                    return dataSet;
                }
            }
            catch(Exception) 
            { 
                throw; 
            }
        }

        /// <summary>
        /// Function that takes a SQL query statement and determines how many values the query returns.
        /// </summary>
        /// <param name="queryCommand"></param>
        /// <returns>integer(number of requested query values)</returns>
        private int DetermineNumberOfOrdinals(string queryCommand)
        {
            // Looks at the SELECT part of the query. counts matching groups seperated by a comma. 
            // if an asterix is used, the query if performed, to manually count the returned values. 
            try
            {
                int numberOfOrdinals = 1;
                string pattern = @"SELECT\s(.+)\sFROM";

                if (Regex.IsMatch(queryCommand, pattern))
                {
                    string asterixPattern = @"\*";
                    var match = Regex.Match(queryCommand, pattern);
                    string ordinals = match.Groups[1].Value;
                    if (Regex.IsMatch(ordinals, asterixPattern))
                    {
                        using (var connection = new SqliteConnection($"Data Source={this.databaseName}.db"))
                        {
                            var command = connection.CreateCommand();
                            command.CommandText = $"{queryCommand}";
                            connection.Open();

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        for (int i = 0; i < 100; i++)
                                        {
                                            try
                                            {
                                                reader.GetValue(i);
                                            }
                                            catch
                                            {
                                                return i;
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                    else
                    {
                        foreach (char c in ordinals)
                        {
                            if (c == ',')
                            {
                                numberOfOrdinals++;
                            }
                        }
                        return numberOfOrdinals;
                    }
                }
                else
                {
                    return 0;
                }
                return numberOfOrdinals;
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}
