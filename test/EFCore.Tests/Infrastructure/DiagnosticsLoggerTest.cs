// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class DiagnosticsLoggerTest
    {
        [ConditionalFact]
        public void Can_filter_for_messages_of_one_category()
        {
            FilterTest(c => c == DbLoggerCategory.Database.Command.Name, "SQL1", "SQL2");
        }

        [ConditionalFact]
        public void Can_filter_for_messages_of_one_subcategory()
        {
            FilterTest(c => c.StartsWith(DbLoggerCategory.Database.Name, StringComparison.Ordinal), "DB1", "SQL1", "DB2", "SQL2");
        }

        [ConditionalFact]
        public void Can_filter_for_all_EF_messages()
        {
            FilterTest(
                c => c.StartsWith(DbLoggerCategory.Name, StringComparison.Ordinal), "DB1", "SQL1", "Query1", "DB2", "SQL2", "Query2");
        }

        [ConditionalFact]
        public void Can_get_all_messages()
        {
            FilterTest(c => true, "DB1", "SQL1", "Query1", "Random1", "DB2", "SQL2", "Query2", "Random2");
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private void FilterTest(Func<string, bool> filter, params string[] expected)
        {
            var loggerFactory = new ListLoggerFactory(filter);

            var dbLogger = new DiagnosticsLogger<DbLoggerCategory.Database>(
                loggerFactory, new LoggingOptions(), new DiagnosticListener("Fake"), new TestLoggingDefinitions());
            var sqlLogger = new DiagnosticsLogger<DbLoggerCategory.Database.Command>(
                loggerFactory, new LoggingOptions(), new DiagnosticListener("Fake"), new TestLoggingDefinitions());
            var queryLogger = new DiagnosticsLogger<DbLoggerCategory.Query>(
                loggerFactory, new LoggingOptions(), new DiagnosticListener("Fake"), new TestLoggingDefinitions());
            var randomLogger = loggerFactory.CreateLogger("Random");

            dbLogger.Logger.LogInformation(1, "DB1");
            sqlLogger.Logger.LogInformation(2, "SQL1");
            queryLogger.Logger.LogInformation(3, "Query1");
            randomLogger.LogInformation(4, "Random1");

            dbLogger.Logger.LogInformation(1, "DB2");
            sqlLogger.Logger.LogInformation(2, "SQL2");
            queryLogger.Logger.LogInformation(3, "Query2");
            randomLogger.LogInformation(4, "Random2");

            Assert.Equal(expected, loggerFactory.Log.Select(l => l.Message));
        }
    }
}
