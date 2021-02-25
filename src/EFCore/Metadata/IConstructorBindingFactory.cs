// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using CA = System.Diagnostics.CodeAnalysis;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         A factory for finding and creating <see cref="InstantiationBinding" /> instances for
    ///         a given CLR constructor.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface IConstructorBindingFactory
    {
        /// <summary>
        ///     Create a <see cref="InstantiationBinding" /> for the constructor with most parameters and
        ///     the constructor with only service property parameters.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="constructorBinding"> The binding for the constructor with most parameters. </param>
        /// <param name="serviceOnlyBinding"> The binding for the constructor with only service property parameters. </param>
        void GetBindings(
            [NotNull] IConventionEntityType entityType,
            [NotNull] out InstantiationBinding constructorBinding,
            [NotNull] out InstantiationBinding? serviceOnlyBinding);

        /// <summary>
        ///     Create a <see cref="InstantiationBinding" /> for the constructor with most parameters and
        ///     the constructor with only service property parameters.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="constructorBinding"> The binding for the constructor with most parameters. </param>
        /// <param name="serviceOnlyBinding"> The binding for the constructor with only service property parameters. </param>
        void GetBindings(
            [NotNull] IMutableEntityType entityType,
            [NotNull] out InstantiationBinding constructorBinding,
            [NotNull] out InstantiationBinding? serviceOnlyBinding);

        /// <summary>
        ///     Create a <see cref="InstantiationBinding" /> for the constructor with most parameters and
        ///     the constructor with only service property parameters.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="constructorBinding"> The binding for the constructor with most parameters. </param>
        /// <param name="serviceOnlyBinding"> The binding for the constructor with only service property parameters. </param>
        void GetBindings(
            [NotNull] IReadOnlyEntityType entityType,
            [NotNull] out InstantiationBinding constructorBinding,
            [NotNull] out InstantiationBinding? serviceOnlyBinding);

        /// <summary>
        ///     Attempts to create a <see cref="InstantiationBinding" /> for the given entity type and
        ///     <see cref="ConstructorInfo" />
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="constructor"> The constructor to use. </param>
        /// <param name="binding"> The binding, or <see langword="null" /> if <see langword="null" /> could be created. </param>
        /// <param name="unboundParameters"> The parameters that could not be bound. </param>
        /// <returns> <see langword="true" /> if a binding was created; <see langword="false" /> otherwise. </returns>
        bool TryBindConstructor(
            [NotNull] IConventionEntityType entityType,
            [NotNull] ConstructorInfo constructor,
            [CanBeNull, CA.NotNullWhen(true)] out InstantiationBinding? binding,
            [CanBeNull, CA.NotNullWhen(false)] out IEnumerable<ParameterInfo>? unboundParameters);

        /// <summary>
        ///     Attempts to create a <see cref="InstantiationBinding" /> for the given entity type and
        ///     <see cref="ConstructorInfo" />
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="constructor"> The constructor to use. </param>
        /// <param name="binding"> The binding, or <see langword="null" /> if <see langword="null" /> could be created. </param>
        /// <param name="unboundParameters"> The parameters that could not be bound. </param>
        /// <returns> <see langword="true" /> if a binding was created; <see langword="false" /> otherwise. </returns>
        bool TryBindConstructor(
            [NotNull] IMutableEntityType entityType,
            [NotNull] ConstructorInfo constructor,
            [CanBeNull, CA.NotNullWhen(true)] out InstantiationBinding? binding,
            [CanBeNull, CA.NotNullWhen(false)] out IEnumerable<ParameterInfo>? unboundParameters);
    }
}
