// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class DefaultModelSource : IModelSource
    {
        private readonly EntitySetFinder _setFinder;

        public DefaultModelSource([NotNull] EntitySetFinder setFinder)
        {
            Check.NotNull(setFinder, "setFinder");

            _setFinder = setFinder;
        }

        public virtual IModel GetModel(EntityContext context)
        {
            Check.NotNull(context, "context");

            var model = new Model();

            foreach (var setInfo in _setFinder.FindSets(context))
            {
                if (model.TryGetEntityType(setInfo.EntityType) == null)
                {
                    model.AddEntityType(new EntityType(setInfo.EntityType));
                }
            }

            // TODO: Use conventions/builder appropriately
            new SimpleTemporaryConvention().Apply(model);

            context.OnModelCreating(new ModelBuilder(model));

            return model;
        }
    }
}
