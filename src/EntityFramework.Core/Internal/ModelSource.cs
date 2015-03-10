// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Internal
{
    public class ModelSource : IModelSource
    {
        private readonly ThreadSafeDictionaryCache<Type, IModel> _models = new ThreadSafeDictionaryCache<Type, IModel>();
        
        public ModelSource([NotNull] DbSetFinder setFinder, [NotNull] IModelValidator modelValidator)
        {
            SetFinder = setFinder;
            Validator = modelValidator;
        }

        protected DbSetFinder SetFinder { get; }
        protected IModelValidator Validator { get; }

        public virtual IModel GetModel(DbContext context, IModelBuilderFactory modelBuilderFactory) 
            => _models.GetOrAdd(context.GetType(), k => CreateModel(context, modelBuilderFactory));

        protected virtual IModel CreateModel(DbContext context, IModelBuilderFactory modelBuilderFactory)
        {
            var model = new Model();
            var modelBuilder = modelBuilderFactory.CreateConventionBuilder(model);

            FindSets(modelBuilder, context);

            OnModelCreating(context, modelBuilder);

            Validator.Validate(model);

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
