// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
    public sealed record ProviderConventionSetBuilderDependencies
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
        ///     <para>
        ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///         any release. You should only use it directly in your code with extreme caution and knowing that
        ///         doing so can result in application failures when updating to a new Entity Framework Core release.
        ///     </para>
        /// </summary>
        [EntityFrameworkInternal]
        public ProviderConventionSetBuilderDependencies(
            ITypeMappingSource typeMappingSource,
            IConstructorBindingFactory constructorBindingFactory,
            IParameterBindingFactories parameterBindingFactories,
            IMemberClassifier memberClassifier,
            IDiagnosticsLogger<DbLoggerCategory.Model> logger,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> validationLogger,
            IDbSetFinder setFinder,
            ICurrentDbContext currentContext,
            IModelValidator validator)
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
            ConstructorBindingFactory = constructorBindingFactory;
            ParameterBindingFactories = parameterBindingFactories;
            MemberClassifier = memberClassifier;
            Logger = logger;
            ValidationLogger = validationLogger;
            SetFinder = setFinder;
            _currentContext = currentContext;
#pragma warning disable CS0618 // Type or member is obsolete
            ModelValidator = validator;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        ///     The type mapping source.
        /// </summary>
        public ITypeMappingSource TypeMappingSource { get; init; }

        /// <summary>
        ///     The parameter binding factories.
        /// </summary>
        public IParameterBindingFactories ParameterBindingFactories { get; init; }

        /// <summary>
        ///     The member classifier.
        /// </summary>
        [EntityFrameworkInternal]
        public IMemberClassifier MemberClassifier { get; init; }

        /// <summary>
        ///     The constructor binding factory.
        /// </summary>
        public IConstructorBindingFactory ConstructorBindingFactory { get; init; }

        /// <summary>
        ///     The model logger.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Model> Logger { get; init; }

        /// <summary>
        ///     The model validation logger.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Model.Validation> ValidationLogger { get; init; }

        /// <summary>
        ///     The set finder.
        /// </summary>
        public IDbSetFinder SetFinder { get; init; }

        /// <summary>
        ///     The current context instance.
        /// </summary>
        public Type ContextType
            => _currentContext.Context.GetType();

        /// <summary>
        ///     The model validator.
        /// </summary>
        [Obsolete("The validation is no longer performed by a convention")]
        public IModelValidator ModelValidator { get; init; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="currentContext"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ProviderConventionSetBuilderDependencies With(ICurrentDbContext currentContext)
#pragma warning disable CS0618 // Type or member is obsolete
            => new(
                TypeMappingSource,
                ConstructorBindingFactory,
                ParameterBindingFactories,
                MemberClassifier,
                Logger,
                ValidationLogger,
                SetFinder,
                currentContext,
                ModelValidator);
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
