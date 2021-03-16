// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when a property is added to the entity type.
    /// </summary>
    public interface IPropertyAddedConvention : IConvention
    {
        /// <summary>
        ///     Called after a property is added to the entity type.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessPropertyAdded(
            [NotNull] IConventionPropertyBuilder propertyBuilder,
            [NotNull] IConventionContext<IConventionPropertyBuilder> context);
    }
}
