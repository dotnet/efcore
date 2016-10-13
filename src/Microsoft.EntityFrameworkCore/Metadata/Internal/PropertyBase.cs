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
    public abstract class PropertyBase : ConventionalAnnotatable, IMutablePropertyBase
    {
        private FieldInfo _fieldInfo;
        private ConfigurationSource? _fieldInfoConfigurationSource;

        // Warning: Never access these fields directly as access needs to be thread-safe
        private IClrPropertyGetter _getter;
        private IClrPropertySetter _setter;
        private PropertyAccessors _accessors;

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
        public virtual IMutableTypeBase DeclaringType => ((IMutablePropertyBase)this).DeclaringType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsShadowProperty => MemberInfo == null;

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

            var typesInHierarchy = DeclaringType.ClrType.GetTypesInHierarchy().ToList();

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
                CoreStrings.MissingBackingField(fieldName, Name, DeclaringType.DisplayName()));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetFieldInfo(
            [CanBeNull] FieldInfo fieldInfo, ConfigurationSource configurationSource, bool runConventions = true)
        {
            if (fieldInfo != null
                && !fieldInfo.FieldType.GetTypeInfo().IsAssignableFrom(ClrType.GetTypeInfo()))
            {
                throw new InvalidOperationException(
                    CoreStrings.BadBackingFieldType(
                        fieldInfo.Name,
                        fieldInfo.FieldType.ShortDisplayName(),
                        DeclaringType.DisplayName(),
                        Name,
                        ClrType.ShortDisplayName()));
            }

            UpdateFieldInfoConfigurationSource(configurationSource);

            if (!ReferenceEquals(FieldInfo, fieldInfo))
            {
                var oldFieldInfo = FieldInfo;
                _fieldInfo = fieldInfo;

                PropertyMetadataChanged();

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

        ITypeBase IPropertyBase.DeclaringType => DeclaringType;

        [Obsolete("Use DeclaringType, IProperty.DeclaringEntityType, or INavigation.DeclaringEntityType.")]
        IEntityType IPropertyBase.DeclaringEntityType => (IEntityType)DeclaringType;
    }
}
