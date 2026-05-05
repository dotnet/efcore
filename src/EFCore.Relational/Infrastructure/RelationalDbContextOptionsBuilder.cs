// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Allows relational database specific configuration to be performed on <see cref="DbContextOptions" />.
/// </summary>
/// <remarks>
///     <para>
///         Instances of this class are typically returned from methods that configure the context to use a
///         particular relational database provider.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> for more information and examples.
///     </para>
/// </remarks>
public abstract class RelationalDbContextOptionsBuilder<TBuilder, TExtension> : IRelationalDbContextOptionsBuilderInfrastructure
    where TBuilder : RelationalDbContextOptionsBuilder<TBuilder, TExtension>
    where TExtension : RelationalOptionsExtension, new()
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationalDbContextOptionsBuilder{TBuilder, TExtension}" /> class.
    /// </summary>
    /// <param name="optionsBuilder">The core options builder.</param>
    protected RelationalDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
        => OptionsBuilder = optionsBuilder;

    /// <summary>
    ///     Gets the core options builder.
    /// </summary>
    protected virtual DbContextOptionsBuilder OptionsBuilder { get; }

    /// <inheritdoc />
    DbContextOptionsBuilder IRelationalDbContextOptionsBuilderInfrastructure.OptionsBuilder
        => OptionsBuilder;

    /// <summary>
    ///     Configures the maximum number of statements that will be included in commands sent to the database
    ///     during <see cref="DbContext.SaveChanges()" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-saving-data">Saving data with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="maxBatchSize">The maximum number of statements.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public virtual TBuilder MaxBatchSize(int maxBatchSize)
        => WithOption(e => (TExtension)e.WithMaxBatchSize(maxBatchSize));

    /// <summary>
    ///     Configures the minimum number of statements that are needed for a multi-statement command sent to the database
    ///     during <see cref="DbContext.SaveChanges()" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-saving-data">Saving data with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="minBatchSize">The minimum number of statements.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public virtual TBuilder MinBatchSize(int minBatchSize)
        => WithOption(e => (TExtension)e.WithMinBatchSize(minBatchSize));

    /// <summary>
    ///     Configures the wait time (in seconds) before terminating the attempt to execute a command and generating an error.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This sets the <see cref="DbCommand.CommandTimeout" /> property on the ADO.NET provider being used.
    ///     </para>
    ///     <para>
    ///         An <see cref="ArgumentException" /> is generated if <paramref name="commandTimeout" /> value is less than 0.
    ///     </para>
    ///     <para>
    ///         Zero (0) typically means no timeout will be applied, consult your ADO.NET provider documentation.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-connections">Connections and connection strings</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="commandTimeout">The time in seconds to wait for the command to execute.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public virtual TBuilder CommandTimeout(int? commandTimeout)
        => WithOption(e => (TExtension)e.WithCommandTimeout(commandTimeout));

    /// <summary>
    ///     Configures the assembly where migrations are maintained for this context.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    /// <param name="assemblyName">The name of the assembly.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public virtual TBuilder MigrationsAssembly(string? assemblyName)
        => WithOption(e => (TExtension)e.WithMigrationsAssembly(Check.NullButNotEmpty(assemblyName, nameof(assemblyName))));

    /// <summary>
    ///     Configures the assembly where migrations are maintained for this context.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    /// <param name="assembly">The <see cref="Assembly" /> where the migrations are located.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public virtual TBuilder MigrationsAssembly(Assembly assembly)
        => WithOption(e => (TExtension)e.WithMigrationsAssembly(assembly));

    /// <summary>
    ///     Configures the name of the table used to record which migrations have been applied to the database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schema">The schema of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public virtual TBuilder MigrationsHistoryTable(string tableName, string? schema = null)
    {
        Check.NotEmpty(tableName, nameof(tableName));
        Check.NullButNotEmpty(schema, nameof(schema));

        return WithOption(e => (TExtension)e.WithMigrationsHistoryTableName(tableName).WithMigrationsHistoryTableSchema(schema));
    }

    /// <summary>
    ///     Configures the context to use relational database semantics when comparing null values. By default,
    ///     Entity Framework will use C# semantics for null values, and generate SQL to compensate for differences
    ///     in how the database handles nulls.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-relational-nulls">Relational database null semantics</see> for more information and examples.
    /// </remarks>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public virtual TBuilder UseRelationalNulls(bool useRelationalNulls = true)
        => WithOption(e => (TExtension)e.WithUseRelationalNulls(useRelationalNulls));

    /// <summary>
    ///     Configures the <see cref="QuerySplittingBehavior" /> to use when loading related collections in a query.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-split-queries">EF Core split queries</see> for more information and examples.
    /// </remarks>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public virtual TBuilder UseQuerySplittingBehavior(QuerySplittingBehavior querySplittingBehavior)
        => WithOption(e => (TExtension)e.WithUseQuerySplittingBehavior(querySplittingBehavior));

    /// <summary>
    ///     Configures the context to use the provided <see cref="IExecutionStrategy" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="getExecutionStrategy">A function that returns a new instance of an execution strategy.</param>
    public virtual TBuilder ExecutionStrategy(
        Func<ExecutionStrategyDependencies, IExecutionStrategy> getExecutionStrategy)
        => WithOption(
            e => (TExtension)e.WithExecutionStrategyFactory(Check.NotNull(getExecutionStrategy, nameof(getExecutionStrategy))));

    /// <summary>
    ///     Configures the context to translate parameterized collections to inline constants.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When a LINQ query contains a parameterized collection, by default EF Core parameterizes the entire collection as a single
    ///         SQL parameter, if possible. For example, on SQL Server, the LINQ query <c>Where(b => ids.Contains(b.Id)</c> is translated to
    ///         <c>WHERE [b].[Id] IN (SELECT [i].[value] FROM OPENJSON(@__ids_0) ...)</c>. While this helps with query plan caching, it can
    ///         produce worse query plans for certain query types.
    ///     </para>
    ///     <para>
    ///         <see cref="TranslateParameterizedCollectionsToConstants" /> instructs EF to translate the collection to a set of constants:
    ///         <c>WHERE [b].[Id] IN (1, 2, 3)</c>. This can produce better query plans for certain query types, but can also lead to query
    ///         plan bloat.
    ///     </para>
    ///     <para>
    ///         Note that it's possible to cause EF to translate a specific collection in a specific query to constants by wrapping the
    ///         parameterized collection in <see cref="EF.Constant{T}" />: <c>Where(b => EF.Constant(ids).Contains(b.Id)</c>. This overrides
    ///         the default. Likewise, you can translate a specific collection in a specific query to a single parameter by wrapping the
    ///         parameterized collection in <see cref="EF.Parameter{T}(T)" />: <c>Where(b => EF.Parameter(ids).Contains(b.Id)</c>. This
    ///         overrides the <see cref="TranslateParameterizedCollectionsToConstants" /> setting.
    ///     </para>
    /// </remarks>
    public virtual TBuilder TranslateParameterizedCollectionsToConstants()
        => WithOption(e => (TExtension)e.WithParameterizedCollectionTranslationMode(ParameterizedCollectionTranslationMode.Constantize));

    /// <summary>
    ///     Configures the context to translate parameterized collections to parameters.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When a LINQ query contains a parameterized collection, by default EF Core parameterizes the entire collection as a single
    ///         SQL parameter, if possible. For example, on SQL Server, the LINQ query <c>Where(b => ids.Contains(b.Id)</c> is translated to
    ///         <c>WHERE [b].[Id] IN (SELECT [i].[value] FROM OPENJSON(@__ids_0) ...)</c>. While this helps with query plan caching, it can
    ///         produce worse query plans for certain query types.
    ///     </para>
    ///     <para>
    ///         <see cref="TranslateParameterizedCollectionsToParameters" /> explicitly instructs EF to perform the default translation
    ///         of parameterized collections, which is translating them to parameters.
    ///     </para>
    ///     <para>
    ///         Note that it's possible to cause EF to translate a specific collection in a specific query to constants by wrapping the
    ///         parameterized collection in <see cref="EF.Constant{T}" />: <c>Where(b => EF.Constant(ids).Contains(b.Id)</c>. This overrides
    ///         the default.
    ///     </para>
    /// </remarks>
    public virtual TBuilder TranslateParameterizedCollectionsToParameters()
        => WithOption(e => (TExtension)e.WithParameterizedCollectionTranslationMode(ParameterizedCollectionTranslationMode.Parameterize));

    /// <summary>
    ///     Sets an option by cloning the extension used to store the settings. This ensures the builder
    ///     does not modify options that are already in use elsewhere.
    /// </summary>
    /// <param name="setAction">An action to set the option.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    protected virtual TBuilder WithOption(Func<TExtension, TExtension> setAction)
    {
        ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(
            setAction(OptionsBuilder.Options.FindExtension<TExtension>() ?? new TExtension()));

        return (TBuilder)this;
    }

    #region Hidden System.Object members

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString()
        => base.ToString();

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}
