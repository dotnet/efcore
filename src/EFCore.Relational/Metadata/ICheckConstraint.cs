// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a check constraint in the <see cref="IEntityType" />.
    /// </summary>
    public interface ICheckConstraint : IReadOnlyCheckConstraint, IAnnotatable
    {
        /// <summary>
        ///     Gets the <see cref="IEntityType" /> in which this check constraint is defined.
        /// </summary>
        new IEntityType EntityType { get; }
    }
}
