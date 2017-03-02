// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Creates instances of <see cref="ParameterNameGenerator" />.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IParameterNameGeneratorFactory
    {
        /// <summary>
        ///     Gets a new <see cref="ParameterNameGenerator" />.
        /// </summary>
        /// <returns> The newly created <see cref="ParameterNameGenerator" />. </returns>
        ParameterNameGenerator Create();
    }
}
