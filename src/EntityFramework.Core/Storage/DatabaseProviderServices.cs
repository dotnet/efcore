// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DatabaseProviderServices : IDatabaseProviderServices
    {
        protected DatabaseProviderServices([NotNull] IServiceProvider services)
        {
            Check.NotNull(services, nameof(services));

            Services = services;
        }

        public abstract string InvariantName { get; }

        protected virtual IServiceProvider Services { get; }

        protected virtual TService GetService<TService>() => Services.GetRequiredService<TService>();

        public virtual IConventionSetBuilder ConventionSetBuilder => null;
        public virtual IValueGeneratorSelector ValueGeneratorSelector => GetService<ValueGeneratorSelector>();
        public virtual IModelValidator ModelValidator => GetService<LoggingModelValidator>();
        public virtual ICompiledQueryCacheKeyGenerator CompiledQueryCacheKeyGenerator
            => GetService<CompiledQueryCacheKeyGenerator>();

        public abstract IDatabase Database { get; }
        public abstract IDatabaseCreator Creator { get; }
        public abstract IModelSource ModelSource { get; }
        public abstract IValueGeneratorCache ValueGeneratorCache { get; }
        public abstract IQueryContextFactory QueryContextFactory { get; }
        public abstract IQueryCompilationContextFactory QueryCompilationContextFactory { get; }
    }
}
