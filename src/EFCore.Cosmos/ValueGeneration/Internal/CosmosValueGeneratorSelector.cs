// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.ValueGeneration.Internal
{
    public class CosmosValueGeneratorSelector : ValueGeneratorSelector
    {
        public CosmosValueGeneratorSelector(ValueGeneratorSelectorDependencies dependencies)
            : base(dependencies)
        {
        }

        public override ValueGenerator Create(IProperty property, IEntityType entityType)
        {
            var type = property.ClrType.UnwrapNullableType().UnwrapEnumType();

            if (property.GetPropertyName() == ""
                && type == typeof(int))
            {
                return new TemporaryIntValueGenerator();
            }

            return base.Create(property, entityType);
        }
    }
}
