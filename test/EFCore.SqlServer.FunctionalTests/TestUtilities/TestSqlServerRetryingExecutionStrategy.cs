// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestSqlServerRetryingExecutionStrategy : SqlServerRetryingExecutionStrategy
    {
        private const bool ErrorNumberDebugMode = false;

        private static readonly int[] _additionalErrorNumbers =
        {
            -1, // Physical connection is not usable
            -2, // Timeout
            42008, // Mirroring (Only when a database is deleted and another one is created in fast succession)
            42019 // CREATE DATABASE operation failed
        };

        public TestSqlServerRetryingExecutionStrategy()
            : base(
                new DbContext(new DbContextOptionsBuilder().UseSqlServer(TestEnvironment.DefaultConnection).Options),
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

        public TestSqlServerRetryingExecutionStrategy(ExecutionStrategyDependencies dependencies)
            : base(dependencies, DefaultMaxRetryCount, DefaultMaxDelay, _additionalErrorNumbers)
        {
        }

        protected override bool ShouldRetryOn(Exception exception)
        {
            if (base.ShouldRetryOn(exception))
            {
                return true;
            }

            if (ErrorNumberDebugMode
                && exception is SqlException sqlException)
            {
                var message = "Didn't retry on";
                foreach (SqlError err in sqlException.Errors)
                {
                    message += " " + err.Number;
                }

                message += Environment.NewLine;
                throw new InvalidOperationException(message + exception, exception);
            }

            return exception is InvalidOperationException invalidOperationException
                && invalidOperationException.Message == "Internal .Net Framework Data Provider error 6."
                ? true
                : false;
        }

        public new virtual TimeSpan? GetNextDelay(Exception lastException)
        {
            ExceptionsEncountered.Add(lastException);
            return base.GetNextDelay(lastException);
        }

        public static new bool Suspended
        {
            get => ExecutionStrategy.Suspended;
            set => ExecutionStrategy.Suspended = value;
        }
    }
}
