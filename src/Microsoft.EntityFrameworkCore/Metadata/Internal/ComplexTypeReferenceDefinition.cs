// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
    public class ComplexTypeReferenceDefinition : PropertyBase, IMutableComplexTypeReferenceDefinition
    {
        private ConfigurationSource _configurationSource;
        private bool _required = true;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexTypeReferenceDefinition(
            [NotNull] string name,
            [CanBeNull] PropertyInfo propertyInfo,
            [CanBeNull] FieldInfo fieldInfo,
            [NotNull] ComplexTypeDefinition referencedType,
            [NotNull] ComplexTypeDefinition declaringType,
            ConfigurationSource configurationSource)
            : base(name, propertyInfo, fieldInfo)
        {
            Check.NotNull(referencedType, nameof(referencedType));
            Check.NotNull(declaringType, nameof(declaringType));

            DeclaringType = declaringType;
            ReferencedComplexTypeDefinition = referencedType;
            _configurationSource = configurationSource;

            // TODO: ComplexType builders
            //Builder = new InternalPropertyBuilder(this, declaringEntityType.Model.Builder);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeDefinition ReferencedComplexTypeDefinition { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual ComplexTypeDefinition DeclaringType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void PropertyMetadataChanged() => DeclaringType.PropertyMetadataChanged();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsRequired
        {
            get { return _required; }
            set { SetIsRequired(value, ConfigurationSource.Explicit); }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetIsRequired(bool required, ConfigurationSource configurationSource)
        {
            if (!required
                && !ClrType.GetTypeInfo().IsClass)
            {
                throw new InvalidOperationException(
                    CoreStrings.ComplexTypeStructIsRequired(Name, ReferencedComplexTypeDefinition.DisplayName(), DeclaringType.DisplayName()));
            }

            _required = required;
            IsRequiredConfigurationSource = configurationSource.Max(IsRequiredConfigurationSource);
            PropertyMetadataChanged();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? IsRequiredConfigurationSource { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource GetConfigurationSource() => _configurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
            => _configurationSource = _configurationSource.Max(configurationSource);

        // Needed for a workaround before reference counting is implemented
        // Issue #214
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetConfigurationSource(ConfigurationSource configurationSource)
            => _configurationSource = configurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Type ClrType => ReferencedComplexTypeDefinition.ClrType;

        // TODO: ComplexType builders
        //protected override void OnFieldInfoSet(FieldInfo oldFieldInfo)
        //    => DeclaringType.Model.ConventionDispatcher.OnPropertyFieldChanged(Builder, oldFieldInfo);

        ITypeBase IPropertyBase.DeclaringType => DeclaringType;
        IMutableTypeBase IMutablePropertyBase.DeclaringType => DeclaringType;

        IComplexTypeDefinition IComplexTypeReferenceDefinition.ReferencedComplexTypeDefinition => ReferencedComplexTypeDefinition;
        IMutableComplexTypeDefinition IMutableComplexTypeReferenceDefinition.ReferencedComplexTypeDefinition => ReferencedComplexTypeDefinition;

        // TODO: ComplexType debug strings public override string ToString() => this.ToDebugString();

        // TODO: ComplexType debug strings public virtual DebugView<Property> DebugView => new DebugView<Property>(this, m => m.ToDebugString(false));
    }
}
