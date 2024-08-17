using Microsoft.Data.Sqlite;
using System.Data;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace DbUtil
{
    public class Database
    {
        private string databaseName;

        public Database(string databaseName) 
        {
            this.databaseName = databaseName;
            var connection = new SqliteConnection($"Data Source={this.databaseName}.db");
        }

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
        /// Takes a query statement and returns a list of lists with the queried data.  
        /// </summary>
        /// <param name="queryStatement"></param>
        /// <returns>"List<object>"</returns>
        public List<object> PerformQuery(string queryStatement)
        {
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

        private int DetermineNumberOfOrdinals(string queryCommand)
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
    }
}
