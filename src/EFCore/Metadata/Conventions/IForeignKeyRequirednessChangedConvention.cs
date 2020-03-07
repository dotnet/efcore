// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when the requiredness for a foreign key is changed.
    /// </summary>
    public interface IForeignKeyRequirednessChangedConvention : IConvention
    {
        /// <summary>
        ///     Called after the requiredness for a foreign key is changed.
        /// </summary>
        /// <param name="relationshipBuilder"> The builder for the foreign key. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessForeignKeyRequirednessChanged(
            [NotNull] IConventionRelationshipBuilder relationshipBuilder,
            [NotNull] IConventionContext<IConventionRelationshipBuilder> context);
    }
}
