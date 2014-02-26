// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class DefaultModelLoader : ModelLoader
    {
        public override IModel LoadModel(EntityContext context)
        {
            Check.NotNull(context, "context");

            var model = new Model();

            var setProperties = context.GetType().GetRuntimeProperties()
                .Where(
                    p => !p.IsStatic()
                         && !p.GetIndexParameters().Any()
                         && p.DeclaringType != typeof(EntityContext)
                         && p.PropertyType.GetTypeInfo().IsGenericType
                         && p.PropertyType.GetGenericTypeDefinition() == typeof(EntitySet<>))
                .OrderBy(p => p.Name);

            foreach (var setProperty in setProperties)
            {
                var type = setProperty.PropertyType.GetTypeInfo().GenericTypeArguments.Single();

                if (model.EntityType(type) == null)
                {
                    model.AddEntityType(new EntityType(type));
                }
            }

            // TODO: Use conventions/builder appropriately
            new SimpleTemporaryConvention().Apply(model);

            // TODO: Initialize context EntitySets
            return model;
        }
    }
}
