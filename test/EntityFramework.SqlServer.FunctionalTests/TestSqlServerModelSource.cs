// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class TestSqlServerModelSource : SqlServerModelSource
    {
        private readonly TestModelSource _testModelSource;

        public TestSqlServerModelSource(Action<ModelBuilder> onModelCreating, IDbSetFinder setFinder, IModelValidator modelValidator)
            : base(setFinder, modelValidator)
        {
            _testModelSource = new TestModelSource(onModelCreating, setFinder);
        }

        public override IModel GetModel(DbContext context, IModelBuilderFactory modelBuilderFactory) 
            => _testModelSource.GetModel(context, modelBuilderFactory);

        public static Func<IServiceProvider, ISqlServerModelSource> GetFactory(Action<ModelBuilder> onModelCreating) 
            => p => new TestSqlServerModelSource(onModelCreating, p.GetRequiredService<IDbSetFinder>(), p.GetRequiredService<IModelValidator>());
    }
}
