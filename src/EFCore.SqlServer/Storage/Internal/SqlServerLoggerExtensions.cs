// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public static class SqlServerLoggerExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void DecimalTypeDefaultWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Model.Validation> diagnostics,
            [NotNull] IProperty property)
        {
            var eventId = SqlServerEventId.DecimalTypeDefaultWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    SqlServerStrings.DefaultDecimalTypeColumn(property.Name, property.DeclaringEntityType.DisplayName()));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        Property = property
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ByteIdentityColumnWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Model.Validation> diagnostics,
            [NotNull] IProperty property)
        {
            var eventId = SqlServerEventId.ByteIdentityColumnWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    SqlServerStrings.ByteIdentityColumn(property.Name, property.DeclaringEntityType.DisplayName()));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        Property = property
                    });
            }
        }
    }
}
