// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Dynamic;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ModelConfiguration
{
    private readonly Dictionary<Type, PropertyConfiguration> _properties = new();
    private readonly Dictionary<Type, PropertyConfiguration> _typeMappings = new();
    private readonly Dictionary<Type, ComplexPropertyConfiguration> _complexProperties = new();
    private readonly HashSet<Type> _ignoredTypes = [];
    private readonly Dictionary<Type, TypeConfigurationType?> _configurationTypes = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsEmpty()
        => _properties.Count == 0
            && _ignoredTypes.Count == 0
            && _typeMappings.Count == 0
            && _complexProperties.Count == 0;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ModelConfiguration Validate()
    {
        Type? configuredType = null;
        var stringType = GetConfigurationType(typeof(string), null, ref configuredType);
        if (stringType != null
            && stringType != TypeConfigurationType.Property)
        {
            throw new InvalidOperationException(
                CoreStrings.UnconfigurableType(
                    typeof(string).DisplayName(fullName: false),
                    stringType,
                    TypeConfigurationType.Property,
                    configuredType!.DisplayName(fullName: false)));
        }

        configuredType = null;
        var intType = GetConfigurationType(typeof(int?), null, ref configuredType);
        if (intType != null
            && intType != TypeConfigurationType.Property)
        {
            throw new InvalidOperationException(
                CoreStrings.UnconfigurableType(
                    typeof(int?).DisplayName(fullName: false),
                    intType,
                    TypeConfigurationType.Property,
                    configuredType!.DisplayName(fullName: false)));
        }

        configuredType = null;
        var propertyBagType = GetConfigurationType(Model.DefaultPropertyBagType, null, ref configuredType);
        if (propertyBagType != null
            && !propertyBagType.Value.IsEntityType())
        {
            throw new InvalidOperationException(
                CoreStrings.UnconfigurableType(
                    Model.DefaultPropertyBagType.DisplayName(fullName: false),
                    propertyBagType,
                    TypeConfigurationType.SharedTypeEntityType,
                    configuredType!.DisplayName(fullName: false)));
        }

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TypeConfigurationType? GetConfigurationType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
    {
        Type? configuredType = null;
        return GetConfigurationType(type, null, ref configuredType);
    }

    private TypeConfigurationType? GetConfigurationType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type,
        TypeConfigurationType? previousConfiguration,
        ref Type? previousType,
        bool getBaseTypes = true)
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

        if (type.IsNullableValueType())
        {
            configurationType = GetConfigurationType(
                Nullable.GetUnderlyingType(type)!, configurationType, ref configuredType, getBaseTypes: false);
        }

        if (type.IsConstructedGenericType)
        {
            configurationType = GetConfigurationType(
                type.GetGenericTypeDefinition(), configurationType, ref configuredType, getBaseTypes: false);
        }

        if (getBaseTypes)
        {
            if (type.BaseType != null)
            {
                configurationType = GetConfigurationType(
                    type.BaseType, configurationType, ref configuredType);
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
        else if (_properties.ContainsKey(type))
        {
            EnsureCompatible(TypeConfigurationType.Property, type, configurationType, configuredType);
            configurationType = TypeConfigurationType.Property;
            configuredType = type;
        }
        else if (_complexProperties.ContainsKey(type))
        {
            EnsureCompatible(TypeConfigurationType.ComplexType, type, configurationType, configuredType);
            configurationType = TypeConfigurationType.ComplexType;
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
        TypeConfigurationType configurationType,
        Type type,
        TypeConfigurationType? previousConfiguration,
        Type? previousType)
    {
        if (previousConfiguration != null
            && previousConfiguration.Value != configurationType)
        {
            throw new InvalidOperationException(
                CoreStrings.TypeConfigurationConflict(
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
    public virtual IEnumerable<ITypeMappingConfiguration> GetTypeMappingConfigurations()
        => _typeMappings.Values;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ITypeMappingConfiguration? FindTypeMappingConfiguration(Type scalarType)
        => _typeMappings.Count == 0
            ? null
            : _typeMappings.GetValueOrDefault(scalarType);

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
    public virtual void ConfigureComplexProperty(IMutableComplexProperty property)
    {
        var types = property.ClrType.GetBaseTypesAndInterfacesInclusive();
        for (var i = types.Count - 1; i >= 0; i--)
        {
            var type = types[i];

            if (_complexProperties.TryGetValue(type, out var configuration))
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
    public virtual PropertyConfiguration GetOrAddTypeMapping(Type type)
    {
        var typeMappingConfiguration = FindTypeMapping(type);
        if (typeMappingConfiguration == null)
        {
            if (type == typeof(object)
                || type == typeof(ExpandoObject)
                || type == typeof(SortedDictionary<string, object>)
                || type == typeof(Dictionary<string, object>)
                || type.IsNullableValueType()
                || !type.IsInstantiable())
            {
                throw new InvalidOperationException(
                    CoreStrings.UnconfigurableTypeMapping(type.DisplayName(fullName: false)));
            }

            typeMappingConfiguration = new PropertyConfiguration(type);
            _typeMappings.Add(type, typeMappingConfiguration);
        }

        return typeMappingConfiguration;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PropertyConfiguration? FindTypeMapping(Type type)
        => _typeMappings.TryGetValue(type, out var property)
            ? property
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ComplexPropertyConfiguration GetOrAddComplexProperty(Type type)
    {
        var property = FindComplexProperty(type);
        if (property == null)
        {
            RemoveIgnored(type);

            property = new ComplexPropertyConfiguration(type);
            _complexProperties.Add(type, property);
        }

        return property;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ComplexPropertyConfiguration? FindComplexProperty(Type type)
        => _complexProperties.TryGetValue(type, out var property)
            ? property
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool RemoveComplexProperty(Type type)
        => _complexProperties.Remove(type);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AddIgnored(Type type)
    {
        RemoveProperty(type);
        RemoveComplexProperty(type);
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
