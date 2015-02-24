// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Infrastructure
{
    /// <summary>
    ///     Provides Entity Framework specific APIs for configuring services in an <see cref="IServiceCollection" />.
    ///     These APIs are usually accessed by calling
    ///     <see cref="EntityFrameworkServiceCollectionExtensions.AddEntityFramework(IServiceCollection, IConfiguration)" />
    ///     and then chaining API calls on the returned <see cref="EntityFrameworkServicesBuilder" />.
    /// </summary>
    public class EntityFrameworkServicesBuilder : IAccessor<IServiceCollection>, IAccessor<IConfiguration>
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly IConfiguration _configuration;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityFrameworkServicesBuilder" /> class.
        /// </summary>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> being configured. </param>
        /// <param name="configuration">
        ///     The configuration for the application. This configuration may contain Entity Framework specific settings
        ///     which will be passed into context instances that are created by dependency injection.
        /// </param>
        public EntityFrameworkServicesBuilder(
            [NotNull] IServiceCollection serviceCollection,
            [CanBeNull] IConfiguration configuration = null)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            _serviceCollection = serviceCollection;
            _configuration = configuration;
        }

        /// <summary>
        ///     Gets the <see cref="IServiceCollection" /> being configured.
        /// </summary>
        IServiceCollection IAccessor<IServiceCollection>.Service => _serviceCollection;

        /// <summary>
        ///     The configuration for the application, or null if no configuration has been created. This configuration
        ///     may contain Entity Framework specific settings which will be passed into context instances that are
        ///     created by dependency injection.
        /// </summary>
        IConfiguration IAccessor<IConfiguration>.Service => _configuration;
    }
}
