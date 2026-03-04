// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ParameterBindingFactories : IParameterBindingFactories
{
    private readonly IRegisteredServices _registeredServices;
    private readonly List<IParameterBindingFactory> _parameterBindingFactories;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ParameterBindingFactories(
        IEnumerable<IParameterBindingFactory>? registeredFactories,
        IRegisteredServices registeredServices)
    {
        _registeredServices = registeredServices;
        _parameterBindingFactories = registeredFactories?.ToList() ?? [];
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IParameterBindingFactory? FindFactory(Type parameterType, string parameterName)
        => _parameterBindingFactories.FirstOrDefault(f => f.CanBind(parameterType, parameterName))
            ?? (_registeredServices.Services.Contains(parameterType)
                ? new ServiceParameterBindingFactory(parameterType)
                : null);
}
