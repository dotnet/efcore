// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class DesignLoggerExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MissingSchemaWarning(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string schemaName)
            // No DiagnosticsSource events because these are purely design-time messages
            => DesignStrings.LogMissingSchema.Log(diagnostics, schemaName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SequenceTypeNotSupportedWarning(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string sequenceName,
                [CanBeNull] string dataTypeName)
            // No DiagnosticsSource events because these are purely design-time messages
            => DesignStrings.LogBadSequenceType.Log(diagnostics, sequenceName, dataTypeName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void UnableToGenerateEntityTypeWarning(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string tableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => DesignStrings.LogUnableToGenerateEntityType.Log(diagnostics, tableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ColumnTypeNotMappedWarning(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string columnName,
                [CanBeNull] string dataTypeName)
            // No DiagnosticsSource events because these are purely design-time messages
            => DesignStrings.LogCannotFindTypeMappingForColumn.Log(diagnostics, columnName, dataTypeName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MissingPrimaryKeyWarning(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string tableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => DesignStrings.LogMissingPrimaryKey.Log(diagnostics, tableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void PrimaryKeyColumnsNotMappedWarning(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string tableName,
                [NotNull] IList<string> unmappedColumnNames)
            // No DiagnosticsSource events because these are purely design-time messages
            => DesignStrings.LogPrimaryKeyErrorPropertyNotFound.Log(
                diagnostics,
                tableName,
                string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, unmappedColumnNames));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyReferencesNotMappedTableWarning(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string foreignKeyName,
                [NotNull] string principalTableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => DesignStrings.LogForeignKeyScaffoldErrorPrincipalTableScaffoldingError.Log(diagnostics, foreignKeyName, principalTableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyReferencesMissingPrincipalKeyWarning(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string foreignKeyName,
                [CanBeNull] string principalEntityTypeName,
                [NotNull] IList<string> principalColumnNames)
            // No DiagnosticsSource events because these are purely design-time messages
            => DesignStrings.LogForeignKeyScaffoldErrorPrincipalKeyNotFound.Log(
                diagnostics,
                foreignKeyName,
                string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, principalColumnNames),
                principalEntityTypeName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyPrincipalEndContainsNullableColumnsWarning(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string foreignKeyName,
                [CanBeNull] string indexName,
                [CanBeNull] IList<string> nullablePropertyNames)
            // No DiagnosticsSource events because these are purely design-time messages
            => DesignStrings.LogForeignKeyPrincipalEndContainsNullableColumns.Log(
                diagnostics,
                foreignKeyName,
                indexName,
                nullablePropertyNames.Aggregate((a, b) => a + "," + b));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void NonNullableBoooleanColumnHasDefaultConstraintWarning(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string columnName)
            // No DiagnosticsSource events because these are purely design-time messages
            => DesignStrings.LogNonNullableBoooleanColumnHasDefaultConstraint.Log(
                diagnostics,
                columnName);
    }
}
