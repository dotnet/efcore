// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
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
            var eventId = RelationalDesignEventId.MissingSchemaWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.MissingSchema(schemaName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.MissingTableWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.MissingTable(tableName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.SequenceNotNamedWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.SequencesRequireName);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(eventId.Name, null);
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
            var eventId = RelationalDesignEventId.SequenceTypeNotSupportedWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.BadSequenceType(sequenceName, dataTypeName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.UnableToGenerateEntityTypeWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.UnableToGenerateEntityType(tableName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.ColumnTypeNotMappedWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.CannotFindTypeMappingForColumn(columnName, dataTypeName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.MissingPrimaryKeyWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.MissingPrimaryKey(tableName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.PrimaryKeyColumnsNotMappedWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.PrimaryKeyErrorPropertyNotFound(
                        tableName,
                        string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, unmappedColumnNames)));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.IndexColumnsNotMappedWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.UnableToScaffoldIndexMissingProperty(
                        indexName,
                        string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, unmappedColumnNames)));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.ForeignKeyReferencesMissingTableWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.ForeignKeyScaffoldErrorPrincipalTableNotFound(foreignKeyName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.ForeignKeyReferencesNotMappedTableWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.ForeignKeyScaffoldErrorPrincipalTableScaffoldingError(foreignKeyName, principalTableName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.ForeignKeyColumnsNotMappedWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.ForeignKeyScaffoldErrorPropertyNotFound(
                        foreignKeyName,
                        string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, unmappedColumnNames)));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.ForeignKeyReferencesMissingPrincipalKeyWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.ForeignKeyScaffoldErrorPrincipalKeyNotFound(
                        foreignKeyName, 
                        string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, principalColumnNames), 
                        principalEntityTypeName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.ForeignKeyPrincipalEndContainsNullableColumnsWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.ForeignKeyPrincipalEndContainsNullableColumns(
                        foreignKeyName,
                        indexName,
                        nullablePropertyNames.Aggregate((a, b) => a + "," + b)));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.SequenceFound;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    RelationalDesignStrings.FoundSequence(
                        sequenceName, sequenceTypeName, cyclic,
                        increment, start, min, max));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.TableFound;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    RelationalDesignStrings.FoundTable(tableName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.TableSkipped;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    RelationalDesignStrings.TableNotInSelectionSet(tableName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.ColumnSkipped;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    RelationalDesignStrings.ColumnNotInSelectionSet(columnName, tableName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.IndexColumnFound;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    RelationalDesignStrings.FoundIndexColumn(indexName, tableName, columnName, ordinal));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.ColumnNotNamedWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.ColumnNameEmptyOnTable(tableName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.IndexColumnSkipped;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.IndexColumnNotInSelectionSet(columnName, indexName, tableName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.IndexNotNamedWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.IndexNameEmpty(tableName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.IndexTableMissingWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.UnableToFindTableForIndex(indexName, tableName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.IndexColumnNotNamedWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.ColumnNameEmptyOnIndex(indexName, tableName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.ForeignKeyNotNamedWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.ForeignKeyNameEmpty(tableName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.ForeignKeyColumnMissingWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.ForeignKeyColumnNotInSelectionSet(columnName, foreignKeyName, tableName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.ForeignKeyReferencesMissingPrincipalTableWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.PrincipalTableNotInSelectionSet(foreignKeyName, tableName, principalTableName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.ForeignKeyColumnNotNamedWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.ColumnNameEmptyOnForeignKey(tableName, foreignKeyName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.IndexFound;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    RelationalDesignStrings.FoundIndex(indexName, tableName, unique));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            var eventId = RelationalDesignEventId.ForeignKeyPrincipalColumnMissingWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalDesignStrings.PrincipalColumnNotFound(foreignKeyName, tableName, principalColumnName, principalTableName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
