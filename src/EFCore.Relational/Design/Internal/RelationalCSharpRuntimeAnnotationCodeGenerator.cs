// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Design.Internal;

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

            if (annotations.TryGetAndRemove(
                    RelationalAnnotationNames.RelationalModel,
                    out RelationalModel relationalModel))
            {
                GenerateSimpleAnnotation(RelationalAnnotationNames.RelationalModel, "CreateRelationalModel()", parameters);

                var methodBuilder = new IndentedStringBuilder();
                Create(
                    relationalModel, parameters with
                    {
                        MainBuilder = parameters.MethodBuilder,
                        MethodBuilder = methodBuilder,
                        ScopeVariables = new HashSet<string>()
                    });

                var methods = methodBuilder.ToString();
                if (!string.IsNullOrEmpty(methods))
                {
                    parameters.MethodBuilder.AppendLines(methods);
                }
            }
        }
        else
        {
            annotations.Remove(RelationalAnnotationNames.Collation);

            if (annotations.TryGetAndRemove(
                    RelationalAnnotationNames.DbFunctions,
                    out IReadOnlyDictionary<string, IDbFunction> functions))
            {
                parameters.Namespaces.Add(typeof(Dictionary<,>).Namespace!);
                parameters.Namespaces.Add(typeof(BindingFlags).Namespace!);
                var functionsVariable = Dependencies.CSharpHelper.Identifier("functions", parameters.ScopeVariables, capitalize: false);
                parameters.MainBuilder
                    .Append("var ").Append(functionsVariable).AppendLine(" = new Dictionary<string, IDbFunction>();");

                foreach (var function in functions.OrderBy(t => t.Key).Select(t => t.Value))
                {
                    Create(function, functionsVariable, parameters);
                }

                GenerateSimpleAnnotation(RelationalAnnotationNames.DbFunctions, functionsVariable, parameters);
            }

            if (annotations.TryGetAndRemove(
                    RelationalAnnotationNames.Sequences,
                    out IReadOnlyDictionary<(string, string?), ISequence> sequences))
            {
                parameters.Namespaces.Add(typeof(Dictionary<,>).Namespace!);
                var sequencesVariable = Dependencies.CSharpHelper.Identifier("sequences", parameters.ScopeVariables, capitalize: false);
                var mainBuilder = parameters.MainBuilder;
                mainBuilder.Append("var ").Append(sequencesVariable).Append(" = new Dictionary<(string, string");

                if (parameters.UseNullableReferenceTypes)
                {
                    mainBuilder.Append("?");
                }

                mainBuilder.AppendLine("), ISequence>();");

                foreach (var sequence in sequences.OrderBy(t => t.Key).Select(t => t.Value))
                {
                    Create(sequence, sequencesVariable, parameters);
                }

                GenerateSimpleAnnotation(RelationalAnnotationNames.Sequences, sequencesVariable, parameters);
            }
        }

        base.Generate(model, parameters);
    }

    private void Create(
        IRelationalModel model,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var code = Dependencies.CSharpHelper;
        var mainBuilder = parameters.MainBuilder;
        mainBuilder.AppendLine()
            .AppendLine("private IRelationalModel CreateRelationalModel()")
            .AppendLine("{");

        using (mainBuilder.Indent())
        {
            parameters.Namespaces.Add(typeof(RelationalModel).Namespace!);
            parameters.Namespaces.Add(typeof(RelationalModelExtensions).Namespace!);
            var relationalModelVariable = Dependencies.CSharpHelper
                .Identifier("relationalModel", parameters.ScopeVariables, capitalize: false);
            mainBuilder
                .AppendLine($"var {relationalModelVariable} = new RelationalModel({parameters.TargetName});");

            var metadataVariables = new Dictionary<IAnnotatable, string>();
            var relationalModelParameters = parameters with { TargetName = relationalModelVariable };
            AddNamespace(typeof(List<TableMapping>), parameters.Namespaces);
            foreach (var entityType in model.Model.GetEntityTypes())
            {
                CreateMappings(entityType, declaringVariable: null, metadataVariables, relationalModelParameters);
            }

            foreach (var table in model.Tables)
            {
                foreach (var foreignKey in table.ForeignKeyConstraints)
                {
                    Create(foreignKey, metadataVariables, parameters with { TargetName = metadataVariables[table] });
                }
            }

            foreach (var dbFunction in model.Model.GetDbFunctions())
            {
                if (!dbFunction.IsScalar)
                {
                    continue;
                }

                GetOrCreate(dbFunction.StoreFunction, metadataVariables, relationalModelParameters);
            }

            CreateAnnotations(
                model,
                Generate,
                relationalModelParameters);

            mainBuilder
                .AppendLine($"return {relationalModelVariable}.MakeReadOnly();");
        }

        mainBuilder
            .AppendLine("}");

        void CreateMappings(
            ITypeBase typeBase,
            string? declaringVariable,
            Dictionary<IAnnotatable, string> metadataVariables,
            CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            var code = Dependencies.CSharpHelper;

            var typeBaseVariable = code.Identifier(typeBase.ShortName(), parameters.ScopeVariables, capitalize: false);
            metadataVariables.Add(typeBase, typeBaseVariable);
            if (typeBase is IComplexType complexType)
            {
                parameters.MainBuilder
                    .AppendLine()
                    .Append($"var {typeBaseVariable} = ")
                    .AppendLine($"{declaringVariable}.FindComplexProperty({code.Literal(complexType.ComplexProperty.Name)})!.ComplexType;");
            }
            else
            {
                parameters.MainBuilder
                    .AppendLine()
                    .AppendLine($"var {typeBaseVariable} = FindEntityType({code.Literal(typeBase.Name)})!;");
            }

            // All the mappings below are added in a way that preserves the order
            if (typeBase.GetDefaultMappings().Any())
            {
                var tableMappingsVariable = code.Identifier("defaultTableMappings", parameters.ScopeVariables, capitalize: false);
                mainBuilder
                    .AppendLine()
                    .AppendLine($"var {tableMappingsVariable} = new List<TableMappingBase<ColumnMappingBase>>();")
                    .Append($"{typeBaseVariable}.SetRuntimeAnnotation(")
                    .AppendLine($"{code.Literal(RelationalAnnotationNames.DefaultMappings)}, {tableMappingsVariable});");
                foreach (var mapping in typeBase.GetDefaultMappings())
                {
                    Create(mapping, tableMappingsVariable, metadataVariables, parameters);
                }
            }

            if (typeBase.GetTableMappings().Any())
            {
                var tableMappingsVariable = code.Identifier("tableMappings", parameters.ScopeVariables, capitalize: false);
                mainBuilder
                    .AppendLine()
                    .AppendLine($"var {tableMappingsVariable} = new List<TableMapping>();")
                    .Append($"{typeBaseVariable}.SetRuntimeAnnotation(")
                    .AppendLine($"{code.Literal(RelationalAnnotationNames.TableMappings)}, {tableMappingsVariable});");
                foreach (var mapping in typeBase.GetTableMappings())
                {
                    Create(mapping, tableMappingsVariable, metadataVariables, parameters);
                }
            }

            if (typeBase.GetViewMappings().Any())
            {
                var viewMappingsVariable = code.Identifier("viewMappings", parameters.ScopeVariables, capitalize: false);
                mainBuilder
                    .AppendLine()
                    .AppendLine($"var {viewMappingsVariable} = new List<ViewMapping>();")
                    .Append($"{typeBaseVariable}.SetRuntimeAnnotation(")
                    .AppendLine($"{code.Literal(RelationalAnnotationNames.ViewMappings)}, {viewMappingsVariable});");
                foreach (var mapping in typeBase.GetViewMappings())
                {
                    Create(mapping, viewMappingsVariable, metadataVariables, parameters);
                }
            }

            if (typeBase.GetSqlQueryMappings().Any())
            {
                var sqlQueryMappingsVariable = code.Identifier("sqlQueryMappings", parameters.ScopeVariables, capitalize: false);
                mainBuilder
                    .AppendLine()
                    .AppendLine($"var {sqlQueryMappingsVariable} = new List<SqlQueryMapping>();")
                    .Append($"{typeBaseVariable}.SetRuntimeAnnotation(")
                    .AppendLine($"{code.Literal(RelationalAnnotationNames.SqlQueryMappings)}, {sqlQueryMappingsVariable});");
                foreach (var mapping in typeBase.GetSqlQueryMappings())
                {
                    Create(mapping, sqlQueryMappingsVariable, metadataVariables, parameters);
                }
            }

            if (typeBase.GetFunctionMappings().Any())
            {
                var functionMappingsVariable = code.Identifier("functionMappings", parameters.ScopeVariables, capitalize: false);
                mainBuilder
                    .AppendLine()
                    .AppendLine($"var {functionMappingsVariable} = new List<FunctionMapping>();")
                    .Append($"{typeBaseVariable}.SetRuntimeAnnotation(")
                    .AppendLine($"{code.Literal(RelationalAnnotationNames.FunctionMappings)}, {functionMappingsVariable});");
                foreach (var mapping in typeBase.GetFunctionMappings())
                {
                    Create(mapping, functionMappingsVariable, metadataVariables, parameters);
                }
            }

            if (typeBase.GetDeleteStoredProcedureMappings().Any())
            {
                var deleteSprocMappingsVariable = code.Identifier("deleteSprocMappings", parameters.ScopeVariables, capitalize: false);
                mainBuilder
                    .AppendLine()
                    .AppendLine($"var {deleteSprocMappingsVariable} = new List<StoredProcedureMapping>();")
                    .Append($"{typeBaseVariable}.SetRuntimeAnnotation(")
                    .AppendLine(
                        $"{code.Literal(RelationalAnnotationNames.DeleteStoredProcedureMappings)}, {deleteSprocMappingsVariable});");
                foreach (var mapping in typeBase.GetDeleteStoredProcedureMappings())
                {
                    Create(
                        mapping,
                        deleteSprocMappingsVariable,
                        StoreObjectType.DeleteStoredProcedure,
                        metadataVariables,
                        parameters);
                }
            }

            if (typeBase.GetInsertStoredProcedureMappings().Any())
            {
                var insertSprocMappingsVariable = code.Identifier("insertSprocMappings", parameters.ScopeVariables, capitalize: false);
                mainBuilder
                    .AppendLine()
                    .AppendLine($"var {insertSprocMappingsVariable} = new List<StoredProcedureMapping>();")
                    .Append($"{typeBaseVariable}.SetRuntimeAnnotation(")
                    .AppendLine(
                        $"{code.Literal(RelationalAnnotationNames.InsertStoredProcedureMappings)}, {insertSprocMappingsVariable});");
                foreach (var mapping in typeBase.GetInsertStoredProcedureMappings())
                {
                    Create(
                        mapping,
                        insertSprocMappingsVariable,
                        StoreObjectType.InsertStoredProcedure,
                        metadataVariables,
                        parameters);
                }
            }

            if (typeBase.GetUpdateStoredProcedureMappings().Any())
            {
                var updateSprocMappingsVariable = code.Identifier("updateSprocMappings", parameters.ScopeVariables, capitalize: false);
                mainBuilder
                    .AppendLine()
                    .AppendLine($"var {updateSprocMappingsVariable} = new List<StoredProcedureMapping>();")
                    .Append($"{typeBaseVariable}.SetRuntimeAnnotation(")
                    .AppendLine(
                        $"{code.Literal(RelationalAnnotationNames.UpdateStoredProcedureMappings)}, {updateSprocMappingsVariable});");
                foreach (var mapping in typeBase.GetUpdateStoredProcedureMappings())
                {
                    Create(
                        mapping,
                        updateSprocMappingsVariable,
                        StoreObjectType.UpdateStoredProcedure,
                        metadataVariables,
                        parameters);
                }
            }

            foreach (var complexProperty in typeBase.GetDeclaredComplexProperties())
            {
                CreateMappings(complexProperty.ComplexType, typeBaseVariable, metadataVariables, parameters);
            }
        }
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="model">The relational model to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(IRelationalModel model, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private string GetOrCreate(
        ITableBase table,
        Dictionary<IAnnotatable, string> metadataVariables,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (metadataVariables.TryGetValue(table, out var tableVariable))
        {
            return tableVariable;
        }

        var code = Dependencies.CSharpHelper;
        tableVariable = code.Identifier(table.Name + "TableBase", parameters.ScopeVariables, capitalize: false);
        metadataVariables.Add(table, tableVariable);
        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .Append($"var {tableVariable} = new TableBase({code.Literal(table.Name)}, {code.Literal(table.Schema)}, ")
            .AppendLine($"{parameters.TargetName});");

        var tableParameters = parameters with { TargetName = tableVariable };

        foreach (var column in table.Columns)
        {
            Create(column, metadataVariables, tableParameters);
        }

        CreateAnnotations(
            table,
            Generate,
            tableParameters);

        mainBuilder
            .AppendLine($"{parameters.TargetName}.DefaultTables.Add({code.Literal(table.Name)}, {tableVariable});");

        return tableVariable;
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="table">The table to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(ITableBase table, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private string GetOrCreate(
        ITable table,
        Dictionary<IAnnotatable, string> metadataVariables,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (metadataVariables.TryGetValue(table, out var tableVariable))
        {
            return tableVariable;
        }

        var code = Dependencies.CSharpHelper;
        tableVariable = code.Identifier(table.Name + "Table", parameters.ScopeVariables, capitalize: false);
        metadataVariables.Add(table, tableVariable);
        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .Append($"var {tableVariable} = new Table({code.Literal(table.Name)}, {code.Literal(table.Schema)}, ")
            .AppendLine($"{parameters.TargetName});");

        var tableParameters = parameters with { TargetName = tableVariable };

        foreach (var column in table.Columns)
        {
            Create(column, metadataVariables, tableParameters);
        }

        foreach (var uniqueConstraint in table.UniqueConstraints)
        {
            Create(uniqueConstraint, uniqueConstraint.Columns.Select(c => metadataVariables[c]), tableParameters);
        }

        foreach (var index in table.Indexes)
        {
            Create(index, index.Columns.Select(c => metadataVariables[c]), tableParameters);
        }

        foreach (var trigger in table.Triggers)
        {
            var entityTypeVariable = metadataVariables[trigger.EntityType];

            var triggerName = trigger.GetDatabaseName(StoreObjectIdentifier.Table(table.Name, table.Schema));
            mainBuilder
                .Append($"{tableVariable}.Triggers.Add({code.Literal(triggerName)}, ")
                .AppendLine($"{entityTypeVariable}.FindDeclaredTrigger({code.Literal(trigger.ModelName)}));");
        }

        CreateAnnotations(
            table,
            Generate,
            tableParameters);

        mainBuilder
            .Append($"{parameters.TargetName}.Tables.Add((")
            .AppendLine($"{code.Literal(table.Name)}, {code.Literal(table.Schema)}), {tableVariable});");

        return tableVariable;
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="table">The table to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(ITable table, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private string GetOrCreate(
        IView view,
        Dictionary<IAnnotatable, string> metadataVariables,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (metadataVariables.TryGetValue(view, out var viewVariable))
        {
            return viewVariable;
        }

        var code = Dependencies.CSharpHelper;
        viewVariable = code.Identifier(view.Name + "View", parameters.ScopeVariables, capitalize: false);
        metadataVariables.Add(view, viewVariable);
        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .Append($"var {viewVariable} = new View({code.Literal(view.Name)}, {code.Literal(view.Schema)}, ")
            .AppendLine($"{parameters.TargetName});");

        var viewParameters = parameters with { TargetName = viewVariable };

        foreach (var column in view.Columns)
        {
            Create(column, metadataVariables, viewParameters);
        }

        CreateAnnotations(
            view,
            Generate,
            viewParameters);

        mainBuilder
            .Append($"{parameters.TargetName}.Views.Add((")
            .AppendLine($"{code.Literal(view.Name)}, {code.Literal(view.Schema)}), {viewVariable});");

        return viewVariable;
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="view">The view to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(IView view, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private string GetOrCreate(
        ISqlQuery sqlQuery,
        Dictionary<IAnnotatable, string> metadataVariables,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (metadataVariables.TryGetValue(sqlQuery, out var sqlQueryVariable))
        {
            return sqlQueryVariable;
        }

        var code = Dependencies.CSharpHelper;
        sqlQueryVariable = code.Identifier(sqlQuery.Name + "SqlQuery", parameters.ScopeVariables, capitalize: false);
        metadataVariables.Add(sqlQuery, sqlQueryVariable);
        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .Append($"var {sqlQueryVariable} = new SqlQuery({code.Literal(sqlQuery.Name)}, {parameters.TargetName}, ")
            .AppendLine($"{code.Literal(sqlQuery.Sql)});");

        var sqlQueryParameters = parameters with { TargetName = sqlQueryVariable };

        foreach (var column in sqlQuery.Columns)
        {
            Create(column, metadataVariables, sqlQueryParameters);
        }

        CreateAnnotations(
            sqlQuery,
            Generate,
            sqlQueryParameters);

        mainBuilder
            .Append($"{parameters.TargetName}.Queries.Add(")
            .AppendLine($"{code.Literal(sqlQuery.Name)}, {sqlQueryVariable});");

        return sqlQueryVariable;
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="sqlQuery">The SQL query to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(ISqlQuery sqlQuery, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private string GetOrCreate(
        IStoreFunction function,
        Dictionary<IAnnotatable, string> metadataVariables,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (metadataVariables.TryGetValue(function, out var functionVariable))
        {
            return functionVariable;
        }

        var code = Dependencies.CSharpHelper;
        var mainDbFunctionVariable = GetOrCreate(function.DbFunctions.First(), metadataVariables, parameters);
        functionVariable = code.Identifier(function.Name + "Function", parameters.ScopeVariables, capitalize: false);
        metadataVariables.Add(function, functionVariable);
        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .AppendLine($"var {functionVariable} = new StoreFunction({mainDbFunctionVariable}, {parameters.TargetName});");

        var functionParameters = parameters with { TargetName = functionVariable };

        foreach (var dbFunction in function.DbFunctions.Skip(1))
        {
            var dbFunctionVariable = GetOrCreate(dbFunction, metadataVariables, parameters);
            mainBuilder
                .AppendLine($"{dbFunctionVariable}.StoreFunction = {functionVariable};")
                .AppendLine($"{functionVariable}.DbFunctions.Add({code.Literal(dbFunction.ModelName)}, {dbFunctionVariable});");
        }

        foreach (var parameter in function.Parameters)
        {
            var parameterVariable = code.Identifier(parameter.Name + "FunctionParameter", parameters.ScopeVariables, capitalize: false);
            metadataVariables.Add(parameter, parameterVariable);
            mainBuilder.AppendLine($"var {parameterVariable} = {functionVariable}.FindParameter({code.Literal(parameter.Name)})!;");

            CreateAnnotations(
                parameter,
                Generate,
                parameters with { TargetName = parameterVariable });
        }

        foreach (var column in function.Columns)
        {
            Create(column, metadataVariables, functionParameters);
        }

        CreateAnnotations(
            function,
            Generate,
            functionParameters);

        mainBuilder
            .AppendLine($"{parameters.TargetName}.Functions.Add(").IncrementIndent()
            .Append($"({code.Literal(function.Name)}, {code.Literal(function.Schema)}, ")
            .AppendLine($"{code.Literal(function.DbFunctions.First().Parameters.Select(p => p.StoreType).ToArray())}),")
            .AppendLine($"{functionVariable});").DecrementIndent();

        return functionVariable;
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="function">The function to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(IStoreFunction function, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private string GetOrCreate(
        IDbFunction function,
        Dictionary<IAnnotatable, string> metadataVariables,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (metadataVariables.TryGetValue(function, out var functionVariable))
        {
            return functionVariable;
        }

        var code = Dependencies.CSharpHelper;
        functionVariable = code.Identifier(function.Name, parameters.ScopeVariables, capitalize: false);
        metadataVariables.Add(function, functionVariable);
        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .AppendLine($"var {functionVariable} = (IRuntimeDbFunction)this.FindDbFunction({code.Literal(function.ModelName)})!;");

        return functionVariable;
    }

    private string GetOrCreate(
        IStoreStoredProcedure storeStoredProcedure,
        Dictionary<IAnnotatable, string> metadataVariables,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (metadataVariables.TryGetValue(storeStoredProcedure, out var storedProcedureVariable))
        {
            return storedProcedureVariable;
        }

        var code = Dependencies.CSharpHelper;
        storedProcedureVariable = code.Identifier(storeStoredProcedure.Name + "StoreSproc", parameters.ScopeVariables, capitalize: false);
        metadataVariables.Add(storeStoredProcedure, storedProcedureVariable);
        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .Append($"var {storedProcedureVariable} = new StoreStoredProcedure(")
            .Append($"{code.Literal(storeStoredProcedure.Name)}, {code.Literal(storeStoredProcedure.Schema)}")
            .AppendLine($", {parameters.TargetName});");

        var sprocParameters = parameters with { TargetName = storedProcedureVariable };

        var returnValue = storeStoredProcedure.ReturnValue;
        if (returnValue != null)
        {
            mainBuilder
                .Append($"{storedProcedureVariable}.ReturnValue = new StoreStoredProcedureReturnValue(")
                .AppendLine($"\"\", {code.Literal(returnValue.StoreType)}, {storedProcedureVariable});");
        }

        foreach (var parameter in storeStoredProcedure.Parameters)
        {
            Create(parameter, metadataVariables, sprocParameters);
        }

        foreach (var column in storeStoredProcedure.ResultColumns)
        {
            Create(column, metadataVariables, sprocParameters);
        }

        foreach (var storedProcedure in storeStoredProcedure.StoredProcedures)
        {
            mainBuilder
                .Append($"{storedProcedureVariable}.AddStoredProcedure(")
                .Append(CreateFindSnippet(storedProcedure, metadataVariables))
                .AppendLine(");");
        }

        CreateAnnotations(
            storeStoredProcedure,
            Generate,
            sprocParameters);

        mainBuilder
            .Append($"{parameters.TargetName}.StoredProcedures.Add(")
            .AppendLine(
                $"({code.Literal(storeStoredProcedure.Name)}, {code.Literal(storeStoredProcedure.Schema)}), {storedProcedureVariable});");

        return storedProcedureVariable;
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="storedProcedure">The stored procedure to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(IStoreStoredProcedure storedProcedure, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private string CreateFindSnippet(
        IStoredProcedure storedProcedure,
        Dictionary<IAnnotatable, string> metadataVariables)
    {
        if (metadataVariables.TryGetValue(storedProcedure, out var storedProcedureVariable))
        {
            return storedProcedureVariable;
        }

        var entityTypeVariable = metadataVariables[storedProcedure.EntityType];

        var storeObjectType = storedProcedure.GetStoreIdentifier().StoreObjectType;
        var methodName = storeObjectType switch
        {
            StoreObjectType.InsertStoredProcedure => "GetInsertStoredProcedure",
            StoreObjectType.DeleteStoredProcedure => "GetDeleteStoredProcedure",
            StoreObjectType.UpdateStoredProcedure => "GetUpdateStoredProcedure",
            _ => throw new Exception("Unexpected stored procedure type: " + storeObjectType)
        };

        return $"(IRuntimeStoredProcedure){entityTypeVariable}.{methodName}()!";
    }

    private void Create(
        IColumnBase column,
        Dictionary<IAnnotatable, string> metadataVariables,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var code = Dependencies.CSharpHelper;
        var columnVariable = code.Identifier(column.Name + "ColumnBase", parameters.ScopeVariables, capitalize: false);
        metadataVariables.Add(column, columnVariable);
        var mainBuilder = parameters.MainBuilder;
        var columnType = column is JsonColumnBase ? "JsonColumnBase" : "ColumnBase<ColumnMappingBase>";
        mainBuilder
            .Append($"var {columnVariable} = new {columnType}(")
            .Append($"{code.Literal(column.Name)}, {code.Literal(column.StoreType)}, {parameters.TargetName})");
        GenerateIsNullableInitializer(column.IsNullable, mainBuilder, code)
            .AppendLine(";")
            .AppendLine($"{parameters.TargetName}.Columns.Add({code.Literal(column.Name)}, {columnVariable});");

        CreateAnnotations(
            column,
            Generate,
            parameters with { TargetName = columnVariable });
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="column">The column to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(IColumnBase column, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private void Create(
        IColumn column,
        Dictionary<IAnnotatable, string> metadataVariables,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var code = Dependencies.CSharpHelper;
        var columnVariable = code.Identifier(column.Name + "Column", parameters.ScopeVariables, capitalize: false);
        metadataVariables.Add(column, columnVariable);
        var mainBuilder = parameters.MainBuilder;
        var columnType = column is JsonColumn ? "JsonColumn" : "Column";
        mainBuilder
            .Append($"var {columnVariable} = new {columnType}(")
            .Append($"{code.Literal(column.Name)}, {code.Literal(column.StoreType)}, {parameters.TargetName})");
        GenerateIsNullableInitializer(column.IsNullable, mainBuilder, code)
            .AppendLine(";")
            .AppendLine($"{parameters.TargetName}.Columns.Add({code.Literal(column.Name)}, {columnVariable});");

        CreateAnnotations(
            column,
            Generate,
            parameters with { TargetName = columnVariable });
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="column">The column to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(IColumn column, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private void Create(
        IViewColumn column,
        Dictionary<IAnnotatable, string> metadataVariables,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var code = Dependencies.CSharpHelper;
        var columnVariable = code.Identifier(column.Name + "ViewColumn", parameters.ScopeVariables, capitalize: false);
        metadataVariables.Add(column, columnVariable);
        var mainBuilder = parameters.MainBuilder;
        var columnType = column is JsonViewColumn ? "JsonViewColumn" : "ViewColumn";
        mainBuilder
            .Append($"var {columnVariable} = new {columnType}(")
            .Append($"{code.Literal(column.Name)}, {code.Literal(column.StoreType)}, {parameters.TargetName})");
        GenerateIsNullableInitializer(column.IsNullable, mainBuilder, code)
            .AppendLine(";")
            .AppendLine($"{parameters.TargetName}.Columns.Add({code.Literal(column.Name)}, {columnVariable});");

        CreateAnnotations(
            column,
            Generate,
            parameters with { TargetName = columnVariable });
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="column">The column to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(IViewColumn column, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private void Create(
        ISqlQueryColumn column,
        Dictionary<IAnnotatable, string> metadataVariables,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var code = Dependencies.CSharpHelper;
        var columnVariable = code.Identifier(column.Name + "SqlQueryColumn", parameters.ScopeVariables, capitalize: false);
        metadataVariables.Add(column, columnVariable);
        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .Append($"var {columnVariable} = new SqlQueryColumn(")
            .Append($"{code.Literal(column.Name)}, {code.Literal(column.StoreType)}, {parameters.TargetName})");
        GenerateIsNullableInitializer(column.IsNullable, mainBuilder, code)
            .AppendLine(";")
            .AppendLine($"{parameters.TargetName}.Columns.Add({code.Literal(column.Name)}, {columnVariable});");

        CreateAnnotations(
            column,
            Generate,
            parameters with { TargetName = columnVariable });
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="column">The column to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(ISqlQueryColumn column, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private void Create(
        IFunctionColumn column,
        Dictionary<IAnnotatable, string> metadataVariables,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var code = Dependencies.CSharpHelper;
        var columnVariable = code.Identifier(column.Name + "FunctionColumn", parameters.ScopeVariables, capitalize: false);
        metadataVariables.Add(column, columnVariable);
        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .Append($"var {columnVariable} = new FunctionColumn(")
            .Append($"{code.Literal(column.Name)}, {code.Literal(column.StoreType)}, {parameters.TargetName})");
        GenerateIsNullableInitializer(column.IsNullable, mainBuilder, code)
            .AppendLine(";")
            .AppendLine($"{parameters.TargetName}.Columns.Add({code.Literal(column.Name)}, {columnVariable});");

        CreateAnnotations(
            column,
            Generate,
            parameters with { TargetName = columnVariable });
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="column">The column to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(IFunctionColumn column, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="parameter">The parameter to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(IStoreFunctionParameter parameter, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private void Create(
        IStoreStoredProcedureResultColumn column,
        Dictionary<IAnnotatable, string> metadataVariables,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var code = Dependencies.CSharpHelper;
        var columnVariable = code.Identifier(column.Name + "FunctionColumn", parameters.ScopeVariables, capitalize: false);
        metadataVariables.Add(column, columnVariable);
        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .Append($"var {columnVariable} = new StoreStoredProcedureResultColumn({code.Literal(column.Name)}, ")
            .Append($"{code.Literal(column.StoreType)}, {code.Literal(column.Position)}, {parameters.TargetName})");
        GenerateIsNullableInitializer(column.IsNullable, mainBuilder, code)
            .AppendLine(";")
            .AppendLine($"{parameters.TargetName}.AddResultColumn({columnVariable});");

        CreateAnnotations(
            column,
            Generate,
            parameters with { TargetName = columnVariable });
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="column">The column to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(IStoreStoredProcedureResultColumn column, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private void Create(
        IStoreStoredProcedureParameter parameter,
        Dictionary<IAnnotatable, string> metadataVariables,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var code = Dependencies.CSharpHelper;
        var parameterVariable = code.Identifier(parameter.Name + "Parameter", parameters.ScopeVariables, capitalize: false);
        metadataVariables.Add(parameter, parameterVariable);
        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .Append($"var {parameterVariable} = new StoreStoredProcedureParameter({code.Literal(parameter.Name)}, ")
            .Append($"{code.Literal(parameter.StoreType)}, {code.Literal(parameter.Position)}, {parameters.TargetName}")
            .Append($", {code.Literal(parameter.Direction, fullName: true)})");
        GenerateIsNullableInitializer(parameter.IsNullable, mainBuilder, code)
            .AppendLine(";")
            .AppendLine($"{parameters.TargetName}.AddParameter({parameterVariable});");

        CreateAnnotations(
            parameter,
            Generate,
            parameters with { TargetName = parameterVariable });
    }

    private static IndentedStringBuilder GenerateIsNullableInitializer(
        bool isNullable,
        IndentedStringBuilder mainBuilder,
        ICSharpHelper code)
    {
        if (isNullable)
        {
            mainBuilder
                .AppendLine()
                .AppendLine("{").IncrementIndent()
                .AppendLine($"IsNullable = {code.Literal(isNullable)}").DecrementIndent()
                .Append("}");
        }

        return mainBuilder;
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="parameter">The parameter to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(IStoreStoredProcedureParameter parameter, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private void Create(
        IUniqueConstraint uniqueConstraint,
        IEnumerable<string> columns,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var code = Dependencies.CSharpHelper;
        var uniqueConstraintVariable = code.Identifier(uniqueConstraint.Name, parameters.ScopeVariables, capitalize: false);
        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .Append("var ").Append(uniqueConstraintVariable).Append(" = new ").Append("UniqueConstraint").Append("(")
            .Append(code.Literal(uniqueConstraint.Name)).Append(", ")
            .Append(parameters.TargetName).Append(", ")
            .Append("new[] { ").AppendJoin(columns).AppendLine(" });");

        if (uniqueConstraint.GetIsPrimaryKey())
        {
            mainBuilder
                .Append(parameters.TargetName).Append(".PrimaryKey = ").Append(uniqueConstraintVariable).AppendLine(";");
        }

        CreateAnnotations(
            uniqueConstraint,
            Generate,
            parameters with { TargetName = uniqueConstraintVariable });

        foreach (var mappedForeignKey in uniqueConstraint.MappedKeys)
        {
            var keyVariable = code.Identifier(uniqueConstraintVariable + "Uc", parameters.ScopeVariables, capitalize: false);

            mainBuilder
                .AppendLine($"var {keyVariable} = RelationalModel.GetKey(this,").IncrementIndent()
                .AppendLine($"{code.Literal(mappedForeignKey.DeclaringEntityType.Name)},")
                .AppendLine($"{code.Literal(mappedForeignKey.Properties.Select(p => p.Name).ToArray())});")
                .DecrementIndent();

            mainBuilder.AppendLine($"{uniqueConstraintVariable}.MappedKeys.Add({keyVariable});");
            mainBuilder.AppendLine($"RelationalModel.GetOrCreateUniqueConstraints({keyVariable}).Add({uniqueConstraintVariable});");
        }

        mainBuilder
            .Append($"{parameters.TargetName}.UniqueConstraints.Add({code.Literal(uniqueConstraint.Name)}, ")
            .AppendLine($"{uniqueConstraintVariable});");
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="uniqueConstraint">The unique constraint to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(IUniqueConstraint uniqueConstraint, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private void Create(
        ITableIndex index,
        IEnumerable<string> columns,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var code = Dependencies.CSharpHelper;
        var indexVariable = code.Identifier(index.Name, parameters.ScopeVariables, capitalize: false);
        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .Append("var ").Append(indexVariable).Append(" = new ").Append("TableIndex").AppendLine("(")
            .Append(code.Literal(index.Name)).Append(", ")
            .Append(parameters.TargetName).Append(", ")
            .Append("new[] { ").AppendJoin(columns).Append(" }, ")
            .Append(code.Literal(index.IsUnique)).AppendLine(");");

        CreateAnnotations(
            index,
            Generate,
            parameters with { TargetName = indexVariable });

        foreach (var mappedIndex in index.MappedIndexes)
        {
            var tableIndexVariable = code.Identifier(indexVariable + "Ix", parameters.ScopeVariables, capitalize: false);

            mainBuilder
                .AppendLine($"var {tableIndexVariable} = RelationalModel.GetIndex(this,").IncrementIndent()
                .AppendLine($"{code.Literal(mappedIndex.DeclaringEntityType.Name)},")
                .AppendLine(
                    $"{(mappedIndex.Name == null
                        ? code.Literal(mappedIndex.Properties.Select(p => p.Name).ToArray())
                        : code.Literal(mappedIndex.Name))});")
                .DecrementIndent();

            mainBuilder.AppendLine($"{indexVariable}.MappedIndexes.Add({tableIndexVariable});");
            mainBuilder.AppendLine($"RelationalModel.GetOrCreateTableIndexes({tableIndexVariable}).Add({indexVariable});");
        }

        mainBuilder
            .AppendLine($"{parameters.TargetName}.Indexes.Add({code.Literal(index.Name)}, {indexVariable});");
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="index">The unique constraint to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(ITableIndex index, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private void Create(
        IForeignKeyConstraint foreignKey,
        Dictionary<IAnnotatable, string> metadataVariables,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var code = Dependencies.CSharpHelper;
        var principalTableVariable = metadataVariables[foreignKey.PrincipalTable];
        var foreignKeyConstraintVariable = code.Identifier(foreignKey.Name, parameters.ScopeVariables, capitalize: false);

        AddNamespace(typeof(ReferentialAction), parameters.Namespaces);
        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .AppendLine($"var {foreignKeyConstraintVariable} = new ForeignKeyConstraint(").IncrementIndent()
            .AppendLine($"{code.Literal(foreignKey.Name)}, {parameters.TargetName}, {principalTableVariable},")
            .Append("new[] { ").AppendJoin(foreignKey.Columns.Select(c => metadataVariables[c])).AppendLine(" },")
            .Append($"{principalTableVariable}.FindUniqueConstraint({code.Literal(foreignKey.PrincipalUniqueConstraint.Name)})!, ")
            .Append(code.Literal(foreignKey.OnDeleteAction)).AppendLine(");").DecrementIndent();

        CreateAnnotations(
            foreignKey,
            Generate,
            parameters with { TargetName = foreignKeyConstraintVariable });

        foreach (var mappedForeignKey in foreignKey.MappedForeignKeys)
        {
            var foreignKeyVariable = code.Identifier(foreignKeyConstraintVariable + "Fk", parameters.ScopeVariables, capitalize: false);

            mainBuilder
                .AppendLine($"var {foreignKeyVariable} = RelationalModel.GetForeignKey(this,").IncrementIndent()
                .AppendLine($"{code.Literal(mappedForeignKey.DeclaringEntityType.Name)},")
                .AppendLine($"{code.Literal(mappedForeignKey.Properties.Select(p => p.Name).ToArray())},")
                .AppendLine($"{code.Literal(mappedForeignKey.PrincipalEntityType.Name)},")
                .AppendLine($"{code.Literal(mappedForeignKey.PrincipalKey.Properties.Select(p => p.Name).ToArray())});")
                .DecrementIndent();

            mainBuilder.AppendLine($"{foreignKeyConstraintVariable}.MappedForeignKeys.Add({foreignKeyVariable});");
            mainBuilder.AppendLine(
                $"RelationalModel.GetOrCreateForeignKeyConstraints({foreignKeyVariable}).Add({foreignKeyConstraintVariable});");
        }

        mainBuilder
            .AppendLine($"{metadataVariables[foreignKey.Table]}.ForeignKeyConstraints.Add({foreignKeyConstraintVariable});")
            .AppendLine($"{principalTableVariable}.ReferencingForeignKeyConstraints.Add({foreignKeyConstraintVariable});");
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="foreignKey">The foreign key to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(IForeignKeyConstraint foreignKey, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private void Create(
        ITableMappingBase tableMapping,
        string tableMappingsVariable,
        Dictionary<IAnnotatable, string> metadataVariables,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var code = Dependencies.CSharpHelper;
        var mainBuilder = parameters.MainBuilder;
        var typeBase = tableMapping.TypeBase;
        var typeBaseVariable = metadataVariables[typeBase];

        var table = tableMapping.Table;
        var tableVariable = GetOrCreate(table, metadataVariables, parameters);
        var tableMappingVariable = code.Identifier(table.Name + "MappingBase", parameters.ScopeVariables, capitalize: false);

        GenerateAddMapping(
            tableMapping,
            tableVariable,
            typeBaseVariable,
            tableMappingsVariable,
            tableMappingVariable,
            "TableMappingBase<ColumnMappingBase>",
            parameters);

        CreateAnnotations(
            tableMapping,
            Generate,
            parameters with { TargetName = tableMappingVariable });

        foreach (var columnMapping in tableMapping.ColumnMappings)
        {
            mainBuilder
                .Append("RelationalModel.CreateColumnMapping(")
                .Append($"(ColumnBase<ColumnMappingBase>){metadataVariables[columnMapping.Column]}, ")
                .Append($"{typeBaseVariable}.FindProperty({code.Literal(columnMapping.Property.Name)})!, ")
                .Append(tableMappingVariable).AppendLine(");");
        }
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="tableMapping">The table mapping to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(ITableMappingBase tableMapping, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private void Create(
        ITableMapping tableMapping,
        string tableMappingsVariable,
        Dictionary<IAnnotatable, string> metadataVariables,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var code = Dependencies.CSharpHelper;
        var mainBuilder = parameters.MainBuilder;
        var typeBase = tableMapping.TypeBase;
        var typeBaseVariable = metadataVariables[typeBase];

        var table = tableMapping.Table;
        var tableVariable = GetOrCreate(table, metadataVariables, parameters);
        var tableMappingVariable = code.Identifier(table.Name + "TableMapping", parameters.ScopeVariables, capitalize: false);
        metadataVariables.Add(tableMapping, tableMappingVariable);

        GenerateAddMapping(
            tableMapping,
            tableVariable,
            typeBaseVariable,
            tableMappingsVariable,
            tableMappingVariable,
            "TableMapping",
            parameters);

        CreateAnnotations(
            tableMapping,
            Generate,
            parameters with { TargetName = tableMappingVariable });

        foreach (var columnMapping in tableMapping.ColumnMappings)
        {
            mainBuilder
                .Append($"RelationalModel.CreateColumnMapping({metadataVariables[columnMapping.Column]}, ")
                .Append($"{typeBaseVariable}.FindProperty({code.Literal(columnMapping.Property.Name)})!, ")
                .Append(tableMappingVariable).AppendLine(");");
        }
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="tableMapping">The table mapping to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(ITableMapping tableMapping, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private void Create(
        IViewMapping viewMapping,
        string viewMappingsVariable,
        Dictionary<IAnnotatable, string> metadataVariables,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var code = Dependencies.CSharpHelper;
        var mainBuilder = parameters.MainBuilder;
        var typeBase = viewMapping.TypeBase;
        var typeBaseVariable = metadataVariables[typeBase];

        var view = viewMapping.View;
        var viewVariable = GetOrCreate(view, metadataVariables, parameters);
        var viewMappingVariable = code.Identifier(view.Name + "ViewMapping", parameters.ScopeVariables, capitalize: false);

        GenerateAddMapping(
            viewMapping,
            viewVariable,
            typeBaseVariable,
            viewMappingsVariable,
            viewMappingVariable,
            "ViewMapping",
            parameters);

        CreateAnnotations(
            viewMapping,
            Generate,
            parameters with { TargetName = viewMappingVariable });

        foreach (var columnMapping in viewMapping.ColumnMappings)
        {
            mainBuilder
                .Append($"RelationalModel.CreateViewColumnMapping({metadataVariables[columnMapping.Column]}, ")
                .Append($"{typeBaseVariable}.FindProperty({code.Literal(columnMapping.Property.Name)})!, ")
                .Append(viewMappingVariable).AppendLine(");");
        }
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="viewMapping">The view mapping to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(IViewMapping viewMapping, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private void Create(
        ISqlQueryMapping sqlQueryMapping,
        string sqlQueryMappingsVariable,
        Dictionary<IAnnotatable, string> metadataVariables,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var code = Dependencies.CSharpHelper;
        var mainBuilder = parameters.MainBuilder;
        var typeBase = sqlQueryMapping.TypeBase;
        var typeBaseVariable = metadataVariables[typeBase];

        var sqlQuery = sqlQueryMapping.SqlQuery;
        var sqlQueryVariable = GetOrCreate(sqlQuery, metadataVariables, parameters);
        var sqlQueryMappingVariable = code.Identifier(sqlQuery.Name + "SqlQueryMapping", parameters.ScopeVariables, capitalize: false);

        GenerateAddMapping(
            sqlQueryMapping,
            sqlQueryVariable,
            typeBaseVariable,
            sqlQueryMappingsVariable,
            sqlQueryMappingVariable,
            "SqlQueryMapping",
            parameters);

        if (sqlQueryMapping.IsDefaultSqlQueryMapping)
        {
            mainBuilder
                .AppendLine($"{sqlQueryMappingVariable}.IsDefaultSqlQueryMapping = {code.Literal(true)};");
        }

        CreateAnnotations(
            sqlQueryMapping,
            Generate,
            parameters with { TargetName = sqlQueryMappingVariable });

        foreach (var columnMapping in sqlQueryMapping.ColumnMappings)
        {
            mainBuilder
                .Append($"RelationalModel.CreateSqlQueryColumnMapping({metadataVariables[columnMapping.Column]}, ")
                .Append($"{typeBaseVariable}.FindProperty({code.Literal(columnMapping.Property.Name)})!, ")
                .Append(sqlQueryMappingVariable).AppendLine(");");
        }
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="sqlQueryMapping">The SQL query mapping to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(ISqlQueryMapping sqlQueryMapping, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private void Create(
        IFunctionMapping functionMapping,
        string functionMappingsVariable,
        Dictionary<IAnnotatable, string> metadataVariables,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var code = Dependencies.CSharpHelper;
        var mainBuilder = parameters.MainBuilder;
        var typeBase = functionMapping.TypeBase;
        var typeBaseVariable = metadataVariables[typeBase];

        var storeFunction = functionMapping.StoreFunction;
        var functionVariable = GetOrCreate(storeFunction, metadataVariables, parameters);
        var dbFunctionVariable = metadataVariables[functionMapping.DbFunction];
        var functionMappingVariable = code.Identifier(storeFunction.Name + "FunctionMapping", parameters.ScopeVariables, capitalize: false);

        GenerateAddMapping(
            functionMapping,
            functionVariable,
            typeBaseVariable,
            functionMappingsVariable,
            functionMappingVariable,
            "FunctionMapping",
            parameters,
            $"{dbFunctionVariable}, ");

        if (functionMapping.IsDefaultFunctionMapping)
        {
            mainBuilder
                .AppendLine($"{functionMappingVariable}.IsDefaultFunctionMapping = {code.Literal(true)};");
        }

        CreateAnnotations(
            functionMapping,
            Generate,
            parameters with { TargetName = functionMappingVariable });

        foreach (var columnMapping in functionMapping.ColumnMappings)
        {
            mainBuilder
                .Append($"RelationalModel.CreateFunctionColumnMapping({metadataVariables[columnMapping.Column]}, ")
                .Append($"{typeBaseVariable}.FindProperty({code.Literal(columnMapping.Property.Name)})!, ")
                .Append(functionMappingVariable).AppendLine(");");
        }
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="functionMapping">The function mapping to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(IFunctionMapping functionMapping, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private void Create(
        IStoredProcedureMapping sprocMapping,
        string sprocMappingsVariable,
        StoreObjectType storeObjectType,
        Dictionary<IAnnotatable, string> metadataVariables,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var code = Dependencies.CSharpHelper;
        var mainBuilder = parameters.MainBuilder;
        var typeBase = sprocMapping.TypeBase;
        var typeBaseVariable = metadataVariables[typeBase];

        var storeSproc = sprocMapping.StoreStoredProcedure;
        var storeSprocVariable = GetOrCreate(storeSproc, metadataVariables, parameters);

        var sprocMappingName = storeObjectType switch
        {
            StoreObjectType.InsertStoredProcedure => "InsertStoredProcedureMapping",
            StoreObjectType.DeleteStoredProcedure => "DeleteStoredProcedureMapping",
            StoreObjectType.UpdateStoredProcedure => "UpdateStoredProcedureMapping",
            _ => throw new Exception("Unexpected stored procedure type: " + storeObjectType)
        };

        var sprocSnippet = CreateFindSnippet(sprocMapping.StoredProcedure, metadataVariables);
        var sprocVariable = code.Identifier(storeSproc.Name + sprocMappingName[0] + "Sproc", parameters.ScopeVariables, capitalize: false);
        mainBuilder
            .AppendLine($"var {sprocVariable} = {CreateFindSnippet(sprocMapping.StoredProcedure, metadataVariables)};");

        var sprocMappingVariable = code.Identifier(storeSproc.Name + "SprocMapping", parameters.ScopeVariables, capitalize: false);
        var tableMappingVariable = sprocMapping.TableMapping != null ? metadataVariables[sprocMapping.TableMapping] : null;

        GenerateAddMapping(
            sprocMapping,
            storeSprocVariable,
            typeBaseVariable,
            sprocMappingsVariable,
            sprocMappingVariable,
            "StoredProcedureMapping",
            parameters,
            $"{sprocSnippet}, {tableMappingVariable ?? "null"}, ");

        if (tableMappingVariable != null)
        {
            mainBuilder
                .AppendLine($"{tableMappingVariable}.{sprocMappingName} = {sprocMappingVariable};");
        }

        CreateAnnotations(
            sprocMapping,
            Generate,
            parameters with { TargetName = sprocMappingVariable });

        foreach (var parameterMapping in sprocMapping.ParameterMappings)
        {
            mainBuilder
                .Append($"RelationalModel.CreateStoredProcedureParameterMapping({metadataVariables[parameterMapping.StoreParameter]}, ")
                .Append($"{sprocVariable}.FindParameter({code.Literal(parameterMapping.Parameter.Name)})!, ")
                .Append($"{typeBaseVariable}.FindProperty({code.Literal(parameterMapping.Property.Name)})!, ")
                .Append(sprocMappingVariable).AppendLine(");");
        }

        foreach (var columnMapping in sprocMapping.ResultColumnMappings)
        {
            mainBuilder
                .Append($"RelationalModel.CreateStoredProcedureResultColumnMapping({metadataVariables[columnMapping.StoreResultColumn]}, ")
                .Append($"{sprocVariable}.FindResultColumn({code.Literal(columnMapping.ResultColumn.Name)})!, ")
                .Append($"{typeBaseVariable}.FindProperty({code.Literal(columnMapping.Property.Name)})!, ")
                .Append(sprocMappingVariable).AppendLine(");");
        }
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="sprocMapping">The stored procedure mapping to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(IStoredProcedureMapping sprocMapping, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private void GenerateAddMapping(
        ITableMappingBase tableMapping,
        string tableVariable,
        string entityTypeVariable,
        string tableMappingsVariable,
        string tableMappingVariable,
        string mappingType,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
        string? additionalParameter = null)
    {
        var code = Dependencies.CSharpHelper;
        var mainBuilder = parameters.MainBuilder;
        var typeBase = tableMapping.TypeBase;

        mainBuilder
            .Append($"var {tableMappingVariable} = new {mappingType}({entityTypeVariable}, ")
            .Append($"{tableVariable}, {additionalParameter ?? ""}{code.Literal(tableMapping.IncludesDerivedTypes)}");

        if (tableMapping.IsSharedTablePrincipal.HasValue
            || tableMapping.IsSplitEntityTypePrincipal.HasValue)
        {
            mainBuilder.AppendLine(")")
                .AppendLine("{").IncrementIndent();

            if (tableMapping.IsSharedTablePrincipal.HasValue)
            {
                mainBuilder
                    .Append("IsSharedTablePrincipal = ").Append(code.Literal(tableMapping.IsSharedTablePrincipal)).AppendLine(",");
            }

            if (tableMapping.IsSplitEntityTypePrincipal.HasValue)
            {
                mainBuilder
                    .Append("IsSplitEntityTypePrincipal = ").AppendLine(code.Literal(tableMapping.IsSplitEntityTypePrincipal));
            }

            mainBuilder.DecrementIndent().AppendLine("};");
        }
        else
        {
            mainBuilder.AppendLine(");");
        }

        var table = tableMapping.Table;
        var isOptional = table.IsOptional(typeBase);
        mainBuilder
            .AppendLine($"{tableVariable}.AddTypeMapping({tableMappingVariable}, {code.Literal(isOptional)});")
            .AppendLine($"{tableMappingsVariable}.Add({tableMappingVariable});");

        if (typeBase is IEntityType entityType)
        {
            foreach (var internalForeignKey in table.GetRowInternalForeignKeys(entityType))
            {
                mainBuilder
                    .Append(tableVariable).Append($".AddRowInternalForeignKey({entityTypeVariable}, ")
                    .AppendLine("RelationalModel.GetForeignKey(this,").IncrementIndent()
                    .AppendLine($"{code.Literal(internalForeignKey.DeclaringEntityType.Name)},")
                    .AppendLine($"{code.Literal(internalForeignKey.Properties.Select(p => p.Name).ToArray())},")
                    .AppendLine($"{code.Literal(internalForeignKey.PrincipalEntityType.Name)},")
                    .AppendLine($"{code.Literal(internalForeignKey.PrincipalKey.Properties.Select(p => p.Name).ToArray())}));")
                    .DecrementIndent();
            }
        }
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
            AddNamespace(method.DeclaringType!, parameters.Namespaces);
            mainBuilder.AppendLine(",")
                .AppendLine($"methodInfo: {code.Literal(method.DeclaringType!)}.GetMethod(").IncrementIndent()
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

        parameters = parameters with { TargetName = functionVariable };
        foreach (var parameter in function.Parameters)
        {
            Create(parameter, parameters);
        }

        if (function.TypeMapping != null)
        {
            mainBuilder.Append(functionVariable).Append(".TypeMapping = ");
            Create(function.TypeMapping, parameters with { TargetName = functionVariable });
            mainBuilder.AppendLine(";");
        }

        CreateAnnotations(
            function,
            Generate,
            parameters);

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
        => GenerateSimpleAnnotations(parameters);

    private void Create(IDbFunctionParameter parameter, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
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

        mainBuilder.Append(parameterVariable).Append(".TypeMapping = ");
        Create(parameter.TypeMapping!, parameters with { TargetName = parameterVariable });
        mainBuilder.AppendLine(";");

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
        => GenerateSimpleAnnotations(parameters);

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

        if (sequence.IsCached && sequence.CacheSize.HasValue)
        {
            mainBuilder
                .AppendLine(",")
                .Append("cached: ")
                .Append(code.Literal(sequence.IsCached))
                .AppendLine(",")
                .Append("cacheSize: ")
                .Append(code.Literal(sequence.CacheSize));
        }
        else if (!sequence.IsCached)
        {
            mainBuilder
                .AppendLine(",")
                .Append("cached: ")
                .Append(code.Literal(sequence.IsCached));
        }

        if (sequence.ModelSchema is null && sequence.Schema is not null)
        {
            mainBuilder.AppendLine(",")
                .Append("modelSchemaIsNull: ").Append(code.Literal(true));
        }

        mainBuilder.AppendLine(");").DecrementIndent()
            .AppendLine();

        CreateAnnotations(
            sequence,
            Generate,
            parameters with { TargetName = sequenceVariable });

        mainBuilder
            .Append(sequencesVariable).Append("[(").Append(code.Literal(sequence.Name)).Append(", ")
            .Append(code.Literal(sequence.ModelSchema)).Append(")] = ")
            .Append(sequenceVariable).AppendLine(";")
            .AppendLine();
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="sequence">The sequence to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(ISequence sequence, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

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

            if (annotations.TryGetAndRemove(
                    RelationalAnnotationNames.MappingFragments,
                    out IReadOnlyStoreObjectDictionary<IEntityTypeMappingFragment> fragments))
            {
                AddNamespace(typeof(StoreObjectDictionary<RuntimeEntityTypeMappingFragment>), parameters.Namespaces);
                AddNamespace(typeof(StoreObjectIdentifier), parameters.Namespaces);
                var fragmentsVariable = Dependencies.CSharpHelper.Identifier("fragments", parameters.ScopeVariables, capitalize: false);
                parameters.MainBuilder
                    .Append("var ").Append(fragmentsVariable)
                    .AppendLine(" = new StoreObjectDictionary<RuntimeEntityTypeMappingFragment>();");

                foreach (var fragment in fragments.GetValues())
                {
                    Create(fragment, fragmentsVariable, parameters);
                }

                GenerateSimpleAnnotation(RelationalAnnotationNames.MappingFragments, fragmentsVariable, parameters);
            }

            if (annotations.TryGetAndRemove(
                    RelationalAnnotationNames.InsertStoredProcedure,
                    out StoredProcedure insertStoredProcedure))
            {
                var sprocVariable = Dependencies.CSharpHelper.Identifier("insertSproc", parameters.ScopeVariables, capitalize: false);

                Create(insertStoredProcedure, sprocVariable, parameters);

                GenerateSimpleAnnotation(RelationalAnnotationNames.InsertStoredProcedure, sprocVariable, parameters);
                parameters.MainBuilder.AppendLine();
            }

            if (annotations.TryGetAndRemove(
                    RelationalAnnotationNames.DeleteStoredProcedure,
                    out StoredProcedure deleteStoredProcedure))
            {
                var sprocVariable = Dependencies.CSharpHelper.Identifier("deleteSproc", parameters.ScopeVariables, capitalize: false);

                Create(deleteStoredProcedure, sprocVariable, parameters);

                GenerateSimpleAnnotation(RelationalAnnotationNames.DeleteStoredProcedure, sprocVariable, parameters);
                parameters.MainBuilder.AppendLine();
            }

            if (annotations.TryGetAndRemove(
                    RelationalAnnotationNames.UpdateStoredProcedure,
                    out StoredProcedure updateStoredProcedure))
            {
                var sprocVariable = Dependencies.CSharpHelper.Identifier("updateSproc", parameters.ScopeVariables, capitalize: false);

                Create(updateStoredProcedure, sprocVariable, parameters);

                GenerateSimpleAnnotation(RelationalAnnotationNames.UpdateStoredProcedure, sprocVariable, parameters);
                parameters.MainBuilder.AppendLine();
            }
        }

        base.Generate(entityType, parameters);
    }

    /// <inheritdoc />
    public override void Generate(IComplexType complexType, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var annotations = parameters.Annotations;
        if (parameters.IsRuntime)
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
            // These need to be set explicitly to prevent default values from being generated
            annotations[RelationalAnnotationNames.TableName] = complexType.GetTableName();
            annotations[RelationalAnnotationNames.Schema] = complexType.GetSchema();
            annotations[RelationalAnnotationNames.ViewName] = complexType.GetViewName();
            annotations[RelationalAnnotationNames.ViewSchema] = complexType.GetViewSchema();
            annotations[RelationalAnnotationNames.SqlQuery] = complexType.GetSqlQuery();
            annotations[RelationalAnnotationNames.FunctionName] = complexType.GetFunctionName();
        }

        base.Generate(complexType, parameters);
    }

    private void Create(
        IEntityTypeMappingFragment fragment,
        string fragmentsVariable,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var storeObject = fragment.StoreObject;
        var code = Dependencies.CSharpHelper;
        var overrideVariable =
            code.Identifier(storeObject.Name + "Fragment", parameters.ScopeVariables, capitalize: false);
        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .Append("var ").Append(overrideVariable).AppendLine(" = new RuntimeEntityTypeMappingFragment(").IncrementIndent()
            .Append(parameters.TargetName).AppendLine(",");

        AppendLiteral(storeObject, mainBuilder, code);
        mainBuilder.AppendLine(",")
            .Append(code.Literal(fragment.IsTableExcludedFromMigrations)).AppendLine(");").DecrementIndent();

        CreateAnnotations(
            fragment,
            Generate,
            parameters with { TargetName = overrideVariable });

        mainBuilder.Append(fragmentsVariable).Append(".Add(");
        AppendLiteral(storeObject, mainBuilder, code);

        mainBuilder
            .Append(", ")
            .Append(overrideVariable).AppendLine(");");
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="fragment">The fragment to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(IEntityTypeMappingFragment fragment, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private void Create(IStoredProcedure storedProcedure, string sprocVariable, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        AddNamespace(typeof(RuntimeStoredProcedure), parameters.Namespaces);

        var code = Dependencies.CSharpHelper;
        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .Append("var ").Append(sprocVariable).AppendLine(" = new RuntimeStoredProcedure(").IncrementIndent()
            .Append(parameters.TargetName).AppendLine(",")
            .Append(code.Literal(storedProcedure.Name)).AppendLine(",")
            .Append(code.Literal(storedProcedure.Schema)).AppendLine(",")
            .Append(code.Literal(storedProcedure.IsRowsAffectedReturned))
            .AppendLine(");")
            .DecrementIndent()
            .AppendLine();

        parameters = parameters with { TargetName = sprocVariable };
        foreach (var parameter in storedProcedure.Parameters)
        {
            Create(parameter, parameters);
        }

        foreach (var resultColumn in storedProcedure.ResultColumns)
        {
            Create(resultColumn, parameters);
        }

        CreateAnnotations(
            storedProcedure,
            Generate,
            parameters);
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="storedProcedure">The stored procedure to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(IStoredProcedure storedProcedure, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private void Create(IStoredProcedureParameter parameter, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var code = Dependencies.CSharpHelper;
        var mainBuilder = parameters.MainBuilder;
        var parameterVariable = code.Identifier(parameter.PropertyName ?? parameter.Name, parameters.ScopeVariables, capitalize: false);

        mainBuilder
            .Append("var ").Append(parameterVariable).Append(" = ")
            .Append(parameters.TargetName).AppendLine(".AddParameter(").IncrementIndent()
            .Append(code.Literal(parameter.Name)).Append(", ")
            .Append(code.Literal(parameter.Direction, fullName: true)).Append(", ")
            .Append(code.Literal(parameter.ForRowsAffected)).Append(", ")
            .Append(code.Literal(parameter.PropertyName!)).Append(", ")
            .Append(code.Literal(parameter.ForOriginalValue))
            .AppendLine(");").DecrementIndent();

        CreateAnnotations(
            parameter,
            Generate,
            parameters with { TargetName = parameterVariable });
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="storedProcedure">The stored procedure to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(IStoredProcedureParameter storedProcedure, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    private void Create(IStoredProcedureResultColumn resultColumn, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var code = Dependencies.CSharpHelper;
        var mainBuilder = parameters.MainBuilder;
        var resultColumnVariable = code.Identifier(resultColumn.Name, parameters.ScopeVariables, capitalize: false);

        mainBuilder
            .Append("var ").Append(resultColumnVariable).Append(" = ")
            .Append(parameters.TargetName).AppendLine(".AddResultColumn(").IncrementIndent()
            .Append(code.Literal(resultColumn.Name)).Append(", ")
            .Append(code.Literal(resultColumn.ForRowsAffected)).Append(", ")
            .Append(code.Literal(resultColumn.PropertyName!))
            .AppendLine(");").DecrementIndent();

        CreateAnnotations(
            resultColumn,
            Generate,
            parameters with { TargetName = resultColumnVariable });
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="storedProcedure">The stored procedure to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(IStoredProcedureResultColumn storedProcedure, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="constraint">The check constraint to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(ICheckConstraint constraint, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

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

            if (annotations.TryGetAndRemove(
                    RelationalAnnotationNames.RelationalOverrides,
                    out IReadOnlyStoreObjectDictionary<IRelationalPropertyOverrides> tableOverrides))
            {
                AddNamespace(typeof(StoreObjectDictionary<RuntimeRelationalPropertyOverrides>), parameters.Namespaces);
                AddNamespace(typeof(StoreObjectIdentifier), parameters.Namespaces);
                var overridesVariable = Dependencies.CSharpHelper.Identifier("overrides", parameters.ScopeVariables, capitalize: false);
                parameters.MainBuilder.AppendLine()
                    .Append("var ").Append(overridesVariable)
                    .AppendLine(" = new StoreObjectDictionary<RuntimeRelationalPropertyOverrides>();");

                foreach (var overrides in tableOverrides.GetValues())
                {
                    Create(overrides, overridesVariable, parameters);
                }

                GenerateSimpleAnnotation(RelationalAnnotationNames.RelationalOverrides, overridesVariable, parameters);
                parameters.MainBuilder.AppendLine();
            }
        }

        base.Generate(property, parameters);
    }

    private void Create(
        IRelationalPropertyOverrides overrides,
        string overridesVariable,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var storeObject = overrides.StoreObject;
        var code = Dependencies.CSharpHelper;
        var overrideVariable =
            code.Identifier(parameters.TargetName + Capitalize(storeObject.Name), parameters.ScopeVariables, capitalize: false);
        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .Append("var ").Append(overrideVariable).AppendLine(" = new RuntimeRelationalPropertyOverrides(").IncrementIndent()
            .Append(parameters.TargetName).AppendLine(",");
        AppendLiteral(storeObject, mainBuilder, code);

        mainBuilder.AppendLine(",")
            .Append(code.Literal(overrides.IsColumnNameOverridden)).AppendLine(",")
            .Append(code.Literal(overrides.ColumnName)).AppendLine(");").DecrementIndent();

        CreateAnnotations(
            overrides,
            Generate,
            parameters with { TargetName = overrideVariable });

        mainBuilder.Append(overridesVariable).Append(".Add(");
        AppendLiteral(storeObject, mainBuilder, code);

        mainBuilder
            .Append(", ")
            .Append(overrideVariable).AppendLine(");");
    }

    /// <summary>
    ///     Generates code to create the given annotations.
    /// </summary>
    /// <param name="overrides">The property overrides to which the annotations are applied.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    public virtual void Generate(IRelationalPropertyOverrides overrides, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => GenerateSimpleAnnotations(parameters);

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
    public override void Generate(IIndex index, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (parameters.IsRuntime)
        {
            parameters.Annotations.Remove(RelationalAnnotationNames.TableIndexMappings);
        }

        base.Generate(index, parameters);
    }

    /// <inheritdoc />
    public override bool Create(
        CoreTypeMapping typeMapping,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
        ValueComparer? valueComparer = null,
        ValueComparer? keyValueComparer = null,
        ValueComparer? providerValueComparer = null)
    {
        if (typeMapping is not RelationalTypeMapping relationalTypeMapping)
        {
            return base.Create(typeMapping, parameters, valueComparer, keyValueComparer, providerValueComparer);
        }

        var mainBuilder = parameters.MainBuilder;
        var code = Dependencies.CSharpHelper;
        if (IsSpatial(relationalTypeMapping))
        {
            // Spatial mappings are not supported in the compiled model
            mainBuilder.Append(code.UnknownLiteral(null));
            return false;
        }

        var defaultInstance = (RelationalTypeMapping?)CreateDefaultTypeMapping(relationalTypeMapping, parameters);
        if (defaultInstance == null)
        {
            return true;
        }

        mainBuilder
            .AppendLine(".Clone(")
            .IncrementIndent();

        mainBuilder
            .Append("comparer: ");
        Create(valueComparer ?? relationalTypeMapping.Comparer, parameters, code);

        mainBuilder.AppendLine(",")
            .Append("keyComparer: ");
        Create(keyValueComparer ?? relationalTypeMapping.KeyComparer, parameters, code);

        mainBuilder.AppendLine(",")
            .Append("providerValueComparer: ");
        Create(providerValueComparer ?? relationalTypeMapping.ProviderValueComparer, parameters, code);

        var storeTypeDifferent = relationalTypeMapping.StoreType != defaultInstance.StoreType;
        var sizeDifferent = relationalTypeMapping.Size != null
            && relationalTypeMapping.Size != defaultInstance.Size;
        var precisionDifferent = relationalTypeMapping.Precision != null
            && relationalTypeMapping.Precision != defaultInstance.Precision;
        var scaleDifferent = relationalTypeMapping.Scale != null
            && relationalTypeMapping.Scale != defaultInstance.Scale;
        var dbTypeDifferent = relationalTypeMapping.DbType != null
            && relationalTypeMapping.DbType != defaultInstance.DbType;
        var isUnicodeDifferent = relationalTypeMapping.IsUnicode != defaultInstance.IsUnicode;
        var isFixedLengthDifferent = relationalTypeMapping.IsFixedLength != defaultInstance.IsFixedLength;
        if (storeTypeDifferent
            || sizeDifferent
            || precisionDifferent
            || scaleDifferent
            || dbTypeDifferent
            || isUnicodeDifferent
            || isFixedLengthDifferent)
        {
            AddNamespace(typeof(RelationalTypeMappingInfo), parameters.Namespaces);
            mainBuilder.AppendLine(",")
                .AppendLine("mappingInfo: new RelationalTypeMappingInfo(")
                .IncrementIndent();

            var firstParameter = true;
            if (storeTypeDifferent)
            {
                GenerateArgument(
                    "storeTypeName", code.Literal(relationalTypeMapping.StoreType), mainBuilder, ref firstParameter);
            }

            if (sizeDifferent)
            {
                GenerateArgument(
                    "size", code.Literal(relationalTypeMapping.Size), mainBuilder, ref firstParameter);
            }

            if (isUnicodeDifferent)
            {
                GenerateArgument(
                    "unicode", code.Literal(relationalTypeMapping.IsUnicode), mainBuilder, ref firstParameter);
            }

            if (isFixedLengthDifferent)
            {
                GenerateArgument(
                    "fixedLength", code.Literal(relationalTypeMapping.IsFixedLength), mainBuilder, ref firstParameter);
            }

            if (precisionDifferent)
            {
                GenerateArgument(
                    "precision", code.Literal(relationalTypeMapping.Precision), mainBuilder, ref firstParameter);
            }

            if (scaleDifferent)
            {
                GenerateArgument(
                    "scale", code.Literal(relationalTypeMapping.Scale), mainBuilder, ref firstParameter);
            }

            if (dbTypeDifferent)
            {
                GenerateArgument(
                    "dbType", code.Literal(relationalTypeMapping.DbType!, fullName: true), mainBuilder, ref firstParameter);
            }

            mainBuilder
                .Append(")")
                .DecrementIndent();
        }

        if (relationalTypeMapping.Converter != null
            && relationalTypeMapping.Converter != defaultInstance.Converter)
        {
            mainBuilder.AppendLine(",")
                .Append("converter: ");

            Create(relationalTypeMapping.Converter, parameters, code);
        }

        var typeDifferent = relationalTypeMapping.Converter == null
            && relationalTypeMapping.ClrType != defaultInstance.ClrType;
        if (typeDifferent)
        {
            mainBuilder.AppendLine(",")
                .Append($"clrType: {code.Literal(relationalTypeMapping.ClrType)}");
        }

        var storeTypePostfixDifferent = relationalTypeMapping.StoreTypePostfix != defaultInstance.StoreTypePostfix;
        if (storeTypePostfixDifferent)
        {
            mainBuilder.AppendLine(",")
                .Append($"storeTypePostfix: {code.Literal(relationalTypeMapping.StoreTypePostfix)}");
        }

        if (relationalTypeMapping.JsonValueReaderWriter != null
            && relationalTypeMapping.JsonValueReaderWriter != defaultInstance.JsonValueReaderWriter)
        {
            mainBuilder.AppendLine(",")
                .Append("jsonValueReaderWriter: ");

            CreateJsonValueReaderWriter(relationalTypeMapping.JsonValueReaderWriter, parameters, code);
        }

        if (relationalTypeMapping.ElementTypeMapping != null
            && relationalTypeMapping.ElementTypeMapping != defaultInstance.ElementTypeMapping)
        {
            mainBuilder.AppendLine(",")
                .Append("elementMapping: ");

            Create(relationalTypeMapping.ElementTypeMapping, parameters);
        }

        mainBuilder
            .Append(")")
            .DecrementIndent();

        return true;

        static void GenerateArgument(string name, string value, IndentedStringBuilder builder, ref bool firstArgument)
        {
            if (!firstArgument)
            {
                builder.AppendLine(",");
            }

            firstArgument = false;
            builder.Append($"{name}: {value}");
        }

        static bool IsSpatial(RelationalTypeMapping relationalTypeMapping)
            => IsSpatialType(relationalTypeMapping.GetType())
                || (relationalTypeMapping.ElementTypeMapping is RelationalTypeMapping elementTypeMapping
                    && IsSpatialType(elementTypeMapping.GetType()));

        static bool IsSpatialType(Type relationalTypeMappingType)
            => (relationalTypeMappingType.IsGenericType
                    && relationalTypeMappingType.GetGenericTypeDefinition() == typeof(RelationalGeometryTypeMapping<,>))
                || (relationalTypeMappingType.BaseType != typeof(object)
                    && IsSpatialType(relationalTypeMappingType.BaseType!));
    }

    private static void CreateAnnotations<TAnnotatable>(
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

    private static string Capitalize(string @string)
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

    private static void AppendLiteral(StoreObjectIdentifier storeObject, IndentedStringBuilder builder, ICSharpHelper code)
    {
        builder.Append("StoreObjectIdentifier.");
        switch (storeObject.StoreObjectType)
        {
            case StoreObjectType.Table:
                builder
                    .Append("Table(").Append(code.Literal(storeObject.Name))
                    .Append(", ").Append(code.Literal(storeObject.Schema)).Append(")");
                break;
            case StoreObjectType.View:
                builder
                    .Append("View(").Append(code.Literal(storeObject.Name))
                    .Append(", ").Append(code.Literal(storeObject.Schema)).Append(")");
                break;
            case StoreObjectType.SqlQuery:
                builder
                    .Append("SqlQuery(").Append(code.Literal(storeObject.Name)).Append(")");
                break;
            case StoreObjectType.Function:
                builder
                    .Append("DbFunction(").Append(code.Literal(storeObject.Name)).Append(")");
                break;
            case StoreObjectType.InsertStoredProcedure:
                builder
                    .Append("InsertStoredProcedure(").Append(code.Literal(storeObject.Name))
                    .Append(", ").Append(code.Literal(storeObject.Schema)).Append(")");
                break;
            case StoreObjectType.DeleteStoredProcedure:
                builder
                    .Append("DeleteStoredProcedure(").Append(code.Literal(storeObject.Name))
                    .Append(", ").Append(code.Literal(storeObject.Schema)).Append(")");
                break;
            case StoreObjectType.UpdateStoredProcedure:
                builder
                    .Append("UpdateStoredProcedure(").Append(code.Literal(storeObject.Name))
                    .Append(", ").Append(code.Literal(storeObject.Schema)).Append(")");
                break;
            default:
                Check.DebugFail("Unexpected StoreObjectType: " + storeObject.StoreObjectType);
                break;
        }
    }
}
