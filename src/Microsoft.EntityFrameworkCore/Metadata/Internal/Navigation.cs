// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    [DebuggerDisplay("{DeclaringEntityType.Name,nq}.{Name,nq}")]
    public class Navigation : ConventionalAnnotatable, IMutableNavigation, INavigationAccessors, IPropertyIndexesAccessor
    {
        // Warning: Never access these fields directly as access needs to be thread-safe
        private IClrPropertyGetter _getter;
        private IClrPropertySetter _setter;
        private IClrCollectionAccessor _collectionAccessor;
        private PropertyAccessors _accessors;
        private PropertyIndexes _indexes;

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
            => this.IsDependentToPrincipal()
                ? ForeignKey.DeclaringEntityType
                : ForeignKey.PrincipalEntityType;

        public override string ToString() => DeclaringEntityType + "." + Name;

        public static bool IsCompatible(
            [NotNull] string navigationName,
            bool pointsToPrincipal,
            [NotNull] EntityType dependentType,
            [NotNull] EntityType principalType,
            bool shouldThrow,
            out bool? shouldBeUnique)
        {
            shouldBeUnique = null;
            if (!pointsToPrincipal)
            {
                var canBeUnique = IsCompatible(navigationName, principalType, dependentType, shouldBeCollection: false, shouldThrow: false);
                var canBeNonUnique = IsCompatible(navigationName, principalType, dependentType, shouldBeCollection: true, shouldThrow: false);

                if (canBeUnique != canBeNonUnique)
                {
                    shouldBeUnique = canBeUnique;
                }
                else if (!canBeUnique)
                {
                    if (shouldThrow)
                    {
                        IsCompatible(navigationName, principalType, dependentType, shouldBeCollection: false, shouldThrow: true);
                    }

                    return false;
                }
            }
            else if (!IsCompatible(navigationName, dependentType, principalType, shouldBeCollection: false, shouldThrow: shouldThrow))
            {
                return false;
            }

            return true;
        }

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
                    throw new InvalidOperationException(
                        CoreStrings.NavigationOnShadowEntity(navigationPropertyName, sourceType.DisplayName()));
                }
                return false;
            }

            var targetClrType = targetType.ClrType;
            if (targetClrType == null)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        CoreStrings.NavigationToShadowEntity(navigationPropertyName, sourceType.DisplayName(), targetType.DisplayName()));
                }
                return false;
            }

            var navigationProperty = sourceClrType.GetPropertiesInHierarchy(navigationPropertyName).FirstOrDefault();
            if (navigationProperty == null)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(CoreStrings.NoClrNavigation(navigationPropertyName, sourceType.DisplayName()));
                }
                return false;
            }

            var navigationTargetClrType = navigationProperty.PropertyType.TryGetSequenceType();
            if ((shouldBeCollection == false)
                || (navigationTargetClrType == null)
                || !navigationTargetClrType.GetTypeInfo().IsAssignableFrom(targetClrType.GetTypeInfo()))
            {
                if (shouldBeCollection == true)
                {
                    if (shouldThrow)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.NavigationCollectionWrongClrType(
                                navigationProperty.Name,
                                sourceClrType.Name,
                                navigationProperty.PropertyType.FullName,
                                targetClrType.FullName));
                    }
                    return false;
                }

                if (!navigationProperty.PropertyType.GetTypeInfo().IsAssignableFrom(targetClrType.GetTypeInfo()))
                {
                    if (shouldThrow)
                    {
                        throw new InvalidOperationException(CoreStrings.NavigationSingleWrongClrType(
                            navigationProperty.Name,
                            sourceClrType.Name,
                            navigationProperty.PropertyType.FullName,
                            targetClrType.FullName));
                    }
                    return false;
                }
            }

            return true;
        }

        public virtual bool IsCompatible(
            [NotNull] EntityType principalType,
            [NotNull] EntityType dependentType,
            bool? shouldPointToPrincipal,
            bool? oneToOne)
        {
            Check.NotNull(principalType, nameof(principalType));
            Check.NotNull(dependentType, nameof(dependentType));

            if ((!shouldPointToPrincipal.HasValue
                 || (this.IsDependentToPrincipal() == shouldPointToPrincipal.Value))
                && ForeignKey.IsCompatible(principalType, dependentType, oneToOne))
            {
                return true;
            }

            if (!shouldPointToPrincipal.HasValue
                && ForeignKey.IsCompatible(dependentType, principalType, oneToOne))
            {
                return true;
            }

            return false;
        }

        public virtual Navigation FindInverse()
            => (Navigation)((INavigation)this).FindInverse();

        public virtual EntityType GetTargetType()
            => (EntityType)((INavigation)this).GetTargetType();

        public virtual IClrPropertyGetter Getter
            => NonCapturingLazyInitializer.EnsureInitialized(ref _getter, this, n => new ClrPropertyGetterFactory().Create(n));

        public virtual IClrPropertySetter Setter
            => NonCapturingLazyInitializer.EnsureInitialized(ref _setter, this, n => new ClrPropertySetterFactory().Create(n));

        public virtual IClrCollectionAccessor CollectionAccessor
            => NonCapturingLazyInitializer.EnsureInitialized(ref _collectionAccessor, this, n => new ClrCollectionAccessorFactory().Create(n));

        public virtual PropertyAccessors Accessors
            => NonCapturingLazyInitializer.EnsureInitialized(ref _accessors, this, n => new PropertyAccessorsFactory().Create(n));

        public virtual PropertyIndexes PropertyIndexes
        {
            get { return NonCapturingLazyInitializer.EnsureInitialized(ref _indexes, this, n => DeclaringEntityType.CalculateIndexes(n)); }

            set
            {
                if (value == null)
                {
                    // This path should only kick in when the model is still mutable and therefore access does not need
                    // to be thread-safe.
                    _indexes = null;
                }
                else
                {
                    NonCapturingLazyInitializer.EnsureInitialized(ref _indexes, this, n => value);
                }
            }
        }

        IForeignKey INavigation.ForeignKey => ForeignKey;
        IMutableForeignKey IMutableNavigation.ForeignKey => ForeignKey;
        IEntityType IMetadataElement.DeclaringEntityType => DeclaringEntityType;
        IMutableEntityType IMutableMetadataElement.DeclaringEntityType => DeclaringEntityType;
    }
}
