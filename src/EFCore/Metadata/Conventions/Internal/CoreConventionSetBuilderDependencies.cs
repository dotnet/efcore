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
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="CoreConventionSetBuilder" />
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
    /// </summary>
    // Issue#11266 This type is being used by provider code. Do not break.
    public sealed class CoreConventionSetBuilderDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="CoreConventionSetBuilder" />.
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
        [Obsolete("Use the constructor with most parameters")]
        public CoreConventionSetBuilderDependencies([NotNull] ITypeMapper typeMapper)
            : this(
                new FallbackTypeMappingSource(
                    new TypeMappingSourceDependencies(
                        new ValueConverterSelector(
                            new ValueConverterSelectorDependencies()),
                        Enumerable.Empty<ITypeMappingSourcePlugin>()),
                    typeMapper),
                null,
                null,
                null,
                null)
        {
        }

        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="CoreConventionSetBuilder" />.
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
        public CoreConventionSetBuilderDependencies(
                [NotNull] ITypeMappingSource typeMappingSource,
                [CanBeNull] IConstructorBindingFactory constructorBindingFactory,
                [CanBeNull] IParameterBindingFactories parameterBindingFactories,
                [CanBeNull] IMemberClassifier memberClassifier,
                [CanBeNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger,
#pragma warning disable 618
                [CanBeNull] ITypeMapper _ = null) // Only needed for D.I. to resolve this constructor
#pragma warning restore 618
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
                     ?? new DiagnosticsLogger<DbLoggerCategory.Model>(new LoggerFactory(), new LoggingOptions(), new DiagnosticListener(""));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ITypeMappingSource TypeMappingSource { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IParameterBindingFactories ParameterBindingFactories { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IMemberClassifier MemberClassifier { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IConstructorBindingFactory ConstructorBindingFactory { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Model> Logger { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="typeMapper"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        [Obsolete("Use TypeMappingSource.")]
        public CoreConventionSetBuilderDependencies With([NotNull] ITypeMapper typeMapper)
            => new CoreConventionSetBuilderDependencies(
                new FallbackTypeMappingSource(
                    new TypeMappingSourceDependencies(
                        new ValueConverterSelector(
                            new ValueConverterSelectorDependencies()),
                        Enumerable.Empty<ITypeMappingSourcePlugin>()),
                    typeMapper),
                ConstructorBindingFactory,
                ParameterBindingFactories,
                MemberClassifier,
                Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="typeMappingSource"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public CoreConventionSetBuilderDependencies With([NotNull] ITypeMappingSource typeMappingSource)
            => new CoreConventionSetBuilderDependencies(
                typeMappingSource, ConstructorBindingFactory, ParameterBindingFactories, MemberClassifier, Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="constructorBindingFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public CoreConventionSetBuilderDependencies With([NotNull] IConstructorBindingFactory constructorBindingFactory)
            => new CoreConventionSetBuilderDependencies(
                TypeMappingSource, constructorBindingFactory, ParameterBindingFactories, MemberClassifier, Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="logger"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public CoreConventionSetBuilderDependencies With([NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
            => new CoreConventionSetBuilderDependencies(
                TypeMappingSource, ConstructorBindingFactory, ParameterBindingFactories, MemberClassifier, logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="parameterBindingFactories"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public CoreConventionSetBuilderDependencies With([NotNull] IParameterBindingFactories parameterBindingFactories)
            => new CoreConventionSetBuilderDependencies(
                TypeMappingSource, ConstructorBindingFactory, parameterBindingFactories, MemberClassifier, Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="memberClassifier"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public CoreConventionSetBuilderDependencies With([NotNull] IMemberClassifier memberClassifier)
            => new CoreConventionSetBuilderDependencies(
                TypeMappingSource, ConstructorBindingFactory, ParameterBindingFactories, memberClassifier, Logger);
    }
}
