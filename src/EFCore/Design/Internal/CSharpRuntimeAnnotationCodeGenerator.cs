// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Design.Internal;

/// <summary>
///     Base class to be used by database providers when implementing an <see cref="ICSharpRuntimeAnnotationCodeGenerator" />
/// </summary>
public class CSharpRuntimeAnnotationCodeGenerator : ICSharpRuntimeAnnotationCodeGenerator
{
    /// <summary>
    ///     Initializes a new instance of this class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public CSharpRuntimeAnnotationCodeGenerator(CSharpRuntimeAnnotationCodeGeneratorDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual CSharpRuntimeAnnotationCodeGeneratorDependencies Dependencies { get; }

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
        var mainBuilder = parameters.MainBuilder;
        var constructor = converter.GetType().GetDeclaredConstructor([typeof(JsonValueReaderWriter)]);
        var jsonReaderWriterProperty = converter.GetType().GetProperty(nameof(CollectionToJsonStringConverter<object>.JsonReaderWriter));
        if (constructor == null
            || jsonReaderWriterProperty == null)
        {
            AddNamespace(typeof(ValueConverter<,>), parameters.Namespaces);
            AddNamespace(converter.ModelClrType, parameters.Namespaces);
            AddNamespace(converter.ProviderClrType, parameters.Namespaces);

            mainBuilder
                .Append("new ValueConverter<")
                .Append(codeHelper.Reference(converter.ModelClrType))
                .Append(", ")
                .Append(codeHelper.Reference(converter.ProviderClrType))
                .AppendLine(">(")
                .IncrementIndent()
                .AppendLines(codeHelper.Expression(converter.ConvertToProviderExpression, parameters.Namespaces, null, null),
                    skipFinalNewline: true)
                .AppendLine(",")
                .AppendLines(codeHelper.Expression(converter.ConvertFromProviderExpression, parameters.Namespaces, null, null),
                    skipFinalNewline: true);

            if (converter.ConvertsNulls)
            {
                mainBuilder
                    .AppendLine(",")
                    .Append("convertsNulls: true");
            }

            mainBuilder
                .Append(")")
                .DecrementIndent();
        }
        else
        {
            AddNamespace(converter.GetType(), parameters.Namespaces);

            mainBuilder
                .Append("new ")
                .Append(codeHelper.Reference(converter.GetType()))
                .Append("(");

            CreateJsonValueReaderWriter((JsonValueReaderWriter)jsonReaderWriterProperty.GetValue(converter)!, parameters, codeHelper);

            mainBuilder
                .Append(")");
        }
    }

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
        var mainBuilder = parameters.MainBuilder;

        var constructor = comparer.GetType().GetDeclaredConstructor([typeof(ValueComparer)]);
        var elementComparerProperty = comparer.GetType().GetProperty(nameof(ListComparer<object>.ElementComparer));
        if (constructor == null
            || elementComparerProperty == null)
        {
            AddNamespace(typeof(ValueComparer<>), parameters.Namespaces);
            AddNamespace(comparer.Type, parameters.Namespaces);

            mainBuilder
                .Append("new ValueComparer<")
                .Append(codeHelper.Reference(comparer.Type))
                .AppendLine(">(")
                .IncrementIndent()
                .AppendLines(codeHelper.Expression(comparer.EqualsExpression, parameters.Namespaces, null, null),
                    skipFinalNewline: true)
                .AppendLine(",")
                .AppendLines(codeHelper.Expression(comparer.HashCodeExpression, parameters.Namespaces, null, null),
                    skipFinalNewline: true)
                .AppendLine(",")
                .AppendLines(codeHelper.Expression(comparer.SnapshotExpression, parameters.Namespaces, null, null),
                    skipFinalNewline: true)
                .Append(")")
                .DecrementIndent();
        }
        else
        {
            AddNamespace(comparer.GetType(), parameters.Namespaces);

            mainBuilder
                .Append("new ")
                .Append(codeHelper.Reference(comparer.GetType()))
                .Append("(");

            Create((ValueComparer)elementComparerProperty.GetValue(comparer)!, parameters, codeHelper);

            mainBuilder
                .Append(")");
        }
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
            return;
        }

        if (jsonValueReaderWriter is ICompositeJsonValueReaderWriter compositeJsonValueReaderWriter)
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
            return;
        }

        CreateJsonValueReaderWriter(jsonValueReaderWriterType, parameters, codeHelper);
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
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
        ValueComparer? valueComparer = null,
        ValueComparer? keyValueComparer = null,
        ValueComparer? providerValueComparer = null)
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

        mainBuilder
            .Append("comparer: ");
        Create(valueComparer ?? typeMapping.Comparer, parameters, code);

        mainBuilder.AppendLine(",")
            .Append("keyComparer: ");
        Create(keyValueComparer ?? typeMapping.KeyComparer, parameters, code);

        mainBuilder.AppendLine(",")
            .Append("providerValueComparer: ");
        Create(providerValueComparer ?? typeMapping.ProviderValueComparer, parameters, code);

        if (typeMapping.Converter != null
            && typeMapping.Converter != defaultInstance.Converter)
        {
            mainBuilder.AppendLine(",")
                .Append("converter: ");

            Create(typeMapping.Converter, parameters, code);
        }

        var typeDifferent = typeMapping.Converter == null
            && typeMapping.ClrType != defaultInstance.ClrType;
        if (typeDifferent)
        {
            mainBuilder.AppendLine(",")
                .Append($"clrType: {code.Literal(typeMapping.ClrType)}");
        }

        if (typeMapping.JsonValueReaderWriter != null
            && typeMapping.JsonValueReaderWriter != defaultInstance.JsonValueReaderWriter)
        {
            mainBuilder.AppendLine(",")
                .Append("jsonValueReaderWriter: ");

            CreateJsonValueReaderWriter(typeMapping.JsonValueReaderWriter, parameters, code);
        }

        if (typeMapping.ElementTypeMapping != null
            && typeMapping.ElementTypeMapping != defaultInstance.ElementTypeMapping)
        {
            mainBuilder.AppendLine(",")
                .Append("elementMapping: ");

            Create(typeMapping.ElementTypeMapping, parameters);
        }

        mainBuilder
            .Append(")")
            .DecrementIndent();

        return true;
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
