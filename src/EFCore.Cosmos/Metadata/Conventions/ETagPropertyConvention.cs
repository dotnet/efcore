// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata.Conventions
{
    /// <summary>
    ///     A convention that adds etag metadata on the concurrency token, if present.
    /// </summary>
    public class ETagPropertyConvention : IModelFinalizedConvention
    {
        /// <summary>
        ///     Called after a model is finalized.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public void ProcessModelFinalized(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                foreach (var property in entityType.GetDeclaredProperties())
                {
                    if (property.IsConcurrencyToken)
                    {
                        entityType.SetETagPropertyName(property.Name);
                    }
                }
            }
        }
    }
}
