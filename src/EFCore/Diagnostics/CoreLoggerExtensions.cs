// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     <para>
///         This class contains static methods used by EF Core internals and database providers to
///         write information to an <see cref="ILogger" /> and a <see cref="DiagnosticListener" /> for
///         well-known events.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public static class CoreLoggerExtensions
{
    /// <summary>
    ///     Logs for the <see cref="CoreEventId.SaveChangesFailed" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="context">The context in use.</param>
    /// <param name="exception">The exception that caused this event.</param>
    public static void SaveChangesFailed(
        this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
        DbContext context,
        Exception exception)
    {
        var definition = CoreResources.LogExceptionDuringSaveChanges(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                context.GetType(), Environment.NewLine, exception,
                exception);
        }

        if (diagnostics.NeedsEventData<ISaveChangesInterceptor>(
                definition,
                out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = CreateDbContextErrorEventData(context, exception, definition);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            interceptor?.SaveChangesFailed(eventData);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.SaveChangesFailed" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="context">The context in use.</param>
    /// <param name="exception">The exception that caused this event.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> for the async result.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task SaveChangesFailedAsync(
        this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
        DbContext context,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        var definition = CoreResources.LogExceptionDuringSaveChanges(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                context.GetType(), Environment.NewLine, exception,
                exception);
        }

        if (diagnostics.NeedsEventData<ISaveChangesInterceptor>(
                definition,
                out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = CreateDbContextErrorEventData(context, exception, definition);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.SaveChangesFailedAsync(eventData, cancellationToken);
            }
        }

        return Task.CompletedTask;
    }

    private static DbContextErrorEventData CreateDbContextErrorEventData(
        DbContext context,
        Exception exception,
        EventDefinition<Type?, string, Exception> definition)
        => new(definition, SaveChangesFailed, context, exception);

    private static string SaveChangesFailed(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<Type?, string, Exception>)definition;
        var p = (DbContextErrorEventData)payload;
        return d.GenerateMessage(p.Context?.GetType(), Environment.NewLine, p.Exception);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.SaveChangesFailed" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="context">The context in use.</param>
    public static void SaveChangesCanceled(
        this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
        DbContext context)
    {
        var definition = CoreResources.LogSaveChangesCanceled(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, context.GetType());
        }

        if (diagnostics.NeedsEventData<ISaveChangesInterceptor>(
                definition,
                out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new DbContextEventData(definition, SaveChangesCanceled, context);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            interceptor?.SaveChangesCanceled(eventData);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.SaveChangesFailed" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="context">The context in use.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> for the async result.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task SaveChangesCanceledAsync(
        this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
        DbContext context,
        CancellationToken cancellationToken = default)
    {
        var definition = CoreResources.LogSaveChangesCanceled(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, context.GetType());
        }

        if (diagnostics.NeedsEventData<ISaveChangesInterceptor>(
                definition,
                out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new DbContextEventData(definition, SaveChangesCanceled, context);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.SaveChangesCanceledAsync(eventData, cancellationToken);
            }
        }

        return Task.CompletedTask;
    }

    private static string SaveChangesCanceled(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<Type?>)definition;
        var p = (DbContextEventData)payload;
        return d.GenerateMessage(p.Context?.GetType());
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.OldModelVersionWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="context">The context being used.</param>
    /// <param name="contextOptions">The context options being used.</param>
    public static void OldModelVersionWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
        DbContext context,
        DbContextOptions contextOptions)
    {
        var definition = CoreResources.LogOldModelVersion(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            var modelVersion = contextOptions.FindExtension<CoreOptionsExtension>()?.Model?.GetProductVersion() ?? "";

            definition.Log(
                diagnostics,
                modelVersion,
                ProductInfo.GetVersion());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new ContextInitializedEventData(
                definition,
                OldModelVersion,
                context,
                contextOptions);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string OldModelVersion(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (ContextInitializedEventData)payload;
        var modelVersion = p.ContextOptions.FindExtension<CoreOptionsExtension>()?.Model?.GetProductVersion() ?? "";
        return d.GenerateMessage(
            modelVersion,
            ProductInfo.GetVersion());
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.OptimisticConcurrencyException" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="context">The context in use.</param>
    /// <param name="entries">The entries that were involved in the concurrency violation.</param>
    /// <param name="exception">The exception that caused this event.</param>
    /// <param name="createEventData">Optional delegate to override event data creation.</param>
    public static InterceptionResult OptimisticConcurrencyException(
        this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
        DbContext context,
        IReadOnlyList<IUpdateEntry> entries,
        DbUpdateConcurrencyException exception,
        Func<DbContext, DbUpdateConcurrencyException, IReadOnlyList<IUpdateEntry>, EventDefinition<Exception>,
            ConcurrencyExceptionEventData>? createEventData)
    {
        var definition = CoreResources.LogOptimisticConcurrencyException(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, exception);
        }

        if (diagnostics.NeedsEventData<ISaveChangesInterceptor>(
                definition,
                out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = createEventData?.Invoke(context, exception, entries, definition)
                ?? CreateConcurrencyExceptionEventData(context, exception, entries, definition);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.ThrowingConcurrencyException(eventData, default);
            }
        }

        return default;
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.OptimisticConcurrencyException" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="context">The context in use.</param>
    /// <param name="entries">The entries that were involved in the concurrency violation.</param>
    /// <param name="exception">The exception that caused this event.</param>
    /// <param name="createEventData">Optional delegate to override event data creation.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> for the async result.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static ValueTask<InterceptionResult> OptimisticConcurrencyExceptionAsync(
        this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
        DbContext context,
        IReadOnlyList<IUpdateEntry> entries,
        DbUpdateConcurrencyException exception,
        Func<DbContext, DbUpdateConcurrencyException, IReadOnlyList<IUpdateEntry>, EventDefinition<Exception>,
            ConcurrencyExceptionEventData>? createEventData,
        CancellationToken cancellationToken = default)
    {
        var definition = CoreResources.LogOptimisticConcurrencyException(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, exception);
        }

        if (diagnostics.NeedsEventData<ISaveChangesInterceptor>(
                definition,
                out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = createEventData?.Invoke(context, exception, entries, definition)
                ?? CreateConcurrencyExceptionEventData(context, exception, entries, definition);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.ThrowingConcurrencyExceptionAsync(eventData, default, cancellationToken);
            }
        }

        return default;
    }

    private static ConcurrencyExceptionEventData CreateConcurrencyExceptionEventData(
        DbContext context,
        DbUpdateConcurrencyException exception,
        IReadOnlyList<IUpdateEntry> entries,
        EventDefinition<Exception> definition)
        => new(
            definition,
            OptimisticConcurrencyException,
            context,
            entries,
            exception);

    private static string OptimisticConcurrencyException(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<Exception>)definition;
        var p = (ConcurrencyExceptionEventData)payload;
        return d.GenerateMessage(p.Exception);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.DuplicateDependentEntityTypeInstanceWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="dependent1">The first dependent type.</param>
    /// <param name="dependent2">The second dependent type.</param>
    public static void DuplicateDependentEntityTypeInstanceWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
        IEntityType dependent1,
        IEntityType dependent2)
    {
        var definition = CoreResources.LogDuplicateDependentEntityTypeInstance(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, dependent1.DisplayName(), dependent2.DisplayName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new SharedDependentEntityEventData(
                definition,
                DuplicateDependentEntityTypeInstanceWarning,
                dependent1,
                dependent2);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string DuplicateDependentEntityTypeInstanceWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (SharedDependentEntityEventData)payload;
        return d.GenerateMessage(p.FirstEntityType.DisplayName(), p.SecondEntityType.DisplayName());
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.QueryIterationFailed" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="contextType">The <see cref="DbContext" /> type being used.</param>
    /// <param name="exception">The exception that caused this failure.</param>
    public static void QueryIterationFailed(
        this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
        Type contextType,
        Exception exception)
    {
        var definition = CoreResources.LogExceptionDuringQueryIteration(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                contextType, Environment.NewLine, exception,
                exception);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new DbContextTypeErrorEventData(
                definition,
                QueryIterationFailed,
                contextType,
                exception);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string QueryIterationFailed(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<Type, string, Exception>)definition;
        var p = (DbContextTypeErrorEventData)payload;
        return d.GenerateMessage(p.ContextType, Environment.NewLine, p.Exception);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.QueryCanceled" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="contextType">The <see cref="DbContext" /> type being used.</param>
    public static void QueryCanceled(this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics, Type contextType)
    {
        var definition = CoreResources.LogQueryCanceled(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, contextType);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new DbContextTypeEventData(
                definition,
                QueryCanceled,
                contextType);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string QueryCanceled(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<Type>)definition;
        var p = (DbContextTypeEventData)payload;
        return d.GenerateMessage(p.ContextType);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.QueryCompilationStarting" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="context">The current <see cref="DbContext" />, or <see langword="null" /> if not known.</param>
    /// <param name="expressionPrinter">Used to create a human-readable representation of the expression tree.</param>
    /// <param name="queryExpression">The query expression tree.</param>
    /// <returns>The query expression and event data.</returns>
    public static (Expression Query, QueryExpressionEventData? EventData) QueryCompilationStarting(
        this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
        DbContext? context,
        ExpressionPrinter expressionPrinter,
        Expression queryExpression)
    {
        var definition = CoreResources.LogQueryCompilationStarting(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, Environment.NewLine, expressionPrinter.PrintExpression(queryExpression));
        }

        if (diagnostics.NeedsEventData<IQueryExpressionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new QueryExpressionEventData(
                definition,
                QueryCompilationStarting,
                context,
                queryExpression,
                expressionPrinter);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            if (interceptor != null)
            {
                return (interceptor.QueryCompilationStarting(queryExpression, eventData), eventData);
            }
        }

        return (queryExpression, null);
    }

    private static string QueryCompilationStarting(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (QueryExpressionEventData)payload;
        return d.GenerateMessage(Environment.NewLine, p.ExpressionPrinter.PrintExpression(p.Expression));
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.FirstWithoutOrderByAndFilterWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    public static void FirstWithoutOrderByAndFilterWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics)
    {
        var definition = CoreResources.LogFirstWithoutOrderByAndFilter(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new EventData(
                definition,
                FirstWithoutOrderByAndFilterWarning);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string FirstWithoutOrderByAndFilterWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition)definition;
        return d.GenerateMessage();
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.RowLimitingOperationWithoutOrderByWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    public static void RowLimitingOperationWithoutOrderByWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics)
    {
        var definition = CoreResources.LogRowLimitingOperationWithoutOrderBy(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new EventData(
                definition,
                RowLimitingOperationWithoutOrderByWarning);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string RowLimitingOperationWithoutOrderByWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition)definition;
        return d.GenerateMessage();
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.DistinctAfterOrderByWithoutRowLimitingOperatorWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    public static void DistinctAfterOrderByWithoutRowLimitingOperatorWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics)
    {
        var definition = CoreResources.LogDistinctAfterOrderByWithoutRowLimitingOperatorWarning(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new EventData(
                definition,
                DistinctAfterOrderByWithoutRowLimitingOperatorWarning);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string DistinctAfterOrderByWithoutRowLimitingOperatorWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition)definition;
        return d.GenerateMessage();
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.NavigationBaseIncluded" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="navigation">The navigation being included.</param>
    public static void NavigationBaseIncluded(
        this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
        INavigationBase navigation)
    {
        var definition = CoreResources.LogNavigationBaseIncluded(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, navigation.DeclaringEntityType.ShortName() + "." + navigation.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new NavigationBaseEventData(
                definition,
                NavigationBaseIncluded,
                navigation);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string NavigationBaseIncluded(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string>)definition;
        var p = (NavigationBaseEventData)payload;
        return d.GenerateMessage(p.NavigationBase.DeclaringEntityType.ShortName() + "." + p.NavigationBase.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.NavigationBaseIncludeIgnored" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="navigation">The navigation being included.</param>
    public static void NavigationBaseIncludeIgnored(
        this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
        INavigationBase navigation)
    {
        var definition = CoreResources.LogNavigationBaseIncludeIgnored(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, navigation.DeclaringEntityType.ShortName() + "." + navigation.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new NavigationBaseEventData(
                definition,
                NavigationBaseIncludeIgnored,
                navigation);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string NavigationBaseIncludeIgnored(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string>)definition;
        var p = (NavigationBaseEventData)payload;
        return d.GenerateMessage(p.NavigationBase.DeclaringEntityType.ShortName() + "." + p.NavigationBase.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.QueryExecutionPlanned" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="context">The current <see cref="DbContext" />, or <see langword="null" /> if not known.</param>
    /// <param name="expressionPrinter">Used to create a human-readable representation of the expression tree.</param>
    /// <param name="queryExecutorExpression">The query expression tree.</param>
    public static void QueryExecutionPlanned(
        this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
        DbContext? context,
        ExpressionPrinter expressionPrinter,
        Expression queryExecutorExpression)
    {
        var definition = CoreResources.LogQueryExecutionPlanned(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, Environment.NewLine, expressionPrinter.PrintExpression(queryExecutorExpression));
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new QueryExpressionEventData(
                definition,
                QueryExecutionPlanned,
                context,
                queryExecutorExpression,
                expressionPrinter);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string QueryExecutionPlanned(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (QueryExpressionEventData)payload;
        return d.GenerateMessage(Environment.NewLine, p.ExpressionPrinter.PrintExpression(p.Expression));
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.SensitiveDataLoggingEnabledWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <typeparam name="TLoggerCategory">The logger category for which to log the warning.</typeparam>
    public static void SensitiveDataLoggingEnabledWarning<TLoggerCategory>(
        this IDiagnosticsLogger<TLoggerCategory> diagnostics)
        where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
    {
        var definition = CoreResources.LogSensitiveDataLoggingEnabled(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new EventData(
                definition,
                (d, _) => ((EventDefinition)d).GenerateMessage());

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.TypeLoadingErrorWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="assembly">The assembly that types are being loaded from.</param>
    /// <param name="exception">The exception from the loading error.</param>
    public static void TypeLoadingErrorWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
        Assembly assembly,
        Exception exception)
    {
        var definition = CoreResources.LogTypeLoadingErrorWarning(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, assembly.ToString(), exception.Message);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new TypeLoadingEventData(
                definition,
                TypeLoadingErrorWarning,
                assembly,
                exception);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string TypeLoadingErrorWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (TypeLoadingEventData)payload;
        return d.GenerateMessage(p.Assembly.ToString(), p.Exception.Message);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.SkippedEntityTypeConfigurationWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="type">The <see cref="IEntityTypeConfiguration{TEntity}"/> type.</param>
    public static void SkippedEntityTypeConfigurationWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
        Type type)
    {
        var definition = CoreResources.LogSkippedEntityTypeConfigurationWarning(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, type.DisplayName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new TypeEventData(definition, SkippedEntityTypeConfigurationWarning, type);
            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string SkippedEntityTypeConfigurationWarning(EventDefinitionBase definition, EventData payload)
        => ((EventDefinition<string>)definition).GenerateMessage(((TypeEventData)payload).ClrType.DisplayName());

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.NoEntityTypeConfigurationsWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="assembly">The assembly from which types are being loaded.</param>
    public static void NoEntityTypeConfigurationsWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
        Assembly assembly)
    {
        var definition = CoreResources.LogNoEntityTypeConfigurationsWarning(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, assembly.ToString());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new AssemblyEventData(definition, NoEntityTypeConfigurationsWarning, assembly);
            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string NoEntityTypeConfigurationsWarning(EventDefinitionBase definition, EventData payload)
        => ((EventDefinition<string>)definition).GenerateMessage(((AssemblyEventData)payload).Assembly.ToString());

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.PossibleUnintendedCollectionNavigationNullComparisonWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="navigation">The navigation being used.</param>
    public static void PossibleUnintendedCollectionNavigationNullComparisonWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
        INavigation navigation)
    {
        var definition = CoreResources.LogPossibleUnintendedCollectionNavigationNullComparison(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                $"{navigation.DeclaringEntityType.DisplayName()}.{navigation.Name}");
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new NavigationEventData(
                definition,
                PossibleUnintendedCollectionNavigationNullComparisonWarning,
                navigation);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string PossibleUnintendedCollectionNavigationNullComparisonWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string>)definition;
        var p = (NavigationEventData)payload;
        return d.GenerateMessage($"{p.Navigation.DeclaringEntityType.DisplayName()}.{p.Navigation.Name}");
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.PossibleUnintendedReferenceComparisonWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="left">The left side expression.</param>
    /// <param name="right">The right side expression.</param>
    public static void PossibleUnintendedReferenceComparisonWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
        Expression left,
        Expression right)
    {
        var definition = CoreResources.LogPossibleUnintendedReferenceComparison(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, left, right);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new BinaryExpressionEventData(
                definition,
                PossibleUnintendedReferenceComparisonWarning,
                left,
                right);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string PossibleUnintendedReferenceComparisonWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<object, object>)definition;
        var p = (BinaryExpressionEventData)payload;
        return d.GenerateMessage(p.Left, p.Right);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.InvalidIncludePathError" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="navigationChain">
    ///     The navigation chain used in the
    ///     <see cref="EntityFrameworkQueryableExtensions.Include{TEntity}(IQueryable{TEntity}, string)" />
    /// </param>
    /// <param name="navigationName">The navigation name which was not found in the model.</param>
    public static void InvalidIncludePathError(
        this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
        string navigationChain,
        string navigationName)
    {
        var definition = CoreResources.LogInvalidIncludePath(diagnostics);
        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, navigationName, navigationChain);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new InvalidIncludePathEventData(
                definition,
                InvalidIncludePathError,
                navigationChain,
                navigationName);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string InvalidIncludePathError(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<object, object>)definition;
        var p = (InvalidIncludePathEventData)payload;

        return d.GenerateMessage(p.NavigationName, p.NavigationChain);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.ServiceProviderCreated" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public static void ServiceProviderCreated(
        this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
        IServiceProvider serviceProvider)
    {
        var definition = CoreResources.LogServiceProviderCreated(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new ServiceProviderEventData(
                definition,
                (d, _) => ((EventDefinition)d).GenerateMessage(),
                serviceProvider);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.ManyServiceProvidersCreatedWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="serviceProviders">The service providers that have been created.</param>
    public static void ManyServiceProvidersCreatedWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
        ICollection<IServiceProvider> serviceProviders)
    {
        var definition = CoreResources.LogManyServiceProvidersCreated(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new ServiceProvidersEventData(
                definition,
                (d, _) => ((EventDefinition)d).GenerateMessage(),
                serviceProviders);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.ServiceProviderDebugInfo" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="newDebugInfo">Debug information for the new service providers.</param>
    /// <param name="cachedDebugInfos">Debug information for existing service providers.</param>
    public static void ServiceProviderDebugInfo(
        this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
        IDictionary<string, string> newDebugInfo,
        IList<IDictionary<string, string>> cachedDebugInfos)
    {
        var definition = CoreResources.LogServiceProviderDebugInfo(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, GenerateDebugInfoString(newDebugInfo, cachedDebugInfos));
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new ServiceProviderDebugInfoEventData(
                definition,
                (d, p) => ServiceProviderDebugInfo(d, p),
                newDebugInfo,
                cachedDebugInfos);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ServiceProviderDebugInfo(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string>)definition;
        var p = (ServiceProviderDebugInfoEventData)payload;
        return d.GenerateMessage(
            GenerateDebugInfoString(p.NewDebugInfo, p.CachedDebugInfos));
    }

    private static string GenerateDebugInfoString(
        IDictionary<string, string> newDebugInfo,
        IList<IDictionary<string, string>> cachedDebugInfos)
    {
        List<string>? leastConflicts = null;

        foreach (var cachedDebugInfo in cachedDebugInfos)
        {
            var newKeys = new HashSet<string>(newDebugInfo.Keys);

            var conflicts = new List<string>();
            foreach (var key in cachedDebugInfo.Keys)
            {
                if (newDebugInfo.TryGetValue(key, out var value))
                {
                    if (value != cachedDebugInfo[key])
                    {
                        conflicts.Add(CoreStrings.ServiceProviderConfigChanged(key));
                    }
                }
                else
                {
                    conflicts.Add(CoreStrings.ServiceProviderConfigRemoved(key));
                }

                newKeys.Remove(key);
            }

            foreach (var addedKey in newKeys)
            {
                conflicts.Add(CoreStrings.ServiceProviderConfigAdded(addedKey));
            }

            if (leastConflicts == null
                || leastConflicts.Count > conflicts.Count)
            {
                leastConflicts = conflicts;
            }
        }

        return leastConflicts is null ? string.Empty : string.Join(", ", leastConflicts);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.ContextInitialized" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="context">The context being used.</param>
    /// <param name="contextOptions">The context options being used.</param>
    public static void ContextInitialized(
        this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
        DbContext context,
        DbContextOptions contextOptions)
    {
        var definition = CoreResources.LogContextInitialized(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                ProductInfo.GetVersion(),
                context.GetType().ShortDisplayName(),
                context.Database.ProviderName,
                GetProviderVersion(context),
                contextOptions.BuildOptionsFragment());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new ContextInitializedEventData(
                definition,
                ContextInitialized,
                context,
                contextOptions);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ContextInitialized(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string?, string?, string>)definition;
        var p = (ContextInitializedEventData)payload;
        return d.GenerateMessage(
            ProductInfo.GetVersion(),
            p.Context.GetType().ShortDisplayName(),
            p.Context.Database.ProviderName,
            GetProviderVersion(p.Context),
            p.ContextOptions.BuildOptionsFragment());
    }

    private static string? GetProviderVersion(DbContext context)
        => context.GetService<IEnumerable<IDatabaseProvider>>().FirstOrDefault()?.Version;

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.ExecutionStrategyRetrying" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="exceptionsEncountered">The exceptions(s) that caused the failure.</param>
    /// <param name="delay">The delay that before the next retry.</param>
    /// <param name="async">Indicates whether execution is asynchronous or not.</param>
    public static void ExecutionStrategyRetrying(
        this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
        IReadOnlyList<Exception> exceptionsEncountered,
        TimeSpan delay,
        bool async)
    {
        var definition = CoreResources.LogExecutionStrategyRetrying(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            var lastException = exceptionsEncountered[^1];
            definition.Log(
                diagnostics,
                (int)delay.TotalMilliseconds, Environment.NewLine, lastException,
                lastException);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new ExecutionStrategyEventData(
                definition,
                ExecutionStrategyRetrying,
                exceptionsEncountered,
                delay,
                async);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ExecutionStrategyRetrying(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<int, string, Exception>)definition;
        var p = (ExecutionStrategyEventData)payload;
        return d.GenerateMessage(
            (int)p.Delay.TotalMilliseconds, Environment.NewLine, p.ExceptionsEncountered[^1]);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.LazyLoadOnDisposedContextWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="context">The context being used.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="navigationName">The name of the navigation property.</param>
    public static void LazyLoadOnDisposedContextWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
        DbContext? context,
        object entityType,
        string navigationName)
    {
        var definition = CoreResources.LogLazyLoadOnDisposedContext(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, entityType.GetType().ShortDisplayName(), navigationName);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new LazyLoadingEventData(
                definition,
                LazyLoadOnDisposedContextWarning,
                context,
                entityType,
                navigationName);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string LazyLoadOnDisposedContextWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (LazyLoadingEventData)payload;
        return d.GenerateMessage(p.Entity.GetType().ShortDisplayName(), p.NavigationPropertyName);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.NavigationLazyLoading" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="context">The context being used.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="navigationName">The name of the navigation property.</param>
    public static void NavigationLazyLoading(
        this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
        DbContext context,
        object entityType,
        string navigationName)
    {
        var definition = CoreResources.LogNavigationLazyLoading(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, entityType.GetType().ShortDisplayName(), navigationName);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new LazyLoadingEventData(
                definition,
                NavigationLazyLoading,
                context,
                entityType,
                navigationName);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string NavigationLazyLoading(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (LazyLoadingEventData)payload;
        return d.GenerateMessage(p.Entity.GetType().ShortDisplayName(), p.NavigationPropertyName);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.DetachedLazyLoadingWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="context">The context being used.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="navigationName">The name of the navigation property.</param>
    public static void DetachedLazyLoadingWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
        DbContext? context,
        object entityType,
        string navigationName)
    {
        var definition = CoreResources.LogDetachedLazyLoading(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, navigationName, entityType.GetType().ShortDisplayName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new LazyLoadingEventData(
                definition,
                DetachedLazyLoadingWarning,
                context,
                entityType,
                navigationName);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string DetachedLazyLoadingWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (LazyLoadingEventData)payload;
        return d.GenerateMessage(p.NavigationPropertyName, p.Entity.GetType().ShortDisplayName());
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.RedundantAddServicesCallWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="serviceProvider">The service provider used.</param>
    public static void RedundantAddServicesCallWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
        IServiceProvider serviceProvider)
    {
        var definition = CoreResources.LogRedundantAddServicesCall(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new ServiceProviderEventData(
                definition,
                (d, _) => ((EventDefinition)d).GenerateMessage(),
                serviceProvider);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.ShadowForeignKeyPropertyCreated" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="property">The property.</param>
    /// <param name="basePropertyName">The property name that was uniquified.</param>
    public static void ShadowForeignKeyPropertyCreated(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        IProperty property,
        string basePropertyName)
    {
        var definition = CoreResources.LogShadowForeignKeyPropertyCreated(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, property.DeclaringType.DisplayName(), property.Name, basePropertyName);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new UniquifiedPropertyEventData(
                definition,
                ShadowForeignKeyPropertyCreated,
                property,
                basePropertyName);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ShadowForeignKeyPropertyCreated(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string>)definition;
        var p = (UniquifiedPropertyEventData)payload;
        return d.GenerateMessage(p.Property.DeclaringType.DisplayName(), p.Property.Name, p.BasePropertyName);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.ShadowPropertyCreated" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="property">The property.</param>
    public static void ShadowPropertyCreated(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        IProperty property)
    {
        var definition = CoreResources.LogShadowPropertyCreated(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, property.DeclaringType.DisplayName(), property.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new PropertyEventData(
                definition,
                ShadowPropertyCreated,
                property);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ShadowPropertyCreated(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (PropertyEventData)payload;
        return d.GenerateMessage(p.Property.DeclaringType.DisplayName(), p.Property.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.CollectionWithoutComparer" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="property">The property.</param>
    public static void CollectionWithoutComparer(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        IProperty property)
    {
        var definition = CoreResources.LogCollectionWithoutComparer(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, property.DeclaringType.DisplayName(), property.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new PropertyEventData(
                definition,
                CollectionWithoutComparer,
                property);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string CollectionWithoutComparer(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (PropertyEventData)payload;
        return d.GenerateMessage(p.Property.DeclaringType.DisplayName(), p.Property.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.RedundantIndexRemoved" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="redundantIndex">The redundant index.</param>
    /// <param name="otherIndex">The other index.</param>
    public static void RedundantIndexRemoved(
        this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
        IReadOnlyList<IReadOnlyPropertyBase> redundantIndex,
        IReadOnlyList<IReadOnlyPropertyBase> otherIndex)
    {
        var definition = CoreResources.LogRedundantIndexRemoved(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                redundantIndex.Format(),
                redundantIndex.First().DeclaringType.DisplayName(),
                otherIndex.Format());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new TwoPropertyBaseCollectionsEventData(
                definition,
                RedundantIndexRemoved,
                redundantIndex,
                otherIndex);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string RedundantIndexRemoved(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string>)definition;
        var p = (TwoPropertyBaseCollectionsEventData)payload;
        return d.GenerateMessage(
            p.FirstPropertyCollection.Format(),
            p.FirstPropertyCollection.First().DeclaringType.DisplayName(),
            p.SecondPropertyCollection.Format());
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.RedundantForeignKeyWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="redundantForeignKey">The redundant foreign key.</param>
    public static void RedundantForeignKeyWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        IForeignKey redundantForeignKey)
    {
        var definition = CoreResources.LogRedundantForeignKey(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                redundantForeignKey.Properties.Format(),
                redundantForeignKey.DeclaringEntityType.DisplayName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new ForeignKeyEventData(
                definition,
                RedundantForeignKeyWarning,
                redundantForeignKey);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string RedundantForeignKeyWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (ForeignKeyEventData)payload;
        return d.GenerateMessage(
            p.ForeignKey.Properties.Format(),
            p.ForeignKey.DeclaringEntityType.DisplayName());
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.IncompatibleMatchingForeignKeyProperties" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="dependentToPrincipalNavigationSpecification">
    ///     The name of the navigation property or entity type on the dependent end of the
    ///     relationship.
    /// </param>
    /// <param name="principalToDependentNavigationSpecification">
    ///     The name of the navigation property or entity type on the principal end of the
    ///     relationship.
    /// </param>
    /// <param name="foreignKeyProperties">The properties that make up the foreign key.</param>
    /// <param name="principalKeyProperties">The corresponding keys on the principal side.</param>
    public static void IncompatibleMatchingForeignKeyProperties(
        this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
        string dependentToPrincipalNavigationSpecification,
        string principalToDependentNavigationSpecification,
        IReadOnlyList<IReadOnlyPropertyBase> foreignKeyProperties,
        IReadOnlyList<IReadOnlyPropertyBase> principalKeyProperties)
    {
        var definition = CoreResources.LogIncompatibleMatchingForeignKeyProperties(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                dependentToPrincipalNavigationSpecification,
                principalToDependentNavigationSpecification,
                foreignKeyProperties.Format(includeTypes: true),
                principalKeyProperties.Format(includeTypes: true));
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new ForeignKeyCandidateEventData(
                definition,
                IncompatibleMatchingForeignKeyProperties,
                dependentToPrincipalNavigationSpecification,
                principalToDependentNavigationSpecification,
                foreignKeyProperties,
                principalKeyProperties);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string IncompatibleMatchingForeignKeyProperties(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string, string>)definition;
        var p = (ForeignKeyCandidateEventData)payload;
        return d.GenerateMessage(
            p.DependentToPrincipalNavigationSpecification,
            p.PrincipalToDependentNavigationSpecification,
            p.FirstPropertyCollection.Format(includeTypes: true),
            p.SecondPropertyCollection.Format(includeTypes: true));
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.AmbiguousEndRequiredWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="foreignKey">The foreign key.</param>
    public static void AmbiguousEndRequiredWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
        IForeignKey foreignKey)
    {
        var definition = CoreResources.LogAmbiguousEndRequired(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                foreignKey.Properties.Format(),
                foreignKey.DeclaringEntityType.DisplayName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new ForeignKeyEventData(
                definition,
                AmbiguousEndRequiredWarning,
                foreignKey);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string AmbiguousEndRequiredWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (ForeignKeyEventData)payload;
        return d.GenerateMessage(
            p.ForeignKey.Properties.Format(),
            p.ForeignKey.DeclaringEntityType.DisplayName());
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.RequiredAttributeOnCollection" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="navigation">The navigation property.</param>
    public static void RequiredAttributeOnCollection(
        this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
        IReadOnlyNavigation navigation)
    {
        var definition = CoreResources.LogRequiredAttributeOnCollection(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, navigation.DeclaringEntityType.DisplayName(), navigation.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new NavigationEventData(
                definition,
                RequiredAttributeOnCollection,
                navigation);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string RequiredAttributeOnCollection(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (NavigationEventData)payload;
        return d.GenerateMessage(p.Navigation.DeclaringEntityType.DisplayName(), p.Navigation.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.RequiredAttributeOnSkipNavigation" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="navigation">The navigation property.</param>
    public static void RequiredAttributeOnSkipNavigation(
        this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
        IReadOnlySkipNavigation navigation)
    {
        var definition = CoreResources.LogRequiredAttributeOnSkipNavigation(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, navigation.DeclaringEntityType.DisplayName(), navigation.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new SkipNavigationEventData(
                definition,
                RequiredAttributeOnSkipNavigation,
                navigation);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string RequiredAttributeOnSkipNavigation(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (SkipNavigationEventData)payload;
        return d.GenerateMessage(p.Navigation.DeclaringEntityType.DisplayName(), p.Navigation.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.ConflictingShadowForeignKeysWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="foreignKey">The foreign key.</param>
    public static void ConflictingShadowForeignKeysWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
        IReadOnlyForeignKey foreignKey)
    {
        var definition = CoreResources.LogConflictingShadowForeignKeys(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            var declaringTypeName = foreignKey.DeclaringEntityType.DisplayName();
            definition.Log(
                diagnostics,
                declaringTypeName,
                foreignKey.PrincipalEntityType.DisplayName(),
                declaringTypeName);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new ForeignKeyEventData(
                definition,
                ConflictingShadowForeignKeysWarning,
                foreignKey);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ConflictingShadowForeignKeysWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string>)definition;
        var p = (ForeignKeyEventData)payload;
        return d.GenerateMessage(
            p.ForeignKey.DeclaringEntityType.DisplayName(),
            p.ForeignKey.PrincipalEntityType.DisplayName(),
            p.ForeignKey.DeclaringEntityType.DisplayName());
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.MultiplePrimaryKeyCandidates" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="firstProperty">The first property.</param>
    /// <param name="secondProperty">The second property.</param>
    public static void MultiplePrimaryKeyCandidates(
        this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
        IReadOnlyProperty firstProperty,
        IReadOnlyProperty secondProperty)
    {
        var definition = CoreResources.LogMultiplePrimaryKeyCandidates(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                firstProperty.Name,
                secondProperty.Name,
                firstProperty.DeclaringType.DisplayName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new TwoPropertyBaseCollectionsEventData(
                definition,
                MultiplePrimaryKeyCandidates,
                new[] { firstProperty },
                new[] { secondProperty });

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string MultiplePrimaryKeyCandidates(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string>)definition;
        var p = (TwoPropertyBaseCollectionsEventData)payload;
        var firstProperty = p.FirstPropertyCollection[0];
        var secondProperty = p.SecondPropertyCollection[0];
        return d.GenerateMessage(
            firstProperty.Name,
            secondProperty.Name,
            firstProperty.DeclaringType.DisplayName());
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.MultipleNavigationProperties" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="firstPropertyCollection">The first set of properties.</param>
    /// <param name="secondPropertyCollection">The second set of properties.</param>
    public static void MultipleNavigationProperties(
        this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
        IEnumerable<Tuple<MemberInfo?, Type>> firstPropertyCollection,
        IEnumerable<Tuple<MemberInfo?, Type>> secondPropertyCollection)
    {
        var definition = CoreResources.LogMultipleNavigationProperties(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                firstPropertyCollection.First().Item2.ShortDisplayName(),
                secondPropertyCollection.First().Item2.ShortDisplayName(),
                Property.Format(firstPropertyCollection.Select(p => p.Item1?.Name)),
                Property.Format(secondPropertyCollection.Select(p => p.Item1?.Name)));
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new TwoUnmappedPropertyCollectionsEventData(
                definition,
                MultipleNavigationProperties,
                firstPropertyCollection,
                secondPropertyCollection);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string MultipleNavigationProperties(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string, string>)definition;
        var p = (TwoUnmappedPropertyCollectionsEventData)payload;
        return d.GenerateMessage(
            p.FirstPropertyCollection.First().Item2.ShortDisplayName(),
            p.SecondPropertyCollection.First().Item2.ShortDisplayName(),
            Property.Format(p.FirstPropertyCollection.Select(i => i.Item1?.Name)),
            Property.Format(p.SecondPropertyCollection.Select(i => i.Item1?.Name)));
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.MultipleInversePropertiesSameTargetWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="conflictingNavigations">The list of conflicting navigation properties.</param>
    /// <param name="inverseNavigation">The inverse navigation property.</param>
    /// <param name="targetType">The target type.</param>
    public static void MultipleInversePropertiesSameTargetWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
        IEnumerable<Tuple<MemberInfo?, Type>> conflictingNavigations,
        MemberInfo inverseNavigation,
        Type targetType)
    {
        var definition = CoreResources.LogMultipleInversePropertiesSameTarget(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                string.Join(", ", conflictingNavigations.Select(n => n.Item2.ShortDisplayName() + "." + n.Item1?.Name)),
                inverseNavigation.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new TwoUnmappedPropertyCollectionsEventData(
                definition,
                MultipleInversePropertiesSameTargetWarning,
                conflictingNavigations,
                new[] { new Tuple<MemberInfo?, Type>(inverseNavigation, targetType) });

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string MultipleInversePropertiesSameTargetWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string?>)definition;
        var p = (TwoUnmappedPropertyCollectionsEventData)payload;
        return d.GenerateMessage(
            string.Join(
                ", ", p.FirstPropertyCollection.Select(n => n.Item2.ShortDisplayName() + "." + n.Item1!.Name)),
            p.SecondPropertyCollection.First().Item1?.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.NonOwnershipInverseNavigationWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="declaringType">The declaring entity type.</param>
    /// <param name="navigation">The navigation property.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="inverseNavigation">The inverse navigation property.</param>
    /// <param name="ownershipNavigation">The ownership navigation property.</param>
    public static void NonOwnershipInverseNavigationWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
        IReadOnlyEntityType declaringType,
        MemberInfo navigation,
        IReadOnlyEntityType targetType,
        MemberInfo inverseNavigation,
        MemberInfo ownershipNavigation)
    {
        var definition = CoreResources.LogNonOwnershipInverseNavigation(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                targetType.DisplayName(),
                inverseNavigation.Name,
                declaringType.DisplayName(),
                navigation.Name,
                ownershipNavigation.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new TwoUnmappedPropertyCollectionsEventData(
                definition,
                NonOwnershipInverseNavigationWarning,
                new[] { new Tuple<MemberInfo?, Type>(navigation, declaringType.ClrType) },
                new[]
                {
                    new Tuple<MemberInfo?, Type>(inverseNavigation, targetType.ClrType),
                    new Tuple<MemberInfo?, Type>(ownershipNavigation, targetType.ClrType)
                });

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string NonOwnershipInverseNavigationWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string?, string, string?, string?>)definition;
        var p = (TwoUnmappedPropertyCollectionsEventData)payload;
        var (memberInfo1, type1) = p.FirstPropertyCollection.First();
        var (memberInfo2, type2) = p.SecondPropertyCollection.First();
        var ownershipNavigation = p.SecondPropertyCollection.Last();
        return d.GenerateMessage(
            type2.ShortDisplayName(),
            memberInfo2?.Name,
            type1.ShortDisplayName(),
            memberInfo1?.Name,
            ownershipNavigation.Item1?.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.ForeignKeyAttributesOnBothPropertiesWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="firstNavigation">The first navigation property.</param>
    /// <param name="secondNavigation">The second navigation property.</param>
    /// <param name="firstProperty">The first property.</param>
    /// <param name="secondProperty">The second property.</param>
    public static void ForeignKeyAttributesOnBothPropertiesWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
        IReadOnlyNavigation firstNavigation,
        IReadOnlyNavigation secondNavigation,
        MemberInfo firstProperty,
        MemberInfo secondProperty)
    {
        var definition = CoreResources.LogForeignKeyAttributesOnBothProperties(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                firstNavigation.DeclaringEntityType.ClrType.ShortDisplayName(),
                firstNavigation.GetIdentifyingMemberInfo()!.Name,
                secondNavigation.DeclaringEntityType.ClrType.ShortDisplayName(),
                secondNavigation.GetIdentifyingMemberInfo()!.Name,
                firstProperty.Name,
                secondProperty.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new TwoUnmappedPropertyCollectionsEventData(
                definition,
                ForeignKeyAttributesOnBothPropertiesWarning,
                new[]
                {
                    new Tuple<MemberInfo?, Type>(
                        firstNavigation.GetIdentifyingMemberInfo()!, firstNavigation.DeclaringEntityType.ClrType),
                    new Tuple<MemberInfo?, Type>(firstProperty, firstNavigation.DeclaringEntityType.ClrType)
                },
                new[]
                {
                    new Tuple<MemberInfo?, Type>(
                        secondNavigation.GetIdentifyingMemberInfo()!, secondNavigation.DeclaringEntityType.ClrType),
                    new Tuple<MemberInfo?, Type>(secondProperty, secondNavigation.DeclaringEntityType.ClrType)
                });

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ForeignKeyAttributesOnBothPropertiesWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string?, string, string?, string?, string?>)definition;
        var p = (TwoUnmappedPropertyCollectionsEventData)payload;
        var (memberInfo1, type1) = p.FirstPropertyCollection.First();
        var firstProperty = p.FirstPropertyCollection.Last();
        var (memberInfo2, type2) = p.SecondPropertyCollection.First();
        var secondProperty = p.SecondPropertyCollection.Last();
        return d.GenerateMessage(
            type1.ShortDisplayName(),
            memberInfo1?.Name,
            type2.ShortDisplayName(),
            memberInfo2?.Name,
            firstProperty.Item1?.Name,
            secondProperty.Item1?.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.ForeignKeyAttributesOnBothNavigationsWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="firstNavigation">The first navigation property.</param>
    /// <param name="secondNavigation">The second navigation property.</param>
    public static void ForeignKeyAttributesOnBothNavigationsWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
        IReadOnlyNavigation firstNavigation,
        IReadOnlyNavigation secondNavigation)
    {
        var definition = CoreResources.LogForeignKeyAttributesOnBothNavigations(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                firstNavigation.DeclaringEntityType.DisplayName(),
                firstNavigation.Name,
                secondNavigation.DeclaringEntityType.DisplayName(),
                secondNavigation.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new TwoPropertyBaseCollectionsEventData(
                definition,
                ForeignKeyAttributesOnBothNavigationsWarning,
                new[] { firstNavigation },
                new[] { secondNavigation });

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ForeignKeyAttributesOnBothNavigationsWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string, string>)definition;
        var p = (TwoPropertyBaseCollectionsEventData)payload;
        var firstNavigation = p.FirstPropertyCollection[0];
        var secondNavigation = p.SecondPropertyCollection[0];
        return d.GenerateMessage(
            firstNavigation.DeclaringType.DisplayName(),
            firstNavigation.Name,
            secondNavigation.DeclaringType.DisplayName(),
            secondNavigation.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.ConflictingForeignKeyAttributesOnNavigationAndPropertyWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="navigation">The navigation property.</param>
    /// <param name="property">The property.</param>
    public static void ConflictingForeignKeyAttributesOnNavigationAndPropertyWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
        IReadOnlyNavigation navigation,
        MemberInfo property)
    {
        var definition = CoreResources.LogConflictingForeignKeyAttributesOnNavigationAndProperty(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                navigation.DeclaringEntityType.ClrType.ShortDisplayName(),
                navigation.GetIdentifyingMemberInfo()?.Name,
                property.DeclaringType!.ShortDisplayName(),
                property.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new TwoUnmappedPropertyCollectionsEventData(
                definition,
                ConflictingForeignKeyAttributesOnNavigationAndPropertyWarning,
                new[] { new Tuple<MemberInfo?, Type>(navigation.GetIdentifyingMemberInfo()!, navigation.DeclaringEntityType.ClrType) },
                new[] { new Tuple<MemberInfo?, Type>(property, property.DeclaringType!) });

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ConflictingForeignKeyAttributesOnNavigationAndPropertyWarning(
        EventDefinitionBase definition,
        EventData payload)
    {
        var d = (EventDefinition<string, string?, string, string?>)definition;
        var p = (TwoUnmappedPropertyCollectionsEventData)payload;
        var (memberInfo1, type1) = p.FirstPropertyCollection.First();
        var (memberInfo2, type2) = p.SecondPropertyCollection.First();
        return d.GenerateMessage(
            type1.ShortDisplayName(),
            memberInfo1?.Name,
            type2.ShortDisplayName(),
            memberInfo2?.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.DetectChangesStarting" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="context">The context being used.</param>
    public static void DetectChangesStarting(
        this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
        DbContext context)
    {
        var definition = CoreResources.LogDetectChangesStarting(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, context.GetType().ShortDisplayName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new DbContextEventData(
                definition,
                DetectChangesStarting,
                context);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string DetectChangesStarting(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string?>)definition;
        var p = (DbContextEventData)payload;
        return d.GenerateMessage(p.Context?.GetType().ShortDisplayName());
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.DetectChangesCompleted" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="context">The context being used.</param>
    public static void DetectChangesCompleted(
        this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
        DbContext context)
    {
        var definition = CoreResources.LogDetectChangesCompleted(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, context.GetType().ShortDisplayName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new DbContextEventData(
                definition,
                DetectChangesCompleted,
                context);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string DetectChangesCompleted(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string?>)definition;
        var p = (DbContextEventData)payload;
        return d.GenerateMessage(p.Context?.GetType().ShortDisplayName());
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.PropertyChangeDetected" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="internalEntityEntry">The internal entity entry.</param>
    /// <param name="property">The property.</param>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    public static void PropertyChangeDetected(
        this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
        InternalEntityEntry internalEntityEntry,
        IProperty property,
        object? oldValue,
        object? newValue)
    {
        var definition = CoreResources.LogPropertyChangeDetected(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, property.DeclaringType.ShortName(), property.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new PropertyChangedEventData(
                definition,
                PropertyChangeDetected,
                new EntityEntry(internalEntityEntry),
                property,
                oldValue,
                newValue);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string PropertyChangeDetected(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (PropertyChangedEventData)payload;
        return d.GenerateMessage(
            p.Property.DeclaringType.ShortName(),
            p.Property.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.PropertyChangeDetected" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="internalEntityEntry">The internal entity entry.</param>
    /// <param name="property">The property.</param>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    public static void PropertyChangeDetectedSensitive(
        this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
        InternalEntityEntry internalEntityEntry,
        IProperty property,
        object? oldValue,
        object? newValue)
    {
        var definition = CoreResources.LogPropertyChangeDetectedSensitive(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                property.DeclaringType.ShortName(),
                property.Name,
                oldValue,
                newValue,
                internalEntityEntry.BuildCurrentValuesString(property.DeclaringType.ContainingEntityType.FindPrimaryKey()!.Properties));
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new PropertyChangedEventData(
                definition,
                PropertyChangeDetectedSensitive,
                new EntityEntry(internalEntityEntry),
                property,
                oldValue,
                newValue);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string PropertyChangeDetectedSensitive(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, object?, object?, string>)definition;
        var p = (PropertyChangedEventData)payload;
        return d.GenerateMessage(
            p.Property.DeclaringType.ShortName(),
            p.Property.Name,
            p.OldValue,
            p.NewValue,
            p.EntityEntry.GetInfrastructure().BuildCurrentValuesString(
                p.Property.DeclaringType.ContainingEntityType.FindPrimaryKey()!.Properties));
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.ForeignKeyChangeDetected" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="internalEntityEntry">The internal entity entry.</param>
    /// <param name="property">The property.</param>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">the new value.</param>
    public static void ForeignKeyChangeDetected(
        this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
        InternalEntityEntry internalEntityEntry,
        IProperty property,
        object? oldValue,
        object? newValue)
    {
        var definition = CoreResources.LogForeignKeyChangeDetected(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                property.DeclaringType.ShortName(),
                property.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new PropertyChangedEventData(
                definition,
                ForeignKeyChangeDetected,
                new EntityEntry(internalEntityEntry),
                property,
                oldValue,
                newValue);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ForeignKeyChangeDetected(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (PropertyChangedEventData)payload;
        return d.GenerateMessage(
            p.Property.DeclaringType.ShortName(),
            p.Property.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.ForeignKeyChangeDetected" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="internalEntityEntry">The internal entity entry.</param>
    /// <param name="property">The property.</param>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    public static void ForeignKeyChangeDetectedSensitive(
        this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
        InternalEntityEntry internalEntityEntry,
        IProperty property,
        object? oldValue,
        object? newValue)
    {
        var definition = CoreResources.LogForeignKeyChangeDetectedSensitive(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                property.DeclaringType.ShortName(),
                property.Name,
                oldValue,
                newValue,
                internalEntityEntry.BuildCurrentValuesString(property.DeclaringType.ContainingEntityType.FindPrimaryKey()!.Properties));
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new PropertyChangedEventData(
                definition,
                ForeignKeyChangeDetectedSensitive,
                new EntityEntry(internalEntityEntry),
                property,
                oldValue,
                newValue);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ForeignKeyChangeDetectedSensitive(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, object?, object?, string>)definition;
        var p = (PropertyChangedEventData)payload;
        return d.GenerateMessage(
            p.Property.DeclaringType.ShortName(),
            p.Property.Name,
            p.OldValue,
            p.NewValue,
            p.EntityEntry.GetInfrastructure().BuildCurrentValuesString(
                p.Property.DeclaringType.ContainingEntityType.FindPrimaryKey()!.Properties));
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.CollectionChangeDetected" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="internalEntityEntry">The internal entity entry.</param>
    /// <param name="navigation">The navigation property.</param>
    /// <param name="added">The added values.</param>
    /// <param name="removed">The removed values.</param>
    public static void CollectionChangeDetected(
        this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
        InternalEntityEntry internalEntityEntry,
        INavigation navigation,
        ISet<object> added,
        ISet<object> removed)
    {
        var definition = CoreResources.LogCollectionChangeDetected(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                added.Count,
                removed.Count,
                navigation.DeclaringEntityType.ShortName(),
                navigation.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new CollectionChangedEventData(
                definition,
                CollectionChangeDetected,
                new EntityEntry(internalEntityEntry),
                navigation,
                added,
                removed);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string CollectionChangeDetected(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<int, int, string, string>)definition;
        var p = (CollectionChangedEventData)payload;
        return d.GenerateMessage(
            p.Added.Count(),
            p.Removed.Count(),
            p.Navigation.DeclaringEntityType.ShortName(),
            p.Navigation.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.CollectionChangeDetected" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="internalEntityEntry">The internal entity entry.</param>
    /// <param name="navigation">The navigation property.</param>
    /// <param name="added">The added values.</param>
    /// <param name="removed">The removed values.</param>
    public static void CollectionChangeDetectedSensitive(
        this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
        InternalEntityEntry internalEntityEntry,
        INavigation navigation,
        ISet<object> added,
        ISet<object> removed)
    {
        var definition = CoreResources.LogCollectionChangeDetectedSensitive(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                added.Count,
                removed.Count,
                navigation.DeclaringEntityType.ShortName(),
                navigation.Name,
                internalEntityEntry.BuildCurrentValuesString(navigation.DeclaringEntityType.FindPrimaryKey()!.Properties));
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new CollectionChangedEventData(
                definition,
                CollectionChangeDetectedSensitive,
                new EntityEntry(internalEntityEntry),
                navigation,
                added,
                removed);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string CollectionChangeDetectedSensitive(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<int, int, string, string, string>)definition;
        var p = (CollectionChangedEventData)payload;
        return d.GenerateMessage(
            p.Added.Count(),
            p.Removed.Count(),
            p.Navigation.DeclaringEntityType.ShortName(),
            p.Navigation.Name,
            p.EntityEntry.GetInfrastructure().BuildCurrentValuesString(p.Navigation.DeclaringEntityType.FindPrimaryKey()!.Properties));
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.CollectionChangeDetected" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="internalEntityEntry">The internal entity entry.</param>
    /// <param name="navigation">The navigation property.</param>
    /// <param name="added">The added values.</param>
    /// <param name="removed">The removed values.</param>
    public static void SkipCollectionChangeDetected(
        this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
        InternalEntityEntry internalEntityEntry,
        ISkipNavigation navigation,
        ISet<object> added,
        ISet<object> removed)
    {
        var definition = CoreResources.LogSkipCollectionChangeDetected(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                added.Count,
                removed.Count,
                navigation.DeclaringEntityType.ShortName(),
                navigation.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new SkipCollectionChangedEventData(
                definition,
                SkipCollectionChangeDetected,
                new EntityEntry(internalEntityEntry),
                navigation,
                added,
                removed);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string SkipCollectionChangeDetected(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<int, int, string, string>)definition;
        var p = (SkipCollectionChangedEventData)payload;
        return d.GenerateMessage(
            p.Added.Count(),
            p.Removed.Count(),
            p.Navigation.DeclaringEntityType.ShortName(),
            p.Navigation.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.CollectionChangeDetected" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="internalEntityEntry">The internal entity entry.</param>
    /// <param name="navigation">The navigation property.</param>
    /// <param name="added">The added values.</param>
    /// <param name="removed">The removed values.</param>
    public static void SkipCollectionChangeDetectedSensitive(
        this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
        InternalEntityEntry internalEntityEntry,
        ISkipNavigation navigation,
        ISet<object> added,
        ISet<object> removed)
    {
        var definition = CoreResources.LogSkipCollectionChangeDetectedSensitive(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                added.Count,
                removed.Count,
                navigation.DeclaringEntityType.ShortName(),
                navigation.Name,
                internalEntityEntry.BuildCurrentValuesString(navigation.DeclaringEntityType.FindPrimaryKey()!.Properties));
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new SkipCollectionChangedEventData(
                definition,
                SkipCollectionChangeDetectedSensitive,
                new EntityEntry(internalEntityEntry),
                navigation,
                added,
                removed);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string SkipCollectionChangeDetectedSensitive(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<int, int, string, string, string>)definition;
        var p = (SkipCollectionChangedEventData)payload;
        return d.GenerateMessage(
            p.Added.Count(),
            p.Removed.Count(),
            p.Navigation.DeclaringEntityType.ShortName(),
            p.Navigation.Name,
            p.EntityEntry.GetInfrastructure().BuildCurrentValuesString(p.Navigation.DeclaringEntityType.FindPrimaryKey()!.Properties));
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.ReferenceChangeDetected" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="internalEntityEntry">The internal entity entry.</param>
    /// <param name="navigation">The navigation property.</param>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    public static void ReferenceChangeDetected(
        this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
        InternalEntityEntry internalEntityEntry,
        INavigation navigation,
        object? oldValue,
        object? newValue)
    {
        var definition = CoreResources.LogReferenceChangeDetected(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, navigation.DeclaringEntityType.ShortName(), navigation.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new ReferenceChangedEventData(
                definition,
                ReferenceChangeDetected,
                new EntityEntry(internalEntityEntry),
                navigation,
                oldValue,
                newValue);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ReferenceChangeDetected(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (ReferenceChangedEventData)payload;
        return d.GenerateMessage(
            p.Navigation.DeclaringEntityType.ShortName(),
            p.Navigation.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.ReferenceChangeDetected" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="internalEntityEntry">The internal entity entry.</param>
    /// <param name="navigation">The navigation property.</param>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    public static void ReferenceChangeDetectedSensitive(
        this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
        InternalEntityEntry internalEntityEntry,
        INavigation navigation,
        object? oldValue,
        object? newValue)
    {
        var definition = CoreResources.LogReferenceChangeDetectedSensitive(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                navigation.DeclaringEntityType.ShortName(),
                navigation.Name,
                internalEntityEntry.BuildCurrentValuesString(navigation.DeclaringEntityType.FindPrimaryKey()!.Properties));
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new ReferenceChangedEventData(
                definition,
                ReferenceChangeDetectedSensitive,
                new EntityEntry(internalEntityEntry),
                navigation,
                oldValue,
                newValue);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ReferenceChangeDetectedSensitive(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string>)definition;
        var p = (ReferenceChangedEventData)payload;
        return d.GenerateMessage(
            p.Navigation.DeclaringEntityType.ShortName(),
            p.Navigation.Name,
            p.EntityEntry.GetInfrastructure().BuildCurrentValuesString(p.Navigation.DeclaringEntityType.FindPrimaryKey()!.Properties));
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.StringEnumValueInJson" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="enumType">The type.</param>
    public static void StringEnumValueInJson(
        this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
        Type enumType)
    {
        var definition = CoreResources.LogStringEnumValueInJson(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, enumType.ShortDisplayName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new TypeEventData(
                definition,
                StringEnumValueInJson,
                enumType);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string StringEnumValueInJson(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string>)definition;
        var p = (TypeEventData)payload;
        return d.GenerateMessage(p.ClrType.ShortDisplayName());
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.StartedTracking" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="internalEntityEntry">The internal entity entry.</param>
    public static void StartedTracking(
        this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
        InternalEntityEntry internalEntityEntry)
    {
        var definition = CoreResources.LogStartedTracking(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                internalEntityEntry.Context.GetType().ShortDisplayName(),
                internalEntityEntry.EntityType.ShortName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new EntityEntryEventData(
                definition,
                StartedTracking,
                new EntityEntry(internalEntityEntry));

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string StartedTracking(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (EntityEntryEventData)payload;
        return d.GenerateMessage(
            p.EntityEntry.Context.GetType().ShortDisplayName(),
            p.EntityEntry.Metadata.ShortName());
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.StartedTracking" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="internalEntityEntry">The internal entity entry.</param>
    public static void StartedTrackingSensitive(
        this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
        InternalEntityEntry internalEntityEntry)
    {
        var definition = CoreResources.LogStartedTrackingSensitive(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                internalEntityEntry.Context.GetType().ShortDisplayName(),
                internalEntityEntry.EntityType.ShortName(),
                internalEntityEntry.BuildCurrentValuesString(internalEntityEntry.EntityType.FindPrimaryKey()!.Properties));
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new EntityEntryEventData(
                definition,
                StartedTrackingSensitive,
                new EntityEntry(internalEntityEntry));

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string StartedTrackingSensitive(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string>)definition;
        var p = (EntityEntryEventData)payload;
        return d.GenerateMessage(
            p.EntityEntry.Context.GetType().ShortDisplayName(),
            p.EntityEntry.Metadata.ShortName(),
            p.EntityEntry.GetInfrastructure().BuildCurrentValuesString(p.EntityEntry.Metadata.FindPrimaryKey()!.Properties));
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.StateChanged" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="internalEntityEntry">The internal entity entry.</param>
    /// <param name="oldState">The old value.</param>
    /// <param name="newState">The new value.</param>
    public static void StateChanged(
        this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
        InternalEntityEntry internalEntityEntry,
        EntityState oldState,
        EntityState newState)
    {
        var definition = CoreResources.LogStateChanged(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                internalEntityEntry.EntityType.ShortName(),
                internalEntityEntry.Context.GetType().ShortDisplayName(),
                oldState,
                newState);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new StateChangedEventData(
                definition,
                StateChanged,
                new EntityEntry(internalEntityEntry),
                oldState,
                newState);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string StateChanged(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, EntityState, EntityState>)definition;
        var p = (StateChangedEventData)payload;
        return d.GenerateMessage(
            p.EntityEntry.Metadata.ShortName(),
            p.EntityEntry.Context.GetType().ShortDisplayName(),
            p.OldState,
            p.NewState);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.StateChanged" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="internalEntityEntry">The internal entity entry.</param>
    /// <param name="oldState">The old state.</param>
    /// <param name="newState">The new state.</param>
    public static void StateChangedSensitive(
        this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
        InternalEntityEntry internalEntityEntry,
        EntityState oldState,
        EntityState newState)
    {
        var definition = CoreResources.LogStateChangedSensitive(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                internalEntityEntry.EntityType.ShortName(),
                internalEntityEntry.BuildCurrentValuesString(internalEntityEntry.EntityType.FindPrimaryKey()!.Properties),
                internalEntityEntry.Context.GetType().ShortDisplayName(),
                oldState,
                newState);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new StateChangedEventData(
                definition,
                StateChangedSensitive,
                new EntityEntry(internalEntityEntry),
                oldState,
                newState);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string StateChangedSensitive(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string, EntityState, EntityState>)definition;
        var p = (StateChangedEventData)payload;
        return d.GenerateMessage(
            p.EntityEntry.Metadata.ShortName(),
            p.EntityEntry.GetInfrastructure().BuildCurrentValuesString(p.EntityEntry.Metadata.FindPrimaryKey()!.Properties),
            p.EntityEntry.Context.GetType().ShortDisplayName(),
            p.OldState,
            p.NewState);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.ValueGenerated" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="internalEntityEntry">The internal entity entry.</param>
    /// <param name="property">The property.</param>
    /// <param name="value">The value generated.</param>
    /// <param name="temporary">Indicates whether or not the value is a temporary or permanent value.</param>
    public static void ValueGenerated(
        this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
        InternalEntityEntry internalEntityEntry,
        IProperty property,
        object? value,
        bool temporary)
    {
        var definition = temporary
            ? CoreResources.LogTempValueGenerated(diagnostics)
            : CoreResources.LogValueGenerated(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                internalEntityEntry.Context.GetType().ShortDisplayName(),
                internalEntityEntry.EntityType.ShortName(),
                property.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new PropertyValueEventData(
                definition,
                ValueGenerated,
                new EntityEntry(internalEntityEntry),
                property,
                value);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ValueGenerated(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string>)definition;
        var p = (PropertyValueEventData)payload;
        return d.GenerateMessage(
            p.EntityEntry.Context.GetType().ShortDisplayName(),
            p.EntityEntry.Metadata.ShortName(),
            p.Property.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.ValueGenerated" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="internalEntityEntry">The internal entity entry.</param>
    /// <param name="property">The property.</param>
    /// <param name="value">The value generated.</param>
    /// <param name="temporary">Indicates whether or not the value is a temporary or permanent value.</param>
    public static void ValueGeneratedSensitive(
        this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
        InternalEntityEntry internalEntityEntry,
        IProperty property,
        object? value,
        bool temporary)
    {
        var definition = temporary
            ? CoreResources.LogTempValueGeneratedSensitive(diagnostics)
            : CoreResources.LogValueGeneratedSensitive(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                internalEntityEntry.Context.GetType().ShortDisplayName(),
                value,
                internalEntityEntry.EntityType.ShortName(),
                property.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new PropertyValueEventData(
                definition,
                ValueGeneratedSensitive,
                new EntityEntry(internalEntityEntry),
                property,
                value);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ValueGeneratedSensitive(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, object?, string, string>)definition;
        var p = (PropertyValueEventData)payload;
        return d.GenerateMessage(
            p.EntityEntry.Context.GetType().ShortDisplayName(),
            p.Value,
            p.EntityEntry.Metadata.ShortName(),
            p.Property.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.CascadeDelete" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="internalChildEntry">The child internal entity entry.</param>
    /// <param name="internalParentEntry">The parent internal entity entry.</param>
    /// <param name="state">The target state.</param>
    public static void CascadeDelete(
        this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
        InternalEntityEntry internalChildEntry,
        InternalEntityEntry internalParentEntry,
        EntityState state)
    {
        var definition = CoreResources.LogCascadeDelete(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                internalChildEntry.EntityType.ShortName(),
                state,
                internalParentEntry.EntityType.ShortName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new CascadeDeleteEventData(
                definition,
                CascadeDelete,
                new EntityEntry(internalChildEntry),
                new EntityEntry(internalParentEntry),
                state);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string CascadeDelete(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, EntityState, string>)definition;
        var p = (CascadeDeleteEventData)payload;
        return d.GenerateMessage(
            p.EntityEntry.Metadata.ShortName(),
            p.State,
            p.ParentEntityEntry.Metadata.ShortName());
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.CascadeDelete" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="internalChildEntry">The child internal entity entry.</param>
    /// <param name="internalParentEntry">The parent internal entity entry.</param>
    /// <param name="state">The target state.</param>
    public static void CascadeDeleteSensitive(
        this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
        InternalEntityEntry internalChildEntry,
        InternalEntityEntry internalParentEntry,
        EntityState state)
    {
        var definition = CoreResources.LogCascadeDeleteSensitive(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                internalChildEntry.EntityType.ShortName(),
                internalChildEntry.BuildCurrentValuesString(internalChildEntry.EntityType.FindPrimaryKey()!.Properties),
                state,
                internalParentEntry.EntityType.ShortName(),
                internalParentEntry.BuildCurrentValuesString(internalParentEntry.EntityType.FindPrimaryKey()!.Properties));
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new CascadeDeleteEventData(
                definition,
                CascadeDeleteSensitive,
                new EntityEntry(internalChildEntry),
                new EntityEntry(internalParentEntry),
                state);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string CascadeDeleteSensitive(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, EntityState, string, string>)definition;
        var p = (CascadeDeleteEventData)payload;
        return d.GenerateMessage(
            p.EntityEntry.Metadata.ShortName(),
            p.EntityEntry.GetInfrastructure().BuildCurrentValuesString(p.EntityEntry.Metadata.FindPrimaryKey()!.Properties),
            p.State,
            p.ParentEntityEntry.Metadata.ShortName(),
            p.ParentEntityEntry.GetInfrastructure()
                .BuildCurrentValuesString(p.ParentEntityEntry.Metadata.FindPrimaryKey()!.Properties));
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.CascadeDeleteOrphan" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="internalChildEntry">The child internal entity entry.</param>
    /// <param name="parentEntityType">The parent entity type.</param>
    /// <param name="state">The target state.</param>
    public static void CascadeDeleteOrphan(
        this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
        InternalEntityEntry internalChildEntry,
        IEntityType parentEntityType,
        EntityState state)
    {
        var definition = CoreResources.LogCascadeDeleteOrphan(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                internalChildEntry.EntityType.ShortName(),
                state,
                parentEntityType.ShortName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new CascadeDeleteOrphanEventData(
                definition,
                CascadeDeleteOrphan,
                new EntityEntry(internalChildEntry),
                parentEntityType,
                state);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string CascadeDeleteOrphan(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, EntityState, string>)definition;
        var p = (CascadeDeleteOrphanEventData)payload;
        return d.GenerateMessage(
            p.EntityEntry.Metadata.ShortName(),
            p.State,
            p.ParentEntityType.ShortName());
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.MappedNavigationIgnoredWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="navigation">The navigation.</param>
    public static void MappedNavigationIgnoredWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
        INavigationBase navigation)
    {
        var definition = CoreResources.LogMappedNavigationIgnored(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, navigation.DeclaringType.ShortName(), navigation.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new NavigationBaseEventData(definition, MappedNavigationIgnoredWarning, navigation);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string MappedNavigationIgnoredWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (NavigationBaseEventData)payload;
        return d.GenerateMessage(p.NavigationBase.DeclaringType.ShortName(), p.NavigationBase.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.MappedPropertyIgnoredWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="property">The property.</param>
    public static void MappedPropertyIgnoredWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
        IProperty property)
    {
        var definition = CoreResources.LogMappedPropertyIgnored(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, property.DeclaringType.ShortName(), property.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new PropertyEventData(definition, MappedPropertyIgnoredWarning, property);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string MappedPropertyIgnoredWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (PropertyEventData)payload;
        return d.GenerateMessage(p.Property.DeclaringType.ShortName(), p.Property.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.MappedComplexPropertyIgnoredWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="property">The property.</param>
    public static void MappedComplexPropertyIgnoredWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
        IComplexProperty property)
    {
        var definition = CoreResources.LogMappedComplexPropertyIgnored(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, property.DeclaringType.ShortName(), property.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new ComplexPropertyEventData(definition, MappedComplexPropertyIgnoredWarning, property);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string MappedComplexPropertyIgnoredWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (ComplexPropertyEventData)payload;
        return d.GenerateMessage(p.Property.DeclaringType.ShortName(), p.Property.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.MappedEntityTypeIgnoredWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="entityType">The entity type.</param>
    public static void MappedEntityTypeIgnoredWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
        IEntityType entityType)
    {
        var definition = CoreResources.LogMappedEntityTypeIgnored(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, entityType.ShortName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new EntityTypeEventData(definition, MappedEntityTypeIgnoredWarning, entityType);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string MappedEntityTypeIgnoredWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string>)definition;
        var p = (EntityTypeEventData)payload;
        return d.GenerateMessage(p.EntityType.ShortName());
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.CascadeDeleteOrphan" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="internalChildEntry">The child internal entity entry.</param>
    /// <param name="parentEntityType">The parent entity type.</param>
    /// <param name="state">The target state.</param>
    public static void CascadeDeleteOrphanSensitive(
        this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
        InternalEntityEntry internalChildEntry,
        IEntityType parentEntityType,
        EntityState state)
    {
        var definition = CoreResources.LogCascadeDeleteOrphanSensitive(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                internalChildEntry.EntityType.ShortName(),
                internalChildEntry.BuildCurrentValuesString(internalChildEntry.EntityType.FindPrimaryKey()!.Properties),
                state,
                parentEntityType.ShortName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new CascadeDeleteOrphanEventData(
                definition,
                CascadeDeleteOrphanSensitive,
                new EntityEntry(internalChildEntry),
                parentEntityType,
                state);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string CascadeDeleteOrphanSensitive(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, EntityState, string>)definition;
        var p = (CascadeDeleteOrphanEventData)payload;
        return d.GenerateMessage(
            p.EntityEntry.Metadata.ShortName(),
            p.EntityEntry.GetInfrastructure().BuildCurrentValuesString(p.EntityEntry.Metadata.FindPrimaryKey()!.Properties),
            p.State,
            p.ParentEntityType.ShortName());
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.SaveChangesStarting" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="context">The context being used.</param>
    /// <returns>The, possibly intercepted, result.</returns>
    public static InterceptionResult<int> SaveChangesStarting(
        this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
        DbContext context)
    {
        var definition = CoreResources.LogSaveChangesStarting(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, context.GetType().ShortDisplayName());
        }

        if (diagnostics.NeedsEventData<ISaveChangesInterceptor>(
                definition,
                out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = CreateSaveChangesStartingEventData(context, definition);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.SavingChanges(eventData, default);
            }
        }

        return default;
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.SaveChangesStarting" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="context">The context being used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The, possibly intercepted, result.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static ValueTask<InterceptionResult<int>> SaveChangesStartingAsync(
        this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
        DbContext context,
        CancellationToken cancellationToken = default)
    {
        var definition = CoreResources.LogSaveChangesStarting(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, context.GetType().ShortDisplayName());
        }

        if (diagnostics.NeedsEventData<ISaveChangesInterceptor>(
                definition,
                out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = CreateSaveChangesStartingEventData(context, definition);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.SavingChangesAsync(eventData, default, cancellationToken);
            }
        }

        return default;
    }

    private static DbContextEventData CreateSaveChangesStartingEventData(DbContext context, EventDefinition<string?> definition)
        => new(
            definition,
            SaveChangesStarting,
            context);

    private static string SaveChangesStarting(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string?>)definition;
        var p = (DbContextEventData)payload;
        return d.GenerateMessage(p.Context?.GetType().ShortDisplayName());
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.SaveChangesCompleted" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="context">The context being used.</param>
    /// <param name="entitiesSavedCount">The number of entities saved.</param>
    /// <returns>The, possibly intercepted, result.</returns>
    public static int SaveChangesCompleted(
        this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
        DbContext context,
        int entitiesSavedCount)
    {
        var definition = CoreResources.LogSaveChangesCompleted(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, context.GetType().ShortDisplayName(), entitiesSavedCount);
        }

        if (diagnostics.NeedsEventData<ISaveChangesInterceptor>(
                definition,
                out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = CreateSaveChangesCompletedEventData(context, entitiesSavedCount, definition);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.SavedChanges(eventData, entitiesSavedCount);
            }
        }

        return entitiesSavedCount;
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.SaveChangesCompleted" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="context">The context being used.</param>
    /// <param name="entitiesSavedCount">The number of entities saved.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The, possibly intercepted, result.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static ValueTask<int> SaveChangesCompletedAsync(
        this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
        DbContext context,
        int entitiesSavedCount,
        CancellationToken cancellationToken = default)
    {
        var definition = CoreResources.LogSaveChangesCompleted(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, context.GetType().ShortDisplayName(), entitiesSavedCount);
        }

        if (diagnostics.NeedsEventData<ISaveChangesInterceptor>(
                definition,
                out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = CreateSaveChangesCompletedEventData(context, entitiesSavedCount, definition);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.SavedChangesAsync(eventData, entitiesSavedCount, cancellationToken);
            }
        }

        return ValueTask.FromResult(entitiesSavedCount);
    }

    private static SaveChangesCompletedEventData CreateSaveChangesCompletedEventData(
        DbContext context,
        int entitiesSavedCount,
        EventDefinition<string?, int> definition)
        => new(
            definition,
            SaveChangesCompleted,
            context,
            entitiesSavedCount);

    private static string SaveChangesCompleted(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string?, int>)definition;
        var p = (SaveChangesCompletedEventData)payload;
        return d.GenerateMessage(
            p.Context?.GetType().ShortDisplayName(),
            p.EntitiesSavedCount);
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.ContextDisposed" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="context">The context being used.</param>
    public static void ContextDisposed(
        this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
        DbContext context)
    {
        var definition = CoreResources.LogContextDisposed(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, context.GetType().ShortDisplayName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new DbContextEventData(
                definition,
                ContextDisposed,
                context);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ContextDisposed(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string?>)definition;
        var p = (DbContextEventData)payload;
        return d.GenerateMessage(p.Context?.GetType().ShortDisplayName());
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.ConflictingKeylessAndKeyAttributesWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="property">The property which is being defined as part of a key.</param>
    public static void ConflictingKeylessAndKeyAttributesWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
        IReadOnlyProperty property)
    {
        var definition = CoreResources.LogConflictingKeylessAndKeyAttributes(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, property.Name, property.DeclaringType.DisplayName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new PropertyEventData(
                definition,
                ConflictingKeylessAndKeyAttributesWarning,
                property);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ConflictingKeylessAndKeyAttributesWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (PropertyEventData)payload;
        return d.GenerateMessage(
            p.Property.Name,
            p.Property.DeclaringType.DisplayName());
    }

    /// <summary>
    ///     Logs for the <see cref="CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="foreignKey">Foreign key which is used in the incorrectly setup navigation.</param>
    public static void PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        IForeignKey foreignKey)
    {
        var definition = CoreResources.LogPossibleIncorrectRequiredNavigationWithQueryFilterInteraction(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                foreignKey.PrincipalEntityType.DisplayName(),
                foreignKey.DeclaringEntityType.DisplayName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new ForeignKeyEventData(
                definition,
                PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning,
                foreignKey);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning(
        EventDefinitionBase definition,
        EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (ForeignKeyEventData)payload;
        return d.GenerateMessage(
            p.ForeignKey.PrincipalEntityType.DisplayName(),
            p.ForeignKey.DeclaringEntityType.DisplayName());
    }
}
