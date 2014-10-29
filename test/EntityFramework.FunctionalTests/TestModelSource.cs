// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public class TestModelSource : IModelSource
    {
        private static readonly DbSetFinder _setFinder = new DbSetFinder();
        private readonly Action<ModelBuilder> _onModelCreating;

        public TestModelSource(Action<ModelBuilder> onModelCreating)
        {
            _onModelCreating = onModelCreating;
        }

        public IModel GetModel(DbContext context, IModelBuilderFactory modelBuilderFactory)
        {
            var model = new Model();
            var modelBuilder = modelBuilderFactory.CreateConventionBuilder(model);

            foreach (var setInfo in _setFinder.FindSets(context))
            {
                modelBuilder.Entity(setInfo.EntityType);
            }

            if (_onModelCreating == null)
            {
                ModelSourceHelpers.OnModelCreating(context, modelBuilder);
            }
            else
            {
                _onModelCreating(modelBuilder);
            }

            return model;
        }
    }
}
