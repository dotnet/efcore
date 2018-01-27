// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public abstract class TypeBase : ConventionalAnnotatable, IMutableTypeBase
    {
        private readonly object _typeOrName;
        private ConfigurationSource _configurationSource;
        private readonly Dictionary<string, ConfigurationSource> _ignoredMembers = new Dictionary<string, ConfigurationSource>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected TypeBase([NotNull] string name, [NotNull] Model model, ConfigurationSource configurationSource)
            : this(model, configurationSource)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(model, nameof(model));

            _typeOrName = name;
#if DEBUG
            DebugName = name;
#endif
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected TypeBase([NotNull] Type clrType, [NotNull] Model model, ConfigurationSource configurationSource)
            : this(model, configurationSource)
        {
            Check.NotNull(model, nameof(model));

            _typeOrName = clrType;
#if DEBUG
            DebugName = clrType.DisplayName();
#endif
        }

        private TypeBase([NotNull] Model model, ConfigurationSource configurationSource)
        {
            Model = model;
            _configurationSource = configurationSource;
        }

#if DEBUG
        // For breakpoint conditions
        private string DebugName { [UsedImplicitly] get; }
#endif

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Type ClrType
        {
            [DebuggerStepThrough] get => _typeOrName as Type;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Model Model { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Name
        {
            [DebuggerStepThrough] get => ClrType != null ? ClrType.DisplayName() : (string)_typeOrName;
        }

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
        {
            Check.NotEmpty(name, nameof(name));

            if (_ignoredMembers.TryGetValue(name, out var ignoredConfigurationSource))
            {
                return ignoredConfigurationSource;
            }

            return null;
        }

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
