// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     <para>
    ///         Base class to be used by relational database providers when implementing an <see cref="ICSharpSlimAnnotationCodeGenerator" />
    ///     </para>
    /// </summary>
    public class RelationalCSharpSlimAnnotationCodeGenerator : CSharpSlimAnnotationCodeGenerator
    {
        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this service. </param>
        public RelationalCSharpSlimAnnotationCodeGenerator(
            CSharpSlimAnnotationCodeGeneratorDependencies dependencies,
            RelationalCSharpSlimAnnotationCodeGeneratorDependencies relationalDependencies)
            : base(dependencies)
        {
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            RelationalDependencies = relationalDependencies;
        }

        /// <summary>
        ///     Parameter object containing relational dependencies for this service.
        /// </summary>
        protected virtual RelationalCSharpSlimAnnotationCodeGeneratorDependencies RelationalDependencies { get; }

        /// <inheritdoc />
        public override void Generate(IModel model, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            if (parameters.IsRuntime)
            {
                parameters.Annotations.Remove(RelationalAnnotationNames.ModelDependencies);
                parameters.Annotations.Remove(RelationalAnnotationNames.RelationalModel);
            }
            else
            {
                if (parameters.Annotations.TryGetAndRemove(RelationalAnnotationNames.DbFunctions,
                    out SortedDictionary<string, IDbFunction> functions))
                {
                    parameters.Namespaces.Add(typeof(SortedDictionary<,>).Namespace!);
                    parameters.Namespaces.Add(typeof(BindingFlags).Namespace!);
                    var functionsVariable = Dependencies.CSharpHelper.Identifier("functions", parameters.ScopeVariables, capitalize: false);
                    parameters.MainBuilder
                        .Append("var ").Append(functionsVariable).AppendLine(" = new SortedDictionary<string, IDbFunction>();");

                    foreach (var function in functions.Values)
                    {
                        Create(function, functionsVariable, parameters);
                    }

                    GenerateSimpleAnnotation(RelationalAnnotationNames.DbFunctions, functionsVariable, parameters);
                }

                if (parameters.Annotations.TryGetAndRemove(RelationalAnnotationNames.Sequences,
                    out SortedDictionary<(string, string?), ISequence> sequences))
                {
                    parameters.Namespaces.Add(typeof(SortedDictionary<,>).Namespace!);
                    var sequencesVariable = Dependencies.CSharpHelper.Identifier("sequences", parameters.ScopeVariables, capitalize: false);
                    parameters.MainBuilder
                        .Append("var ").Append(sequencesVariable).AppendLine(" = new SortedDictionary<(string, string), ISequence>();");

                    foreach (var sequencePair in sequences)
                    {
                        Create(sequencePair.Value, sequencesVariable, parameters);
                    }

                    GenerateSimpleAnnotation(RelationalAnnotationNames.Sequences, sequencesVariable, parameters);
                }
            }

            base.Generate(model, parameters);
        }

        private void Create(
            IDbFunction function,
            string functionsVariable,
            CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            if (function.Translation != null)
            {
                throw new InvalidOperationException(RelationalStrings.CompiledModelFunctionTranslation(function.Name));
            }

            if (function is IConventionDbFunction conventionFunction
                && conventionFunction.GetTypeMappingConfigurationSource() != null)
            {
                throw new InvalidOperationException(RelationalStrings.CompiledModelFunctionTypeMapping(function.Name));
            }

            AddNamespace(function.ReturnType, parameters.Namespaces);

            var code = Dependencies.CSharpHelper;
            var functionVariable = code.Identifier(
                function.MethodInfo?.Name ?? function.Name, parameters.ScopeVariables, capitalize: false);
            var mainBuilder = parameters.MainBuilder;
            mainBuilder
                .Append("var ").Append(functionVariable).AppendLine(" = new SlimDbFunction(").IncrementIndent()
                .Append(code.Literal(function.ModelName)).AppendLine(",")
                .Append(parameters.TargetName).AppendLine(",")
                .Append(code.Literal(function.ReturnType)).AppendLine(",")
                .Append(code.Literal(function.Name));

            if (function.Schema != null)
            {
                mainBuilder.AppendLine(",")
                    .Append("schema: ").Append(code.Literal(function.Schema));
            }

            if (function.StoreType != null)
            {
                mainBuilder.AppendLine(",")
                    .Append("storeType: ").Append(code.Literal(function.StoreType));
            }

            if (function.MethodInfo != null)
            {
                var method = function.MethodInfo;
                mainBuilder.AppendLine(",")
                    .Append("methodInfo: ").Append(code.Literal(method.DeclaringType!)).AppendLine(".GetMethod(").IncrementIndent()
                    .Append(code.Literal(method.Name)).AppendLine(",")
                    .Append(method.IsPublic ? "BindingFlags.Public" : "BindingFlags.NonPublic")
                    .Append(method.IsStatic ? " | BindingFlags.Static" : " | BindingFlags.Instance")
                    .AppendLine(" | BindingFlags.DeclaredOnly,")
                    .AppendLine("null,")
                    .Append("new Type[] { ").Append(string.Join(", ", method.GetParameters().Select(p => code.Literal(p.ParameterType)))).AppendLine(" },")
                    .Append("null)").DecrementIndent();
            }

            if (function.IsScalar)
            {
                mainBuilder.AppendLine(",")
                    .Append("scalar: ").Append(code.Literal(function.IsScalar));
            }

            if (function.IsAggregate)
            {
                mainBuilder.AppendLine(",")
                    .Append("aggregate: ").Append(code.Literal(function.IsAggregate));
            }

            if (function.IsNullable)
            {
                mainBuilder.AppendLine(",")
                    .Append("nullable: ").Append(code.Literal(function.IsNullable));
            }

            if (function.IsBuiltIn)
            {
                mainBuilder.AppendLine(",")
                    .Append("builtIn: ").Append(code.Literal(function.IsBuiltIn));
            }

            mainBuilder.AppendLine(");").DecrementIndent()
                .AppendLine();

            var parameterParameters = parameters with { TargetName = functionVariable };
            foreach (var parameter in function.Parameters)
            {
                Create(parameter, parameterParameters);
            }

            CreateAnnotations(
                function,
                Generate,
                parameters with { TargetName = functionVariable });

            mainBuilder
                .Append(functionsVariable).Append("[").Append(code.Literal(function.ModelName)).Append("] = ").Append(functionVariable).AppendLine(";")
                .AppendLine();
        }

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="function"> The function to which the annotations are applied. </param>
        /// <param name="parameters"> Additional parameters used during code generation. </param>
        public virtual void Generate(IDbFunction function, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            GenerateSimpleAnnotations(parameters);
        }

        private void Create(IDbFunctionParameter parameter, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            if (parameter is IConventionDbFunctionParameter conventionParameter
                    && conventionParameter.GetTypeMappingConfigurationSource() != null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.CompiledModelFunctionParameterTypeMapping(parameter.Function.Name, parameter.Name));
            }

            AddNamespace(parameter.ClrType, parameters.Namespaces);

            var code = Dependencies.CSharpHelper;
            var mainBuilder = parameters.MainBuilder;
            var parameterVariable = code.Identifier(parameter.Name, parameters.ScopeVariables, capitalize: false);
            mainBuilder
                .Append("var ").Append(parameterVariable).Append(" = ")
                .Append(parameters.TargetName).AppendLine(".AddParameter(").IncrementIndent()
                .Append(code.Literal(parameter.Name)).AppendLine(",")
                .Append(code.Literal(parameter.ClrType)).AppendLine(",")
                .Append(code.Literal(parameter.PropagatesNullability)).AppendLine(",")
                .Append(code.Literal(parameter.StoreType)).AppendLine(");").DecrementIndent();

            CreateAnnotations(
                parameter,
                Generate,
                parameters with { TargetName = parameterVariable });

            mainBuilder.AppendLine();
        }

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="functionParameter"> The function parameter to which the annotations are applied. </param>
        /// <param name="parameters"> Additional parameters used during code generation. </param>
        public virtual void Generate(IDbFunctionParameter functionParameter, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            GenerateSimpleAnnotations(parameters);
        }

        private void Create(ISequence sequence, string sequencesVariable, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            var code = Dependencies.CSharpHelper;
            var sequenceVariable = code.Identifier(sequence.Name, parameters.ScopeVariables, capitalize: false);
            var mainBuilder = parameters.MainBuilder;
            mainBuilder
                .Append("var ").Append(sequenceVariable).AppendLine(" = new SlimSequence(").IncrementIndent()
                .Append(code.Literal(sequence.Name)).AppendLine(",")
                .Append(parameters.TargetName).AppendLine(",")
                .Append(code.Literal(sequence.Type)).AppendLine(",")
                .Append(code.Literal(sequence.StartValue)).AppendLine(",")
                .Append(code.Literal(sequence.IncrementBy));

            if (sequence.Schema != null)
            {
                mainBuilder.AppendLine(",")
                    .Append("schema: ").Append(code.Literal(sequence.Schema));
            }

            if (sequence.IsCyclic)
            {
                mainBuilder.AppendLine(",")
                    .Append("cyclic: ").Append(code.Literal(sequence.IsCyclic));
            }

            if (sequence.MinValue != null)
            {
                mainBuilder.AppendLine(",")
                    .Append("minValue: ").Append(code.Literal(sequence.MinValue));
            }

            if (sequence.MaxValue != null)
            {
                mainBuilder.AppendLine(",")
                    .Append("maxValue: ").Append(code.Literal(sequence.MaxValue));
            }

            mainBuilder.AppendLine(");").DecrementIndent()
                .AppendLine();

            CreateAnnotations(
                sequence,
                Generate,
                parameters with { TargetName = sequenceVariable });

            mainBuilder.AppendLine();

            mainBuilder
                .Append(sequencesVariable).Append("[(").Append(code.Literal(sequence.Name)).Append(", ")
                    .Append(code.UnknownLiteral(sequence.Schema)).Append(")] = ")
                .Append(sequenceVariable).AppendLine(";")
                .AppendLine();
        }

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="sequence"> The sequence to which the annotations are applied. </param>
        /// <param name="parameters"> Additional parameters used during code generation. </param>
        public virtual void Generate(ISequence sequence, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public override void Generate(IEntityType entityType, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            var annotations = parameters.Annotations;
            if (parameters.IsRuntime)
            {
                annotations.Remove(RelationalAnnotationNames.TableMappings);
                annotations.Remove(RelationalAnnotationNames.ViewMappings);
                annotations.Remove(RelationalAnnotationNames.SqlQueryMappings);
                annotations.Remove(RelationalAnnotationNames.FunctionMappings);
                annotations.Remove(RelationalAnnotationNames.DefaultMappings);
            }
            else
            {
                if (annotations.TryGetAndRemove(RelationalAnnotationNames.CheckConstraints,
                    out Dictionary<string, ICheckConstraint> constraints))
                {
                    parameters.Namespaces.Add(typeof(SortedDictionary<,>).Namespace!);
                    var constraintsVariable = Dependencies.CSharpHelper.Identifier("constraints", parameters.ScopeVariables, capitalize: false);
                    parameters.MainBuilder
                        .Append("var ").Append(constraintsVariable).AppendLine(" = new SortedDictionary<string, ICheckConstraint>();");

                    foreach (var constraintPair in constraints)
                    {
                        Create(constraintPair.Value, constraintsVariable, parameters);
                    }

                    GenerateSimpleAnnotation(RelationalAnnotationNames.CheckConstraints, constraintsVariable, parameters);
                }

                // These need to be set explicitly to prevent default values from being generated
                annotations[RelationalAnnotationNames.TableName] = entityType.GetTableName();
                annotations[RelationalAnnotationNames.Schema] = entityType.GetSchema();
                annotations[RelationalAnnotationNames.ViewName] = entityType.GetViewName();
                annotations[RelationalAnnotationNames.ViewSchema] = entityType.GetViewSchema();
                annotations[RelationalAnnotationNames.SqlQuery] = entityType.GetSqlQuery();
                annotations[RelationalAnnotationNames.FunctionName] = entityType.GetFunctionName();
            }

            base.Generate(entityType, parameters);
        }

        private void Create(ICheckConstraint constraint, string constraintsVariable, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            var code = Dependencies.CSharpHelper;
            var constraintVariable = code.Identifier(constraint.Name, parameters.ScopeVariables, capitalize: false);
            var mainBuilder = parameters.MainBuilder;
            mainBuilder
                .Append("var ").Append(constraintVariable).AppendLine(" = new SlimCheckConstraint(").IncrementIndent()
                .Append(code.Literal(constraint.Name)).AppendLine(",")
                .Append(parameters.TargetName).AppendLine(",")
                .Append(code.Literal(constraint.Sql)).AppendLine(");").DecrementIndent()
                .AppendLine();

            CreateAnnotations(
                constraint,
                Generate,
                parameters with { TargetName = constraintVariable });

            mainBuilder.AppendLine();

            mainBuilder
                .Append(constraintsVariable).Append("[").Append(code.Literal(constraint.Name)).Append("] = ")
                .Append(constraintVariable).AppendLine(";")
                .AppendLine();
        }

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="constraint"> The check constraint to which the annotations are applied. </param>
        /// <param name="parameters"> Additional parameters used during code generation. </param>
        public virtual void Generate(ICheckConstraint constraint, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public override void Generate(IProperty property, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            var annotations = parameters.Annotations;
            if (parameters.IsRuntime)
            {
                annotations.Remove(RelationalAnnotationNames.TableColumnMappings);
                annotations.Remove(RelationalAnnotationNames.ViewColumnMappings);
                annotations.Remove(RelationalAnnotationNames.SqlQueryColumnMappings);
                annotations.Remove(RelationalAnnotationNames.FunctionColumnMappings);
                annotations.Remove(RelationalAnnotationNames.DefaultColumnMappings);
            }
            else
            {
                if (annotations.TryGetAndRemove(RelationalAnnotationNames.RelationalOverrides,
                    out SortedDictionary<StoreObjectIdentifier, object> overrides))
                {
                    parameters.Namespaces.Add(typeof(SortedDictionary<,>).Namespace!);
                    var overridesVariable = Dependencies.CSharpHelper.Identifier("overrides", parameters.ScopeVariables, capitalize: false);
                    parameters.MainBuilder
                        .Append("var ").Append(overridesVariable).AppendLine(" = new SortedDictionary<StoreObjectIdentifier, object>();");

                    foreach (var overridePair in overrides)
                    {
                        Create((IRelationalPropertyOverrides)overridePair.Value, overridePair.Key, overridesVariable, parameters);
                    }

                    GenerateSimpleAnnotation(RelationalAnnotationNames.RelationalOverrides, overridesVariable, parameters);
                }
            }

            base.Generate(property, parameters);
        }

        private void Create(
            IRelationalPropertyOverrides overrides,
            StoreObjectIdentifier storeObject,
            string overridesVariable,
            CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            var code = Dependencies.CSharpHelper;
            var overrideVariable =
                code.Identifier(parameters.TargetName + Capitalize(storeObject.Name), parameters.ScopeVariables, capitalize: false);
            var mainBuilder = parameters.MainBuilder;
            mainBuilder
                .Append("var ").Append(overrideVariable).AppendLine(" = new SlimRelationalPropertyOverrides(").IncrementIndent()
                .Append(parameters.TargetName).AppendLine(",")
                .Append(code.Literal(overrides.ColumnNameOverriden)).AppendLine(",")
                .Append(code.UnknownLiteral(overrides.ColumnName)).AppendLine(");").DecrementIndent()
                .AppendLine();

            CreateAnnotations(
                overrides,
                GenerateOverrides,
                parameters with { TargetName = overrideVariable });

            mainBuilder.AppendLine()
                .Append(overridesVariable).Append("[StoreObjectIdentifier.");

            switch (storeObject.StoreObjectType)
            {
                case StoreObjectType.Table:
                    mainBuilder
                        .Append("Table(").Append(code.Literal(storeObject.Name))
                        .Append(", ").Append(code.UnknownLiteral(storeObject.Schema)).Append(")");
                    break;
                case StoreObjectType.View:
                    mainBuilder
                        .Append("View(").Append(code.Literal(storeObject.Name))
                        .Append(", ").Append(code.UnknownLiteral(storeObject.Schema)).Append(")");
                    break;
                case StoreObjectType.SqlQuery:
                    mainBuilder
                        .Append("SqlQuery(").Append(code.Literal(storeObject.Name)).Append(")");
                    break;
                case StoreObjectType.Function:
                    mainBuilder
                        .Append("DbFunction(").Append(code.Literal(storeObject.Name)).Append(")");
                    break;
            }

            mainBuilder
                .Append("] = ")
                .Append(overrideVariable).AppendLine(";")
                .AppendLine();
        }

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="overrides"> The property overrides to which the annotations are applied. </param>
        /// <param name="parameters"> Additional parameters used during code generation. </param>
        public virtual void GenerateOverrides(IAnnotatable overrides, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public override void Generate(IKey key, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            if (parameters.IsRuntime)
            {
                parameters.Annotations.Remove(RelationalAnnotationNames.UniqueConstraintMappings);
            }

            base.Generate(key, parameters);
        }

        /// <inheritdoc />
        public override void Generate(IForeignKey foreignKey, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            if (parameters.IsRuntime)
            {
                parameters.Annotations.Remove(RelationalAnnotationNames.ForeignKeyMappings);
            }

            base.Generate(foreignKey, parameters);
        }

        /// <inheritdoc />
        public override void Generate(INavigation navigation, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            base.Generate(navigation, parameters);
        }

        /// <inheritdoc />
        public override void Generate(ISkipNavigation navigation, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            base.Generate(navigation, parameters);
        }

        /// <inheritdoc />
        public override void Generate(IIndex index, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            if (parameters.IsRuntime)
            {
                parameters.Annotations.Remove(RelationalAnnotationNames.TableIndexMappings);
            }

            base.Generate(index, parameters);
        }

        private void GenerateSimpleAnnotations(CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            foreach (var (name, value) in parameters.Annotations)
            {
                if (value != null)
                {
                    AddNamespace(value.GetType(), parameters.Namespaces);
                }

                GenerateSimpleAnnotation(name, Dependencies.CSharpHelper.UnknownLiteral(value), parameters);
            }
        }

        private void CreateAnnotations<TAnnotatable>(
            TAnnotatable annotatable,
            Action<TAnnotatable, CSharpSlimAnnotationCodeGeneratorParameters> process,
            CSharpSlimAnnotationCodeGeneratorParameters parameters)
            where TAnnotatable : IAnnotatable
        {
            process(annotatable,
                parameters with
                {
                    Annotations = annotatable.GetAnnotations().ToDictionary(a => a.Name, a => a.Value),
                    IsRuntime = false
                });

            process(annotatable,
                parameters with
                {
                    Annotations = annotatable.GetRuntimeAnnotations().ToDictionary(a => a.Name, a => a.Value),
                    IsRuntime = true
                });
        }

        private string Capitalize(string @string)
        {
            switch (@string.Length)
            {
                case 0:
                    return @string;
                case 1:
                    return char.ToUpper(@string[0]).ToString();
                default:
                    return char.ToUpper(@string[0]) + @string[1..];
            }
        }
    }
}
