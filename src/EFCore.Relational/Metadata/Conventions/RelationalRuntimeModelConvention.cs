// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that creates an optimized copy of the mutable model. This convention is typically
///     implemented by database providers to update provider annotations when creating a read-only model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class RelationalRuntimeModelConvention : RuntimeModelConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="RelationalRuntimeModelConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public RelationalRuntimeModelConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
        : base(dependencies)
    {
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <summary>
    ///     Updates the model annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="model">The source model.</param>
    /// <param name="runtimeModel">The target model that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected override void ProcessModelAnnotations(
        Dictionary<string, object?> annotations,
        IModel model,
        RuntimeModel runtimeModel,
        bool runtime)
    {
        base.ProcessModelAnnotations(annotations, model, runtimeModel, runtime);

        if (runtime)
        {
            annotations[RelationalAnnotationNames.RelationalModel] =
                RelationalModel.Create(
                    runtimeModel,
                    RelationalDependencies.RelationalAnnotationProvider,
                    (IRelationalTypeMappingSource)Dependencies.TypeMappingSource,
                    designTime: false);
        }
        else
        {
            annotations.Remove(RelationalAnnotationNames.Collation);

            if (annotations.TryGetValue(RelationalAnnotationNames.DbFunctions, out var functions))
            {
                var runtimeFunctions = new Dictionary<string, IDbFunction>(StringComparer.Ordinal);
                foreach (var (key, dbFunction) in (Dictionary<string, IDbFunction>)functions!)
                {
                    var runtimeFunction = Create(dbFunction, runtimeModel);
                    runtimeFunctions[key] = runtimeFunction;

                    foreach (var parameter in dbFunction.Parameters)
                    {
                        var runtimeParameter = Create(parameter, runtimeFunction);

                        CreateAnnotations(
                            parameter, runtimeParameter, static (convention, annotations, source, target, runtime) =>
                                convention.ProcessFunctionParameterAnnotations(annotations, source, target, runtime));
                    }

                    CreateAnnotations(
                        dbFunction, runtimeFunction, static (convention, annotations, source, target, runtime) =>
                            convention.ProcessFunctionAnnotations(annotations, source, target, runtime));
                }

                annotations[RelationalAnnotationNames.DbFunctions] = runtimeFunctions;
            }

            if (annotations.TryGetValue(RelationalAnnotationNames.Sequences, out var sequences))
            {
                var runtimeSequences = new Dictionary<(string, string?), ISequence>();
                foreach (var (key, value) in (Dictionary<(string, string?), ISequence>)sequences!)
                {
                    var runtimeSequence = Create(value, runtimeModel);
                    runtimeSequences[key] = runtimeSequence;

                    CreateAnnotations(
                        value, runtimeSequence, static (convention, annotations, source, target, runtime) =>
                            convention.ProcessSequenceAnnotations(annotations, source, target, runtime));
                }

                annotations[RelationalAnnotationNames.Sequences] = runtimeSequences;
            }
        }
    }

    /// <summary>
    ///     Updates the entity type annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="entityType">The source entity type.</param>
    /// <param name="runtimeEntityType">The target entity type that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected override void ProcessEntityTypeAnnotations(
        Dictionary<string, object?> annotations,
        IEntityType entityType,
        RuntimeEntityType runtimeEntityType,
        bool runtime)
    {
        base.ProcessEntityTypeAnnotations(annotations, entityType, runtimeEntityType, runtime);

        if (runtime)
        {
            annotations.Remove(RelationalAnnotationNames.TableMappings);
            annotations.Remove(RelationalAnnotationNames.ViewMappings);
            annotations.Remove(RelationalAnnotationNames.SqlQueryMappings);
            annotations.Remove(RelationalAnnotationNames.FunctionMappings);
            annotations.Remove(RelationalAnnotationNames.InsertStoredProcedureMappings);
            annotations.Remove(RelationalAnnotationNames.DeleteStoredProcedureMappings);
            annotations.Remove(RelationalAnnotationNames.UpdateStoredProcedureMappings);
            annotations.Remove(RelationalAnnotationNames.DefaultMappings);
        }
        else
        {
            annotations.Remove(RelationalAnnotationNames.CheckConstraints);
            annotations.Remove(RelationalAnnotationNames.Comment);
            annotations.Remove(RelationalAnnotationNames.IsTableExcludedFromMigrations);

            // These need to be set explicitly to prevent default values from being generated
            annotations[RelationalAnnotationNames.TableName] = entityType.GetTableName();
            annotations[RelationalAnnotationNames.Schema] = entityType.GetSchema();
            annotations[RelationalAnnotationNames.ViewName] = entityType.GetViewName();
            annotations[RelationalAnnotationNames.ViewSchema] = entityType.GetViewSchema();
            annotations[RelationalAnnotationNames.SqlQuery] = entityType.GetSqlQuery();
            annotations[RelationalAnnotationNames.FunctionName] = entityType.GetFunctionName();

            if (annotations.TryGetValue(RelationalAnnotationNames.MappingFragments, out var mappingFragments))
            {
                var entityTypeMappingFragment = (IReadOnlyStoreObjectDictionary<IEntityTypeMappingFragment>)mappingFragments!;
                var runtimeEntityTypeMappingFragment = new StoreObjectDictionary<RuntimeEntityTypeMappingFragment>();
                foreach (var fragment in entityTypeMappingFragment.GetValues())
                {
                    var runtimeMappingFragment = Create(fragment, runtimeEntityType);
                    runtimeEntityTypeMappingFragment.Add(fragment.StoreObject, runtimeMappingFragment);

                    CreateAnnotations(
                        fragment, runtimeMappingFragment,
                        static (convention, annotations, source, target, runtime) =>
                            convention.ProcessEntityTypeMappingFragmentAnnotations(annotations, source, target, runtime));
                }

                annotations[RelationalAnnotationNames.MappingFragments] = runtimeEntityTypeMappingFragment;
            }

            if (annotations.TryGetValue(RelationalAnnotationNames.InsertStoredProcedure, out var insertStoredProcedure))
            {
                var runtimeSproc = Create((IStoredProcedure)insertStoredProcedure!, runtimeEntityType);

                CreateAnnotations(
                    (IStoredProcedure)insertStoredProcedure!, runtimeSproc,
                    static (convention, annotations, source, target, runtime)
                        => convention.ProcessStoredProcedureAnnotations(annotations, source, target, runtime));

                annotations[RelationalAnnotationNames.InsertStoredProcedure] = runtimeSproc;
            }

            if (annotations.TryGetValue(RelationalAnnotationNames.DeleteStoredProcedure, out var deleteStoredProcedure))
            {
                var runtimeSproc = Create((IStoredProcedure)deleteStoredProcedure!, runtimeEntityType);

                CreateAnnotations(
                    (IStoredProcedure)deleteStoredProcedure!, runtimeSproc,
                    static (convention, annotations, source, target, runtime)
                        => convention.ProcessStoredProcedureAnnotations(annotations, source, target, runtime));

                annotations[RelationalAnnotationNames.DeleteStoredProcedure] = runtimeSproc;
            }

            if (annotations.TryGetValue(RelationalAnnotationNames.UpdateStoredProcedure, out var updateStoredProcedure))
            {
                var runtimeSproc = Create((IStoredProcedure)updateStoredProcedure!, runtimeEntityType);

                CreateAnnotations(
                    (IStoredProcedure)updateStoredProcedure!, runtimeSproc,
                    static (convention, annotations, source, target, runtime)
                        => convention.ProcessStoredProcedureAnnotations(annotations, source, target, runtime));

                annotations[RelationalAnnotationNames.UpdateStoredProcedure] = runtimeSproc;
            }
        }
    }

    /// <summary>
    ///     Updates the complex type annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="complexType">The source complex type.</param>
    /// <param name="runtimeComplexType">The target complex type that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected override void ProcessComplexTypeAnnotations(
        Dictionary<string, object?> annotations,
        IComplexType complexType,
        RuntimeComplexType runtimeComplexType,
        bool runtime)
    {
        base.ProcessComplexTypeAnnotations(annotations, complexType, runtimeComplexType, runtime);

        if (runtime)
        {
            annotations.Remove(RelationalAnnotationNames.TableMappings);
            annotations.Remove(RelationalAnnotationNames.ViewMappings);
            annotations.Remove(RelationalAnnotationNames.SqlQueryMappings);
            annotations.Remove(RelationalAnnotationNames.FunctionMappings);
            annotations.Remove(RelationalAnnotationNames.InsertStoredProcedureMappings);
            annotations.Remove(RelationalAnnotationNames.DeleteStoredProcedureMappings);
            annotations.Remove(RelationalAnnotationNames.UpdateStoredProcedureMappings);
            annotations.Remove(RelationalAnnotationNames.DefaultMappings);
        }
    }

    private static RuntimeEntityTypeMappingFragment Create(
        IEntityTypeMappingFragment entityTypeMappingFragment,
        RuntimeEntityType runtimeEntityType)
        => new(
            runtimeEntityType,
            entityTypeMappingFragment.StoreObject,
            entityTypeMappingFragment.IsTableExcludedFromMigrations);

    /// <summary>
    ///     Updates the relational property overrides annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="entityTypeMappingFragment">The source relational property overrides.</param>
    /// <param name="runtimeEntityTypeMappingFragment">The target relational property overrides that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected virtual void ProcessEntityTypeMappingFragmentAnnotations(
        Dictionary<string, object?> annotations,
        IEntityTypeMappingFragment entityTypeMappingFragment,
        RuntimeEntityTypeMappingFragment runtimeEntityTypeMappingFragment,
        bool runtime)
    {
    }

    private void CreateAnnotations<TSource, TTarget>(
        TSource source,
        TTarget target,
        Action<RelationalRuntimeModelConvention, Dictionary<string, object?>, TSource, TTarget, bool> process)
        where TSource : IAnnotatable
        where TTarget : AnnotatableBase
    {
        var annotations = source.GetAnnotations().ToDictionary(a => a.Name, a => a.Value);
        process(this, annotations, source, target, false);
        target.AddAnnotations(annotations);

        annotations = source.GetRuntimeAnnotations().ToDictionary(a => a.Name, a => a.Value);
        process(this, annotations, source, target, true);
        target.AddRuntimeAnnotations(annotations);
    }

    private static RuntimeDbFunction Create(IDbFunction function, RuntimeModel runtimeModel)
        => new(
            function.ModelName,
            runtimeModel,
            function.ReturnType,
            function.Name,
            function.Schema,
            function.StoreType,
            function.MethodInfo,
            function.IsScalar,
            function.IsAggregate,
            function.IsNullable,
            function.IsBuiltIn,
            function.TypeMapping,
            function.Translation);

    /// <summary>
    ///     Updates the function annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="function">The source function.</param>
    /// <param name="runtimeFunction">The target function that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected virtual void ProcessFunctionAnnotations(
        Dictionary<string, object?> annotations,
        IDbFunction function,
        RuntimeDbFunction runtimeFunction,
        bool runtime)
    {
    }

    private static RuntimeDbFunctionParameter Create(IDbFunctionParameter parameter, RuntimeDbFunction runtimeFunction)
        => runtimeFunction.AddParameter(
            parameter.Name,
            parameter.ClrType,
            parameter.PropagatesNullability,
            parameter.StoreType,
            parameter.TypeMapping);

    /// <summary>
    ///     Updates the function parameter annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="parameter">The source function parameter.</param>
    /// <param name="runtimeParameter">The target function parameter that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected virtual void ProcessFunctionParameterAnnotations(
        Dictionary<string, object?> annotations,
        IDbFunctionParameter parameter,
        RuntimeDbFunctionParameter runtimeParameter,
        bool runtime)
    {
    }

    private static RuntimeSequence Create(ISequence sequence, RuntimeModel runtimeModel)
        => new(
            sequence.Name,
            runtimeModel,
            sequence.Type,
            sequence.Schema,
            sequence.StartValue,
            sequence.IncrementBy,
            sequence.IsCyclic,
            sequence.MinValue,
            sequence.MaxValue,
            sequence.IsCached,
            sequence.CacheSize,
            sequence.ModelSchema is null);

    /// <summary>
    ///     Updates the sequence annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="sequence">The source sequence.</param>
    /// <param name="runtimeSequence">The target sequence that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected virtual void ProcessSequenceAnnotations(
        Dictionary<string, object?> annotations,
        ISequence sequence,
        RuntimeSequence runtimeSequence,
        bool runtime)
    {
    }

    /// <summary>
    ///     Updates the property annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="property">The source property.</param>
    /// <param name="runtimeProperty">The target property that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected override void ProcessPropertyAnnotations(
        Dictionary<string, object?> annotations,
        IProperty property,
        RuntimeProperty runtimeProperty,
        bool runtime)
    {
        base.ProcessPropertyAnnotations(annotations, property, runtimeProperty, runtime);

        if (runtime)
        {
            annotations.Remove(RelationalAnnotationNames.TableColumnMappings);
            annotations.Remove(RelationalAnnotationNames.ViewColumnMappings);
            annotations.Remove(RelationalAnnotationNames.SqlQueryColumnMappings);
            annotations.Remove(RelationalAnnotationNames.FunctionColumnMappings);
            annotations.Remove(RelationalAnnotationNames.InsertStoredProcedureParameterMappings);
            annotations.Remove(RelationalAnnotationNames.InsertStoredProcedureResultColumnMappings);
            annotations.Remove(RelationalAnnotationNames.DeleteStoredProcedureParameterMappings);
            annotations.Remove(RelationalAnnotationNames.UpdateStoredProcedureParameterMappings);
            annotations.Remove(RelationalAnnotationNames.UpdateStoredProcedureResultColumnMappings);
            annotations.Remove(RelationalAnnotationNames.DefaultColumnMappings);
        }
        else
        {
            annotations.Remove(RelationalAnnotationNames.ColumnOrder);
            annotations.Remove(RelationalAnnotationNames.Comment);
            annotations.Remove(RelationalAnnotationNames.Collation);

            if (annotations.TryGetValue(RelationalAnnotationNames.RelationalOverrides, out var relationalOverrides))
            {
                var tableOverrides = (IReadOnlyStoreObjectDictionary<IRelationalPropertyOverrides>)relationalOverrides!;
                var runtimeTableOverrides = new StoreObjectDictionary<RuntimeRelationalPropertyOverrides>();
                foreach (var overrides in tableOverrides.GetValues())
                {
                    var runtimeOverrides = Create(overrides, runtimeProperty);
                    runtimeTableOverrides.Add(overrides.StoreObject, runtimeOverrides);

                    CreateAnnotations(
                        overrides, runtimeOverrides,
                        static (convention, annotations, source, target, runtime) =>
                            convention.ProcessPropertyOverridesAnnotations(annotations, source, target, runtime));
                }

                annotations[RelationalAnnotationNames.RelationalOverrides] = runtimeTableOverrides;
            }
        }
    }

    private static RuntimeRelationalPropertyOverrides Create(
        IRelationalPropertyOverrides propertyOverrides,
        RuntimeProperty runtimeProperty)
        => new(
            runtimeProperty,
            propertyOverrides.StoreObject,
            propertyOverrides.IsColumnNameOverridden,
            propertyOverrides.ColumnName);

    /// <summary>
    ///     Updates the relational property overrides annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="propertyOverrides">The source relational property overrides.</param>
    /// <param name="runtimePropertyOverrides">The target relational property overrides that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected virtual void ProcessPropertyOverridesAnnotations(
        Dictionary<string, object?> annotations,
        IRelationalPropertyOverrides propertyOverrides,
        RuntimeRelationalPropertyOverrides runtimePropertyOverrides,
        bool runtime)
    {
    }

    /// <summary>
    ///     Updates the key annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="key">The source key.</param>
    /// <param name="runtimeKey">The target key that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected override void ProcessKeyAnnotations(
        Dictionary<string, object?> annotations,
        IKey key,
        RuntimeKey runtimeKey,
        bool runtime)
    {
        base.ProcessKeyAnnotations(annotations, key, runtimeKey, runtime);

        if (runtime)
        {
            annotations.Remove(RelationalAnnotationNames.UniqueConstraintMappings);
        }
    }

    /// <summary>
    ///     Updates the index annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="index">The source index.</param>
    /// <param name="runtimeIndex">The target index that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected override void ProcessIndexAnnotations(
        Dictionary<string, object?> annotations,
        IIndex index,
        RuntimeIndex runtimeIndex,
        bool runtime)
    {
        base.ProcessIndexAnnotations(annotations, index, runtimeIndex, runtime);

        if (runtime)
        {
            annotations.Remove(RelationalAnnotationNames.TableIndexMappings);
        }
    }

    /// <summary>
    ///     Updates the foreign key annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="foreignKey">The source foreign key.</param>
    /// <param name="runtimeForeignKey">The target foreign key that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected override void ProcessForeignKeyAnnotations(
        Dictionary<string, object?> annotations,
        IForeignKey foreignKey,
        RuntimeForeignKey runtimeForeignKey,
        bool runtime)
    {
        base.ProcessForeignKeyAnnotations(annotations, foreignKey, runtimeForeignKey, runtime);

        if (runtime)
        {
            annotations.Remove(RelationalAnnotationNames.ForeignKeyMappings);
        }
    }

    private RuntimeStoredProcedure Create(IStoredProcedure storedProcedure, RuntimeEntityType runtimeEntityType)
    {
        var runtimeStoredProcedure = new RuntimeStoredProcedure(
            runtimeEntityType,
            storedProcedure.Name,
            storedProcedure.Schema,
            storedProcedure.IsRowsAffectedReturned);

        foreach (var parameter in storedProcedure.Parameters)
        {
            var runtimeParameter = Create(parameter, runtimeStoredProcedure);
            CreateAnnotations(
                parameter, runtimeParameter, static (convention, annotations, source, target, runtime) =>
                    convention.ProcessStoredProcedureParameterAnnotations(annotations, source, target, runtime));
        }

        foreach (var resultColumn in storedProcedure.ResultColumns)
        {
            var runtimeResultColumn = Create(resultColumn, runtimeStoredProcedure);
            CreateAnnotations(
                resultColumn, runtimeResultColumn, static (convention, annotations, source, target, runtime) =>
                    convention.ProcessStoredProcedureResultColumnAnnotations(annotations, source, target, runtime));
        }

        return runtimeStoredProcedure;
    }

    private RuntimeStoredProcedureParameter Create(
        IStoredProcedureParameter parameter,
        RuntimeStoredProcedure runtimeStoredProcedure)
        => runtimeStoredProcedure.AddParameter(
            parameter.Name,
            parameter.Direction,
            parameter.ForRowsAffected,
            parameter.PropertyName,
            parameter.ForOriginalValue);

    private RuntimeStoredProcedureResultColumn Create(
        IStoredProcedureResultColumn resultColumn,
        RuntimeStoredProcedure runtimeStoredProcedure)
        => runtimeStoredProcedure.AddResultColumn(
            resultColumn.Name,
            resultColumn.ForRowsAffected,
            resultColumn.PropertyName);

    /// <summary>
    ///     Updates the stored procedure annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="storedProcedure">The source stored procedure.</param>
    /// <param name="runtimeStoredProcedure">The target stored procedure that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected virtual void ProcessStoredProcedureAnnotations(
        Dictionary<string, object?> annotations,
        IStoredProcedure storedProcedure,
        RuntimeStoredProcedure runtimeStoredProcedure,
        bool runtime)
    {
    }

    /// <summary>
    ///     Updates the stored procedure parameter annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="parameter">The source stored procedure parameter.</param>
    /// <param name="runtimeParameter">The target stored procedure parameter that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected virtual void ProcessStoredProcedureParameterAnnotations(
        Dictionary<string, object?> annotations,
        IStoredProcedureParameter parameter,
        RuntimeStoredProcedureParameter runtimeParameter,
        bool runtime)
    {
    }

    /// <summary>
    ///     Updates the stored procedure result column annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="resultColumn">The source fstored procedure result column.</param>
    /// <param name="runtimeResultColumn">The target stored procedure result column that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected virtual void ProcessStoredProcedureResultColumnAnnotations(
        Dictionary<string, object?> annotations,
        IStoredProcedureResultColumn resultColumn,
        RuntimeStoredProcedureResultColumn runtimeResultColumn,
        bool runtime)
    {
    }
}
