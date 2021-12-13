// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
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
    private static readonly MethodInfo _hasAnnotationMethodInfo
        = typeof(ModelBuilder).GetRequiredRuntimeMethod(nameof(ModelBuilder.HasAnnotation), typeof(string), typeof(string));

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

        if (model.GetProductVersion() is string productVersion)
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
        foreach (var entityType in entityTypes.Where(
                     e => e.FindOwnership() == null))
        {
            stringBuilder.AppendLine();

            GenerateEntityType(modelBuilderName, entityType, stringBuilder);
        }

        foreach (var entityType in entityTypes.Where(
                     e => e.FindOwnership() == null
                         && (e.GetDeclaredForeignKeys().Any()
                             || e.GetDeclaredReferencingForeignKeys().Any(fk => fk.IsOwnership))))
        {
            stringBuilder.AppendLine();

            GenerateEntityTypeRelationships(modelBuilderName, entityType, stringBuilder);
        }

        foreach (var entityType in entityTypes.Where(
                     e => e.FindOwnership() == null
                         && e.GetDeclaredNavigations().Any(n => !n.IsOnDependent && !n.ForeignKey.IsOwnership)))
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
                .Append(Code.Literal(baseType.Name))
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
        stringBuilder
            .AppendLine()
            .Append(modelBuilderName)
            .Append(".HasSequence");

        if (sequence.Type != Sequence.DefaultClrType)
        {
            stringBuilder
                .Append("<")
                .Append(Code.Reference(sequence.Type))
                .Append(">");
        }

        stringBuilder
            .Append("(")
            .Append(Code.Literal(sequence.Name));

        if (!string.IsNullOrEmpty(sequence.Schema)
            && sequence.Model.GetDefaultSchema() != sequence.Schema)
        {
            stringBuilder
                .Append(", ")
                .Append(Code.Literal(sequence.Schema));
        }

        stringBuilder.Append(")");

        using (stringBuilder.Indent())
        {
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
        }

        stringBuilder.AppendLine(";");
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

        stringBuilder
            .AppendLine()
            .Append(".")
            .Append(nameof(RelationalPropertyBuilderExtensions.HasColumnType))
            .Append("(")
            .Append(
                Code.Literal(
                    property.GetColumnType()
                    ?? Dependencies.RelationalTypeMappingSource.GetMapping(property).StoreType))
            .Append(")");
        annotations.Remove(RelationalAnnotationNames.ColumnType);

        GenerateFluentApiForDefaultValue(property, stringBuilder);
        annotations.Remove(RelationalAnnotationNames.DefaultValue);

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
        var annotationList = entityType.GetAnnotations().ToList();

        var discriminatorPropertyAnnotation = annotationList.FirstOrDefault(a => a.Name == CoreAnnotationNames.DiscriminatorProperty);
        var discriminatorMappingCompleteAnnotation =
            annotationList.FirstOrDefault(a => a.Name == CoreAnnotationNames.DiscriminatorMappingComplete);
        var discriminatorValueAnnotation = annotationList.FirstOrDefault(a => a.Name == CoreAnnotationNames.DiscriminatorValue);

        var annotations = Dependencies.AnnotationCodeGenerator
            .FilterIgnoredAnnotations(entityType.GetAnnotations())
            .ToDictionary(a => a.Name, a => a);

        var tableNameAnnotation = annotations.Find(RelationalAnnotationNames.TableName);
        if (tableNameAnnotation?.Value != null
            || entityType.BaseType == null)
        {
            var tableName = (string?)tableNameAnnotation?.Value ?? entityType.GetTableName();
            if (tableName != null
                || tableNameAnnotation != null)
            {
                stringBuilder
                    .AppendLine()
                    .Append(entityTypeBuilderName)
                    .Append(".ToTable(");

                var schemaAnnotation = annotations.Find(RelationalAnnotationNames.Schema);
                var schema = (string?)schemaAnnotation?.Value ?? entityType.GetSchema();
                if (tableName == null
                    && (schemaAnnotation == null || schema == null))
                {
                    stringBuilder.Append("(string)");
                }

                stringBuilder.Append(Code.UnknownLiteral(tableName));

                if (tableNameAnnotation != null)
                {
                    annotations.Remove(tableNameAnnotation.Name);
                }

                var isExcludedAnnotation = annotations.Find(RelationalAnnotationNames.IsTableExcludedFromMigrations);
                if (schema != null
                    || (schemaAnnotation != null && tableName != null))
                {
                    stringBuilder
                        .Append(", ");

                    if (schema == null
                        && ((bool?)isExcludedAnnotation?.Value) != true)
                    {
                        stringBuilder.Append("(string)");
                    }

                    stringBuilder.Append(Code.UnknownLiteral(schema));
                }

                if (isExcludedAnnotation != null)
                {
                    if (((bool?)isExcludedAnnotation.Value) == true)
                    {
                        stringBuilder
                            .Append(", t => t.ExcludeFromMigrations()");
                    }

                    annotations.Remove(isExcludedAnnotation.Name);
                }

                stringBuilder.AppendLine(");");
            }
        }

        annotations.Remove(RelationalAnnotationNames.Schema);

        var viewNameAnnotation = annotations.Find(RelationalAnnotationNames.ViewName);
        if (viewNameAnnotation?.Value != null
            || entityType.BaseType == null)
        {
            var viewName = (string?)viewNameAnnotation?.Value ?? entityType.GetViewName();
            if (viewName != null
                || viewNameAnnotation != null)
            {
                stringBuilder
                    .AppendLine()
                    .Append(entityTypeBuilderName)
                    .Append(".ToView(")
                    .Append(Code.UnknownLiteral(viewName));
                if (viewNameAnnotation != null)
                {
                    annotations.Remove(viewNameAnnotation.Name);
                }

                var viewSchemaAnnotation = annotations.Find(RelationalAnnotationNames.ViewSchema);
                if (viewSchemaAnnotation?.Value != null)
                {
                    stringBuilder
                        .Append(", ")
                        .Append(Code.Literal((string)viewSchemaAnnotation.Value));
                    annotations.Remove(viewSchemaAnnotation.Name);
                }

                stringBuilder.AppendLine(");");
            }
        }

        annotations.Remove(RelationalAnnotationNames.ViewSchema);
        annotations.Remove(RelationalAnnotationNames.ViewDefinitionSql);

        var functionNameAnnotation = annotations.Find(RelationalAnnotationNames.FunctionName);
        if (functionNameAnnotation?.Value != null
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
                    .Append(Code.UnknownLiteral(functionName))
                    .AppendLine(");");
                if (functionNameAnnotation != null)
                {
                    annotations.Remove(functionNameAnnotation.Name);
                }
            }
        }

        var sqlQueryAnnotation = annotations.Find(RelationalAnnotationNames.SqlQuery);
        if (sqlQueryAnnotation?.Value != null
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
                    .Append(Code.UnknownLiteral(sqlQuery))
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
                var value = discriminatorMappingCompleteAnnotation.Value;

                stringBuilder
                    .Append(".")
                    .Append("IsComplete")
                    .Append("(")
                    .Append(Code.UnknownLiteral(value))
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

        if (checkConstraint.Name != (checkConstraint.GetDefaultName() ?? checkConstraint.ModelName))
        {
            stringBuilder
                .Append(", c => c.HasName(")
                .Append(Code.Literal(checkConstraint.Name))
                .Append(")");
        }

        stringBuilder.AppendLine(");");
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
                .Append(Code.Literal(foreignKey.PrincipalEntityType.Name))
                .Append(", ")
                .Append(
                    foreignKey.DependentToPrincipal == null
                        ? Code.UnknownLiteral(null)
                        : Code.Literal(foreignKey.DependentToPrincipal.Name));
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
                .Append(Code.Literal(foreignKey.DeclaringEntityType.Name))
                .Append(", ")
                .Append(string.Join(", ", foreignKey.Properties.Select(p => Code.Literal(p.Name))))
                .Append(")");

            if (foreignKey.PrincipalKey != foreignKey.PrincipalEntityType.FindPrimaryKey())
            {
                stringBuilder
                    .AppendLine()
                    .Append(".HasPrincipalKey(")
                    .Append(Code.Literal(foreignKey.PrincipalEntityType.Name))
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
                .Append(Code.UnknownLiteral(precision));

            if (property.GetScale() is int scale)
            {
                if (scale != 0)
                {
                    stringBuilder
                        .Append(", ")
                        .Append(Code.UnknownLiteral(scale));
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

    private void GenerateFluentApiForDefaultValue(
        IProperty property,
        IndentedStringBuilder stringBuilder)
    {
        if (!property.TryGetDefaultValue(out var defaultValue))
        {
            return;
        }

        stringBuilder
            .AppendLine()
            .Append(".")
            .Append(nameof(RelationalPropertyBuilderExtensions.HasDefaultValue))
            .Append("(");

        if (defaultValue != DBNull.Value)
        {
            stringBuilder
                .Append(
                    Code.UnknownLiteral(
                        FindValueConverter(property) is ValueConverter valueConverter
                            ? valueConverter.ConvertToProvider(defaultValue)
                            : defaultValue));
        }

        stringBuilder
            .Append(")");
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
            var call = new MethodCallCodeFragment(_hasAnnotationMethodInfo, annotation.Name, annotation.Value);
            chainedCall = chainedCall is null ? call : chainedCall.Chain(call);
        }

        // First generate single Fluent API call chain
        if (chainedCall is not null)
        {
            if (inChainedCall)
            {
                stringBuilder
                    .AppendLine()
                    .AppendLines(Code.Fragment(chainedCall), skipFinalNewline: true);
            }
            else
            {
                if (leadingNewline)
                {
                    stringBuilder.AppendLine();
                }

                stringBuilder.AppendLines(Code.Fragment(chainedCall, builderName), skipFinalNewline: true);
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
}
