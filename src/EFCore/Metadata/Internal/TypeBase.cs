// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public abstract class TypeBase : ConventionalAnnotatable, IMutableTypeBase
    {
        private ConfigurationSource _configurationSource;
        private readonly Dictionary<string, ConfigurationSource> _ignoredMembers
            = new Dictionary<string, ConfigurationSource>(StringComparer.Ordinal);

        private Dictionary<string, PropertyInfo> _runtimeProperties;
        private Dictionary<string, FieldInfo> _runtimeFields;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected TypeBase([NotNull] string name, [NotNull] Model model, ConfigurationSource configurationSource)
            : this(model, configurationSource)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(model, nameof(model));

            Name = name;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected TypeBase([NotNull] Type clrType, [NotNull] Model model, ConfigurationSource configurationSource)
            : this(model, configurationSource)
        {
            Check.NotNull(model, nameof(model));

            Name = model.GetDisplayName(clrType);
            ClrType = clrType;
        }

        private TypeBase([NotNull] Model model, ConfigurationSource configurationSource)
        {
            Model = model;
            _configurationSource = configurationSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Type ClrType { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Model Model { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Name { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DebuggerStepThrough]
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
        public virtual Dictionary<string, PropertyInfo> GetRuntimeProperties()
        {
            if (ClrType == null)
            {
                return null;
            }

            if (_runtimeProperties == null)
            {
                _runtimeProperties = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);
                foreach (var property in ClrType.GetRuntimeProperties())
                {
                    if (!property.IsStatic()
                        && !_runtimeProperties.ContainsKey(property.Name))
                    {
                        _runtimeProperties[property.Name] = property;
                    }
                }
            }

            return _runtimeProperties;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Dictionary<string, FieldInfo> GetRuntimeFields()
        {
            if (ClrType == null)
            {
                return null;
            }

            if (_runtimeFields == null)
            {
                _runtimeFields = new Dictionary<string, FieldInfo>(StringComparer.Ordinal);
                foreach (var field in ClrType.GetRuntimeFields())
                {
                    if (!field.IsStatic
                        && !_runtimeFields.ContainsKey(field.Name))
                    {
                        _runtimeFields[field.Name] = field;
                    }
                }
            }

            return _runtimeFields;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void ClearCaches()
        {
            _runtimeProperties = null;
            _runtimeFields = null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Ignore([NotNull] string name, ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            Check.NotNull(name, nameof(name));

            if (_ignoredMembers.TryGetValue(name, out var existingIgnoredConfigurationSource))
            {
                _ignoredMembers[name] = configurationSource.Max(existingIgnoredConfigurationSource);
                return;
            }

            _ignoredMembers[name] = configurationSource;

            OnTypeMemberIgnored(name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public abstract void OnTypeMemberIgnored([NotNull] string name);

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
            => _ignoredMembers.TryGetValue(Check.NotEmpty(name, nameof(name)), out var ignoredConfigurationSource)
                ? (ConfigurationSource?)ignoredConfigurationSource
                : null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? FindIgnoredMemberConfigurationSource([NotNull] string name)
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

        IModel ITypeBase.Model
        {
            [DebuggerStepThrough] get => Model;
        }

        IMutableModel IMutableTypeBase.Model
        {
            [DebuggerStepThrough] get => Model;
        }

        Type ITypeBase.ClrType
        {
            [DebuggerStepThrough] get => ClrType;
        }
    }
}
