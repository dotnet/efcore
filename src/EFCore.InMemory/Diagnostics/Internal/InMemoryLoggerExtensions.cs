// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Update;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.InMemory.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class InMemoryLoggerExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void TransactionIgnoredWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics)
        {
            var definition = InMemoryResources.LogTransactionsNotSupported(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics);
            }

            if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = new EventData(
                    definition,
                    (d, _) => ((EventDefinition)d).GenerateMessage());

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ChangesSaved(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] IEnumerable<IUpdateEntry> entries,
            int rowsAffected)
        {
            var definition = InMemoryResources.LogSavedChanges(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, rowsAffected);
            }

            if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = new SaveChangesEventData(
                    definition,
                    ChangesSaved,
                    entries,
                    rowsAffected);

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
            }
        }

        private static string ChangesSaved(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<int>)definition;
            var p = (SaveChangesEventData)payload;
            return d.GenerateMessage(p.RowsAffected);
        }
    }
}
