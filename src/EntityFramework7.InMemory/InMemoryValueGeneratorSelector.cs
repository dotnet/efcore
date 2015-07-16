// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryValueGeneratorSelector : ValueGeneratorSelector
    {
        private readonly InMemoryIntegerValueGeneratorFactory _inMemoryFactory = new InMemoryIntegerValueGeneratorFactory();

        public InMemoryValueGeneratorSelector([NotNull] IValueGeneratorCache cache)
            : base(cache)
        {
        }

        public override ValueGenerator Create(IProperty property, IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            return property.ClrType.IsInteger()
                ? _inMemoryFactory.Create(property)
                : base.Create(property, entityType);
        }
    }
}
