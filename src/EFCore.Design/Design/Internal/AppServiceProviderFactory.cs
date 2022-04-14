// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Hosting;

namespace Microsoft.EntityFrameworkCore.Design.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class AppServiceProviderFactory
{
    private readonly Assembly _startupAssembly;
    private readonly IOperationReporter _reporter;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public AppServiceProviderFactory(Assembly startupAssembly, IOperationReporter reporter)
    {
        _startupAssembly = startupAssembly;
        _reporter = reporter;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IServiceProvider Create(string[] args)
    {
        _reporter.WriteVerbose(DesignStrings.FindingServiceProvider(_startupAssembly.GetName().Name));

        return CreateFromHosting(args)
            ?? CreateEmptyServiceProvider();
    }

    private IServiceProvider? CreateFromHosting(string[] args)
    {
        _reporter.WriteVerbose(DesignStrings.FindingHostingServices);

        var serviceProviderFactory = HostFactoryResolver.ResolveServiceProviderFactory(_startupAssembly);
        if (serviceProviderFactory == null)
        {
            _reporter.WriteVerbose(DesignStrings.NoCreateHostBuilder);

            return null;
        }

        var aspnetCoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var dotnetEnvironment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var environment = aspnetCoreEnvironment
            ?? dotnetEnvironment
            ?? "Development";
        if (aspnetCoreEnvironment == null)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);
        }

        if (dotnetEnvironment == null)
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", environment);
        }

        _reporter.WriteVerbose(DesignStrings.UsingEnvironment(environment));

        try
        {
            var services = serviceProviderFactory(args);
            if (services == null)
            {
                _reporter.WriteWarning(DesignStrings.MalformedCreateHostBuilder);

                return null;
            }

            _reporter.WriteVerbose(DesignStrings.UsingHostingServices);

            return services.CreateScope().ServiceProvider;
        }
        catch (Exception ex)
        {
            if (ex is TargetInvocationException)
            {
                ex = ex.InnerException!;
            }

            _reporter.WriteVerbose(ex.ToString());
            _reporter.WriteWarning(DesignStrings.InvokeCreateHostBuilderFailed(ex.Message));

            return null;
        }
    }

    private IServiceProvider CreateEmptyServiceProvider()
    {
        _reporter.WriteVerbose(DesignStrings.NoServiceProvider);

        return new ServiceCollection().BuildServiceProvider();
    }
}
