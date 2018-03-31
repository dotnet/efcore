// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     A registry of <see cref="ValueConverterInfo" /> that can be used to find
    ///     the preferred converter to use to convert to and from a given model type
    ///     to a type that the database provider supports.
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
