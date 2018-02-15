// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Properties for relational-specific annotations accessed through
    ///     <see cref="RelationalMetadataExtensions.Relational(IProperty)" />.
    /// </summary>
    public interface IRelationalPropertyAnnotations
    {
        /// <summary>
        ///     The name of the column to which the property is mapped.
        /// </summary>
        string ColumnName { get; }

        /// <summary>
        ///     The database type of the column to which the property is mapped.
        /// </summary>
        string ColumnType { get; }

        /// <summary>
        ///     The default constraint SQL expression that should be used when creating a column for this property.
        /// </summary>
        string DefaultValueSql { get; }

        /// <summary>
        ///     The computed constraint SQL expression that should be used when creating a column for this property.
        /// </summary>
        string ComputedColumnSql { get; }

        /// <summary>
        ///     The default value to use in the definition of the column when creating a column for this property.
        /// </summary>
        object DefaultValue { get; }

        /// <summary>
        ///     A flag indicating if the property as capable of storing only fixed-length data, such as strings.
        /// </summary>
        bool IsFixedLength { get; }
    }
}
