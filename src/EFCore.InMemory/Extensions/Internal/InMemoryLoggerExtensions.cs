// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Update;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class InMemoryLoggerExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void TransactionIgnoredWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Transaction> diagnostics)
        {
            var definition = InMemoryStrings.LogTransactionsNotSupported;

            definition.Log(diagnostics);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(definition.EventId.Name, null);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ChangesSaved(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Update> diagnostics,
            [NotNull] IEnumerable<IUpdateEntry> entries,
            int rowsAffected)
        {
            var definition = InMemoryStrings.LogSavedChanges;

            definition.Log(diagnostics, rowsAffected);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        Entries = entries,
                        RowsAffected = rowsAffected
                    });
            }
        }
    }
}
