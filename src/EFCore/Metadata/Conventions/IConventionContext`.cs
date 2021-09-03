// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Contextual information associated with each convention call.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information.
    /// </remarks>
    /// <typeparam name="TMetadata"> The type of the metadata object. </typeparam>
    public interface IConventionContext<in TMetadata> : IConventionContext
    {
        /// <summary>
        ///     <para>
        ///         Calling this will prevent further processing of the associated event by other conventions.
        ///     </para>
        ///     <para>
        ///         The common use case is when the metadata object was replaced by the convention.
        ///     </para>
        /// </summary>
        /// <param name="result"> The new metadata object or <see langword="null" />. </param>
        void StopProcessing(TMetadata? result);

        /// <summary>
        ///     <para>
        ///         Calling this will prevent further processing of the associated event by other conventions
        ///         if the given objects are different.
        ///     </para>
        ///     <para>
        ///         The common use case is when the metadata object was replaced by the convention.
        ///     </para>
        /// </summary>
        /// <param name="result"> The new metadata object or <see langword="null" />. </param>
        void StopProcessingIfChanged(TMetadata? result);
    }
}
