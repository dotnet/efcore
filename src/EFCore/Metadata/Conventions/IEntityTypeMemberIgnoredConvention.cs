// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when an entity type member is ignored.
    /// </summary>
    public interface IEntityTypeMemberIgnoredConvention : IConvention
    {
        /// <summary>
        ///     Called after an entity type member is ignored.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="name"> The name of the ignored member. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessEntityTypeMemberIgnored(
            IConventionEntityTypeBuilder entityTypeBuilder,
            string name,
            IConventionContext<string> context);
    }
}
