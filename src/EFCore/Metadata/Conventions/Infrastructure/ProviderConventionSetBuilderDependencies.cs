// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

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
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
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
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public sealed class ProviderConventionSetBuilderDependencies
    {
        private readonly ICurrentDbContext _currentContext;

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
        ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///         any release. You should only use it directly in your code with extreme caution and knowing that
        ///         doing so can result in application failures when updating to a new Entity Framework Core release.
        ///     </para>
        /// </summary>
        /// <param name="typeMappingSource"> The type mapping source. </param>
        /// <param name="constructorBindingFactory"> The constructor binding factory. </param>
        /// <param name="parameterBindingFactories"> The parameter binding factories. </param>
        /// <param name="memberClassifier"> The member classifier. </param>
        /// <param name="logger"> The model logger. </param>
        /// <param name="validationLogger"> The model validation logger. </param>
        /// <param name="setFinder"> The set finder. </param>
        /// <param name="currentContext"> The current context instance. </param>
        /// <param name="validator"> The model validator. </param>
        public ProviderConventionSetBuilderDependencies(
            [NotNull] ITypeMappingSource typeMappingSource,
            [NotNull] IConstructorBindingFactory constructorBindingFactory,
            [NotNull] IParameterBindingFactories parameterBindingFactories,
            [NotNull] IMemberClassifier memberClassifier,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> validationLogger,
            [NotNull] IDbSetFinder setFinder,
            [NotNull] ICurrentDbContext currentContext,
            [NotNull] IModelValidator validator)
        {
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));
            Check.NotNull(constructorBindingFactory, nameof(constructorBindingFactory));
            Check.NotNull(parameterBindingFactories, nameof(parameterBindingFactories));
            Check.NotNull(memberClassifier, nameof(memberClassifier));
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(validationLogger, nameof(validationLogger));
            Check.NotNull(setFinder, nameof(setFinder));
            Check.NotNull(validator, nameof(validator));

            TypeMappingSource = typeMappingSource;
            ParameterBindingFactories = parameterBindingFactories;
            MemberClassifier = memberClassifier;
            ConstructorBindingFactory = constructorBindingFactory;
            Logger = logger;
            ValidationLogger = validationLogger;
            SetFinder = setFinder;
            _currentContext = currentContext;
            ModelValidator = validator;
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
        ///     The model logger.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Model> Logger { get; }

        /// <summary>
        ///     The model validation logger.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Model.Validation> ValidationLogger { get; }

        /// <summary>
        ///     The set finder.
        /// </summary>
        public IDbSetFinder SetFinder { get; }

        /// <summary>
        ///     The current context instance.
        /// </summary>
        public Type ContextType => _currentContext.Context.GetType();

        /// <summary>
        ///     The model validator.
        /// </summary>
        public IModelValidator ModelValidator { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="typeMappingSource"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ProviderConventionSetBuilderDependencies With([NotNull] ITypeMappingSource typeMappingSource)
            => new ProviderConventionSetBuilderDependencies(
                typeMappingSource, ConstructorBindingFactory, ParameterBindingFactories, MemberClassifier, Logger, ValidationLogger,
                SetFinder, _currentContext, ModelValidator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="constructorBindingFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ProviderConventionSetBuilderDependencies With([NotNull] IConstructorBindingFactory constructorBindingFactory)
            => new ProviderConventionSetBuilderDependencies(
                TypeMappingSource, constructorBindingFactory, ParameterBindingFactories, MemberClassifier, Logger, ValidationLogger,
                SetFinder, _currentContext, ModelValidator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="logger"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ProviderConventionSetBuilderDependencies With([NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
            => new ProviderConventionSetBuilderDependencies(
                TypeMappingSource, ConstructorBindingFactory, ParameterBindingFactories, MemberClassifier, logger, ValidationLogger,
                SetFinder, _currentContext, ModelValidator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="validationLogger"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ProviderConventionSetBuilderDependencies With([NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> validationLogger)
            => new ProviderConventionSetBuilderDependencies(
                TypeMappingSource, ConstructorBindingFactory, ParameterBindingFactories, MemberClassifier, Logger, validationLogger,
                SetFinder, _currentContext, ModelValidator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="parameterBindingFactories"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ProviderConventionSetBuilderDependencies With([NotNull] IParameterBindingFactories parameterBindingFactories)
            => new ProviderConventionSetBuilderDependencies(
                TypeMappingSource, ConstructorBindingFactory, parameterBindingFactories, MemberClassifier, Logger, ValidationLogger,
                SetFinder, _currentContext, ModelValidator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="memberClassifier"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ProviderConventionSetBuilderDependencies With([NotNull] IMemberClassifier memberClassifier)
            => new ProviderConventionSetBuilderDependencies(
                TypeMappingSource, ConstructorBindingFactory, ParameterBindingFactories, memberClassifier, Logger, ValidationLogger,
                SetFinder, _currentContext, ModelValidator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="setFinder"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ProviderConventionSetBuilderDependencies With([NotNull] IDbSetFinder setFinder)
            => new ProviderConventionSetBuilderDependencies(
                TypeMappingSource, ConstructorBindingFactory, ParameterBindingFactories, MemberClassifier, Logger, ValidationLogger,
                setFinder, _currentContext, ModelValidator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="currentContext"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ProviderConventionSetBuilderDependencies With([NotNull] ICurrentDbContext currentContext)
            => new ProviderConventionSetBuilderDependencies(
                TypeMappingSource, ConstructorBindingFactory, ParameterBindingFactories, MemberClassifier, Logger, ValidationLogger,
                SetFinder, currentContext, ModelValidator);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="validator"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ProviderConventionSetBuilderDependencies With([NotNull] IModelValidator validator)
            => new ProviderConventionSetBuilderDependencies(
                TypeMappingSource, ConstructorBindingFactory, ParameterBindingFactories, MemberClassifier, Logger, ValidationLogger,
                SetFinder, _currentContext, validator);
    }
}
