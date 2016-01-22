// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    public class DiscriminatorBuilder<TDiscriminator>
    {
        public DiscriminatorBuilder([NotNull] DiscriminatorBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            Builder = builder;
        }

        private DiscriminatorBuilder Builder { get; }

        public virtual DiscriminatorBuilder<TDiscriminator> HasValue([CanBeNull] TDiscriminator value)
        {
            var builder = Builder.HasValue(value);
            return builder == null ? null : new DiscriminatorBuilder<TDiscriminator>(builder);
        }

        public virtual DiscriminatorBuilder<TDiscriminator> HasValue<TEntity>([CanBeNull] TDiscriminator value)
            => HasValue(typeof(TEntity), value);

        public virtual DiscriminatorBuilder<TDiscriminator> HasValue([NotNull] Type entityType, [CanBeNull] TDiscriminator value)
        {
            var builder = Builder.HasValue(entityType, value);
            return builder == null ? null : new DiscriminatorBuilder<TDiscriminator>(builder);
        }

        public virtual DiscriminatorBuilder<TDiscriminator> HasValue([NotNull] string entityTypeName, [CanBeNull] TDiscriminator value)
        {
            var builder = Builder.HasValue(entityTypeName, value);
            return builder == null ? null : new DiscriminatorBuilder<TDiscriminator>(builder);
        }
    }
}
