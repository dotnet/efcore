// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a check constraint in the <see cref="IModel" />.
    /// </summary>
    public interface ICheckConstraint
    {
        /// <summary>
        ///     Gets the name of the check constraint in the database.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     The database table that contains the check constraint.
        /// </summary>
        string Table { get; }

        /// <summary>
        ///     The database table schema that contains the check constraint.
        /// </summary>
        string Schema { get; }

        /// <summary>
        ///     The <see cref="IModel" /> in which this check constraint is defined.
        /// </summary>
        IModel Model { get; }

        /// <summary>
        ///     Gets the constraint sql used in a check constraint in the database.
        /// </summary>
        string Sql { get; }
    }
}
