// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         A ColumnModification factory.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers; it is generally not used in application code.
    ///     </para>
    /// </summary>
    public interface IColumnModificationFactory
    {
        /// <summary>
        ///     Creates a new <see cref="ColumnModification" /> instance.
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
        /// <returns> A new instance of ColumnModification. </returns>
        ColumnModification CreateColumnModification(
            IUpdateEntry entry,
            IProperty property,
            IColumn column,
            Func<string> generateParameterName,
            RelationalTypeMapping typeMapping,
            bool valueIsRead,
            bool valueIsWrite,
            bool columnIsKey,
            bool columnIsCondition,
            bool sensitiveLoggingEnabled);

        /// <summary>
        ///     Creates a new <see cref="ColumnModification" /> instance.
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
        /// <param name="valueIsNullable"> A value indicating whether the value could be null. </param>
        /// <returns> A new instance of ColumnModification. </returns>
        ColumnModification CreateColumnModification(
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
            bool? valueIsNullable = null);
    }
}
