// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SkipNavigation : PropertyBase, IMutableSkipNavigation, IConventionSkipNavigation
    {
        private ConfigurationSource _configurationSource;
        private ConfigurationSource? _inverseConfigurationSource;

        // Warning: Never access these fields directly as access needs to be thread-safe
        private IClrCollectionAccessor _collectionAccessor;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SkipNavigation(
            [NotNull] string name,
            [CanBeNull] PropertyInfo propertyInfo,
            [CanBeNull] FieldInfo fieldInfo,
            [NotNull] EntityType targetEntityType,
            [NotNull] ForeignKey foreignKey,
            bool collection,
            bool onPrincipal,
            ConfigurationSource configurationSource)
            : base(name, propertyInfo, fieldInfo)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            TargetEntityType = targetEntityType;
            ForeignKey = foreignKey;
            IsCollection = collection;
            IsOnPrincipal = onPrincipal;
            _configurationSource = configurationSource;
            Builder = new InternalSkipNavigationBuilder(this, targetEntityType.Model.Builder);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Type ClrType => this.GetIdentifyingMemberInfo()?.GetMemberType();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ForeignKey ForeignKey { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalSkipNavigationBuilder Builder { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override TypeBase DeclaringType => IsOnPrincipal ? ForeignKey.PrincipalEntityType : ForeignKey.DeclaringEntityType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType AssociationEntityType => IsOnPrincipal ? ForeignKey.DeclaringEntityType : ForeignKey.PrincipalEntityType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType TargetEntityType { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SkipNavigation Inverse { get; [param: NotNull] private set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsCollection { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsOnPrincipal { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource GetConfigurationSource() => _configurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool UpdateConfigurationSource(ConfigurationSource configurationSource)
        {
            var oldConfigurationSource = _configurationSource;
            _configurationSource = configurationSource.Max(_configurationSource);
            return _configurationSource != oldConfigurationSource;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionSkipNavigation SetInverse([CanBeNull] SkipNavigation inverse, ConfigurationSource configurationSource)
        {
            var oldInverse = Inverse;
            var isChanging = inverse != Inverse;
            if (inverse == null)
            {
                Inverse = null;
                _inverseConfigurationSource = null;

                return isChanging
                    ? DeclaringType.Model.ConventionDispatcher.OnSkipNavigationInverseChanged(Builder, inverse, oldInverse)
                    : inverse;
            }

            if (inverse.DeclaringType != TargetEntityType)
            {
                throw new InvalidOperationException(CoreStrings.SkipNavigationWrongInverse(
                    inverse.Name, inverse.DeclaringType.DisplayName(), Name, TargetEntityType.DisplayName()));
            }

            if (inverse.AssociationEntityType != AssociationEntityType)
            {
                throw new InvalidOperationException(CoreStrings.SkipInverseMismatchedAssociationType(
                    inverse.Name, inverse.AssociationEntityType.DisplayName(), Name, AssociationEntityType.DisplayName()));
            }

            Inverse = inverse;
            UpdateInverseConfigurationSource(configurationSource);

            return isChanging
                ? DeclaringType.Model.ConventionDispatcher.OnSkipNavigationInverseChanged(Builder, inverse, oldInverse)
                : inverse;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetInverseConfigurationSource()
            => _inverseConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void UpdateInverseConfigurationSource(ConfigurationSource configurationSource)
            => _inverseConfigurationSource = _inverseConfigurationSource.Max(configurationSource);

        /// <summary>
        ///     Runs the conventions when an annotation was set or removed.
        /// </summary>
        /// <param name="name"> The key of the set annotation. </param>
        /// <param name="annotation"> The annotation set. </param>
        /// <param name="oldAnnotation"> The old annotation. </param>
        /// <returns> The annotation that was set. </returns>
        protected override IConventionAnnotation OnAnnotationSet(
            string name, IConventionAnnotation annotation, IConventionAnnotation oldAnnotation)
            => DeclaringType.Model.ConventionDispatcher.OnSkipNavigationAnnotationChanged(
                Builder, name, annotation, oldAnnotation);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IClrCollectionAccessor CollectionAccessor
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _collectionAccessor, this, n => new ClrCollectionAccessorFactory().Create(n));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DebugView DebugView
            => new DebugView(
                () => this.ToDebugString(MetadataDebugStringOptions.ShortDefault),
                () => this.ToDebugString(MetadataDebugStringOptions.LongDefault));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public override string ToString() => this.ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        IConventionSkipNavigationBuilder IConventionSkipNavigation.Builder
        {
            [DebuggerStepThrough]
            get => Builder;
        }

        IEntityType ISkipNavigation.TargetEntityType
        {
            [DebuggerStepThrough]
            get => TargetEntityType;
        }

        IMutableEntityType IMutableSkipNavigation.TargetEntityType
        {
            [DebuggerStepThrough]
            get => TargetEntityType;
        }

        IConventionEntityType IConventionSkipNavigation.TargetEntityType
        {
            [DebuggerStepThrough]
            get => TargetEntityType;
        }

        IForeignKey ISkipNavigation.ForeignKey
        {
            [DebuggerStepThrough]
            get => ForeignKey;
        }

        IMutableForeignKey IMutableSkipNavigation.ForeignKey
        {
            [DebuggerStepThrough]
            get => ForeignKey;
        }

        IConventionForeignKey IConventionSkipNavigation.ForeignKey
        {
            [DebuggerStepThrough]
            get => ForeignKey;
        }

        ISkipNavigation ISkipNavigation.Inverse
        {
            [DebuggerStepThrough]
            get => Inverse;
        }

        IMutableSkipNavigation IMutableSkipNavigation.Inverse
        {
            [DebuggerStepThrough]
            get => Inverse;
        }

        IConventionSkipNavigation IConventionSkipNavigation.Inverse
        {
            [DebuggerStepThrough]
            get => Inverse;
        }

        [DebuggerStepThrough]
        IConventionSkipNavigation IMutableSkipNavigation.SetInverse([CanBeNull] IMutableSkipNavigation inverse)
            => SetInverse((SkipNavigation)inverse, ConfigurationSource.Explicit);

        [DebuggerStepThrough]
        IConventionSkipNavigation IConventionSkipNavigation.SetInverse([CanBeNull] IConventionSkipNavigation inverse, bool fromDataAnnotation)
            => SetInverse((SkipNavigation)inverse, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }
}
