// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
            QueryContextDependencies dependencies,
            RelationalQueryContextDependencies relationalDependencies)
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

        /// <summary>
        ///     The command logger to use while executing the query.
        /// </summary>
        public new virtual IRelationalCommandDiagnosticsLogger CommandLogger
            => (IRelationalCommandDiagnosticsLogger)base.CommandLogger;
    }
}
