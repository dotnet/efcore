// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata.Conventions
{
    /// <summary>
    ///     A convention that adds etag metadata on the concurrency token, if present.
    /// </summary>
    public class ETagPropertyConvention : IModelFinalizingConvention
    {
        /// <inheritdoc />
        public virtual void ProcessModelFinalizing(
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
