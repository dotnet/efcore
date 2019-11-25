// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     Converts identifiers to the plural and singular equivalents.
    /// </summary>
    public interface IPluralizer
    {
        /// <summary>
        ///     Gets the plural version of the given identifier. Returns the same
        ///     identifier if it is already pluralized.
        /// </summary>
        /// <param name="identifier"> The identifier to be pluralized. </param>
        /// <returns> The pluralized identifier. </returns>
        string Pluralize([CanBeNull] string identifier);

        /// <summary>
        ///     Gets the singular version of the given identifier. Returns the same
        ///     identifier if it is already singularized.
        /// </summary>
        /// <param name="identifier"> The identifier to be singularized. </param>
        /// <returns> The singularized identifier. </returns>
        string Singularize([CanBeNull] string identifier);
    }
}
