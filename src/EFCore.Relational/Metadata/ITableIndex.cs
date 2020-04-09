// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a table index.
    /// </summary>
    public interface ITableIndex : IAnnotatable
    {
        /// <summary>
        ///     Gets the name of the index.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the mapped indexes.
        /// </summary>
        IEnumerable<IIndex> MappedIndexes { get; }

        /// <summary>
        ///     Gets the table on with the index is declared.
        /// </summary>
        ITable Table { get; }

        /// <summary>
        ///     Gets the columns that are participating in the index.
        /// </summary>
        IReadOnlyList<IColumn> Columns { get; }

        /// <summary>
        ///     Gets a value indicating whether the index enforces uniqueness.
        /// </summary>
        bool IsUnique { get; }

        /// <summary>
        ///     Gets the expression used as the index filter.
        /// </summary>
        string Filter { get; }
    }
}
