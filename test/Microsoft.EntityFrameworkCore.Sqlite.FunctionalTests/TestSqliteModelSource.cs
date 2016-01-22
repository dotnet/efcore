// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Infrastructure.Internal;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class TestSqliteModelSource : SqliteModelSource
    {
        private readonly TestModelSource _testModelSource;

        public TestSqliteModelSource(
            Action<ModelBuilder> onModelCreating,
            IDbSetFinder setFinder,
            ICoreConventionSetBuilder coreConventionSetBuilder)
            : base(setFinder, coreConventionSetBuilder, new ModelCustomizer(), new ModelCacheKeyFactory())
        {
            _testModelSource = new TestModelSource(onModelCreating, setFinder, coreConventionSetBuilder, new ModelCustomizer(), new ModelCacheKeyFactory());
        }

        public override IModel GetModel(DbContext context, IConventionSetBuilder conventionSetBuilder, IModelValidator validator) =>
            _testModelSource.GetModel(context, conventionSetBuilder, validator);

        public static Func<IServiceProvider, SqliteModelSource> GetFactory(Action<ModelBuilder> onModelCreating) =>
            p => new TestSqliteModelSource(
                onModelCreating,
                p.GetRequiredService<IDbSetFinder>(),
                p.GetRequiredService<ICoreConventionSetBuilder>());
    }
}
