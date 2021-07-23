// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when a key is added to the entity type.
    /// </summary>
    public interface IKeyAddedConvention : IConvention
    {
        /// <summary>
        ///     Called after a key is added to the entity type.
        /// </summary>
        /// <param name="keyBuilder"> The builder for the key. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessKeyAdded(IConventionKeyBuilder keyBuilder, IConventionContext<IConventionKeyBuilder> context);
    }
}
