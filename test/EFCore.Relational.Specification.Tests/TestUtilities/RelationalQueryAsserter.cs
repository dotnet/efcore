// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class RelationalQueryAsserter<TContext> : QueryAsserter<TContext>
        where TContext : DbContext
    {
        private readonly bool _canExecuteQueryString;

        public RelationalQueryAsserter(
            Func<TContext> contextCreator,
            ISetSource expectedData,
            Dictionary<Type, object> entitySorters,
            Dictionary<Type, object> entityAsserters,
            bool canExecuteQueryString,
            ExpectedQueryRewritingVisitor expectedQueryRewritingVisitor = null)
            : base(contextCreator, expectedData, entitySorters, entityAsserters, expectedQueryRewritingVisitor)
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
            // should never vbe fewer.
            Assert.True(count >= expectedCount);

            return (command.Connection, command.Transaction, command.CommandTimeout, count);
        }

        private static void ExecuteTheirDbCommand(
            IQueryable queryable,
            (DbConnection, DbTransaction, int, int) commandDependencies)
        {
            var (connection, transaction, timeout, expectedCount) = commandDependencies;

            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = queryable.ToQueryString();
            command.CommandTimeout = timeout;

            var count = ExecuteReader(command);

            Assert.Equal(expectedCount, count);
        }

        private static int ExecuteReader(DbCommand command)
        {
            using var reader = command.ExecuteReader();

            // Not materializing objects here since automatic creation of objects does not
            // work for some SQL types, such as geometry/geography
            var count = 0;
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    count++;
                }
            }

            return count;
        }
    }
}
