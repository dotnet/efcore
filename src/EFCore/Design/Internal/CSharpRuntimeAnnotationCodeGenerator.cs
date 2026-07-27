// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Design.Internal;

/// <summary>
///     Base class to be used by database providers when implementing an <see cref="ICSharpRuntimeAnnotationCodeGenerator" />
/// </summary>
/// <remarks>
///     Initializes a new instance of this class.
/// </remarks>
/// <param name="dependencies">Parameter object containing dependencies for this service.</param>
public class CSharpRuntimeAnnotationCodeGenerator(CSharpRuntimeAnnotationCodeGeneratorDependencies dependencies)
    : ICSharpRuntimeAnnotationCodeGenerator
{
    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual CSharpRuntimeAnnotationCodeGeneratorDependencies Dependencies { get; } = dependencies;

    /// <inheritdoc />
    public virtual void Generate(IModel model, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var annotations = parameters.Annotations;
        if (!parameters.IsRuntime)
        {
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key)
                    && key != CoreAnnotationNames.ProductVersion
                    && key != CoreAnnotationNames.FullChangeTrackingNotificationsRequired)
                {
                    annotations.Remove(key);
                }
            }
        }
        else
        {
            annotations.Remove(CoreAnnotationNames.ModelDependencies);
            annotations.Remove(CoreAnnotationNames.ReadOnlyModel);
            annotations.Remove(CoreAnnotationNames.DetailedErrorsEnabled);
        }

        GenerateSimpleAnnotations(parameters);
    }

    /// <inheritdoc />
    public virtual void Generate(IEntityType entityType, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var annotations = parameters.Annotations;
        if (!parameters.IsRuntime)
        {
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key)
                    && key != CoreAnnotationNames.DiscriminatorMappingComplete)
                {
                    annotations.Remove(key);
                }
            }
        }

        GenerateSimpleAnnotations(parameters);
    }

    /// <inheritdoc />
    public virtual void Generate(IComplexProperty complexProperty, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var annotations = parameters.Annotations;
        if (!parameters.IsRuntime)
        {
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key)
                    && key != CoreAnnotationNames.DiscriminatorMappingComplete)
                {
                    annotations.Remove(key);
                }
            }
        }

        GenerateSimpleAnnotations(parameters);
    }

    /// <inheritdoc />
    public virtual void Generate(IComplexType complexType, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var annotations = parameters.Annotations;
        if (!parameters.IsRuntime)
        {
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key)
                    && key != CoreAnnotationNames.DiscriminatorMappingComplete)
                {
                    annotations.Remove(key);
                }
            }
        }

        GenerateSimpleAnnotations(parameters);
    }

    /// <inheritdoc />
    public virtual void Generate(IProperty property, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key))
                {
                    annotations.Remove(key);
                }
            }
        }

        GenerateSimpleAnnotations(parameters);
    }

    /// <inheritdoc />
    public virtual void Generate(IServiceProperty property, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key))
                {
                    annotations.Remove(key);
                }
            }
        }

        GenerateSimpleAnnotations(parameters);
    }

    /// <inheritdoc />
    public virtual void Generate(IElementType elementType, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key))
                {
                    annotations.Remove(key);
                }
            }
        }

        GenerateSimpleAnnotations(parameters);
    }

    /// <inheritdoc />
    public virtual void Generate(IKey key, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
            foreach (var (s, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(s))
                {
                    annotations.Remove(s);
                }
            }
        }

        GenerateSimpleAnnotations(parameters);
    }

    /// <inheritdoc />
    public virtual void Generate(IForeignKey foreignKey, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key))
                {
                    annotations.Remove(key);
                }
            }
        }

        GenerateSimpleAnnotations(parameters);
    }

    /// <inheritdoc />
    public virtual void Generate(INavigation navigation, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key))
                {
                    annotations.Remove(key);
                }
            }
        }

        GenerateSimpleAnnotations(parameters);
    }

    /// <inheritdoc />
    public virtual void Generate(ISkipNavigation navigation, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key))
                {
                    annotations.Remove(key);
                }
            }
        }

        GenerateSimpleAnnotations(parameters);
    }

    /// <inheritdoc />
    public virtual void Generate(IIndex index, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key))
                {
                    annotations.Remove(key);
                }
            }
        }

        GenerateSimpleAnnotations(parameters);
    }

    /// <inheritdoc />
    public virtual void Generate(ITrigger trigger, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key))
                {
                    annotations.Remove(key);
                }
            }
        }

        GenerateSimpleAnnotations(parameters);
    }

    /// <inheritdoc />
    public virtual void Generate(ITypeMappingConfiguration typeConfiguration, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key))
                {
                    annotations.Remove(key);
                }
            }
        }

        GenerateSimpleAnnotations(parameters);
    }

    /// <summary>
    ///     Generates code to create the given annotations using literals.
    /// </summary>
    /// <param name="parameters">Parameters used during code generation.</param>
    protected virtual void GenerateSimpleAnnotations(CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        foreach (var (name, value) in parameters.Annotations.OrderBy(a => a.Key))
        {
            if (value != null)
            {
                AddNamespace(value as Type ?? value.GetType(), parameters.Namespaces);
            }

            GenerateSimpleAnnotation(name, Dependencies.CSharpHelper.UnknownLiteral(value), parameters);
        }
    }

    /// <summary>
    ///     Generates code to create the given annotation.
    /// </summary>
    /// <param name="annotationName">The annotation name.</param>
    /// <param name="valueString">The annotation value as a literal.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    protected virtual void GenerateSimpleAnnotation(
        string annotationName,
        string valueString,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (parameters.TargetName != "this")
        {
            parameters.MainBuilder
                .Append(parameters.TargetName)
                .Append('.');
        }

        parameters.MainBuilder
            .Append(parameters.IsRuntime ? "AddRuntimeAnnotation(" : "AddAnnotation(")
            .Append(Dependencies.CSharpHelper.Literal(annotationName))
            .Append(", ")
            .Append(valueString)
            .AppendLine(");");
    }

    /// <summary>
    ///     Adds the namespaces for the given type.
    /// </summary>
    /// <param name="type">A type.</param>
    /// <param name="namespaces">The set of namespaces to add to.</param>
    public static void AddNamespace(Type type, ISet<string> namespaces)
    {
        if (type.IsNested)
        {
            AddNamespace(type.DeclaringType!, namespaces);
        }
        else if (!string.IsNullOrEmpty(type.Namespace))
        {
            namespaces.Add(type.Namespace);
        }

        if (type.IsGenericType)
        {
            foreach (var argument in type.GenericTypeArguments)
            {
                AddNamespace(argument, namespaces);
            }
        }

        if (type.IsArray)
        {
            AddNamespace(type.GetSequenceType(), namespaces);
        }
    }

    /// <inheritdoc />
    public void Create(ValueConverter converter, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => Create(converter, parameters, Dependencies.CSharpHelper);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void Create(
        ValueConverter converter,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
        ICSharpHelper codeHelper)
    {
        if (TryCreateInstanceConverter(converter, parameters, codeHelper))
        {
            return;
        }

        var mainBuilder = parameters.MainBuilder;
        var constructor = converter.GetType().GetDeclaredConstructor([typeof(JsonValueReaderWriter)]);
        var jsonReaderWriterProperty = converter.GetType().GetProperty(nameof(CollectionToJsonStringConverter<>.JsonReaderWriter));
        if (constructor == null
            || jsonReaderWriterProperty == null)
        {
            var missingUnsafeAccessors = new HashSet<string>();
            mainBuilder
                .AppendLines(
                    codeHelper.Expression(converter.ConstructorExpressionWithoutMappingHints, parameters.Namespaces, missingUnsafeAccessors),
                    skipFinalNewline: true);


            Check.DebugAssert(
                missingUnsafeAccessors.Count == 0, "Generated unsafe accessors not handled: " + string.Join(Environment.NewLine, missingUnsafeAccessors));
        }
        else
        {
            AddNamespace(converter.GetType(), parameters.Namespaces);

            mainBuilder
                .Append("new ")
                .Append(codeHelper.Reference(converter.GetType()))
                .Append("(");

            CreateJsonValueReaderWriter(
                (JsonValueReaderWriter)jsonReaderWriterProperty.GetValue(converter)!,
                parameters,
                codeHelper);

            mainBuilder
                .Append(")");
        }
    }

    private static bool TryCreateInstanceConverter(
        ValueConverter converter,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
        ICSharpHelper codeHelper)
    {
        // Built-in value converters expose a cached singleton via a public static Instance property. When the model uses that
        // singleton, the generated code can simply reference it instead of reconstructing the converter from its expressions.
        var converterType = converter.GetType();
        var instanceProperty = converterType.GetProperty(
            "Instance", BindingFlags.Public | BindingFlags.Static);
        if (instanceProperty == null
            || !converterType.IsAssignableFrom(instanceProperty.PropertyType)
            || !ReferenceEquals(converter, instanceProperty.GetValue(null)))
        {
            return false;
        }

        AddNamespace(converterType, parameters.Namespaces);
        parameters.MainBuilder
            .Append(codeHelper.Reference(converterType))
            .Append(".Instance");

        return true;
    }

    /// <inheritdoc />
    public void Create(ValueComparer comparer, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        => Create(comparer, parameters, Dependencies.CSharpHelper);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void Create(
        ValueComparer comparer,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
        ICSharpHelper codeHelper)
    {
        if (TryCreateDefaultComparer(comparer, parameters, codeHelper))
        {
            return;
        }

        var mainBuilder = parameters.MainBuilder;

        var comparerType = comparer.GetType();
        var containsNestedComparerCtor = comparerType.GetTypeInfo().DeclaredConstructors
            .Where(x => !x.IsStatic)
            .Select(x => x.GetParameters())
            .Where(ps => ps.Length == 1)
            .Select(ps => ps[0].ParameterType)
            .Any(t => t == typeof(ValueComparer) || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ValueComparer<>)));

        if (!containsNestedComparerCtor
            || comparer is not IInfrastructure<ValueComparer> { Instance: { } underlyingValueComparer })
        {
            AddNamespace(typeof(ValueComparer<>), parameters.Namespaces);
            AddNamespace(comparer.Type, parameters.Namespaces);

            var unsafeAccessors = new HashSet<string>();

            mainBuilder
                .Append("new ValueComparer<")
                .Append(codeHelper.Reference(comparer.Type))
                .AppendLine(">(")
                .IncrementIndent()
                .AppendLines(
                    codeHelper.Expression(comparer.EqualsExpression, parameters.Namespaces, unsafeAccessors),
                    skipFinalNewline: true)
                .AppendLine(",")
                .AppendLines(
                    codeHelper.Expression(comparer.HashCodeExpression, parameters.Namespaces, unsafeAccessors),
                    skipFinalNewline: true)
                .AppendLine(",")
                .AppendLines(
                    codeHelper.Expression(comparer.SnapshotExpression, parameters.Namespaces, unsafeAccessors),
                    skipFinalNewline: true)
                .Append(")")
                .DecrementIndent();

            Check.DebugAssert(
                unsafeAccessors.Count == 0, "Generated unsafe accessors not handled: " + string.Join(Environment.NewLine, unsafeAccessors));
        }
        else
        {
            AddNamespace(comparerType, parameters.Namespaces);

            mainBuilder
                .Append("new ")
                .Append(codeHelper.Reference(comparerType))
                .Append("(");

            Create(underlyingValueComparer, parameters, codeHelper);

            mainBuilder
                .Append(")");
        }
    }

    private static bool TryCreateDefaultComparer(
        ValueComparer comparer,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
        ICSharpHelper codeHelper)
    {
        var comparerType = comparer.GetType();
        var defaultProperty = comparerType.GetProperty(
            nameof(ValueComparer<object>.Default), BindingFlags.Public | BindingFlags.Static);
        var structuralProperty = comparerType.GetProperty(
            nameof(ValueComparer<object>.DefaultWithStructuralComparisons), BindingFlags.Public | BindingFlags.Static);

        if (defaultProperty == null || structuralProperty == null)
        {
            return false;
        }

        PropertyInfo defaultComparerProperty;
        if (ReferenceEquals(comparer, defaultProperty.GetValue(null)))
        {
            defaultComparerProperty = defaultProperty;
        }
        else if (ReferenceEquals(comparer, structuralProperty.GetValue(null)))
        {
            defaultComparerProperty = structuralProperty;
        }
        else
        {
            return false;
        }

        var declaringType = defaultComparerProperty.DeclaringType!;
        AddNamespace(declaringType, parameters.Namespaces);
        parameters.MainBuilder
            .Append(codeHelper.Reference(declaringType))
            .Append('.')
            .Append(defaultComparerProperty.Name);

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void CreateJsonValueReaderWriter(
        JsonValueReaderWriter jsonValueReaderWriter,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
        ICSharpHelper codeHelper)
    {
        var mainBuilder = parameters.MainBuilder;
        var jsonValueReaderWriterType = jsonValueReaderWriter.GetType();

        if (jsonValueReaderWriter is IJsonConvertedValueReaderWriter jsonConvertedValueReaderWriter)
        {
            AddNamespace(jsonValueReaderWriterType, parameters.Namespaces);

            mainBuilder
                .Append("new ")
                .Append(codeHelper.Reference(jsonValueReaderWriterType))
                .AppendLine("(")
                .IncrementIndent();
            CreateJsonValueReaderWriter(jsonConvertedValueReaderWriter.InnerReaderWriter, parameters, codeHelper);
            mainBuilder.AppendLine(",");
            Create(jsonConvertedValueReaderWriter.Converter, parameters, codeHelper);
            mainBuilder
                .Append(")")
                .DecrementIndent();
        }
        else if (jsonValueReaderWriter is ICompositeJsonValueReaderWriter compositeJsonValueReaderWriter)
        {
            AddNamespace(jsonValueReaderWriterType, parameters.Namespaces);

            mainBuilder
                .Append("new ")
                .Append(codeHelper.Reference(jsonValueReaderWriterType))
                .AppendLine("(")
                .IncrementIndent();
            CreateJsonValueReaderWriter(compositeJsonValueReaderWriter.InnerReaderWriter, parameters, codeHelper);
            mainBuilder
                .Append(")")
                .DecrementIndent();
        }
        else
        {
            var missingUnsafeAccessors = new HashSet<string>();

            mainBuilder
                .AppendLines(
                    codeHelper.Expression(jsonValueReaderWriter.ConstructorExpression, parameters.Namespaces, missingUnsafeAccessors),
                    skipFinalNewline: true);

            Check.DebugAssert(
                missingUnsafeAccessors.Count == 0, "Generated unsafe accessors not handled: " + string.Join(Environment.NewLine, missingUnsafeAccessors));
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void CreateJsonValueReaderWriter(
        Type jsonValueReaderWriterType,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
        ICSharpHelper codeHelper)
    {
        var mainBuilder = parameters.MainBuilder;
        AddNamespace(jsonValueReaderWriterType, parameters.Namespaces);

        var instanceProperty = jsonValueReaderWriterType.GetProperty("Instance");
        if (instanceProperty != null
            && instanceProperty.IsStatic()
            && instanceProperty.GetMethod?.IsPublic == true
            && jsonValueReaderWriterType.IsAssignableFrom(instanceProperty.PropertyType)
            && jsonValueReaderWriterType.IsPublic)
        {
            mainBuilder
                .Append(codeHelper.Reference(jsonValueReaderWriterType))
                .Append(".Instance");
        }
        else
        {
            mainBuilder
                .Append("new ")
                .Append(codeHelper.Reference(jsonValueReaderWriterType))
                .Append("()");
        }
    }

    /// <inheritdoc />
    public virtual bool Create(
        CoreTypeMapping typeMapping,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var mainBuilder = parameters.MainBuilder;
        var code = Dependencies.CSharpHelper;
        var defaultInstance = CreateDefaultTypeMapping(typeMapping, parameters);
        if (defaultInstance == null)
        {
            return true;
        }

        mainBuilder
            .AppendLine(".Clone(")
            .IncrementIndent();

        var firstArgument = true;
        CreateComparers(typeMapping, parameters, code, ref firstArgument);

        if (typeMapping.Converter != null
            && typeMapping.Converter != defaultInstance.Converter)
        {
            AppendArgument("converter", parameters, ref firstArgument);
            Create(typeMapping.Converter, parameters, code);
        }

        if (typeMapping.JsonValueReaderWriter != null
            && typeMapping.JsonValueReaderWriter != defaultInstance.JsonValueReaderWriter)
        {
            AppendArgument("jsonValueReaderWriter", parameters, ref firstArgument);
            CreateJsonValueReaderWriter(typeMapping.JsonValueReaderWriter, parameters, code);
        }

        if (typeMapping.ElementTypeMapping != null
            && typeMapping.ElementTypeMapping != defaultInstance.ElementTypeMapping)
        {
            AppendArgument("elementMapping", parameters, ref firstArgument);
            Create(typeMapping.ElementTypeMapping, parameters);
        }

        mainBuilder
            .Append(")")
            .DecrementIndent();

        return true;
    }

    /// <summary>
    ///     Writes the name of an argument in a type mapping <c>Clone</c> call, prefixing it with a separator unless it is the first one.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    /// <param name="firstArgument">Whether this is the first argument in the argument list. Set to <see langword="false" /> on return.</param>
    protected static void AppendArgument(
        string name,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
        ref bool firstArgument)
    {
        if (!firstArgument)
        {
            parameters.MainBuilder.AppendLine(",");
        }

        firstArgument = false;
        parameters.MainBuilder.Append(name).Append(": ");
    }

    /// <summary>
    ///     Generates code for the comparer, key comparer and provider value comparer arguments of a type mapping
    ///     <c>Clone</c> call, unless they can be omitted because they are the defaults for the mapping's CLR type.
    /// </summary>
    /// <param name="typeMapping">The type mapping being generated.</param>
    /// <param name="parameters">Additional parameters used during code generation.</param>
    /// <param name="codeHelper">The C# helper.</param>
    /// <param name="firstArgument">Whether the next argument to be written is the first one in the argument list.</param>
    protected virtual void CreateComparers(
        CoreTypeMapping typeMapping,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
        ICSharpHelper codeHelper,
        ref bool firstArgument)
    {
        if (!(typeMapping.HasDefaultComparers && DefaultComparersAreAotSafe(typeMapping)))
        {
            var comparerBuilder = new IndentedStringBuilder();
            Create(typeMapping.Comparer, parameters with { MainBuilder = comparerBuilder }, codeHelper);
            var comparerCode = comparerBuilder.ToString();

            AppendArgument("comparer", parameters, ref firstArgument);
            parameters.MainBuilder.AppendLines(comparerCode, skipFinalNewline: true);

            // The key comparer only needs to be generated when it differs from the value comparer.
            if (!ReferenceEquals(typeMapping.Comparer, typeMapping.KeyComparer))
            {
                var keyComparerBuilder = new IndentedStringBuilder();
                Create(typeMapping.KeyComparer, parameters with { MainBuilder = keyComparerBuilder }, codeHelper);
                var keyComparerCode = keyComparerBuilder.ToString();

                if (keyComparerCode != comparerCode)
                {
                    AppendArgument("keyComparer", parameters, ref firstArgument);
                    parameters.MainBuilder.AppendLines(keyComparerCode, skipFinalNewline: true);
                }
            }
        }

        // The default provider value comparer is created reflectively when the mapping has a converter, so it must always be baked into
        // the compiled model to remain NativeAOT-compatible. Without a converter it is the same as the key comparer, so it can be omitted
        // whenever the key comparer is.
        if (typeMapping.Converter is not null)
        {
            AppendArgument("providerValueComparer", parameters, ref firstArgument);
            Create(typeMapping.ProviderValueComparer, parameters, codeHelper);
        }
    }

    private static bool DefaultComparersAreAotSafe(CoreTypeMapping typeMapping)
    {
        // The default comparers are reconstructed at runtime without reflection only when the mapping (or one of its base types)
        // overrides CreateDefaultComparer as a generic type whose type argument matches the mapping's CLR type
        // (e.g. RelationalTypeMapping<int>, InMemoryTypeMapping<Guid>).
        var declaringType = typeMapping.GetType()
            .GetMethod("CreateDefaultComparer", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.DeclaringType;

        return declaringType is { IsGenericType: true }
            && declaringType.GetGenericArguments()[0] == typeMapping.ClrType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual CoreTypeMapping? CreateDefaultTypeMapping(
        CoreTypeMapping typeMapping,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var typeMappingType = typeMapping.GetType();
        var mainBuilder = parameters.MainBuilder;
        var code = Dependencies.CSharpHelper;
        var defaultProperty = typeMappingType.GetProperty("Default");
        if (defaultProperty == null
            || !defaultProperty.IsStatic()
            || (defaultProperty.GetMethod?.IsPublic) != true
            || !typeMappingType.IsAssignableFrom(defaultProperty.PropertyType)
            || !typeMappingType.IsPublic)
        {
            throw new InvalidOperationException(
                CoreStrings.CompiledModelIncompatibleTypeMapping(typeMappingType.ShortDisplayName()));
        }

        AddNamespace(typeMappingType, parameters.Namespaces);
        mainBuilder
            .Append(code.Reference(typeMappingType))
            .Append(".Default");

        var defaultInstance = (CoreTypeMapping)defaultProperty.GetValue(null)!;
        return typeMapping == defaultInstance ? null : defaultInstance;
    }
}
