// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
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
            Check.NotNull(annotationCodeGenerator, nameof(annotationCodeGenerator));
            Check.NotNull(cSharpHelper, nameof(cSharpHelper));

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
            Check.NotNull(model, nameof(model));
            Check.NotNull(options, nameof(options));

            var scaffoldedFiles = new List<ScaffoldedFile>();
            var modelCode = CreateModel(model, options.ModelNamespace, options.ContextType, options.UseNullableReferenceTypes);
            var modelFileName = options.ContextType.ShortDisplayName() + ModelSuffix + FileExtension;
            scaffoldedFiles.Add(new ScaffoldedFile { Path = modelFileName, Code = modelCode });

            var entityTypeIds = new Dictionary<IEntityType, (string Variable, string Class)>();
            var modelBuilderCode = CreateModelBuilder(model, options.ModelNamespace, options.ContextType, entityTypeIds, options.UseNullableReferenceTypes);
            var modelBuilderFileName = options.ContextType.ShortDisplayName() + ModelBuilderSuffix + FileExtension;
            scaffoldedFiles.Add(new ScaffoldedFile { Path = modelBuilderFileName, Code = modelBuilderCode });

            foreach (var (entityType, namePair) in entityTypeIds)
            {
                var generatedCode = GenerateEntityType(entityType, options.ModelNamespace, namePair.Class, options.UseNullableReferenceTypes);

                var entityTypeFileName = namePair.Class + FileExtension;
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

            if (nullable)
            {
                builder.AppendLine("#nullable enable");
            }
            else
            {
                builder.AppendLine("#nullable disable");
            }

            builder.AppendLine();

            return builder.ToString();
        }

        private string CreateModel(
            IModel model,
            string @namespace,
            Type contextType,
            bool nullable)
        {
            var mainBuilder = new IndentedStringBuilder();
            var namespaces = new SortedSet<string>(new NamespaceComparer()) {
                contextType.Namespace!,
                typeof(RuntimeModel).Namespace!,
                typeof(DbContextAttribute).Namespace!
            };

            if (!string.IsNullOrEmpty(@namespace))
            {
                mainBuilder
                    .Append("namespace ").AppendLine(_code.Namespace(@namespace))
                    .AppendLine("{");
                mainBuilder.Indent();
            }

            var className = _code.Identifier(contextType.ShortDisplayName()) + ModelSuffix;
            mainBuilder
                .Append("[DbContext(typeof(").Append(_code.Reference(contextType)).AppendLine("))]")
                .Append("partial class ").Append(className).AppendLine(" : " + nameof(RuntimeModel))
                .AppendLine("{");

            using (mainBuilder.Indent())
            {
                mainBuilder
                    .Append("private static ").Append(className).AppendLine(nullable ? "? _instance;" : " _instance;")
                    .Append("public static IModel Instance")
                    .AppendLines(@"
{
    get
    {
        if (_instance == null)
        {
            _instance = new " + className + @"();
            _instance.Initialize();
            _instance.Customize();
        }

        return _instance;
    }
}");

                mainBuilder
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
            Dictionary<IEntityType, (string Variable, string Class)> entityTypeIds,
            bool nullable)
        {
            var mainBuilder = new IndentedStringBuilder();
            var methodBuilder = new IndentedStringBuilder();
            var namespaces = new SortedSet<string>(new NamespaceComparer()) {
                typeof(RuntimeModel).Namespace!,
                typeof(DbContextAttribute).Namespace!
            };

            if (!string.IsNullOrEmpty(@namespace))
            {
                mainBuilder
                    .Append("namespace ").AppendLine(_code.Namespace(@namespace))
                    .AppendLine("{");
                mainBuilder.Indent();
            }

            var className = _code.Identifier(contextType.ShortDisplayName()) + ModelSuffix;
            mainBuilder
                .Append("partial class ").AppendLine(className)
                .AppendLine("{");

            using (mainBuilder.Indent())
            {
                mainBuilder
                    .AppendLine("partial void Initialize()")
                    .AppendLine("{");
                using (mainBuilder.Indent())
                {
                    var entityTypes = model.GetEntityTypesInHierarchicalOrder();
                    var variables = new HashSet<string>();

                    foreach (var entityType in entityTypes)
                    {
                        var variableName = _code.Identifier(entityType.ShortName(), variables, capitalize: false);

                        var firstChar = variableName[0] == '@' ? variableName[1] : variableName[0];
                        var entityClassName = firstChar == '_'
                            ? EntityTypeSuffix + variableName[1..]
                            : char.ToUpperInvariant(firstChar) + variableName[(variableName[0] == '@' ? 2 : 1)..] + EntityTypeSuffix;

                        entityTypeIds[entityType] = (variableName, entityClassName);

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
                                .Append(entityTypeIds[entityType.BaseType].Variable);
                        }

                        mainBuilder
                            .AppendLine(");");
                    }

                    if (entityTypes.Count > 0)
                    {
                        mainBuilder.AppendLine();
                    }

                    var anyForeignKeys = false;
                    foreach (var (entityType, namePair) in entityTypeIds)
                    {
                        var foreignKeyNumber = 1;
                        var (variableName, entityClassName) = namePair;
                        foreach (var foreignKey in entityType.GetDeclaredForeignKeys())
                        {
                            anyForeignKeys = true;
                            var principalVariable = entityTypeIds[foreignKey.PrincipalEntityType].Variable;

                            mainBuilder
                                .Append(entityClassName)
                                .Append(".CreateForeignKey")
                                .Append(foreignKeyNumber++.ToString())
                                .Append("(")
                                .Append(variableName)
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
                    foreach (var (entityType, namePair) in entityTypeIds)
                    {
                        var navigationNumber = 1;
                        var (variableName, entityClassName) = namePair;
                        foreach (var navigation in entityType.GetDeclaredSkipNavigations())
                        {
                            anySkipNavigations = true;
                            var targetVariable = entityTypeIds[navigation.TargetEntityType].Variable;
                            var joinVariable = entityTypeIds[navigation.JoinEntityType].Variable;

                            mainBuilder
                                .Append(entityClassName)
                                .Append(".CreateSkipNavigation")
                                .Append(navigationNumber++.ToString())
                                .Append("(")
                                .Append(variableName)
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

                    foreach (var (entityType, namePair) in entityTypeIds)
                    {
                        var (variableName, entityClassName) = namePair;

                        mainBuilder
                            .Append(entityClassName)
                            .Append(".CreateAnnotations")
                            .Append("(")
                            .Append(variableName)
                            .AppendLine(");");
                    }

                    if (entityTypes.Count > 0)
                    {
                        mainBuilder.AppendLine();
                    }

                    CreateAnnotations(
                        model,
                        _annotationCodeGenerator.Generate,
                        new CSharpRuntimeAnnotationCodeGeneratorParameters(
                            "this",
                            className,
                            mainBuilder,
                            methodBuilder,
                            namespaces,
                            variables));
                }

                mainBuilder
                    .AppendLine("}");

                var methods = methodBuilder.ToString();
                if (!string.IsNullOrEmpty(methods))
                {
                    mainBuilder.AppendLine()
                        .AppendLines(methods);
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

        private string GenerateEntityType(IEntityType entityType, string @namespace, string className, bool nullable)
        {
            var mainBuilder = new IndentedStringBuilder();
            var methodBuilder = new IndentedStringBuilder();
            var namespaces = new SortedSet<string>(new NamespaceComparer())
            {
                typeof(BindingFlags).Namespace!,
                typeof(RuntimeEntityType).Namespace!
            };

            if (!string.IsNullOrEmpty(@namespace))
            {
                mainBuilder
                    .Append("namespace ").AppendLine(_code.Namespace(@namespace))
                    .AppendLine("{");
                mainBuilder.Indent();
            }

            mainBuilder
                .Append("partial class ").AppendLine(className)
                .AppendLine("{");
            using (mainBuilder.Indent())
            {
                CreateEntityType(entityType, mainBuilder, methodBuilder, namespaces, className, nullable);

                var foreignKeyNumber = 1;
                foreach (var foreignKey in entityType.GetDeclaredForeignKeys())
                {
                    CreateForeignKey(foreignKey, foreignKeyNumber++, mainBuilder, methodBuilder, namespaces, className, nullable);
                }

                var navigationNumber = 1;
                foreach (var navigation in entityType.GetDeclaredSkipNavigations())
                {
                    CreateSkipNavigation(navigation, navigationNumber++, mainBuilder, methodBuilder, namespaces, className, nullable);
                }

                CreateAnnotations(entityType, mainBuilder, methodBuilder, namespaces, className);
            }

            mainBuilder.AppendLine("}");

            if (!string.IsNullOrEmpty(@namespace))
            {
                mainBuilder.DecrementIndent();
                mainBuilder.AppendLine("}");
            }

            return GenerateHeader(namespaces, @namespace, nullable) + mainBuilder + methodBuilder;
        }

        private void CreateEntityType(
            IEntityType entityType,
            IndentedStringBuilder mainBuilder,
            IndentedStringBuilder methodBuilder,
            SortedSet<string> namespaces,
            string className,
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

            using (mainBuilder.Indent())
            {
                var entityTypeVariable = "runtimeEntityType";
                var variables = new HashSet<string>
                {
                    "model",
                    "baseEntityType",
                    entityTypeVariable
                };

                var parameters = new CSharpRuntimeAnnotationCodeGeneratorParameters(
                    entityTypeVariable,
                    className,
                    mainBuilder,
                    methodBuilder,
                    namespaces,
                    variables);

                Create(entityType, parameters, className);

                var propertyVariables = new Dictionary<IProperty, string>();
                foreach (var property in entityType.GetDeclaredProperties())
                {
                    Create(property, propertyVariables, parameters, className);
                }

                foreach (var property in entityType.GetDeclaredServiceProperties())
                {
                    Create(property, parameters);
                }

                foreach (var key in entityType.GetDeclaredKeys())
                {
                    Create(key, propertyVariables, parameters, nullable);
                }

                foreach (var index in entityType.GetDeclaredIndexes())
                {
                    Create(index, propertyVariables, parameters, nullable);
                }

                mainBuilder
                    .Append("return ")
                    .Append(entityTypeVariable)
                    .AppendLine(";");
            }

            mainBuilder
                .AppendLine("}");
        }

        private void Create(IEntityType entityType, CSharpRuntimeAnnotationCodeGeneratorParameters parameters, string className)
        {
            var runtimeEntityType = entityType as IRuntimeEntityType;
            if ((entityType.ConstructorBinding is not null
                && ((runtimeEntityType?.GetConstructorBindingConfigurationSource()).OverridesStrictly(ConfigurationSource.Convention)
                    || entityType.ConstructorBinding is FactoryMethodBinding))
                || (runtimeEntityType?.ServiceOnlyConstructorBinding is not null
                    && (runtimeEntityType.GetServiceOnlyConstructorBindingConfigurationSource().OverridesStrictly(ConfigurationSource.Convention)
                        || runtimeEntityType.ServiceOnlyConstructorBinding is FactoryMethodBinding)))
            {
                throw new InvalidOperationException(DesignStrings.CompiledModelConstructorBinding(
                    entityType.ShortName(), "Customize()", className));
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
                    .Append(")");
            }

            if (entityType.IsPropertyBag)
            {
                mainBuilder.AppendLine(",")
                    .Append("propertyBag: ")
                    .Append(_code.Literal(true));
            }

            mainBuilder
                .AppendLine(");")
                .AppendLine()
                .DecrementIndent();
        }

        private void Create(
            IProperty property,
            Dictionary<IProperty, string> propertyVariables,
            CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
            string className)
        {
            var valueGeneratorFactoryType = (Type?)property[CoreAnnotationNames.ValueGeneratorFactoryType];
            if (valueGeneratorFactoryType == null
                && property.GetValueGeneratorFactory() != null)
            {
                throw new InvalidOperationException(
                    DesignStrings.CompiledModelValueGenerator(
                        property.DeclaringEntityType.ShortName(), property.Name, nameof(PropertyBuilder.HasValueGeneratorFactory)));
            }

            var valueComparerType = (Type?)property[CoreAnnotationNames.ValueComparerType];
            if (valueComparerType == null
                && property[CoreAnnotationNames.ValueComparer] != null)
            {
                throw new InvalidOperationException(
                    DesignStrings.CompiledModelValueComparer(
                        property.DeclaringEntityType.ShortName(), property.Name, nameof(PropertyBuilder.HasConversion)));
            }

            var valueConverterType = (Type?)property[CoreAnnotationNames.ValueConverterType];
            if (valueConverterType == null
                && property.GetValueConverter() != null)
            {
                throw new InvalidOperationException(
                    DesignStrings.CompiledModelValueConverter(
                        property.DeclaringEntityType.ShortName(), property.Name, nameof(PropertyBuilder.HasConversion)));
            }

            if (property is IConventionProperty conventionProperty
                && conventionProperty.GetTypeMappingConfigurationSource() != null)
            {
                throw new InvalidOperationException(
                    DesignStrings.CompiledModelTypeMapping(
                        property.DeclaringEntityType.ShortName(), property.Name, "Customize()", className));
            }

            var variableName = _code.Identifier(property.Name, parameters.ScopeVariables, capitalize: false);
            propertyVariables[property] = variableName;

            if (property.ClrType.Namespace != null)
            {
                parameters.Namespaces.Add(property.ClrType.Namespace);
            }

            var mainBuilder = parameters.MainBuilder;
            mainBuilder
                .Append("var ").Append(variableName).Append(" = ").Append(parameters.TargetName).AppendLine(".AddProperty(")
                .IncrementIndent()
                .Append(_code.Literal(property.Name));

            PropertyBaseParameters(property, parameters);

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
                if (providerClrType.Namespace != null)
                {
                    parameters.Namespaces.Add(providerClrType.Namespace);
                }

                mainBuilder.AppendLine(",")
                    .Append("providerPropertyType: ")
                    .Append(_code.Literal(providerClrType));
            }

            if (valueGeneratorFactoryType != null)
            {
                if (valueGeneratorFactoryType.Namespace != null)
                {
                    parameters.Namespaces.Add(valueGeneratorFactoryType.Namespace);
                }

                mainBuilder.AppendLine(",")
                    .Append("valueGeneratorFactory: new ")
                    .Append(_code.Reference(valueGeneratorFactoryType))
                    .Append("().Create");
            }

            if (valueConverterType != null)
            {
                if (valueConverterType.Namespace != null)
                {
                    parameters.Namespaces.Add(valueConverterType.Namespace);
                }

                mainBuilder.AppendLine(",")
                    .Append("valueConverter: new ")
                    .Append(_code.Reference(valueConverterType))
                    .Append("()");
            }

            if (valueComparerType != null)
            {
                if (valueComparerType.Namespace != null)
                {
                    parameters.Namespaces.Add(valueComparerType.Namespace);
                }

                mainBuilder.AppendLine(",")
                    .Append("valueComparer: new ")
                    .Append(_code.Reference(valueComparerType))
                    .Append("()");
            }

            mainBuilder
                .AppendLine(");")
                .DecrementIndent();

            CreateAnnotations(
                property,
                _annotationCodeGenerator.Generate,
                parameters with { TargetName = variableName });

            mainBuilder.AppendLine();
        }

        private void PropertyBaseParameters(
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
                if (propertyInfo.DeclaringType?.Namespace != null)
                {
                    parameters.Namespaces.Add(propertyInfo.DeclaringType.Namespace);
                }

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
                        .Append(propertyInfo.GetAccessors().Any() ? "BindingFlags.Public" : "BindingFlags.NonPublic")
                        .Append(propertyInfo.IsStatic() ? " | BindingFlags.Static" : " | BindingFlags.Instance")
                        .Append(" | BindingFlags.DeclaredOnly)");
                }
            }

            var fieldInfo = property.FieldInfo;
            if (fieldInfo != null)
            {
                if (fieldInfo.DeclaringType?.Namespace != null)
                {
                    parameters.Namespaces.Add(fieldInfo.DeclaringType.Namespace);
                }

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
            Dictionary<IProperty, string>? propertyVariables = null)
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

                if (propertyVariables != null
                    && propertyVariables.TryGetValue(property, out var propertyVariable))
                {
                    mainBuilder.Append(propertyVariable);
                }
                else
                {
                    mainBuilder
                        .Append(entityTypeVariable)
                        .Append(".FindProperty(")
                        .Append(_code.Literal(property.Name))
                        .Append(")");

                    if (nullable)
                    {
                        mainBuilder
                            .Append("!");
                    }
                }
            }

            mainBuilder.Append(" }");
        }

        private void Create(
            IServiceProperty property,
            CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            var variableName = _code.Identifier(property.Name, parameters.ScopeVariables, capitalize: false);

            var mainBuilder = parameters.MainBuilder;
            mainBuilder
                .Append("var ").Append(variableName).Append(" = ").Append(parameters.TargetName).AppendLine(".AddServiceProperty(")
                .IncrementIndent()
                .Append(_code.Literal(property.Name));

            PropertyBaseParameters(property, parameters, skipType: true);

            mainBuilder
                .AppendLine(");")
                .DecrementIndent();

            CreateAnnotations(
                property,
                _annotationCodeGenerator.Generate,
                parameters with { TargetName = variableName });

            mainBuilder.AppendLine();
        }

        private void Create(
            IKey key,
            Dictionary<IProperty, string> propertyVariables,
            CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
            bool nullable)
        {
            var variableName = _code.Identifier("key", parameters.ScopeVariables);

            var mainBuilder = parameters.MainBuilder;
            mainBuilder
                .Append("var ").Append(variableName).Append(" = ").Append(parameters.TargetName).AppendLine(".AddKey(")
                .IncrementIndent();
            FindProperties(parameters.TargetName, key.Properties, mainBuilder, nullable, propertyVariables);
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
            Dictionary<IProperty, string> propertyVariables,
            CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
            bool nullable)
        {
            var variableName = _code.Identifier(index.Name ?? "index", parameters.ScopeVariables, capitalize: false);

            var mainBuilder = parameters.MainBuilder;
            mainBuilder
                .Append("var ").Append(variableName).Append(" = ").Append(parameters.TargetName).AppendLine(".AddIndex(")
                .IncrementIndent();

            FindProperties(parameters.TargetName, index.Properties, mainBuilder, nullable, propertyVariables);

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

        private void CreateForeignKey(
            IForeignKey foreignKey,
            int foreignKeyNumber,
            IndentedStringBuilder mainBuilder,
            IndentedStringBuilder methodBuilder,
            SortedSet<string> namespaces,
            string className,
            bool nullable)
        {
            var declaringEntityType = "declaringEntityType";
            var principalEntityType = "principalEntityType";
            mainBuilder.AppendLine()
                .Append("public static RuntimeForeignKey CreateForeignKey").Append(foreignKeyNumber.ToString())
                .Append("(RuntimeEntityType ").Append(declaringEntityType)
                .Append(", RuntimeEntityType ").Append(principalEntityType).AppendLine(")")
                .AppendLine("{");

            using (mainBuilder.Indent())
            {
                var foreignKeyVariable = "runtimeForeignKey";
                var variables = new HashSet<string>
                {
                    declaringEntityType,
                    principalEntityType,
                    foreignKeyVariable
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
                        mainBuilder,
                        methodBuilder,
                        namespaces,
                        variables);

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
            var navigationVariable = _code.Identifier(navigation.Name, parameters.ScopeVariables, capitalize: false);
            mainBuilder
                .Append("var ").Append(navigationVariable).Append(" = ")
                .Append(parameters.TargetName).Append(".AddNavigation(").IncrementIndent()
                .Append(_code.Literal(navigation.Name)).AppendLine(",")
                .Append(foreignKeyVariable).AppendLine(",")
                .Append("onDependent: ").Append(_code.Literal(navigation.IsOnDependent));

            PropertyBaseParameters(navigation, parameters);

            if (navigation.IsEagerLoaded)
            {
                mainBuilder.AppendLine(",")
                    .Append("eagerLoaded: ").Append(_code.Literal(true));
            }

            mainBuilder
                .AppendLine(");")
                .AppendLine()
                .DecrementIndent();

            CreateAnnotations(
                navigation,
                _annotationCodeGenerator.Generate,
                parameters with { TargetName = navigationVariable });
        }

        private void CreateSkipNavigation(
            ISkipNavigation navigation,
            int navigationNumber,
            IndentedStringBuilder mainBuilder,
            IndentedStringBuilder methodBuilder,
            SortedSet<string> namespaces,
            string className,
            bool nullable)
        {
            var declaringEntityType = "declaringEntityType";
            var targetEntityType = "targetEntityType";
            var joinEntityType = "joinEntityType";
            mainBuilder.AppendLine()
                .Append("public static RuntimeSkipNavigation CreateSkipNavigation")
                .Append(navigationNumber.ToString())
                .Append("(RuntimeEntityType ").Append(declaringEntityType)
                .Append(", RuntimeEntityType ").Append(targetEntityType)
                .Append(", RuntimeEntityType ").Append(joinEntityType).AppendLine(")")
                .AppendLine("{");

            using (mainBuilder.Indent())
            {
                var navigationVariable = "skipNavigation";
                var variables = new HashSet<string>
                {
                    declaringEntityType,
                    targetEntityType,
                    joinEntityType,
                    navigationVariable
                };

                var parameters = new CSharpRuntimeAnnotationCodeGeneratorParameters(
                        navigationVariable,
                        className,
                        mainBuilder,
                        methodBuilder,
                        namespaces,
                        variables);

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

                PropertyBaseParameters(navigation, parameters with { TargetName = declaringEntityType });

                if (navigation.IsEagerLoaded)
                {
                    mainBuilder.AppendLine(",")
                        .Append("eagerLoaded: ").Append(_code.Literal(true));
                }

                mainBuilder
                    .AppendLine(");")
                    .DecrementIndent();

                mainBuilder.AppendLine();

                variables.Add("inverse");
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

                CreateAnnotations(
                    navigation,
                    _annotationCodeGenerator.Generate,
                    parameters);

                mainBuilder
                    .Append("return ")
                    .Append(navigationVariable)
                    .AppendLine(";");
            }

            mainBuilder
                .AppendLine("}");
        }

        private void CreateAnnotations(
            IEntityType entityType,
            IndentedStringBuilder mainBuilder,
            IndentedStringBuilder methodBuilder,
            SortedSet<string> namespaces,
            string className)
        {
            mainBuilder.AppendLine()
                .Append("public static void CreateAnnotations")
                .AppendLine("(RuntimeEntityType runtimeEntityType)")
                .AppendLine("{");

            using (mainBuilder.Indent())
            {
                var entityTypeVariable = "runtimeEntityType";
                var variables = new HashSet<string>
                {
                    entityTypeVariable
                };

                CreateAnnotations(
                    entityType,
                    _annotationCodeGenerator.Generate,
                    new CSharpRuntimeAnnotationCodeGeneratorParameters(
                        entityTypeVariable,
                        className,
                        mainBuilder,
                        methodBuilder,
                        namespaces,
                        variables));

                mainBuilder
                    .AppendLine()
                    .AppendLine("Customize(runtimeEntityType);");
            }

            mainBuilder
                .AppendLine("}")
                .AppendLine()
                .AppendLine("static partial void Customize(RuntimeEntityType runtimeEntityType);");
        }

        private void CreateAnnotations<TAnnotatable>(
            TAnnotatable annotatable,
            Action<TAnnotatable, CSharpRuntimeAnnotationCodeGeneratorParameters> process,
            CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
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

        private static void AddNamespace(Type type, ISet<string> namespaces)
        {
            if (type.Namespace != null)
            {
                namespaces.Add(type.Namespace);
            }

            if (type.IsGenericType)
            {
                foreach(var argument in type.GenericTypeArguments)
                {
                    AddNamespace(argument, namespaces);
                }
            }

            var sequenceType = type.TryGetSequenceType();
            if (sequenceType != null)
            {
                AddNamespace(sequenceType, namespaces);
            }
        }
    }
}
