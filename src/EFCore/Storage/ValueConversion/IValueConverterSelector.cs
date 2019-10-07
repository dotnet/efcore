// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     <para>
    ///         A registry of <see cref="ValueConverterInfo" /> that can be used to find
    ///         the preferred converter to use to convert to and from a given model type
    ///         to a type that the database provider supports.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface IValueConverterSelector
    {
        /// <summary>
        ///     Returns the list of <see cref="ValueConverterInfo" /> instances that can be
        ///     used to convert the given model type. Converters nearer the front of
        ///     the list should be used in preference to converters nearer the end.
        /// </summary>
        /// <param name="modelClrType"> The type for which a converter is needed. </param>
        /// <param name="providerClrType"> The store type to target, or null for any. </param>
        /// <returns> The converters available. </returns>
        IEnumerable<ValueConverterInfo> Select(
            [NotNull] Type modelClrType,
            [CanBeNull] Type providerClrType = null);
    }
}
