// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Internal
{
    public abstract class ModelSource : IModelSource
    {
        private readonly ThreadSafeDictionaryCache<Type, IModel> _models = new ThreadSafeDictionaryCache<Type, IModel>();
        protected IDbSetFinder SetFinder { get; }

        protected ModelSource([NotNull] IDbSetFinder setFinder)
        {
            SetFinder = setFinder;
        }

        public virtual IModel GetModel(DbContext context, IModelBuilderFactory modelBuilderFactory, IModelValidator validator)
            => _models.GetOrAdd(context.GetType(), k => CreateModel(context, modelBuilderFactory, validator));

        protected virtual IModel CreateModel(DbContext context, IModelBuilderFactory modelBuilderFactory, IModelValidator validator)
        {
            var model = new Model();
            var modelBuilder = modelBuilderFactory.CreateConventionBuilder(model);

            FindSets(modelBuilder, context);

            OnModelCreating(context, modelBuilder);

            validator.Validate(model);

            return model;
        }

        protected virtual void FindSets(ModelBuilder modelBuilder, DbContext context)
        {
            foreach (var setInfo in SetFinder.FindSets(context))
            {
                modelBuilder.Entity(setInfo.EntityType);
            }
        }

        public static void OnModelCreating([NotNull] DbContext context, [NotNull] ModelBuilder modelBuilder)
            => context.OnModelCreating(modelBuilder);
    }
}
