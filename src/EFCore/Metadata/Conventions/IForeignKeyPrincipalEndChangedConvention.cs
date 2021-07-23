// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when the principal end of a foreign key is changed.
    /// </summary>
    public interface IForeignKeyPrincipalEndChangedConvention : IConvention
    {
        /// <summary>
        ///     Called after the principal end of a foreign key is changed.
        /// </summary>
        /// <param name="relationshipBuilder"> The builder for the foreign key. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessForeignKeyPrincipalEndChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IConventionContext<IConventionForeignKeyBuilder> context);
    }
}
