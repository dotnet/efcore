// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         Represents an update, insert, or delete operation for a single column. <see cref="IModificationCommand" />s
    ///         contain lists of <see cref="IColumnModification" />s.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers; it is generally not used in application code.
    ///     </para>
    /// </summary>
    public interface IColumnModification
    {
        /// <summary>
        ///     The <see cref="IUpdateEntry" /> that represents the entity that is being modified.
        /// </summary>
        public IUpdateEntry? Entry { get; }

        /// <summary>
        ///     The property that maps to the column.
        /// </summary>
        public IProperty? Property { get; }

        /// <summary>
        ///     The relational type mapping for the column.
        /// </summary>
        public RelationalTypeMapping? TypeMapping { get; }

        /// <summary>
        ///     A value indicating whether the column could contain a null value.
        /// </summary>
        public bool? IsNullable { get; }

        /// <summary>
        ///     Indicates whether or not a value must be read from the database for the column.
        /// </summary>
        public bool IsRead { get; }

        /// <summary>
        ///     Indicates whether or not a value must be written to the database for the column.
        /// </summary>
        public bool IsWrite { get; }

        /// <summary>
        ///     Indicates whether or not the column is used in the <c>WHERE</c> clause when updating.
        /// </summary>
        public bool IsCondition { get; }

        /// <summary>
        ///     Indicates whether or not the column is concurrency token.
        /// </summary>
        [Obsolete]
        public bool IsConcurrencyToken { get; }

        /// <summary>
        ///     Indicates whether or not the column is part of a primary or alternate key.
        /// </summary>
        public bool IsKey { get; }

#pragma warning disable CS8775 // Member must have a non-null value when exiting with 'true'.
        /// <summary>
        ///     Indicates whether the original value of the property must be passed as a parameter to the SQL
        /// </summary>
        [MemberNotNullWhen(true, nameof(OriginalParameterName))]
        public bool UseOriginalValueParameter { get; }

        /// <summary>
        ///     Indicates whether the current value of the property must be passed as a parameter to the SQL
        /// </summary>
        [MemberNotNullWhen(true, nameof(ParameterName))]
        public bool UseCurrentValueParameter { get; }
#pragma warning restore CS8775

        /// <summary>
        ///     The parameter name to use for the current value parameter (<see cref="UseCurrentValueParameter" />), if needed.
        /// </summary>
        public string? ParameterName { get; }

        /// <summary>
        ///     The parameter name to use for the original value parameter (<see cref="UseOriginalValueParameter" />), if needed.
        /// </summary>
        public string? OriginalParameterName { get; }

        /// <summary>
        ///     The name of the column.
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        ///     The database type of the column.
        /// </summary>
        public string? ColumnType { get; }

        /// <summary>
        ///     The original value of the property mapped to this column.
        /// </summary>
        public object? OriginalValue  { get; }

        /// <summary>
        ///     Gets or sets the current value of the property mapped to this column.
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        ///     Adds a modification affecting the same database value.
        /// </summary>
        /// <param name="modification"> The modification for the shared column. </param>
        public void AddSharedColumnModification(IColumnModification modification);
    }
}
