// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    /// <summary>
    ///     A factory that creates value generators for the discriminator property that always outputs
    ///     the discriminator value for the given entity type.
    /// </summary>
    public class DiscriminatorValueGeneratorFactory : ValueGeneratorFactory
    {
        /// <inheritdoc />
        public override ValueGenerator Create(IProperty property, IEntityType entityType)
            => new DiscriminatorValueGenerator(entityType.GetDiscriminatorValue()!);
    }
}
