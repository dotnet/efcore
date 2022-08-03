// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations.Design;

/// <summary>
///     Used to generate C# code for creating an <see cref="IModel" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see>, and
///     <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public class CSharpSnapshotGenerator : ICSharpSnapshotGenerator
{
    private static readonly MethodInfo HasAnnotationMethodInfo
        = typeof(ModelBuilder).GetRuntimeMethod(nameof(ModelBuilder.HasAnnotation), new[] { typeof(string), typeof(string) })!;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CSharpSnapshotGenerator" /> class.
    /// </summary>
    /// <param name="dependencies">The dependencies.</param>
    public CSharpSnapshotGenerator(CSharpSnapshotGeneratorDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual CSharpSnapshotGeneratorDependencies Dependencies { get; }

    private ICSharpHelper Code
        => Dependencies.CSharpHelper;

    /// <summary>
    ///     Generates code for creating an <see cref="IModel" />.
    /// </summary>
    /// <param name="modelBuilderName">The <see cref="ModelBuilder" /> variable name.</param>
    /// <param name="model">The model.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    public virtual void Generate(string modelBuilderName, IModel model, IndentedStringBuilder stringBuilder)
    {
        var annotations = Dependencies.AnnotationCodeGenerator
            .FilterIgnoredAnnotations(model.GetAnnotations())
            .ToDictionary(a => a.Name, a => a);

        if (model.GetProductVersion() is { } productVersion)
        {
            annotations[CoreAnnotationNames.ProductVersion] = new Annotation(CoreAnnotationNames.ProductVersion, productVersion);
        }

        GenerateAnnotations(modelBuilderName, model, stringBuilder, annotations, inChainedCall: false, leadingNewline: false);

        foreach (var sequence in model.GetSequences())
        {
            GenerateSequence(modelBuilderName, sequence, stringBuilder);
        }

        GenerateEntityTypes(modelBuilderName, model.GetEntityTypesInHierarchicalOrder(), stringBuilder);
    }

    /// <summary>
    ///     Generates code for <see cref="IEntityType" /> objects.
    /// </summary>
    /// <param name="modelBuilderName">The name of the builder variable.</param>
    /// <param name="entityTypes">The entity types.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateEntityTypes(
        string modelBuilderName,
        IEnumerable<IEntityType> entityTypes,
        IndentedStringBuilder stringBuilder)
    {
        var nonOwnedTypes = entityTypes.Where(e => e.FindOwnership() == null).ToList();
        foreach (var entityType in nonOwnedTypes)
        {
            stringBuilder.AppendLine();

            GenerateEntityType(modelBuilderName, entityType, stringBuilder);
        }

        foreach (var entityType in nonOwnedTypes.Where(
                     e => e.GetDeclaredForeignKeys().Any()
                             || e.GetDeclaredReferencingForeignKeys().Any(fk => fk.IsOwnership)))
        {
            stringBuilder.AppendLine();

            GenerateEntityTypeRelationships(modelBuilderName, entityType, stringBuilder);
        }

        foreach (var entityType in nonOwnedTypes.Where(
                     e => e.GetDeclaredNavigations().Any(n => !n.IsOnDependent && !n.ForeignKey.IsOwnership)))
        {
            stringBuilder.AppendLine();

            GenerateEntityTypeNavigations(modelBuilderName, entityType, stringBuilder);
        }
    }

    /// <summary>
    ///     Generates code for an <see cref="IEntityType" />.
    /// </summary>
    /// <param name="modelBuilderName">The name of the builder variable.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateEntityType(
        string modelBuilderName,
        IEntityType entityType,
        IndentedStringBuilder stringBuilder)
    {
        var ownership = entityType.FindOwnership();
        var ownerNavigation = ownership?.PrincipalToDependent!.Name;

        var entityTypeName = entityType.Name;
        if (ownerNavigation != null
            && entityType.HasSharedClrType
            && entityTypeName == ownership!.PrincipalEntityType.GetOwnedName(entityType.ClrType.ShortDisplayName(), ownerNavigation))
        {
            entityTypeName = entityType.ClrType.DisplayName();
        }

        var entityTypeBuilderName = GenerateEntityTypeBuilderName();

        stringBuilder
            .Append(modelBuilderName)
            .Append(
                ownerNavigation != null
                    ? ownership!.IsUnique ? ".OwnsOne(" : ".OwnsMany("
                    : ".Entity(")
            .Append(Code.Literal(entityTypeName));

        if (ownerNavigation != null)
        {
            stringBuilder
                .Append(", ")
                .Append(Code.Literal(ownerNavigation));
        }

        stringBuilder
            .Append(", ")
            .Append(entityTypeBuilderName)
            .AppendLine(" =>");

        using (stringBuilder.Indent())
        {
            stringBuilder.Append("{");

            using (stringBuilder.Indent())
            {
                GenerateBaseType(entityTypeBuilderName, entityType.BaseType, stringBuilder);

                GenerateProperties(entityTypeBuilderName, entityType.GetDeclaredProperties(), stringBuilder);

                GenerateKeys(
                    entityTypeBuilderName,
                    entityType.GetDeclaredKeys(),
                    entityType.BaseType == null ? entityType.FindPrimaryKey() : null,
                    stringBuilder);

                GenerateIndexes(entityTypeBuilderName, entityType.GetDeclaredIndexes(), stringBuilder);

                GenerateEntityTypeAnnotations(entityTypeBuilderName, entityType, stringBuilder);

                GenerateCheckConstraints(entityTypeBuilderName, entityType, stringBuilder);

                if (ownerNavigation != null)
                {
                    GenerateRelationships(entityTypeBuilderName, entityType, stringBuilder);

                    GenerateNavigations(
                        entityTypeBuilderName, entityType.GetDeclaredNavigations()
                            .Where(n => !n.IsOnDependent && !n.ForeignKey.IsOwnership), stringBuilder);
                }

                GenerateData(
                    entityTypeBuilderName, entityType.GetProperties(), entityType.GetSeedData(providerValues: true), stringBuilder);
            }

            stringBuilder
                .AppendLine("});");
        }

        string GenerateEntityTypeBuilderName()
        {
            if (modelBuilderName.StartsWith("b", StringComparison.Ordinal))
            {
                // ReSharper disable once InlineOutVariableDeclaration
                var counter = 1;
                if (modelBuilderName.Length > 1
                    && int.TryParse(modelBuilderName[1..], out counter))
                {
                    counter++;
                }

                return "b" + (counter == 0 ? "" : counter.ToString());
            }

            return "b";
        }
    }

    /// <summary>
    ///     Generates code for owned entity types.
    /// </summary>
    /// <param name="entityTypeBuilderName">The name of the builder variable.</param>
    /// <param name="ownerships">The foreign keys identifying each entity type.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateOwnedTypes(
        string entityTypeBuilderName,
        IEnumerable<IForeignKey> ownerships,
        IndentedStringBuilder stringBuilder)
    {
        foreach (var ownership in ownerships)
        {
            stringBuilder.AppendLine();

            GenerateOwnedType(entityTypeBuilderName, ownership, stringBuilder);
        }
    }

    /// <summary>
    ///     Generates code for an owned entity types.
    /// </summary>
    /// <param name="entityTypeBuilderName">The name of the builder variable.</param>
    /// <param name="ownership">The foreign key identifying the entity type.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateOwnedType(
        string entityTypeBuilderName,
        IForeignKey ownership,
        IndentedStringBuilder stringBuilder)
        => GenerateEntityType(entityTypeBuilderName, ownership.DeclaringEntityType, stringBuilder);

    /// <summary>
    ///     Generates code for the relationships of an <see cref="IEntityType" />.
    /// </summary>
    /// <param name="modelBuilderName">The name of the builder variable.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateEntityTypeRelationships(
        string modelBuilderName,
        IEntityType entityType,
        IndentedStringBuilder stringBuilder)
    {
        stringBuilder
            .Append(modelBuilderName)
            .Append(".Entity(")
            .Append(Code.Literal(entityType.Name))
            .AppendLine(", b =>");

        using (stringBuilder.Indent())
        {
            stringBuilder.Append("{");

            using (stringBuilder.Indent())
            {
                GenerateRelationships("b", entityType, stringBuilder);
            }

            stringBuilder.AppendLine("});");
        }
    }

    /// <summary>
    ///     Generates code for the relationships of an <see cref="IEntityType" />.
    /// </summary>
    /// <param name="entityTypeBuilderName">The name of the builder variable.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateRelationships(
        string entityTypeBuilderName,
        IEntityType entityType,
        IndentedStringBuilder stringBuilder)
    {
        GenerateForeignKeys(entityTypeBuilderName, entityType.GetDeclaredForeignKeys(), stringBuilder);

        GenerateOwnedTypes(
            entityTypeBuilderName, entityType.GetDeclaredReferencingForeignKeys().Where(fk => fk.IsOwnership), stringBuilder);

        GenerateNavigations(
            entityTypeBuilderName, entityType.GetDeclaredNavigations()
                .Where(n => n.IsOnDependent || (!n.IsOnDependent && n.ForeignKey.IsOwnership)), stringBuilder);
    }

    /// <summary>
    ///     Generates code for the base type of an <see cref="IEntityType" />.
    /// </summary>
    /// <param name="entityTypeBuilderName">The name of the builder variable.</param>
    /// <param name="baseType">The base entity type.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateBaseType(
        string entityTypeBuilderName,
        IEntityType? baseType,
        IndentedStringBuilder stringBuilder)
    {
        if (baseType != null)
        {
            stringBuilder
                .AppendLine()
                .Append(entityTypeBuilderName)
                .Append(".HasBaseType(")
                .Append(Code.Literal(GetFullName(baseType)))
                .AppendLine(");");
        }
    }

    /// <summary>
    ///     Generates code for an <see cref="ISequence" />.
    /// </summary>
    /// <param name="modelBuilderName">The name of the builder variable.</param>
    /// <param name="sequence">The sequence.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateSequence(
        string modelBuilderName,
        ISequence sequence,
        IndentedStringBuilder stringBuilder)
    {
        var sequenceBuilderNameBuilder = new StringBuilder();
        sequenceBuilderNameBuilder
            .Append(modelBuilderName)
            .Append(".HasSequence");

        if (sequence.Type != Sequence.DefaultClrType)
        {
            sequenceBuilderNameBuilder
                .Append("<")
                .Append(Code.Reference(sequence.Type))
                .Append(">");
        }

        sequenceBuilderNameBuilder
            .Append("(")
            .Append(Code.Literal(sequence.Name));

        if (!string.IsNullOrEmpty(sequence.Schema)
            && sequence.Model.GetDefaultSchema() != sequence.Schema)
        {
            sequenceBuilderNameBuilder
                .Append(", ")
                .Append(Code.Literal(sequence.Schema));
        }

        sequenceBuilderNameBuilder.Append(")");
        var sequenceBuilderName = sequenceBuilderNameBuilder.ToString();

        stringBuilder
            .AppendLine()
            .Append(sequenceBuilderName);

        // Note that GenerateAnnotations below does the corresponding decrement
        stringBuilder.IncrementIndent();

        if (sequence.StartValue != Sequence.DefaultStartValue)
        {
            stringBuilder
                .AppendLine()
                .Append(".StartsAt(")
                .Append(Code.Literal(sequence.StartValue))
                .Append(")");
        }

        if (sequence.IncrementBy != Sequence.DefaultIncrementBy)
        {
            stringBuilder
                .AppendLine()
                .Append(".IncrementsBy(")
                .Append(Code.Literal(sequence.IncrementBy))
                .Append(")");
        }

        if (sequence.MinValue != Sequence.DefaultMinValue)
        {
            stringBuilder
                .AppendLine()
                .Append(".HasMin(")
                .Append(Code.Literal(sequence.MinValue))
                .Append(")");
        }

        if (sequence.MaxValue != Sequence.DefaultMaxValue)
        {
            stringBuilder
                .AppendLine()
                .Append(".HasMax(")
                .Append(Code.Literal(sequence.MaxValue))
                .Append(")");
        }

        if (sequence.IsCyclic != Sequence.DefaultIsCyclic)
        {
            stringBuilder
                .AppendLine()
                .Append(".IsCyclic()");
        }

        GenerateSequenceAnnotations(sequenceBuilderName, sequence, stringBuilder);
    }

    /// <summary>
    ///     Generates code for sequence annotations.
    /// </summary>
    /// <param name="sequenceBuilderName">The name of the sequence builder variable.</param>
    /// <param name="sequence">The sequence.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateSequenceAnnotations(
        string sequenceBuilderName,
        ISequence sequence,
        IndentedStringBuilder stringBuilder)
    {
        var annotations = Dependencies.AnnotationCodeGenerator
            .FilterIgnoredAnnotations(sequence.GetAnnotations())
            .ToDictionary(a => a.Name, a => a);

        GenerateAnnotations(sequenceBuilderName, sequence, stringBuilder, annotations, inChainedCall: true);
    }

    /// <summary>
    ///     Generates code for <see cref="IProperty" /> objects.
    /// </summary>
    /// <param name="entityTypeBuilderName">The name of the builder variable.</param>
    /// <param name="properties">The properties.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateProperties(
        string entityTypeBuilderName,
        IEnumerable<IProperty> properties,
        IndentedStringBuilder stringBuilder)
    {
        foreach (var property in properties)
        {
            GenerateProperty(entityTypeBuilderName, property, stringBuilder);
        }
    }

    /// <summary>
    ///     Generates code for an <see cref="IProperty" />.
    /// </summary>
    /// <param name="entityTypeBuilderName">The name of the builder variable.</param>
    /// <param name="property">The property.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateProperty(
        string entityTypeBuilderName,
        IProperty property,
        IndentedStringBuilder stringBuilder)
    {
        var clrType = FindValueConverter(property)?.ProviderClrType.MakeNullable(property.IsNullable)
            ?? property.ClrType;

        var propertyBuilderName = $"{entityTypeBuilderName}.Property<{Code.Reference(clrType)}>({Code.Literal(property.Name)})";

        stringBuilder
            .AppendLine()
            .Append(propertyBuilderName);

        // Note that GenerateAnnotations below does the corresponding decrement
        stringBuilder.IncrementIndent();

        if (property.IsConcurrencyToken)
        {
            stringBuilder
                .AppendLine()
                .Append(".IsConcurrencyToken()");
        }

        if (property.IsNullable != (clrType.IsNullableType() && !property.IsPrimaryKey()))
        {
            stringBuilder
                .AppendLine()
                .Append(".IsRequired()");
        }

        if (property.ValueGenerated != ValueGenerated.Never)
        {
            stringBuilder
                .AppendLine()
                .Append(
                    property.ValueGenerated == ValueGenerated.OnAdd
                        ? ".ValueGeneratedOnAdd()"
                        : property.ValueGenerated == ValueGenerated.OnUpdate
                            ? ".ValueGeneratedOnUpdate()"
                            : property.ValueGenerated == ValueGenerated.OnUpdateSometimes
                                ? ".ValueGeneratedOnUpdateSometimes()"
                                : ".ValueGeneratedOnAddOrUpdate()");
        }

        GeneratePropertyAnnotations(propertyBuilderName, property, stringBuilder);
    }

    /// <summary>
    ///     Generates code for the annotations on an <see cref="IProperty" />.
    /// </summary>
    /// <param name="propertyBuilderName">The name of the builder variable.</param>
    /// <param name="property">The property.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GeneratePropertyAnnotations(
        string propertyBuilderName,
        IProperty property,
        IndentedStringBuilder stringBuilder)
    {
        var annotations = Dependencies.AnnotationCodeGenerator
            .FilterIgnoredAnnotations(property.GetAnnotations())
            .ToDictionary(a => a.Name, a => a);

        GenerateFluentApiForMaxLength(property, stringBuilder);
        GenerateFluentApiForPrecisionAndScale(property, stringBuilder);
        GenerateFluentApiForIsUnicode(property, stringBuilder);

        if (!annotations.ContainsKey(RelationalAnnotationNames.ColumnType))
        {
            annotations[RelationalAnnotationNames.ColumnType] = new Annotation(
                RelationalAnnotationNames.ColumnType,
                property.GetColumnType());
        }

        if (annotations.ContainsKey(RelationalAnnotationNames.DefaultValue)
            && property.TryGetDefaultValue(out var defaultValue)
            && defaultValue != DBNull.Value
            && FindValueConverter(property) is ValueConverter valueConverter)
        {
            annotations[RelationalAnnotationNames.DefaultValue] = new Annotation(
                RelationalAnnotationNames.DefaultValue,
                valueConverter.ConvertToProvider(defaultValue));
        }

        GenerateAnnotations(propertyBuilderName, property, stringBuilder, annotations, inChainedCall: true);
    }

    private ValueConverter? FindValueConverter(IProperty property)
        => property.GetValueConverter()
            ?? (property.FindTypeMapping()
                ?? Dependencies.RelationalTypeMappingSource.FindMapping(property))?.Converter;

    /// <summary>
    ///     Generates code for <see cref="IKey" /> objects.
    /// </summary>
    /// <param name="entityTypeBuilderName">The name of the builder variable.</param>
    /// <param name="keys">The keys.</param>
    /// <param name="primaryKey">The primary key.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateKeys(
        string entityTypeBuilderName,
        IEnumerable<IKey> keys,
        IKey? primaryKey,
        IndentedStringBuilder stringBuilder)
    {
        if (primaryKey != null)
        {
            GenerateKey(entityTypeBuilderName, primaryKey, stringBuilder, primary: true);
        }

        if (primaryKey?.DeclaringEntityType.IsOwned() != true)
        {
            foreach (var key in keys.Where(
                         key => key != primaryKey
                             && (!key.GetReferencingForeignKeys().Any()
                                 || key.GetAnnotations().Any(a => a.Name != RelationalAnnotationNames.UniqueConstraintMappings))))
            {
                GenerateKey(entityTypeBuilderName, key, stringBuilder);
            }
        }
    }

    /// <summary>
    ///     Generates code for an <see cref="IKey" />.
    /// </summary>
    /// <param name="entityTypeBuilderName">The name of the builder variable.</param>
    /// <param name="key">The key.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    /// <param name="primary">A value indicating whether the key is primary.</param>
    protected virtual void GenerateKey(
        string entityTypeBuilderName,
        IKey key,
        IndentedStringBuilder stringBuilder,
        bool primary = false)
    {
        var keyBuilderName = new StringBuilder()
            .Append(entityTypeBuilderName)
            .Append(primary ? ".HasKey(" : ".HasAlternateKey(")
            .Append(string.Join(", ", key.Properties.Select(p => Code.Literal(p.Name))))
            .Append(')')
            .ToString();

        stringBuilder
            .AppendLine()
            .Append(keyBuilderName);

        // Note that GenerateAnnotations below does the corresponding decrement
        stringBuilder.IncrementIndent();

        GenerateKeyAnnotations(keyBuilderName, key, stringBuilder);
    }

    /// <summary>
    ///     Generates code for the annotations on a key.
    /// </summary>
    /// <param name="keyBuilderName">The name of the builder variable.</param>
    /// <param name="key">The key.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateKeyAnnotations(string keyBuilderName, IKey key, IndentedStringBuilder stringBuilder)
    {
        var annotations = Dependencies.AnnotationCodeGenerator
            .FilterIgnoredAnnotations(key.GetAnnotations())
            .ToDictionary(a => a.Name, a => a);

        GenerateAnnotations(keyBuilderName, key, stringBuilder, annotations, inChainedCall: true);
    }

    /// <summary>
    ///     Generates code for <see cref="IIndex" /> objects.
    /// </summary>
    /// <param name="entityTypeBuilderName">The name of the builder variable.</param>
    /// <param name="indexes">The indexes.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateIndexes(
        string entityTypeBuilderName,
        IEnumerable<IIndex> indexes,
        IndentedStringBuilder stringBuilder)
    {
        foreach (var index in indexes)
        {
            GenerateIndex(entityTypeBuilderName, index, stringBuilder);
        }
    }

    /// <summary>
    ///     Generates code an <see cref="IIndex" />.
    /// </summary>
    /// <param name="entityTypeBuilderName">The name of the builder variable.</param>
    /// <param name="index">The index.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateIndex(
        string entityTypeBuilderName,
        IIndex index,
        IndentedStringBuilder stringBuilder)
    {
        // Note - method names below are meant to be hard-coded
        // because old snapshot files will fail if they are changed

        var indexProperties = string.Join(", ", index.Properties.Select(p => Code.Literal(p.Name)));
        var indexBuilderName = $"{entityTypeBuilderName}.HasIndex("
            + (index.Name is null
                ? indexProperties
                : $"new[] {{ {indexProperties} }}, {Code.Literal(index.Name)}")
            + ")";

        stringBuilder
            .AppendLine()
            .Append(indexBuilderName);

        // Note that GenerateAnnotations below does the corresponding decrement
        stringBuilder.IncrementIndent();

        if (index.IsUnique)
        {
            stringBuilder
                .AppendLine()
                .Append(".IsUnique()");
        }

        if (index.IsDescending is not null)
        {
            stringBuilder
                .AppendLine()
                .Append(".IsDescending(")
                .Append(string.Join(", ", index.IsDescending.Select(Code.Literal)))
                .Append(')');
        }

        GenerateIndexAnnotations(indexBuilderName, index, stringBuilder);
    }

    /// <summary>
    ///     Generates code for the annotations on an index.
    /// </summary>
    /// <param name="indexBuilderName">The name of the builder variable.</param>
    /// <param name="index">The index.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateIndexAnnotations(
        string indexBuilderName,
        IIndex index,
        IndentedStringBuilder stringBuilder)
    {
        var annotations = Dependencies.AnnotationCodeGenerator
            .FilterIgnoredAnnotations(index.GetAnnotations())
            .ToDictionary(a => a.Name, a => a);

        GenerateAnnotations(indexBuilderName, index, stringBuilder, annotations, inChainedCall: true);
    }

    /// <summary>
    ///     Generates code for the annotations on an entity type.
    /// </summary>
    /// <param name="entityTypeBuilderName">The name of the builder variable.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateEntityTypeAnnotations(
        string entityTypeBuilderName,
        IEntityType entityType,
        IndentedStringBuilder stringBuilder)
    {
        IAnnotation? discriminatorPropertyAnnotation = null;
        IAnnotation? discriminatorValueAnnotation = null;
        IAnnotation? discriminatorMappingCompleteAnnotation = null;

        foreach (var annotation in entityType.GetAnnotations())
        {
            switch (annotation.Name)
            {
                case CoreAnnotationNames.DiscriminatorProperty:
                    discriminatorPropertyAnnotation = annotation;
                    break;
                case CoreAnnotationNames.DiscriminatorValue:
                    discriminatorValueAnnotation = annotation;
                    break;
                case CoreAnnotationNames.DiscriminatorMappingComplete:
                    discriminatorMappingCompleteAnnotation = annotation;
                    break;
            }
        }

        var annotations = Dependencies.AnnotationCodeGenerator
            .FilterIgnoredAnnotations(entityType.GetAnnotations())
            .ToDictionary(a => a.Name, a => a);

        GenerateTableMapping(entityTypeBuilderName, entityType, stringBuilder, annotations);
        GenerateSplitTableMapping(entityTypeBuilderName, entityType, stringBuilder);

        GenerateViewMapping(entityTypeBuilderName, entityType, stringBuilder, annotations);
        GenerateSplitViewMapping(entityTypeBuilderName, entityType, stringBuilder);

        var functionNameAnnotation = annotations.Find(RelationalAnnotationNames.FunctionName);
        if (functionNameAnnotation != null
            || entityType.BaseType == null)
        {
            var functionName = (string?)functionNameAnnotation?.Value ?? entityType.GetFunctionName();
            if (functionName != null
                || functionNameAnnotation != null)
            {
                stringBuilder
                    .AppendLine()
                    .Append(entityTypeBuilderName)
                    .Append(".ToFunction(")
                    .Append(Code.Literal(functionName))
                    .AppendLine(");");
                if (functionNameAnnotation != null)
                {
                    annotations.Remove(functionNameAnnotation.Name);
                }
            }
        }

        var sqlQueryAnnotation = annotations.Find(RelationalAnnotationNames.SqlQuery);
        if (sqlQueryAnnotation != null
            || entityType.BaseType == null)
        {
            var sqlQuery = (string?)sqlQueryAnnotation?.Value ?? entityType.GetSqlQuery();
            if (sqlQuery != null
                || sqlQueryAnnotation != null)
            {
                stringBuilder
                    .AppendLine()
                    .Append(entityTypeBuilderName)
                    .Append(".ToSqlQuery(")
                    .Append(Code.Literal(sqlQuery))
                    .AppendLine(");");
                if (sqlQueryAnnotation != null)
                {
                    annotations.Remove(sqlQueryAnnotation.Name);
                }
            }
        }

        if ((discriminatorPropertyAnnotation?.Value
                ?? discriminatorMappingCompleteAnnotation?.Value
                ?? discriminatorValueAnnotation?.Value)
            != null)
        {
            stringBuilder
                .AppendLine()
                .Append(entityTypeBuilderName)
                .Append(".")
                .Append("HasDiscriminator");

            if (discriminatorPropertyAnnotation?.Value != null)
            {
                var discriminatorProperty = entityType.FindProperty((string)discriminatorPropertyAnnotation.Value)!;
                var propertyClrType = FindValueConverter(discriminatorProperty)?.ProviderClrType
                        .MakeNullable(discriminatorProperty.IsNullable)
                    ?? discriminatorProperty.ClrType;
                stringBuilder
                    .Append("<")
                    .Append(Code.Reference(propertyClrType))
                    .Append(">(")
                    .Append(Code.Literal((string)discriminatorPropertyAnnotation.Value))
                    .Append(")");
            }
            else
            {
                stringBuilder
                    .Append("()");
            }

            if (discriminatorMappingCompleteAnnotation?.Value != null)
            {
                var value = (bool)discriminatorMappingCompleteAnnotation.Value;

                stringBuilder
                    .Append(".")
                    .Append("IsComplete")
                    .Append("(")
                    .Append(Code.Literal(value))
                    .Append(")");
            }

            if (discriminatorValueAnnotation?.Value != null)
            {
                var value = discriminatorValueAnnotation.Value;
                var discriminatorProperty = entityType.FindDiscriminatorProperty();
                if (discriminatorProperty != null)
                {
                    var valueConverter = FindValueConverter(discriminatorProperty);
                    if (valueConverter != null)
                    {
                        value = valueConverter.ConvertToProvider(value);
                    }
                }

                stringBuilder
                    .Append(".")
                    .Append("HasValue")
                    .Append("(")
                    .Append(Code.UnknownLiteral(value))
                    .Append(")");
            }

            stringBuilder.AppendLine(";");
        }

        GenerateAnnotations(entityTypeBuilderName, entityType, stringBuilder, annotations, inChainedCall: false);
    }

    private void GenerateTableMapping(
        string entityTypeBuilderName,
        IEntityType entityType,
        IndentedStringBuilder stringBuilder,
        Dictionary<string, IAnnotation> annotations)
    {
        annotations.TryGetAndRemove(RelationalAnnotationNames.TableName, out IAnnotation tableNameAnnotation);
        annotations.TryGetAndRemove(RelationalAnnotationNames.Schema, out IAnnotation schemaAnnotation);
        var table = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table);
        var tableName = (string?)tableNameAnnotation?.Value ?? table?.Name;
        if (tableNameAnnotation == null
            && entityType.BaseType != null
            && entityType.BaseType.GetTableName() == tableName)
        {
            return;
        }

        stringBuilder
            .AppendLine()
            .Append(entityTypeBuilderName)
            .Append(".ToTable(");

        var schema = (string?)schemaAnnotation?.Value ?? table?.Schema;
        if (tableName == null
            && (schemaAnnotation == null || schema == null))
        {
            stringBuilder.Append("(string)");
        }

        stringBuilder.Append(Code.Literal(tableName));

        annotations.TryGetAndRemove(RelationalAnnotationNames.IsTableExcludedFromMigrations, out IAnnotation isExcludedAnnotation);
        var isExcludedFromMigrations = (isExcludedAnnotation?.Value as bool?) == true;
        if (isExcludedAnnotation is not null)
        {
            annotations.Remove(isExcludedAnnotation.Name);
        }

        var hasTriggers = entityType.GetTriggers().Any(t => t.TableName == tableName! && t.TableSchema == schema);
        var hasOverrides = table != null
            && entityType.GetProperties().Select(p => p.FindOverrides(table.Value)).Any(o => o != null);
        var requiresTableBuilder = isExcludedFromMigrations
            || hasTriggers
            || hasOverrides;

        if (schema != null
            || (schemaAnnotation != null && tableName != null))
        {
            stringBuilder
                .Append(", ");

            if (schema == null && !requiresTableBuilder)
            {
                stringBuilder.Append("(string)");
            }

            stringBuilder.Append(Code.Literal(schema));
        }

        if (requiresTableBuilder)
        {
            using (stringBuilder.Indent())
            {
                stringBuilder
                .AppendLine(", t =>")
                .Append("{");

                using (stringBuilder.Indent())
                {
                    if (isExcludedFromMigrations)
                    {
                        stringBuilder
                            .AppendLine()
                            .AppendLine("t.ExcludeFromMigrations();");
                    }

                    if (hasTriggers)
                    {
                        GenerateTriggers("t", entityType, tableName!, schema, stringBuilder);
                    }

                    if (hasOverrides)
                    {
                        GeneratePropertyOverrides("t", entityType, table!.Value, stringBuilder);
                    }
                }

                stringBuilder
                    .Append("}");
            }
        }

        stringBuilder.AppendLine(");");
    }

    private void GenerateSplitTableMapping(
        string entityTypeBuilderName,
        IEntityType entityType,
        IndentedStringBuilder stringBuilder)
    {
        foreach (var fragment in entityType.GetMappingFragments(StoreObjectType.Table))
        {
            var table = fragment.StoreObject;
            stringBuilder
                .AppendLine()
                .Append(entityTypeBuilderName)
                .Append(".SplitToTable(")
                .Append(Code.Literal(table.Name))
                .Append(", ")
                .Append(Code.Literal(table.Schema))
                .AppendLine(", t =>");

            using (stringBuilder.Indent())
            {
                stringBuilder.Append("{");

                using (stringBuilder.Indent())
                {
                    GenerateTriggers("t", entityType, table.Name, table.Schema, stringBuilder);
                    GeneratePropertyOverrides("t", entityType, table, stringBuilder);
                    GenerateEntityTypeMappingFragmentAnnotations("t", fragment, stringBuilder);
                }

                stringBuilder
                    .Append("}")
                    .AppendLine(");");
            }
        }
    }

    private void GenerateViewMapping(string entityTypeBuilderName, IEntityType entityType, IndentedStringBuilder stringBuilder, Dictionary<string, IAnnotation> annotations)
    {
        annotations.TryGetAndRemove(RelationalAnnotationNames.ViewName, out IAnnotation viewNameAnnotation);
        annotations.TryGetAndRemove(RelationalAnnotationNames.ViewSchema, out IAnnotation viewSchemaAnnotation);
        annotations.Remove(RelationalAnnotationNames.ViewDefinitionSql);

        var view = StoreObjectIdentifier.Create(entityType, StoreObjectType.View);
        var viewName = (string?)viewNameAnnotation?.Value ?? view?.Name;
        if (viewNameAnnotation == null
            && (viewName == null ||
                entityType.BaseType?.GetViewName() == viewName))
        {
            return;
        }

        stringBuilder
            .AppendLine()
            .Append(entityTypeBuilderName)
            .Append(".ToView(")
            .Append(Code.Literal(viewName));
        if (viewNameAnnotation != null)
        {
            annotations.Remove(viewNameAnnotation.Name);
        }

        var hasOverrides = view != null
            && entityType.GetProperties().Select(p => p.FindOverrides(view.Value)).Any(o => o != null);

        var schema = (string?)viewSchemaAnnotation?.Value ?? view?.Schema;
        if (schema != null
            || viewSchemaAnnotation != null)
        {
            stringBuilder
                .Append(", ");

            if (schema == null && !hasOverrides)
            {
                stringBuilder.Append("(string)");
            }

            stringBuilder.Append(Code.Literal(schema));
        }

        if (hasOverrides)
        {
            stringBuilder.AppendLine(", v =>");

            using (stringBuilder.Indent())
            {
                stringBuilder.Append("{");

                using (stringBuilder.Indent())
                {
                    GeneratePropertyOverrides("v", entityType, view!.Value, stringBuilder);
                }

                stringBuilder.Append("}");
            }
        }

        stringBuilder.AppendLine(");");
    }

    private void GenerateSplitViewMapping(
        string entityTypeBuilderName,
        IEntityType entityType,
        IndentedStringBuilder stringBuilder)
    {
        foreach (var fragment in entityType.GetMappingFragments(StoreObjectType.View))
        {
            stringBuilder
                .AppendLine()
                .Append(entityTypeBuilderName)
                .Append(".SplitToView(")
                .Append(Code.Literal(fragment.StoreObject.Name))
                .Append(", ")
                .Append(Code.Literal(fragment.StoreObject.Schema))
                .AppendLine(", v =>");

            using (stringBuilder.Indent())
            {
                stringBuilder.Append("{");

                using (stringBuilder.Indent())
                {
                    GeneratePropertyOverrides("v", entityType, fragment.StoreObject, stringBuilder);
                    GenerateEntityTypeMappingFragmentAnnotations("v", fragment, stringBuilder);
                }

                stringBuilder
                    .Append("}")
                    .AppendLine(");");
            }
        }
    }

    /// <summary>
    ///     Generates code for mapping fragment annotations.
    /// </summary>
    /// <param name="tableBuilderName">The name of the table builder variable.</param>
    /// <param name="fragment">The mapping fragment.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateEntityTypeMappingFragmentAnnotations(
        string tableBuilderName,
        IEntityTypeMappingFragment fragment,
        IndentedStringBuilder stringBuilder)
    {
        var annotations = Dependencies.AnnotationCodeGenerator
            .FilterIgnoredAnnotations(fragment.GetAnnotations())
            .ToDictionary(a => a.Name, a => a);
        if (annotations.Count > 0)
        {
            GenerateAnnotations(tableBuilderName, fragment, stringBuilder, annotations, inChainedCall: false);
        }
    }

    /// <summary>
    ///     Generates code for <see cref="ICheckConstraint" /> objects.
    /// </summary>
    /// <param name="entityTypeBuilderName">The name of the builder variable.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateCheckConstraints(
        string entityTypeBuilderName,
        IEntityType entityType,
        IndentedStringBuilder stringBuilder)
    {
        var constraintsForEntity = entityType.GetCheckConstraints();

        foreach (var checkConstraint in constraintsForEntity)
        {
            stringBuilder.AppendLine();

            GenerateCheckConstraint(entityTypeBuilderName, checkConstraint, stringBuilder);
        }
    }

    /// <summary>
    ///     Generates code for an <see cref="ICheckConstraint" />.
    /// </summary>
    /// <param name="entityTypeBuilderName">The name of the builder variable.</param>
    /// <param name="checkConstraint">The check constraint.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateCheckConstraint(
        string entityTypeBuilderName,
        ICheckConstraint checkConstraint,
        IndentedStringBuilder stringBuilder)
    {
        stringBuilder
            .Append(entityTypeBuilderName)
            .Append(".HasCheckConstraint(")
            .Append(Code.Literal(checkConstraint.ModelName))
            .Append(", ")
            .Append(Code.Literal(checkConstraint.Sql));

        GenerateCheckConstraintAnnotations(checkConstraint, stringBuilder);
        stringBuilder.AppendLine(");");
    }

    /// <summary>
    ///     Generates code for check constraint annotations.
    /// </summary>
    /// <param name="checkConstraint">The check constraint.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateCheckConstraintAnnotations(
        ICheckConstraint checkConstraint,
        IndentedStringBuilder stringBuilder)
    {
        var hasNonDefaultName = checkConstraint.Name != null
                && checkConstraint.Name != (checkConstraint.GetDefaultName() ?? checkConstraint.ModelName);
        var annotations = Dependencies.AnnotationCodeGenerator
            .FilterIgnoredAnnotations(checkConstraint.GetAnnotations())
            .ToDictionary(a => a.Name, a => a);
        if (annotations.Count > 0
            || hasNonDefaultName)
        {
            if (annotations.Count > 0)
            {
                stringBuilder
                    .Append(", c =>")
                    .AppendLine()
                    .IncrementIndent()
                    .AppendLine("{")
                    .IncrementIndent();
            }
            else
            {
                stringBuilder.Append(", c => ");
            }

            if (hasNonDefaultName)
            {
                stringBuilder
                    .Append("c.HasName(")
                    .Append(Code.Literal(checkConstraint.Name!))
                    .Append(")");
            }

            if (annotations.Count > 0)
            {
                if (hasNonDefaultName)
                {
                    stringBuilder.AppendLine(";");
                }

                GenerateAnnotations("c", checkConstraint, stringBuilder, annotations, inChainedCall: false);
                stringBuilder
                    .DecrementIndent()
                    .Append("}")
                    .DecrementIndent();
            }
        }
    }

    /// <summary>
    ///     Generates code for <see cref="ITrigger" /> objects.
    /// </summary>
    /// <param name="tableBuilderName">The name of the table builder variable.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="table">The table name.</param>
    /// <param name="schema">The table schema.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateTriggers(
        string tableBuilderName,
        IEntityType entityType,
        string table,
        string? schema,
        IndentedStringBuilder stringBuilder)
    {
        foreach (var trigger in entityType.GetTriggers())
        {
            if (trigger.TableName != table || trigger.TableSchema != schema)
            {
                continue;
            }

            GenerateTrigger(tableBuilderName, trigger, stringBuilder);
        }
    }

    /// <summary>
    ///     Generates code for an <see cref="ITrigger" />.
    /// </summary>
    /// <param name="tableBuilderName">The name of the table builder variable.</param>
    /// <param name="trigger">The trigger.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateTrigger(
        string tableBuilderName,
        ITrigger trigger,
        IndentedStringBuilder stringBuilder)
    {
        var triggerBuilderName = $"{tableBuilderName}.HasTrigger({Code.Literal(trigger.ModelName)})";
        stringBuilder
            .AppendLine()
            .Append(triggerBuilderName);

        // Note that GenerateAnnotations below does the corresponding decrement
        stringBuilder.IncrementIndent();

        if (trigger.Name != trigger.GetDefaultName()!)
        {
            stringBuilder
                .AppendLine()
                .Append(".HasName(")
                .Append(Code.Literal(trigger.Name))
                .Append(")");
        }

        GenerateTriggerAnnotations(triggerBuilderName, trigger, stringBuilder);
    }

    /// <summary>
    ///     Generates code for trigger annotations.
    /// </summary>
    /// <param name="triggerBuilderName">The name of the builder variable.</param>
    /// <param name="trigger">The trigger.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateTriggerAnnotations(
        string triggerBuilderName,
        ITrigger trigger,
        IndentedStringBuilder stringBuilder)
    {
        var annotations = Dependencies.AnnotationCodeGenerator
            .FilterIgnoredAnnotations(trigger.GetAnnotations())
            .ToDictionary(a => a.Name, a => a);

        GenerateAnnotations(triggerBuilderName, trigger, stringBuilder, annotations, inChainedCall: true);
    }

    /// <summary>
    ///     Generates code for <see cref="IRelationalPropertyOverrides" /> objects.
    /// </summary>
    /// <param name="tableBuilderName">The name of the table builder variable.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="storeObject">The store object identifier.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GeneratePropertyOverrides(
        string tableBuilderName,
        IEntityType entityType,
        StoreObjectIdentifier storeObject,
        IndentedStringBuilder stringBuilder)
    {
        foreach (var property in entityType.GetProperties())
        {
            var overrides = property.FindOverrides(storeObject);
            if (overrides != null)
            {
                GeneratePropertyOverride(tableBuilderName, overrides, stringBuilder);
            }
        }
    }

    /// <summary>
    ///     Generates code for an <see cref="IRelationalPropertyOverrides" />.
    /// </summary>
    /// <param name="tableBuilderName">The name of the table builder variable.</param>
    /// <param name="overrides">The overrides.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GeneratePropertyOverride(
        string tableBuilderName,
        IRelationalPropertyOverrides overrides,
        IndentedStringBuilder stringBuilder)
    {
        var propertyBuilderName = $"{tableBuilderName}.Property({Code.Literal(overrides.Property.Name)})";
        stringBuilder
            .AppendLine()
            .Append(propertyBuilderName);

        // Note that GenerateAnnotations below does the corresponding decrement
        stringBuilder.IncrementIndent();

        if (overrides.IsColumnNameOverridden)
        {
            stringBuilder
                .AppendLine()
                .Append(".")
                .Append(nameof(ColumnBuilder.HasColumnName))
                .Append("(")
                .Append(Code.Literal(overrides.ColumnName))
                .Append(")");
        }

        GeneratePropertyOverridesAnnotations(propertyBuilderName, overrides, stringBuilder);
    }

    /// <summary>
    ///     Generates code for property overrides annotations.
    /// </summary>
    /// <param name="propertyBuilderName">The name of the builder variable.</param>
    /// <param name="overrides">The overrides.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GeneratePropertyOverridesAnnotations(
        string propertyBuilderName,
        IRelationalPropertyOverrides overrides,
        IndentedStringBuilder stringBuilder)
    {
        var annotations = Dependencies.AnnotationCodeGenerator
            .FilterIgnoredAnnotations(overrides.GetAnnotations())
            .ToDictionary(a => a.Name, a => a);
        GenerateAnnotations(propertyBuilderName, overrides, stringBuilder, annotations, inChainedCall: true);
    }

    /// <summary>
    ///     Generates code for <see cref="IForeignKey" /> objects.
    /// </summary>
    /// <param name="entityTypeBuilderName">The name of the builder variable.</param>
    /// <param name="foreignKeys">The foreign keys.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateForeignKeys(
        string entityTypeBuilderName,
        IEnumerable<IForeignKey> foreignKeys,
        IndentedStringBuilder stringBuilder)
    {
        foreach (var foreignKey in foreignKeys)
        {
            stringBuilder.AppendLine();

            GenerateForeignKey(entityTypeBuilderName, foreignKey, stringBuilder);
        }
    }

    /// <summary>
    ///     Generates code for an <see cref="IForeignKey" />.
    /// </summary>
    /// <param name="entityTypeBuilderName">The name of the builder variable.</param>
    /// <param name="foreignKey">The foreign key.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateForeignKey(
        string entityTypeBuilderName,
        IForeignKey foreignKey,
        IndentedStringBuilder stringBuilder)
    {
        var foreignKeyBuilderNameStringBuilder = new StringBuilder();

        if (!foreignKey.IsOwnership)
        {
            foreignKeyBuilderNameStringBuilder
                .Append(entityTypeBuilderName)
                .Append(".HasOne(")
                .Append(Code.Literal(GetFullName(foreignKey.PrincipalEntityType)))
                .Append(", ")
                .Append(Code.Literal(foreignKey.DependentToPrincipal?.Name));
        }
        else
        {
            foreignKeyBuilderNameStringBuilder
                .Append(entityTypeBuilderName)
                .Append(".WithOwner(");

            if (foreignKey.DependentToPrincipal != null)
            {
                foreignKeyBuilderNameStringBuilder
                    .Append(Code.Literal(foreignKey.DependentToPrincipal.Name));
            }
        }

        foreignKeyBuilderNameStringBuilder.Append(')');

        var foreignKeyBuilderName = foreignKeyBuilderNameStringBuilder.ToString();

        stringBuilder
            .Append(foreignKeyBuilderName)
            .AppendLine();

        // Note that GenerateAnnotations below does the corresponding decrement
        stringBuilder.IncrementIndent();

        if (foreignKey.IsUnique
            && !foreignKey.IsOwnership)
        {
            stringBuilder
                .Append(".WithOne(");

            if (foreignKey.PrincipalToDependent != null)
            {
                stringBuilder
                    .Append(Code.Literal(foreignKey.PrincipalToDependent.Name));
            }

            stringBuilder
                .AppendLine(")")
                .Append(".HasForeignKey(")
                .Append(Code.Literal(GetFullName(foreignKey.DeclaringEntityType)))
                .Append(", ")
                .Append(string.Join(", ", foreignKey.Properties.Select(p => Code.Literal(p.Name))))
                .Append(")");

            if (foreignKey.PrincipalKey != foreignKey.PrincipalEntityType.FindPrimaryKey())
            {
                stringBuilder
                    .AppendLine()
                    .Append(".HasPrincipalKey(")
                    .Append(Code.Literal(GetFullName(foreignKey.PrincipalEntityType)))
                    .Append(", ")
                    .Append(string.Join(", ", foreignKey.PrincipalKey.Properties.Select(p => Code.Literal(p.Name))))
                    .Append(")");
            }
        }
        else
        {
            if (!foreignKey.IsOwnership)
            {
                stringBuilder
                    .Append(".WithMany(");

                if (foreignKey.PrincipalToDependent != null)
                {
                    stringBuilder
                        .Append(Code.Literal(foreignKey.PrincipalToDependent.Name));
                }

                stringBuilder
                    .AppendLine(")");
            }

            stringBuilder
                .Append(".HasForeignKey(")
                .Append(string.Join(", ", foreignKey.Properties.Select(p => Code.Literal(p.Name))))
                .Append(")");

            if (foreignKey.PrincipalKey != foreignKey.PrincipalEntityType.FindPrimaryKey())
            {
                stringBuilder
                    .AppendLine()
                    .Append(".HasPrincipalKey(")
                    .Append(string.Join(", ", foreignKey.PrincipalKey.Properties.Select(p => Code.Literal(p.Name))))
                    .Append(")");
            }
        }

        if (!foreignKey.IsOwnership)
        {
            if (foreignKey.DeleteBehavior != DeleteBehavior.ClientSetNull)
            {
                stringBuilder
                    .AppendLine()
                    .Append(".OnDelete(")
                    .Append(Code.Literal(foreignKey.DeleteBehavior))
                    .Append(")");
            }

            if (foreignKey.IsRequired)
            {
                stringBuilder
                    .AppendLine()
                    .Append(".IsRequired()");
            }
        }

        GenerateForeignKeyAnnotations(foreignKeyBuilderName, foreignKey, stringBuilder);
    }

    /// <summary>
    ///     Generates code for the annotations on a foreign key.
    /// </summary>
    /// <param name="foreignKeyBuilderName">The name of the builder variable.</param>
    /// <param name="foreignKey">The foreign key.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateForeignKeyAnnotations(
        string foreignKeyBuilderName,
        IForeignKey foreignKey,
        IndentedStringBuilder stringBuilder)
    {
        var annotations = Dependencies.AnnotationCodeGenerator
            .FilterIgnoredAnnotations(foreignKey.GetAnnotations())
            .ToDictionary(a => a.Name, a => a);

        GenerateAnnotations(foreignKeyBuilderName, foreignKey, stringBuilder, annotations, inChainedCall: true);
    }

    /// <summary>
    ///     Generates code for the navigations of an <see cref="IEntityType" />.
    /// </summary>
    /// <param name="modelBuilderName">The name of the builder variable.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateEntityTypeNavigations(
        string modelBuilderName,
        IEntityType entityType,
        IndentedStringBuilder stringBuilder)
    {
        stringBuilder
            .Append(modelBuilderName)
            .Append(".Entity(")
            .Append(Code.Literal(entityType.Name))
            .AppendLine(", b =>");

        using (stringBuilder.Indent())
        {
            stringBuilder.Append("{");

            using (stringBuilder.Indent())
            {
                GenerateNavigations(
                    "b", entityType.GetDeclaredNavigations()
                        .Where(n => !n.IsOnDependent && !n.ForeignKey.IsOwnership), stringBuilder);
            }

            stringBuilder.AppendLine("});");
        }
    }

    /// <summary>
    ///     Generates code for <see cref="INavigation" /> objects.
    /// </summary>
    /// <param name="entityTypeBuilderName">The name of the builder variable.</param>
    /// <param name="navigations">The navigations.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateNavigations(
        string entityTypeBuilderName,
        IEnumerable<INavigation> navigations,
        IndentedStringBuilder stringBuilder)
    {
        foreach (var navigation in navigations)
        {
            stringBuilder.AppendLine();

            GenerateNavigation(entityTypeBuilderName, navigation, stringBuilder);
        }
    }

    /// <summary>
    ///     Generates code for an <see cref="INavigation" />.
    /// </summary>
    /// <param name="entityTypeBuilderName">The name of the builder variable.</param>
    /// <param name="navigation">The navigation.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateNavigation(
        string entityTypeBuilderName,
        INavigation navigation,
        IndentedStringBuilder stringBuilder)
    {
        var navigationBuilderName = $"{entityTypeBuilderName}.Navigation({Code.Literal(navigation.Name)})";

        stringBuilder.Append(navigationBuilderName);

        // Note that GenerateAnnotations below does the corresponding decrement
        stringBuilder.IncrementIndent();

        if (!navigation.IsOnDependent
            && !navigation.IsCollection
            && navigation.ForeignKey.IsRequiredDependent)
        {
            stringBuilder
                .AppendLine()
                .Append(".IsRequired()");
        }

        GenerateNavigationAnnotations(navigationBuilderName, navigation, stringBuilder);
    }

    /// <summary>
    ///     Generates code for the annotations on a navigation.
    /// </summary>
    /// <param name="navigationBuilderName">The name of the builder variable.</param>
    /// <param name="navigation">The navigation.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateNavigationAnnotations(
        string navigationBuilderName,
        INavigation navigation,
        IndentedStringBuilder stringBuilder)
    {
        var annotations = Dependencies.AnnotationCodeGenerator
            .FilterIgnoredAnnotations(navigation.GetAnnotations())
            .ToDictionary(a => a.Name, a => a);

        GenerateAnnotations(navigationBuilderName, navigation, stringBuilder, annotations, inChainedCall: true);
    }

    /// <summary>
    ///     Generates code for data seeding.
    /// </summary>
    /// <param name="entityTypeBuilderName">The name of the builder variable.</param>
    /// <param name="properties">The properties to generate.</param>
    /// <param name="data">The data to be seeded.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    protected virtual void GenerateData(
        string entityTypeBuilderName,
        IEnumerable<IProperty> properties,
        IEnumerable<IDictionary<string, object?>> data,
        IndentedStringBuilder stringBuilder)
    {
        var dataList = data.ToList();
        if (dataList.Count == 0)
        {
            return;
        }

        var propertiesToOutput = properties.ToList();

        stringBuilder
            .AppendLine()
            .Append(entityTypeBuilderName)
            .Append(".")
            .Append(nameof(EntityTypeBuilder.HasData))
            .AppendLine("(");

        using (stringBuilder.Indent())
        {
            var firstDatum = true;
            foreach (var o in dataList)
            {
                if (!firstDatum)
                {
                    stringBuilder.AppendLine(",");
                }
                else
                {
                    firstDatum = false;
                }

                stringBuilder
                    .AppendLine("new")
                    .AppendLine("{");

                using (stringBuilder.Indent())
                {
                    var firstProperty = true;
                    foreach (var property in propertiesToOutput)
                    {
                        if (o.TryGetValue(property.Name, out var value)
                            && value != null)
                        {
                            if (!firstProperty)
                            {
                                stringBuilder.AppendLine(",");
                            }
                            else
                            {
                                firstProperty = false;
                            }

                            stringBuilder
                                .Append(Code.Identifier(property.Name))
                                .Append(" = ")
                                .Append(Code.UnknownLiteral(value));
                        }
                    }

                    stringBuilder.AppendLine();
                }

                stringBuilder.Append("}");
            }
        }

        stringBuilder
            .AppendLine(");");
    }

    private void GenerateFluentApiForMaxLength(
        IProperty property,
        IndentedStringBuilder stringBuilder)
    {
        if (property.GetMaxLength() is int maxLength)
        {
            stringBuilder
                .AppendLine()
                .Append(".")
                .Append(nameof(PropertyBuilder.HasMaxLength))
                .Append("(")
                .Append(Code.Literal(maxLength))
                .Append(")");
        }
    }

    private void GenerateFluentApiForPrecisionAndScale(
        IProperty property,
        IndentedStringBuilder stringBuilder)
    {
        if (property.GetPrecision() is int precision)
        {
            stringBuilder
                .AppendLine()
                .Append(".")
                .Append(nameof(PropertyBuilder.HasPrecision))
                .Append("(")
                .Append(Code.Literal(precision));

            if (property.GetScale() is int scale)
            {
                if (scale != 0)
                {
                    stringBuilder
                        .Append(", ")
                        .Append(Code.Literal(scale));
                }
            }

            stringBuilder.Append(")");
        }
    }

    private void GenerateFluentApiForIsUnicode(
        IProperty property,
        IndentedStringBuilder stringBuilder)
    {
        if (property.IsUnicode() is bool unicode)
        {
            stringBuilder
                .AppendLine()
                .Append(".")
                .Append(nameof(PropertyBuilder.IsUnicode))
                .Append("(")
                .Append(Code.Literal(unicode))
                .Append(")");
        }
    }

    private void GenerateAnnotations(
        string builderName,
        IAnnotatable annotatable,
        IndentedStringBuilder stringBuilder,
        Dictionary<string, IAnnotation> annotations,
        bool inChainedCall,
        bool leadingNewline = true)
    {
        var fluentApiCalls = Dependencies.AnnotationCodeGenerator.GenerateFluentApiCalls(annotatable, annotations);

        MethodCallCodeFragment? chainedCall = null;
        var typeQualifiedCalls = new List<MethodCallCodeFragment>();

        // Chain together all Fluent API calls which we can, and leave the others to be generated as type-qualified
        foreach (var call in fluentApiCalls)
        {
            if (call.MethodInfo is not null
                && call.MethodInfo.IsStatic
                && (call.MethodInfo.DeclaringType is null
                    || call.MethodInfo.DeclaringType.Assembly != typeof(RelationalModelBuilderExtensions).Assembly))
            {
                typeQualifiedCalls.Add(call);
            }
            else
            {
                chainedCall = chainedCall is null ? call : chainedCall.Chain(call);
            }
        }

        // Append remaining raw annotations which did not get generated as Fluent API calls
        foreach (var annotation in annotations.Values.OrderBy(a => a.Name))
        {
            var call = new MethodCallCodeFragment(HasAnnotationMethodInfo, annotation.Name, annotation.Value);
            chainedCall = chainedCall is null ? call : chainedCall.Chain(call);
        }

        // First generate single Fluent API call chain
        if (chainedCall is not null)
        {
            if (inChainedCall)
            {
                if (chainedCall.ChainedCall is null)
                {
                    stringBuilder.AppendLine();
                }

                stringBuilder.Append(Code.Fragment(chainedCall, stringBuilder.CurrentIndent));
            }
            else
            {
                if (leadingNewline)
                {
                    stringBuilder.AppendLine();
                }

                stringBuilder.Append(builderName);
                stringBuilder.Append(Code.Fragment(chainedCall, stringBuilder.CurrentIndent + 1));
                stringBuilder.AppendLine(";");
            }

            leadingNewline = true;
        }

        if (inChainedCall)
        {
            stringBuilder.AppendLine(";");
            stringBuilder.DecrementIndent();
        }

        // Then generate separate fully-qualified calls
        if (typeQualifiedCalls.Count > 0)
        {
            if (leadingNewline)
            {
                stringBuilder.AppendLine();
            }

            foreach (var call in typeQualifiedCalls)
            {
                stringBuilder.Append(Code.Fragment(call, builderName, typeQualified: true));
                stringBuilder.AppendLine(";");
            }
        }
    }

    private static string GetFullName(IReadOnlyEntityType entityType)
    {
        var entityTypeName = entityType.Name;
        var ownership = entityType.FindOwnership();
        if (ownership == null)
        {
            return entityTypeName;
        }

        if (entityType.HasSharedClrType
            && entityTypeName == ownership!.PrincipalEntityType.GetOwnedName(
                entityType.ClrType.ShortDisplayName(), ownership.PrincipalToDependent!.Name))
        {
            entityTypeName = entityType.ClrType.DisplayName();
        }

        return GetFullName(ownership.PrincipalEntityType) + "."
                + ownership.PrincipalToDependent!.Name + "#" + entityTypeName;
    }
}
