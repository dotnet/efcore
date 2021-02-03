// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="IReadOnlyModel" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
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
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public sealed record SingletonModelDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="ModelCustomizer" />.
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
        [EntityFrameworkInternal]
        public SingletonModelDependencies(
            [NotNull] ITypeMappingSource typeMappingSource,
            [NotNull] IConstructorBindingFactory constructorBindingFactory,
            [NotNull] IParameterBindingFactories parameterBindingFactories)
        {
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));
            Check.NotNull(constructorBindingFactory, nameof(constructorBindingFactory));
            Check.NotNull(parameterBindingFactories, nameof(parameterBindingFactories));

            TypeMappingSource = typeMappingSource;
            ConstructorBindingFactory = constructorBindingFactory;
            ParameterBindingFactories = parameterBindingFactories;
        }

        /// <summary>
        ///     The type mapper.
        /// </summary>
        public ITypeMappingSource TypeMappingSource { get; [param: NotNull] init; }

        /// <summary>
        ///     The constructor binding factory.
        /// </summary>
        public IConstructorBindingFactory ConstructorBindingFactory { get; [param: NotNull] init; }

        /// <summary>
        ///     The parameter binding factories.
        /// </summary>
        public IParameterBindingFactories ParameterBindingFactories { get; [param: NotNull] init; }
    }
}
