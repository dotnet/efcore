// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     A base class inherited by database providers that gives access to annotations
///     used by relational EF Core components on various elements of the <see cref="IReadOnlyModel" />.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
///         examples.
///     </para>
/// </remarks>
public class RelationalAnnotationProvider : IRelationalAnnotationProvider
{
    /// <summary>
    ///     Initializes a new instance of this class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public RelationalAnnotationProvider(RelationalAnnotationProviderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalAnnotationProviderDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IRelationalModel model, bool designTime)
        => Enumerable.Empty<IAnnotation>();

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(ITable table, bool designTime)
        => Enumerable.Empty<IAnnotation>();

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IColumn column, bool designTime)
        => Enumerable.Empty<IAnnotation>();

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IView view, bool designTime)
        => Enumerable.Empty<IAnnotation>();

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IViewColumn column, bool designTime)
        => Enumerable.Empty<IAnnotation>();

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(ISqlQuery sqlQuery, bool designTime)
        => Enumerable.Empty<IAnnotation>();

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(ISqlQueryColumn column, bool designTime)
        => Enumerable.Empty<IAnnotation>();

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IStoreFunction function, bool designTime)
        => Enumerable.Empty<IAnnotation>();

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IStoreFunctionParameter parameter, bool designTime)
        => Enumerable.Empty<IAnnotation>();

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IFunctionColumn column, bool designTime)
        => Enumerable.Empty<IAnnotation>();

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IStoreStoredProcedure storedProcedure, bool designTime)
        => Enumerable.Empty<IAnnotation>();

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IStoreStoredProcedureParameter parameter, bool designTime)
        => Enumerable.Empty<IAnnotation>();

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IStoreStoredProcedureResultColumn column, bool designTime)
        => Enumerable.Empty<IAnnotation>();

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IForeignKeyConstraint foreignKey, bool designTime)
        => Enumerable.Empty<IAnnotation>();

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(ITableIndex index, bool designTime)
        => Enumerable.Empty<IAnnotation>();

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IUniqueConstraint constraint, bool designTime)
        => Enumerable.Empty<IAnnotation>();

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(ISequence sequence, bool designTime)
        => Enumerable.Empty<IAnnotation>();

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(ICheckConstraint checkConstraint, bool designTime)
        => Enumerable.Empty<IAnnotation>();

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(ITrigger trigger, bool designTime)
        => Enumerable.Empty<IAnnotation>();
}
