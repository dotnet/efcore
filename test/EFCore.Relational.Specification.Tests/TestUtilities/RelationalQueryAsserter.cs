// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class RelationalQueryAsserter : QueryAsserter
{
    private readonly bool _canExecuteQueryString;

    public RelationalQueryAsserter(
        IQueryFixtureBase queryFixture,
        Func<Expression, Expression> rewriteExpectedQueryExpression,
        Func<Expression, Expression> rewriteServerQueryExpression,
        bool ignoreEntryCount = false,
        bool canExecuteQueryString = false)
        : base(queryFixture, rewriteExpectedQueryExpression, rewriteServerQueryExpression, ignoreEntryCount)
    {
        _canExecuteQueryString = canExecuteQueryString;
    }

    protected override void AssertRogueExecution(int expectedCount, IQueryable queryable)
    {
        var dependencies = ExecuteOurDbCommand(expectedCount, queryable);

        if (_canExecuteQueryString)
        {
            ExecuteTheirDbCommand(queryable, dependencies);
        }
    }

    private static (DbConnection, DbTransaction, int, int) ExecuteOurDbCommand(int expectedCount, IQueryable queryable)
    {
        using var command = queryable.CreateDbCommand();
        var count = ExecuteReader(command);

        // There may be more rows returned than entity instances created, but there
        // should never be fewer.
        Assert.True(count >= expectedCount);

        return (command.Connection, command.Transaction, command.CommandTimeout, count);
    }

    private static void ExecuteTheirDbCommand(
        IQueryable queryable,
        (DbConnection, DbTransaction, int, int) commandDependencies)
    {
        var (connection, transaction, timeout, expectedCount) = commandDependencies;

        var queryString = queryable.ToQueryString();

        if (queryString.EndsWith(RelationalStrings.SplitQueryString, StringComparison.Ordinal))
        {
            queryString = queryString.Substring(0, queryString.Length - RelationalStrings.SplitQueryString.Length);
        }

        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = queryString;
        command.CommandTimeout = timeout;

        var count = ExecuteReader(command);

        Assert.Equal(expectedCount, count);
    }

    private static int ExecuteReader(DbCommand command)
    {
        var needToOpen = command.Connection?.State == ConnectionState.Closed;
        if (needToOpen)
        {
            command.Connection.Open();
        }

        var count = 0;

        using (var reader = command.ExecuteReader())
        {
            // Not materializing objects here since automatic creation of objects does not
            // work for some SQL types, such as geometry/geography
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    count++;
                }
            }
        }

        if (needToOpen)
        {
            command.Connection.Close();
        }

        return count;
    }
}
