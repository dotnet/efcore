// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Navigation : PropertyBase, INavigation
    {
        public Navigation([NotNull] string name, [NotNull] ForeignKey foreignKey)
            : base(Check.NotEmpty(name, nameof(name)))
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            ForeignKey = foreignKey;
        }

        public virtual ForeignKey ForeignKey { get; }

        public override EntityType DeclaringEntityType
            => this.PointsToPrincipal()
                ? ForeignKey.DeclaringEntityType
                : ForeignKey.PrincipalEntityType;

        IForeignKey INavigation.ForeignKey => ForeignKey;

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
                    throw new InvalidOperationException(Strings.NavigationOnShadowEntity(navigationPropertyName, sourceType.Name));
                }
                return false;
            }

            var targetClrType = targetType.ClrType;
            if (targetClrType == null)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(Strings.NavigationToShadowEntity(navigationPropertyName, sourceType.Name, targetType.Name));
                }
                return false;
            }

            var navigationProperty = sourceClrType.GetPropertiesInHierarchy(navigationPropertyName).FirstOrDefault();
            if (navigationProperty == null)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(Strings.NoClrNavigation(navigationPropertyName, sourceType.Name));
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
                        throw new InvalidOperationException(Strings.NavigationCollectionWrongClrType(
                            navigationProperty.Name, sourceClrType.FullName, navigationProperty.PropertyType.FullName, targetClrType.FullName));
                    }
                    return false;
                }

                if (!navigationProperty.PropertyType.GetTypeInfo().IsAssignableFrom(targetClrType.GetTypeInfo()))
                {
                    if (shouldThrow)
                    {
                        throw new InvalidOperationException(Strings.NavigationSingleWrongClrType(
                            navigationProperty.Name, sourceClrType.FullName, navigationProperty.PropertyType.FullName, targetClrType.FullName));
                    }
                    return false;
                }
            }

            return true;
        }
    }
}
