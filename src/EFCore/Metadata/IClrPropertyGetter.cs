// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Represents operations backed by compiled delegates that support getting the value
    ///         of a mapped EF property.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    public interface IClrPropertyGetter
    {
        /// <summary>
        ///     Gets the property value.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <returns>The property value.</returns>
        object? GetClrValue(object entity);

        /// <summary>
        ///     Checks whether or not the property is set to the CLR default for its type.
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <returns><see langword="true" /> if the property value is the CLR default; <see langword="false" /> it is any other value.</returns>
        bool HasDefaultValue(object entity);
    }
}
