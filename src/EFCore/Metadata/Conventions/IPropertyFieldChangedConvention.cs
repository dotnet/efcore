// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when the backing field for a property is changed.
    /// </summary>
    public interface IPropertyFieldChangedConvention : IConvention
    {
        /// <summary>
        ///     Called after the backing field for a property is changed.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property. </param>
        /// <param name="newFieldInfo"> The new field. </param>
        /// <param name="oldFieldInfo"> The old field. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessPropertyFieldChanged(
            [NotNull] IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] FieldInfo newFieldInfo,
            [CanBeNull] FieldInfo oldFieldInfo,
            [NotNull] IConventionContext<FieldInfo> context);
    }
}
