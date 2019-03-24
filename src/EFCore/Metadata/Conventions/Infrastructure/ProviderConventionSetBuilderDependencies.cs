// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="ProviderConventionSetBuilder" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///         directly from your code. This API may change or be removed in future releases.
    ///     </para>
    ///     <para>
    ///         Do not construct instances of this class directly from either provider or application code as the
    ///         constructor signature may change as new dependencies are added. Instead, use this type in
    ///         your constructor so that an instance will be created and injected automatically by the
    ///         dependency injection container. To create an instance with some dependent services replaced,
    ///         first resolve the object from the dependency injection container, then replace selected
    ///         services using the 'With...' methods. Do not call the constructor at any point in this process.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public sealed class ProviderConventionSetBuilderDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="ProviderConventionSetBuilder" />.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from either provider or application code as it may change
        ///         as new dependencies are added. Instead, use this type in your constructor so that an instance
        ///         will be created and injected automatically by the dependency injection container. To create
        ///         an instance with some dependent services replaced, first resolve the object from the dependency
        ///         injection container, then replace selected services using the 'With...' methods. Do not call
        ///         the constructor at any point in this process.
        ///     </para>
        ///     <para>
        ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///         directly from your code. This API may change or be removed in future releases.
        ///     </para>
        /// </summary>
        /// <param name="typeMappingSource"> The type mapping source. </param>
        /// <param name="constructorBindingFactory"> The constructor binding factory. </param>
        /// <param name="parameterBindingFactories"> The parameter binding factories. </param>
        /// <param name="memberClassifier"> The member classifier. </param>
        /// <param name="logger"> The model logger. </param>
        /// <param name="setFinder"> The set finder. </param>
        /// <param name="context"> The current context instance. </param>
        public ProviderConventionSetBuilderDependencies(
            [NotNull] ITypeMappingSource typeMappingSource,
            [CanBeNull] IConstructorBindingFactory constructorBindingFactory,
            [CanBeNull] IParameterBindingFactories parameterBindingFactories,
            [CanBeNull] IMemberClassifier memberClassifier,
            [CanBeNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger,
            [CanBeNull] IDbSetFinder setFinder,
            [CanBeNull] ICurrentDbContext context)
        {
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));

            TypeMappingSource = typeMappingSource;

            if (parameterBindingFactories == null)
            {
                parameterBindingFactories = new ParameterBindingFactories(
                    null,
                    new RegisteredServices(Enumerable.Empty<Type>()));
            }

            ParameterBindingFactories = parameterBindingFactories;

            if (memberClassifier == null)
            {
                memberClassifier = new MemberClassifier(
                    typeMappingSource,
                    parameterBindingFactories);
            }

            MemberClassifier = memberClassifier;

            if (constructorBindingFactory == null)
            {
                ConstructorBindingFactory = new ConstructorBindingFactory(
                    new PropertyParameterBindingFactory(),
                    parameterBindingFactories);
            }

            ConstructorBindingFactory = constructorBindingFactory;

            Logger = logger
                     ?? new DiagnosticsLogger<DbLoggerCategory.Model>(
                         new ScopedLoggerFactory(new LoggerFactory(), dispose: true),
                         new LoggingOptions(),
                         new DiagnosticListener(""),
                         new LoggingDefinitions());

            SetFinder = setFinder;
            Context = context;
        }

        /// <summary>
        ///     The type mapping source.
        /// </summary>
        public ITypeMappingSource TypeMappingSource { get; }

        /// <summary>
        ///     The parameter binding factories.
        /// </summary>
        public IParameterBindingFactories ParameterBindingFactories { get; }

        /// <summary>
        ///     The member classifier.
        /// </summary>
        public IMemberClassifier MemberClassifier { get; }

        /// <summary>
        ///     The constructor binding factory.
        /// </summary>
        public IConstructorBindingFactory ConstructorBindingFactory { get; }

        /// <summary>
        ///     The logger.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Model> Logger { get; }

        /// <summary>
        ///     The set finder.
        /// </summary>
        public IDbSetFinder SetFinder { get; }

        /// <summary>
        ///     The current context instance.
        /// </summary>
        public ICurrentDbContext Context { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="typeMappingSource"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ProviderConventionSetBuilderDependencies With([NotNull] ITypeMappingSource typeMappingSource)
            => new ProviderConventionSetBuilderDependencies(
                typeMappingSource, ConstructorBindingFactory, ParameterBindingFactories, MemberClassifier, Logger, SetFinder, Context);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="constructorBindingFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ProviderConventionSetBuilderDependencies With([NotNull] IConstructorBindingFactory constructorBindingFactory)
            => new ProviderConventionSetBuilderDependencies(
                TypeMappingSource, constructorBindingFactory, ParameterBindingFactories, MemberClassifier, Logger, SetFinder, Context);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="logger"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ProviderConventionSetBuilderDependencies With([NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
            => new ProviderConventionSetBuilderDependencies(
                TypeMappingSource, ConstructorBindingFactory, ParameterBindingFactories, MemberClassifier, logger, SetFinder, Context);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="parameterBindingFactories"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ProviderConventionSetBuilderDependencies With([NotNull] IParameterBindingFactories parameterBindingFactories)
            => new ProviderConventionSetBuilderDependencies(
                TypeMappingSource, ConstructorBindingFactory, parameterBindingFactories, MemberClassifier, Logger, SetFinder, Context);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="memberClassifier"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ProviderConventionSetBuilderDependencies With([NotNull] IMemberClassifier memberClassifier)
            => new ProviderConventionSetBuilderDependencies(
                TypeMappingSource, ConstructorBindingFactory, ParameterBindingFactories, memberClassifier, Logger, SetFinder, Context);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="setFinder"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ProviderConventionSetBuilderDependencies With([NotNull] IDbSetFinder setFinder)
            => new ProviderConventionSetBuilderDependencies(
                TypeMappingSource, ConstructorBindingFactory, ParameterBindingFactories, MemberClassifier, Logger, setFinder, Context);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="context"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ProviderConventionSetBuilderDependencies With([NotNull] ICurrentDbContext context)
            => new ProviderConventionSetBuilderDependencies(
                TypeMappingSource, ConstructorBindingFactory, ParameterBindingFactories, MemberClassifier, Logger, SetFinder, context);
    }
}
