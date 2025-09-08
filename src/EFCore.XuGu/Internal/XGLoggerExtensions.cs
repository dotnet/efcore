// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.XuGu.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class XGLoggerExtensions
    {
        private static string DecimalTypeKeyWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (PropertyEventData)payload;
            return d.GenerateMessage(
                p.Property.Name,
                p.Property.DeclaringType.DisplayName());
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void DecimalTypeDefaultWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            [NotNull] IProperty property)
        {
            var definition = XGResources.LogDefaultDecimalTypeColumn(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, property.Name, property.DeclaringType.DisplayName());
            }

            if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = new PropertyEventData(
                    definition,
                    DecimalTypeDefaultWarning,
                    property);

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
            }
        }

        private static string DecimalTypeDefaultWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (PropertyEventData)payload;
            return d.GenerateMessage(
                p.Property.Name,
                p.Property.DeclaringType.DisplayName());
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ByteIdentityColumnWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            [NotNull] IProperty property)
        {
            var definition = XGResources.LogByteIdentityColumn(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, property.Name, property.DeclaringType.DisplayName());
            }

            if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = new PropertyEventData(
                    definition,
                    ByteIdentityColumnWarning,
                    property);

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
            }
        }

        private static string ByteIdentityColumnWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (PropertyEventData)payload;
            return d.GenerateMessage(
                p.Property.Name,
                p.Property.DeclaringType.DisplayName());
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ColumnFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [NotNull] string tableName,
            [NotNull] string columnName,
            int ordinal,
            [NotNull] string dataTypeName,
            int maxLength,
            int precision,
            int scale,
            bool nullable,
            bool identity,
            [CanBeNull] string defaultValue,
            [CanBeNull] string computedValue,
            bool? stored)
        {
            var definition = XGResources.LogFoundColumn(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(
                    diagnostics,
                    l => l.LogDebug(
                        definition.EventId,
                        null,
                        definition.MessageFormat,
                        tableName,
                        columnName,
                        ordinal,
                        dataTypeName,
                        maxLength,
                        precision,
                        scale,
                        nullable,
                        identity,
                        defaultValue,
                        computedValue,
                        stored));
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
            [NotNull] string foreignKeyName,
            [NotNull] string tableName,
            [NotNull] string principalTableName,
            [NotNull] string onDeleteAction)
        {
            var definition = XGResources.LogFoundForeignKey(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, foreignKeyName, tableName, principalTableName, onDeleteAction);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void DefaultSchemaFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [NotNull] string schemaName)
        {
            var definition = XGResources.LogFoundDefaultSchema(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, schemaName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void TypeAliasFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [NotNull] string typeAliasName,
            [NotNull] string systemTypeName)
        {
            var definition = XGResources.LogFoundTypeAlias(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, typeAliasName, systemTypeName);
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
            [NotNull] string primaryKeyName,
            [NotNull] string tableName)
        {
            var definition = XGResources.LogFoundPrimaryKey(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, primaryKeyName, tableName);
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
            [NotNull] string uniqueConstraintName,
            [NotNull] string tableName)
        {
            var definition = XGResources.LogFoundUniqueConstraint(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, uniqueConstraintName, tableName);
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
            [NotNull] string indexName,
            [NotNull] string tableName,
            bool unique)
        {
            var definition = XGResources.LogFoundIndex(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, indexName, tableName, unique);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ForeignKeyReferencesMissingPrincipalTableWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string foreignKeyName,
            [CanBeNull] string tableName,
            [CanBeNull] string principalTableName)
        {
            var definition = XGResources.LogPrincipalTableNotInSelectionSet(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, foreignKeyName, tableName, principalTableName);
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
            [NotNull] string foreignKeyName,
            [NotNull] string tableName,
            [NotNull] string principalColumnName,
            [NotNull] string principalTableName)
        {
            var definition = XGResources.LogPrincipalColumnNotFound(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, foreignKeyName, tableName, principalColumnName, principalTableName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void MissingSchemaWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string schemaName)
        {
            var definition = XGResources.LogMissingSchema(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, schemaName);
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
            var definition = XGResources.LogMissingTable(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, tableName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void SequenceFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [NotNull] string sequenceName,
            [NotNull] string sequenceTypeName,
            bool cyclic,
            int increment,
            long start,
            long min,
            long max)
        {
            // No DiagnosticsSource events because these are purely design-time messages
            var definition = XGResources.LogFoundSequence(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(
                    diagnostics,
                    l => l.LogDebug(
                        definition.EventId,
                        null,
                        definition.MessageFormat,
                        sequenceName,
                        sequenceTypeName,
                        cyclic,
                        increment,
                        start,
                        min,
                        max));
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void TableFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [NotNull] string tableName)
        {
            var definition = XGResources.LogFoundTable(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, tableName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void DefaultValueNotSupportedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] string defaultValue,
            [NotNull] ServerVersion serverVersion,
            [NotNull] string columnType)
        {
            var definition = XGResources.LogDefaultValueNotSupported(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, defaultValue, serverVersion.ToString(), columnType);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }
    }
}
