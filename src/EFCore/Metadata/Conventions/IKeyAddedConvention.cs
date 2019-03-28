// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
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
        void ProcessKeyAdded([NotNull] IConventionKeyBuilder keyBuilder, [NotNull] IConventionContext<IConventionKeyBuilder> context);
    }
}
