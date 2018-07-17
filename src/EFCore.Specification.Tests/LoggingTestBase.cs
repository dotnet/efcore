// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class LoggingTestBase
    {
        [Fact]
        public void Logs_context_initialization_default_options()
        {
            Assert.Equal(ExpectedMessage(DefaultOptions), ActualMessage(CreateOptionsBuilder()));
        }

        [Fact]
        public void Logs_context_initialization_no_tracking()
        {
            Assert.Equal(
                ExpectedMessage("NoTracking " + DefaultOptions),
                ActualMessage(CreateOptionsBuilder().UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)));
        }

        [Fact]
        public void Logs_context_initialization_sensitive_data_logging()
        {
            Assert.Equal(
                ExpectedMessage("SensitiveDataLoggingEnabled " + DefaultOptions),
                ActualMessage(CreateOptionsBuilder().EnableSensitiveDataLogging()));
        }

        protected virtual string ExpectedMessage(string optionsFragment)
            => CoreStrings.LogContextInitialized.GenerateMessage(
                ProductInfo.GetVersion(),
                nameof(LoggingContext),
                ProviderName,
                optionsFragment ?? "None").Trim();

        protected abstract DbContextOptionsBuilder CreateOptionsBuilder();

        protected abstract string ProviderName { get; }

        protected virtual string DefaultOptions => null;

        protected virtual string ActualMessage(DbContextOptionsBuilder optionsBuilder)
        {
            var loggerFactory = new ListLoggerFactory();
            using (var context = new LoggingContext(optionsBuilder.UseLoggerFactory(loggerFactory)))
            {
                var _ = context.Model;
            }

            return loggerFactory.Log.Single(t => t.Id.Id == CoreEventId.ContextInitialized.Id).Message;
        }

        protected class LoggingContext : DbContext
        {
            public LoggingContext(DbContextOptionsBuilder optionsBuilder)
                : base(optionsBuilder.Options)
            {
            }
        }
    }
}
