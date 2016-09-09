// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ComplexPropertyDefinition : PropertyBase, IMutableComplexPropertyDefinition
    {
        private ConfigurationSource _configurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexPropertyDefinition(
            [NotNull] string name,
            [NotNull] Type clrType,
            [NotNull] ComplexTypeDefinition declaringType,
            ConfigurationSource configurationSource)
            : base(name, null)
        {
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(declaringType, nameof(declaringType));

            DeclaringType = declaringType;
            ClrType = clrType;
            Facets = new ComplexPropertyDefinitionPropertyFacets(this);
            Initialize(declaringType, configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexPropertyDefinition(
            [NotNull] PropertyInfo propertyInfo,
            [NotNull] ComplexTypeDefinition declaringType,
            ConfigurationSource configurationSource)
            : base(Check.NotNull(propertyInfo, nameof(propertyInfo)).Name, propertyInfo)
        {
            Check.NotNull(declaringType, nameof(declaringType));

            DeclaringType = declaringType;
            ClrType = propertyInfo.PropertyType;
            Facets = new ComplexPropertyDefinitionPropertyFacets(this);
            Initialize(declaringType, configurationSource);
        }

        private void Initialize(ComplexTypeDefinition declaringType, ConfigurationSource configurationSource)
        {
            _configurationSource = configurationSource;

            // TODO: ComplexType builders
            //Builder = new InternalPropertyBuilder(this, declaringEntityType.Model.Builder);
        }

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
        public virtual PropertyFacets Facets { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsNullable
        {
            get { return Facets.IsNullable; }
            set { Facets.IsNullable = value; }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsReadOnlyBeforeSave
        {
            get { return Facets.IsReadOnlyBeforeSave; }
            set { Facets.IsReadOnlyBeforeSave = value; }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsReadOnlyAfterSave
        {
            get { return Facets.IsReadOnlyAfterSave; }
            set { Facets.IsReadOnlyAfterSave = value; }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsStoreGeneratedAlways
        {
            get { return Facets.IsStoreGeneratedAlways; }
            set { Facets.IsStoreGeneratedAlways = value; }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ValueGenerated ValueGenerated
        {
            get { return Facets.ValueGenerated; }
            set { Facets.ValueGenerated = value; }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool RequiresValueGenerator
        {
            get { return Facets.RequiresValueGenerator; }
            set { Facets.RequiresValueGenerator = value; }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsShadowProperty => MemberInfo == null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsConcurrencyToken
        {
            get { return Facets.IsConcurrencyToken; }
            set { Facets.IsConcurrencyToken = value; }
        }

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
        public override Type ClrType { get; }

        // TODO: ComplexType builders
        //protected override void OnFieldInfoSet(FieldInfo oldFieldInfo)
        //    => DeclaringType.Model.ConventionDispatcher.OnPropertyFieldChanged(Builder, oldFieldInfo);

        ITypeBase IPropertyBase.DeclaringType => DeclaringType;
        IComplexTypeDefinition IComplexPropertyDefinition.DeclaringType => DeclaringType;
        IMutableComplexTypeDefinition IMutableComplexPropertyDefinition.DeclaringType => DeclaringType;
        IMutableTypeBase IMutablePropertyBase.DeclaringType => DeclaringType;

        // TODO: ComplexType debug strings public override string ToString() => this.ToDebugString();

        // TODO: ComplexType debug strings public virtual DebugView<Property> DebugView => new DebugView<Property>(this, m => m.ToDebugString(false));
    }
}
