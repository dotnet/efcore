// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Data.Entity.Infrastructure
{
    /// <summary>
    ///     Provides Entity Framework specific APIs for configuring services in an <see cref="IServiceCollection" />.
    ///     These APIs are usually accessed by calling
    ///     <see cref="EntityFrameworkServiceCollectionExtensions.AddEntityFramework(IServiceCollection)" />
    ///     and then chaining API calls on the returned <see cref="EntityFrameworkServicesBuilder" />.
    /// </summary>
    public class EntityFrameworkServicesBuilder : IInfrastructure<IServiceCollection>
    {
        private readonly IServiceCollection _serviceCollection;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityFrameworkServicesBuilder" /> class.
        /// </summary>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> being configured. </param>
        public EntityFrameworkServicesBuilder([NotNull] IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            _serviceCollection = serviceCollection;
        }

        /// <summary>
        ///     <para>
        ///         Registers the given context as a service in the <see cref="IServiceCollection" /> and ensures services that the context 
        ///         uses are resolved from the <see cref="IServiceProvider" />.
        ///     </para>
        ///     <para>
        ///         You use this method when using dependency injection in your application, such as with ASP.NET.
        ///         For more information on setting up dependency injection, see http://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        /// </summary>
        /// <example>
        ///     <code>
        ///         public void ConfigureServices(IServiceCollection services) 
        ///         {
        ///             var connectionString = "connection string to database";
        /// 
        ///             services.AddEntityFramework() 
        ///                 .AddSqlServer()
        ///                 .AddDbContext&lt;MyContext&gt;(options => options.UseSqlServer(connectionString)); 
        ///         }
        ///     </code>
        /// </example>
        /// <typeparam name="TContext"> The type of context to be registered. </typeparam>
        /// <param name="optionsAction">
        ///     <para>
        ///         An optional action to configure the <see cref="DbContextOptions" /> for the context. This provides an
        ///         alternative to performing configuration of the context by overriding the
        ///         <see cref="DbContext.OnConfiguring" /> method in your derived context.
        ///     </para>
        ///     <para>
        ///         If an action is supplied here, the <see cref="DbContext.OnConfiguring" /> method will still be run if it has
        ///         been overridden on the derived context. <see cref="DbContext.OnConfiguring" /> configuration will be applied
        ///         in addition to configuration performed here.
        ///     </para>
        ///     <para>
        ///         You do not need to expose a <see cref="DbContextOptions" /> constructor parameter for the options to be passed to the
        ///         context. If you choose to expose a constructor parameter, we recommend typing it as the generic
        ///         <see cref="DbContextOptions{TContext}" />. You can use the non-generic <see cref="DbContextOptions"/> but this will only
        ///         work if you have one derived context type registered in your <see cref="IServiceProvider"/>.
        ///     </para>
        /// </param>
        /// <returns>
        ///     A builder that allows further Entity Framework specific setup of the <see cref="IServiceCollection" />.
        /// </returns>
        public virtual EntityFrameworkServicesBuilder AddDbContext<TContext>([CanBeNull] Action<DbContextOptionsBuilder> optionsAction = null)
            where TContext : DbContext
        {
            _serviceCollection.AddSingleton(_ => DbContextOptionsFactory<TContext>(optionsAction));
            _serviceCollection.AddSingleton<DbContextOptions>(p => p.GetRequiredService<DbContextOptions<TContext>>());

            _serviceCollection.AddScoped(typeof(TContext), DbContextActivator.CreateInstance<TContext>);

            return this;
        }

        private static DbContextOptions<TContext> DbContextOptionsFactory<TContext>(
            [CanBeNull] Action<DbContextOptionsBuilder> optionsAction)
            where TContext : DbContext
        {
            var options = new DbContextOptions<TContext>(new Dictionary<Type, IDbContextOptionsExtension>());

            if (optionsAction != null)
            {
                var builder = new DbContextOptionsBuilder<TContext>(options);
                optionsAction(builder);
                options = builder.Options;
            }

            return options;
        }

        /// <summary>
        ///     <para>
        ///         Gets the <see cref="IServiceCollection" /> being configured.
        ///     </para>
        ///     <para>
        ///         This property is intended for use by extension methods that need to make use of services
        ///         not directly exposed in the public API surface.
        ///     </para>
        /// </summary>
        IServiceCollection IInfrastructure<IServiceCollection>.Instance => _serviceCollection;
    }
}
