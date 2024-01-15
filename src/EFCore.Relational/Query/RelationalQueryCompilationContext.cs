// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         The primary data structure representing the state/components used during relational query compilation.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class RelationalQueryCompilationContext : QueryCompilationContext
{
    /// <summary>
    ///     Creates a new instance of the <see cref="RelationalQueryCompilationContext" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this class.</param>
    /// <param name="async">A bool value indicating whether it is for async query.</param>
    public RelationalQueryCompilationContext(
        QueryCompilationContextDependencies dependencies,
        RelationalQueryCompilationContextDependencies relationalDependencies,
        bool async)
        : base(dependencies, async)
    {
        RelationalDependencies = relationalDependencies;
        QuerySplittingBehavior = RelationalOptionsExtension.Extract(ContextOptions).QuerySplittingBehavior;
        SqlAliasManager = relationalDependencies.SqlAliasManagerFactory.Create();
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalQueryCompilationContextDependencies RelationalDependencies { get; }

    /// <summary>
    ///     A value indicating the <see cref="EntityFrameworkCore.QuerySplittingBehavior" /> configured for the query.
    ///     If no value has been configured then <see cref="Microsoft.EntityFrameworkCore.QuerySplittingBehavior.SingleQuery" />
    ///     will be used.
    /// </summary>
    public virtual QuerySplittingBehavior? QuerySplittingBehavior { get; internal set; }

    /// <summary>
    ///     A manager for SQL aliases, capable of generate uniquified table aliases.
    /// </summary>
    public virtual SqlAliasManager SqlAliasManager { get; }
}
