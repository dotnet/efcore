// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a check constraint in the <see cref="IReadOnlyEntityType" />.
    /// </summary>
    public interface IReadOnlyCheckConstraint : IReadOnlyAnnotatable
    {
        /// <summary>
        ///     Gets the name of the check constraint in the database.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the <see cref="IReadOnlyEntityType" /> in which this check constraint is defined.
        /// </summary>
        IReadOnlyEntityType EntityType { get; }

        /// <summary>
        ///     Gets the constraint sql used in a check constraint in the database.
        /// </summary>
        string Sql { get; }
    }
}
