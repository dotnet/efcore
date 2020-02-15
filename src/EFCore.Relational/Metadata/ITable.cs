// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a table in the database.
    /// </summary>
    public interface ITable : ITableBase
    {
        /// <summary>
        ///     The entity type mappings.
        /// </summary>
        new IEnumerable<ITableMapping> EntityTypeMappings { get; }

        /// <summary>
        ///     The columns defined for this table.
        /// </summary>
        new IEnumerable<IColumn> Columns { get; }

        /// <summary>
        ///     Indicates whether the table should be managed by migrations
        /// </summary>
        bool IsMigratable { get; }

        /// <summary>
        ///     The check constraints for this table.
        /// </summary>
        IEnumerable<ICheckConstraint> CheckConstraints
            => EntityTypeMappings.SelectMany(m => CheckConstraint.GetCheckConstraints(m.EntityType))
                .Distinct((x, y) => x.Name == y.Name);

        /// <summary>
        ///     The comment for this table.
        /// </summary>
        public virtual string Comment
            => EntityTypeMappings.Select(e => e.EntityType.GetComment()).FirstOrDefault(c => c != null);

        /// <summary>
        ///     Returns the column with a given name. Returns <c>null</c> if no column with the given name is defined.
        /// </summary>
        new IColumn FindColumn([NotNull] string name);
    }
}
