// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CSharpRuntimeModelCodeGenerator : ICompiledModelCodeGenerator
{
    private readonly ICSharpHelper _code;
    private readonly ICSharpRuntimeAnnotationCodeGenerator _annotationCodeGenerator;

    private const string FileExtension = ".cs";
    private const string AssemblyAttributesSuffix = "AssemblyAttributes";
    private const string ModelSuffix = "Model";
    private const string ModelBuilderSuffix = "ModelBuilder";
    private const string EntityTypeSuffix = "EntityType";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CSharpRuntimeModelCodeGenerator(
        ICSharpRuntimeAnnotationCodeGenerator annotationCodeGenerator,
        ICSharpHelper cSharpHelper)
    {
        _annotationCodeGenerator = annotationCodeGenerator;
        _code = cSharpHelper;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Language
        => "C#";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyCollection<ScaffoldedFile> GenerateModel(
        IModel model,
        CompiledModelCodeGenerationOptions options)
    {
        // Translated expressions don't have nullability annotations
        var nullable = false;
        var scaffoldedFiles = new List<ScaffoldedFile>();

        var assemblyAttributesCode = CreateAssemblyAttributes(options.ModelNamespace, options.ContextType, nullable);
        var assemblyInfoFileName = options.ContextType.ShortDisplayName() + AssemblyAttributesSuffix + FileExtension;
        scaffoldedFiles.Add(new ScaffoldedFile { Path = assemblyInfoFileName, Code = assemblyAttributesCode });

        var modelCode = CreateModel(options.ModelNamespace, options.ContextType, nullable);
        var modelFileName = options.ContextType.ShortDisplayName() + ModelSuffix + FileExtension;
        scaffoldedFiles.Add(new ScaffoldedFile { Path = modelFileName, Code = modelCode });

        var configurationClassNames = new Dictionary<ITypeBase, string>();
        var modelBuilderCode = CreateModelBuilder(
            model, options.ModelNamespace, options.ContextType, configurationClassNames, nullable);
        var modelBuilderFileName = options.ContextType.ShortDisplayName() + ModelBuilderSuffix + FileExtension;
        scaffoldedFiles.Add(new ScaffoldedFile { Path = modelBuilderFileName, Code = modelBuilderCode });

        foreach (var entityType in model.GetEntityTypesInHierarchicalOrder())
        {
            var generatedCode = GenerateEntityType(
                entityType, options.ModelNamespace, configurationClassNames, nullable);

            var entityTypeFileName = configurationClassNames[entityType] + FileExtension;
            scaffoldedFiles.Add(new ScaffoldedFile { Path = entityTypeFileName, Code = generatedCode });
        }

        return scaffoldedFiles;
    }

    private static string GenerateHeader(SortedSet<string> namespaces, string currentNamespace, bool nullable)
    {
        for (var i = 0; i < currentNamespace.Length; i++)
        {
            if (currentNamespace[i] != '.')
            {
                continue;
            }

            namespaces.Remove(currentNamespace[..i]);
        }

        namespaces.Remove(currentNamespace);

        var builder = new StringBuilder();
        builder.AppendLine("// <auto-generated />");
        foreach (var @namespace in namespaces)
        {
            builder
                .Append("using ")
                .Append(@namespace)
                .AppendLine(";");
        }

        builder.AppendLine()
            .AppendLine("#pragma warning disable 219, 612, 618");

        builder.AppendLine(nullable ? "#nullable enable" : "#nullable disable");

        builder.AppendLine();

        return builder.ToString();
    }

    private string CreateAssemblyAttributes(
        string @namespace,
        Type contextType,
        bool nullable)
    {
        var mainBuilder = new IndentedStringBuilder();
        var namespaces = new SortedSet<string>(new NamespaceComparer())
        {
            typeof(DbContextModelAttribute).Namespace!,
            @namespace
        };

        AddNamespace(contextType, namespaces);

        mainBuilder
            .Append("[assembly: DbContextModel(typeof(").Append(_code.Reference(contextType))
            .Append("), typeof(").Append(GetModelClassName(contextType)).AppendLine("))]");

        return GenerateHeader(namespaces, currentNamespace: "", nullable) + mainBuilder;
    }

    private string GetModelClassName(Type contextType) => _code.Identifier(contextType.ShortDisplayName()) + ModelSuffix;

    private string CreateModel(
        string @namespace,
        Type contextType,
        bool nullable)
    {
        var mainBuilder = new IndentedStringBuilder();
        var namespaces = new SortedSet<string>(new NamespaceComparer())
        {
            typeof(RuntimeModel).Namespace!, typeof(DbContextAttribute).Namespace!
        };

        AddNamespace(contextType, namespaces);

        if (!string.IsNullOrEmpty(@namespace))
        {
            mainBuilder
                .Append("namespace ").AppendLine(_code.Namespace(@namespace))
                .AppendLine("{");
            mainBuilder.Indent();
        }

        var className = GetModelClassName(contextType);
        mainBuilder
            .Append("[DbContext(typeof(").Append(_code.Reference(contextType)).AppendLine("))]")
            .Append("public partial class ").Append(className).AppendLine(" : " + nameof(RuntimeModel))
            .AppendLine("{")
            .AppendLine("    private static readonly bool _useOldBehavior31751 =")
            .AppendLine(@"        System.AppContext.TryGetSwitch(""Microsoft.EntityFrameworkCore.Issue31751"", out var enabled31751) && enabled31751;")
            .AppendLine();

        using (mainBuilder.Indent())
        {
            mainBuilder
                .Append("static ").Append(className).Append("()")
                .AppendLines(
                    @"
{
    var model = new "
                    + className
                    + @"();

    if (_useOldBehavior31751)
    {
        model.Initialize();
    }
    else
    {
        var thread = new System.Threading.Thread(RunInitialization, 10 * 1024 * 1024);
        thread.Start();
        thread.Join();

        void RunInitialization()
        {
            model.Initialize();
        }
    }

    model.Customize();")
                .Append("    _instance = (")
                .Append(className)
                .AppendLine(")model.FinalizeModel();")
                .AppendLine("}")
                .AppendLine()
                .Append("private static ").Append(className).AppendLine(" _instance;")
                .AppendLine("public static IModel Instance => _instance;")
                .AppendLine()
                .AppendLine("partial void Initialize();")
                .AppendLine()
                .AppendLine("partial void Customize();");
        }

        mainBuilder.AppendLine("}");

        if (!string.IsNullOrEmpty(@namespace))
        {
            mainBuilder.DecrementIndent();
            mainBuilder.AppendLine("}");
        }

        return GenerateHeader(namespaces, @namespace, nullable) + mainBuilder;
    }

    private string CreateModelBuilder(
        IModel model,
        string @namespace,
        Type contextType,
        Dictionary<ITypeBase, string> configurationClassNames,
        bool nullable)
    {
        var mainBuilder = new IndentedStringBuilder();
        var methodBuilder = new IndentedStringBuilder();
        var namespaces = new SortedSet<string>(new NamespaceComparer())
        {
            typeof(RuntimeModel).Namespace!, typeof(DbContextAttribute).Namespace!
        };

        if (!string.IsNullOrEmpty(@namespace))
        {
            mainBuilder
                .Append("namespace ").AppendLine(_code.Namespace(@namespace))
                .AppendLine("{");
            mainBuilder.Indent();
        }

        var className = GetModelClassName(contextType);
        mainBuilder
            .Append("public partial class ").AppendLine(className)
            .AppendLine("{");

        using (mainBuilder.Indent())
        {
            AddNamespace(typeof(Guid), namespaces);
            mainBuilder
                .AppendLine($"private {className}()")
                .IncrementIndent()
                .Append(": base(skipDetectChanges: ")
                .Append(_code.Literal(((IRuntimeModel)model).SkipDetectChanges))
                .Append(", modelId: ")
                .Append(_code.Literal(model.ModelId))
                .Append(", entityTypeCount: ")
                .Append(_code.Literal(model.GetEntityTypes().Count()));

            var typeConfigurationCount = model.GetTypeMappingConfigurations().Count();
            if (typeConfigurationCount > 0)
            {
                mainBuilder
                    .Append(", typeConfigurationCount: ")
                    .Append(_code.Literal(typeConfigurationCount));
            }

            mainBuilder
                .AppendLine(")")
                .DecrementIndent()
                .AppendLine("{")
                .AppendLine("}")
                .AppendLine();

            mainBuilder
                .AppendLine("partial void Initialize()")
                .AppendLine("{");
            using (mainBuilder.Indent())
            {
                var entityTypes = model.GetEntityTypesInHierarchicalOrder();
                var scopeVariables = new BidirectionalDictionary<object, string>();

                var anyEntityTypes = false;
                foreach (var entityType in entityTypes)
                {
                    anyEntityTypes = true;
                    var variableName = _code.Identifier(entityType.ShortName(), entityType, scopeVariables.Inverse, capitalize: false);

                    var firstChar = variableName[0] == '@' ? variableName[1] : variableName[0];
                    var entityClassName = firstChar == '_'
                        ? EntityTypeSuffix + variableName[1..]
                        : char.ToUpperInvariant(firstChar) + variableName[(variableName[0] == '@' ? 2 : 1)..] + EntityTypeSuffix;

                    configurationClassNames[entityType] = entityClassName;

                    mainBuilder
                        .Append("var ")
                        .Append(variableName)
                        .Append(" = ")
                        .Append(entityClassName)
                        .Append(".Create(this");

                    if (entityType.BaseType != null)
                    {
                        mainBuilder
                            .Append(", ")
                            .Append(scopeVariables[entityType.BaseType]);
                    }

                    mainBuilder
                        .AppendLine(");");
                }

                if (anyEntityTypes)
                {
                    mainBuilder.AppendLine();
                }

                var anyForeignKeys = false;
                foreach (var entityType in entityTypes)
                {
                    var foreignKeyNumber = 1;
                    foreach (var foreignKey in entityType.GetDeclaredForeignKeys())
                    {
                        anyForeignKeys = true;
                        var principalVariable = scopeVariables[foreignKey.PrincipalEntityType];

                        mainBuilder
                            .Append(configurationClassNames[entityType])
                            .Append(".CreateForeignKey")
                            .Append(foreignKeyNumber++.ToString())
                            .Append("(")
                            .Append(scopeVariables[entityType])
                            .Append(", ")
                            .Append(principalVariable)
                            .AppendLine(");");
                    }
                }

                if (anyForeignKeys)
                {
                    mainBuilder.AppendLine();
                }

                var anySkipNavigations = false;
                foreach (var entityType in entityTypes)
                {
                    var navigationNumber = 1;
                    foreach (var navigation in entityType.GetDeclaredSkipNavigations())
                    {
                        anySkipNavigations = true;
                        var targetVariable = scopeVariables[navigation.TargetEntityType];
                        var joinVariable = scopeVariables[navigation.JoinEntityType];

                        mainBuilder
                            .Append(configurationClassNames[entityType])
                            .Append(".CreateSkipNavigation")
                            .Append(navigationNumber++.ToString())
                            .Append("(")
                            .Append(scopeVariables[entityType])
                            .Append(", ")
                            .Append(targetVariable)
                            .Append(", ")
                            .Append(joinVariable)
                            .AppendLine(");");
                    }
                }

                if (anySkipNavigations)
                {
                    mainBuilder.AppendLine();
                }

                foreach (var (entityType, entityClassName) in configurationClassNames)
                {
                    mainBuilder
                        .Append(entityClassName)
                        .Append(".CreateAnnotations")
                        .Append("(")
                        .Append(scopeVariables[entityType])
                        .AppendLine(");");
                }

                if (anyEntityTypes)
                {
                    mainBuilder.AppendLine();
                }

                var parameters = new CSharpRuntimeAnnotationCodeGeneratorParameters(
                    "this",
                    className,
                    @namespace,
                    mainBuilder,
                    methodBuilder,
                    namespaces,
                    scopeVariables.Inverse,
                    scopeVariables,
                    configurationClassNames,
                    nullable);

                foreach (var typeConfiguration in model.GetTypeMappingConfigurations())
                {
                    Create(typeConfiguration, parameters);
                }

                CreateAnnotations(model, _annotationCodeGenerator.Generate, parameters);
            }

            mainBuilder
                .AppendLine("}");

            var methods = methodBuilder.ToString();
            if (!string.IsNullOrEmpty(methods))
            {
                mainBuilder.AppendLines(methods);
            }
        }

        mainBuilder.AppendLine("}");

        if (!string.IsNullOrEmpty(@namespace))
        {
            mainBuilder.DecrementIndent();
            mainBuilder.AppendLine("}");
        }

        return GenerateHeader(namespaces, @namespace, nullable) + mainBuilder;
    }

    private void Create(
        ITypeMappingConfiguration typeConfiguration,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var variableName = _code.Identifier("type", typeConfiguration, parameters.ScopeObjects, capitalize: false);

        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .Append("var ").Append(variableName).Append(" = ").Append(parameters.TargetName).AppendLine(".AddTypeMappingConfiguration(")
            .IncrementIndent()
            .Append(_code.Literal(typeConfiguration.ClrType));

        AddNamespace(typeConfiguration.ClrType, parameters.Namespaces);

        if (typeConfiguration.GetMaxLength() != null)
        {
            mainBuilder.AppendLine(",")
                .Append("maxLength: ")
                .Append(_code.Literal(typeConfiguration.GetMaxLength()));
        }

        if (typeConfiguration.IsUnicode() != null)
        {
            mainBuilder.AppendLine(",")
                .Append("unicode: ")
                .Append(_code.Literal(typeConfiguration.IsUnicode()));
        }

        if (typeConfiguration.GetPrecision() != null)
        {
            mainBuilder.AppendLine(",")
                .Append("precision: ")
                .Append(_code.Literal(typeConfiguration.GetPrecision()));
        }

        if (typeConfiguration.GetScale() != null)
        {
            mainBuilder.AppendLine(",")
                .Append("scale: ")
                .Append(_code.Literal(typeConfiguration.GetScale()));
        }

        var providerClrType = typeConfiguration.GetProviderClrType();
        if (providerClrType != null)
        {
            AddNamespace(providerClrType, parameters.Namespaces);

            mainBuilder.AppendLine(",")
                .Append("providerPropertyType: ")
                .Append(_code.Literal(providerClrType));
        }

        var valueConverterType = (Type?)typeConfiguration[CoreAnnotationNames.ValueConverterType];
        if (valueConverterType != null)
        {
            AddNamespace(valueConverterType, parameters.Namespaces);

            mainBuilder.AppendLine(",")
                .Append("valueConverter: new ")
                .Append(_code.Reference(valueConverterType))
                .Append("()");
        }

        mainBuilder
            .AppendLine(");")
            .DecrementIndent();

        CreateAnnotations(
            typeConfiguration,
            _annotationCodeGenerator.Generate,
            parameters with { TargetName = variableName });

        mainBuilder.AppendLine();
    }

    private string GenerateEntityType(IEntityType entityType, string @namespace, Dictionary<ITypeBase, string> entityClassNames, bool nullable)
    {
        var mainBuilder = new IndentedStringBuilder();
        var methodBuilder = new IndentedStringBuilder();
        var namespaces = new SortedSet<string>(new NamespaceComparer())
        {
            typeof(BindingFlags).Namespace!, typeof(RuntimeEntityType).Namespace!
        };

        if (!string.IsNullOrEmpty(@namespace))
        {
            mainBuilder
                .Append("namespace ").AppendLine(_code.Namespace(@namespace))
                .AppendLine("{");
            mainBuilder.Indent();
        }

        AddNamespace(typeof(EntityFrameworkInternalAttribute), namespaces);
        var className = entityClassNames[entityType];
        mainBuilder
            .AppendLine("[EntityFrameworkInternal]")
            .Append("public partial class ").AppendLine(className)
            .AppendLine("{");
        using (mainBuilder.Indent())
        {
            CreateEntityType(entityType, @namespace, mainBuilder, methodBuilder, namespaces, entityClassNames, nullable);

            foreach (var complexProperty in entityType.GetDeclaredComplexProperties())
            {
                CreateComplexProperty(complexProperty, @namespace, mainBuilder, methodBuilder, namespaces, entityClassNames, className, nullable);
            }

            var foreignKeyNumber = 1;
            foreach (var foreignKey in entityType.GetDeclaredForeignKeys())
            {
                CreateForeignKey(foreignKey, foreignKeyNumber++, @namespace, mainBuilder, methodBuilder, namespaces, entityClassNames, className, nullable);
            }

            var navigationNumber = 1;
            foreach (var navigation in entityType.GetDeclaredSkipNavigations())
            {
                CreateSkipNavigation(navigation, navigationNumber++, @namespace, mainBuilder, methodBuilder, namespaces, entityClassNames, className, nullable);
            }

            CreateAnnotations(entityType, @namespace, mainBuilder, methodBuilder, namespaces, entityClassNames, nullable);

            var methods = methodBuilder.ToString();
            if (!string.IsNullOrEmpty(methods))
            {
                mainBuilder.AppendLines(methods);
            }
        }

        mainBuilder.AppendLine("}");

        if (!string.IsNullOrEmpty(@namespace))
        {
            mainBuilder.DecrementIndent();
            mainBuilder.AppendLine("}");
        }

        return GenerateHeader(namespaces, @namespace, nullable) + mainBuilder;
    }

    private void CreateEntityType(
        IEntityType entityType,
        string @namespace,
        IndentedStringBuilder mainBuilder,
        IndentedStringBuilder methodBuilder,
        SortedSet<string> namespaces,
        Dictionary<ITypeBase, string> configurationClassNames,
        bool nullable)
    {
        mainBuilder
            .Append("public static RuntimeEntityType Create")
            .Append("(RuntimeModel model, RuntimeEntityType");

        if (nullable)
        {
            mainBuilder
                .Append("?");
        }

        mainBuilder.AppendLine(" baseEntityType = null)")
            .AppendLine("{");

        var className = configurationClassNames[entityType];
        using (mainBuilder.Indent())
        {
            const string entityTypeVariable = "runtimeEntityType";
            var scopeVariables = new BidirectionalDictionary<object, string>
            {
                { entityType.Model, "model" },
                { entityType.BaseType ?? new object(), "baseEntityType" },
                { entityType, entityTypeVariable }
            };

            var parameters = new CSharpRuntimeAnnotationCodeGeneratorParameters(
                entityTypeVariable,
                className,
                @namespace,
                mainBuilder,
                methodBuilder,
                namespaces,
                scopeVariables.Inverse,
                scopeVariables,
                configurationClassNames,
                nullable);

            Create(entityType, parameters);

            foreach (var property in entityType.GetDeclaredProperties())
            {
                Create(property, memberAccessReplacements: null, parameters);
            }

            foreach (var property in entityType.GetDeclaredServiceProperties())
            {
                Create(property, memberAccessReplacements: null, parameters);
            }

            foreach (var complexProperty in entityType.GetDeclaredComplexProperties())
            {
                mainBuilder
                    .Append(_code.Identifier(complexProperty.Name, capitalize: true))
                    .Append("ComplexProperty")
                    .Append(".Create")
                    .Append("(")
                    .Append(entityTypeVariable)
                    .AppendLine(");");
            }

            foreach (var key in entityType.GetDeclaredKeys())
            {
                Create(key, parameters, nullable);
            }

            foreach (var index in entityType.GetDeclaredIndexes())
            {
                Create(index, parameters, nullable);
            }

            foreach (var trigger in entityType.GetDeclaredTriggers())
            {
                Create(trigger, parameters);
            }

            mainBuilder
                .Append("return ")
                .Append(entityTypeVariable)
                .AppendLine(";");
        }

        mainBuilder
            .AppendLine("}");
    }

    private void Create(IEntityType entityType, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var runtimeEntityType = entityType as IRuntimeEntityType;
        if ((entityType.ConstructorBinding is not null
                && ((runtimeEntityType?.GetConstructorBindingConfigurationSource()).OverridesStrictly(ConfigurationSource.Convention)
                    || entityType.ConstructorBinding is FactoryMethodBinding))
            || (runtimeEntityType?.ServiceOnlyConstructorBinding is not null
                && (runtimeEntityType.GetServiceOnlyConstructorBindingConfigurationSource()
                        .OverridesStrictly(ConfigurationSource.Convention)
                    || runtimeEntityType.ServiceOnlyConstructorBinding is FactoryMethodBinding)))
        {
            throw new InvalidOperationException(
                DesignStrings.CompiledModelConstructorBinding(
                    entityType.ShortName(), "Customize()", parameters.ClassName));
        }

        if (entityType.GetQueryFilter() != null)
        {
            throw new InvalidOperationException(DesignStrings.CompiledModelQueryFilter(entityType.ShortName()));
        }

#pragma warning disable CS0618 // Type or member is obsolete
        if (entityType.GetDefiningQuery() != null)
        {
            // TODO: Move to InMemoryCSharpRuntimeAnnotationCodeGenerator, see #21624
            throw new InvalidOperationException(DesignStrings.CompiledModelDefiningQuery(entityType.ShortName()));
        }
#pragma warning restore CS0618 // Type or member is obsolete

        AddNamespace(entityType.ClrType, parameters.Namespaces);

        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .Append("var ")
            .Append(parameters.TargetName)
            .AppendLine(" = model.AddEntityType(")
            .IncrementIndent()
            .Append(_code.Literal(entityType.Name)).AppendLine(",")
            .Append(_code.Literal(entityType.ClrType)).AppendLine(",")
            .Append("baseEntityType");

        if (entityType.HasSharedClrType)
        {
            mainBuilder.AppendLine(",")
                .Append("sharedClrType: ")
                .Append(_code.Literal(entityType.HasSharedClrType));
        }

        var discriminatorProperty = entityType.GetDiscriminatorPropertyName();
        if (discriminatorProperty != null)
        {
            mainBuilder.AppendLine(",")
                .Append("discriminatorProperty: ")
                .Append(_code.Literal(discriminatorProperty));
        }

        var changeTrackingStrategy = entityType.GetChangeTrackingStrategy();
        if (changeTrackingStrategy != ChangeTrackingStrategy.Snapshot)
        {
            parameters.Namespaces.Add(typeof(ChangeTrackingStrategy).Namespace!);

            mainBuilder.AppendLine(",")
                .Append("changeTrackingStrategy: ")
                .Append(_code.Literal(changeTrackingStrategy));
        }

        var indexerPropertyInfo = entityType.FindIndexerPropertyInfo();
        if (indexerPropertyInfo != null)
        {
            mainBuilder.AppendLine(",")
                .Append("indexerPropertyInfo: RuntimeEntityType.FindIndexerProperty(")
                .Append(_code.Literal(entityType.ClrType))
                .Append(')');
        }

        if (entityType.IsPropertyBag)
        {
            mainBuilder.AppendLine(",")
                .Append("propertyBag: ")
                .Append(_code.Literal(true));
        }

        var discriminatorValue = entityType.GetDiscriminatorValue();
        if (discriminatorValue != null)
        {
            AddNamespace(discriminatorValue.GetType(), parameters.Namespaces);

            mainBuilder.AppendLine(",")
                .Append("discriminatorValue: ")
                .Append(_code.UnknownLiteral(discriminatorValue));
        }

        var derivedTypesCount = entityType.GetDirectlyDerivedTypes().Count();
        if (derivedTypesCount != 0)
        {
            mainBuilder.AppendLine(",")
                .Append("derivedTypesCount: ")
                .Append(_code.Literal(derivedTypesCount));
        }

        mainBuilder.AppendLine(",")
            .Append("propertyCount: ")
            .Append(_code.Literal(entityType.GetDeclaredProperties().Count()));

        var complexPropertyCount = entityType.GetDeclaredComplexProperties().Count();
        if (complexPropertyCount != 0)
        {
            mainBuilder.AppendLine(",")
                .Append("complexPropertyCount: ")
                .Append(_code.Literal(complexPropertyCount));
        }

        var navigationCount = entityType.GetDeclaredNavigations().Count();
        if (navigationCount != 0)
        {
            mainBuilder.AppendLine(",")
                .Append("navigationCount: ")
                .Append(_code.Literal(navigationCount));
        }

        var skipNavigationCount = entityType.GetDeclaredSkipNavigations().Count();
        if (skipNavigationCount != 0)
        {
            mainBuilder.AppendLine(",")
                .Append("skipNavigationCount: ")
                .Append(_code.Literal(skipNavigationCount));
        }

        var servicePropertyCount = entityType.GetDeclaredServiceProperties().Count();
        if (servicePropertyCount != 0)
        {
            mainBuilder.AppendLine(",")
                .Append("servicePropertyCount: ")
                .Append(_code.Literal(servicePropertyCount));
        }

        var foreignKeyCount = entityType.GetDeclaredForeignKeys().Count();
        if (foreignKeyCount != 0)
        {
            mainBuilder.AppendLine(",")
                .Append("foreignKeyCount: ")
                .Append(_code.Literal(foreignKeyCount));
        }

        var unnamedIndexCount = entityType.GetDeclaredIndexes().Count(i => i.Name == null);
        if (unnamedIndexCount != 0)
        {
            mainBuilder.AppendLine(",")
                .Append("unnamedIndexCount: ")
                .Append(_code.Literal(unnamedIndexCount));
        }

        var namedIndexCount = entityType.GetDeclaredIndexes().Count(i => i.Name != null);
        if (namedIndexCount != 0)
        {
            mainBuilder.AppendLine(",")
                .Append("namedIndexCount: ")
                .Append(_code.Literal(namedIndexCount));
        }

        var keyCount = entityType.GetDeclaredKeys().Count();
        if (keyCount != 0)
        {
            mainBuilder.AppendLine(",")
                .Append("keyCount: ")
                .Append(_code.Literal(keyCount));
        }

        var triggerCount = entityType.GetDeclaredTriggers().Count();
        if (triggerCount != 0)
        {
            mainBuilder.AppendLine(",")
                .Append("triggerCount: ")
                .Append(_code.Literal(triggerCount));
        }

        mainBuilder
            .AppendLine(");")
            .AppendLine()
            .DecrementIndent();
    }

    private void Create(
        IProperty property,
        Dictionary<MemberInfo, QualifiedName>? memberAccessReplacements,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var variableName = _code.Identifier(property.Name, property, parameters.ScopeObjects, capitalize: false);

        var valueGeneratorFactoryType = (Type?)property[CoreAnnotationNames.ValueGeneratorFactoryType];
        if (valueGeneratorFactoryType == null
            && property.GetValueGeneratorFactory() != null)
        {
            throw new InvalidOperationException(
                DesignStrings.CompiledModelValueGenerator(
                    property.DeclaringType.ShortName(), property.Name, nameof(PropertyBuilder.HasValueGeneratorFactory)));
        }

        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .Append("var ").Append(variableName).Append(" = ").Append(parameters.TargetName).AppendLine(".AddProperty(")
            .IncrementIndent()
            .Append(_code.Literal(property.Name));

        GeneratePropertyBaseParameters(property, parameters);

        if (property.IsNullable)
        {
            mainBuilder.AppendLine(",")
                .Append("nullable: ")
                .Append(_code.Literal(true));
        }

        if (property.IsConcurrencyToken)
        {
            mainBuilder.AppendLine(",")
                .Append("concurrencyToken: ")
                .Append(_code.Literal(true));
        }

        if (property.ValueGenerated != ValueGenerated.Never)
        {
            mainBuilder.AppendLine(",")
                .Append("valueGenerated: ")
                .Append(_code.Literal(property.ValueGenerated));
        }

        if (property.GetBeforeSaveBehavior() != PropertySaveBehavior.Save)
        {
            mainBuilder.AppendLine(",")
                .Append("beforeSaveBehavior: ")
                .Append(_code.Literal(property.GetBeforeSaveBehavior()));
        }

        if (property.GetAfterSaveBehavior() != PropertySaveBehavior.Save)
        {
            mainBuilder.AppendLine(",")
                .Append("afterSaveBehavior: ")
                .Append(_code.Literal(property.GetAfterSaveBehavior()));
        }

        if (property.GetMaxLength() != null)
        {
            mainBuilder.AppendLine(",")
                .Append("maxLength: ")
                .Append(_code.Literal(property.GetMaxLength()));
        }

        if (property.IsUnicode() != null)
        {
            mainBuilder.AppendLine(",")
                .Append("unicode: ")
                .Append(_code.Literal(property.IsUnicode()));
        }

        if (property.GetPrecision() != null)
        {
            mainBuilder.AppendLine(",")
                .Append("precision: ")
                .Append(_code.Literal(property.GetPrecision()));
        }

        if (property.GetScale() != null)
        {
            mainBuilder.AppendLine(",")
                .Append("scale: ")
                .Append(_code.Literal(property.GetScale()));
        }

        var providerClrType = property.GetProviderClrType();
        if (providerClrType != null)
        {
            AddNamespace(providerClrType, parameters.Namespaces);

            mainBuilder.AppendLine(",")
                .Append("providerPropertyType: ")
                .Append(_code.Literal(providerClrType));
        }

        if (valueGeneratorFactoryType != null)
        {
            AddNamespace(valueGeneratorFactoryType, parameters.Namespaces);

            mainBuilder.AppendLine(",")
                .Append("valueGeneratorFactory: new ")
                .Append(_code.Reference(valueGeneratorFactoryType))
                .Append("().Create");
        }

        var valueConverterType = GetValueConverterType(property);
        if (valueConverterType != null)
        {
            AddNamespace(valueConverterType, parameters.Namespaces);

            mainBuilder.AppendLine(",")
                .Append("valueConverter: new ")
                .Append(_code.Reference(valueConverterType))
                .Append("()");
        }

        var valueComparerType = (Type?)property[CoreAnnotationNames.ValueComparerType];
        if (valueComparerType != null)
        {
            AddNamespace(valueComparerType, parameters.Namespaces);

            mainBuilder.AppendLine(",")
                .Append("valueComparer: new ")
                .Append(_code.Reference(valueComparerType))
                .Append("()");
        }

        var providerValueComparerType = (Type?)property[CoreAnnotationNames.ProviderValueComparerType];
        if (providerValueComparerType != null)
        {
            AddNamespace(providerValueComparerType, parameters.Namespaces);

            mainBuilder.AppendLine(",")
                .Append("providerValueComparer: new ")
                .Append(_code.Reference(providerValueComparerType))
                .Append("()");
        }

        var sentinel = property.Sentinel;
        var converter = property.FindTypeMapping()?.Converter;
        if (sentinel != null
            && converter == null)
        {
            mainBuilder.AppendLine(",")
                .Append("sentinel: ")
                .Append(_code.UnknownLiteral(sentinel));
        }

        var jsonValueReaderWriterType = (Type?)property[CoreAnnotationNames.JsonValueReaderWriterType];
        if (jsonValueReaderWriterType != null)
        {
            mainBuilder.AppendLine(",")
                .Append("jsonValueReaderWriter: ");

            CSharpRuntimeAnnotationCodeGenerator.CreateJsonValueReaderWriter(jsonValueReaderWriterType, parameters, _code);
        }

        mainBuilder
            .AppendLine(");")
            .DecrementIndent();

        var propertyParameters = parameters with { TargetName = variableName };

        SetPropertyBaseProperties(property, memberAccessReplacements, propertyParameters);

        mainBuilder.Append(variableName).Append(".TypeMapping = ");
        _annotationCodeGenerator.Create(property.GetTypeMapping(), property, propertyParameters);
        mainBuilder.AppendLine(";");

        if (property.IsKey()
            || property.IsForeignKey()
            || property.IsUniqueIndex())
        {
            var currentComparerType = CurrentValueComparerFactory.Instance.GetComparerType(property);
            AddNamespace(currentComparerType, parameters.Namespaces);

            mainBuilder
                .Append(variableName).Append(".SetCurrentValueComparer(new ")
                .Append(_code.Reference(currentComparerType))
                .AppendLine($"({variableName}));");
        }

        if (sentinel != null
            && converter != null)
        {
            mainBuilder.Append(variableName).Append(".SetSentinelFromProviderValue(")
                .Append(_code.UnknownLiteral(converter?.ConvertToProvider(sentinel) ?? sentinel))
                .AppendLine(");");
        }

        CreateAnnotations(
            property,
            _annotationCodeGenerator.Generate,
            parameters with { TargetName = variableName });

        parameters.MainBuilder.AppendLine();
    }

    private void
        SetPropertyBaseProperties(
        IPropertyBase property,
        Dictionary<MemberInfo, QualifiedName>? memberAccessReplacements,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var variableName = parameters.TargetName;
        var mainBuilder = parameters.MainBuilder;
        if (!property.IsShadowProperty()
            && property is not IServiceProperty) // Service properties don't use property accessors
        {
            memberAccessReplacements = CreatePrivateAccessors(property, memberAccessReplacements, parameters);

            ClrPropertyGetterFactory.Instance.Create(
                property,
                out var getterExpression,
                out var hasSentinelExpression,
                out var structuralGetterExpression,
                out var hasStructuralSentinelExpression);

            mainBuilder
                .Append(variableName).AppendLine(".SetGetter(")
                .IncrementIndent()
                .AppendLines(_code.Expression(getterExpression, parameters.Namespaces, (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(",")
                .AppendLines(_code.Expression(hasSentinelExpression, parameters.Namespaces, (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(",")
                .AppendLines(_code.Expression(structuralGetterExpression, parameters.Namespaces, (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(",")
                .AppendLines(_code.Expression(hasStructuralSentinelExpression, parameters.Namespaces, (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(");")
                .DecrementIndent();

            ClrPropertySetterFactory.Instance.Create(property, out var setterExpression);

            mainBuilder
                .Append(variableName).AppendLine(".SetSetter(")
                .IncrementIndent()
                .AppendLines(_code.Expression(setterExpression, parameters.Namespaces, (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(");")
                .DecrementIndent();

            ClrPropertyMaterializationSetterFactory.Instance.Create(property, out var materializationSetterExpression);

            mainBuilder
                .Append(variableName).AppendLine(".SetMaterializationSetter(")
                .IncrementIndent()
                .AppendLines(_code.Expression(materializationSetterExpression, parameters.Namespaces, (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(");")
                .DecrementIndent();

            PropertyAccessorsFactory.Instance.Create(property,
                out var currentValueGetter,
                out var preStoreGeneratedCurrentValueGetter,
                out var originalValueGetter,
                out var relationshipSnapshotGetter,
                out var valueBufferGetter);

            mainBuilder
                .Append(variableName).AppendLine(".SetAccessors(")
                .IncrementIndent()
                .AppendLines(_code.Expression(currentValueGetter, parameters.Namespaces, (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(",")
                .AppendLines(_code.Expression(preStoreGeneratedCurrentValueGetter, parameters.Namespaces, (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(",")
                .AppendLines(originalValueGetter == null
                    ? "null"
                    : _code.Expression(originalValueGetter, parameters.Namespaces, (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(",")
                .AppendLines(_code.Expression(relationshipSnapshotGetter, parameters.Namespaces, (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(",")
                .AppendLines(valueBufferGetter == null
                    ? "null"
                    : _code.Expression(valueBufferGetter, parameters.Namespaces, (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(");")
                .DecrementIndent();
        }

        var propertyIndexes = ((IRuntimePropertyBase)property).PropertyIndexes;
        mainBuilder
            .Append(variableName).AppendLine(".SetPropertyIndexes(")
            .IncrementIndent()
            .Append("index: ").Append(_code.Literal(propertyIndexes.Index)).AppendLine(",")
            .Append("originalValueIndex: ").Append(_code.Literal(propertyIndexes.OriginalValueIndex)).AppendLine(",")
            .Append("shadowIndex: ").Append(_code.Literal(propertyIndexes.ShadowIndex)).AppendLine(",")
            .Append("relationshipIndex: ").Append(_code.Literal(propertyIndexes.RelationshipIndex)).AppendLine(",")
            .Append("storeGenerationIndex: ").Append(_code.Literal(propertyIndexes.StoreGenerationIndex)).AppendLine(");")
            .DecrementIndent();
    }

    private Dictionary<MemberInfo, QualifiedName>? CreatePrivateAccessors(
        IPropertyBase property,
        Dictionary<MemberInfo, QualifiedName>? memberAccessReplacements,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
        bool create = true,
        bool qualify = false)
    {
        if (property.IsShadowProperty()
            || property.IsIndexerProperty())
        {
            return memberAccessReplacements;
        }

        var getter = CreatePrivateAccessor(property, forMaterialization: false, forSet: false, create, qualify, ref memberAccessReplacements, parameters);
        var setter = CreatePrivateAccessor(property, forMaterialization: false, forSet: true, create, qualify, ref memberAccessReplacements, parameters);
        var queryGetter = CreatePrivateAccessor(property, forMaterialization: true, forSet: false, create, qualify, ref memberAccessReplacements, parameters);
        var querySetter = CreatePrivateAccessor(property, forMaterialization: true, forSet: true, create, qualify, ref memberAccessReplacements, parameters);

        if (getter != null
            || setter != null
            || queryGetter != null
            || querySetter != null)
        {
            var accessors = new (string?, string?)[]
            {
                (getter?.Name, getter?.Namespace),
                (setter?.Name, setter?.Namespace),
                (queryGetter?.Name, queryGetter?.Namespace),
                (querySetter?.Name, querySetter?.Namespace)
            };

            var i = accessors.Length;
            for (; i > 1; i--)
            {
                if (accessors[i - 1].Item1 != null)
                {
                    break;
                }
            }

            property.AddRuntimeAnnotation(CoreAnnotationNames.UnsafeAccessors, accessors.Take(i).ToArray());
        }

        return memberAccessReplacements;
    }

    private QualifiedName? CreatePrivateAccessor(
        IPropertyBase property,
        bool forMaterialization,
        bool forSet,
        bool create,
        bool qualify,
        ref Dictionary<MemberInfo, QualifiedName>? memberAccessReplacements,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var member = property.GetMemberInfo(forMaterialization, forSet);

        switch (member)
        {
            case FieldInfo field:
            {
                if (field.IsPublic
                    || (memberAccessReplacements?.ContainsKey(field)) == true)
                {
                    return null;
                }

                memberAccessReplacements ??= [];
                var methodName = LinqToCSharpSyntaxTranslator.GetUnsafeAccessorName(field);

                var qualifiedName = new QualifiedName(
                    parameters.ConfigurationClassNames[property.DeclaringType] + "." + methodName,
                    parameters.Namespace);

                memberAccessReplacements.Add(field, qualify
                    ? qualifiedName
                    : new QualifiedName(methodName, ""));

                if (create)
                {
                    AddNamespace(typeof(UnsafeAccessorAttribute), parameters.Namespaces);
                    AddNamespace(field.FieldType, parameters.Namespaces);
                    AddNamespace(property.DeclaringType.ClrType, parameters.Namespaces);

                    parameters.MethodBuilder
                        .AppendLine()
                        .AppendLine($"[UnsafeAccessor(UnsafeAccessorKind.Field, Name = \"{field.Name}\")]")
                        .Append($"public static extern ref {_code.Reference(field.FieldType)} {methodName}(")
                        .AppendLine($"{_code.Reference(property.DeclaringType.ClrType)} @this);");

                    return qualifiedName;
                }

                break;
            }
            case PropertyInfo propertyInfo:
            {
                var methodInfo = forSet ? propertyInfo.SetMethod! : propertyInfo.GetMethod!;
                if (methodInfo.IsPublic
                    || methodInfo.IsStatic
                    || (memberAccessReplacements?.ContainsKey(methodInfo)) == true)
                {
                    return null;
                }

                memberAccessReplacements ??= [];
                var methodName = LinqToCSharpSyntaxTranslator.GetUnsafeAccessorName(methodInfo);

                var qualifiedName = new QualifiedName(
                    parameters.ConfigurationClassNames[property.DeclaringType] + "." + methodName,
                    parameters.Namespace);

                memberAccessReplacements.Add(methodInfo, qualify
                    ? qualifiedName
                    : new QualifiedName(methodName, ""));

                if (create)
                {
                    AddNamespace(typeof(UnsafeAccessorAttribute), parameters.Namespaces);
                    AddNamespace(methodInfo.ReturnType, parameters.Namespaces);
                    AddNamespace(property.DeclaringType.ClrType, parameters.Namespaces);
                    foreach (var parameter in methodInfo.GetParameters())
                    {
                        AddNamespace(parameter.ParameterType, parameters.Namespaces);
                    }

                    var returnType = methodInfo.ReturnType == typeof(void)
                        ? "void"
                        : _code.Reference(methodInfo.ReturnType);

                    parameters.MethodBuilder
                        .AppendLine()
                        .AppendLine($"[UnsafeAccessor(UnsafeAccessorKind.Method, Name = \"{methodInfo.Name}\")]")
                        .Append($"public static extern {returnType} {methodName}(")
                        .Append($"{_code.Reference(property.DeclaringType.ClrType)} @this");

                    if (methodInfo.GetParameters().Length > 0)
                    {
                        parameters.MethodBuilder
                            .Append($", ")
                            .AppendJoin(methodInfo.GetParameters().Select(p => _code.Reference(p.ParameterType) + " " + _code.Identifier(p.Name!)), ", ");
                    }

                    parameters.MethodBuilder.AppendLine(");");

                    return qualifiedName;
                }

                break;
            }
        }

        return null;
    }

    private static Type? GetValueConverterType(IProperty property)
    {
        var annotation = property.FindAnnotation(CoreAnnotationNames.ValueConverterType);
        return annotation != null
            ? (Type?)annotation.Value
            : ((Property)property).GetConversion(throwOnProviderClrTypeConflict: false, throwOnValueConverterConflict: false)
                .ValueConverterType;
    }

    private void GeneratePropertyBaseParameters(
        IPropertyBase property,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
        bool skipType = false)
    {
        var mainBuilder = parameters.MainBuilder;

        if (!skipType)
        {
            AddNamespace(property.ClrType, parameters.Namespaces);
            mainBuilder.AppendLine(",")
                .Append(_code.Literal(property.ClrType));
        }

        var propertyInfo = property.PropertyInfo;
        if (propertyInfo != null)
        {
            AddNamespace(propertyInfo.DeclaringType!, parameters.Namespaces);

            mainBuilder.AppendLine(",")
                .Append("propertyInfo: ");

            if (property.IsIndexerProperty())
            {
                mainBuilder
                    .Append(parameters.TargetName)
                    .Append(".FindIndexerPropertyInfo()");
            }
            else
            {
                mainBuilder
                    .Append(_code.Literal(propertyInfo.DeclaringType!))
                    .Append(".GetProperty(")
                    .Append(_code.Literal(propertyInfo.Name))
                    .Append(", ")
                    .Append(propertyInfo.GetAccessors().Length != 0 ? "BindingFlags.Public" : "BindingFlags.NonPublic")
                    .Append(propertyInfo.IsStatic() ? " | BindingFlags.Static" : " | BindingFlags.Instance")
                    .Append(" | BindingFlags.DeclaredOnly)");
            }
        }

        var fieldInfo = property.FieldInfo;
        if (fieldInfo != null)
        {
            AddNamespace(fieldInfo.DeclaringType!, parameters.Namespaces);

            mainBuilder.AppendLine(",")
                .Append("fieldInfo: ")
                .Append(_code.Literal(fieldInfo.DeclaringType!))
                .Append(".GetField(")
                .Append(_code.Literal(fieldInfo.Name))
                .Append(", ")
                .Append(fieldInfo.IsPublic ? "BindingFlags.Public" : "BindingFlags.NonPublic")
                .Append(fieldInfo.IsStatic ? " | BindingFlags.Static" : " | BindingFlags.Instance")
                .Append(" | BindingFlags.DeclaredOnly)");
        }

        var propertyAccessMode = property.GetPropertyAccessMode();
        if (propertyAccessMode != Model.DefaultPropertyAccessMode)
        {
            parameters.Namespaces.Add(typeof(PropertyAccessMode).Namespace!);

            mainBuilder.AppendLine(",")
                .Append("propertyAccessMode: ")
                .Append(_code.Literal(propertyAccessMode));
        }
    }

    private void FindProperties(
        string entityTypeVariable,
        IEnumerable<IProperty> properties,
        IndentedStringBuilder mainBuilder,
        bool nullable,
        IDictionary<object, string>? scopeVariables = null)
    {
        mainBuilder.Append("new[] { ");
        var first = true;
        foreach (var property in properties)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                mainBuilder.Append(", ");
            }

            if (scopeVariables != null
                && scopeVariables.TryGetValue(property, out var propertyVariable))
            {
                mainBuilder.Append(propertyVariable);
            }
            else
            {
                mainBuilder
                    .Append(entityTypeVariable)
                    .Append(".FindProperty(")
                    .Append(_code.Literal(property.Name))
                    .Append(')');

                if (nullable)
                {
                    mainBuilder
                        .Append('!');
                }
            }
        }

        mainBuilder.Append(" }");
    }

    private void Create(
        IServiceProperty property,
        Dictionary<MemberInfo, QualifiedName>? memberAccessReplacements,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var variableName = _code.Identifier(property.Name, property, parameters.ScopeObjects, capitalize: false);

        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .Append("var ").Append(variableName).Append(" = ").Append(parameters.TargetName).AppendLine(".AddServiceProperty(")
            .IncrementIndent()
            .Append(_code.Literal(property.Name));

        GeneratePropertyBaseParameters(property, parameters, skipType: true);

        AddNamespace(property.ClrType, parameters.Namespaces);
        mainBuilder
            .AppendLine(",")
            .Append("serviceType: typeof(" + _code.Reference(property.ClrType) + ")");

        mainBuilder
            .AppendLine(");")
            .DecrementIndent();

        var propertyParameters = parameters with { TargetName = variableName };

        SetPropertyBaseProperties(property, memberAccessReplacements, propertyParameters);

        CreateAnnotations(property, _annotationCodeGenerator.Generate, propertyParameters);

        mainBuilder.AppendLine();
    }

    private void Create(
        IKey key,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
        bool nullable)
    {
        var variableName = _code.Identifier("key", key, parameters.ScopeObjects);

        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .Append("var ").Append(variableName).Append(" = ").Append(parameters.TargetName).AppendLine(".AddKey(")
            .IncrementIndent();
        FindProperties(parameters.TargetName, key.Properties, mainBuilder, nullable, parameters.ScopeVariables);
        mainBuilder
            .AppendLine(");")
            .DecrementIndent();

        if (key.IsPrimaryKey())
        {
            mainBuilder
                .Append(parameters.TargetName)
                .Append(".SetPrimaryKey(")
                .Append(variableName)
                .AppendLine(");");
        }

        CreateAnnotations(
            key,
            _annotationCodeGenerator.Generate,
            parameters with { TargetName = variableName });

        mainBuilder.AppendLine();
    }

    private void Create(
        IIndex index,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
        bool nullable)
    {
        var variableName = _code.Identifier(index.Name ?? "index", index, parameters.ScopeObjects, capitalize: false);

        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .Append("var ").Append(variableName).Append(" = ").Append(parameters.TargetName).AppendLine(".AddIndex(")
            .IncrementIndent();

        FindProperties(parameters.TargetName, index.Properties, mainBuilder, nullable, parameters.ScopeVariables);

        if (index.Name != null)
        {
            mainBuilder.AppendLine(",")
                .Append("name: ")
                .Append(_code.Literal(index.Name));
        }

        if (index.IsUnique)
        {
            mainBuilder.AppendLine(",")
                .Append("unique: ")
                .Append(_code.Literal(true));
        }

        mainBuilder
            .AppendLine(");")
            .DecrementIndent();

        CreateAnnotations(
            index,
            _annotationCodeGenerator.Generate,
            parameters with { TargetName = variableName });

        mainBuilder.AppendLine();
    }

    private void CreateComplexProperty(
        IComplexProperty complexProperty,
        string @namespace,
        IndentedStringBuilder mainBuilder,
        IndentedStringBuilder methodBuilder,
        SortedSet<string> namespaces,
        Dictionary<ITypeBase, string> configurationClassNames,
        string topClassName,
        bool nullable)
    {
        var className = _code.Identifier(complexProperty.Name, capitalize: true);
        mainBuilder
            .AppendLine()
            .Append("public static class ")
            .Append(className)
            .AppendLine("ComplexProperty")
            .AppendLine("{");

        methodBuilder = new IndentedStringBuilder();
        var complexType = complexProperty.ComplexType;
        configurationClassNames[complexType] = configurationClassNames[complexProperty.DeclaringType] + "." + className;
        using (mainBuilder.Indent())
        {
            var declaringTypeVariable = "declaringType";
            mainBuilder
                .Append("public static RuntimeComplexProperty Create(")
                .Append(complexProperty.DeclaringType is IEntityType ? "RuntimeEntityType " : "RuntimeComplexType ")
                .Append(declaringTypeVariable)
                .AppendLine(")")
                .AppendLine("{");

            using (mainBuilder.Indent())
            {
                const string complexPropertyVariable = "complexProperty";
                const string complexTypeVariable = "complexType";

                var scopeVariables = new BidirectionalDictionary<object, string>
                {
                    { complexProperty.DeclaringType, declaringTypeVariable },
                    { complexProperty, complexPropertyVariable },
                    { complexType, complexTypeVariable }
                };

                mainBuilder
                    .Append("var ").Append(complexPropertyVariable).Append(" = ")
                    .Append(declaringTypeVariable).Append(".AddComplexProperty(")
                    .IncrementIndent()
                    .Append(_code.Literal(complexProperty.Name))
                    .AppendLine(",")
                    .Append(_code.Literal(complexProperty.ClrType))
                    .AppendLine(",")
                    .Append(_code.Literal(complexType.Name))
                    .AppendLine(",")
                    .Append(_code.Literal(complexType.ClrType));

                AddNamespace(complexProperty.ClrType, namespaces);
                AddNamespace(complexType.ClrType, namespaces);

                var parameters = new CSharpRuntimeAnnotationCodeGeneratorParameters(
                    declaringTypeVariable,
                    topClassName,
                    @namespace,
                    mainBuilder,
                    methodBuilder,
                    namespaces,
                    scopeVariables.Inverse,
                    scopeVariables,
                    configurationClassNames,
                    nullable);

                GeneratePropertyBaseParameters(complexProperty, parameters, skipType: true);

                if (complexProperty.IsNullable)
                {
                    mainBuilder.AppendLine(",")
                        .Append("nullable: ")
                        .Append(_code.Literal(true));
                }

                if (complexProperty.IsCollection)
                {
                    mainBuilder.AppendLine(",")
                        .Append("collection: ")
                        .Append(_code.Literal(true));
                }

                var changeTrackingStrategy = complexType.GetChangeTrackingStrategy();
                if (changeTrackingStrategy != ChangeTrackingStrategy.Snapshot)
                {
                    namespaces.Add(typeof(ChangeTrackingStrategy).Namespace!);

                    mainBuilder.AppendLine(",")
                        .Append("changeTrackingStrategy: ")
                        .Append(_code.Literal(changeTrackingStrategy));
                }

                var indexerPropertyInfo = complexType.FindIndexerPropertyInfo();
                if (indexerPropertyInfo != null)
                {
                    mainBuilder.AppendLine(",")
                        .Append("indexerPropertyInfo: RuntimeEntityType.FindIndexerProperty(")
                        .Append(_code.Literal(complexType.ClrType))
                        .Append(")");
                }

                if (complexType.IsPropertyBag)
                {
                    mainBuilder.AppendLine(",")
                        .Append("propertyBag: ")
                        .Append(_code.Literal(true));
                }

                mainBuilder.AppendLine(",")
                    .Append("propertyCount: ")
                    .Append(_code.Literal(complexType.GetDeclaredProperties().Count()));

                var complexPropertyCount = complexType.GetDeclaredComplexProperties().Count();
                if (complexPropertyCount != 0)
                {
                    mainBuilder.AppendLine(",")
                        .Append("complexPropertyCount: ")
                        .Append(_code.Literal(complexPropertyCount));
                }

                mainBuilder
                    .AppendLine(");")
                    .AppendLine()
                    .DecrementIndent();

                mainBuilder
                    .Append("var ").Append(complexTypeVariable).Append(" = ")
                    .Append(complexPropertyVariable).AppendLine(".ComplexType;");

                var complexTypeParameters = parameters with { TargetName = complexTypeVariable };
                var complexPropertyParameters = parameters with { TargetName = complexPropertyVariable };

                Dictionary<MemberInfo, QualifiedName>? memberAccessReplacements = null;

                foreach (var chainedComplexProperty in complexProperty.GetChainToComplexProperty())
                {
                    memberAccessReplacements = CreatePrivateAccessors(chainedComplexProperty, memberAccessReplacements, complexTypeParameters, create: chainedComplexProperty == complexProperty);
                }

                SetPropertyBaseProperties(complexProperty, memberAccessReplacements, complexPropertyParameters);

                foreach (var property in complexType.GetProperties())
                {
                    Create(property, memberAccessReplacements, complexTypeParameters);
                }

                foreach (var nestedComplexProperty in complexType.GetComplexProperties())
                {
                    mainBuilder
                        .Append(_code.Identifier(nestedComplexProperty.Name, capitalize: true))
                        .Append("ComplexProperty")
                        .Append(".Create")
                        .Append("(")
                        .Append(complexTypeVariable)
                        .AppendLine(");");
                }

                CreateAnnotations(complexType, _annotationCodeGenerator.Generate, complexTypeParameters);
                CreateAnnotations(complexProperty, _annotationCodeGenerator.Generate, complexPropertyParameters);

                mainBuilder
                    .Append("return ")
                    .Append(complexPropertyVariable)
                    .AppendLine(";");
            }

            mainBuilder.AppendLine("}");
        }

        using (mainBuilder.Indent())
        {
            foreach (var nestedComplexProperty in complexType.GetComplexProperties())
            {
                CreateComplexProperty(
                    nestedComplexProperty,
                    @namespace,
                    mainBuilder,
                    methodBuilder,
                    namespaces,
                    configurationClassNames,
                    topClassName,
                    nullable);
            }
        }

        var methods = methodBuilder.ToString();
        if (!string.IsNullOrEmpty(methods))
        {
            mainBuilder.AppendLines(methods);
        }

        mainBuilder.AppendLine("}");
    }

    private void CreateForeignKey(
        IForeignKey foreignKey,
        int foreignKeyNumber,
        string @namespace,
        IndentedStringBuilder mainBuilder,
        IndentedStringBuilder methodBuilder,
        SortedSet<string> namespaces,
        Dictionary<ITypeBase, string> configurationClassNames,
        string className,
        bool nullable)
    {
        const string declaringEntityType = "declaringEntityType";
        const string principalEntityType = "principalEntityType";
        mainBuilder.AppendLine()
            .Append("public static RuntimeForeignKey CreateForeignKey").Append(foreignKeyNumber.ToString())
            .Append("(RuntimeEntityType ").Append(declaringEntityType)
            .Append(", RuntimeEntityType ").Append(principalEntityType).AppendLine(")")
            .AppendLine("{");

        using (mainBuilder.Indent())
        {
            const string foreignKeyVariable = "runtimeForeignKey";
            var scopeVariables = new BidirectionalDictionary<object, string>
            {
                { foreignKey.DeclaringEntityType, declaringEntityType },
                { foreignKey.DeclaringEntityType != foreignKey.PrincipalEntityType
                    ? foreignKey.PrincipalEntityType : new object(), principalEntityType },
                { foreignKey, foreignKeyVariable }
            };

            mainBuilder
                .Append("var ").Append(foreignKeyVariable).Append(" = ")
                .Append(declaringEntityType).Append(".AddForeignKey(").IncrementIndent();
            FindProperties(declaringEntityType, foreignKey.Properties, mainBuilder, nullable);

            mainBuilder.AppendLine(",")
                .Append(principalEntityType).Append(".FindKey(");
            FindProperties(principalEntityType, foreignKey.PrincipalKey.Properties, mainBuilder, nullable);
            mainBuilder.Append(")");
            if (nullable)
            {
                mainBuilder.Append("!");
            }

            mainBuilder.AppendLine(",")
                .Append(principalEntityType);

            if (foreignKey.DeleteBehavior != ForeignKey.DefaultDeleteBehavior)
            {
                namespaces.Add(typeof(DeleteBehavior).Namespace!);

                mainBuilder.AppendLine(",")
                    .Append("deleteBehavior: ")
                    .Append(_code.Literal(foreignKey.DeleteBehavior));
            }

            if (foreignKey.IsUnique)
            {
                mainBuilder.AppendLine(",")
                    .Append("unique: ")
                    .Append(_code.Literal(true));
            }

            if (foreignKey.IsRequired)
            {
                mainBuilder.AppendLine(",")
                    .Append("required: ")
                    .Append(_code.Literal(true));
            }

            if (foreignKey.IsRequiredDependent)
            {
                mainBuilder.AppendLine(",")
                    .Append("requiredDependent: ")
                    .Append(_code.Literal(true));
            }

            if (foreignKey.IsOwnership)
            {
                mainBuilder.AppendLine(",")
                    .Append("ownership: ")
                    .Append(_code.Literal(true));
            }

            mainBuilder
                .AppendLine(");")
                .AppendLine()
                .DecrementIndent();

            var parameters = new CSharpRuntimeAnnotationCodeGeneratorParameters(
                foreignKeyVariable,
                className,
                @namespace,
                mainBuilder,
                methodBuilder,
                namespaces,
                scopeVariables.Inverse,
                scopeVariables,
                configurationClassNames,
                nullable);

            var navigation = foreignKey.DependentToPrincipal;
            if (navigation != null)
            {
                Create(navigation, foreignKeyVariable, parameters with { TargetName = declaringEntityType });
            }

            navigation = foreignKey.PrincipalToDependent;
            if (navigation != null)
            {
                Create(navigation, foreignKeyVariable, parameters with { TargetName = principalEntityType });
            }

            CreateAnnotations(
                foreignKey,
                _annotationCodeGenerator.Generate,
                parameters);

            mainBuilder
                .Append("return ")
                .Append(foreignKeyVariable)
                .AppendLine(";");
        }

        mainBuilder
            .AppendLine("}");
    }

    private void Create(
        INavigation navigation,
        string foreignKeyVariable,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var mainBuilder = parameters.MainBuilder;
        var navigationVariable = _code.Identifier(navigation.Name, navigation, parameters.ScopeObjects, capitalize: false);
        mainBuilder
            .Append("var ").Append(navigationVariable).Append(" = ")
            .Append(parameters.TargetName).Append(".AddNavigation(").IncrementIndent()
            .Append(_code.Literal(navigation.Name)).AppendLine(",")
            .Append(foreignKeyVariable).AppendLine(",")
            .Append("onDependent: ").Append(_code.Literal(navigation.IsOnDependent));

        GeneratePropertyBaseParameters(navigation, parameters);

        if (navigation.IsEagerLoaded)
        {
            mainBuilder.AppendLine(",")
                .Append("eagerLoaded: ").Append(_code.Literal(true));
        }

        if (!navigation.LazyLoadingEnabled)
        {
            mainBuilder.AppendLine(",")
                .Append("lazyLoadingEnabled: ").Append(_code.Literal(false));
        }

        mainBuilder
            .AppendLine(");")
            .AppendLine()
            .DecrementIndent();

        var navigationParameters = parameters with { TargetName = navigationVariable };

        var memberAccessReplacements = CreatePrivateAccessors(navigation, null, navigationParameters, create: false, qualify: true);

        SetNavigationBaseProperties(navigation, memberAccessReplacements, navigationParameters);

        CreateAnnotations(navigation, _annotationCodeGenerator.Generate, navigationParameters);
    }

    private void SetNavigationBaseProperties(
        INavigationBase navigation,
        Dictionary<MemberInfo, QualifiedName>? memberAccessReplacements,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        SetPropertyBaseProperties(navigation, memberAccessReplacements, parameters);

        if (!navigation.IsCollection)
        {
            return;
        }

        var mainBuilder = parameters.MainBuilder;
        ClrCollectionAccessorFactory.Instance.Create(
            navigation,
            out var entityType,
            out var propertyType,
            out var elementType,
            out var getCollection,
            out var setCollection,
            out var setCollectionForMaterialization,
            out var createAndSetCollection,
            out var createCollection);

        AddNamespace(propertyType, parameters.Namespaces);
        mainBuilder
            .Append(parameters.TargetName)
            .AppendLine($".SetCollectionAccessor<{_code.Reference(entityType)}, {_code.Reference(propertyType)}, {_code.Reference(elementType)}>(")
            .IncrementIndent()
            .AppendLines(getCollection == null
                ? "null"
                : _code.Expression(getCollection, parameters.Namespaces, (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
            .AppendLine(",")
            .AppendLines(setCollection == null
                ? "null"
                : _code.Expression(setCollection, parameters.Namespaces, (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
            .AppendLine(",")
            .AppendLines(setCollectionForMaterialization == null
                ? "null"
                : _code.Expression(setCollectionForMaterialization, parameters.Namespaces, (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
            .AppendLine(",")
            .AppendLines(createAndSetCollection == null
                ? "null"
                : _code.Expression(createAndSetCollection, parameters.Namespaces, (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
            .AppendLine(",")
            .AppendLines(createCollection == null
                ? "null"
                : _code.Expression(createCollection, parameters.Namespaces, (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
            .AppendLine(");")
            .DecrementIndent();
    }

    private void CreateSkipNavigation(
        ISkipNavigation navigation,
        int navigationNumber,
        string @namespace,
        IndentedStringBuilder mainBuilder,
        IndentedStringBuilder methodBuilder,
        SortedSet<string> namespaces,
        Dictionary<ITypeBase, string> configurationClassNames,
        string className,
        bool nullable)
    {
        const string declaringEntityType = "declaringEntityType";
        const string targetEntityType = "targetEntityType";
        const string joinEntityType = "joinEntityType";
        mainBuilder.AppendLine()
            .Append("public static RuntimeSkipNavigation CreateSkipNavigation")
            .Append(navigationNumber.ToString())
            .Append("(RuntimeEntityType ").Append(declaringEntityType)
            .Append(", RuntimeEntityType ").Append(targetEntityType)
            .Append(", RuntimeEntityType ").Append(joinEntityType).AppendLine(")")
            .AppendLine("{");

        using (mainBuilder.Indent())
        {
            const string navigationVariable = "skipNavigation";
            var scopeVariables = new BidirectionalDictionary<object, string>
            {
                { navigation.DeclaringEntityType, declaringEntityType },
                { navigation.TargetEntityType, targetEntityType },
                { navigation.JoinEntityType, joinEntityType },
                { navigation, navigationVariable }
            };

            var parameters = new CSharpRuntimeAnnotationCodeGeneratorParameters(
                navigationVariable,
                className,
                @namespace,
                mainBuilder,
                methodBuilder,
                namespaces,
                scopeVariables.Inverse,
                scopeVariables,
                configurationClassNames,
                nullable);

            mainBuilder
                .Append("var ").Append(navigationVariable).Append(" = ")
                .Append(declaringEntityType).AppendLine(".AddSkipNavigation(").IncrementIndent()
                .Append(_code.Literal(navigation.Name)).AppendLine(",")
                .Append(targetEntityType).AppendLine(",")
                .Append(joinEntityType).AppendLine(".FindForeignKey(");
            using (mainBuilder.Indent())
            {
                FindProperties(joinEntityType, navigation.ForeignKey.Properties, mainBuilder, nullable);
                mainBuilder.AppendLine(",")
                    .Append(declaringEntityType).Append(".FindKey(");
                FindProperties(declaringEntityType, navigation.ForeignKey.PrincipalKey.Properties, mainBuilder, nullable);
                mainBuilder.Append(")");
                if (nullable)
                {
                    mainBuilder.Append("!");
                }

                mainBuilder.AppendLine(",")
                    .Append(declaringEntityType).Append(")");
                if (nullable)
                {
                    mainBuilder.Append("!");
                }
            }

            mainBuilder.AppendLine(",")
                .Append(_code.Literal(navigation.IsCollection)).AppendLine(",")
                .Append(_code.Literal(navigation.IsOnDependent));

            GeneratePropertyBaseParameters(navigation, parameters with { TargetName = declaringEntityType });

            if (navigation.IsEagerLoaded)
            {
                mainBuilder.AppendLine(",")
                    .Append("eagerLoaded: ").Append(_code.Literal(true));
            }

            if (!navigation.LazyLoadingEnabled)
            {
                mainBuilder.AppendLine(",")
                    .Append("lazyLoadingEnabled: ").Append(_code.Literal(false));
            }

            mainBuilder
                .AppendLine(");")
                .DecrementIndent();

            mainBuilder.AppendLine();

            scopeVariables.Add(navigation.Inverse, "inverse");
            mainBuilder
                .Append("var inverse = ").Append(targetEntityType).Append(".FindSkipNavigation(")
                .Append(_code.Literal(navigation.Inverse.Name)).AppendLine(");")
                .AppendLine("if (inverse != null)")
                .AppendLine("{");
            using (mainBuilder.Indent())
            {
                mainBuilder
                    .Append(navigationVariable).AppendLine(".Inverse = inverse;")
                    .Append("inverse.Inverse = ").Append(navigationVariable).AppendLine(";");
            }

            mainBuilder
                .AppendLine("}")
                .AppendLine();

            var memberAccessReplacements = new Dictionary<MemberInfo, QualifiedName>();

            SetNavigationBaseProperties(navigation, memberAccessReplacements, parameters);

            CreateAnnotations(navigation, _annotationCodeGenerator.Generate, parameters);

            mainBuilder
                .Append("return ")
                .Append(navigationVariable)
                .AppendLine(";");
        }

        mainBuilder
            .AppendLine("}");
    }

    private void Create(ITrigger trigger, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var triggerVariable = _code.Identifier(trigger.ModelName, trigger, parameters.ScopeObjects, capitalize: false);

        var mainBuilder = parameters.MainBuilder;
        mainBuilder
            .Append("var ").Append(triggerVariable).Append(" = ").Append(parameters.TargetName).AppendLine(".AddTrigger(")
            .IncrementIndent()
            .Append(_code.Literal(trigger.ModelName))
            .AppendLine(");")
            .DecrementIndent();

        CreateAnnotations(
            trigger,
            _annotationCodeGenerator.Generate,
            parameters with { TargetName = triggerVariable });

        mainBuilder.AppendLine();
    }

    private void CreateAnnotations(
        IEntityType entityType,
        string @namespace,
        IndentedStringBuilder mainBuilder,
        IndentedStringBuilder methodBuilder,
        SortedSet<string> namespaces,
        Dictionary<ITypeBase, string> configurationClassNames,
        bool nullable)
    {
        mainBuilder.AppendLine()
            .Append("public static void CreateAnnotations")
            .AppendLine("(RuntimeEntityType runtimeEntityType)")
            .AppendLine("{");

        var className = configurationClassNames[entityType];
        using (mainBuilder.Indent())
        {
            const string entityTypeVariable = "runtimeEntityType";
            var scopeVariables = new BidirectionalDictionary<object, string> { { entityType, entityTypeVariable } };

            var parameters = new CSharpRuntimeAnnotationCodeGeneratorParameters(
                    entityTypeVariable,
                    className,
                    @namespace,
                    mainBuilder,
                    methodBuilder,
                    namespaces,
                    scopeVariables.Inverse,
                    scopeVariables,
                    configurationClassNames,
                    nullable);

            Dictionary<MemberInfo, QualifiedName>? memberAccessReplacements = null;
            memberAccessReplacements = GenerateMemberReferences(entityType, memberAccessReplacements, parameters);

            foreach (var navigation in entityType.GetNavigations())
            {
                var variableName = _code.Identifier(navigation.Name, navigation, parameters.ScopeObjects, capitalize: false);

                memberAccessReplacements = CreatePrivateAccessors(navigation, memberAccessReplacements, parameters, create: navigation.DeclaringType == entityType, qualify: navigation.DeclaringType != entityType);

                mainBuilder
                    .Append($"var {variableName} = ")
                    .AppendLine($"{parameters.TargetName}.FindNavigation({_code.Literal(navigation.Name)})!;");
            }

            var runtimeType = (IRuntimeEntityType)entityType;

            var originalValuesFactory = OriginalValuesFactoryFactory.Instance.CreateExpression(runtimeType);
            mainBuilder
                .Append(parameters.TargetName).AppendLine(".SetOriginalValuesFactory(")
                .IncrementIndent()
                .AppendLines(_code.Expression(originalValuesFactory, parameters.Namespaces, (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(");")
                .DecrementIndent();

            var storeGeneratedValuesFactory = StoreGeneratedValuesFactoryFactory.Instance.CreateEmptyExpression(runtimeType);
            mainBuilder
                .Append(parameters.TargetName).AppendLine(".SetStoreGeneratedValuesFactory(")
                .IncrementIndent()
                .AppendLines(_code.Expression(storeGeneratedValuesFactory, parameters.Namespaces, (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(");")
                .DecrementIndent();

            var temporaryValuesFactory = TemporaryValuesFactoryFactory.Instance.CreateExpression(runtimeType);
            mainBuilder
                .Append(parameters.TargetName).AppendLine(".SetTemporaryValuesFactory(")
                .IncrementIndent()
                .AppendLines(_code.Expression(temporaryValuesFactory, parameters.Namespaces, (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(");")
                .DecrementIndent();

            var shadowValuesFactory = ShadowValuesFactoryFactory.Instance.CreateExpression(runtimeType);
            mainBuilder
                .Append(parameters.TargetName).AppendLine(".SetShadowValuesFactory(")
                .IncrementIndent()
                .AppendLines(_code.Expression(shadowValuesFactory, parameters.Namespaces, (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(");")
                .DecrementIndent();

            var emptyShadowValuesFactory = EmptyShadowValuesFactoryFactory.Instance.CreateEmptyExpression(runtimeType);
            mainBuilder
                .Append(parameters.TargetName).AppendLine(".SetEmptyShadowValuesFactory(")
                .IncrementIndent()
                .AppendLines(_code.Expression(emptyShadowValuesFactory, parameters.Namespaces, (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(");")
                .DecrementIndent();

            var relationshipSnapshotFactory = RelationshipSnapshotFactoryFactory.Instance.CreateExpression(runtimeType);
            mainBuilder
                .Append(parameters.TargetName).AppendLine(".SetRelationshipSnapshotFactory(")
                .IncrementIndent()
                .AppendLines(_code.Expression(relationshipSnapshotFactory, parameters.Namespaces, (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(");")
                .DecrementIndent();

            AddNamespace(typeof(PropertyCounts), parameters.Namespaces);
            var counts = runtimeType.Counts;
            mainBuilder
                .Append(parameters.TargetName).AppendLine(".Counts = new PropertyCounts(")
                .IncrementIndent()
                .Append("propertyCount: ").Append(_code.Literal(counts.PropertyCount)).AppendLine(",")
                .Append("navigationCount: ").Append(_code.Literal(counts.NavigationCount)).AppendLine(",")
                .Append("complexPropertyCount: ").Append(_code.Literal(counts.ComplexPropertyCount)).AppendLine(",")
                .Append("originalValueCount: ").Append(_code.Literal(counts.OriginalValueCount)).AppendLine(",")
                .Append("shadowCount: ").Append(_code.Literal(counts.ShadowCount)).AppendLine(",")
                .Append("relationshipCount: ").Append(_code.Literal(counts.RelationshipCount)).AppendLine(",")
                .Append("storeGeneratedCount: ").Append(_code.Literal(counts.StoreGeneratedCount)).AppendLine(");")
                .DecrementIndent();

            CreateAnnotations(entityType, _annotationCodeGenerator.Generate, parameters);

            mainBuilder
                .AppendLine()
                .AppendLine("Customize(runtimeEntityType);");
        }

        mainBuilder
            .AppendLine("}")
            .AppendLine()
            .AppendLine("static partial void Customize(RuntimeEntityType runtimeEntityType);");

        Dictionary<MemberInfo, QualifiedName>? GenerateMemberReferences(
            ITypeBase structuralType,
            Dictionary<MemberInfo, QualifiedName>? memberAccessReplacements,
            CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
            bool nested = false)
        {
            var mainBuilder = parameters.MainBuilder;
            foreach (var property in structuralType.GetProperties())
            {
                var variableName = _code.Identifier(property.Name, property, parameters.ScopeObjects, capitalize: false);

                memberAccessReplacements = CreatePrivateAccessors(
                    property, memberAccessReplacements, parameters, create: false, qualify: nested || property.DeclaringType != structuralType);

                mainBuilder
                    .Append($"var {variableName} = ")
                    .AppendLine($"{parameters.ScopeVariables[structuralType]}.FindProperty({_code.Literal(property.Name)})!;");
            }

            foreach (var complexProperty in structuralType.GetComplexProperties())
            {
                var variableName = _code.Identifier(complexProperty.Name, complexProperty, parameters.ScopeObjects, capitalize: false);

                memberAccessReplacements = CreatePrivateAccessors(
                    complexProperty, memberAccessReplacements, parameters, create: false, qualify: nested || complexProperty.DeclaringType != structuralType);

                mainBuilder
                    .Append($"var {variableName} = ")
                    .AppendLine($"{parameters.ScopeVariables[structuralType]}.FindComplexProperty({_code.Literal(complexProperty.Name)})!;");

                var typeVariableName = _code.Identifier(
                    complexProperty.ComplexType.ShortName(), complexProperty.ComplexType, parameters.ScopeObjects, capitalize: false);

                mainBuilder
                    .Append($"var {typeVariableName} = ")
                    .AppendLine($"{variableName}.ComplexType;");

                memberAccessReplacements = GenerateMemberReferences(complexProperty.ComplexType, memberAccessReplacements, parameters, nested: true);
            }

            return memberAccessReplacements;
        }
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
                Annotations = annotatable.GetRuntimeAnnotations().ToDictionary(a => a.Name, a => a.Value),
                IsRuntime = true
            });
    }

    private static void AddNamespace(Type type, ISet<string> namespaces)
        => CSharpRuntimeAnnotationCodeGenerator.AddNamespace(type, namespaces);
}
