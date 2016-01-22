// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    public class RelationalValueGeneratorSelector : ValueGeneratorSelector
    {
        public RelationalValueGeneratorSelector(
            [NotNull] IValueGeneratorCache cache,
            [NotNull] IRelationalAnnotationProvider relationalExtensions)
            : base(cache)
        {
            Check.NotNull(relationalExtensions, nameof(relationalExtensions));

            RelationalExtensions = relationalExtensions;
        }

        protected virtual IRelationalAnnotationProvider RelationalExtensions { get; }

        public override ValueGenerator Create(IProperty property, IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            if ((property.DeclaringEntityType.BaseType == null)
                && (RelationalExtensions.For(property.DeclaringEntityType).DiscriminatorProperty == property))
            {
                return new DiscriminatorValueGenerator(RelationalExtensions.For(entityType).DiscriminatorValue);
            }

            return base.Create(property, entityType);
        }
    }
}
