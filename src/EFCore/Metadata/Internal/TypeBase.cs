// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract class TypeBase : ConventionAnnotatable, IMutableTypeBase, IConventionTypeBase
    {
        private ConfigurationSource _configurationSource;

        private readonly Dictionary<string, ConfigurationSource> _ignoredMembers
            = new Dictionary<string, ConfigurationSource>(StringComparer.Ordinal);

        private bool _indexerPropertyInitialized;
        private PropertyInfo _indexerPropertyInfo;
        private Dictionary<string, PropertyInfo> _runtimeProperties;
        private Dictionary<string, FieldInfo> _runtimeFields;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected TypeBase([NotNull] string name, [NotNull] Model model, ConfigurationSource configurationSource)
            : this(model, configurationSource)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(model, nameof(model));

            Name = name;
            HasSharedClrType = false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected TypeBase([NotNull] Type type, [NotNull] Model model, ConfigurationSource configurationSource)
            : this(model, configurationSource)
        {
            Check.NotNull(model, nameof(model));

            Name = model.GetDisplayName(type);
            ClrType = type;
            HasSharedClrType = false;
            IsPropertyBag = type.IsPropertyBagType();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected TypeBase([NotNull] string name, [NotNull] Type type, [NotNull] Model model, ConfigurationSource configurationSource)
            : this(model, configurationSource)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(type, nameof(type));
            Check.NotNull(model, nameof(model));

            Name = name;
            ClrType = type;
            HasSharedClrType = true;
            IsPropertyBag = type.IsPropertyBagType();
        }

        private TypeBase([NotNull] Model model, ConfigurationSource configurationSource)
        {
            Model = model;
            _configurationSource = configurationSource;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Type ClrType { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Model Model { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string Name { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool HasSharedClrType { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsPropertyBag { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public virtual ConfigurationSource GetConfigurationSource()
            => _configurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
            => _configurationSource = configurationSource.Max(_configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyDictionary<string, PropertyInfo> GetRuntimeProperties()
        {
            if (ClrType == null)
            {
                return null;
            }

            if (_runtimeProperties == null)
            {
                var runtimeProperties = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);
                foreach (var property in ClrType.GetRuntimeProperties())
                {
                    if (!property.IsStatic()
                        && !runtimeProperties.ContainsKey(property.Name))
                    {
                        runtimeProperties[property.Name] = property;
                    }
                }

                Interlocked.CompareExchange(ref _runtimeProperties, runtimeProperties, null);
            }

            return _runtimeProperties;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyDictionary<string, FieldInfo> GetRuntimeFields()
        {
            if (ClrType == null)
            {
                return null;
            }

            if (_runtimeFields == null)
            {
                var runtimeFields = new Dictionary<string, FieldInfo>(StringComparer.Ordinal);
                foreach (var field in ClrType.GetRuntimeFields())
                {
                    if (!field.IsStatic
                        && !runtimeFields.ContainsKey(field.Name))
                    {
                        runtimeFields[field.Name] = field;
                    }
                }

                Interlocked.CompareExchange(ref _runtimeFields, runtimeFields, null);
            }

            return _runtimeFields;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual PropertyInfo FindIndexerPropertyInfo()
        {
            if (ClrType == null)
            {
                return null;
            }

            if (!_indexerPropertyInitialized)
            {
                var indexerPropertyInfo = ClrType.FindIndexerProperty();

                Interlocked.CompareExchange(ref _indexerPropertyInfo, indexerPropertyInfo, null);
                _indexerPropertyInitialized = true;
            }

            return _indexerPropertyInfo;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual PropertyAccessMode? SetPropertyAccessMode(
            PropertyAccessMode? propertyAccessMode,
            ConfigurationSource configurationSource)
        {
            this.SetOrRemoveAnnotation(CoreAnnotationNames.PropertyAccessMode, propertyAccessMode, configurationSource);

            return propertyAccessMode;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual PropertyAccessMode? SetNavigationAccessMode(
            PropertyAccessMode? propertyAccessMode,
            ConfigurationSource configurationSource)
        {
            this.SetOrRemoveAnnotation(CoreAnnotationNames.NavigationAccessMode, propertyAccessMode, configurationSource);

            return propertyAccessMode;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string AddIgnored([NotNull] string name, ConfigurationSource configurationSource)
        {
            Check.NotNull(name, nameof(name));

            if (_ignoredMembers.TryGetValue(name, out var existingIgnoredConfigurationSource))
            {
                _ignoredMembers[name] = configurationSource.Max(existingIgnoredConfigurationSource);
                return name;
            }

            _ignoredMembers[name] = configurationSource;

            return OnTypeMemberIgnored(name);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public abstract string OnTypeMemberIgnored([NotNull] string name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<string> GetIgnoredMembers()
            => _ignoredMembers.Keys;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? FindDeclaredIgnoredConfigurationSource([NotNull] string name)
            => _ignoredMembers.TryGetValue(Check.NotEmpty(name, nameof(name)), out var ignoredConfigurationSource)
                ? (ConfigurationSource?)ignoredConfigurationSource
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? FindIgnoredConfigurationSource(string name)
            => FindDeclaredIgnoredConfigurationSource(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsIgnored(string name)
            => FindIgnoredConfigurationSource(name) != null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string RemoveIgnored(string name)
        {
            Check.NotNull(name, nameof(name));
            return _ignoredMembers.Remove(name) ? name : null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IModel ITypeBase.Model
        {
            [DebuggerStepThrough] get => Model;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableModel IMutableTypeBase.Model
        {
            [DebuggerStepThrough] get => Model;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionModel IConventionTypeBase.Model
        {
            [DebuggerStepThrough] get => Model;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        Type ITypeBase.ClrType
        {
            [DebuggerStepThrough] get => ClrType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        string IMutableTypeBase.AddIgnored(string name)
            => AddIgnored(name, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        string IConventionTypeBase.AddIgnored(string name, bool fromDataAnnotation)
            => AddIgnored(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }
}
