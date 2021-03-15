// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when a property is removed from the entity type.
    /// </summary>
    public interface IPropertyRemovedConvention : IConvention
    {
        /// <summary>
        ///     Called after a property is removed from the entity type.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type that contained the property. </param>
        /// <param name="property"> The removed property. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessPropertyRemoved(
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] IConventionProperty property,
            [NotNull] IConventionContext<IConventionProperty> context);
    }
}
