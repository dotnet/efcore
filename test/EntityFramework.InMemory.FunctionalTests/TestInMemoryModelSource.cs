// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class TestInMemoryModelSource : InMemoryModelSource
    {
        private readonly TestModelSource _testModelSource;

        public TestInMemoryModelSource(Action<ModelBuilder> onModelCreating, IDbSetFinder setFinder)
            : base(setFinder)
        {
            _testModelSource = new TestModelSource(onModelCreating, setFinder);
        }

        public override IModel GetModel(DbContext context, IModelBuilderFactory modelBuilderFactory, IModelValidator validator) 
            => _testModelSource.GetModel(context, modelBuilderFactory, validator);

        public static Func<IServiceProvider, InMemoryModelSource> GetFactory(Action<ModelBuilder> onModelCreating) 
            => p => new TestInMemoryModelSource(onModelCreating, p.GetRequiredService<IDbSetFinder>());
    }
}
