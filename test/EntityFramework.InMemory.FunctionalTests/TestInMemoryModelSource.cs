// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class TestInMemoryModelSource : InMemoryModelSource
    {
        private readonly TestModelSource _testModelSource;

        public TestInMemoryModelSource(Action<ModelBuilder> onModelCreating, DbSetFinder setFinder, ModelValidator modelValidator)
            : base(setFinder, modelValidator)
        {
            _testModelSource = new TestModelSource(onModelCreating, setFinder);
        }

        public override IModel GetModel(DbContext context, IModelBuilderFactory modelBuilderFactory)
        {
            return _testModelSource.GetModel(context, modelBuilderFactory);
        }
        
        public static Func<IServiceProvider, IInMemoryModelSource> GetFactory(Action<ModelBuilder> onModelCreating)
        {
            return p => new TestInMemoryModelSource(onModelCreating, p.GetRequiredService<DbSetFinder>(), p.GetRequiredService<ModelValidator>());
        }
    }
}
