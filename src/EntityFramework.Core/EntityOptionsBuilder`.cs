// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity
{
    public class EntityOptionsBuilder<TContext> : EntityOptionsBuilder
        where TContext : DbContext
    {
        public EntityOptionsBuilder()
            : this(new EntityOptions<TContext>())
        {
        }

        public EntityOptionsBuilder([NotNull] EntityOptions<TContext> options)
            : base(options)
        {
        }

        public new virtual EntityOptions<TContext> Options => (EntityOptions<TContext>)base.Options;

        public new virtual EntityOptionsBuilder<TContext> UseModel([NotNull] IModel model) => (EntityOptionsBuilder<TContext>)base.UseModel(model);
    }
}
