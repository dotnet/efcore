// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        => Dependencies = dependencies;

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalAnnotationProviderDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IRelationalModel model, bool designTime)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(ITable table, bool designTime)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IColumn column, bool designTime)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IView view, bool designTime)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IViewColumn column, bool designTime)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(ISqlQuery sqlQuery, bool designTime)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(ISqlQueryColumn column, bool designTime)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IStoreFunction function, bool designTime)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IStoreFunctionParameter parameter, bool designTime)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IFunctionColumn column, bool designTime)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IStoreStoredProcedure storedProcedure, bool designTime)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IStoreStoredProcedureParameter parameter, bool designTime)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IStoreStoredProcedureResultColumn column, bool designTime)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IForeignKeyConstraint foreignKey, bool designTime)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(ITableIndex index, bool designTime)
    {
        IIndex? modelIndex = null;
        foreach (var mappedIndex in index.MappedIndexes)
        {
            if (mappedIndex.IsJsonIndex())
            {
                modelIndex = mappedIndex;
                break;
            }
        }

        if (!designTime
            || modelIndex is null)
        {
            yield break;
        }

        yield return new Annotation(RelationalAnnotationNames.JsonIndex, CreateJsonIndex(modelIndex));
    }

    /// <summary>
    ///     Builds a <see cref="RelationalJsonIndex" /> for the given mapped JSON index.
    /// </summary>
    /// <remarks>
    ///     Providers can override this to customize JSON index element resolution. The base
    ///     implementation handles indexes whose leaves are either scalar properties inside JSON-mapped
    ///     complex types, or non-collection complex properties whose type is itself JSON-mapped. When
    ///     overriding, use <see cref="FindJsonElement" /> to resolve the JSON element for an individual
    ///     property on the index's table.
    /// </remarks>
    /// <param name="modelIndex">The mapped JSON index.</param>
    /// <returns>The <see cref="RelationalJsonIndex" /> describing the JSON paths.</returns>
    protected virtual RelationalJsonIndex CreateJsonIndex(IIndex modelIndex)
    {
        var tableIndex = modelIndex.GetMappedTableIndexes().First();
        var elements = new IRelationalJsonElement[modelIndex.Properties.Count];
        for (var i = 0; i < modelIndex.Properties.Count; i++)
        {
            elements[i] = FindJsonElement(modelIndex.Properties[i], tableIndex.Table);
        }

        return new RelationalJsonIndex(elements, modelIndex.CollectionIndices);
    }

    /// <summary>
    ///     Resolves the <see cref="IRelationalJsonElement" /> for the given property on the given table.
    ///     All JSON element mappings are populated before table-index annotations are gathered, so a
    ///     mapping is expected to exist for any property reaching this code path.
    /// </summary>
    /// <param name="property">The property (scalar or complex) participating in the index.</param>
    /// <param name="table">The table containing the index.</param>
    /// <returns>The JSON element on the given table.</returns>
    protected static IRelationalJsonElement FindJsonElement(IPropertyBase property, ITable table)
    {
        // Read the JsonElementMappings runtime annotation directly: GetJsonElementMappings() would
        // call EnsureRelationalModel, recursively re-entering RelationalModel.Create.
        var mappings = (IEnumerable<IJsonElementMapping>?)property.FindRuntimeAnnotationValue(
            RelationalAnnotationNames.JsonElementMappings)
            ?? throw new UnreachableException($"Missing JSON element mappings for property '{property.Name}'.");
        foreach (var mapping in mappings)
        {
            if (mapping.TableMapping.Table == table)
            {
                return mapping.Element;
            }
        }

        throw new UnreachableException($"No JSON element mapping for property '{property.Name}' on table '{table.Name}'.");
    }

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(IUniqueConstraint constraint, bool designTime)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(ISequence sequence, bool designTime)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(ICheckConstraint checkConstraint, bool designTime)
        => [];

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> For(ITrigger trigger, bool designTime)
        => [];
}
