// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class RelationalDesignLoggerExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MissingSchemaWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string schemaName)
        {
            var definition = RelationalDesignStrings.LogMissingSchema;

            definition.Log(diagnostics, schemaName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        SchemaName = schemaName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MissingTableWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
        {
            var definition = RelationalDesignStrings.LogMissingTable;

            definition.Log(diagnostics, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SequenceNotNamedWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics)
        {
            var definition = RelationalDesignStrings.LogSequencesRequireName;

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
        public static void SequenceTypeNotSupportedWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string sequenceName,
            [CanBeNull] string dataTypeName)
        {
            var definition = RelationalDesignStrings.LogBadSequenceType;

            definition.Log(diagnostics, sequenceName, dataTypeName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        SequenceName = sequenceName,
                        DataTypeName = dataTypeName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void UnableToGenerateEntityTypeWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
        {
            var definition = RelationalDesignStrings.LogUnableToGenerateEntityType;

            definition.Log(diagnostics, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ColumnTypeNotMappedWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string columnName,
            [CanBeNull] string dataTypeName)
        {
            var definition = RelationalDesignStrings.LogCannotFindTypeMappingForColumn;

            definition.Log(diagnostics, columnName, dataTypeName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        ColumnName = columnName,
                        DataTypeName = dataTypeName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MissingPrimaryKeyWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
        {
            var definition = RelationalDesignStrings.LogMissingPrimaryKey;

            definition.Log(diagnostics, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void PrimaryKeyColumnsNotMappedWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName,
            [NotNull] IList<string> unmappedColumnNames)
        {
            var definition = RelationalDesignStrings.LogPrimaryKeyErrorPropertyNotFound;

            definition.Log(
                diagnostics,
                tableName,
                string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, unmappedColumnNames));

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName,
                        UnmappedColumnNames = unmappedColumnNames
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IndexColumnsNotMappedWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string indexName,
            [NotNull] IList<string> unmappedColumnNames)
        {
            var definition = RelationalDesignStrings.LogUnableToScaffoldIndexMissingProperty;

            definition.Log(
                diagnostics,
                indexName,
                string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, unmappedColumnNames));

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        IndexName = indexName,
                        UnmappedColumnNames = unmappedColumnNames
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyReferencesMissingTableWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string foreignKeyName)
        {
            var definition = RelationalDesignStrings.LogForeignKeyScaffoldErrorPrincipalTableNotFound;

            definition.Log(diagnostics, foreignKeyName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        ForeignKeyName = foreignKeyName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyReferencesNotMappedTableWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string foreignKeyName,
            [NotNull] string principalTableName)
        {
            var definition = RelationalDesignStrings.LogForeignKeyScaffoldErrorPrincipalTableScaffoldingError;

            definition.Log(diagnostics, foreignKeyName, principalTableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        ForeignKeyName = foreignKeyName,
                        PrincipalTableName = principalTableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyColumnsNotMappedWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string foreignKeyName,
            [NotNull] IList<string> unmappedColumnNames)
        {
            var definition = RelationalDesignStrings.LogForeignKeyScaffoldErrorPropertyNotFound;

            definition.Log(
                diagnostics,
                foreignKeyName,
                string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, unmappedColumnNames));

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        ForeignKeyName = foreignKeyName,
                        UnmappedColumnNames = unmappedColumnNames
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyReferencesMissingPrincipalKeyWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string foreignKeyName,
            [CanBeNull] string principalEntityTypeName,
            [NotNull] IList<string> principalColumnNames)
        {
            var definition = RelationalDesignStrings.LogForeignKeyScaffoldErrorPrincipalKeyNotFound;

            definition.Log(
                diagnostics,
                foreignKeyName,
                string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, principalColumnNames),
                principalEntityTypeName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        ForeignKeyName = foreignKeyName,
                        PrincipalEntityTypeName = principalEntityTypeName,
                        PrincipalColumnNames = principalColumnNames
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyPrincipalEndContainsNullableColumnsWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string foreignKeyName,
            [CanBeNull] string indexName,
            [CanBeNull] IList<string> nullablePropertyNames)
        {
            var definition = RelationalDesignStrings.LogForeignKeyPrincipalEndContainsNullableColumns;

            definition.Log(
                diagnostics,
                foreignKeyName,
                indexName,
                nullablePropertyNames.Aggregate((a, b) => a + "," + b));

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        ForeignKeyName = foreignKeyName,
                        IndexName = indexName,
                        NullablePropertyNames = nullablePropertyNames
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SequenceFound(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string sequenceName,
            [CanBeNull] string sequenceTypeName,
            bool? cyclic,
            int? increment,
            long? start,
            long? min,
            long? max)
        {
            var definition = RelationalDesignStrings.LogFoundSequence;

            Debug.Assert(LogLevel.Debug == definition.Level);

            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    l => l.LogDebug(
                        definition.EventId,
                        null,
                        definition.RawMessage,
                        sequenceName,
                        sequenceTypeName,
                        cyclic,
                        increment,
                        start,
                        min,
                        max));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        SequenceName = sequenceName,
                        SequenceTypeName = sequenceTypeName,
                        Cyclic = cyclic,
                        Increment = increment,
                        Start = start,
                        Min = min,
                        Max = max
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void TableFound(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
        {
            var definition = RelationalDesignStrings.LogFoundTable;

            definition.Log(diagnostics, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void TableSkipped(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
        {
            var definition = RelationalDesignStrings.LogTableNotInSelectionSet;

            definition.Log(diagnostics, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ColumnSkipped(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName,
            [CanBeNull] string columnName)
        {
            var definition = RelationalDesignStrings.LogColumnNotInSelectionSet;

            definition.Log(diagnostics, columnName, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName,
                        ColumnName = columnName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IndexColumnFound(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName,
            [CanBeNull] string indexName,
            bool? unique,
            [CanBeNull] string columnName,
            int? ordinal)
        {
            var definition = RelationalDesignStrings.LogFoundIndexColumn;

            definition.Log(diagnostics, indexName, tableName, columnName, ordinal);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName,
                        IndexName = indexName,
                        Unique = unique,
                        ColumnName = columnName,
                        Ordinal = ordinal
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ColumnNotNamedWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
        {
            var definition = RelationalDesignStrings.LogColumnNameEmptyOnTable;

            definition.Log(diagnostics, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IndexColumnSkipped(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName,
            [CanBeNull] string indexName,
            [CanBeNull] string columnName)
        {
            var definition = RelationalDesignStrings.LogIndexColumnNotInSelectionSet;

            definition.Log(diagnostics, columnName, indexName, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName,
                        IndexName = indexName,
                        ColumnName = columnName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IndexNotNamedWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
        {
            var definition = RelationalDesignStrings.LogIndexNameEmpty;

            definition.Log(diagnostics, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IndexTableMissingWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string indexName,
            [CanBeNull] string tableName)
        {
            var definition = RelationalDesignStrings.LogUnableToFindTableForIndex;

            definition.Log(diagnostics, indexName, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        IndexName = indexName,
                        TableName = tableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IndexColumnNotNamedWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string indexName,
            [CanBeNull] string tableName)
        {
            var definition = RelationalDesignStrings.LogColumnNameEmptyOnIndex;

            definition.Log(diagnostics, indexName, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        IndexName = indexName,
                        TableName = tableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyNotNamedWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
        {
            var definition = RelationalDesignStrings.LogForeignKeyNameEmpty;

            definition.Log(diagnostics, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyColumnMissingWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string columnName,
            [CanBeNull] string foreignKeyName,
            [CanBeNull] string tableName)
        {
            var definition = RelationalDesignStrings.LogForeignKeyColumnNotInSelectionSet;

            definition.Log(diagnostics, columnName, foreignKeyName, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        ColumnName = columnName,
                        ForeignKeyName = foreignKeyName,
                        TableName = tableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyReferencesMissingPrincipalTableWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string foreignKeyName,
            [CanBeNull] string tableName,
            [CanBeNull] string principalTableName)
        {
            var definition = RelationalDesignStrings.LogPrincipalTableNotInSelectionSet;

            definition.Log(diagnostics, foreignKeyName, tableName, principalTableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        ForeignKeyName = foreignKeyName,
                        TableName = tableName,
                        PrincipalTableName = principalTableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyColumnNotNamedWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string foreignKeyName,
            [CanBeNull] string tableName)
        {
            var definition = RelationalDesignStrings.LogColumnNameEmptyOnForeignKey;

            definition.Log(diagnostics, tableName, foreignKeyName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        ForeignKeyName = foreignKeyName,
                        TableName = tableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IndexFound(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string indexName,
            [CanBeNull] string tableName,
            bool? unique)
        {
            var definition = RelationalDesignStrings.LogFoundIndex;

            definition.Log(diagnostics, indexName, tableName, unique);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        IndexName = indexName,
                        TableName = tableName,
                        Unique = unique
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyPrincipalColumnMissingWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string foreignKeyName,
            [CanBeNull] string tableName,
            [CanBeNull] string principalColumnName,
            [CanBeNull] string principalTableName)
        {
            var definition = RelationalDesignStrings.LogPrincipalColumnNotFound;

            definition.Log(diagnostics, foreignKeyName, tableName, principalColumnName, principalTableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        ForeignKeyName = foreignKeyName,
                        TableName = tableName,
                        PrincipalColumnName = principalColumnName,
                        PrincipalTableName = principalTableName
                    });
            }
        }
    }
}
