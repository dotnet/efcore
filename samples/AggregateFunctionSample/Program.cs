using System;
using Microsoft.Data.Sqlite;

namespace AggregateFunctionSample
{
    class Program
    {
        static void Main()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            var createCommand = connection.CreateCommand();
            createCommand.CommandText =
            @"
                CREATE TABLE student (
                    gpa REAL
                );

                INSERT INTO student
                VALUES (4.0),
                       (3.0),
                       (3.0),
                       (2.0),
                       (2.0),
                       (2.0),
                       (2.0),
                       (1.0),
                       (1.0),
                       (0.0);
            ";
            createCommand.ExecuteNonQuery();

            connection.CreateAggregate(
                "stdev",
                new StdDevContext(),

                // This is called for each row
                (StdDevContext context, double value) =>
                {
                    context.Count++;
                    var delta = value - context.Mean;
                    context.Mean += delta / context.Count;
                    context.Sum += delta * (value - context.Mean);

                    return context;
                },

                // This is called to get the final result
                context => Math.Sqrt(context.Sum / (context.Count - 1)));

            var queryCommand = connection.CreateCommand();
            queryCommand.CommandText =
            @"
                SELECT stdev(gpa)
                FROM student
            ";
            var stdDev = (double)queryCommand.ExecuteScalar();

            Console.WriteLine($"Standard deviation: {stdDev}");
        }

        struct StdDevContext
        {
            public int Count { get; set; }
            public double Mean { get; set; }
            public double Sum { get; set; }
        }
    }
}
