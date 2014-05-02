// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
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
