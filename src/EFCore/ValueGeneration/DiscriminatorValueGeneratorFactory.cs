// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
