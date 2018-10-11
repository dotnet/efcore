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
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class Navigation : PropertyBase, IMutableNavigation
    {
        // Warning: Never access these fields directly as access needs to be thread-safe
        private IClrCollectionAccessor _collectionAccessor;

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

            Builder = new InternalNavigationBuilder(this, foreignKey.DeclaringEntityType.Model.Builder);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Type ClrType => this.GetIdentifyingMemberInfo()?.GetMemberType() ?? typeof(object);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ForeignKey ForeignKey { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalNavigationBuilder Builder
        {
            [DebuggerStepThrough] get;
            [DebuggerStepThrough]
            [param: CanBeNull]
            set;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsEagerLoaded { get; set; }

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
        public override TypeBase DeclaringType
        {
            [DebuggerStepThrough] get => DeclaringEntityType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void PropertyMetadataChanged() => DeclaringType.PropertyMetadataChanged();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static MemberInfo GetClrMember(
            [NotNull] string navigationName,
            [NotNull] EntityType sourceType,
            [NotNull] EntityType targetType,
            bool shouldThrow)
        {
            var sourceClrType = sourceType.ClrType;
            var navigationProperty = sourceClrType?.GetMembersInHierarchy(navigationName).FirstOrDefault();
            return !IsCompatible(navigationName, navigationProperty, sourceType, targetType, null, shouldThrow) ? null : navigationProperty;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsCompatible(
            [NotNull] string navigationName,
            [CanBeNull] MemberInfo navigationProperty,
            [NotNull] EntityType sourceType,
            [NotNull] EntityType targetType,
            bool? shouldBeCollection,
            bool shouldThrow)
        {
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

            return navigationProperty == null
                   || IsCompatible(navigationProperty, sourceType.ClrType, targetClrType, shouldBeCollection, shouldThrow);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsCompatible(
            [NotNull] MemberInfo navigationProperty,
            [NotNull] Type sourceClrType,
            [NotNull] Type targetClrType,
            bool? shouldBeCollection,
            bool shouldThrow)
        {
            if (!navigationProperty.DeclaringType.GetTypeInfo().IsAssignableFrom(sourceClrType.GetTypeInfo()))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        CoreStrings.NoClrNavigation(
                            navigationProperty.Name, sourceClrType.ShortDisplayName()));
                }

                return false;
            }

            var navigationTargetClrType = navigationProperty.GetMemberType().TryGetSequenceType();
            if (shouldBeCollection == false
                || navigationTargetClrType?.GetTypeInfo().IsAssignableFrom(targetClrType.GetTypeInfo()) != true)
            {
                if (shouldBeCollection == true)
                {
                    if (shouldThrow)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.NavigationCollectionWrongClrType(
                                navigationProperty.Name,
                                sourceClrType.ShortDisplayName(),
                                navigationProperty.GetMemberType().ShortDisplayName(),
                                targetClrType.ShortDisplayName()));
                    }

                    return false;
                }

                if (!navigationProperty.GetMemberType().GetTypeInfo().IsAssignableFrom(targetClrType.GetTypeInfo()))
                {
                    if (shouldThrow)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.NavigationSingleWrongClrType(
                                navigationProperty.Name,
                                sourceClrType.ShortDisplayName(),
                                navigationProperty.GetMemberType().ShortDisplayName(),
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
        [DebuggerStepThrough]
        public virtual Navigation FindInverse()
            => (Navigation)((INavigation)this).FindInverse();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DebuggerStepThrough]
        public virtual EntityType GetTargetType()
            => (EntityType)((INavigation)this).GetTargetType();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IClrCollectionAccessor CollectionAccessor
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _collectionAccessor, this, n =>
                    !n.IsCollection() || n.IsShadowProperty
                        ? null
                        : new ClrCollectionAccessorFactory().Create(n));

        IForeignKey INavigation.ForeignKey
        {
            [DebuggerStepThrough]
            get => ForeignKey;
        }

        IMutableForeignKey IMutableNavigation.ForeignKey
        {
            [DebuggerStepThrough]
            get => ForeignKey;
        }

        IEntityType INavigation.DeclaringEntityType
        {
            [DebuggerStepThrough]
            get => DeclaringEntityType;
        }

        IMutableEntityType IMutableNavigation.DeclaringEntityType
        {
            [DebuggerStepThrough]
            get => DeclaringEntityType;
        }

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
