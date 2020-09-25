// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <inheritdoc />
    public class DbContextServices : IDbContextServices
    {
        private IServiceProvider _scopedProvider;
        private IDbContextOptions _contextOptions;
        private ICurrentDbContext _currentContext;
        private IModel _modelFromSource;
        private bool _inOnModelCreating;

        /// <inheritdoc />
        public virtual IDbContextServices Initialize(
            IServiceProvider scopedProvider,
            IDbContextOptions contextOptions,
            DbContext context)
        {
            _scopedProvider = scopedProvider;
            _contextOptions = contextOptions;
            _currentContext = new CurrentDbContext(context);

            var providers = _scopedProvider.GetService<IEnumerable<IDatabaseProvider>>()?.ToList();
            var providerCount = providers?.Count ?? 0;

            if (providerCount > 1)
            {
                throw new InvalidOperationException(CoreStrings.MultipleProvidersConfigured(BuildDatabaseNamesString(providers)));
            }

            if (providerCount == 0
                || !providers[0].IsConfigured(contextOptions))
            {
                throw new InvalidOperationException(CoreStrings.NoProviderConfigured);
            }

            return this;
        }

        private static string BuildDatabaseNamesString(IEnumerable<IDatabaseProvider> available)
            => string.Join(", ", available.Select(e => "'" + e.Name + "'"));

        private IModel CreateModel()
        {
            if (_inOnModelCreating)
            {
                throw new InvalidOperationException(CoreStrings.RecursiveOnModelCreating);
            }

            try
            {
                _inOnModelCreating = true;

                var dependencies = _scopedProvider.GetService<IModelCreationDependencies>();
                return dependencies.ModelSource.GetModel(
                    _currentContext.Context,
                    dependencies.ConventionSetBuilder,
                    dependencies.ModelDependencies);
            }
            finally
            {
                _inOnModelCreating = false;
            }
        }

        /// <inheritdoc />
        public virtual ICurrentDbContext CurrentContext
            => _currentContext;

        /// <inheritdoc />
        public virtual IModel Model
            => CoreOptions?.Model
                ?? (_modelFromSource ??= CreateModel());

        private CoreOptionsExtension CoreOptions
            => _contextOptions?.FindExtension<CoreOptionsExtension>();

        /// <inheritdoc />
        public virtual IDbContextOptions ContextOptions
            => _contextOptions;

        /// <inheritdoc />
        public virtual IServiceProvider InternalServiceProvider
            => _scopedProvider;
    }
}
