// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class DefaultModelSource : IModelSource
    {
        private readonly ThreadSafeDictionaryCache<Type, IModel> _models = new ThreadSafeDictionaryCache<Type, IModel>();

        private readonly EntitySetFinder _setFinder;

        public DefaultModelSource([NotNull] EntitySetFinder setFinder)
        {
            Check.NotNull(setFinder, "setFinder");

            _setFinder = setFinder;
        }

        public virtual IModel GetModel(EntityContext context)
        {
            Check.NotNull(context, "context");

            return _models.GetOrAdd(context.GetType(), k => CreateModel(context));
        }

        private IModel CreateModel(EntityContext context)
        {
            var model = new Model();
            var modelBuilder = new ConventionalModelBuilder(model);

            foreach (var setInfo in _setFinder.FindSets(context))
            {
                modelBuilder.Entity(setInfo.EntityType);
            }

            context.OnModelCreating(modelBuilder);

            return model;
        }
    }
}
