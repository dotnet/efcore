// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
    ///         Parameters for creating a <see cref="ColumnModification" /> instance.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers; it is generally not used in application code.
    ///     </para>
    /// </summary>
    public struct ColumnModificationParameters
    {
#if DEBUG
        /// <summary>
        ///   Internal Debug API.
        /// </summary>
        public enum EnumDebugInitKind
        {
            /// <summary>
            ///   Internal Debug API.
            /// </summary>
            Kind001=1,

           /// <summary>
           ///   Internal Debug API.
           /// </summary>
            Kind002=1,
        };//enum EnumDebugInitKind

        /// <summary>
        ///   Internal Debug API.
        /// </summary>
        public readonly EnumDebugInitKind DebugInitKind;
#endif

        /// <summary>
        ///  A delegate for generating parameter names for the update SQL.
        /// </summary>
        public Func<string>? GenerateParameterName;

        /// <summary>
        ///  The original value of the property mapped to column.
        /// </summary>
        public object? OriginalValue;

        /// <summary>
        ///  Current value of the property mapped to column.
        /// </summary>
        public object? Value;

        /// <summary>
        ///  Indicates whether or not potentially sensitive data (e.g. database values) can be logged.
        /// </summary>
        public bool SensitiveLoggingEnabled;

        /// <summary>
        ///     The <see cref="IUpdateEntry" /> that represents the entity that is being modified.
        /// </summary>
        public IUpdateEntry? Entry;

        /// <summary>
        ///     The property that maps to the column.
        /// </summary>
        public IProperty? Property;

        /// <summary>
        ///     The relational type mapping for the column.
        /// </summary>
        public RelationalTypeMapping? TypeMapping;

        /// <summary>
        ///     A value indicating whether the column could contain a null value.
        /// </summary>
        public bool? IsNullable;

        /// <summary>
        ///     Indicates whether or not a value must be read from the database for the column.
        /// </summary>
        public bool IsRead;

        /// <summary>
        ///     Indicates whether or not a value must be written to the database for the column.
        /// </summary>
        public bool IsWrite;

        /// <summary>
        ///     Indicates whether or not the column is used in the <c>WHERE</c> clause when updating.
        /// </summary>
        public bool IsCondition;

        // /// <summary>
        // ///     Indicates whether or not the column is concurrency token.
        // /// </summary>
        // [Obsolete]
        // public bool IsConcurrencyToken;

        /// <summary>
        ///     Indicates whether or not the column is part of a primary or alternate key.
        /// </summary>
        public bool IsKey;

        /// <summary>
        ///     The name of the column.
        /// </summary>
        public string ColumnName;

        /// <summary>
        ///     The database type of the column.
        /// </summary>
        public string? ColumnType;

        /// <summary>
        ///     Creates a new <see cref="ColumnModificationParameters" /> instance.
        /// </summary>
        /// <param name="columnName"> The name of the column. </param>
        /// <param name="originalValue"> The original value of the property mapped to this column. </param>
        /// <param name="value"> Gets or sets the current value of the property mapped to this column. </param>
        /// <param name="property"> The property that maps to the column. </param>
        /// <param name="columnType"> The database type of the column. </param>
        /// <param name="typeMapping"> The relational type mapping to be used for the command parameter. </param>
        /// <param name="valueIsRead"> Indicates whether or not a value must be read from the database for the column. </param>
        /// <param name="valueIsWrite"> Indicates whether or not a value must be written to the database for the column. </param>
        /// <param name="columnIsKey"> Indicates whether or not the column part of a primary or alternate key.</param>
        /// <param name="columnIsCondition"> Indicates whether or not the column is used in the <c>WHERE</c> clause when updating. </param>
        /// <param name="sensitiveLoggingEnabled"> Indicates whether or not potentially sensitive data (e.g. database values) can be logged. </param>
        /// <param name="isNullable"> A value indicating whether the value could be null. </param>
        public ColumnModificationParameters(
            string columnName,
            object? originalValue,
            object? value,
            IProperty? property,
            string? columnType,
            RelationalTypeMapping? typeMapping,
            bool valueIsRead,
            bool valueIsWrite,
            bool columnIsKey,
            bool columnIsCondition,
            bool sensitiveLoggingEnabled,
            bool? isNullable = null)
        {
            Check.NotNull(columnName, nameof(columnName));

#if DEBUG
            this.DebugInitKind=EnumDebugInitKind.Kind001;
#endif

            this.ColumnName = columnName;
            this.OriginalValue = originalValue;
            this.Value = value;
            this.Property = property;
            this.ColumnType = columnType;
            this.TypeMapping = typeMapping;
            this.IsRead = valueIsRead;
            this.IsWrite = valueIsWrite;
            this.IsKey = columnIsKey;
            this.IsCondition = columnIsCondition;
            this.SensitiveLoggingEnabled = sensitiveLoggingEnabled;
            this.IsNullable = isNullable;

            this.GenerateParameterName = null;
            this.Entry = null;

            //this.IsConcurrencyToken = false;
        }

        /// <summary>
        ///     Creates a new <see cref="ColumnModificationParameters" /> instance.
        /// </summary>
        /// <param name="entry"> The <see cref="IUpdateEntry" /> that represents the entity that is being modified. </param>
        /// <param name="property"> The property that maps to the column. </param>
        /// <param name="column"> The column to be modified. </param>
        /// <param name="generateParameterName"> A delegate for generating parameter names for the update SQL. </param>
        /// <param name="typeMapping"> The relational type mapping to be used for the command parameter. </param>
        /// <param name="valueIsRead"> Indicates whether or not a value must be read from the database for the column. </param>
        /// <param name="valueIsWrite"> Indicates whether or not a value must be written to the database for the column. </param>
        /// <param name="columnIsKey"> Indicates whether or not the column part of a primary or alternate key.</param>
        /// <param name="columnIsCondition"> Indicates whether or not the column is used in the <c>WHERE</c> clause when updating. </param>
        /// <param name="sensitiveLoggingEnabled"> Indicates whether or not potentially sensitive data (e.g. database values) can be logged. </param>
        public ColumnModificationParameters(
            IUpdateEntry entry,
            IProperty property,
            IColumn column,
            Func<string> generateParameterName,
            RelationalTypeMapping typeMapping,
            bool valueIsRead,
            bool valueIsWrite,
            bool columnIsKey,
            bool columnIsCondition,
            bool sensitiveLoggingEnabled)
        {
            Check.NotNull(entry, nameof(entry));
            Check.NotNull(property, nameof(property));
            Check.NotNull(column, nameof(column));
            Check.NotNull(column.Name, "column.Name");
            Check.NotNull(generateParameterName, nameof(generateParameterName));

#if DEBUG
            this.DebugInitKind=EnumDebugInitKind.Kind002;
#endif

            this.ColumnName = column.Name;
            this.OriginalValue = null;
            this.Value = null;
            this.Property = property;
            this.ColumnType = column.StoreType;
            this.TypeMapping = typeMapping;
            this.IsRead = valueIsRead;
            this.IsWrite = valueIsWrite;
            this.IsKey = columnIsKey;
            this.IsCondition = columnIsCondition;
            this.SensitiveLoggingEnabled = sensitiveLoggingEnabled;
            this.IsNullable = column.IsNullable;

            this.GenerateParameterName = generateParameterName;
            this.Entry = entry;

            //this.IsConcurrencyToken = false;
        }
    };//struct ColumnModificationParameters
}
