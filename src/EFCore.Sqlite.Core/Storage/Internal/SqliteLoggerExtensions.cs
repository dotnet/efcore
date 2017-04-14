// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public static class SqliteLoggerExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SchemaConfiguredWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Model.Validation> diagnostics,
            [NotNull] IEntityType entityType,
            [NotNull] string schema)
        {
            var eventId = SqliteEventId.SchemaConfiguredWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    SqliteStrings.SchemaConfigured(entityType.DisplayName(), schema));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        EntityType = entityType,
                        Schema = schema
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SequenceConfiguredWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Model.Validation> diagnostics,
            [NotNull] ISequence sequence)
        {
            var eventId = SqliteEventId.SequenceConfiguredWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    SqliteStrings.SequenceConfigured(sequence.Name));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        Sequence = sequence
                    });
            }
        }
    }
}
