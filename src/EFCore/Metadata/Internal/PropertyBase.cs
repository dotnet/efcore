// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
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
    public abstract class PropertyBase : ConventionalAnnotatable, IMutablePropertyBase
    {
        private FieldInfo _fieldInfo;
        private ConfigurationSource? _fieldInfoConfigurationSource;
        private bool _isIndexedProperty;

        // Warning: Never access these fields directly as access needs to be thread-safe
        private IClrPropertyGetter _getter;
        private IClrPropertySetter _setter;
        private PropertyAccessors _accessors;
        private PropertyIndexes _indexes;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
            _isIndexedProperty = propertyInfo != null
                && propertyInfo.IsEFIndexerProperty();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Name { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public abstract TypeBase DeclaringType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsShadowProperty
        {
            [DebuggerStepThrough] get => this.GetIdentifyingMemberInfo() == null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsIndexedProperty => _isIndexedProperty;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual PropertyInfo PropertyInfo { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual FieldInfo FieldInfo
        {
            [DebuggerStepThrough] get => _fieldInfo;
            [DebuggerStepThrough]
            [param: CanBeNull]
            set => SetFieldInfo(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetField([CanBeNull] string fieldName, ConfigurationSource configurationSource)
        {
            if (fieldName == null)
            {
                SetFieldInfo(null, configurationSource);
                return;
            }

            if (FieldInfo?.GetSimpleMemberName() == fieldName)
            {
                SetFieldInfo(FieldInfo, configurationSource);
                return;
            }

            var fieldInfo = GetFieldInfo(fieldName, DeclaringType, Name, shouldThrow: true);
            if (fieldInfo != null)
            {
                SetFieldInfo(fieldInfo, configurationSource);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static FieldInfo GetFieldInfo(
            [NotNull] string fieldName, [NotNull] TypeBase type, [CanBeNull] string propertyName, bool shouldThrow)
        {
            Debug.Assert(propertyName != null || !shouldThrow);

            if (!type.GetRuntimeFields().TryGetValue(fieldName, out var fieldInfo)
                && shouldThrow)
            {
                throw new InvalidOperationException(
                    CoreStrings.MissingBackingField(fieldName, propertyName, type.DisplayName()));
            }

            return fieldInfo;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetFieldInfo([CanBeNull] FieldInfo fieldInfo, ConfigurationSource configurationSource)
        {
            if (Equals(FieldInfo, fieldInfo))
            {
                UpdateFieldInfoConfigurationSource(configurationSource);
                return;
            }

            if (fieldInfo != null)
            {
                IsCompatible(fieldInfo, ClrType, DeclaringType.ClrType, Name, shouldThrow: true);
            }

            UpdateFieldInfoConfigurationSource(configurationSource);

            var oldFieldInfo = FieldInfo;
            _fieldInfo = fieldInfo;

            PropertyMetadataChanged();

            OnFieldInfoSet(oldFieldInfo);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsCompatible(
            [NotNull] FieldInfo fieldInfo,
            [NotNull] Type propertyType,
            [NotNull] Type entityClrType,
            [CanBeNull] string propertyName,
            bool shouldThrow)
        {
            Debug.Assert(propertyName != null || !shouldThrow);

            var fieldTypeInfo = fieldInfo.FieldType.GetTypeInfo();
            if (!fieldTypeInfo.IsAssignableFrom(propertyType.GetTypeInfo())
                && !propertyType.GetTypeInfo().IsAssignableFrom(fieldTypeInfo))
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

            if (!fieldInfo.DeclaringType.GetTypeInfo().IsAssignableFrom(entityClrType.GetTypeInfo()))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        CoreStrings.MissingBackingField(fieldInfo.Name, propertyName, entityClrType.ShortDisplayName()));
                }

                return false;
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected abstract void PropertyMetadataChanged();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void OnFieldInfoSet([CanBeNull] FieldInfo oldFieldInfo)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetFieldInfoConfigurationSource() => _fieldInfoConfigurationSource;

        private void UpdateFieldInfoConfigurationSource(ConfigurationSource configurationSource)
            => _fieldInfoConfigurationSource = configurationSource.Max(_fieldInfoConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public abstract Type ClrType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IClrPropertyGetter Getter => _isIndexedProperty
            ? NonCapturingLazyInitializer.EnsureInitialized(ref _getter, this, p => new IndexedPropertyGetterFactory().Create(p))
            : NonCapturingLazyInitializer.EnsureInitialized(ref _getter, this, p => new ClrPropertyGetterFactory().Create(p));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IClrPropertySetter Setter => _isIndexedProperty
            ? NonCapturingLazyInitializer.EnsureInitialized(ref _setter, this, p => new IndexedPropertySetterFactory().Create(p))
            : NonCapturingLazyInitializer.EnsureInitialized(ref _setter, this, p => new ClrPropertySetterFactory().Create(p));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual PropertyAccessors Accessors
            => NonCapturingLazyInitializer.EnsureInitialized(ref _accessors, this, p => new PropertyAccessorsFactory().Create(p));

        ITypeBase IPropertyBase.DeclaringType => DeclaringType;
        IMutableTypeBase IMutablePropertyBase.DeclaringType => DeclaringType;
    }
}
