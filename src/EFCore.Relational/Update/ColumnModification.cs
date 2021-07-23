// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         Represents an update, insert, or delete operation for a single column. <see cref="ModificationCommand" />s
    ///         contain lists of <see cref="ColumnModification" />s.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers; it is generally not used in application code.
    ///     </para>
    /// </summary>
    public class ColumnModification
    {
        private string? _parameterName;
        private string? _originalParameterName;
        private readonly Func<string>? _generateParameterName;
        private readonly object? _originalValue;
        private object? _value;
        private readonly bool _useParameters;
        private readonly bool _sensitiveLoggingEnabled;
        private List<ColumnModification>? _sharedColumnModifications;

        /// <summary>
        ///     Creates a new <see cref="ColumnModification" /> instance.
        /// </summary>
        /// <param name="entry"> The <see cref="IUpdateEntry" /> that represents the entity that is being modified. </param>
        /// <param name="property"> The property that maps to the column. </param>
        /// <param name="column"> The column to be modified. </param>
        /// <param name="generateParameterName"> A delegate for generating parameter names for the update SQL. </param>
        /// <param name="typeMapping"> The relational type mapping to be used for the command parameter. </param>
        /// <param name="isRead"> Indicates whether or not a value must be read from the database for the column. </param>
        /// <param name="isWrite"> Indicates whether or not a value must be written to the database for the column. </param>
        /// <param name="isKey"> Indicates whether or not the column part of a primary or alternate key.</param>
        /// <param name="isCondition"> Indicates whether or not the column is used in the <c>WHERE</c> clause when updating. </param>
        /// <param name="sensitiveLoggingEnabled"> Indicates whether or not potentially sensitive data (e.g. database values) can be logged. </param>
        public ColumnModification(
            IUpdateEntry entry,
            IProperty property,
            IColumn column,
            Func<string> generateParameterName,
            RelationalTypeMapping typeMapping,
            bool isRead,
            bool isWrite,
            bool isKey,
            bool isCondition,
            bool sensitiveLoggingEnabled)
            : this(
                Check.NotNull(column, nameof(column)).Name,
                originalValue: null,
                value: null,
                property: property,
                column.StoreType,
                typeMapping,
                isRead: isRead,
                isWrite: isWrite,
                isKey: isKey,
                isCondition: isCondition,
                sensitiveLoggingEnabled: sensitiveLoggingEnabled,
                column.IsNullable)
        {
            Check.NotNull(entry, nameof(entry));
            Check.NotNull(property, nameof(property));
            Check.NotNull(generateParameterName, nameof(generateParameterName));

            Entry = entry;
            _generateParameterName = generateParameterName;
            _useParameters = true;
        }

        /// <summary>
        ///     Creates a new <see cref="ColumnModification" /> instance.
        /// </summary>
        /// <param name="entry"> The <see cref="IUpdateEntry" /> that represents the entity that is being modified. </param>
        /// <param name="property"> The property that maps to the column. </param>
        /// <param name="generateParameterName"> A delegate for generating parameter names for the update SQL. </param>
        /// <param name="isRead"> Indicates whether or not a value must be read from the database for the column. </param>
        /// <param name="isWrite"> Indicates whether or not a value must be written to the database for the column. </param>
        /// <param name="isKey"> Indicates whether or not the column part of a primary or alternate key.</param>
        /// <param name="isCondition"> Indicates whether or not the column is used in the <c>WHERE</c> clause when updating. </param>
        /// <param name="isConcurrencyToken"> Indicates whether or not the column is acting as an optimistic concurrency token. </param>
        /// <param name="sensitiveLoggingEnabled"> Indicates whether or not potentially sensitive data (e.g. database values) can be logged. </param>
        [Obsolete("Use the constructor with column")]
        public ColumnModification(
            IUpdateEntry entry,
            IProperty property,
            Func<string> generateParameterName,
            bool isRead,
            bool isWrite,
            bool isKey,
            bool isCondition,
            bool isConcurrencyToken,
            bool sensitiveLoggingEnabled)
            : this(
                entry,
                property,
                Check.NotNull(property, nameof(property)).GetTableColumnMappings().First().Column,
                generateParameterName,
                Check.NotNull(property, nameof(property)).GetTableColumnMappings().First().TypeMapping,
                isRead: isRead,
                isWrite: isWrite,
                isKey: isKey,
                isCondition: isCondition,
                sensitiveLoggingEnabled: sensitiveLoggingEnabled)
        {
        }

        /// <summary>
        ///     Creates a new <see cref="ColumnModification" /> instance.
        /// </summary>
        /// <param name="columnName"> The name of the column. </param>
        /// <param name="originalValue"> The original value of the property mapped to this column. </param>
        /// <param name="value"> Gets or sets the current value of the property mapped to this column. </param>
        /// <param name="property"> The property that maps to the column. </param>
        /// <param name="columnType"> The database type of the column. </param>
        /// <param name="typeMapping"> The relational type mapping to be used for the command parameter. </param>
        /// <param name="isRead"> Indicates whether or not a value must be read from the database for the column. </param>
        /// <param name="isWrite"> Indicates whether or not a value must be written to the database for the column. </param>
        /// <param name="isKey"> Indicates whether or not the column part of a primary or alternate key.</param>
        /// <param name="isCondition"> Indicates whether or not the column is used in the <c>WHERE</c> clause when updating. </param>
        /// <param name="sensitiveLoggingEnabled"> Indicates whether or not potentially sensitive data (e.g. database values) can be logged. </param>
        /// <param name="isNullable"> A value indicating whether the value could be null. </param>
        public ColumnModification(
            string columnName,
            object? originalValue,
            object? value,
            IProperty? property,
            string? columnType,
            RelationalTypeMapping? typeMapping,
            bool isRead,
            bool isWrite,
            bool isKey,
            bool isCondition,
            bool sensitiveLoggingEnabled,
            bool? isNullable = null)
        {
            Check.NotNull(columnName, nameof(columnName));

            ColumnName = columnName;
            _originalValue = originalValue;
            _value = value;
            Property = property;
            ColumnType = columnType;
            TypeMapping = typeMapping;
            IsRead = isRead;
            IsWrite = isWrite;
            IsKey = isKey;
            IsCondition = isCondition;
            _sensitiveLoggingEnabled = sensitiveLoggingEnabled;
            IsNullable = isNullable;
        }

        /// <summary>
        ///     Creates a new <see cref="ColumnModification" /> instance.
        /// </summary>
        /// <param name="columnName"> The name of the column. </param>
        /// <param name="originalValue"> The original value of the property mapped to this column. </param>
        /// <param name="value"> Gets or sets the current value of the property mapped to this column. </param>
        /// <param name="property"> The property that maps to the column. </param>
        /// <param name="columnType"> The database type of the column. </param>
        /// <param name="isRead"> Indicates whether or not a value must be read from the database for the column. </param>
        /// <param name="isWrite"> Indicates whether or not a value must be written to the database for the column. </param>
        /// <param name="isKey"> Indicates whether or not the column part of a primary or alternate key.</param>
        /// <param name="isCondition"> Indicates whether or not the column is used in the <c>WHERE</c> clause when updating. </param>
        /// <param name="sensitiveLoggingEnabled"> Indicates whether or not potentially sensitive data (e.g. database values) can be logged. </param>
        [Obsolete("Use the constructor with type mapping")]
        public ColumnModification(
            string columnName,
            object? originalValue,
            object? value,
            IProperty? property,
            string? columnType,
            bool isRead,
            bool isWrite,
            bool isKey,
            bool isCondition,
            bool sensitiveLoggingEnabled)
        {
            Check.NotNull(columnName, nameof(columnName));

            ColumnName = columnName;
            _originalValue = originalValue;
            _value = value;
            Property = property;
            ColumnType = columnType;
            IsRead = isRead;
            IsWrite = isWrite;
            IsKey = isKey;
            IsCondition = isCondition;
            _sensitiveLoggingEnabled = sensitiveLoggingEnabled;
        }

        /// <summary>
        ///     Creates a new <see cref="ColumnModification" /> instance.
        /// </summary>
        /// <param name="columnName"> The name of the column. </param>
        /// <param name="originalValue"> The original value of the property mapped to this column. </param>
        /// <param name="value"> Gets or sets the current value of the property mapped to this column. </param>
        /// <param name="property"> The property that maps to the column. </param>
        /// <param name="isRead"> Indicates whether or not a value must be read from the database for the column. </param>
        /// <param name="isWrite"> Indicates whether or not a value must be written to the database for the column. </param>
        /// <param name="isKey"> Indicates whether or not the column part of a primary or alternate key.</param>
        /// <param name="isCondition"> Indicates whether or not the column is used in the <c>WHERE</c> clause when updating. </param>
        /// <param name="sensitiveLoggingEnabled"> Indicates whether or not potentially sensitive data (e.g. database values) can be logged. </param>
        [Obsolete("Use the constructor with columnType")]
        public ColumnModification(
            string columnName,
            object? originalValue,
            object? value,
            IProperty? property,
            bool isRead,
            bool isWrite,
            bool isKey,
            bool isCondition,
            bool sensitiveLoggingEnabled)
            : this(
                columnName,
                originalValue: originalValue,
                value: value,
                property: property,
                columnType: null,
                isRead: isRead,
                isWrite: isWrite,
                isKey: isKey,
                isCondition: isCondition,
                sensitiveLoggingEnabled: sensitiveLoggingEnabled)
        {
        }

        /// <summary>
        ///     The <see cref="IUpdateEntry" /> that represents the entity that is being modified.
        /// </summary>
        public virtual IUpdateEntry? Entry { get; }

        /// <summary>
        ///     The property that maps to the column.
        /// </summary>
        public virtual IProperty? Property { get; }

        /// <summary>
        ///     The relational type mapping for the column.
        /// </summary>
        public virtual RelationalTypeMapping? TypeMapping { get; }

        /// <summary>
        ///     A value indicating whether the column could contain a null value.
        /// </summary>
        public virtual bool? IsNullable { get; }

        /// <summary>
        ///     Indicates whether or not a value must be read from the database for the column.
        /// </summary>
        public virtual bool IsRead { get; }

        /// <summary>
        ///     Indicates whether or not a value must be written to the database for the column.
        /// </summary>
        public virtual bool IsWrite { get; }

        /// <summary>
        ///     Indicates whether or not the column is used in the <c>WHERE</c> clause when updating.
        /// </summary>
        public virtual bool IsCondition { get; }

        /// <summary>
        ///     Indicates whether or not the column is concurrency token.
        /// </summary>
        [Obsolete]
        public virtual bool IsConcurrencyToken { get; }

        /// <summary>
        ///     Indicates whether or not the column is part of a primary or alternate key.
        /// </summary>
        public virtual bool IsKey { get; }

#pragma warning disable CS8775 // Member must have a non-null value when exiting with 'true'.
        /// <summary>
        ///     Indicates whether the original value of the property must be passed as a parameter to the SQL
        /// </summary>
        [MemberNotNullWhen(true, nameof(OriginalParameterName))]
        public virtual bool UseOriginalValueParameter
            => _useParameters && IsCondition;

        /// <summary>
        ///     Indicates whether the current value of the property must be passed as a parameter to the SQL
        /// </summary>
        [MemberNotNullWhen(true, nameof(ParameterName))]
        public virtual bool UseCurrentValueParameter
            => _useParameters && IsWrite;
#pragma warning restore CS8775

        /// <summary>
        ///     The parameter name to use for the current value parameter (<see cref="UseCurrentValueParameter" />), if needed.
        /// </summary>
        public virtual string? ParameterName
            => _parameterName ??= UseCurrentValueParameter ? _generateParameterName!() : null;

        /// <summary>
        ///     The parameter name to use for the original value parameter (<see cref="UseOriginalValueParameter" />), if needed.
        /// </summary>
        public virtual string? OriginalParameterName
            => _originalParameterName ??= UseOriginalValueParameter ? _generateParameterName!() : null;

        /// <summary>
        ///     The name of the column.
        /// </summary>
        public virtual string ColumnName { get; }

        /// <summary>
        ///     The database type of the column.
        /// </summary>
        public virtual string? ColumnType { get; }

        /// <summary>
        ///     The original value of the property mapped to this column.
        /// </summary>
        public virtual object? OriginalValue
            => Entry == null
                ? _originalValue
                : Entry.SharedIdentityEntry == null
                    ? Entry.GetOriginalValue(Property!)
                    : Entry.SharedIdentityEntry.GetOriginalValue(Property!);

        /// <summary>
        ///     Gets or sets the current value of the property mapped to this column.
        /// </summary>
        public virtual object? Value
        {
            get
                => Entry == null
                    ? _value
                    : Entry.EntityState == EntityState.Deleted
                        ? null
                        : Entry.GetCurrentValue(Property!);
            set
            {
                if (Entry == null)
                {
                    _value = value;
                }
                else
                {
                    Entry.SetStoreGeneratedValue(Property!, value);
                    if (_sharedColumnModifications != null)
                    {
                        foreach (var sharedModification in _sharedColumnModifications)
                        {
                            sharedModification.Value = value;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Adds a modification affecting the same database value.
        /// </summary>
        /// <param name="modification"> The modification for the shared column. </param>
        public virtual void AddSharedColumnModification(ColumnModification modification)
        {
            Check.DebugAssert(Entry is not null, "Entry is not null");
            Check.DebugAssert(Property is not null, "Property is not null");
            Check.DebugAssert(modification.Entry is not null, "modification.Entry is not null");
            Check.DebugAssert(modification.Property is not null, "modification.Property is not null");

            _sharedColumnModifications ??= new List<ColumnModification>();

            if (UseCurrentValueParameter
                && !StructuralComparisons.StructuralEqualityComparer.Equals(Value, modification.Value))
            {
                if (_sensitiveLoggingEnabled)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingRowValuesSensitive(
                            Entry.EntityType.DisplayName(),
                            modification.Entry!.EntityType.DisplayName(),
                            Entry.BuildCurrentValuesString(Entry.EntityType.FindPrimaryKey()!.Properties),
                            Entry.BuildCurrentValuesString(new[] { Property }),
                            modification.Entry.BuildCurrentValuesString(new[] { modification.Property }),
                            ColumnName));
                }

                throw new InvalidOperationException(
                    RelationalStrings.ConflictingRowValues(
                        Entry.EntityType.DisplayName(),
                        modification.Entry.EntityType.DisplayName(),
                        new[] { Property }.Format(),
                        new[] { modification.Property }.Format(),
                        ColumnName));
            }

            if (UseOriginalValueParameter
                && !StructuralComparisons.StructuralEqualityComparer.Equals(OriginalValue, modification.OriginalValue))
            {
                if (Entry.EntityState == EntityState.Modified
                    && modification.Entry.EntityState == EntityState.Added
                    && modification.Entry.SharedIdentityEntry == null)
                {
                    modification.Entry.SetOriginalValue(modification.Property, OriginalValue);
                }
                else
                {
                    if (_sensitiveLoggingEnabled)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.ConflictingOriginalRowValuesSensitive(
                                Entry.EntityType.DisplayName(),
                                modification.Entry.EntityType.DisplayName(),
                                Entry.BuildCurrentValuesString(Entry.EntityType.FindPrimaryKey()!.Properties),
                                Entry.BuildOriginalValuesString(new[] { Property }),
                                modification.Entry.BuildOriginalValuesString(new[] { modification.Property }),
                                ColumnName));
                    }

                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingOriginalRowValues(
                            Entry.EntityType.DisplayName(),
                            modification.Entry.EntityType.DisplayName(),
                            new[] { Property }.Format(),
                            new[] { modification.Property }.Format(),
                            ColumnName));
                }
            }

            _sharedColumnModifications.Add(modification);
        }
    }
}
