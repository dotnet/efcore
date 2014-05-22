// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.Framework.Logging
{
    internal static class RelationalLoggerExtensions
    {
        public static void WriteSql([NotNull] this ILogger logger, [NotNull] string sql)
        {
            Check.NotNull(logger, "logger");
            Check.NotEmpty(sql, "sql");

            logger.WriteCore(TraceType.Verbose, RelationalLoggingEventIds.Sql, sql, null, (o, _) => (string)o);
        }
    }
}
