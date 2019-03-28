// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when the nullability for a property is changed.
    /// </summary>
    public interface IPropertyNullabilityChangedConvention : IConvention
    {
        /// <summary>
        ///     Called after the nullability for a property is changed.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessPropertyNullabilityChanged(
            [NotNull] IConventionPropertyBuilder propertyBuilder,
            [NotNull] IConventionContext<IConventionPropertyBuilder> context);
    }
}
