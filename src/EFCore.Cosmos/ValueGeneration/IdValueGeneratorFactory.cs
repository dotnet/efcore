// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Cosmos.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Cosmos.ValueGeneration
{
    /// <summary>
    ///     A factory that creates value generators for the 'id' property that combines the primary key values.
    /// </summary>
    public class IdValueGeneratorFactory : ValueGeneratorFactory
    {
        /// <inheritdoc />
        public override ValueGenerator Create(IProperty property, IEntityType entityType)
            => new IdValueGenerator();
    }
}
