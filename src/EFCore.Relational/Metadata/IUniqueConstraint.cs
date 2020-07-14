// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a unique constraint.
    /// </summary>
    public interface IUniqueConstraint : IAnnotatable
    {
        /// <summary>
        ///     Gets the name of the unique constraint.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the mapped keys.
        /// </summary>
        IEnumerable<IKey> MappedKeys { get; }

        /// <summary>
        ///     Gets the table on with the unique constraint is declared.
        /// </summary>
        ITable Table { get; }

        /// <summary>
        ///     Gets the columns that are participating in the unique constraint.
        /// </summary>
        IReadOnlyList<IColumn> Columns { get; }
    }
}
