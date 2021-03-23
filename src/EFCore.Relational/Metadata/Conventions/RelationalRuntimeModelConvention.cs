// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that creates an optimized copy of the mutable model. This convention is typically
    ///     implemented by database providers to update provider annotations when creating a read-only model.
    /// </summary>
    public class RelationalRuntimeModelConvention : RuntimeModelConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RelationalModelConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public RelationalRuntimeModelConvention(
            ProviderConventionSetBuilderDependencies dependencies,
            RelationalConventionSetBuilderDependencies relationalDependencies)
            : base(dependencies)
        {
            RelationalDependencies = relationalDependencies;
        }

        /// <summary>
        ///     The service dependencies for <see cref="RelationalConventionSetBuilder" />
        /// </summary>
        protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

        /// <summary>
        ///     Updates the model annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="model"> The source model. </param>
        /// <param name="runtimeModel"> The target model that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected override void ProcessModelAnnotations(
            Dictionary<string, object?> annotations, IModel model, RuntimeModel runtimeModel, bool runtime)
        {
            base.ProcessModelAnnotations(annotations, model, runtimeModel, runtime);

            if (runtime)
            {
                annotations[RelationalAnnotationNames.RelationalModel] =
                    RelationalModel.Create(runtimeModel, RelationalDependencies.RelationalAnnotationProvider);
            }
            else
            {
                if (annotations.TryGetValue(RelationalAnnotationNames.DbFunctions, out var functions))
                {
                    var runtimeFunctions = new SortedDictionary<string, IDbFunction>();
                    foreach (var functionPair in (SortedDictionary<string, IDbFunction>)functions!)
                    {
                        var runtimeFunction = Create(functionPair.Value, runtimeModel);
                        runtimeFunctions[functionPair.Key] = runtimeFunction;

                        foreach (var parameter in functionPair.Value.Parameters)
                        {
                            var runtimeParameter = Create(parameter, runtimeFunction);

                            CreateAnnotations(parameter, runtimeParameter, static (convention, annotations, source, target, runtime) =>
                                convention.ProcessFunctionParameterAnnotations(annotations, source, target, runtime));
                        }

                        CreateAnnotations(functionPair.Value, runtimeFunction, static (convention, annotations, source, target, runtime) =>
                            convention.ProcessFunctionAnnotations(annotations, source, target, runtime));
                    }

                    annotations[RelationalAnnotationNames.DbFunctions] = runtimeFunctions;
                }

                if (annotations.TryGetValue(RelationalAnnotationNames.Sequences, out var sequences))
                {
                    var runtimeSequences = new SortedDictionary<(string, string?), ISequence>();
                    foreach (var sequencePair in (SortedDictionary<(string, string?), ISequence>)sequences!)
                    {
                        var runtimeSequence = Create(sequencePair.Value, runtimeModel);
                        runtimeSequences[sequencePair.Key] = runtimeSequence;

                        CreateAnnotations(sequencePair.Value, runtimeSequence, static (convention, annotations, source, target, runtime) =>
                            convention.ProcessSequenceAnnotations(annotations, source, target, runtime));
                    }

                    annotations[RelationalAnnotationNames.Sequences] = runtimeSequences;
                }
            }
        }

        /// <summary>
        ///     Updates the entity type annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="entityType"> The source entity type. </param>
        /// <param name="runtimeEntityType"> The target entity type that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected override void ProcessEntityTypeAnnotations(
            IDictionary<string, object?> annotations, IEntityType entityType, RuntimeEntityType runtimeEntityType, bool runtime)
        {
            base.ProcessEntityTypeAnnotations(annotations, entityType, runtimeEntityType, runtime);

            if (runtime)
            {
                annotations.Remove(RelationalAnnotationNames.TableMappings);
                annotations.Remove(RelationalAnnotationNames.ViewMappings);
                annotations.Remove(RelationalAnnotationNames.SqlQueryMappings);
                annotations.Remove(RelationalAnnotationNames.FunctionMappings);
                annotations.Remove(RelationalAnnotationNames.DefaultMappings);
            }
            else
            {
                if (annotations.TryGetValue(RelationalAnnotationNames.CheckConstraints, out var constraints))
                {
                    var runtimeCheckConstraints = new Dictionary<string, ICheckConstraint>();
                    foreach (var constraintPair in (Dictionary<string, ICheckConstraint>?)constraints!)
                    {
                        var runtimeCheckConstraint = Create(constraintPair.Value, runtimeEntityType);
                        runtimeCheckConstraints[constraintPair.Key] = runtimeCheckConstraint;

                        CreateAnnotations(constraintPair.Value, runtimeCheckConstraint, static (convention, annotations, source, target, runtime) =>
                            convention.ProcessCheckConstraintAnnotations(annotations, source, target, runtime));
                    }

                    annotations[RelationalAnnotationNames.CheckConstraints] = runtimeCheckConstraints;
                }

                // These need to be set explicitly to prevent default values from being generated
                annotations[RelationalAnnotationNames.TableName] = entityType.GetTableName();
                annotations[RelationalAnnotationNames.Schema] = entityType.GetSchema();
                annotations[RelationalAnnotationNames.ViewName] = entityType.GetViewName();
                annotations[RelationalAnnotationNames.ViewSchema] = entityType.GetViewSchema();
                annotations[RelationalAnnotationNames.SqlQuery] = entityType.GetSqlQuery();
                annotations[RelationalAnnotationNames.FunctionName] = entityType.GetFunctionName();
            }
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

        private RuntimeDbFunction Create(IDbFunction function, RuntimeModel runtimeModel)
            => new RuntimeDbFunction(
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
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="function"> The source function. </param>
        /// <param name="runtimeFunction"> The target function that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessFunctionAnnotations(
            Dictionary<string, object?> annotations,
            IDbFunction function,
            RuntimeDbFunction runtimeFunction,
            bool runtime)
        {
        }

        private RuntimeDbFunctionParameter Create(IDbFunctionParameter parameter, RuntimeDbFunction runtimeFunction)
            => runtimeFunction.AddParameter(
                parameter.Name,
                parameter.ClrType,
                parameter.PropagatesNullability,
                parameter.StoreType,
                parameter.TypeMapping);

        /// <summary>
        ///     Updates the function parameter annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="parameter"> The source function parameter. </param>
        /// <param name="runtimeParameter"> The target function parameter that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessFunctionParameterAnnotations(
            Dictionary<string, object?> annotations,
            IDbFunctionParameter parameter,
            RuntimeDbFunctionParameter runtimeParameter,
            bool runtime)
        {
        }

        private RuntimeSequence Create(ISequence sequence, RuntimeModel runtimeModel)
            => new RuntimeSequence(
                sequence.Name,
                runtimeModel,
                sequence.Type,
                sequence.StartValue,
                sequence.IncrementBy,
                sequence.Schema,
                sequence.IsCyclic,
                sequence.MinValue,
                sequence.MaxValue);

        /// <summary>
        ///     Updates the sequence annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="sequence"> The source sequence. </param>
        /// <param name="runtimeSequence"> The target sequence that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessSequenceAnnotations(
            Dictionary<string, object?> annotations,
            ISequence sequence,
            RuntimeSequence runtimeSequence,
            bool runtime)
        {
        }

        private RuntimeCheckConstraint Create(ICheckConstraint checkConstraint, RuntimeEntityType runtimeEntityType)
            => new RuntimeCheckConstraint(
                checkConstraint.Name,
                runtimeEntityType,
                checkConstraint.Sql);

        /// <summary>
        ///     Updates the check constraint annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="checkConstraint"> The source check constraint. </param>
        /// <param name="runtimeCheckConstraint"> The target check constraint that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessCheckConstraintAnnotations(
            Dictionary<string, object?> annotations,
            ICheckConstraint checkConstraint,
            RuntimeCheckConstraint runtimeCheckConstraint,
            bool runtime)
        {
        }

        /// <summary>
        ///     Updates the property annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="property"> The source property. </param>
        /// <param name="runtimeProperty"> The target property that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected override void ProcessPropertyAnnotations(
            Dictionary<string, object?> annotations, IProperty property, RuntimeProperty runtimeProperty, bool runtime)
        {
            base.ProcessPropertyAnnotations(annotations, property, runtimeProperty, runtime);

            if (runtime)
            {
                annotations.Remove(RelationalAnnotationNames.TableColumnMappings);
                annotations.Remove(RelationalAnnotationNames.ViewColumnMappings);
                annotations.Remove(RelationalAnnotationNames.SqlQueryColumnMappings);
                annotations.Remove(RelationalAnnotationNames.FunctionColumnMappings);
                annotations.Remove(RelationalAnnotationNames.DefaultColumnMappings);
            }
            else
            {
                if (annotations.TryGetValue(RelationalAnnotationNames.RelationalOverrides, out var overrides))
                {
                    var runtimePropertyOverrides = new SortedDictionary<StoreObjectIdentifier, object>();
                    foreach (var overridesPair in (SortedDictionary<StoreObjectIdentifier, object>?)overrides!)
                    {
                        var runtimeOverrides = Create((IRelationalPropertyOverrides)overridesPair.Value, slimProperty);
                        slimPropertyOverrides[overridesPair.Key] = slimOverrides;

                        CreateAnnotations((IRelationalPropertyOverrides)overridesPair.Value, runtimeOverrides,
                            static (convention, annotations, source, target, runtime) =>
                                convention.ProcessPropertyOverridesAnnotations(annotations, source, target, runtime));
                    }

                    annotations[RelationalAnnotationNames.RelationalOverrides] = runtimePropertyOverrides;
                }
            }
        }

        private RuntimeRelationalPropertyOverrides Create(
            IRelationalPropertyOverrides propertyOverrides,
            RuntimeProperty runtimeProperty)
            => new RuntimeRelationalPropertyOverrides(
                runtimeProperty,
                propertyOverrides.ColumnNameOverriden,
                propertyOverrides.ColumnName);

        /// <summary>
        ///     Updates the relational property overrides annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="propertyOverrides"> The source relational property overrides. </param>
        /// <param name="runtimePropertyOverrides"> The target relational property overrides that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
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
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="key"> The source key. </param>
        /// <param name="runtimeKey"> The target key that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected override void ProcessKeyAnnotations(
            IDictionary<string, object?> annotations,
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
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="index"> The source index. </param>
        /// <param name="runtimeIndex"> The target index that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
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
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="foreignKey"> The source foreign key. </param>
        /// <param name="runtimeForeignKey"> The target foreign key that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
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
    }
}
