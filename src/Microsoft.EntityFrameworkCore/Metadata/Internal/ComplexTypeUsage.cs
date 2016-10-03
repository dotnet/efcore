// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
    public class ComplexTypeUsage : PropertyBase, IMutableComplexTypeUsage
    {
        private ConfigurationSource _configurationSource;
        private readonly ComplexTypeReferenceDefinition _referenceDefinition;
        private bool? _required;

        private readonly SortedDictionary<string, ComplexProperty> _properties
            = new SortedDictionary<string, ComplexProperty>(StringComparer.Ordinal);

        private readonly SortedDictionary<string, ComplexTypeUsage> _complexTypeUsages
            = new SortedDictionary<string, ComplexTypeUsage>(StringComparer.Ordinal);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexTypeUsage(
            [NotNull] string name,
            [CanBeNull] PropertyInfo propertyInfo,
            [CanBeNull] FieldInfo fieldInfo,
            [NotNull] EntityType declaringEntityType,
            [NotNull] ComplexTypeDefinition complexTypeDefinition,
            ConfigurationSource configurationSource)
            : base(name, propertyInfo, fieldInfo)
        {
            DeclaringType = declaringEntityType;
            DeclaringEntityType = declaringEntityType;
            Definition = complexTypeDefinition;
            _configurationSource = configurationSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexTypeUsage(
            [NotNull] ComplexTypeUsage parentUsage,
            [NotNull] ComplexTypeReferenceDefinition referenceDefinition,
            ConfigurationSource configurationSource)
            : base(referenceDefinition.Name, referenceDefinition.PropertyInfo, referenceDefinition.FieldInfo)
        {
            DeclaringType = parentUsage;
            DeclaringEntityType = parentUsage.DeclaringEntityType;
            Definition = referenceDefinition.ReferencedComplexTypeDefinition;
            _configurationSource = configurationSource;
            _referenceDefinition = referenceDefinition;
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
        public new virtual IMutableTypeBase DeclaringType { get; }

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
        public virtual bool IsRequired
        {
            get { return _required ?? _referenceDefinition?.IsRequired ?? true; }
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
                    CoreStrings.ComplexTypeStructIsRequired(
                        Name, Definition.DisplayName(), DeclaringType.DisplayName()));
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexProperty FindProperty([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            ComplexProperty property;
            return _properties.TryGetValue(name, out property)
                ? property
                : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ComplexProperty> GetProperties() => _properties.Values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexProperty AddProperty(
            [NotNull] ComplexPropertyDefinition propertyDefinition,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            Check.NotNull(propertyDefinition, nameof(propertyDefinition));

            if (propertyDefinition.DeclaringType != Definition)
            {
                throw new InvalidOperationException(CoreStrings.ComplexPropertyWrongType(
                    propertyDefinition.Name, this.DisplayName(), propertyDefinition.DeclaringType.DisplayName()));
            }

            var duplicateProperty = FindProperty(propertyDefinition.Name);
            if (duplicateProperty != null)
            {
                throw new InvalidOperationException(CoreStrings.DuplicateProperty(
                    propertyDefinition.Name, this.DisplayName(), duplicateProperty.DeclaringType.DisplayName()));
            }

            var duplicateUsage = FindComplexTypeUsage(propertyDefinition.Name);
            if (duplicateUsage != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.ConflictingPropertyToReference(propertyDefinition.Name, this.DisplayName(),
                        duplicateUsage.DeclaringType.DisplayName()));
            }

            var property = new ComplexProperty(this, propertyDefinition, configurationSource);

            _properties.Add(property.Name, property);

            PropertyMetadataChanged();

            if (runConventions)
            {
                property = (ComplexProperty)Model.ConventionDispatcher.OnPropertyAdded(property.Builder)?.Metadata;
            }

            return property;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexProperty RemoveProperty([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            var property = FindProperty(name);
            return property == null
                ? null
                : RemoveProperty(property);
        }

        private ComplexProperty RemoveProperty(ComplexProperty property)
        {
            // TODO: ComplexType Check if property usage is in use in any entity.

            _properties.Remove(property.Name);

            property.Builder = null;

            PropertyMetadataChanged();

            return property;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeUsage FindComplexTypeUsage([NotNull] string name)
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeUsage AddComplexTypeUsage(
            [NotNull] ComplexTypeReferenceDefinition referenceDefinition,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            Check.NotNull(referenceDefinition, nameof(referenceDefinition));

            if (referenceDefinition.DeclaringType != Definition)
            {
                throw new InvalidOperationException(CoreStrings.ComplexReferenceWrongType(
                    referenceDefinition.Name, referenceDefinition.ReferencedComplexTypeDefinition.DisplayName(), 
                    this.DisplayName(), referenceDefinition.DeclaringType.DisplayName()));
            }

            var duplicateReference = FindComplexTypeUsage(referenceDefinition.Name);
            if (duplicateReference != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.DuplicateComplexReference(referenceDefinition.Name, this.DisplayName(),
                        duplicateReference.DeclaringType.DisplayName()));
            }

            var duplicateProperty = FindProperty(referenceDefinition.Name);
            if (duplicateProperty != null)
            {
                throw new InvalidOperationException(CoreStrings.ConflictingPropertyToReference(
                    referenceDefinition.Name, this.DisplayName(), duplicateProperty.DeclaringType.DisplayName()));
            }
            var usage = new ComplexTypeUsage(this, referenceDefinition, configurationSource);

            _complexTypeUsages.Add(usage.Name, usage);

            PropertyMetadataChanged();

            if (runConventions)
            {
                // TODO: ComplexType builders
                //usage = Model.ConventionDispatcher.OnPropertyAdded(property.Builder)?.Metadata;
            }

            return usage;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeUsage RemoveComplexTypeUsage([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            var usage = FindComplexTypeUsage(name);
            return usage == null
                ? null
                : RemoveComplexTypeUsage(usage);
        }

        private ComplexTypeUsage RemoveComplexTypeUsage(ComplexTypeUsage usage)
        {
            // TODO: ComplexType Check if property usage is in use in any entity.

            _complexTypeUsages.Remove(usage.Name);

            // TODO: ComplexType builders
            //usage.Builder = null;

            PropertyMetadataChanged();

            return usage;
        }

        IComplexTypeDefinition IComplexTypeUsage.Definition => Definition;
        IMutableComplexTypeDefinition IMutableComplexTypeUsage.Definition => Definition;

        IComplexTypeUsage IComplexTypeUsage.FindComplexTypeUsage(string name) => FindComplexTypeUsage(name);
        IMutableComplexTypeUsage IMutableComplexTypeUsage.FindComplexTypeUsage(string name) => FindComplexTypeUsage(name);

        IEnumerable<IComplexTypeUsage> IComplexTypeUsage.GetComplexTypeUsages() => GetComplexTypeUsages();
        IEnumerable<IMutableComplexTypeUsage> IMutableComplexTypeUsage.GetComplexTypeUsages() => GetComplexTypeUsages();

        IMutableComplexTypeUsage IMutableComplexTypeUsage.AddComplexTypeUsage(IMutableComplexTypeReferenceDefinition referenceDefinition) => AddComplexTypeUsage((ComplexTypeReferenceDefinition)referenceDefinition);
        IMutableComplexTypeUsage IMutableComplexTypeUsage.RemoveComplexTypeUsage(string name) => RemoveComplexTypeUsage(name);

        IComplexProperty IComplexTypeUsage.FindProperty(string name) => FindProperty(name);
        IMutableComplexProperty IMutableComplexTypeUsage.FindProperty(string name) => FindProperty(name);

        IEnumerable<IComplexProperty> IComplexTypeUsage.GetProperties() => GetProperties();
        IEnumerable<IMutableComplexProperty> IMutableComplexTypeUsage.GetProperties() => GetProperties();

        IMutableComplexProperty IMutableComplexTypeUsage.AddProperty(IMutableComplexPropertyDefinition propertyDefinition) => AddProperty((ComplexPropertyDefinition)propertyDefinition);
        IMutableComplexProperty IMutableComplexTypeUsage.RemoveProperty(string name) => RemoveProperty(name);

        IEntityType IComplexTypeUsage.DeclaringEntityType => DeclaringEntityType;
        IMutableEntityType IMutableComplexTypeUsage.DeclaringEntityType => DeclaringEntityType;

        IModel ITypeBase.Model => Model;
        IMutableModel IMutableTypeBase.Model => Model;
    }
}
