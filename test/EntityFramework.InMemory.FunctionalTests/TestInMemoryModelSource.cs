// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class TestInMemoryModelSource : InMemoryModelSource
    {
        private readonly Action<ModelBuilder> _onModelCreating;

        public TestInMemoryModelSource(Action<ModelBuilder> onModelCreating)
            : base(new DbSetFinder())
        {
            _onModelCreating = onModelCreating;
        }

        protected override IModel CreateModel(DbContext context, ModelBuilderFactory modelBuilderFactory)
        {
            var model = new Model();
            var modelBuilder = modelBuilderFactory.CreateConventionBuilder(model);

            FindSets(modelBuilder, context);

            _onModelCreating(modelBuilder);

            return model;
        }
    }
}
