// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    /// <summary>
    ///     Base class to be used by relational database providers when implementing an <see cref="ICSharpRuntimeAnnotationCodeGenerator" />
    /// </summary>
#pragma warning disable EF1001 // Internal EF Core API usage.
    public class RelationalCSharpRuntimeAnnotationCodeGenerator : CSharpRuntimeAnnotationCodeGenerator
    {
        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
        /// <param name="relationalDependencies">Parameter object containing relational dependencies for this service.</param>
        public RelationalCSharpRuntimeAnnotationCodeGenerator(
            CSharpRuntimeAnnotationCodeGeneratorDependencies dependencies,
            RelationalCSharpRuntimeAnnotationCodeGeneratorDependencies relationalDependencies)
            : base(dependencies)
        {
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            RelationalDependencies = relationalDependencies;
        }

        /// <summary>
        ///     Relational provider-specific dependencies for this service.
        /// </summary>
        protected virtual RelationalCSharpRuntimeAnnotationCodeGeneratorDependencies RelationalDependencies { get; }

        /// <inheritdoc />
        public override void Generate(IModel model, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            var annotations = parameters.Annotations;
            if (parameters.IsRuntime)
            {
                annotations.Remove(RelationalAnnotationNames.ModelDependencies);
                annotations.Remove(RelationalAnnotationNames.RelationalModel);
            }
            else
            {
                annotations.Remove(RelationalAnnotationNames.Collation);

                if (annotations.TryGetAndRemove(
                    RelationalAnnotationNames.DbFunctions,
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

                if (annotations.TryGetAndRemove(
                    RelationalAnnotationNames.Sequences,
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
            CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            if (function.Translation != null)
            {
                throw new InvalidOperationException(RelationalStrings.CompiledModelFunctionTranslation(function.Name));
            }

            if (function is IConventionDbFunction conventionFunction
                && conventionFunction.GetTypeMappingConfigurationSource() != null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.CompiledModelFunctionTypeMapping(
                        function.Name, "Customize()", parameters.ClassName));
            }

            AddNamespace(function.ReturnType, parameters.Namespaces);

            var code = Dependencies.CSharpHelper;
            var functionVariable = code.Identifier(
                function.MethodInfo?.Name ?? function.Name, parameters.ScopeVariables, capitalize: false);
            var mainBuilder = parameters.MainBuilder;
            mainBuilder
                .Append("var ").Append(functionVariable).AppendLine(" = new RuntimeDbFunction(").IncrementIndent()
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
                    .Append(code.Literal(method.Name!)).AppendLine(",")
                    .Append(method.IsPublic ? "BindingFlags.Public" : "BindingFlags.NonPublic")
                    .Append(method.IsStatic ? " | BindingFlags.Static" : " | BindingFlags.Instance")
                    .AppendLine(" | BindingFlags.DeclaredOnly,")
                    .AppendLine("null,")
                    .Append("new Type[] { ").Append(string.Join(", ", method.GetParameters().Select(p => code.Literal(p.ParameterType))))
                    .AppendLine(" },")
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
                .Append(functionsVariable).Append("[").Append(code.Literal(function.ModelName)).Append("] = ").Append(functionVariable)
                .AppendLine(";")
                .AppendLine();
        }

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="function">The function to which the annotations are applied.</param>
        /// <param name="parameters">Additional parameters used during code generation.</param>
        public virtual void Generate(IDbFunction function, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            GenerateSimpleAnnotations(parameters);
        }

        private void Create(IDbFunctionParameter parameter, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            if (parameter is IConventionDbFunctionParameter conventionParameter
                && conventionParameter.GetTypeMappingConfigurationSource() != null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.CompiledModelFunctionParameterTypeMapping(
                        parameter.Function.Name, parameter.Name, "Customize()", parameters.ClassName));
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
        /// <param name="functionParameter">The function parameter to which the annotations are applied.</param>
        /// <param name="parameters">Additional parameters used during code generation.</param>
        public virtual void Generate(IDbFunctionParameter functionParameter, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            GenerateSimpleAnnotations(parameters);
        }

        private void Create(ISequence sequence, string sequencesVariable, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            var code = Dependencies.CSharpHelper;
            var sequenceVariable = code.Identifier(sequence.Name, parameters.ScopeVariables, capitalize: false);
            var mainBuilder = parameters.MainBuilder;
            mainBuilder
                .Append("var ").Append(sequenceVariable).AppendLine(" = new RuntimeSequence(").IncrementIndent()
                .Append(code.Literal(sequence.Name)).AppendLine(",")
                .Append(parameters.TargetName).AppendLine(",")
                .Append(code.Literal(sequence.Type));

            if (sequence.Schema != null)
            {
                mainBuilder.AppendLine(",")
                    .Append("schema: ").Append(code.Literal(sequence.Schema));
            }

            if (sequence.StartValue != Sequence.DefaultStartValue)
            {
                mainBuilder.AppendLine(",")
                    .Append("startValue: ").Append(code.Literal(sequence.StartValue));
            }

            if (sequence.IncrementBy != Sequence.DefaultIncrementBy)
            {
                mainBuilder.AppendLine(",")
                    .Append("incrementBy: ").Append(code.Literal(sequence.IncrementBy));
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

            mainBuilder
                .Append(sequencesVariable).Append("[(").Append(code.Literal(sequence.Name)).Append(", ")
                .Append(code.UnknownLiteral(sequence.Schema)).Append(")] = ")
                .Append(sequenceVariable).AppendLine(";")
                .AppendLine();
        }

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="sequence">The sequence to which the annotations are applied.</param>
        /// <param name="parameters">Additional parameters used during code generation.</param>
        public virtual void Generate(ISequence sequence, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public override void Generate(IEntityType entityType, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
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
            }

            base.Generate(entityType, parameters);
        }

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="constraint">The check constraint to which the annotations are applied.</param>
        /// <param name="parameters">Additional parameters used during code generation.</param>
        public virtual void Generate(ICheckConstraint constraint, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public override void Generate(IProperty property, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
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
                annotations.Remove(RelationalAnnotationNames.ColumnOrder);
                annotations.Remove(RelationalAnnotationNames.Comment);
                annotations.Remove(RelationalAnnotationNames.Collation);

                if (annotations.TryGetAndRemove(
                    RelationalAnnotationNames.RelationalOverrides,
                    out SortedDictionary<StoreObjectIdentifier, object> overrides))
                {
                    parameters.Namespaces.Add(typeof(SortedDictionary<StoreObjectIdentifier, object>).Namespace!);
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
            CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            var code = Dependencies.CSharpHelper;
            var overrideVariable =
                code.Identifier(parameters.TargetName + Capitalize(storeObject.Name), parameters.ScopeVariables, capitalize: false);
            var mainBuilder = parameters.MainBuilder;
            mainBuilder
                .Append("var ").Append(overrideVariable).AppendLine(" = new RuntimeRelationalPropertyOverrides(").IncrementIndent()
                .Append(parameters.TargetName).AppendLine(",")
                .Append(code.Literal(overrides.ColumnNameOverriden)).AppendLine(",")
                .Append(code.UnknownLiteral(overrides.ColumnName)).AppendLine(");").DecrementIndent();

            CreateAnnotations(
                overrides,
                GenerateOverrides,
                parameters with { TargetName = overrideVariable });

            mainBuilder.Append(overridesVariable).Append("[StoreObjectIdentifier.");

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
                .Append(overrideVariable).AppendLine(";");
        }

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="overrides">The property overrides to which the annotations are applied.</param>
        /// <param name="parameters">Additional parameters used during code generation.</param>
        public virtual void GenerateOverrides(IAnnotatable overrides, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public override void Generate(IKey key, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            if (parameters.IsRuntime)
            {
                parameters.Annotations.Remove(RelationalAnnotationNames.UniqueConstraintMappings);
            }

            base.Generate(key, parameters);
        }

        /// <inheritdoc />
        public override void Generate(IForeignKey foreignKey, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            if (parameters.IsRuntime)
            {
                parameters.Annotations.Remove(RelationalAnnotationNames.ForeignKeyMappings);
            }

            base.Generate(foreignKey, parameters);
        }

        /// <inheritdoc />
        public override void Generate(INavigation navigation, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            base.Generate(navigation, parameters);
        }

        /// <inheritdoc />
        public override void Generate(ISkipNavigation navigation, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            base.Generate(navigation, parameters);
        }

        /// <inheritdoc />
        public override void Generate(IIndex index, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            var annotations = parameters.Annotations;
            if (parameters.IsRuntime)
            {
                annotations.Remove(RelationalAnnotationNames.TableIndexMappings);
            }
            else
            {
                annotations.Remove(RelationalAnnotationNames.Filter);
            }

            base.Generate(index, parameters);
        }

        private void CreateAnnotations<TAnnotatable>(
            TAnnotatable annotatable,
            Action<TAnnotatable, CSharpRuntimeAnnotationCodeGeneratorParameters> process,
            CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
            where TAnnotatable : IAnnotatable
        {
            process(
                annotatable,
                parameters with { Annotations = annotatable.GetAnnotations().ToDictionary(a => a.Name, a => a.Value), IsRuntime = false });

            process(
                annotatable,
                parameters with
                {
                    Annotations = annotatable.GetRuntimeAnnotations().ToDictionary(a => a.Name, a => a.Value), IsRuntime = true
                });
        }

        private string Capitalize(string @string)
        {
            switch (@string.Length)
            {
                case 0:
                    return @string;
                case 1:
                    return char.ToUpperInvariant(@string[0]).ToString();
                default:
                    if (char.IsUpper(@string[0]))
                    {
                        return @string;
                    }

                    return char.ToUpperInvariant(@string[0]) + @string[1..];
            }
        }
    }
}
