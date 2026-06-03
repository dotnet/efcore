// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         The principal data structure used by a compiled query during execution.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     and <see href="https://aka.ms/efcore-docs-how-query-works">How EF Core queries work</see> for more information and examples.
/// </remarks>
public abstract class QueryContext : IParameterValues
{
    private readonly IDictionary<string, object?> _parameterValues = new Dictionary<string, object?>();
    private IStateManager? _stateManager;

    /// <summary>
    ///     <para>
    ///         Creates a new <see cref="QueryContext" /> instance.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="dependencies">The dependencies to use.</param>
    protected QueryContext(QueryContextDependencies dependencies)
    {
        Dependencies = dependencies;
        Context = dependencies.CurrentContext.Context;
    }

    /// <summary>
    ///     The current DbContext in using while executing the query.
    /// </summary>
    public virtual DbContext Context { get; }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual QueryContextDependencies Dependencies { get; }

    /// <summary>
    ///     Sets the navigation for given entity as loaded.
    /// </summary>
    /// <param name="entity">The entity instance.</param>
    /// <param name="navigation">The navigation property.</param>
    public virtual void SetNavigationIsLoaded(object entity, INavigationBase navigation)
        // InitializeStateManager will populate the field before calling here
        => _stateManager!.TryGetEntry(entity)!.SetIsLoaded(navigation);

    /// <summary>
    ///     The execution strategy to use while executing the query.
    /// </summary>
    public virtual IExecutionStrategy ExecutionStrategy
        => Dependencies.ExecutionStrategy;

    /// <summary>
    ///     The concurrency detector to use while executing the query.
    /// </summary>
    public virtual IConcurrencyDetector ConcurrencyDetector
        => Dependencies.ConcurrencyDetector;

    /// <summary>
    ///     The exception detector to use while executing the query.
    /// </summary>
    public virtual IExceptionDetector ExceptionDetector
        => Dependencies.ExceptionDetector;

    /// <summary>
    ///     The cancellation token to use while executing the query.
    /// </summary>
    public virtual CancellationToken CancellationToken { get; set; }

    /// <summary>
    ///     The command logger to use while executing the query.
    /// </summary>
    public virtual IDiagnosticsLogger<DbLoggerCategory.Database.Command> CommandLogger
        => Dependencies.CommandLogger;

    /// <summary>
    ///     The query logger to use while executing the query.
    /// </summary>
    public virtual IDiagnosticsLogger<DbLoggerCategory.Query> QueryLogger
        => Dependencies.QueryLogger;

    /// <summary>
    ///     The parameter values to use while executing the query.
    /// </summary>
    public virtual IReadOnlyDictionary<string, object?> ParameterValues
        => (IReadOnlyDictionary<string, object?>)_parameterValues;

    /// <summary>
    ///     Adds a parameter to <see cref="ParameterValues" /> for this query.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    public virtual void AddParameter(string name, object? value)
        => _parameterValues.Add(name, value);

    /// <summary>
    ///     Initializes the <see cref="IStateManager" /> to be used with this QueryContext.
    /// </summary>
    /// <param name="standAlone">Whether a stand-alone <see cref="IStateManager" /> should be created to perform identity resolution.</param>
    public virtual void InitializeStateManager(bool standAlone = false)
        => _stateManager ??= standAlone
            ? new StateManager(Dependencies.StateManager.Dependencies)
            : Dependencies.StateManager;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual InternalEntityEntry? TryGetEntry(
            IKey key,
            object[] keyValues,
            bool throwOnNullKey,
            out bool hasNullKey)
        // InitializeStateManager will populate the field before calling here
        => _stateManager!.TryGetEntry(key, keyValues, throwOnNullKey, out hasNullKey);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual InternalEntityEntry StartTracking(
            IEntityType entityType,
            object entity,
            in ISnapshot snapshot)
        // InitializeStateManager will populate the field before calling here
        => _stateManager!.StartTrackingFromQuery(entityType, entity, snapshot);
}
