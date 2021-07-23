// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
