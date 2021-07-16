// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
