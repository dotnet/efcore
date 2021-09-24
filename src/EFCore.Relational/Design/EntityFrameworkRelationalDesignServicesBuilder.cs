// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     <para>
    ///         A builder API designed for database providers to use when implementing <see cref="IDesignTimeServices" />.
    ///     </para>
    ///     <para>
    ///         Providers should create an instance of this class, use its methods to register
    ///         services, and then call <see cref="TryAddCoreServices" /> to fill out the remaining Entity
    ///         Framework services.
    ///     </para>
    ///     <para>
    ///         Entity Framework ensures that services are registered with the appropriate scope. In some cases a provider
    ///         may register a service with a different scope, but great care must be taken that all its dependencies
    ///         can handle the new scope, and that it does not cause issue for services that depend on it.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information.
    /// </remarks>
    public class EntityFrameworkRelationalDesignServicesBuilder : EntityFrameworkDesignServicesBuilder
    {
        /// <summary>
        ///     <para>
        ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///         any release. You should only use it directly in your code with extreme caution and knowing that
        ///         doing so can result in application failures when updating to a new Entity Framework Core release.
        ///     </para>
        ///     <para>
        ///         This dictionary is exposed for testing and provider-validation only.
        ///         It should not be used from application code.
        ///     </para>
        /// </summary>
        [EntityFrameworkInternal]
        public static readonly IDictionary<Type, ServiceCharacteristics> RelationalServices
            = new Dictionary<Type, ServiceCharacteristics>
            {
                { typeof(IAnnotationCodeGenerator), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IProviderConfigurationCodeGenerator), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IDatabaseModelFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) }
            };

        /// <summary>
        ///     Creates a new <see cref="EntityFrameworkDesignServicesBuilder" /> for
        ///     registration of provider services.
        /// </summary>
        /// <param name="serviceCollection">The collection to which services will be registered.</param>
        public EntityFrameworkRelationalDesignServicesBuilder(IServiceCollection serviceCollection)
            : base(serviceCollection)
        {
        }

        /// <summary>
        ///     Gets the <see cref="ServiceCharacteristics" /> for the given service type.
        /// </summary>
        /// <param name="serviceType">The type that defines the service API.</param>
        /// <returns>The <see cref="ServiceCharacteristics" /> for the type or <see langword="null" /> if it's not an EF service.</returns>
        protected override ServiceCharacteristics? TryGetServiceCharacteristics(Type serviceType)
            => RelationalServices.TryGetValue(serviceType, out var characteristics)
                ? characteristics
                : base.TryGetServiceCharacteristics(serviceType);

        /// <summary>
        ///     Registers default implementations of all services, including relational services, not already
        ///     registered by the provider. Relational database providers must call this method as the last
        ///     step of service registration--that is, after all provider services have been registered.
        /// </summary>
        /// <returns>This builder, such that further calls can be chained.</returns>
        public override EntityFrameworkServicesBuilder TryAddCoreServices()
        {
            TryAdd<IAnnotationCodeGenerator, AnnotationCodeGenerator>();
#pragma warning disable EF1001 // Internal EF Core API usage.
            TryAdd<ICSharpRuntimeAnnotationCodeGenerator, RelationalCSharpRuntimeAnnotationCodeGenerator>();
#pragma warning restore EF1001 // Internal EF Core API usage.

            ServiceCollectionMap.GetInfrastructure()
                .AddDependencySingleton<AnnotationCodeGeneratorDependencies>()
                .AddDependencySingleton<ProviderCodeGeneratorDependencies>()
                .AddDependencySingleton<RelationalCSharpRuntimeAnnotationCodeGeneratorDependencies>();

            return base.TryAddCoreServices();
        }
    }
}
