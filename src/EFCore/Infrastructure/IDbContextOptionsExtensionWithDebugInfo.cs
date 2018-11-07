// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Extended interface for extensions that also allows for debugging info to be gathered. This
    ///         interface will likely be merged into <see cref="IDbContextOptionsExtension" /> in EF Core 3.0.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IDbContextOptionsExtensionWithDebugInfo : IDbContextOptionsExtension
    {
        /// <summary>
        ///     Populates a dictionary of information that may change between uses of the
        ///     extension such that it can be compared to a previous configuration for
        ///     this option and differences can be logged. The dictionary key should be prefixed by the
        ///     extension name. For example, <c>"SqlServer:"</c>.
        /// </summary>
        /// <param name="debugInfo"> The dictionary to populate. </param>
        void PopulateDebugInfo([NotNull] IDictionary<string, string> debugInfo);
    }
}
