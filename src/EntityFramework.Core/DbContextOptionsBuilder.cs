// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class DbContextOptionsBuilder : IDbContextOptionsBuilderInfrastructure
    {
        private DbContextOptions _options;

        public DbContextOptionsBuilder()
            : this(new DbContextOptions<DbContext>())
        {
        }

        public DbContextOptionsBuilder([NotNull] DbContextOptions options)
        {
            Check.NotNull(options, nameof(options));

            _options = options;
        }

        public virtual DbContextOptions Options => _options;

        public virtual bool IsConfigured => _options.Extensions.Any();

        public virtual DbContextOptionsBuilder UseModel([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            ((IDbContextOptionsBuilderInfrastructure)this).AddOrUpdateExtension(new CoreOptionsExtension { Model = model });

            return this;
        }

        void IDbContextOptionsBuilderInfrastructure.AddOrUpdateExtension<TExtension>(TExtension extension)
        {
            Check.NotNull(extension, nameof(extension));

            _options = _options.WithExtension(extension);
        }
    }
}
