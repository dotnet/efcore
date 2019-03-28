// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when the foreign key properties or principal key are changed.
    /// </summary>
    public interface IForeignKeyPropertiesChangedConvention : IConvention
    {
        /// <summary>
        ///     Called after the foreign key properties or principal key are changed.
        /// </summary>
        /// <param name="relationshipBuilder"> The builder for the foreign key. </param>
        /// <param name="oldDependentProperties"> The old foreign key properties. </param>
        /// <param name="oldPrincipalKey"> The old principal key. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessForeignKeyPropertiesChanged(
            [NotNull] IConventionRelationshipBuilder relationshipBuilder,
            [NotNull] IReadOnlyList<IConventionProperty> oldDependentProperties,
            [NotNull] IConventionKey oldPrincipalKey,
            [NotNull] IConventionContext<IConventionRelationshipBuilder> context);
    }
}
