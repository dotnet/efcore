// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public abstract class NavigationAttributeNavigationConvention<TAttribute> : INavigationConvention
        where TAttribute : Attribute
    {
        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder, Navigation navigation)
        {
            Check.NotNull(relationshipBuilder, nameof(relationshipBuilder));
            Check.NotNull(navigation, nameof(navigation));

            var attributes = navigation.DeclaringEntityType?.ClrType?.GetRuntimeProperties().FirstOrDefault(pi => pi.Name == navigation.Name)?.GetCustomAttributes<TAttribute>(true);

            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    relationshipBuilder = Apply(relationshipBuilder, navigation, attribute);
                    if (relationshipBuilder == null)
                    {
                        break;
                    }
                }
            }
            return relationshipBuilder;
        }

        public abstract InternalRelationshipBuilder Apply([NotNull] InternalRelationshipBuilder relationshipBuilder, [NotNull] Navigation navigation, [NotNull] TAttribute attribute);
    }
}
