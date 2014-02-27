// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class EntitySetFinder
    {
        public virtual PropertyInfo[] FindSets([NotNull] EntityContext context)
        {
            Check.NotNull(context, "context");

            return context.GetType().GetRuntimeProperties()
                .Where(
                    p => !p.IsStatic()
                         && !p.GetIndexParameters().Any()
                         && p.DeclaringType != typeof(EntityContext)
                         && p.PropertyType.GetTypeInfo().IsGenericType
                         && p.PropertyType.GetGenericTypeDefinition() == typeof(EntitySet<>))
                .OrderBy(p => p.Name)
                .ToArray();
        }
    }
}
