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
    public class RelationalSlimModelConvention : SlimModelConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RelationalModelConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public RelationalSlimModelConvention(
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
        /// <param name="slimModel"> The target model that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected override void ProcessModelAnnotations(
            Dictionary<string, object?> annotations, IModel model, SlimModel slimModel, bool runtime)
        {
            base.ProcessModelAnnotations(annotations, model, slimModel, runtime);

            if (runtime)
            {
                annotations[RelationalAnnotationNames.RelationalModel] =
                    RelationalModel.Create(slimModel, RelationalDependencies.RelationalAnnotationProvider);
            }
            else
            {
                if (annotations.TryGetValue(RelationalAnnotationNames.DbFunctions, out var functions))
                {
                    var slimFunctions = new SortedDictionary<string, IDbFunction>();
                    foreach (var functionPair in (SortedDictionary<string, IDbFunction>)functions!)
                    {
                        var slimFunction = Create(functionPair.Value, slimModel);
                        slimFunctions[functionPair.Key] = slimFunction;

                        foreach (var parameter in functionPair.Value.Parameters)
                        {
                            var slimParameter = Create(parameter, slimFunction);

                            CreateAnnotations(parameter, slimParameter, static (convention, annotations, source, target, runtime) =>
                                convention.ProcessFunctionParameterAnnotations(annotations, source, target, runtime));
                        }

                        CreateAnnotations(functionPair.Value, slimFunction, static (convention, annotations, source, target, runtime) =>
                            convention.ProcessFunctionAnnotations(annotations, source, target, runtime));
                    }

                    annotations[RelationalAnnotationNames.DbFunctions] = slimFunctions;
                }

                if (annotations.TryGetValue(RelationalAnnotationNames.Sequences, out var sequences))
                {
                    var slimSequences = new SortedDictionary<(string, string?), ISequence>();
                    foreach (var sequencePair in (SortedDictionary<(string, string?), ISequence>)sequences!)
                    {
                        var slimSequence = Create(sequencePair.Value, slimModel);
                        slimSequences[sequencePair.Key] = slimSequence;

                        CreateAnnotations(sequencePair.Value, slimSequence, static (convention, annotations, source, target, runtime) =>
                            convention.ProcessSequenceAnnotations(annotations, source, target, runtime));
                    }

                    annotations[RelationalAnnotationNames.Sequences] = slimSequences;
                }
            }
        }

        /// <summary>
        ///     Updates the entity type annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="entityType"> The source entity type. </param>
        /// <param name="slimEntityType"> The target entity type that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected override void ProcessEntityTypeAnnotations(
            IDictionary<string, object?> annotations, IEntityType entityType, SlimEntityType slimEntityType, bool runtime)
        {
            base.ProcessEntityTypeAnnotations(annotations, entityType, slimEntityType, runtime);

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
                    var slimCheckConstraints = new Dictionary<string, ICheckConstraint>();
                    foreach (var constraintPair in (Dictionary<string, ICheckConstraint>?)constraints!)
                    {
                        var slimCheckConstraint = Create(constraintPair.Value, slimEntityType);
                        slimCheckConstraints[constraintPair.Key] = slimCheckConstraint;

                        CreateAnnotations(constraintPair.Value, slimCheckConstraint, static (convention, annotations, source, target, runtime) =>
                            convention.ProcessCheckConstraintAnnotations(annotations, source, target, runtime));
                    }

                    annotations[RelationalAnnotationNames.CheckConstraints] = slimCheckConstraints;
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
            Action<RelationalSlimModelConvention, Dictionary<string, object?>, TSource, TTarget, bool> process)
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

        private SlimDbFunction Create(IDbFunction function, SlimModel slimModel)
            => new SlimDbFunction(
                function.ModelName,
                slimModel,
                function.MethodInfo,
                function.ReturnType,
                function.IsScalar,
                function.IsAggregate,
                function.IsNullable,
                function.IsBuiltIn,
                function.Name,
                function.Schema,
                function.StoreType,
                function.TypeMapping,
                function.Translation);

        /// <summary>
        ///     Updates the function annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="function"> The source function. </param>
        /// <param name="slimFunction"> The target function that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessFunctionAnnotations(
            Dictionary<string, object?> annotations,
            IDbFunction function,
            SlimDbFunction slimFunction,
            bool runtime)
        {
        }

        private SlimDbFunctionParameter Create(IDbFunctionParameter parameter, SlimDbFunction slimFunction)
            => slimFunction.AddParameter(
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
        /// <param name="slimParameter"> The target function parameter that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessFunctionParameterAnnotations(
            Dictionary<string, object?> annotations,
            IDbFunctionParameter parameter,
            SlimDbFunctionParameter slimParameter,
            bool runtime)
        {
        }

        private SlimSequence Create(ISequence sequence, SlimModel slimModel)
            => new SlimSequence(
                sequence.Name,
                sequence.Schema,
                slimModel,
                sequence.Type,
                sequence.StartValue,
                sequence.IncrementBy,
                sequence.IsCyclic,
                sequence.MinValue,
                sequence.MaxValue);

        /// <summary>
        ///     Updates the sequence annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="sequence"> The source sequence. </param>
        /// <param name="slimSequence"> The target sequence that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessSequenceAnnotations(
            Dictionary<string, object?> annotations,
            ISequence sequence,
            SlimSequence slimSequence,
            bool runtime)
        {
        }

        private SlimCheckConstraint Create(ICheckConstraint checkConstraint, SlimEntityType slimEntityType)
            => new SlimCheckConstraint(
                checkConstraint.Name,
                slimEntityType,
                checkConstraint.Sql);

        /// <summary>
        ///     Updates the check constraint annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="checkConstraint"> The source check constraint. </param>
        /// <param name="slimCheckConstraint"> The target check constraint that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessCheckConstraintAnnotations(
            Dictionary<string, object?> annotations,
            ICheckConstraint checkConstraint,
            SlimCheckConstraint slimCheckConstraint,
            bool runtime)
        {
        }

        /// <summary>
        ///     Updates the property annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="property"> The source property. </param>
        /// <param name="slimProperty"> The target property that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected override void ProcessPropertyAnnotations(
            Dictionary<string, object?> annotations, IProperty property, SlimProperty slimProperty, bool runtime)
        {
            base.ProcessPropertyAnnotations(annotations, property, slimProperty, runtime);

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
                    var slimPropertyOverrides = new SortedDictionary<StoreObjectIdentifier, IRelationalPropertyOverrides>();
                    foreach (var overridesPair in (SortedDictionary<StoreObjectIdentifier, IRelationalPropertyOverrides>?)overrides!)
                    {
                        var slimOverrides = Create(overridesPair.Value, slimProperty);
                        slimPropertyOverrides[overridesPair.Key] = slimOverrides;

                        CreateAnnotations(overridesPair.Value, slimOverrides, static (convention, annotations, source, target, runtime) =>
                            convention.ProcessPropertyOverridesAnnotations(annotations, source, target, runtime));
                    }

                    annotations[RelationalAnnotationNames.RelationalOverrides] = slimPropertyOverrides;
                }
            }
        }

        private SlimRelationalPropertyOverrides Create(
            IRelationalPropertyOverrides propertyOverrides,
            SlimProperty slimProperty)
            => new SlimRelationalPropertyOverrides(
                slimProperty,
                propertyOverrides.ColumnName,
                propertyOverrides.ColumnNameOverriden);

        /// <summary>
        ///     Updates the relational property overrides annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="propertyOverrides"> The source relational property overrides. </param>
        /// <param name="slimPropertyOverrides"> The target relational property overrides that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessPropertyOverridesAnnotations(
            Dictionary<string, object?> annotations,
            IRelationalPropertyOverrides propertyOverrides,
            SlimRelationalPropertyOverrides slimPropertyOverrides,
            bool runtime)
        {
        }

        /// <summary>
        ///     Updates the key annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="key"> The source key. </param>
        /// <param name="slimKey"> The target key that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected override void ProcessKeyAnnotations(
            IDictionary<string, object?> annotations,
            IKey key,
            SlimKey slimKey,
            bool runtime)
        {
            base.ProcessKeyAnnotations(annotations, key, slimKey, runtime);

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
        /// <param name="slimIndex"> The target index that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected override void ProcessIndexAnnotations(
            Dictionary<string, object?> annotations,
            IIndex index,
            SlimIndex slimIndex,
            bool runtime)
        {
            base.ProcessIndexAnnotations(annotations, index, slimIndex, runtime);

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
        /// <param name="slimForeignKey"> The target foreign key that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected override void ProcessForeignKeyAnnotations(
            Dictionary<string, object?> annotations,
            IForeignKey foreignKey,
            SlimForeignKey slimForeignKey,
            bool runtime)
        {
            base.ProcessForeignKeyAnnotations(annotations, foreignKey, slimForeignKey, runtime);

            if (runtime)
            {
                annotations.Remove(RelationalAnnotationNames.ForeignKeyMappings);
            }
        }
    }
}
