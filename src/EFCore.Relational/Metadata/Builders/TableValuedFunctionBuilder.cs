// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     Provides a simple API for configuring a <see cref="IMutableDbFunction" />.
    /// </summary>
    public class TableValuedFunctionBuilder : DbFunctionBuilderBase
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public TableValuedFunctionBuilder([NotNull] IMutableDbFunction function)
            : base(function)
        {
        }

        /// <summary>
        ///     Sets the name of the database function.
        /// </summary>
        /// <param name="name"> The name of the function in the database. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual TableValuedFunctionBuilder HasName([NotNull] string name)
            => (TableValuedFunctionBuilder)base.HasName(name);

        /// <summary>
        ///     Sets the schema of the database function.
        /// </summary>
        /// <param name="schema"> The schema of the function in the database. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual TableValuedFunctionBuilder HasSchema([CanBeNull] string schema)
            => (TableValuedFunctionBuilder)base.HasSchema(schema);
    }
}
