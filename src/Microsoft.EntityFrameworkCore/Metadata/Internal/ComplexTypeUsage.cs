// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ComplexTypeUsage : PropertyBase, IMutableComplexTypeUsage
    {
        private ConfigurationSource _configurationSource;

        private readonly SortedDictionary<string, IMutableComplexProperty> _properties
            = new SortedDictionary<string, IMutableComplexProperty>(StringComparer.Ordinal);

        private readonly SortedDictionary<string, ComplexTypeUsage> _complexTypeUsages
            = new SortedDictionary<string, ComplexTypeUsage>(StringComparer.Ordinal);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexTypeUsage(
            [NotNull] string name,
            [NotNull] EntityType declaringEntityType,
            [NotNull] ComplexTypeDefinition complexTypeDefinition,
            ConfigurationSource configurationSource)
            : base(name, null)
        {
            DeclaringEntityType = declaringEntityType;
            DeclaringType = declaringEntityType;
            Definition = complexTypeDefinition;
            _configurationSource = configurationSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexTypeUsage(
            [NotNull] PropertyInfo propertyInfo,
            [NotNull] EntityType declaringEntityType,
            [NotNull] ComplexTypeDefinition complexTypeDefinition,
            ConfigurationSource configurationSource)
            : base(propertyInfo.Name, propertyInfo)
        {
            DeclaringEntityType = declaringEntityType;
            DeclaringType = declaringEntityType;
            Definition = complexTypeDefinition;
            _configurationSource = configurationSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexTypeUsage(
            [NotNull] string name,
            [NotNull] ComplexTypeUsage declaringComplexType,
            [NotNull] ComplexTypeDefinition complexTypeDefinition,
            ConfigurationSource configurationSource)
            : base(name, null)
        {
            DeclaringEntityType = declaringComplexType.DeclaringEntityType;
            DeclaringType = declaringComplexType;
            Definition = complexTypeDefinition;
            _configurationSource = configurationSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexTypeUsage(
            [NotNull] PropertyInfo propertyInfo,
            [NotNull] ComplexTypeUsage declaringComplexType,
            [NotNull] ComplexTypeDefinition complexTypeDefinition,
            ConfigurationSource configurationSource)
            : base(propertyInfo.Name, propertyInfo)
        {
            DeclaringEntityType = declaringComplexType.DeclaringEntityType;
            DeclaringType = declaringComplexType;
            Definition = complexTypeDefinition;
            _configurationSource = configurationSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType DeclaringEntityType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IMutableTypeBase DeclaringType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Model Model => DeclaringEntityType.Model;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeDefinition Definition { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void PropertyMetadataChanged()
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Type ClrType => Definition.ClrType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsRequired { get; set; } // TODO: ComplexType 

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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IMutableComplexProperty FindProperty(string name)
        {
            Check.NotEmpty(name, nameof(name));

            IMutableComplexProperty property;
            return _properties.TryGetValue(name, out property)
                ? property
                : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<IMutableComplexProperty> GetProperties() => _properties.Values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeUsage FindComplexTypeUsage(string name)
        {
            Check.NotEmpty(name, nameof(name));

            ComplexTypeUsage property;
            return _complexTypeUsages.TryGetValue(name, out property)
                ? property
                : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ComplexTypeUsage> GetComplexTypeUsages() => _complexTypeUsages.Values;

        IComplexTypeUsage IComplexTypeUsage.FindComplexTypeUsage(string name) => FindComplexTypeUsage(name);
        IMutableComplexTypeUsage IMutableComplexTypeUsage.FindComplexTypeUsage(string name) => FindComplexTypeUsage(name);

        IEnumerable<IComplexTypeUsage> IComplexTypeUsage.GetComplexTypeUsages() => GetComplexTypeUsages();
        IEnumerable<IMutableComplexTypeUsage> IMutableComplexTypeUsage.GetComplexTypeUsages() => GetComplexTypeUsages();

        IComplexProperty IComplexTypeUsage.FindProperty(string name) => FindProperty(name);
        IMutableComplexProperty IMutableComplexTypeUsage.FindProperty(string name) => FindProperty(name);

        IEnumerable<IComplexProperty> IComplexTypeUsage.GetProperties() => GetProperties();
        IEnumerable<IMutableComplexProperty> IMutableComplexTypeUsage.GetProperties() => GetProperties();

        IEntityType IComplexTypeUsage.DeclaringEntityType => DeclaringEntityType;
        IMutableEntityType IMutableComplexTypeUsage.DeclaringEntityType => DeclaringEntityType;

        IModel ITypeBase.Model => Model;
        IMutableModel IMutableTypeBase.Model => Model;
    }
}
