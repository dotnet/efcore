// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when the base type of an entity type changes.
    /// </summary>
    public interface IEntityTypeBaseTypeChangedConvention : IConvention
    {
        /// <summary>
        ///     Called after the base type of an entity type changes.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="newBaseType"> The new base entity type. </param>
        /// <param name="oldBaseType"> The old base entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessEntityTypeBaseTypeChanged(
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] IConventionEntityType newBaseType,
            [CanBeNull] IConventionEntityType oldBaseType,
            [NotNull] IConventionContext<IConventionEntityType> context);
    }
}
