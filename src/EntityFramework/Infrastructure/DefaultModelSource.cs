// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class DefaultModelSource : IModelSource
    {
        private readonly ThreadSafeDictionaryCache<Type, IModel> _models = new ThreadSafeDictionaryCache<Type, IModel>();

        private readonly DbSetFinder _setFinder;

        public DefaultModelSource([NotNull] DbSetFinder setFinder)
        {
            Check.NotNull(setFinder, "setFinder");

            _setFinder = setFinder;
        }

        public virtual IModel GetModel(DbContext context)
        {
            Check.NotNull(context, "context");

            return _models.GetOrAdd(context.GetType(), k => CreateModel(context));
        }

        private IModel CreateModel(DbContext context)
        {
            var model = new Model();
            var modelBuilder = new ConventionModelBuilder(model);

            foreach (var setInfo in _setFinder.FindSets(context))
            {
                modelBuilder.GetEntity(setInfo.EntityType);
            }

            context.OnModelCreating(modelBuilder);

            return model;
        }
    }
}
