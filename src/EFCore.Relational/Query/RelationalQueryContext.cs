// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         The principal data structure used by a compiled relational query during execution.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class RelationalQueryContext : QueryContext
    {
        /// <summary>
        ///     <para>
        ///         Creates a new <see cref="RelationalQueryContext" /> instance.
        ///     </para>
        ///     <para>
        ///         This type is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this class. </param>
        /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this class. </param>
        public RelationalQueryContext(
            [NotNull] QueryContextDependencies dependencies,
            [NotNull] RelationalQueryContextDependencies relationalDependencies)
            : base(dependencies)
        {
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            RelationalDependencies = relationalDependencies;
        }

        /// <summary>
        ///     Parameter object containing relational service dependencies.
        /// </summary>
        protected virtual RelationalQueryContextDependencies RelationalDependencies { get; }

        /// <summary>
        ///     A factory for creating a readable query string from a <see cref="DbCommand" />
        /// </summary>
        public virtual IRelationalQueryStringFactory RelationalQueryStringFactory
            => RelationalDependencies.RelationalQueryStringFactory;

        /// <summary>
        ///     Gets the active relational connection.
        /// </summary>
        /// <value>
        ///     The connection.
        /// </value>
        public virtual IRelationalConnection Connection
            => RelationalDependencies.RelationalConnection;
    }
}
