// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public abstract class RelationalTestHelpers : TestHelpers
{
    protected virtual EntityFrameworkDesignServicesBuilder CreateEntityFrameworkDesignServicesBuilder(IServiceCollection services)
        => new(services);

    public IServiceProvider CreateDesignServiceProvider(
        IServiceCollection customServices = null,
        Action<EntityFrameworkDesignServicesBuilder> replaceServices = null,
        Type additionalDesignTimeServices = null,
        IOperationReporter reporter = null)
        => CreateDesignServiceProvider(
            CreateContext().GetService<IDatabaseProvider>().Name,
            customServices,
            replaceServices,
            additionalDesignTimeServices,
            reporter);

    public IServiceProvider CreateDesignServiceProvider(
        string provider,
        IServiceCollection customServices = null,
        Action<EntityFrameworkDesignServicesBuilder> replaceServices = null,
        Type additionalDesignTimeServices = null,
        IOperationReporter reporter = null)
        => CreateServiceProvider(
            customServices, services =>
            {
                if (replaceServices != null)
                {
                    var builder = CreateEntityFrameworkDesignServicesBuilder(services);
                    replaceServices(builder);
                }

                if (additionalDesignTimeServices != null)
                {
                    ConfigureDesignTimeServices(additionalDesignTimeServices, services);
                }

                ConfigureProviderServices(provider, services);
                services.AddEntityFrameworkDesignTimeServices(reporter);

                return services;
            });

    private void ConfigureProviderServices(string provider, IServiceCollection services)
    {
        var providerAssembly = Assembly.Load(new AssemblyName(provider));

        var providerServicesAttribute = providerAssembly.GetCustomAttribute<DesignTimeProviderServicesAttribute>();
        if (providerServicesAttribute == null)
        {
            throw new InvalidOperationException(DesignStrings.CannotFindDesignTimeProviderAssemblyAttribute(provider));
        }

        var designTimeServicesType = providerAssembly.GetType(
            providerServicesAttribute.TypeName,
            throwOnError: true,
            ignoreCase: false)!;

        ConfigureDesignTimeServices(designTimeServicesType, services);
    }

    private static void ConfigureDesignTimeServices(
        Type designTimeServicesType,
        IServiceCollection services)
    {
        var designTimeServices = (IDesignTimeServices)Activator.CreateInstance(designTimeServicesType)!;
        designTimeServices.ConfigureDesignTimeServices(services);
    }
}
