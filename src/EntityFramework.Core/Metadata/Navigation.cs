// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    [DebuggerDisplay("{DeclaringEntityType.Name,nq}.{Name,nq}")]
    public class Navigation : Annotatable, IMutableNavigation
    {
        public Navigation([NotNull] string name, [NotNull] ForeignKey foreignKey)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(foreignKey, nameof(foreignKey));

            Name = name;
            ForeignKey = foreignKey;
        }

        public virtual string Name { get; }
        public virtual ForeignKey ForeignKey { get; }

        public virtual EntityType DeclaringEntityType
            => this.PointsToPrincipal()
                ? ForeignKey.DeclaringEntityType
                : ForeignKey.PrincipalEntityType;

        public override string ToString() => DeclaringEntityType + "." + Name;

        public static bool IsCompatible(
            [NotNull] string navigationPropertyName,
            [NotNull] EntityType sourceType,
            [NotNull] EntityType targetType,
            bool? shouldBeCollection,
            bool shouldThrow)
        {
            Check.NotNull(navigationPropertyName, nameof(navigationPropertyName));
            Check.NotNull(sourceType, nameof(sourceType));
            Check.NotNull(targetType, nameof(targetType));

            var sourceClrType = sourceType.ClrType;
            if (sourceClrType == null)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(CoreStrings.NavigationOnShadowEntity(navigationPropertyName, sourceType.Name));
                }
                return false;
            }

            var targetClrType = targetType.ClrType;
            if (targetClrType == null)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(CoreStrings.NavigationToShadowEntity(navigationPropertyName, sourceType.Name, targetType.Name));
                }
                return false;
            }

            var navigationProperty = sourceClrType.GetPropertiesInHierarchy(navigationPropertyName).FirstOrDefault();
            if (navigationProperty == null)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(CoreStrings.NoClrNavigation(navigationPropertyName, sourceType.Name));
                }
                return false;
            }

            var navigationTargetClrType = navigationProperty.PropertyType.TryGetSequenceType();
            if (shouldBeCollection == false
                || navigationTargetClrType == null
                || !navigationTargetClrType.GetTypeInfo().IsAssignableFrom(targetClrType.GetTypeInfo()))
            {
                if (shouldBeCollection == true)
                {
                    if (shouldThrow)
                    {
                        throw new InvalidOperationException(CoreStrings.NavigationCollectionWrongClrType(
                            navigationProperty.Name, sourceClrType.FullName, navigationProperty.PropertyType.FullName, targetClrType.FullName));
                    }
                    return false;
                }

                if (!navigationProperty.PropertyType.GetTypeInfo().IsAssignableFrom(targetClrType.GetTypeInfo()))
                {
                    if (shouldThrow)
                    {
                        throw new InvalidOperationException(CoreStrings.NavigationSingleWrongClrType(
                            navigationProperty.Name, sourceClrType.FullName, navigationProperty.PropertyType.FullName, targetClrType.FullName));
                    }
                    return false;
                }
            }

            return true;
        }

        IForeignKey INavigation.ForeignKey => ForeignKey;
        IMutableForeignKey IMutableNavigation.ForeignKey => ForeignKey;
        IEntityType IPropertyBase.DeclaringEntityType => DeclaringEntityType;
        IMutableEntityType IMutableNavigation.DeclaringEntityType => DeclaringEntityType;
    }
}
