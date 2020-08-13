using System;
using Microsoft.Data.Sqlite;

namespace DateAndTimeSample
{
    class Program
    {
        static void Main()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            // SQLite doesn't support primitive DateTime and TimeSpan values. Instead, it
            // provides date and time functions to help you perform operations based on strings
            // and Julian day values.

            var dateTime1 = new DateTime(2017, 10, 20);
            var dateTime2 = new DateTime(2017, 10, 16);
            var timeSpan1 = new TimeSpan(1, 00, 0);
            var timeSpan2 = new TimeSpan(0, 30, 0);

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT

                    -- Comparisons work as expected
                    $dateTime1 > $dateTime2,
                    $timeSpan1 > $timeSpan2,

                    -- DateTime operations require converting to Julian days
                    julianday($dateTime1) - julianday($dateTime2),

                    -- TimeSpan operations require REAL parameters
                    $realTimeSpan1 - $realTimeSpan2,

                    -- More examples
                    julianday($dateTime1) - $realTimeSpan1,
                    $realTimeSpan1 / $realTimeSpan2,
                    $realTimeSpan1 / 2.0
            ";
            command.Parameters.AddWithValue("$dateTime1", dateTime1);
            command.Parameters.AddWithValue("$dateTime2", dateTime2);
            command.Parameters.AddWithValue("$timeSpan1", timeSpan1);
            command.Parameters.AddWithValue("$timeSpan2", timeSpan2);
            command.Parameters.Add("$realTimeSpan1", SqliteType.Real).Value = timeSpan1;
            command.Parameters.Add("$realTimeSpan2", SqliteType.Real).Value = timeSpan2;

            using (var reader = command.ExecuteReader())
            {
                reader.Read();

                var dateTimeComparison = reader.GetBoolean(0);
                Console.WriteLine($"{dateTime1} > {dateTime2} = {dateTimeComparison}");

                var timeSpanComparison = reader.GetBoolean(1);
                Console.WriteLine($"{timeSpan1} > {timeSpan2} = {timeSpanComparison}");

                var dateTimeDifference = reader.GetTimeSpan(2);
                Console.WriteLine($"{dateTime1} - {dateTime2} = {dateTimeDifference}");

                var timeSpanDifference = reader.GetTimeSpan(3);
                Console.WriteLine($"{timeSpan1} - {timeSpan2} = {timeSpanDifference}");

                var dateTimeSubtractTimeSpan = reader.GetDateTime(4);
                Console.WriteLine($"{dateTime1} - {timeSpan1} = {dateTimeSubtractTimeSpan}");

                var timeSpanDividedByTimeSpan = reader.GetDouble(5);
                Console.WriteLine($"{timeSpan1} / {timeSpan2} = {timeSpanDividedByTimeSpan}");

                var timeSpanDividedByDivisor = reader.GetTimeSpan(6);
                Console.WriteLine($"{timeSpan1} / 2.0 = {timeSpanDividedByDivisor}");
            }
        }
    }
}
