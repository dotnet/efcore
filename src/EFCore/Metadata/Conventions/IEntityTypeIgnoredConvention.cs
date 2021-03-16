// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when an entity type is ignored.
    /// </summary>
    public interface IEntityTypeIgnoredConvention : IConvention
    {
        /// <summary>
        ///     Called after an entity type is ignored.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="name"> The name of the ignored entity type. </param>
        /// <param name="type"> The ignored entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessEntityTypeIgnored(
            IConventionModelBuilder modelBuilder,
            string name,
            Type? type,
            IConventionContext<string> context);
    }
}
