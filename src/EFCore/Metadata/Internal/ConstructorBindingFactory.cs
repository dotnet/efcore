// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ConstructorBindingFactory : IConstructorBindingFactory
{
    private readonly IPropertyParameterBindingFactory _propertyFactory;
    private readonly IParameterBindingFactories _factories;

    private static readonly MethodInfo _createInstance =
        typeof(Activator).GetMethod(nameof(Activator.CreateInstance), BindingFlags.Public | BindingFlags.Static, [typeof(Type)])!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ConstructorBindingFactory(
        IPropertyParameterBindingFactory propertyFactory,
        IParameterBindingFactories factories)
    {
        _propertyFactory = propertyFactory;
        _factories = factories;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void GetBindings(
        IConventionEntityType entityType,
        out InstantiationBinding constructorBinding,
        out InstantiationBinding? serviceOnlyBinding)
        => GetBindings(
            entityType,
            static (f, e, p, n) => f.FindParameter((IEntityType)e, p, n),
            static (f, e, p, n) => f?.Bind(e, p, n),
            out constructorBinding,
            out serviceOnlyBinding);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void GetBindings(
        IMutableEntityType entityType,
        out InstantiationBinding constructorBinding,
        out InstantiationBinding? serviceOnlyBinding)
        => GetBindings(
            entityType,
            static (f, e, p, n) => f.FindParameter((IEntityType)e, p, n),
            static (f, e, p, n) => f?.Bind(e, p, n),
            out constructorBinding,
            out serviceOnlyBinding);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void GetBindings(
        IReadOnlyEntityType entityType,
        out InstantiationBinding constructorBinding,
        out InstantiationBinding? serviceOnlyBinding)
        => GetBindings(
            entityType,
            static (f, e, p, n) => f.FindParameter((IEntityType)e, p, n),
            static (f, e, p, n) => f?.Bind(e, p, n),
            out constructorBinding,
            out serviceOnlyBinding);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void GetBindings(
        IReadOnlyComplexType complexType,
        out InstantiationBinding constructorBinding,
        out InstantiationBinding? serviceOnlyBinding)
        => GetBindings(
            complexType,
            static (f, e, p, n) => f.FindParameter((IComplexType)e, p, n),
            static (f, e, p, n) => null,
            out constructorBinding,
            out serviceOnlyBinding);

    private void GetBindings<T>(
        T type,
        Func<IPropertyParameterBindingFactory, T, Type, string, ParameterBinding?> bindToProperty,
        Func<IParameterBindingFactory?, T, Type, string, ParameterBinding?> bind,
        out InstantiationBinding constructorBinding,
        out InstantiationBinding? serviceOnlyBinding)
        where T : IReadOnlyTypeBase
    {
        var maxServiceParams = 0;
        var maxServiceOnlyParams = 0;
        var minPropertyParams = int.MaxValue;
        var foundBindings = new List<InstantiationBinding>();
        var foundServiceOnlyBindings = new List<InstantiationBinding>();
        var bindingFailures = new List<IEnumerable<ParameterInfo>>();

        var clrType = type.ClrType.UnwrapNullableType();
        var constructors = clrType.GetTypeInfo().DeclaredConstructors.Where(c => !c.IsStatic).ToList();
        foreach (var constructor in constructors)
        {
            // Trying to find the constructor with the most service properties
            // followed by the least scalar property parameters
            if (TryBindConstructor(
                    type, constructor, bindToProperty, bind, out var binding, out var failures))
            {
                var serviceParamCount = binding.ParameterBindings.OfType<ServiceParameterBinding>().Count();
                var propertyParamCount = binding.ParameterBindings.Count - serviceParamCount;

                if (propertyParamCount == 0)
                {
                    if (serviceParamCount == maxServiceOnlyParams)
                    {
                        foundServiceOnlyBindings.Add(binding);
                    }
                    else if (serviceParamCount > maxServiceOnlyParams)
                    {
                        foundServiceOnlyBindings.Clear();
                        foundServiceOnlyBindings.Add(binding);

                        maxServiceOnlyParams = serviceParamCount;
                    }
                }

                if (serviceParamCount == maxServiceParams
                    && propertyParamCount == minPropertyParams)
                {
                    foundBindings.Add(binding);
                }
                else if (serviceParamCount > maxServiceParams)
                {
                    foundBindings.Clear();
                    foundBindings.Add(binding);

                    maxServiceParams = serviceParamCount;
                    minPropertyParams = propertyParamCount;
                }
                else if (propertyParamCount < minPropertyParams)
                {
                    foundBindings.Clear();
                    foundBindings.Add(binding);

                    maxServiceParams = serviceParamCount;
                    minPropertyParams = propertyParamCount;
                }
            }
            else
            {
                bindingFailures.Add(failures);
            }
        }

        if (foundBindings.Count == 0
            && constructors.Count == 0
            && clrType.IsValueType)
        {
            foundBindings.Add(new DefaultValueBinding(clrType));
        }

        if (foundBindings.Count == 0)
        {
            var constructorErrors = bindingFailures.SelectMany(f => f)
                .GroupBy(f => (ConstructorInfo)f.Member)
                .Select(
                    x => "    "
                        + CoreStrings.ConstructorBindingFailed(
                            string.Join("', '", x.Select(f => f.Name)),
                            $"{type.DisplayName()}({string.Join(", ", ConstructConstructor(x))})")
                );

            throw new InvalidOperationException(
                CoreStrings.ConstructorNotFound(
                    type.DisplayName(),
                    Environment.NewLine + string.Join(Environment.NewLine, constructorErrors) + Environment.NewLine));
        }

        if (foundBindings.Count > 1)
        {
            throw new InvalidOperationException(
                CoreStrings.ConstructorConflict(
                    FormatConstructorString(type, foundBindings[0]),
                    FormatConstructorString(type, foundBindings[1])));
        }

        constructorBinding = foundBindings[0];
        serviceOnlyBinding = foundServiceOnlyBindings.Count == 1 ? foundServiceOnlyBindings[0] : null;

        IEnumerable<string> ConstructConstructor(IGrouping<ConstructorInfo, ParameterInfo> parameters)
            => parameters.Key.GetParameters().Select(y => $"{y.ParameterType.ShortDisplayName()} {y.Name}");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryBindConstructor(
        IMutableEntityType entityType,
        ConstructorInfo constructor,
        [NotNullWhen(true)] out InstantiationBinding? binding,
        [NotNullWhen(false)] out IEnumerable<ParameterInfo>? unboundParameters)
        => TryBindConstructor(
            entityType,
            constructor,
            static (f, e, p, n) => f.FindParameter((IEntityType)e, p, n),
            static (f, e, p, n) => f?.Bind(e, p, n),
            out binding,
            out unboundParameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryBindConstructor(
        IConventionEntityType entityType,
        ConstructorInfo constructor,
        [NotNullWhen(true)] out InstantiationBinding? binding,
        [NotNullWhen(false)] out IEnumerable<ParameterInfo>? unboundParameters)
        => TryBindConstructor(
            entityType,
            constructor,
            static (f, e, p, n) => f.FindParameter((IEntityType)e, p, n),
            static (f, e, p, n) => f?.Bind(e, p, n),
            out binding,
            out unboundParameters);

    private bool TryBindConstructor<T>(
        T entityType,
        ConstructorInfo constructor,
        Func<IPropertyParameterBindingFactory, T, Type, string, ParameterBinding?> bindToProperty,
        Func<IParameterBindingFactory?, T, Type, string, ParameterBinding?> bind,
        [NotNullWhen(true)] out InstantiationBinding? binding,
        [NotNullWhen(false)] out IEnumerable<ParameterInfo>? unboundParameters)
        where T : IReadOnlyTypeBase
    {
        var bindings = new List<ParameterBinding>();
        List<ParameterInfo>? unboundParametersList = null;
        foreach (var parameter in constructor.GetParameters())
        {
            var parameterBinding = BindParameter(entityType, bindToProperty, bind, parameter);
            if (parameterBinding == null)
            {
                unboundParametersList ??= [];
                unboundParametersList.Add(parameter);
            }
            else
            {
                bindings.Add(parameterBinding);
            }
        }

        if (unboundParametersList != null)
        {
            unboundParameters = unboundParametersList;
            binding = null;

            return false;
        }

        unboundParameters = null;
        binding = new ConstructorBinding(constructor, bindings);

        return true;
    }

    private ParameterBinding? BindParameter<T>(
        T entityType,
        Func<IPropertyParameterBindingFactory, T, Type, string, ParameterBinding?> bindToProperty,
        Func<IParameterBindingFactory?, T, Type, string, ParameterBinding?> bind,
        ParameterInfo p)
        where T : IReadOnlyTypeBase
        => string.IsNullOrEmpty(p.Name)
            ? null
            : bindToProperty(_propertyFactory, entityType, p.ParameterType, p.Name)
            ?? bind(_factories.FindFactory(p.ParameterType, p.Name), entityType, p.ParameterType, p.Name);

    private static string FormatConstructorString<T>(T entityType, InstantiationBinding binding)
        where T : IReadOnlyTypeBase
        => entityType.ClrType.ShortDisplayName()
            + "("
            + string.Join(", ", binding.ParameterBindings.Select(b => b.ParameterType.ShortDisplayName()))
            + ")";
}
