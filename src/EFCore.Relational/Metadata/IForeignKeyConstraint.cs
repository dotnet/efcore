// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a foreign key constraint.
    /// </summary>
    public interface IForeignKeyConstraint : IAnnotatable
    {
        /// <summary>
        ///     Gets the name of the foreign key constraint.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the mapped foreign keys.
        /// </summary>
        IEnumerable<IForeignKey> MappedForeignKeys { get; }

        /// <summary>
        ///     Gets the table on with the foreign key constraint is declared.
        /// </summary>
        ITable Table { get; }

        /// <summary>
        ///     Gets the table that is referenced by the foreign key constraint.
        /// </summary>
        ITable PrincipalTable { get; }

        /// <summary>
        ///     Gets the columns that are participating in the foreign key constraint.
        /// </summary>
        IReadOnlyList<IColumn> Columns { get; }

        /// <summary>
        ///     Gets the columns that are referenced by the foreign key constraint.
        /// </summary>
        IReadOnlyList<IColumn> PrincipalColumns { get; }

        /// <summary>
        ///     Gets the action to be performed when the referenced row is deleted.
        /// </summary>
        ReferentialAction OnDeleteAction { get; }
    }
}
