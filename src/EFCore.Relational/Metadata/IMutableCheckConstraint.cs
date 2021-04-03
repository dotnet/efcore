// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a check constraint in the <see cref="IMutableEntityType" />.
    /// </summary>
    public interface IMutableCheckConstraint : IReadOnlyCheckConstraint, IMutableAnnotatable
    {
        /// <summary>
        ///     Gets the entity type on which this check constraint is defined.
        /// </summary>
        new IMutableEntityType EntityType { get; }
    }
}
