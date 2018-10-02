// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Storage;
using Oracle.ManagedDataAccess.Client;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestOracleRetryingExecutionStrategy : OracleRetryingExecutionStrategy
    {
        private static readonly int[] _additionalErrorNumbers =
        {
            -1, // Physical connection is not usable
            -2, // Timeout
            42008, // Mirroring (Only when a database is deleted and another one is created in fast succession)
            42019 // CREATE DATABASE operation failed
        };

        public TestOracleRetryingExecutionStrategy()
            : base(
                new DbContext(new DbContextOptionsBuilder().UseOracle(TestEnvironment.DefaultConnection).Options),
                DefaultMaxRetryCount, DefaultMaxDelay, _additionalErrorNumbers)
        {
        }

        public TestOracleRetryingExecutionStrategy(DbContext context)
            : base(context, DefaultMaxRetryCount, DefaultMaxDelay, _additionalErrorNumbers)
        {
        }

        public TestOracleRetryingExecutionStrategy(DbContext context, TimeSpan maxDelay)
            : base(context, DefaultMaxRetryCount, maxDelay, _additionalErrorNumbers)
        {
        }

        public TestOracleRetryingExecutionStrategy(ExecutionStrategyDependencies dependencies)
            : base(dependencies, DefaultMaxRetryCount, DefaultMaxDelay, _additionalErrorNumbers)
        {
        }

        protected override bool ShouldRetryOn(Exception exception)
        {
            if (base.ShouldRetryOn(exception))
            {
                return true;
            }

            if (exception is OracleException sqlException)
            {
                var message = "Didn't retry on";
                foreach (OracleError err in sqlException.Errors)
                {
                    message += " " + err.Number;
                }
                throw new InvalidOperationException(message + exception, exception);
            }

            return exception is InvalidOperationException invalidOperationException
                && invalidOperationException.Message == "Internal .Net Framework Data Provider error 6.";
        }

        public new virtual TimeSpan? GetNextDelay(Exception lastException)
        {
            ExceptionsEncountered.Add(lastException);
            return base.GetNextDelay(lastException);
        }

        public new static bool Suspended
        {
            get => ExecutionStrategy.Suspended;
            set => ExecutionStrategy.Suspended = value;
        }
    }
}
