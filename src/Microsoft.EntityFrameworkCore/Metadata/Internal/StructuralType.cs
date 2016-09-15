// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public abstract class StructuralType : ConventionalAnnotatable, IMutableStructuralType
    {
        private readonly object _typeOrName;

        private ConfigurationSource _configurationSource;
        private readonly Dictionary<string, ConfigurationSource> _ignoredMembers = new Dictionary<string, ConfigurationSource>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected StructuralType([NotNull] string name, [NotNull] Model model, ConfigurationSource configurationSource)
            : this(model, configurationSource)
        {
            Check.NotEmpty(name, nameof(name));

            _typeOrName = name;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected StructuralType([NotNull] Type clrType, [NotNull] Model model, ConfigurationSource configurationSource)
            : this(model, configurationSource)
        {
            Check.ValidEntityType(clrType, nameof(clrType));

            _typeOrName = clrType;
        }

        private StructuralType(Model model, ConfigurationSource configurationSource)
        {
            Check.NotNull(model, nameof(model));

            Model = model;
            _configurationSource = configurationSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Type ClrType => _typeOrName as Type;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Model Model { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Name
            => ClrType != null
                ? ClrType.DisplayName()
                : (string)_typeOrName;

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
        public abstract void PropertyMetadataChanged();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual StructuralProperty AddProperty(string name, Type propertyType, bool shadow)
            => (StructuralProperty)((IMutableStructuralType)this).AddProperty(name, propertyType, shadow);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual StructuralProperty FindProperty(string name)
            => (StructuralProperty)((IMutableStructuralType)this).FindProperty(name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<StructuralProperty> GetProperties()
            => (IEnumerable<StructuralProperty>)((IMutableStructuralType)this).GetProperties();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual StructuralProperty RemoveProperty(string name) 
            => (StructuralProperty)((IMutableStructuralType)this).RemoveProperty(name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Ignore([NotNull] string name, ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            Check.NotNull(name, nameof(name));

            ConfigurationSource existingIgnoredConfigurationSource;
            if (_ignoredMembers.TryGetValue(name, out existingIgnoredConfigurationSource))
            {
                configurationSource = configurationSource.Max(existingIgnoredConfigurationSource);
            }

            _ignoredMembers[name] = configurationSource;

            if (runConventions)
            {
                OnEntityTypeMemberIgnored(name);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected abstract void OnEntityTypeMemberIgnored(string name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<string> GetIgnoredMembers()
            => _ignoredMembers.Keys.ToList();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? FindDeclaredIgnoredMemberConfigurationSource([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            ConfigurationSource ignoredConfigurationSource;
            if (_ignoredMembers.TryGetValue(name, out ignoredConfigurationSource))
            {
                return ignoredConfigurationSource;
            }

            return null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? FindIgnoredMemberConfigurationSource(string name)
            => FindDeclaredIgnoredMemberConfigurationSource(name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Unignore([NotNull] string name)
        {
            Check.NotNull(name, nameof(name));
            _ignoredMembers.Remove(name);
        }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        public virtual IMutableComplexTypeReference AddComplexTypeReference([NotNull] string name, [NotNull] IMutableComplexType complexType, bool shadow)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        public virtual IMutableComplexTypeReference FindComplexTypeReference([NotNull] string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        public virtual IEnumerable<IMutableComplexTypeReference> GetComplexTypeReferences()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        public virtual IMutableComplexTypeReference RemoveComplexTypeReference([NotNull] string name)
        {
            throw new NotImplementedException();
        }

        IModel IStructuralType.Model => Model;
        IMutableModel IMutableStructuralType.Model => Model;
        Type IStructuralType.ClrType => ClrType;

        IMutableStructuralProperty IMutableStructuralType.AddProperty(string name, Type propertyType, bool shadow)
            => AddProperty(name, propertyType, shadow);

        IStructuralProperty IStructuralType.FindProperty(string name) => FindProperty(name);
        IMutableStructuralProperty IMutableStructuralType.FindProperty(string name) => FindProperty(name);

        IEnumerable<IStructuralProperty> IStructuralType.GetProperties() => GetProperties();
        IEnumerable<IMutableStructuralProperty> IMutableStructuralType.GetProperties() => GetProperties();

        IMutableStructuralProperty IMutableStructuralType.RemoveProperty(string name) => RemoveProperty(name);

        IComplexTypeReference IStructuralType.FindComplexTypeReference(string name) => FindComplexTypeReference(name);
        IEnumerable<IComplexTypeReference> IStructuralType.GetComplexTypeReferences() => GetComplexTypeReferences();
    }
}
