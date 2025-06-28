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
    private const string UnsafeAccessorsSuffix = "UnsafeAccessors";

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
        var assemblyInfoFileName = UniquifyFileName(options.ContextType.ShortDisplayName() + AssemblyAttributesSuffix, options);
        scaffoldedFiles.Add(new ScaffoldedFile(assemblyInfoFileName, assemblyAttributesCode));

        var memberAccessReplacements = new Dictionary<MemberInfo, QualifiedName>();
        if (options.ForNativeAot)
        {
            var unsafeAccessorClassNames = new BidirectionalDictionary<Type, string>();
            var unsafeAccessorTypes = new Dictionary<Type, HashSet<MemberInfo>>();
            foreach (var entityType in model.GetEntityTypes())
            {
                RegisterPrivateAccessors(entityType, options, unsafeAccessorClassNames, unsafeAccessorTypes, memberAccessReplacements);

                foreach (var navigation in entityType.GetDeclaredNavigations())
                {
                    RegisterPrivateAccessors(
                        navigation, options.ModelNamespace, unsafeAccessorClassNames, unsafeAccessorTypes, memberAccessReplacements);
                }

                foreach (var navigation in entityType.GetDeclaredSkipNavigations())
                {
                    RegisterPrivateAccessors(
                        navigation, options.ModelNamespace, unsafeAccessorClassNames, unsafeAccessorTypes, memberAccessReplacements);
                }
            }

            foreach (var unsafeAccessorPair in unsafeAccessorTypes)
            {
                var (unsafeAccessorType, members) = unsafeAccessorPair;
                var generatedCode = GenerateUnsafeAccessorType(
                    unsafeAccessorType,
                    members,
                    options.ModelNamespace,
                    unsafeAccessorClassNames[unsafeAccessorType],
                    memberAccessReplacements,
                    nullable);

                var entityTypeFileName = UniquifyFileName(unsafeAccessorClassNames[unsafeAccessorType], options);
                scaffoldedFiles.Add(new ScaffoldedFile(entityTypeFileName, generatedCode));
            }
        }

        var modelCode = CreateModel(options.ModelNamespace, options.ContextType, nullable);
        var modelFileName = UniquifyFileName(options.ContextType.ShortDisplayName() + ModelSuffix, options);
        scaffoldedFiles.Add(new ScaffoldedFile(modelFileName, modelCode));

        var configurationClassNames = new Dictionary<ITypeBase, string>();
        var modelBuilderCode = CreateModelBuilder(
            model, options.ModelNamespace, options.ContextType, configurationClassNames, nullable, options.ForNativeAot);
        var modelBuilderFileName = UniquifyFileName(options.ContextType.ShortDisplayName() + ModelBuilderSuffix, options);
        scaffoldedFiles.Add(new ScaffoldedFile(modelBuilderFileName, modelBuilderCode));

        foreach (var entityType in model.GetEntityTypesInHierarchicalOrder())
        {
            var generatedCode = GenerateEntityType(
                entityType, options.ModelNamespace, configurationClassNames, memberAccessReplacements, nullable, options.ForNativeAot);

            var entityTypeFileName = UniquifyFileName(configurationClassNames[entityType], options);
            scaffoldedFiles.Add(new ScaffoldedFile(entityTypeFileName, generatedCode));
        }

        return scaffoldedFiles;
    }

    private string UniquifyFileName(string name, CompiledModelCodeGenerationOptions options)
        => Uniquifier.Uniquify(
            name,
            options.GeneratedFileNames,
            (options.Suffix ?? "") + FileExtension,
            CompiledModelScaffolder.MaxFileNameLength);

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
        var namespaces = new SortedSet<string>(new NamespaceComparer()) { typeof(DbContextModelAttribute).Namespace!, @namespace };

        AddNamespace(contextType, namespaces);

        mainBuilder
            .Append("[assembly: DbContextModel(typeof(").Append(_code.Reference(contextType))
            .Append("), typeof(").Append(GetModelClassName(contextType)).AppendLine("))]");

        return GenerateHeader(namespaces, currentNamespace: "", nullable) + mainBuilder;
    }

    private string GetModelClassName(Type contextType)
        => _code.Identifier(contextType.ShortDisplayName()) + ModelSuffix;

    private string GenerateUnsafeAccessorType(
        Type type,
        HashSet<MemberInfo> members,
        string @namespace,
        string className,
        Dictionary<MemberInfo, QualifiedName> memberAccessReplacements,
        bool nullable)
    {
        var mainBuilder = new IndentedStringBuilder();
        var namespaces = new SortedSet<string>(new NamespaceComparer());

        AddNamespace(type, namespaces);

        if (!string.IsNullOrEmpty(@namespace))
        {
            mainBuilder
                .Append("namespace ").AppendLine(_code.Namespace(@namespace))
                .AppendLine("{");
            mainBuilder.Indent();
        }

        mainBuilder
            .Append("public static class ").Append(className);
        if (type.IsGenericTypeDefinition)
        {
            var genericParameters = type.GetGenericArguments();
            mainBuilder
                .Append("<")
                .AppendJoin(genericParameters.Select(a => _code.Reference(a)))
                .AppendLine(">");

            using (mainBuilder.Indent())
            {
                foreach (var genericParameter in genericParameters)
                {
                    if (genericParameter.GetGenericParameterConstraints().Length == 0
                        && (genericParameter.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask)
                        == GenericParameterAttributes.None)
                    {
                        continue;
                    }

                    mainBuilder
                        .Append("where ").Append(_code.Reference(genericParameter)).Append(" : ");

                    var constraintList = new List<string>();
                    var constraintAttributes = genericParameter.GenericParameterAttributes;
                    if (constraintAttributes != GenericParameterAttributes.None)
                    {
                        if (constraintAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
                        {
                            constraintList.Add("struct");
                        }

                        if (constraintAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
                        {
                            constraintList.Add("class");
                        }

                        if (constraintAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint)
                            && !constraintAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
                        {
                            constraintList.Add("new()");
                        }

                        Check.DebugAssert(
                            !constraintAttributes.HasFlag(GenericParameterAttributes.VarianceMask),
                            "Variance constraints not supported for type: " + type.DisplayName());
                    }

                    var constraints = genericParameter.GetGenericParameterConstraints();
                    if (constraints.Length != 0)
                    {
                        foreach (var constraint in constraints)
                        {
                            if (constraint == typeof(ValueType))
                            {
                                continue;
                            }

                            AddNamespace(constraint, namespaces);
                            constraintList.Add(_code.Reference(constraint));
                        }
                    }

                    mainBuilder
                        .AppendJoin(constraintList)
                        .AppendLine();
                }
            }
        }
        else
        {
            mainBuilder
                .AppendLine();
        }

        var methodBuilder = new IndentedStringBuilder();
        var scopeVariables = new BidirectionalDictionary<object, string>();
        var parameters = new CSharpRuntimeAnnotationCodeGeneratorParameters(
            "this",
            className,
            @namespace,
            mainBuilder,
            methodBuilder,
            namespaces,
            scopeVariables.Inverse,
            scopeVariables,
            configurationClassNames: [],
            nullable,
            nativeAot: true);

        mainBuilder
            .Append("{");
        using (mainBuilder.Indent())
        {
            foreach (var member in members)
            {
                GeneratePrivateAccessor(member, parameters);
            }

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
            .AppendLine(
                @"        System.AppContext.TryGetSwitch(""Microsoft.EntityFrameworkCore.Issue31751"", out var enabled31751) && enabled31751;")
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
        bool nullable,
        bool nativeAot)
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
                    nullable,
                    nativeAot);

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

    private string GenerateEntityType(
        IEntityType entityType,
        string @namespace,
        Dictionary<ITypeBase, string> entityClassNames,
        Dictionary<MemberInfo, QualifiedName> memberAccessReplacements,
        bool nullable,
        bool nativeAot)
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
            CreateEntityType(
                entityType, @namespace, mainBuilder, methodBuilder, namespaces, entityClassNames, memberAccessReplacements, nullable,
                nativeAot);

            foreach (var complexProperty in entityType.GetDeclaredComplexProperties())
            {
                CreateComplexProperty(
                    complexProperty, @namespace, mainBuilder, methodBuilder, namespaces, entityClassNames, memberAccessReplacements,
                    className, nullable, nativeAot);
            }

            var foreignKeyNumber = 1;
            foreach (var foreignKey in entityType.GetDeclaredForeignKeys())
            {
                CreateForeignKey(
                    foreignKey, foreignKeyNumber++, @namespace, mainBuilder, methodBuilder, namespaces, entityClassNames,
                    memberAccessReplacements, className, nullable, nativeAot);
            }

            var navigationNumber = 1;
            foreach (var navigation in entityType.GetDeclaredSkipNavigations())
            {
                CreateSkipNavigation(
                    navigation, navigationNumber++, @namespace, mainBuilder, methodBuilder, namespaces, entityClassNames,
                    memberAccessReplacements, className, nullable, nativeAot);
            }

            CreateAnnotations(
                entityType, @namespace, mainBuilder, methodBuilder, namespaces, entityClassNames, memberAccessReplacements, nullable,
                nativeAot);

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
        Dictionary<MemberInfo, QualifiedName> memberAccessReplacements,
        bool nullable,
        bool nativeAot)
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
                nullable,
                nativeAot);

            Create(entityType, parameters);

            foreach (var property in entityType.GetDeclaredProperties())
            {
                Create(property, memberAccessReplacements, parameters);
            }

            foreach (var property in entityType.GetDeclaredServiceProperties())
            {
                Create(property, memberAccessReplacements, parameters);
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

        if (entityType.GetDeclaredQueryFilters().Count > 0)
        {
            throw new InvalidOperationException(DesignStrings.CompiledModelQueryFilter(entityType.ShortName()));
        }

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

        var discriminatorProperty = entityType.GetDiscriminatorPropertyName();
        if (discriminatorProperty != null)
        {
            mainBuilder.AppendLine(",")
                .Append("discriminatorProperty: ")
                .Append(_code.Literal(discriminatorProperty));
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
            var valueComparerString = CreateValueComparerType(valueComparerType, property.ClrType, parameters);

            mainBuilder.AppendLine(",")
                .Append("valueComparer: ")
                .Append(valueComparerString);
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

        var shouldSetConverter = providerClrType == null
            && valueConverterType == null
            && converter != null
            && property[CoreAnnotationNames.ValueConverter] != null
            && !parameters.ForNativeAot;
        var typeMappingSet = false;

        if (parameters.ForNativeAot
            || (shouldSetConverter && converter!.MappingHints != null))
        {
            shouldSetConverter = false;
            typeMappingSet = true;
            mainBuilder.Append(variableName).Append(".TypeMapping = ");
            _annotationCodeGenerator.Create(property.GetTypeMapping(), property, propertyParameters);
            mainBuilder.AppendLine(";");
        }

        if (parameters.ForNativeAot
            && (property.IsKey()
                || property.IsForeignKey()
                || property.IsUniqueIndex()))
        {
            var currentComparerType = CurrentValueComparerFactory.Instance.GetComparerType(property);
            AddNamespace(currentComparerType, parameters.Namespaces);

            mainBuilder
                .Append(variableName).Append(".SetCurrentValueComparer(new ")
                .Append(_code.Reference(currentComparerType))
                .AppendLine($"({variableName}));");
        }

        if (shouldSetConverter)
        {
            mainBuilder.Append(variableName).Append(".SetValueConverter(");
            _annotationCodeGenerator.Create(converter!, parameters);
            mainBuilder.AppendLine(");");
        }

        var valueComparer = property.GetValueComparer();
        var typeMappingComparer = property.GetTypeMapping().Comparer;
        if (valueComparerType == null
            && (!parameters.ForNativeAot || valueComparer != typeMappingComparer)
            && (parameters.ForNativeAot || property[CoreAnnotationNames.ValueComparer] != null))
        {
            SetValueComparer(valueComparer, typeMappingComparer, nameof(CoreTypeMapping.Comparer), propertyParameters);
        }

        var keyValueComparer = property.GetKeyValueComparer();
        var typeMappingKeyComparer = property.GetTypeMapping().KeyComparer;
        if (valueComparer != keyValueComparer
            && (!parameters.ForNativeAot || keyValueComparer != typeMappingKeyComparer)
            && (parameters.ForNativeAot || property[CoreAnnotationNames.ValueComparer] != null))
        {
            SetValueComparer(keyValueComparer, typeMappingKeyComparer, nameof(CoreTypeMapping.KeyComparer), propertyParameters);
        }

        var providerValueComparer = property.GetProviderValueComparer();
        var defaultProviderValueComparer = property.ClrType.UnwrapNullableType()
            == (property.GetTypeMapping().Converter?.ProviderClrType ?? property.ClrType).UnwrapNullableType()
                ? property.GetKeyValueComparer()
                : property.GetTypeMapping().ProviderValueComparer;
        if (providerValueComparerType == null
            && (!parameters.ForNativeAot || providerValueComparer != defaultProviderValueComparer)
            && (parameters.ForNativeAot || property[CoreAnnotationNames.ProviderValueComparer] != null))
        {
            SetValueComparer(
                providerValueComparer, property.GetTypeMapping().ProviderValueComparer, nameof(CoreTypeMapping.ProviderValueComparer),
                propertyParameters);
        }

        if (sentinel != null
            && converter != null)
        {
            mainBuilder.Append(variableName).Append(".SetSentinelFromProviderValue(")
                .Append(_code.UnknownLiteral(converter?.ConvertToProvider(sentinel) ?? sentinel))
                .AppendLine(");");
        }

        var elementType = property.GetElementType();
        if (elementType != null)
        {
            Check.DebugAssert(property.IsPrimitiveCollection, $"{property.Name} has an element type, but it's not a primitive collection.");
            Create(elementType, typeMappingSet, propertyParameters);
        }

        CreateAnnotations(
            property,
            _annotationCodeGenerator.Generate,
            propertyParameters);

        mainBuilder.AppendLine();
    }

    private void Create(IElementType elementType, bool typeMappingSet, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var mainBuilder = parameters.MainBuilder;
        var elementVariableName = _code.Identifier(parameters.TargetName + "ElementType", elementType, parameters.ScopeObjects, capitalize: false);
        var elementParameters = parameters with { TargetName = elementVariableName };

        mainBuilder
            .Append("var ").Append(elementVariableName).Append(" = ")
            .Append(parameters.TargetName).Append(".SetElementType(").IncrementIndent()
            .Append(_code.Literal(elementType.ClrType));

        if (elementType.IsNullable)
        {
            mainBuilder.AppendLine(",")
                .Append("nullable: ")
                .Append(_code.Literal(elementType.IsNullable));
        }

        if (elementType.GetMaxLength() != null)
        {
            mainBuilder.AppendLine(",")
                .Append("maxLength: ")
                .Append(_code.Literal(elementType.GetMaxLength()));
        }

        if (elementType.IsUnicode() != null)
        {
            mainBuilder.AppendLine(",")
                .Append("unicode: ")
                .Append(_code.Literal(elementType.IsUnicode()));
        }

        if (elementType.GetPrecision() != null)
        {
            mainBuilder.AppendLine(",")
                .Append("precision: ")
                .Append(_code.Literal(elementType.GetPrecision()));
        }

        if (elementType.GetScale() != null)
        {
            mainBuilder.AppendLine(",")
                .Append("scale: ")
                .Append(_code.Literal(elementType.GetScale()));
        }

        var providerClrType = elementType.GetProviderClrType();
        if (providerClrType != null)
        {
            AddNamespace(providerClrType, parameters.Namespaces);
            mainBuilder.AppendLine(",")
                .Append("providerClrType: ")
                .Append(_code.Literal(providerClrType));
        }

        var jsonValueReaderWriterType = (Type?)elementType[CoreAnnotationNames.JsonValueReaderWriterType];
        if (jsonValueReaderWriterType != null)
        {
            mainBuilder.AppendLine(",")
                .Append("jsonValueReaderWriter: ");
            CSharpRuntimeAnnotationCodeGenerator.CreateJsonValueReaderWriter(jsonValueReaderWriterType, parameters, _code);
        }

        mainBuilder
            .AppendLine(");")
            .DecrementIndent();

        var converter = elementType.FindTypeMapping()?.Converter;
        var shouldSetConverter = providerClrType == null
            && converter != null
            && elementType[CoreAnnotationNames.ValueConverter] != null
            && !parameters.ForNativeAot;

        if (parameters.ForNativeAot
            || (shouldSetConverter && converter!.MappingHints != null))
        {
            shouldSetConverter = false;
            mainBuilder.Append(elementVariableName).Append(".TypeMapping = ");

            if (typeMappingSet)
            {
                mainBuilder.Append(parameters.TargetName).Append(".TypeMapping.ElementTypeMapping");
            }
            else
            {
                _annotationCodeGenerator.Create(elementType.GetTypeMapping(), elementParameters);
            }

            mainBuilder.AppendLine(";");
        }

        if (shouldSetConverter)
        {
            mainBuilder.Append(elementVariableName).Append(".SetValueConverter(");
            _annotationCodeGenerator.Create(converter!, parameters);
            mainBuilder.AppendLine(");");
        }

        var valueComparer = elementType.GetValueComparer();
        var typeMappingComparer = elementType.GetTypeMapping().Comparer;
        if ((!parameters.ForNativeAot || valueComparer != typeMappingComparer)
            && (parameters.ForNativeAot || elementType[CoreAnnotationNames.ValueComparer] != null))
        {
            SetValueComparer(valueComparer, typeMappingComparer, nameof(CoreTypeMapping.Comparer), elementParameters);
        }

        CreateAnnotations(
            elementType,
            _annotationCodeGenerator.Generate,
            elementParameters);
    }

    private string CreateValueComparerType(Type valueComparerType, Type clrType, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        AddNamespace(valueComparerType, parameters.Namespaces);

        var valueComparerString = $"new {_code.Reference(valueComparerType)}()";
        if (clrType.IsNullableValueType())
        {
            var valueComparerElementType = ((ValueComparer)Activator.CreateInstance(valueComparerType)!).Type;
            if (!valueComparerElementType.IsNullableValueType())
            {
                AddNamespace(typeof(NullableValueComparer<>), parameters.Namespaces);
                valueComparerString = $"new NullableValueComparer<{_code.Reference(valueComparerType)}>({valueComparerString})";
            }
        }

        return valueComparerString;
    }

    private void SetValueComparer(
        ValueComparer valueComparer,
        ValueComparer typeMappingComparer,
        string typeMappingComparerProperty,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var mainBuilder = parameters.MainBuilder;
        var valueComparerType = valueComparer.GetType();
        if (valueComparer is IInfrastructure<ValueComparer> { Instance: ValueComparer underlyingValueComparer }
            && typeMappingComparer == underlyingValueComparer
            && valueComparerType.GetDeclaredConstructor([typeof(ValueComparer)]) != null)
        {
            if (!parameters.ForNativeAot
                && valueComparerType.IsGenericType
                && valueComparerType.GetGenericTypeDefinition() == typeof(NullableValueComparer<>))
            {
                return;
            }

            if (parameters.ForNativeAot)
            {
                AddNamespace(valueComparerType, parameters.Namespaces);

                mainBuilder
                    .Append(parameters.TargetName)
                    .Append(".Set").Append(typeMappingComparerProperty).Append("(")
                    .Append("new ").Append(_code.Reference(valueComparerType)).Append("(")
                    .Append(parameters.TargetName).Append(".TypeMapping.").Append(typeMappingComparerProperty)
                    .AppendLine("));");

                return;
            }
        }

        mainBuilder
            .Append(parameters.TargetName)
            .Append(".Set").Append(typeMappingComparerProperty).Append("(");

        _annotationCodeGenerator.Create(valueComparer, parameters);

        mainBuilder
            .AppendLine(");");
    }

    private void
        SetPropertyBaseProperties(
            IPropertyBase property,
            Dictionary<MemberInfo, QualifiedName>? memberAccessReplacements,
            CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.ForNativeAot)
        {
            return;
        }

        var variableName = parameters.TargetName;
        var mainBuilder = parameters.MainBuilder;
        var unsafeAccessors = new HashSet<string>();

        if (!property.IsShadowProperty()
            && property is not IServiceProperty) // Service properties don't use property accessors
        {
            ClrPropertyGetterFactory.Instance.Create(
                property,
                out var getClrValueUsingContainingEntityExpression,
                out var hasSentinelValueUsingContainingEntityExpression,
                out var getClrValueExpression,
                out var hasSentinelValueExpression);

            mainBuilder
                .Append(variableName).AppendLine(".SetGetter(")
                .IncrementIndent();
            if (property.DeclaringType is not IEntityType)
            {
                mainBuilder
                    .AppendLines(
                        _code.Expression(
                            getClrValueUsingContainingEntityExpression, parameters.Namespaces, unsafeAccessors,
                            (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                    .AppendLine(",")
                    .AppendLines(
                        _code.Expression(
                            hasSentinelValueUsingContainingEntityExpression, parameters.Namespaces, unsafeAccessors,
                            (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                    .AppendLine(",");
            }

            // For properties declared on entity types, use only two parameters
            mainBuilder
                .AppendLines(
                    _code.Expression(
                        getClrValueExpression, parameters.Namespaces, unsafeAccessors,
                        (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(",")
                .AppendLines(
                    _code.Expression(
                        hasSentinelValueExpression, parameters.Namespaces, unsafeAccessors,
                        (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(");")
                .DecrementIndent();

            ClrPropertySetterFactory.Instance.Create(property, out var setterExpression);

            mainBuilder
                .Append(variableName).AppendLine(".SetSetter(")
                .IncrementIndent()
                .AppendLines(
                    _code.Expression(
                        setterExpression, parameters.Namespaces, unsafeAccessors,
                        (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(");")
                .DecrementIndent();

            ClrPropertyMaterializationSetterFactory.Instance.Create(property, out var materializationSetterExpression);

            mainBuilder
                .Append(variableName).AppendLine(".SetMaterializationSetter(")
                .IncrementIndent()
                .AppendLines(
                    _code.Expression(
                        materializationSetterExpression, parameters.Namespaces, unsafeAccessors,
                        (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(");")
                .DecrementIndent();

            if (property.IsCollection
                && property is IComplexProperty)
            {
                ClrIndexedCollectionAccessorFactory.Instance.Create(
                    property,
                    out _, out _, out _,
                    out var get, out var set, out var setForMaterialization);

                mainBuilder
                    .Append(variableName).AppendLine(".SetIndexedCollectionAccessor(")
                    .IncrementIndent()
                    .AppendLines(
                        _code.Expression(
                            get!, parameters.Namespaces, unsafeAccessors,
                            (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                    .AppendLine(",")
                    .AppendLines(
                        _code.Expression(
                            set!, parameters.Namespaces, unsafeAccessors,
                            (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                    .AppendLine(",")
                    .AppendLines(
                        _code.Expression(
                            setForMaterialization!, parameters.Namespaces, unsafeAccessors,
                            (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                    .AppendLine(");")
                    .DecrementIndent();
            }
        }

        if (property is not IServiceProperty)
        {
            PropertyAccessorsFactory.Instance.Create(
                property,
                out var currentValueGetter,
                out var preStoreGeneratedCurrentValueGetter,
                out var originalValueGetter,
                out var relationshipSnapshotGetter);

            mainBuilder
                .Append(variableName).AppendLine(".SetAccessors(")
                .IncrementIndent()
                .AppendLines(
                    _code.Expression(
                        currentValueGetter, parameters.Namespaces, unsafeAccessors,
                        (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(",")
                .AppendLines(
                    _code.Expression(
                        preStoreGeneratedCurrentValueGetter, parameters.Namespaces, unsafeAccessors,
                        (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(",")
                .AppendLines(
                    originalValueGetter == null
                        ? "null"
                        : _code.Expression(
                            originalValueGetter, parameters.Namespaces, unsafeAccessors,
                            (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements),
                    skipFinalNewline: true)
                .AppendLine(",")
                .AppendLines(
                    _code.Expression(
                        relationshipSnapshotGetter, parameters.Namespaces, unsafeAccessors,
                        (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements), skipFinalNewline: true)
                .AppendLine(");")
                .DecrementIndent();

            Check.DebugAssert(
                unsafeAccessors.Count == 0, "Generated unsafe accessors not handled: " + string.Join(Environment.NewLine, unsafeAccessors));
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

    private void RegisterPrivateAccessors(
        ITypeBase structuralType,
        CompiledModelCodeGenerationOptions options,
        BidirectionalDictionary<Type, string> unsafeAccessorClassNames,
        Dictionary<Type, HashSet<MemberInfo>> unsafeAccessorTypes,
        Dictionary<MemberInfo, QualifiedName> memberAccessReplacements)
    {
        foreach (var property in structuralType.GetDeclaredProperties())
        {
            RegisterPrivateAccessors(
                property, options.ModelNamespace, unsafeAccessorClassNames, unsafeAccessorTypes, memberAccessReplacements);
        }

        foreach (var property in structuralType.GetDeclaredComplexProperties())
        {
            RegisterPrivateAccessors(
                property, options.ModelNamespace, unsafeAccessorClassNames, unsafeAccessorTypes, memberAccessReplacements);

            RegisterPrivateAccessors(
                property.ComplexType, options, unsafeAccessorClassNames, unsafeAccessorTypes, memberAccessReplacements);
        }
    }

    private Dictionary<MemberInfo, QualifiedName>? RegisterPrivateAccessors(
        IPropertyBase property,
        string @namespace,
        BidirectionalDictionary<Type, string> unsafeAccessorClassNames,
        Dictionary<Type, HashSet<MemberInfo>> unsafeAccessorTypes,
        Dictionary<MemberInfo, QualifiedName>? memberAccessReplacements)
    {
        if (property.IsShadowProperty()
            || property.IsIndexerProperty())
        {
            return memberAccessReplacements;
        }

        var getter = RegisterPrivateAccessor(
            property, forMaterialization: false, forSet: false, @namespace, unsafeAccessorClassNames, unsafeAccessorTypes,
            ref memberAccessReplacements);
        var setter = RegisterPrivateAccessor(
            property, forMaterialization: false, forSet: true, @namespace, unsafeAccessorClassNames, unsafeAccessorTypes,
            ref memberAccessReplacements);
        var queryGetter = RegisterPrivateAccessor(
            property, forMaterialization: true, forSet: false, @namespace, unsafeAccessorClassNames, unsafeAccessorTypes,
            ref memberAccessReplacements);
        var querySetter = RegisterPrivateAccessor(
            property, forMaterialization: true, forSet: true, @namespace, unsafeAccessorClassNames, unsafeAccessorTypes,
            ref memberAccessReplacements);

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

    private QualifiedName? RegisterPrivateAccessor(
        IPropertyBase property,
        bool forMaterialization,
        bool forSet,
        string @namespace,
        BidirectionalDictionary<Type, string> unsafeAccessorClassNames,
        Dictionary<Type, HashSet<MemberInfo>> unsafeAccessorTypes,
        ref Dictionary<MemberInfo, QualifiedName>? memberAccessReplacements)
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

                member = methodInfo;

                break;
            }
        }

        memberAccessReplacements ??= [];
        var methodName = LinqToCSharpSyntaxTranslator.GetUnsafeAccessorName(member);

        var declaringType = member.DeclaringType!;
        if (declaringType.IsGenericType
            && !declaringType.IsGenericTypeDefinition)
        {
            var genericArguments = string.Join(", ", declaringType.GetGenericArguments().Select(a => _code.Reference(a)));
            declaringType = declaringType.GetGenericTypeDefinition();

            if (!unsafeAccessorClassNames.TryGetValue(declaringType, out var className))
            {
                className = Uniquifier.Uniquify(
                    declaringType.Name[..declaringType.Name.IndexOf('`')], unsafeAccessorClassNames.Inverse, UnsafeAccessorsSuffix,
                    int.MaxValue);
                unsafeAccessorClassNames[declaringType] = className;
            }

            var qualifiedName = new QualifiedName($"{className}<{genericArguments}>.{methodName}", @namespace);
            memberAccessReplacements.Add(member, qualifiedName);
            member = declaringType.GetMemberWithSameMetadataDefinitionAs(member);
        }
        else
        {
            if (!unsafeAccessorClassNames.TryGetValue(declaringType, out var className))
            {
                className = Uniquifier.Uniquify(
                    declaringType.Name, unsafeAccessorClassNames.Inverse, UnsafeAccessorsSuffix, int.MaxValue);
                unsafeAccessorClassNames[declaringType] = className;
            }

            var qualifiedName = new QualifiedName(className + "." + methodName, @namespace);
            memberAccessReplacements.Add(member, qualifiedName);
        }

        unsafeAccessorTypes.GetOrAddNew(declaringType).Add(member);

        return null;
    }

    private void GeneratePrivateAccessor(
        MemberInfo member,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var methodName = LinqToCSharpSyntaxTranslator.GetUnsafeAccessorName(member);
        var declaringType = member.DeclaringType!;
        AddNamespace(declaringType, parameters.Namespaces);
        AddNamespace(typeof(UnsafeAccessorAttribute), parameters.Namespaces);
        switch (member)
        {
            case FieldInfo field:
            {
                AddNamespace(field.FieldType, parameters.Namespaces);

                parameters.MainBuilder
                    .AppendLine()
                    .AppendLine($"[UnsafeAccessor(UnsafeAccessorKind.Field, Name = \"{field.Name}\")]")
                    .Append($"public static extern ref {_code.Reference(field.FieldType)} {methodName}(")
                    .AppendLine($"{_code.Reference(declaringType)} @this);");
                break;
            }
            case MethodInfo methodInfo:
            {
                AddNamespace(methodInfo.ReturnType, parameters.Namespaces);
                foreach (var parameter in methodInfo.GetParameters())
                {
                    AddNamespace(parameter.ParameterType, parameters.Namespaces);
                }

                var returnType = methodInfo.ReturnType == typeof(void)
                    ? "void"
                    : _code.Reference(methodInfo.ReturnType);

                parameters.MainBuilder
                    .AppendLine()
                    .AppendLine($"[UnsafeAccessor(UnsafeAccessorKind.Method, Name = \"{methodInfo.Name}\")]")
                    .Append($"public static extern {returnType} {methodName}(")
                    .Append($"{_code.Reference(declaringType)} @this");

                if (methodInfo.GetParameters().Length > 0)
                {
                    parameters.MainBuilder
                        .Append(", ")
                        .AppendJoin(
                            methodInfo.GetParameters().Select(p => _code.Reference(p.ParameterType) + " " + _code.Identifier(p.Name!)));
                }

                parameters.MainBuilder.AppendLine(");");
                break;
            }
            default:
                Check.DebugAssert(false, "Unsupported member type: " + member);
                break;
        }
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
        Dictionary<MemberInfo, QualifiedName> memberAccessReplacements,
        string topClassName,
        bool nullable,
        bool nativeAot)
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
                    nullable,
                    nativeAot);

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

                var discriminatorPropertyName = complexType.GetDiscriminatorPropertyName();
                if (discriminatorPropertyName != null)
                {
                    mainBuilder.AppendLine(",")
                        .Append("discriminatorProperty: ")
                        .Append(_code.Literal(discriminatorPropertyName));
                }

                var discriminatorValue = complexType.GetDiscriminatorValue();
                if (discriminatorValue != null)
                {
                    AddNamespace(discriminatorValue.GetType(), parameters.Namespaces);

                    mainBuilder.AppendLine(",")
                        .Append("discriminatorValue: ")
                        .Append(_code.UnknownLiteral(discriminatorValue));
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
                    memberAccessReplacements,
                    topClassName,
                    nullable,
                    nativeAot);
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
        Dictionary<MemberInfo, QualifiedName> memberAccessReplacements,
        string className,
        bool nullable,
        bool nativeAot)
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
                {
                    foreignKey.DeclaringEntityType != foreignKey.PrincipalEntityType
                        ? foreignKey.PrincipalEntityType
                        : new object(),
                    principalEntityType
                },
                { foreignKey, foreignKeyVariable }
            };

            mainBuilder
                .Append("var ").Append(foreignKeyVariable).Append(" = ")
                .Append(declaringEntityType).Append(".AddForeignKey(").IncrementIndent();
            FindProperties(declaringEntityType, foreignKey.Properties, mainBuilder, nullable, scopeVariables);

            mainBuilder.AppendLine(",")
                .Append(principalEntityType).Append(".FindKey(");
            FindProperties(principalEntityType, foreignKey.PrincipalKey.Properties, mainBuilder, nullable, scopeVariables);
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
                nullable,
                nativeAot);

            var navigation = foreignKey.DependentToPrincipal;
            if (navigation != null)
            {
                Create(navigation, foreignKeyVariable, memberAccessReplacements, parameters with { TargetName = declaringEntityType });
            }

            navigation = foreignKey.PrincipalToDependent;
            if (navigation != null)
            {
                Create(navigation, foreignKeyVariable, memberAccessReplacements, parameters with { TargetName = principalEntityType });
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
        Dictionary<MemberInfo, QualifiedName> memberAccessReplacements,
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

        SetNavigationBaseProperties(navigation, memberAccessReplacements, navigationParameters);

        CreateAnnotations(navigation, _annotationCodeGenerator.Generate, navigationParameters);
    }

    private void SetNavigationBaseProperties(
        INavigationBase navigation,
        Dictionary<MemberInfo, QualifiedName> memberAccessReplacements,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        SetPropertyBaseProperties(navigation, memberAccessReplacements, parameters);

        if (!navigation.IsCollection)
        {
            return;
        }

        if (parameters.ForNativeAot)
        {
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

            var unsafeAccessors = new HashSet<string>();

            AddNamespace(propertyType, parameters.Namespaces);
            mainBuilder
                .Append(parameters.TargetName)
                .AppendLine(
                    $".SetCollectionAccessor<{_code.Reference(entityType)}, {_code.Reference(propertyType)}, {_code.Reference(elementType)}>(")
                .IncrementIndent()
                .AppendLines(
                    getCollection == null
                        ? "null"
                        : _code.Expression(
                            getCollection, parameters.Namespaces, unsafeAccessors,
                            (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements),
                    skipFinalNewline: true)
                .AppendLine(",")
                .AppendLines(
                    setCollection == null
                        ? "null"
                        : _code.Expression(
                            setCollection, parameters.Namespaces, unsafeAccessors,
                            (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements),
                    skipFinalNewline: true)
                .AppendLine(",")
                .AppendLines(
                    setCollectionForMaterialization == null
                        ? "null"
                        : _code.Expression(
                            setCollectionForMaterialization, parameters.Namespaces, unsafeAccessors,
                            (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements),
                    skipFinalNewline: true)
                .AppendLine(",")
                .AppendLines(
                    createAndSetCollection == null
                        ? "null"
                        : _code.Expression(
                            createAndSetCollection, parameters.Namespaces, unsafeAccessors,
                            (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements),
                    skipFinalNewline: true)
                .AppendLine(",")
                .AppendLines(
                    createCollection == null
                        ? "null"
                        : _code.Expression(
                            createCollection, parameters.Namespaces, unsafeAccessors,
                            (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements),
                    skipFinalNewline: true)
                .AppendLine(");")
                .DecrementIndent();

            Check.DebugAssert(
                unsafeAccessors.Count == 0, "Generated unsafe accessors not handled: " + string.Join(Environment.NewLine, unsafeAccessors));
        }
    }

    private void CreateSkipNavigation(
        ISkipNavigation navigation,
        int navigationNumber,
        string @namespace,
        IndentedStringBuilder mainBuilder,
        IndentedStringBuilder methodBuilder,
        SortedSet<string> namespaces,
        Dictionary<ITypeBase, string> configurationClassNames,
        Dictionary<MemberInfo, QualifiedName> memberAccessReplacements,
        string className,
        bool nullable,
        bool nativeAot)
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
                { navigation.JoinEntityType, joinEntityType },
                { navigation, navigationVariable }
            };

            if (navigation.TargetEntityType != navigation.DeclaringEntityType)
            {
                scopeVariables.Add(navigation.TargetEntityType, targetEntityType);
            }

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
                nullable,
                nativeAot);

            mainBuilder
                .Append("var ").Append(navigationVariable).Append(" = ")
                .Append(declaringEntityType).AppendLine(".AddSkipNavigation(").IncrementIndent()
                .Append(_code.Literal(navigation.Name)).AppendLine(",")
                .Append(targetEntityType).AppendLine(",")
                .Append(joinEntityType).AppendLine(".FindForeignKey(");
            using (mainBuilder.Indent())
            {
                FindProperties(joinEntityType, navigation.ForeignKey.Properties, mainBuilder, nullable, scopeVariables);
                mainBuilder.AppendLine(",")
                    .Append(declaringEntityType).Append(".FindKey(");
                FindProperties(declaringEntityType, navigation.ForeignKey.PrincipalKey.Properties, mainBuilder, nullable, scopeVariables);
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
        Dictionary<MemberInfo, QualifiedName> memberAccessReplacements,
        bool nullable,
        bool nativeAot)
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
                nullable,
                nativeAot);

            if (parameters.ForNativeAot)
            {
                GenerateMemberReferences(entityType, parameters);

                foreach (var key in entityType.GetDeclaredKeys())
                {
                    if (scopeVariables == null
                        || !scopeVariables.TryGetValue(key, out var keyVariableName))
                    {
                        keyVariableName = _code.Identifier("key", key, parameters.ScopeObjects);

                        mainBuilder
                            .Append($"var {keyVariableName} = {entityTypeVariable}.{nameof(RuntimeEntityType.FindKey)}(");
                        FindProperties(entityTypeVariable, key.Properties, mainBuilder, nullable, parameters.ScopeVariables);
                        mainBuilder.Append(")");
                        if (nullable)
                        {
                            mainBuilder.Append("!");
                        }

                        mainBuilder.AppendLine(";");
                    }

                    var createKeyValueFactoryMethod = nameof(KeyValueFactoryFactory.CreateCompositeFactory);
                    var keyType = key.GetKeyType();
                    if (key.Properties.Count == 1)
                    {
                        AddNamespace(keyType, parameters.Namespaces);

                        if (keyType.IsNullableType())
                        {
                            var nonNullableKeyType = keyType.UnwrapNullableType();
                            if (nonNullableKeyType == keyType)
                            {
                                // This is just a dummy type to satisfy the generic constraint, it won't actually be used
                                nonNullableKeyType = typeof(int);
                            }

                            createKeyValueFactoryMethod =
                                $"{nameof(KeyValueFactoryFactory.CreateSimpleNullableFactory)}<{_code.Reference(keyType)}, {_code.Reference(nonNullableKeyType)}>";
                        }
                        else
                        {
                            createKeyValueFactoryMethod =
                                $"{nameof(KeyValueFactoryFactory.CreateSimpleNonNullableFactory)}<{_code.Reference(keyType)}>";
                        }
                    }

                    mainBuilder
                        .Append($"{keyVariableName}.{nameof(RuntimeKey.SetPrincipalKeyValueFactory)}(")
                        .AppendLine(
                            $"{_code.Reference(typeof(KeyValueFactoryFactory))}.{createKeyValueFactoryMethod}({keyVariableName}));");

                    mainBuilder
                        .Append($"{keyVariableName}.{nameof(RuntimeKey.SetIdentityMapFactory)}(")
                        .Append($"{_code.Reference(typeof(IdentityMapFactoryFactory))}.{nameof(IdentityMapFactoryFactory.CreateFactory)}")
                        .AppendLine($"<{_code.Reference(keyType)}>({keyVariableName}));");
                }

                foreach (var navigation in entityType.GetNavigations())
                {
                    var variableName = _code.Identifier(navigation.Name, navigation, parameters.ScopeObjects, capitalize: false);

                    mainBuilder
                        .Append($"var {variableName} = ")
                        .Append($"{parameters.TargetName}.FindNavigation({_code.Literal(navigation.Name)})");
                    if (nullable)
                    {
                        mainBuilder.Append("!");
                    }

                    mainBuilder.AppendLine(";");
                }

                foreach (var navigation in entityType.GetSkipNavigations())
                {
                    var variableName = _code.Identifier(navigation.Name, navigation, parameters.ScopeObjects, capitalize: false);

                    mainBuilder
                        .Append($"var {variableName} = ")
                        .Append($"{parameters.TargetName}.FindSkipNavigation({_code.Literal(navigation.Name)})");
                    if (nullable)
                    {
                        mainBuilder.Append("!");
                    }

                    mainBuilder.AppendLine(";");
                }

                var runtimeType = (IRuntimeEntityType)entityType;
                var unsafeAccessors = new HashSet<string>();

                var originalValuesFactory = OriginalValuesFactoryFactory.Instance.CreateExpression(runtimeType);
                mainBuilder
                    .Append(parameters.TargetName).AppendLine(".SetOriginalValuesFactory(")
                    .IncrementIndent()
                    .AppendLines(
                        _code.Expression(
                            originalValuesFactory, parameters.Namespaces, unsafeAccessors,
                            (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements),
                        skipFinalNewline: true)
                    .AppendLine(");")
                    .DecrementIndent();

                var storeGeneratedValuesFactory = StoreGeneratedValuesFactoryFactory.Instance.CreateEmptyExpression(runtimeType);
                mainBuilder
                    .Append(parameters.TargetName).AppendLine(".SetStoreGeneratedValuesFactory(")
                    .IncrementIndent()
                    .AppendLines(
                        _code.Expression(
                            storeGeneratedValuesFactory, parameters.Namespaces, unsafeAccessors,
                            (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements),
                        skipFinalNewline: true)
                    .AppendLine(");")
                    .DecrementIndent();

                var temporaryValuesFactory = TemporaryValuesFactoryFactory.Instance.CreateExpression(runtimeType);
                mainBuilder
                    .Append(parameters.TargetName).AppendLine(".SetTemporaryValuesFactory(")
                    .IncrementIndent()
                    .AppendLines(
                        _code.Expression(
                            temporaryValuesFactory, parameters.Namespaces, unsafeAccessors,
                            (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements),
                        skipFinalNewline: true)
                    .AppendLine(");")
                    .DecrementIndent();

                var shadowValuesFactory = ShadowValuesFactoryFactory.Instance.CreateExpression(runtimeType);
                mainBuilder
                    .Append(parameters.TargetName).AppendLine(".SetShadowValuesFactory(")
                    .IncrementIndent()
                    .AppendLines(
                        _code.Expression(
                            shadowValuesFactory, parameters.Namespaces, unsafeAccessors,
                            (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements),
                        skipFinalNewline: true)
                    .AppendLine(");")
                    .DecrementIndent();

                var emptyShadowValuesFactory = EmptyShadowValuesFactoryFactory.Instance.CreateEmptyExpression(runtimeType);
                mainBuilder
                    .Append(parameters.TargetName).AppendLine(".SetEmptyShadowValuesFactory(")
                    .IncrementIndent()
                    .AppendLines(
                        _code.Expression(
                            emptyShadowValuesFactory, parameters.Namespaces, unsafeAccessors,
                            (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements),
                        skipFinalNewline: true)
                    .AppendLine(");")
                    .DecrementIndent();

                var relationshipSnapshotFactory = RelationshipSnapshotFactoryFactory.Instance.CreateExpression(runtimeType);
                mainBuilder
                    .Append(parameters.TargetName).AppendLine(".SetRelationshipSnapshotFactory(")
                    .IncrementIndent()
                    .AppendLines(
                        _code.Expression(
                            relationshipSnapshotFactory, parameters.Namespaces, unsafeAccessors,
                            (IReadOnlyDictionary<object, string>)parameters.ScopeVariables, memberAccessReplacements),
                        skipFinalNewline: true)
                    .AppendLine(");")
                    .DecrementIndent();

                AddNamespace(typeof(PropertyCounts), parameters.Namespaces);
                var counts = runtimeType.CalculateCounts();
                mainBuilder
                    .Append(parameters.TargetName).AppendLine(".SetCounts(new PropertyCounts(")
                    .IncrementIndent()
                    .Append("propertyCount: ").Append(_code.Literal(counts.PropertyCount)).AppendLine(",")
                    .Append("navigationCount: ").Append(_code.Literal(counts.NavigationCount)).AppendLine(",")
                    .Append("complexPropertyCount: ").Append(_code.Literal(counts.ComplexPropertyCount)).AppendLine(",")
                    .Append("complexCollectionCount: ").Append(_code.Literal(counts.ComplexCollectionCount)).AppendLine(",")
                    .Append("originalValueCount: ").Append(_code.Literal(counts.OriginalValueCount)).AppendLine(",")
                    .Append("shadowCount: ").Append(_code.Literal(counts.ShadowCount)).AppendLine(",")
                    .Append("relationshipCount: ").Append(_code.Literal(counts.RelationshipCount)).AppendLine(",")
                    .Append("storeGeneratedCount: ").Append(_code.Literal(counts.StoreGeneratedCount)).AppendLine("));")
                    .DecrementIndent();

                Check.DebugAssert(
                    unsafeAccessors.Count == 0,
                    "Generated unsafe accessors not handled: " + string.Join(Environment.NewLine, unsafeAccessors));
            }

            CreateAnnotations(entityType, _annotationCodeGenerator.Generate, parameters);

            mainBuilder
                .AppendLine()
                .AppendLine("Customize(runtimeEntityType);");
        }

        mainBuilder
            .AppendLine("}")
            .AppendLine()
            .AppendLine("static partial void Customize(RuntimeEntityType runtimeEntityType);");

        void GenerateMemberReferences(
            ITypeBase structuralType,
            CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            var mainBuilder = parameters.MainBuilder;
            foreach (var property in structuralType.GetProperties())
            {
                var variableName = _code.Identifier(property.Name, property, parameters.ScopeObjects, capitalize: false);

                mainBuilder
                    .Append($"var {variableName} = ")
                    .Append($"{parameters.ScopeVariables[structuralType]}.FindProperty({_code.Literal(property.Name)})");
                if (nullable)
                {
                    mainBuilder.Append("!");
                }

                mainBuilder.AppendLine(";");
            }

            foreach (var complexProperty in structuralType.GetComplexProperties())
            {
                var variableName = _code.Identifier(complexProperty.Name, complexProperty, parameters.ScopeObjects, capitalize: false);

                mainBuilder
                    .Append($"var {variableName} = ")
                    .Append($"{parameters.ScopeVariables[structuralType]}.FindComplexProperty({_code.Literal(complexProperty.Name)})");
                if (nullable)
                {
                    mainBuilder.Append("!");
                }

                mainBuilder.AppendLine(";");

                var typeVariableName = _code.Identifier(
                    complexProperty.ComplexType.ShortName(), complexProperty.ComplexType, parameters.ScopeObjects, capitalize: false);

                mainBuilder
                    .Append($"var {typeVariableName} = ")
                    .AppendLine($"{variableName}.ComplexType;");

                GenerateMemberReferences(complexProperty.ComplexType, parameters);
            }
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
                Annotations = annotatable.GetRuntimeAnnotations().ToDictionary(a => a.Name, a => a.Value), IsRuntime = true
            });
    }

    private static void AddNamespace(Type type, ISet<string> namespaces)
        => CSharpRuntimeAnnotationCodeGenerator.AddNamespace(type, namespaces);
}
