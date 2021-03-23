// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
    public class EntityFrameworkDesignServicesBuilder : EntityFrameworkServicesBuilder
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
        public static readonly IDictionary<Type, ServiceCharacteristics> Services
            = new Dictionary<Type, ServiceCharacteristics>{
                { typeof(IDbContextLogger), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IDiagnosticsLogger<>), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(ICSharpSlimAnnotationCodeGenerator), new ServiceCharacteristics(ServiceLifetime.Singleton) }
            };

        /// <summary>
        ///     Creates a new <see cref="EntityFrameworkDesignServicesBuilder" /> for
        ///     registration of provider services.
        /// </summary>
        /// <param name="serviceCollection"> The collection to which services will be registered. </param>
        public EntityFrameworkDesignServicesBuilder(IServiceCollection serviceCollection)
            : base(serviceCollection)
        {
        }

        /// <summary>
        ///     Gets the <see cref="ServiceCharacteristics" /> for the given service type.
        /// </summary>
        /// <param name="serviceType"> The type that defines the service API. </param>
        /// <returns> The <see cref="ServiceCharacteristics" /> for the type. </returns>
        /// <exception cref="InvalidOperationException"> when the type is not an EF service. </exception>
        protected override ServiceCharacteristics GetServiceCharacteristics(Type serviceType)
            => Services.TryGetValue(serviceType, out var characteristics)
                ? characteristics
                : base.GetServiceCharacteristics(serviceType);

        /// <summary>
        ///     Registers default implementations of all services, including relational services, not already
        ///     registered by the provider. Relational database providers must call this method as the last
        ///     step of service registration--that is, after all provider services have been registered.
        /// </summary>
        /// <returns> This builder, such that further calls can be chained. </returns>
        public override EntityFrameworkServicesBuilder TryAddCoreServices()
        {
            TryAdd<IDbContextLogger, NullDbContextLogger>();
            TryAdd(typeof(IDiagnosticsLogger<>), typeof(DiagnosticsLogger<>));
            TryAdd<ILoggingOptions, LoggingOptions>();
            TryAdd<ICSharpSlimAnnotationCodeGenerator, CSharpSlimAnnotationCodeGenerator>();

            ServiceCollectionMap.GetInfrastructure()
                .AddDependencySingleton<CSharpSlimAnnotationCodeGeneratorDependencies>();

            return this;
        }
    }
}
