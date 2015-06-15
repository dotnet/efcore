// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity
{
    public class DbContextOptionsBuilder<TContext> : DbContextOptionsBuilder
        where TContext : DbContext
    {
        public DbContextOptionsBuilder()
            : this(new DbContextOptions<TContext>())
        {
        }

        public DbContextOptionsBuilder([NotNull] DbContextOptions<TContext> options)
            : base(options)
        {
        }

        public new virtual DbContextOptions<TContext> Options => (DbContextOptions<TContext>)base.Options;

        public new virtual DbContextOptionsBuilder<TContext> UseModel([NotNull] IModel model) => (DbContextOptionsBuilder<TContext>)base.UseModel(model);
    }
}
