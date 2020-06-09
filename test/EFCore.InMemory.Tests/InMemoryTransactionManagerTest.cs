// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class InMemoryTransactionManagerTest
    {
        [ConditionalFact]
        public void CurrentTransaction_returns_null()
        {
            var transactionManager = new InMemoryTransactionManager(CreateLogger());

            Assert.Null(transactionManager.CurrentTransaction);
        }

        [ConditionalFact]
        public void Throws_on_BeginTransaction()
            => AssertThrows(() => new InMemoryTransactionManager(CreateLogger()).BeginTransaction());

        [ConditionalFact]
        public void Throws_on_BeginTransactionAsync()
            => AssertThrows(() => new InMemoryTransactionManager(CreateLogger()).BeginTransactionAsync().GetAwaiter().GetResult());

        [ConditionalFact]
        public void Throws_on_CommitTransaction()
            => AssertThrows(() => new InMemoryTransactionManager(CreateLogger()).CommitTransaction());

        [ConditionalFact]
        public void Throws_on_CommitTransactionAsync()
            => AssertThrows(() => new InMemoryTransactionManager(CreateLogger()).CommitTransactionAsync().GetAwaiter().GetResult());

        [ConditionalFact]
        public void Throws_on_RollbackTransaction()
            => AssertThrows(() => new InMemoryTransactionManager(CreateLogger()).RollbackTransaction());

        [ConditionalFact]
        public void Throws_on_RollbackTransactionAsync()
            => AssertThrows(() => new InMemoryTransactionManager(CreateLogger()).RollbackTransactionAsync().GetAwaiter().GetResult());

        private static void AssertThrows(Action action)
        {
            Assert.Equal(
                CoreStrings.WarningAsErrorTemplate(
                    InMemoryEventId.TransactionIgnoredWarning,
                    InMemoryResources.LogTransactionsNotSupported(new TestLogger<InMemoryLoggingDefinitions>()).GenerateMessage(),
                    "InMemoryEventId.TransactionIgnoredWarning"),
                Assert.Throws<InvalidOperationException>(action).Message);
        }

        private DiagnosticsLogger<DbLoggerCategory.Database.Transaction> CreateLogger()
        {
            var options = new LoggingOptions();
            options.Initialize(new DbContextOptionsBuilder().ConfigureWarnings(w => w.Default(WarningBehavior.Throw)).Options);
            var logger = new DiagnosticsLogger<DbLoggerCategory.Database.Transaction>(
                new ListLoggerFactory(l => false),
                options,
                new DiagnosticListener("Fake"),
                new InMemoryLoggingDefinitions(),
                new NullDbContextLogger());
            return logger;
        }
    }
}
