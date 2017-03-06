// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities
{
    public class TestSqlServerRetryingExecutionStrategy : SqlServerRetryingExecutionStrategy
    {
        private static readonly int[] _additionalErrorNumbers =
        {
            -1, // Physical connection is not usable
            -2, // Timeout
            42008, // Mirroring (Only when a database is deleted and another one is crated in fast succession)
            42019 // CREATE DATABASE operation failed
        };

        public TestSqlServerRetryingExecutionStrategy()
            : base(new DbContext(new DbContextOptionsBuilder().UseSqlServer(TestEnvironment.DefaultConnection).Options),
                  DefaultMaxRetryCount, DefaultMaxDelay, _additionalErrorNumbers)
        {
        }

        public TestSqlServerRetryingExecutionStrategy(DbContext context)
            : base(context, DefaultMaxRetryCount, DefaultMaxDelay, _additionalErrorNumbers)
        {
        }

        public TestSqlServerRetryingExecutionStrategy(DbContext context, TimeSpan maxDelay)
            : base(context, DefaultMaxRetryCount, maxDelay, _additionalErrorNumbers)
        {
        }

        public TestSqlServerRetryingExecutionStrategy(ExecutionStrategyContext context)
            : base(context, DefaultMaxRetryCount, DefaultMaxDelay, _additionalErrorNumbers)
        {
        }

        protected override bool ShouldRetryOn(Exception exception)
        {
            if (base.ShouldRetryOn(exception))
            {
                return true;
            }

            var sqlException = exception as SqlException;
            if (sqlException != null)
            {
                var message = "Didn't retry on";
                foreach (SqlError err in sqlException.Errors)
                {
                    message += " " + err.Number;
                }
                throw new InvalidOperationException(message, exception);
            }

            var invalidOperationException = exception as InvalidOperationException;
            if (invalidOperationException != null
                && invalidOperationException.Message == "Internal .Net Framework Data Provider error 6.")
            {
                return true;
            }

            return false;
        }

        public new virtual TimeSpan? GetNextDelay(Exception lastException)
        {
            ExceptionsEncountered.Add(lastException);
            return base.GetNextDelay(lastException);
        }

        public new static bool Suspended
        {
            get { return ExecutionStrategy.Suspended; }
            set { ExecutionStrategy.Suspended = value; }
        }
    }
}
