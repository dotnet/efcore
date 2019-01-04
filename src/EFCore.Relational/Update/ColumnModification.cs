// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
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
        private string _parameterName;
        private string _originalParameterName;
        private readonly Func<string> _generateParameterName;
        private readonly object _originalValue;
        private object _value;
        private readonly bool _useParameters;

        /// <summary>
        ///     Creates a new <see cref="ColumnModification" /> instance.
        /// </summary>
        /// <param name="entry"> The <see cref="IUpdateEntry" /> that represents the entity that is being modified. </param>
        /// <param name="property"> The property that maps to the column. </param>
        /// <param name="propertyAnnotations"> Provides access to relational-specific annotations for the column. </param>
        /// <param name="generateParameterName"> A delegate for generating parameter names for the update SQL. </param>
        /// <param name="isRead"> Indicates whether or not a value must be read from the database for the column. </param>
        /// <param name="isWrite"> Indicates whether or not a value must be written to the database for the column. </param>
        /// <param name="isKey"> Indicates whether or not the column part of a primary or alternate key.</param>
        /// <param name="isCondition"> Indicates whether or not the column is used in the <c>WHERE</c> clause when updating. </param>
        /// <param name="isConcurrencyToken"> Indicates whether or not the column is acting as an optimistic concurrency token. </param>
        public ColumnModification(
            [NotNull] IUpdateEntry entry,
            [NotNull] IProperty property,
            [NotNull] IRelationalPropertyAnnotations propertyAnnotations,
            [NotNull] Func<string> generateParameterName,
            bool isRead,
            bool isWrite,
            bool isKey,
            bool isCondition,
            bool isConcurrencyToken)
            : this(
                Check.NotNull(propertyAnnotations, nameof(propertyAnnotations)).ColumnName,
                originalValue: null,
                value: null,
                property: property,
                isRead: isRead,
                isWrite: isWrite,
                isKey: isKey,
                isCondition: isCondition)
        {
            Check.NotNull(entry, nameof(entry));
            Check.NotNull(property, nameof(property));
            Check.NotNull(generateParameterName, nameof(generateParameterName));

            Entry = entry;
            IsConcurrencyToken = isConcurrencyToken;
            _generateParameterName = generateParameterName;
            _useParameters = true;
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
        public ColumnModification(
            [NotNull] string columnName,
            [CanBeNull] object originalValue,
            [CanBeNull] object value,
            [CanBeNull] IProperty property,
            bool isRead,
            bool isWrite,
            bool isKey,
            bool isCondition)
        {
            Check.NotNull(columnName, nameof(columnName));

            ColumnName = columnName;
            _originalValue = originalValue;
            _value = value;
            Property = property;
            IsRead = isRead;
            IsWrite = isWrite;
            IsKey = isKey;
            IsCondition = isCondition;
        }

        /// <summary>
        ///     The <see cref="IUpdateEntry" /> that represents the entity that is being modified.
        /// </summary>
        public virtual IUpdateEntry Entry { get; }

        /// <summary>
        ///     The property that maps to the column.
        /// </summary>
        public virtual IProperty Property { get; }

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
        ///     Indicates whether or not the column is acting as an optimistic concurrency token.
        /// </summary>
        public virtual bool IsConcurrencyToken { get; }

        /// <summary>
        ///     Indicates whether or not the column part of a primary or alternate key.
        /// </summary>
        public virtual bool IsKey { get; }

        /// <summary>
        ///     Indicates whether the original value of the property must be passed as a parameter to the SQL
        /// </summary>
        public virtual bool UseOriginalValueParameter => _useParameters && IsCondition && IsConcurrencyToken;

        /// <summary>
        ///     Indicates whether the current value of the property must be passed as a parameter to the SQL
        /// </summary>
        public virtual bool UseCurrentValueParameter => _useParameters && (IsWrite || IsCondition && !IsConcurrencyToken);

        /// <summary>
        ///     The parameter name to use for the current value parameter (<see cref="UseCurrentValueParameter" />), if needed.
        /// </summary>
        public virtual string ParameterName
            => _parameterName ?? (_parameterName = _generateParameterName());

        /// <summary>
        ///     The parameter name to use for the original value parameter (<see cref="UseOriginalValueParameter" />), if needed.
        /// </summary>
        public virtual string OriginalParameterName
            => _originalParameterName ?? (_originalParameterName = _generateParameterName());

        /// <summary>
        ///     The name of the column.
        /// </summary>
        public virtual string ColumnName { get; }

        /// <summary>
        ///     The original value of the property mapped to this column.
        /// </summary>
        public virtual object OriginalValue => Entry == null ? _originalValue : Entry.GetOriginalValue(Property);

        /// <summary>
        ///     Gets or sets the current value of the property mapped to this column.
        /// </summary>
        public virtual object Value
        {
            get => Entry == null ? _value : Entry.GetCurrentValue(Property);
            [param: CanBeNull]
            set
            {
                if (Entry == null)
                {
                    _value = value;
                }
                else
                {
                    Entry.SetStoreGeneratedValue(Property, value);
                }
            }
        }
    }
}
