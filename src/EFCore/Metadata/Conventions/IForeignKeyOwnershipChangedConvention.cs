// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when the ownership value for a foreign key is changed.
    /// </summary>
    public interface IForeignKeyOwnershipChangedConvention : IConvention
    {
        /// <summary>
        ///     Called after the ownership value for a foreign key is changed.
        /// </summary>
        /// <param name="relationshipBuilder"> The builder for the foreign key. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessForeignKeyOwnershipChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IConventionContext<bool?> context);
    }
}
