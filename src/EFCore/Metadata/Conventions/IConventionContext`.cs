// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Contextual information associated with each convention call.
    /// </summary>
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
        /// <param name="result"> The new metadata object. </param>
        void StopProcessing([NotNull] TMetadata result);

        /// <summary>
        ///     <para>
        ///         Calling this will prevent further processing of the associated event by other conventions
        ///         if the given objects are different.
        ///     </para>
        ///     <para>
        ///         The common use case is when the metadata object was replaced by the convention.
        ///     </para>
        /// </summary>
        /// <param name="result"> The new metadata object. </param>
        void StopProcessingIfChanged([NotNull] TMetadata result);
    }
}
