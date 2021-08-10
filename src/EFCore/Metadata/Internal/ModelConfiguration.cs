// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public partial class ModelConfiguration
    {
        private readonly Dictionary<Type, PropertyConfiguration> _properties = new();
        private readonly HashSet<Type> _ignoredTypes = new();
        private readonly Dictionary<Type, TypeConfigurationType?> _configurationTypes = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ModelConfiguration()
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsEmpty()
            => _properties.Count == 0 && _ignoredTypes.Count == 0;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual TypeConfigurationType? GetConfigurationType(Type type)
        {
            Type? configuredType = null;
            return GetConfigurationType(type, null, ref configuredType);
        }

        private TypeConfigurationType? GetConfigurationType(
            Type type, TypeConfigurationType? previousConfiguration, ref Type? previousType, bool getBaseTypes = true)
        {
            if (_configurationTypes.TryGetValue(type, out var configurationType))
            {
                if (configurationType.HasValue)
                {
                    EnsureCompatible(configurationType.Value, type, previousConfiguration, previousType);
                    previousType = type;
                }

                return configurationType ?? previousConfiguration;
            }

            Type? configuredType = null;
            if (getBaseTypes)
            {
                if (type.BaseType != null)
                {
                    configurationType = GetConfigurationType(
                        type.BaseType, configurationType, ref configuredType);
                }

                if (type.IsConstructedGenericType)
                {
                    configurationType = GetConfigurationType(
                        type.GetGenericTypeDefinition(), configurationType, ref configuredType, getBaseTypes: false);
                }

                foreach (var @interface in type.GetDeclaredInterfaces())
                {
                    configurationType = GetConfigurationType(
                        @interface, configurationType, ref configuredType, getBaseTypes: false);
                }
            }

            if (_ignoredTypes.Contains(type))
            {
                EnsureCompatible(TypeConfigurationType.Ignored, type, configurationType, configuredType);
                configurationType = TypeConfigurationType.Ignored;
                configuredType = type;
            }

            if (_properties.ContainsKey(type))
            {
                EnsureCompatible(TypeConfigurationType.Property, type, configurationType, configuredType);
                configurationType = TypeConfigurationType.Property;
                configuredType = type;
            }

            if (configurationType.HasValue)
            {
                EnsureCompatible(configurationType.Value, configuredType!, previousConfiguration, previousType);
                previousType = configuredType;
            }

            _configurationTypes[type] = configurationType;
            return configurationType ?? previousConfiguration;
        }

        private static void EnsureCompatible(
            TypeConfigurationType configurationType, Type type,
            TypeConfigurationType? previousConfiguration, Type? previousType)
        {
            if (previousConfiguration != null
                && previousConfiguration.Value != configurationType)
            {
                throw new InvalidOperationException(CoreStrings.TypeConfigurationConflict(
                    type.DisplayName(fullName: false), configurationType,
                    previousType?.DisplayName(fullName: false), previousConfiguration.Value));
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<IPropertyTypeConfiguration> GetPropertyTypeConfigurations()
            => _properties.Values;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<IPropertyTypeConfiguration>? FindPropertyTypeConfigurations(Type propertyType)
            => GetConfigurationType(propertyType) != TypeConfigurationType.Property
                ? null
                : propertyType.GetBaseTypesAndInterfacesInclusive()
                    .Select(type => _properties.GetValueOrDefault(type)!)
                    .Where(type => type != null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ConfigureProperty(IMutableProperty property)
        {
            var types = property.ClrType.GetBaseTypesAndInterfacesInclusive();
            for (var i = types.Count - 1; i >= 0; i--)
            {
                var type = types[i];

                if (_properties.TryGetValue(type, out var configuration))
                {
                    configuration.Apply(property);
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual PropertyConfiguration GetOrAddProperty(Type type)
        {
            if (type.UnwrapNullableType() == typeof(object)
                || type == Model.DefaultPropertyBagType)
            {
                throw new InvalidOperationException(
                    CoreStrings.UnconfigurableType(type.DisplayName(fullName: false), TypeConfigurationType.Property));
            }

            var property = FindProperty(type);
            if (property == null)
            {
                RemoveIgnored(type);

                property = new PropertyConfiguration(type);
                _properties.Add(type, property);
            }

            return property;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual PropertyConfiguration? FindProperty(Type type)
            => _properties.TryGetValue(type, out var property)
            ? property
            : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool RemoveProperty(Type type)
            => _properties.Remove(type);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void AddIgnored(Type type)
        {
            if (type.UnwrapNullableType() == typeof(int)
                || type.UnwrapNullableType() == typeof(int?)
                || type.UnwrapNullableType() == typeof(string)
                || type.UnwrapNullableType() == typeof(object)
                || type == Model.DefaultPropertyBagType)
            {
                throw new InvalidOperationException(
                    CoreStrings.UnconfigurableType(type.DisplayName(fullName: false), TypeConfigurationType.Ignored));
            }

            RemoveProperty(type);
            _ignoredTypes.Add(type);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsIgnored(Type type)
            => _ignoredTypes.Contains(type);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool RemoveIgnored(Type type)
            => _ignoredTypes.Remove(type);
    }
}
