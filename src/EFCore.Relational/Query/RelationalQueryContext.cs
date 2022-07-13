// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

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
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this class.</param>
    public RelationalQueryContext(
        QueryContextDependencies dependencies,
        RelationalQueryContextDependencies relationalDependencies)
        : base(dependencies)
    {
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
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
