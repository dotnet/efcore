// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when a foreign key is added to the entity type.
    /// </summary>
    public interface IForeignKeyAddedConvention : IConvention
    {
        /// <summary>
        ///     Called after a foreign key is added to the entity type.
        /// </summary>
        /// <param name="foreignKeyBuilder"> The builder for the foreign key. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessForeignKeyAdded(
            IConventionForeignKeyBuilder foreignKeyBuilder,
            IConventionContext<IConventionForeignKeyBuilder> context);
    }
}
