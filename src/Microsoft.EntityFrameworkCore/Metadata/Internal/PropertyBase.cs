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
    public abstract class PropertyBase : ConventionalAnnotatable, IPropertyBase
    {
        private FieldInfo _fieldInfo;
        private ConfigurationSource? _fieldInfoConfigurationSource;

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
            [CanBeNull] PropertyInfo propertyInfo)
        {
            Check.NotEmpty(name, nameof(name));

            Name = name;
            PropertyInfo = propertyInfo;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Name { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public abstract EntityType DeclaringEntityType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual PropertyInfo PropertyInfo { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual FieldInfo FieldInfo
        {
            get { return _fieldInfo; }
            [param: CanBeNull] set { SetFieldInfo(value, ConfigurationSource.Explicit); }
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

            var typesInHierarchy = DeclaringEntityType.ClrType.GetTypesInHierarchy().ToList();

            foreach (var type in typesInHierarchy)
            {
                var fields = type.GetRuntimeFields().ToDictionary(f => f.Name);
                FieldInfo fieldInfo;
                if (fields.TryGetValue(fieldName, out fieldInfo))
                {
                    SetFieldInfo(fieldInfo, configurationSource);
                    return;
                }
            }

            throw new InvalidOperationException(
                CoreStrings.MissingBackingField(fieldName, Name, DeclaringEntityType.DisplayName()));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetFieldInfo(
            [CanBeNull] FieldInfo fieldInfo, ConfigurationSource configurationSource, bool runConventions = true)
        {
            if (fieldInfo != null
                && !fieldInfo.FieldType.GetTypeInfo().IsAssignableFrom(this.GetClrType().GetTypeInfo()))
            {
                throw new InvalidOperationException(
                    CoreStrings.BadBackingFieldType(
                        fieldInfo.Name,
                        fieldInfo.FieldType.ShortDisplayName(),
                        DeclaringEntityType.DisplayName(),
                        Name,
                        this.GetClrType().ShortDisplayName()));
            }

            UpdateFieldInfoConfigurationSource(configurationSource);

            if (!ReferenceEquals(FieldInfo, fieldInfo))
            {
                var oldFieldInfo = FieldInfo;
                _fieldInfo = fieldInfo;

                DeclaringEntityType.PropertyMetadataChanged();

                if (runConventions)
                {
                    OnFieldInfoSet(oldFieldInfo);
                }
            }
        }

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
        public virtual MemberInfo MemberInfo => (MemberInfo)PropertyInfo ?? FieldInfo;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public abstract Type ClrType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IClrPropertyGetter Getter
            => NonCapturingLazyInitializer.EnsureInitialized(ref _getter, this, p => new ClrPropertyGetterFactory().Create(p));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IClrPropertySetter Setter
            => NonCapturingLazyInitializer.EnsureInitialized(ref _setter, this, p => new ClrPropertySetterFactory().Create(p));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual PropertyAccessors Accessors
            => NonCapturingLazyInitializer.EnsureInitialized(ref _accessors, this, p => new PropertyAccessorsFactory().Create(p));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual PropertyIndexes PropertyIndexes
        {
            get
            {
                return NonCapturingLazyInitializer.EnsureInitialized(ref _indexes, this,
                    property => property.DeclaringEntityType.CalculateIndexes(property));
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

        IEntityType IPropertyBase.DeclaringEntityType => DeclaringEntityType;
    }
}
