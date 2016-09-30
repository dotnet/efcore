// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class Navigation : PropertyBase, IMutableNavigation
    {
        // Warning: Never access these fields directly as access needs to be thread-safe
        private IClrCollectionAccessor _collectionAccessor;
        private PropertyIndexes _indexes;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Navigation(
            [NotNull] string name,
            [CanBeNull] PropertyInfo propertyInfo,
            [CanBeNull] FieldInfo fieldInfo,
            [NotNull] ForeignKey foreignKey)
            : base(name, propertyInfo, fieldInfo)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            ForeignKey = foreignKey;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Type ClrType => PropertyInfo?.PropertyType ?? typeof(object);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ForeignKey ForeignKey { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType DeclaringEntityType
            => this.IsDependentToPrincipal()
                ? ForeignKey.DeclaringEntityType
                : ForeignKey.PrincipalEntityType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual EntityType DeclaringType => DeclaringEntityType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void PropertyMetadataChanged() => DeclaringType.PropertyMetadataChanged();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static PropertyInfo GetClrProperty(
            [NotNull] string navigationName,
            [NotNull] EntityType sourceType,
            [NotNull] EntityType targetType,
            bool shouldThrow)
        {
            var sourceClrType = sourceType.ClrType;
            var navigationProperty = sourceClrType?.GetPropertiesInHierarchy(navigationName).FirstOrDefault();
            if (!IsCompatible(navigationName, navigationProperty, sourceType, targetType, null, shouldThrow))
            {
                return null;
            }

            return navigationProperty;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsCompatible(
            [NotNull] string navigationName,
            [CanBeNull] PropertyInfo navigationProperty,
            [NotNull] EntityType sourceType,
            [NotNull] EntityType targetType,
            bool? shouldBeCollection,
            bool shouldThrow)
        {
            if (navigationProperty == null)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(CoreStrings.NoClrNavigation(navigationName, sourceType.DisplayName()));
                }
                return false;
            }

            var targetClrType = targetType.ClrType;
            if (targetClrType == null)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        CoreStrings.NavigationToShadowEntity(navigationName, sourceType.DisplayName(), targetType.DisplayName()));
                }
                return false;
            }

            return IsCompatible(navigationProperty, sourceType.ClrType, targetClrType, shouldBeCollection, shouldThrow);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsCompatible(
            [NotNull] PropertyInfo navigationProperty,
            [NotNull] Type sourceClrType,
            [NotNull] Type targetClrType,
            bool? shouldBeCollection,
            bool shouldThrow)
        {
            if (!navigationProperty.DeclaringType.GetTypeInfo().IsAssignableFrom(sourceClrType.GetTypeInfo()))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(CoreStrings.NoClrNavigation(
                        navigationProperty.Name, sourceClrType.ShortDisplayName()));
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
                            navigationProperty.Name,
                            sourceClrType.ShortDisplayName(),
                            navigationProperty.PropertyType.ShortDisplayName(),
                            targetClrType.ShortDisplayName()));
                    }
                    return false;
                }

                if (!navigationProperty.PropertyType.GetTypeInfo().IsAssignableFrom(targetClrType.GetTypeInfo()))
                {
                    if (shouldThrow)
                    {
                        throw new InvalidOperationException(CoreStrings.NavigationSingleWrongClrType(
                            navigationProperty.Name,
                            sourceClrType.ShortDisplayName(),
                            navigationProperty.PropertyType.ShortDisplayName(),
                            targetClrType.ShortDisplayName()));
                    }
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual PropertyIndexes PropertyIndexes
        {
            get
            {
                return NonCapturingLazyInitializer.EnsureInitialized(ref _indexes, this,
                    property => property.DeclaringType.CalculateIndexes(property));
            }

            [param: CanBeNull]
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
                    NonCapturingLazyInitializer.EnsureInitialized(ref _indexes, value);
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Navigation FindInverse()
            => (Navigation)((INavigation)this).FindInverse();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType GetTargetType()
            => (EntityType)((INavigation)this).GetTargetType();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IClrCollectionAccessor CollectionAccessor
            => NonCapturingLazyInitializer.EnsureInitialized(ref _collectionAccessor, this, n => new ClrCollectionAccessorFactory().Create(n));

        IForeignKey INavigation.ForeignKey => ForeignKey;
        IMutableForeignKey IMutableNavigation.ForeignKey => ForeignKey;
        IEntityType INavigation.DeclaringEntityType => DeclaringEntityType;
        IMutableEntityType IMutableNavigation.DeclaringEntityType => DeclaringEntityType;
        ITypeBase IPropertyBase.DeclaringType => DeclaringType;
        IMutableTypeBase IMutablePropertyBase.DeclaringType => DeclaringType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string ToString() => this.ToDebugString();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DebugView<Navigation> DebugView
            => new DebugView<Navigation>(this, m => m.ToDebugString(false));
    }
}
