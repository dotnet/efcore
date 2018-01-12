// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DbContextServices : IDbContextServices
    {
        private IServiceProvider _scopedProvider;
        private IDbContextOptions _contextOptions;
        private ICurrentDbContext _currentContext;
        private IModel _modelFromSource;
        private bool _inOnModelCreating;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

                return _scopedProvider.GetService<IModelSource>().GetModel(
                    _currentContext.Context,
                    new CompositeConventionSetBuilder(_scopedProvider.GetService<IEnumerable<IConventionSetBuilder>>().ToList()), 
                    _scopedProvider.GetService<IModelValidator>());
            }
            finally
            {
                _inOnModelCreating = false;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ICurrentDbContext CurrentContext => _currentContext;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IModel Model
            => CoreOptions?.Model
               ?? (_modelFromSource
                   ?? (_modelFromSource = CreateModel()));

        private CoreOptionsExtension CoreOptions
            => _contextOptions?.FindExtension<CoreOptionsExtension>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IDbContextOptions ContextOptions => _contextOptions;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IServiceProvider InternalServiceProvider => _scopedProvider;
    }
}
