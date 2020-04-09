// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Cosmos.ValueGeneration.Internal
{
    public class CosmosValueGeneratorSelector : ValueGeneratorSelector
    {
        public CosmosValueGeneratorSelector([NotNull] ValueGeneratorSelectorDependencies dependencies)
            : base(dependencies)
        {
        }

        public override ValueGenerator Create(IProperty property, IEntityType entityType)
        {
            var type = property.ClrType.UnwrapNullableType().UnwrapEnumType();

            if (property.GetJsonPropertyName() == ""
                && type == typeof(int))
            {
                return new TemporaryNumberValueGeneratorFactory().Create(property);
            }

            return base.Create(property, entityType);
        }
    }
}
