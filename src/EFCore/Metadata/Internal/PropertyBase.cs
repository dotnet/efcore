// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract class PropertyBase : ConventionAnnotatable, IMutablePropertyBase, IConventionPropertyBase
    {
        private FieldInfo _fieldInfo;
        private ConfigurationSource? _fieldInfoConfigurationSource;

        // Warning: Never access these fields directly as access needs to be thread-safe
        private IClrPropertyGetter _getter;
        private IClrPropertySetter _setter;
        private IClrPropertySetter _materializationSetter;
        private PropertyAccessors _accessors;
        private PropertyIndexes _indexes;
        private IComparer<IUpdateEntry> _currentValueComparer;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected PropertyBase(
            [NotNull] string name,
            [CanBeNull] PropertyInfo propertyInfo,
            [CanBeNull] FieldInfo fieldInfo)
        {
            Check.NotEmpty(name, nameof(name));

            Name = name;
            PropertyInfo = propertyInfo;
            _fieldInfo = fieldInfo;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string Name { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public abstract TypeBase DeclaringType { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual PropertyInfo PropertyInfo { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual FieldInfo FieldInfo
        {
            [DebuggerStepThrough]
            get => _fieldInfo;
            [DebuggerStepThrough]
            set => SetField(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetField([CanBeNull] string fieldName, ConfigurationSource configurationSource)
        {
            if (fieldName == null)
            {
                SetField((FieldInfo)null, configurationSource);
                return;
            }

            if (FieldInfo?.GetSimpleMemberName() == fieldName)
            {
                SetField(FieldInfo, configurationSource);
                return;
            }

            var fieldInfo = GetFieldInfo(fieldName, DeclaringType, Name, shouldThrow: true);
            if (fieldInfo != null)
            {
                SetField(fieldInfo, configurationSource);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static FieldInfo GetFieldInfo(
            [NotNull] string fieldName, [NotNull] TypeBase type, [CanBeNull] string propertyName, bool shouldThrow)
        {
            Check.DebugAssert(propertyName != null || !shouldThrow, "propertyName is null");

            if (!type.GetRuntimeFields().TryGetValue(fieldName, out var fieldInfo)
                && shouldThrow)
            {
                throw new InvalidOperationException(
                    CoreStrings.MissingBackingField(fieldName, propertyName, type.DisplayName()));
            }

            return fieldInfo;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetField([CanBeNull] FieldInfo fieldInfo, ConfigurationSource configurationSource)
        {
            if (Equals(FieldInfo, fieldInfo))
            {
                UpdateFieldInfoConfigurationSource(configurationSource);
                return;
            }

            if (fieldInfo != null)
            {
                IsCompatible(fieldInfo, ClrType, DeclaringType.ClrType, Name, shouldThrow: true);

                if (PropertyInfo != null
                    && PropertyInfo.IsIndexerProperty())
                {
                    throw new InvalidOperationException(
                        CoreStrings.BackingFieldOnIndexer(fieldInfo.GetSimpleMemberName(), DeclaringType.DisplayName(), Name));
                }
            }

            if (PropertyInfo == null
                && fieldInfo?.GetSimpleMemberName() != Name)
            {
                throw new InvalidOperationException(
                    CoreStrings.FieldNameMismatch(fieldInfo?.GetSimpleMemberName(), DeclaringType.DisplayName(), Name));
            }

            UpdateFieldInfoConfigurationSource(configurationSource);

            var oldFieldInfo = FieldInfo;
            _fieldInfo = fieldInfo;

            OnFieldInfoSet(fieldInfo, oldFieldInfo);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, ConfigurationSource configurationSource)
            => this.SetOrRemoveAnnotation(CoreAnnotationNames.PropertyAccessMode, propertyAccessMode, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsCompatible(
            [NotNull] FieldInfo fieldInfo,
            [CanBeNull] Type propertyType,
            [CanBeNull] Type entityClrType,
            [CanBeNull] string propertyName,
            bool shouldThrow)
        {
            Check.DebugAssert(propertyName != null || !shouldThrow, "propertyName is null");

            if (entityClrType == null
                || !fieldInfo.DeclaringType.IsAssignableFrom(entityClrType))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        CoreStrings.MissingBackingField(fieldInfo.Name, propertyName, entityClrType?.ShortDisplayName()));
                }

                return false;
            }

            var fieldType = fieldInfo.FieldType;
            if (propertyType != null
                && !propertyType.IsCompatibleWith(fieldType))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        CoreStrings.BadBackingFieldType(
                            fieldInfo.Name,
                            fieldInfo.FieldType.ShortDisplayName(),
                            entityClrType.ShortDisplayName(),
                            propertyName,
                            propertyType.ShortDisplayName()));
                }

                return false;
            }

            return true;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual PropertyIndexes PropertyIndexes
        {
            get => NonCapturingLazyInitializer.EnsureInitialized(
                ref _indexes, this,
                property =>
                {
                    var _ = (property.DeclaringType as EntityType)?.Counts;
                });

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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void OnFieldInfoSet([CanBeNull] FieldInfo newFieldInfo, [CanBeNull] FieldInfo oldFieldInfo)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetFieldInfoConfigurationSource() => _fieldInfoConfigurationSource;

        private void UpdateFieldInfoConfigurationSource(ConfigurationSource configurationSource)
            => _fieldInfoConfigurationSource = configurationSource.Max(_fieldInfoConfigurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public abstract Type ClrType { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IClrPropertyGetter Getter =>
            NonCapturingLazyInitializer.EnsureInitialized(
                ref _getter, this, p => new ClrPropertyGetterFactory().Create(p));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IClrPropertySetter Setter =>
            NonCapturingLazyInitializer.EnsureInitialized(
                ref _setter, this, p => new ClrPropertySetterFactory().Create(p));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IClrPropertySetter MaterializationSetter =>
            NonCapturingLazyInitializer.EnsureInitialized(
                ref _materializationSetter, this, p => new ClrPropertyMaterializationSetterFactory().Create(p));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual PropertyAccessors Accessors
            => NonCapturingLazyInitializer.EnsureInitialized(ref _accessors, this, p => new PropertyAccessorsFactory().Create(p));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IComparer<IUpdateEntry> CurrentValueComparer =>
            NonCapturingLazyInitializer.EnsureInitialized(
                ref _currentValueComparer, this, p => new CurrentValueComparerFactory().Create(p));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        ITypeBase IPropertyBase.DeclaringType => DeclaringType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableTypeBase IMutablePropertyBase.DeclaringType => DeclaringType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionTypeBase IConventionPropertyBase.DeclaringType => DeclaringType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IConventionPropertyBase.SetField(FieldInfo fieldInfo, bool fromDataAnnotation)
            => SetField(fieldInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }
}
