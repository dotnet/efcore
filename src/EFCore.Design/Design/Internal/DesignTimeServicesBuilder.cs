// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Design.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class DesignTimeServicesBuilder
{
    private readonly Assembly _assembly;
    private readonly Assembly _startupAssembly;
    private readonly IOperationReporter _reporter;
    private readonly string[] _args;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public DesignTimeServicesBuilder(
        Assembly assembly,
        Assembly startupAssembly,
        IOperationReporter reporter,
        string[] args)
    {
        _startupAssembly = startupAssembly;
        _reporter = reporter;
        _args = args;
        _assembly = assembly;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IServiceProvider Build(DbContext context)
        => CreateServiceCollection(context).BuildServiceProvider();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IServiceCollection CreateServiceCollection(DbContext context)
    {
        var services = new ServiceCollection();
        var provider = context.GetService<IDatabaseProvider>().Name;

        services.AddDbContextDesignTimeServices(context);
        ConfigureReferencedServices(services, provider);
        ConfigureProviderServices(provider, services);
        services.AddEntityFrameworkDesignTimeServices(_reporter);
        ConfigureUserServices(services);
        return services;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IServiceProvider Build(string provider)
        => CreateServiceCollection(provider).BuildServiceProvider();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IServiceCollection CreateServiceCollection(string provider)
    {
        var services = new ServiceCollection();

        ConfigureReferencedServices(services, provider);
        ConfigureProviderServices(provider, services, throwOnError: true);
        services.AddEntityFrameworkDesignTimeServices(_reporter, GetApplicationServices);
        ConfigureUserServices(services);
        return services;
    }

    private IServiceProvider GetApplicationServices()
        => new AppServiceProviderFactory(_startupAssembly, _reporter).Create(_args);

    private void ConfigureUserServices(IServiceCollection services)
    {
        _reporter.WriteVerbose(DesignStrings.FindingDesignTimeServices(_startupAssembly.GetName().Name));

        var designTimeServicesType = _startupAssembly.GetLoadableDefinedTypes()
            .Where(t => typeof(IDesignTimeServices).IsAssignableFrom(t)).Select(t => t.AsType())
            .FirstOrDefault();
        if (designTimeServicesType == null)
        {
            _reporter.WriteVerbose(DesignStrings.NoDesignTimeServices);

            return;
        }

        _reporter.WriteVerbose(DesignStrings.UsingDesignTimeServices(designTimeServicesType.ShortDisplayName()));

        ConfigureDesignTimeServices(designTimeServicesType, services);
    }

    private void ConfigureReferencedServices(IServiceCollection services, string provider)
    {
        _reporter.WriteVerbose(DesignStrings.FindingReferencedServices(_startupAssembly.GetName().Name));
        _reporter.WriteVerbose(DesignStrings.FindingReferencedServices(_assembly.GetName().Name));

        var references = _startupAssembly.GetCustomAttributes<DesignTimeServicesReferenceAttribute>()
            .Concat(_assembly.GetCustomAttributes<DesignTimeServicesReferenceAttribute>())
            .Distinct()
            .ToList();

        if (references.Count == 0)
        {
            _reporter.WriteVerbose(DesignStrings.NoReferencedServices);

            return;
        }

        foreach (var reference in references)
        {
            if (reference.ForProvider != null
                && !string.Equals(reference.ForProvider, provider, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var designTimeServicesType = Type.GetType(reference.TypeName, throwOnError: true)!;

            _reporter.WriteVerbose(
                DesignStrings.UsingReferencedServices(designTimeServicesType.Assembly.GetName().Name));

            ConfigureDesignTimeServices(designTimeServicesType, services);
        }
    }

    private void ConfigureProviderServices(string provider, IServiceCollection services, bool throwOnError = false)
    {
        _reporter.WriteVerbose(DesignStrings.FindingProviderServices(provider));

        Assembly providerAssembly;
        try
        {
            providerAssembly = Assembly.Load(new AssemblyName(provider));
        }
        catch (Exception ex)
        {
            var message = DesignStrings.CannotFindRuntimeProviderAssembly(provider);

            if (!throwOnError)
            {
                _reporter.WriteVerbose(message);

                return;
            }

            throw new OperationException(message, ex);
        }

        var providerServicesAttribute = providerAssembly.GetCustomAttribute<DesignTimeProviderServicesAttribute>();
        if (providerServicesAttribute == null)
        {
            var message = DesignStrings.CannotFindDesignTimeProviderAssemblyAttribute(
                provider);

            if (!throwOnError)
            {
                _reporter.WriteVerbose(message);

                return;
            }

            throw new InvalidOperationException(message);
        }

        var designTimeServicesType = providerAssembly.GetType(
            providerServicesAttribute.TypeName,
            throwOnError: true,
            ignoreCase: false)!;

        _reporter.WriteVerbose(DesignStrings.UsingProviderServices(provider));

        ConfigureDesignTimeServices(designTimeServicesType, services);
    }

    private static void ConfigureDesignTimeServices(
        Type designTimeServicesType,
        IServiceCollection services)
    {
        Check.DebugAssert(designTimeServicesType != null, "designTimeServicesType is null.");

        var designTimeServices = (IDesignTimeServices)Activator.CreateInstance(designTimeServicesType)!;
        designTimeServices.ConfigureDesignTimeServices(services);
    }
}
