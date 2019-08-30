// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Sqlite.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class SqliteLoggerExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void SchemaConfiguredWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            [NotNull] IEntityType entityType,
            [NotNull] string schema)
        {
            var definition = SqliteResources.LogSchemaConfigured(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    entityType.DisplayName(), schema);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new EntityTypeSchemaEventData(
                        definition,
                        SchemaConfiguredWarning,
                        entityType,
                        schema));
            }
        }

        private static string SchemaConfiguredWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (EntityTypeSchemaEventData)payload;
            return d.GenerateMessage(
                p.EntityType.DisplayName(),
                p.Schema);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void SequenceConfiguredWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            [NotNull] ISequence sequence)
        {
            var definition = SqliteResources.LogSequenceConfigured(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    sequence.Name);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new SequenceEventData(
                        definition,
                        SequenceConfiguredWarning,
                        sequence));
            }
        }

        private static string SequenceConfiguredWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (SequenceEventData)payload;
            return d.GenerateMessage(p.Sequence.Name);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ColumnFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName,
            [CanBeNull] string columnName,
            [CanBeNull] string dataTypeName,
            bool notNull,
            [CanBeNull] string defaultValue)
        {
            var definition = SqliteResources.LogFoundColumn(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    tableName, columnName, dataTypeName, notNull, defaultValue);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void SchemasNotSupportedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics)
        {
            var definition = SqliteResources.LogUsingSchemaSelectionsWarning(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(diagnostics, warningBehavior);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ForeignKeyReferencesMissingTableWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string foreignKeyName)
        {
            var definition = SqliteResources.LogForeignKeyScaffoldErrorPrincipalTableNotFound(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    foreignKeyName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void TableFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
        {
            var definition = SqliteResources.LogFoundTable(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    tableName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void MissingTableWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
        {
            var definition = SqliteResources.LogMissingTable(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    tableName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ForeignKeyPrincipalColumnMissingWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string foreignKeyName,
            [CanBeNull] string tableName,
            [CanBeNull] string principalColumnName,
            [CanBeNull] string principalTableName)
        {
            var definition = SqliteResources.LogPrincipalColumnNotFound(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    foreignKeyName, tableName, principalColumnName, principalTableName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void IndexFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string indexName,
            [CanBeNull] string tableName,
            bool? unique)
        {
            var definition = SqliteResources.LogFoundIndex(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    indexName, tableName, unique);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ForeignKeyFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName,
            long id,
            [CanBeNull] string principalTableName,
            [CanBeNull] string deleteAction)
        {
            var definition = SqliteResources.LogFoundForeignKey(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    tableName, id, principalTableName, deleteAction);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void PrimaryKeyFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string primaryKeyName,
            [CanBeNull] string tableName)
        {
            var definition = SqliteResources.LogFoundPrimaryKey(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    primaryKeyName, tableName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void UniqueConstraintFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string uniqueConstraintName,
            [CanBeNull] string tableName)
        {
            var definition = SqliteResources.LogFoundUniqueConstraint(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    uniqueConstraintName, tableName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }
    }
}
