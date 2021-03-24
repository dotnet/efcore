// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures owned entity types with defining navigation as owned entity types
    ///     without defining navigation if there's only one navigation of this type.
    /// </summary>
    [Obsolete("Entity types with defining navigations have been replaced by shared-type entity types")]
    public class OwnedTypesConvention : IEntityTypeRemovedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="OwnedTypesConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public OwnedTypesConvention(ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Called after an entity type is removed from the model.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="entityType"> The removed entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeRemoved(
            IConventionModelBuilder modelBuilder,
            IConventionEntityType entityType,
            IConventionContext<IConventionEntityType> context)
        {
        }
    }
}
