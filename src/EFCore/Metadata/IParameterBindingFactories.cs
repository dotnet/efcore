// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Allows a <see cref="IParameterBindingFactory" /> to be found from those registered in the
    ///         internal service provider.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface IParameterBindingFactories
    {
        /// <summary>
        ///     Attempts to find a <see cref="IParameterBindingFactory" /> that can bind to a parameter with the
        ///     given type and name.
        /// </summary>
        /// <param name="parameterType"> The parameter type. </param>
        /// <param name="parameterName"> The parameter name. </param>
        /// <returns> The found factory, or null if none could be found. </returns>
        IParameterBindingFactory FindFactory([NotNull] Type parameterType, [NotNull] string parameterName);
    }
}
