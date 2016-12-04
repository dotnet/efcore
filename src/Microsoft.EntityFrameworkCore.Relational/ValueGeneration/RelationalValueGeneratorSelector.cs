// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    public class RelationalValueGeneratorSelector : ValueGeneratorSelector
    {
        private readonly TemporaryNumberValueGeneratorFactory _numberFactory
            = new TemporaryNumberValueGeneratorFactory();

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

            if (property.ValueGenerated != ValueGenerated.Never)
            {
                var propertyType = property.ClrType.UnwrapNullableType().UnwrapEnumType();

                if (propertyType.IsInteger()
                    || (propertyType == typeof(decimal))
                    || (propertyType == typeof(float))
                    || (propertyType == typeof(double)))
                {
                    return _numberFactory.Create(property);
                }

                if (propertyType == typeof(DateTime))
                {
                    return new TemporaryDateTimeValueGenerator();
                }

                if (propertyType == typeof(DateTimeOffset))
                {
                    return new TemporaryDateTimeOffsetValueGenerator();
                }
            }

            return base.Create(property, entityType);
        }
    }
}
