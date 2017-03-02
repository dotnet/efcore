// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         This interface is explicitly implemented by type to hide properties that are not intended to be used in application code
    ///         but can be used in extension methods written by database providers etc.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <typeparam name="T"> The type of the property being hidden. </typeparam>
    public interface IInfrastructure<out T>
    {
        /// <summary>
        ///     Gets the value of the property being hidden.
        /// </summary>
        T Instance { get; }
    }
}
