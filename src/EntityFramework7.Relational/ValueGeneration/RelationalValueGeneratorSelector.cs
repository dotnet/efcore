// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration
{
    public class RelationalValueGeneratorSelector : ValueGeneratorSelector
    {
        public RelationalValueGeneratorSelector([NotNull] IValueGeneratorCache cache)
            : base(cache)
        {
        }

        public override ValueGenerator Create(IProperty property, IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            if (property.DeclaringEntityType.BaseType == null
                && property.DeclaringEntityType.Relational().DiscriminatorProperty == property)
            {
                return new DiscriminatorValueGenerator(entityType.Relational().DiscriminatorValue);
            }

            return base.Create(property, entityType);
        }
    }
}
