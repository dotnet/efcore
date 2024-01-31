// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Design-time model extensions.
/// </summary>
public static class ScaffoldingModelExtensions
{
    /// <summary>
    ///     Check whether an entity type could be considered a many-to-many join entity type.
    /// </summary>
    /// <param name="entityType">The entity type to check.</param>
    /// <returns><see langword="true" /> if the entity type could be considered a join entity type.</returns>
    public static bool IsSimpleManyToManyJoinEntityType(this IEntityType entityType)
    {
        if (!entityType.GetNavigations().Any()
            && !entityType.GetSkipNavigations().Any())
        {
            var primaryKey = entityType.FindPrimaryKey();
            var properties = entityType.GetProperties().ToList();
            var foreignKeys = entityType.GetForeignKeys().ToList();
            var referencingForeignKeys = entityType.GetReferencingForeignKeys().ToList();
            if (primaryKey is { Properties.Count: > 1 }
                && referencingForeignKeys.Count == 0
                && foreignKeys.Count == 2
                && primaryKey.Properties.Count == properties.Count
                && foreignKeys[0].Properties.Count + foreignKeys[1].Properties.Count == properties.Count
                && !foreignKeys[0].Properties.Intersect(foreignKeys[1].Properties).Any()
                && foreignKeys[0].IsRequired
                && foreignKeys[1].IsRequired
                && !foreignKeys[0].IsUnique
                && !foreignKeys[1].IsUnique)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Gets a value indicating whether the specified skip navigation represents the left side of the relationship.
    /// </summary>
    /// <param name="skipNavigation">The skip navigation to check.</param>
    /// <returns><see langword="true" /> if it represents the left side.</returns>
    /// <remarks>
    ///     The designation of left and right is arbitrary but deterministic. This method exists primarily to avoid configuring the same
    ///     many-to-many relationship from both of its ends.
    /// </remarks>
    public static bool IsLeftNavigation(this ISkipNavigation skipNavigation)
        => skipNavigation.JoinEntityType.FindPrimaryKey()!.Properties[0].GetContainingForeignKeys().Single().PrincipalEntityType
            == skipNavigation.DeclaringEntityType;

    /// <summary>
    ///     Gets the name that should be used for the <see cref="DbSet{TEntity}" /> property on the <see cref="DbContext" /> class for this entity
    ///     type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The property name.</returns>
    public static string GetDbSetName(this IReadOnlyEntityType entityType)
        => (string?)entityType[ScaffoldingAnnotationNames.DbSetName]
            ?? entityType.GetTableName()
            ?? entityType.ShortName();

    /// <summary>
    ///     Gets a value indicating whether the key would be configured by conventions.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns><see langword="true" /> if the key would be configured by conventions.</returns>
    public static bool IsHandledByConvention(this IKey key)
        => key is IConventionKey conventionKey
            && conventionKey.Properties.SequenceEqual(
                KeyDiscoveryConvention.DiscoverKeyProperties(
                    conventionKey.DeclaringEntityType,
                    conventionKey.DeclaringEntityType.GetProperties()));

    /// <summary>
    ///     Gets value indicating whether this index can be entirely reperesented by a data annotation.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="annotationCodeGenerator">The provider's annotation code generator.</param>
    /// <returns><see langword="true" /> if this index can be reperesented by a data annotation.</returns>
    public static bool IsHandledByDataAnnotations(this IIndex index, IAnnotationCodeGenerator annotationCodeGenerator)
    {
        var indexAnnotations = annotationCodeGenerator.FilterIgnoredAnnotations(index.GetAnnotations())
            .ToDictionary(a => a.Name, a => a);
        annotationCodeGenerator.RemoveAnnotationsHandledByConventions(index, indexAnnotations);

        // Can be represented using IndexAttribute
        return indexAnnotations.Count == 0;
    }

    /// <summary>
    ///     Gets the data annotations to configure an entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="annotationCodeGenerator">The provider's annotation code generator.</param>
    /// <returns>The data annotations.</returns>
    public static IEnumerable<AttributeCodeFragment> GetDataAnnotations(
        this IEntityType entityType,
        IAnnotationCodeGenerator annotationCodeGenerator)
    {
        var primaryKey = entityType.FindPrimaryKey();
        if (primaryKey == null)
        {
            yield return new AttributeCodeFragment(typeof(KeylessAttribute));
        }
        else if (primaryKey.Properties.Count > 1)
        {
            yield return new AttributeCodeFragment(
                typeof(PrimaryKeyAttribute),
                primaryKey.Properties.Select(p => p.Name).Cast<object?>().ToArray());
        }

        var tableName = entityType.GetTableName();
        var schema = entityType.GetSchema();
        var needsSchema = schema != null && schema != entityType.Model.GetDefaultSchema();

        if (entityType.GetViewName() == null
            && (tableName != null && tableName != entityType.GetDbSetName()
                || needsSchema))
        {
            var tableNamedArgs = new Dictionary<string, object?>();
            if (needsSchema)
            {
                tableNamedArgs.Add(nameof(TableAttribute.Schema), schema);
            }

            yield return new AttributeCodeFragment(typeof(TableAttribute), new object?[] { tableName }, tableNamedArgs);
        }

        foreach (var index in entityType.GetIndexes()
                     .Where(
                         i => ((IConventionIndex)i).GetConfigurationSource() != ConfigurationSource.Convention
                             && i.IsHandledByDataAnnotations(annotationCodeGenerator)))
        {
            var indexArgs = new List<object?>();
            var indexNamedArgs = new Dictionary<string, object?>();

            indexArgs.AddRange(index.Properties.Select(p => p.Name));

            if (index.Name != null)
            {
                indexNamedArgs.Add(nameof(IndexAttribute.Name), index.Name);
            }

            if (index.IsUnique)
            {
                indexNamedArgs.Add(nameof(IndexAttribute.IsUnique), true);
            }

            if (index.IsDescending is not null)
            {
                if (index.IsDescending.Count == 0)
                {
                    indexNamedArgs.Add(nameof(IndexAttribute.AllDescending), true);
                }
                else
                {
                    indexNamedArgs.Add(nameof(IndexAttribute.IsDescending), index.IsDescending);
                }
            }

            yield return new AttributeCodeFragment(typeof(IndexAttribute), indexArgs, indexNamedArgs);
        }

        var annotations = annotationCodeGenerator.FilterIgnoredAnnotations(entityType.GetAnnotations())
            .ToDictionary(a => a.Name, a => a);
        annotationCodeGenerator.RemoveAnnotationsHandledByConventions(entityType, annotations);
        foreach (var attribute in annotationCodeGenerator.GenerateDataAnnotationAttributes(entityType, annotations))
        {
            yield return attribute;
        }
    }

    /// <summary>
    ///     Gets the data annotations to configure a property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="annotationCodeGenerator">The provider's annotation code generator.</param>
    /// <returns>The data annotations.</returns>
    public static IEnumerable<AttributeCodeFragment> GetDataAnnotations(
        this IProperty property,
        IAnnotationCodeGenerator annotationCodeGenerator)
    {
        if (property.FindContainingPrimaryKey() != null)
        {
            yield return new AttributeCodeFragment(typeof(KeyAttribute));
        }

        if (!property.IsNullable
            && property.ClrType.IsNullableType()
            && !property.IsPrimaryKey())
        {
            yield return new AttributeCodeFragment(typeof(RequiredAttribute));
        }

        var columnName = property.GetColumnName();
        if (columnName == property.Name)
        {
            columnName = null;
        }

        var columnType = property.GetConfiguredColumnType();

        if (columnName != null
            || columnType != null)
        {
            var columnArgs = new List<object?>();
            var columnNamedArgs = new Dictionary<string, object?>();

            if (columnName != null)
            {
                columnArgs.Add(columnName);
            }

            if (columnType != null)
            {
                columnNamedArgs.Add(nameof(ColumnAttribute.TypeName), columnType);
            }

            yield return new AttributeCodeFragment(typeof(ColumnAttribute), columnArgs, columnNamedArgs);
        }

        var maxLength = property.GetMaxLength();
        if (maxLength.HasValue)
        {
            yield return new AttributeCodeFragment(
                property.ClrType == typeof(string)
                    ? typeof(StringLengthAttribute)
                    : typeof(MaxLengthAttribute),
                maxLength.Value);
        }

        if (property.ClrType == typeof(string))
        {
            var unicode = property.IsUnicode();
            if (unicode.HasValue)
            {
                var unicodeArgs = new List<object?>();

                if (!unicode.Value)
                {
                    unicodeArgs.Add(false);
                }

                yield return new AttributeCodeFragment(typeof(UnicodeAttribute), unicodeArgs.ToArray());
            }
        }

        var precision = property.GetPrecision();
        if (precision.HasValue)
        {
            var precisionArgs = new List<object?> { precision.Value };

            var scale = property.GetScale();
            if (scale.HasValue)
            {
                precisionArgs.Add(scale.Value);
            }

            yield return new AttributeCodeFragment(typeof(PrecisionAttribute), precisionArgs.ToArray());
        }

        var annotations = annotationCodeGenerator.FilterIgnoredAnnotations(property.GetAnnotations())
            .ToDictionary(a => a.Name, a => a);
        annotationCodeGenerator.RemoveAnnotationsHandledByConventions(property, annotations);
        foreach (var attribute in annotationCodeGenerator.GenerateDataAnnotationAttributes(property, annotations))
        {
            yield return attribute;
        }
    }

    /// <summary>
    ///     Gets the data annotations to configure a navigation property.
    /// </summary>
    /// <param name="navigation">The navigation property.</param>
    /// <param name="annotationCodeGenerator">The provider's annotation code generator.</param>
    /// <returns>The data annotations.</returns>
    public static IEnumerable<AttributeCodeFragment> GetDataAnnotations(
        this INavigation navigation,
        IAnnotationCodeGenerator annotationCodeGenerator)
    {
        if (navigation.IsOnDependent)
        {
            yield return new AttributeCodeFragment(
                typeof(ForeignKeyAttribute),
                string.Join(", ", navigation.ForeignKey.Properties.Select(p => p.Name)));
        }

        if (navigation.Inverse != null)
        {
            yield return new AttributeCodeFragment(typeof(InversePropertyAttribute), navigation.Inverse.Name);
        }
    }

    /// <summary>
    ///     Gets the data annotations to configure a skip navigation property.
    /// </summary>
    /// <param name="skipNavigation">The skip navigation property.</param>
    /// <param name="annotationCodeGenerator">The provider's annotation code generator.</param>
    /// <returns>The data annotations.</returns>
    public static IEnumerable<AttributeCodeFragment> GetDataAnnotations(
        this ISkipNavigation skipNavigation,
        IAnnotationCodeGenerator annotationCodeGenerator)
    {
        yield return new AttributeCodeFragment(
            typeof(ForeignKeyAttribute),
            string.Join(", ", skipNavigation.ForeignKey.Properties.Select(p => p.Name)));

        yield return new AttributeCodeFragment(typeof(InversePropertyAttribute), skipNavigation.Inverse.Name);
    }

    /// <summary>
    ///     Gets the fluent API calls to configure a model.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="annotationCodeGenerator">The provider's annotation code generator.</param>
    /// <returns>The fluent API calls.</returns>
    public static FluentApiCodeFragment? GetFluentApiCalls(this IModel model, IAnnotationCodeGenerator annotationCodeGenerator)
    {
        FluentApiCodeFragment? root = null;

        var annotations = annotationCodeGenerator.FilterIgnoredAnnotations(model.GetAnnotations())
            .ToDictionary(a => a.Name, a => a);
        annotationCodeGenerator.RemoveAnnotationsHandledByConventions(model, annotations);

        annotations.Remove(CoreAnnotationNames.ProductVersion);
        annotations.Remove(RelationalAnnotationNames.MaxIdentifierLength);
        annotations.Remove(ScaffoldingAnnotationNames.DatabaseName);

        var annotationsRoot = GenerateAnnotations(model, annotations, annotationCodeGenerator);
        if (annotationsRoot is not null)
        {
            root = root?.Chain(annotationsRoot) ?? annotationsRoot;
        }

        return root;
    }

    /// <summary>
    ///     Gets the fluent API calls to configure an entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="annotationCodeGenerator">The provider's annotation code generator.</param>
    /// <returns>The fluent API calls.</returns>
    public static FluentApiCodeFragment? GetFluentApiCalls(
        this IEntityType entityType,
        IAnnotationCodeGenerator annotationCodeGenerator)
    {
        FluentApiCodeFragment? root = null;

        var annotations = annotationCodeGenerator.FilterIgnoredAnnotations(entityType.GetAnnotations())
            .ToDictionary(a => a.Name, a => a);
        annotationCodeGenerator.RemoveAnnotationsHandledByConventions(entityType, annotations);

        annotations.Remove(RelationalAnnotationNames.TableName);
        annotations.Remove(RelationalAnnotationNames.Schema);
        annotations.Remove(RelationalAnnotationNames.Comment);
        annotations.Remove(RelationalAnnotationNames.ViewName);
        annotations.Remove(RelationalAnnotationNames.ViewSchema);
        annotations.Remove(RelationalAnnotationNames.ViewDefinitionSql);
        annotations.Remove(ScaffoldingAnnotationNames.DbSetName);

        var annotationsHandledByDataAnnotations = new Dictionary<string, IAnnotation>(annotations);

        // Strip out any annotations handled by data annotations
        _ = annotationCodeGenerator.GenerateDataAnnotationAttributes(entityType, annotations);

        foreach (var key in annotations.Keys)
        {
            annotationsHandledByDataAnnotations.Remove(key);
        }

        if (entityType.FindPrimaryKey() is null)
        {
            var hasNoKey = new FluentApiCodeFragment(nameof(EntityTypeBuilder.HasNoKey)) { IsHandledByDataAnnotations = true };

            root = root?.Chain(hasNoKey) ?? hasNoKey;
        }

        var tableName = entityType.GetTableName();
        var schema = entityType.GetSchema();
        var defaultSchema = entityType.Model.GetDefaultSchema();
        var explicitSchema = schema != null && schema != defaultSchema;

        var toTableHandledByConventions = true;
        var toTableHandledByDataAnnotations = true;

        var toTableArguments = new List<object?>();

        if (explicitSchema
            || tableName != null
            && (tableName != entityType.GetDbSetName()
                || (entityType.IsSimpleManyToManyJoinEntityType() && tableName != entityType.ShortName())))
        {
            toTableHandledByConventions = false;

            toTableArguments.Add(tableName);

            if (explicitSchema)
            {
                toTableArguments.Add(schema);
            }
        }

        var toTableNestedCalls = new List<MethodCallCodeFragment>();

        var comment = entityType.GetComment();
        if (comment != null)
        {
            toTableHandledByConventions = false;
            toTableHandledByDataAnnotations = false;

            toTableNestedCalls.Add(new MethodCallCodeFragment(nameof(TableBuilder.HasComment), comment));
        }

        if (entityType.GetDeclaredTriggers().Any())
        {
            toTableHandledByConventions = false;
            toTableHandledByDataAnnotations = false;

            foreach (var trigger in entityType.GetDeclaredTriggers())
            {
                toTableNestedCalls.Add(new MethodCallCodeFragment(nameof(TableBuilder.HasTrigger), trigger.ModelName));
            }
        }

        if (!toTableHandledByConventions)
        {
            if (toTableNestedCalls.Count != 0)
            {
                toTableArguments.Add(new NestedClosureCodeFragment("tb", toTableNestedCalls));
            }

            var toTable = new FluentApiCodeFragment(nameof(RelationalEntityTypeBuilderExtensions.ToTable))
            {
                Arguments = toTableArguments, IsHandledByDataAnnotations = toTableHandledByDataAnnotations
            };

            root = root?.Chain(toTable) ?? toTable;
        }

        var viewName = entityType.GetViewName();
        var viewSchema = entityType.GetViewSchema();
        var explicitViewSchema = viewSchema != null && viewSchema != defaultSchema;

        if (explicitViewSchema
            || viewName != null)
        {
            var toView = new FluentApiCodeFragment(nameof(RelationalEntityTypeBuilderExtensions.ToView)) { Arguments = { viewName } };

            if (explicitViewSchema)
            {
                toView.Arguments.Add(viewSchema);
            }

            root = root?.Chain(toView) ?? toView;
        }

        var annotationsRoot = GenerateAnnotations(entityType, annotations, annotationCodeGenerator);
        if (annotationsRoot is not null)
        {
            root = root?.Chain(annotationsRoot) ?? annotationsRoot;
        }

        annotationsRoot = GenerateAnnotations(
            entityType, annotationsHandledByDataAnnotations, annotationCodeGenerator, isHandledByDataAnnotations: true);
        if (annotationsRoot is not null)
        {
            root = root?.Chain(annotationsRoot) ?? annotationsRoot;
        }

        return root;
    }

    /// <summary>
    ///     Gets the fluent API calls to configure a key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="annotationCodeGenerator">The provider's annotation code generator.</param>
    /// <returns>The fluent API calls.</returns>
    public static FluentApiCodeFragment? GetFluentApiCalls(this IKey key, IAnnotationCodeGenerator annotationCodeGenerator)
    {
        FluentApiCodeFragment? root = null;

        var annotations = annotationCodeGenerator.FilterIgnoredAnnotations(key.GetAnnotations())
            .ToDictionary(a => a.Name, a => a);
        annotationCodeGenerator.RemoveAnnotationsHandledByConventions(key, annotations);

        var annotationsRoot = GenerateAnnotations(key, annotations, annotationCodeGenerator);
        if (annotationsRoot is not null)
        {
            root = root?.Chain(annotationsRoot) ?? annotationsRoot;
        }

        return root;
    }

    /// <summary>
    ///     Gets the fluent API calls to configure an index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="annotationCodeGenerator">The provider's annotation code generator.</param>
    /// <returns>The fluent API calls.</returns>
    public static FluentApiCodeFragment? GetFluentApiCalls(this IIndex index, IAnnotationCodeGenerator annotationCodeGenerator)
    {
        FluentApiCodeFragment? root = null;

        var annotations = annotationCodeGenerator.FilterIgnoredAnnotations(index.GetAnnotations())
            .ToDictionary(a => a.Name, a => a);
        annotationCodeGenerator.RemoveAnnotationsHandledByConventions(index, annotations);

        annotations.Remove(RelationalAnnotationNames.Name);

        if (index.IsUnique)
        {
            var isUnique = new FluentApiCodeFragment(nameof(IndexBuilder.IsUnique));

            root = root?.Chain(isUnique) ?? isUnique;
        }

        if (index.IsDescending is not null)
        {
            var isDescending = new FluentApiCodeFragment(nameof(IndexBuilder.IsDescending))
            {
                Arguments = index.IsDescending.Cast<object?>().ToList()
            };

            root = root?.Chain(isDescending) ?? isDescending;
        }

        var annotationsRoot = GenerateAnnotations(index, annotations, annotationCodeGenerator);
        if (annotationsRoot is not null)
        {
            root = root?.Chain(annotationsRoot) ?? annotationsRoot;
        }

        return root;
    }

    /// <summary>
    ///     Gets the fluent API calls to configure a property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="annotationCodeGenerator">The provider's annotation code generator.</param>
    /// <returns>The fluent API calls.</returns>
    public static FluentApiCodeFragment? GetFluentApiCalls(this IProperty property, IAnnotationCodeGenerator annotationCodeGenerator)
    {
        FluentApiCodeFragment? root = null;

        var annotations = annotationCodeGenerator.FilterIgnoredAnnotations(property.GetAnnotations())
            .ToDictionary(a => a.Name, a => a);
        annotationCodeGenerator.RemoveAnnotationsHandledByConventions(property, annotations);

        annotations.Remove(RelationalAnnotationNames.ColumnOrder);

        var annotationsHandledByDataAnnotations = new Dictionary<string, IAnnotation>(annotations);

        // Strip out any annotations handled as attributes
        // Only relational ones need to be removed here. Core ones are already removed by FilterIgnoredAnnotations
        _ = annotationCodeGenerator.GenerateDataAnnotationAttributes(property, annotations);

        // Handled by GetDataAnnotations above
        annotations.Remove(RelationalAnnotationNames.ColumnName);
        annotations.Remove(RelationalAnnotationNames.ColumnType);

        foreach (var key in annotations.Keys)
        {
            annotationsHandledByDataAnnotations.Remove(key);
        }

        if (!property.IsNullable
            && property.ClrType.IsNullableType()
            && !property.IsPrimaryKey())
        {
            var isRequired = new FluentApiCodeFragment(nameof(PropertyBuilder.IsRequired)) { IsHandledByDataAnnotations = true };

            root = root?.Chain(isRequired) ?? isRequired;
        }

        var maxLength = property.GetMaxLength();
        if (maxLength.HasValue)
        {
            var hasMaxLength = new FluentApiCodeFragment(nameof(PropertyBuilder.HasMaxLength))
            {
                Arguments = { maxLength.Value }, IsHandledByDataAnnotations = true
            };

            root = root?.Chain(hasMaxLength) ?? hasMaxLength;
        }

        var precision = property.GetPrecision();
        var scale = property.GetScale();
        if (precision != null && scale != null && scale != 0)
        {
            var hasPrecision = new FluentApiCodeFragment(nameof(PropertyBuilder.HasPrecision))
            {
                Arguments = { precision.Value, scale.Value }, IsHandledByDataAnnotations = true
            };

            root = root?.Chain(hasPrecision) ?? hasPrecision;
        }
        else if (precision != null)
        {
            var hasPrecision = new FluentApiCodeFragment(nameof(PropertyBuilder.HasPrecision))
            {
                Arguments = { precision.Value }, IsHandledByDataAnnotations = true
            };

            root = root?.Chain(hasPrecision) ?? hasPrecision;
        }

        if (property.IsUnicode() != null)
        {
            var isUnicode = new FluentApiCodeFragment(nameof(PropertyBuilder.IsUnicode)) { IsHandledByDataAnnotations = true };

            if (property.IsUnicode() == false)
            {
                isUnicode.Arguments.Add(false);
            }

            root = root?.Chain(isUnicode) ?? isUnicode;
        }

        var valueGenerated = property.ValueGenerated;
        if (((IConventionProperty)property).GetValueGeneratedConfigurationSource() is ConfigurationSource
            valueGeneratedConfigurationSource
            && valueGeneratedConfigurationSource != ConfigurationSource.Convention
            && ValueGenerationConvention.GetValueGenerated(property) != valueGenerated)
        {
            var valueGeneratedCall = new FluentApiCodeFragment(
                valueGenerated switch
                {
                    ValueGenerated.OnAdd => nameof(PropertyBuilder.ValueGeneratedOnAdd),
                    ValueGenerated.OnAddOrUpdate => property.IsConcurrencyToken
                        ? nameof(PropertyBuilder.IsRowVersion)
                        : nameof(PropertyBuilder.ValueGeneratedOnAddOrUpdate),
                    ValueGenerated.OnUpdate => nameof(PropertyBuilder.ValueGeneratedOnUpdate),
                    ValueGenerated.Never => nameof(PropertyBuilder.ValueGeneratedNever),
                    _ => throw new InvalidOperationException(DesignStrings.UnhandledEnumValue($"{nameof(ValueGenerated)}.{valueGenerated}"))
                });

            root = root?.Chain(valueGeneratedCall) ?? valueGeneratedCall;
        }

        if (property.IsConcurrencyToken)
        {
            var isConcurrencyToken = new FluentApiCodeFragment(nameof(PropertyBuilder.IsConcurrencyToken));

            root = root?.Chain(isConcurrencyToken) ?? isConcurrencyToken;
        }

        var annotationsRoot = GenerateAnnotations(property, annotations, annotationCodeGenerator);
        if (annotationsRoot is not null)
        {
            root = root?.Chain(annotationsRoot) ?? annotationsRoot;
        }

        annotationsRoot = GenerateAnnotations(
            property, annotationsHandledByDataAnnotations, annotationCodeGenerator, isHandledByDataAnnotations: true);
        if (annotationsRoot is not null)
        {
            root = root?.Chain(annotationsRoot) ?? annotationsRoot;
        }

        return root;
    }

    /// <summary>
    ///     Gets the fluent API calls to configure a foreign key.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <param name="annotationCodeGenerator">The provider's annotation code generator.</param>
    /// <param name="useStrings">A value indicating wheter to use string fluent API overloads instead of ones that take a property accessor lambda.</param>
    /// <returns>The fluent API calls.</returns>
    public static FluentApiCodeFragment? GetFluentApiCalls(
        this IForeignKey foreignKey,
        IAnnotationCodeGenerator annotationCodeGenerator,
        bool useStrings = false)
    {
        FluentApiCodeFragment? root = null;

        var annotations = annotationCodeGenerator.FilterIgnoredAnnotations(foreignKey.GetAnnotations())
            .ToDictionary(a => a.Name, a => a);
        annotationCodeGenerator.RemoveAnnotationsHandledByConventions(foreignKey, annotations);

        if (!foreignKey.PrincipalKey.IsPrimaryKey())
        {
            var hasPrincipalKey = new FluentApiCodeFragment(nameof(ReferenceReferenceBuilder.HasPrincipalKey));

            if (foreignKey.IsUnique
                && !useStrings)
            {
                hasPrincipalKey.TypeArguments.Add(foreignKey.PrincipalEntityType.Name);
            }

            if (useStrings)
            {
                hasPrincipalKey.Arguments = foreignKey.PrincipalKey.Properties.Select(p => p.Name).Cast<object?>().ToList();
            }
            else
            {
                hasPrincipalKey.Arguments.Add(
                    new PropertyAccessorCodeFragment("p", foreignKey.PrincipalKey.Properties.Select(p => p.Name).ToList()));
            }

            root = root?.Chain(hasPrincipalKey) ?? hasPrincipalKey;
        }

        var hasForeignKey =
            new FluentApiCodeFragment(nameof(ReferenceReferenceBuilder.HasForeignKey)) { IsHandledByDataAnnotations = true };

        // HACK: Work around issue #29448
        if (!foreignKey.PrincipalKey.IsPrimaryKey())
        {
            hasForeignKey.IsHandledByDataAnnotations = false;
        }

        if (foreignKey.IsUnique)
        {
            hasForeignKey.TypeArguments.Add(foreignKey.DeclaringEntityType.Name);
        }

        if (useStrings)
        {
            hasForeignKey.Arguments = foreignKey.Properties.Select(p => p.Name).Cast<object?>().ToList();
        }
        else
        {
            hasForeignKey.Arguments.Add(
                new PropertyAccessorCodeFragment("d", foreignKey.Properties.Select(p => p.Name).ToList()));
        }

        root = root?.Chain(hasForeignKey) ?? hasForeignKey;

        var defaultOnDeleteAction = foreignKey.IsRequired
            ? DeleteBehavior.Cascade
            : DeleteBehavior.ClientSetNull;
        if (foreignKey.DeleteBehavior != defaultOnDeleteAction)
        {
            var onDelete = new FluentApiCodeFragment(nameof(ReferenceReferenceBuilder.OnDelete))
            {
                Arguments = { foreignKey.DeleteBehavior }
            };

            root = root?.Chain(onDelete) ?? onDelete;
        }

        var annotationsRoot = GenerateAnnotations(foreignKey, annotations, annotationCodeGenerator);
        if (annotationsRoot is not null)
        {
            root = root?.Chain(annotationsRoot) ?? annotationsRoot;
        }

        return root;
    }

    /// <summary>
    ///     Gets the fluent API calls to configure a sequence.
    /// </summary>
    /// <param name="sequence">The sequence.</param>
    /// <param name="annotationCodeGenerator">The provider's annotation code generator.</param>
    /// <returns>The fluent API calls.</returns>
    public static FluentApiCodeFragment? GetFluentApiCalls(this ISequence sequence, IAnnotationCodeGenerator annotationCodeGenerator)
    {
        FluentApiCodeFragment? root = null;

        if (sequence.StartValue != Sequence.DefaultStartValue)
        {
            var startsAt = new FluentApiCodeFragment(nameof(SequenceBuilder.StartsAt)) { Arguments = { sequence.StartValue } };

            root = root?.Chain(startsAt) ?? startsAt;
        }

        if (sequence.IncrementBy != Sequence.DefaultIncrementBy)
        {
            var incrementsBy = new FluentApiCodeFragment(nameof(SequenceBuilder.IncrementsBy)) { Arguments = { sequence.IncrementBy } };

            root = root?.Chain(incrementsBy) ?? incrementsBy;
        }

        if (sequence.MinValue != Sequence.DefaultMinValue)
        {
            var hasMin = new FluentApiCodeFragment(nameof(SequenceBuilder.HasMin)) { Arguments = { sequence.MinValue } };

            root = root?.Chain(hasMin) ?? hasMin;
        }

        if (sequence.MaxValue != Sequence.DefaultMaxValue)
        {
            var hasMax = new FluentApiCodeFragment(nameof(SequenceBuilder.HasMax)) { Arguments = { sequence.MaxValue } };

            root = root?.Chain(hasMax) ?? hasMax;
        }

        if (sequence.IsCyclic != Sequence.DefaultIsCyclic)
        {
            var isCyclic = new FluentApiCodeFragment(nameof(SequenceBuilder.IsCyclic));

            root = root?.Chain(isCyclic) ?? isCyclic;
        }

        if (sequence.IsCached != Sequence.DefaultIsCached)
        {
            var useNoCache = new FluentApiCodeFragment(nameof(SequenceBuilder.UseNoCache));

            root = root?.Chain(useNoCache) ?? useNoCache;
        }
        else
        {
            var useCache = new FluentApiCodeFragment(nameof(SequenceBuilder.UseCache)) { Arguments = { sequence.CacheSize } };

            root = root?.Chain(useCache) ?? useCache;
        }

        return root;
    }

    private static FluentApiCodeFragment? GenerateAnnotations(
        IAnnotatable annotatable,
        Dictionary<string, IAnnotation> annotations,
        IAnnotationCodeGenerator annotationCodeGenerator,
        bool isHandledByDataAnnotations = false)
    {
        FluentApiCodeFragment? root = null;

        if (annotatable is IProperty property
            && annotations.TryGetValue(RelationalAnnotationNames.DefaultValueSql, out _)
            && annotations.TryGetValue(RelationalAnnotationNames.DefaultValue, out var parsedAnnotation))
        {
            if (Equals(property.ClrType.GetDefaultValue(), parsedAnnotation.Value))
            {
                // Default value is CLR default for property, so exclude it from scaffolded model
                annotations.Remove(RelationalAnnotationNames.DefaultValueSql);
                annotations.Remove(RelationalAnnotationNames.DefaultValue);
            }
            else
            {
                // SQL was parsed, so use parsed value and exclude raw value
                annotations.Remove(RelationalAnnotationNames.DefaultValueSql);
            }
        }

        foreach (var methodCall in annotationCodeGenerator.GenerateFluentApiCalls(annotatable, annotations))
        {
            var fluentApiCall = FluentApiCodeFragment.From(methodCall);
            fluentApiCall.IsHandledByDataAnnotations = isHandledByDataAnnotations;

            root = root?.Chain(fluentApiCall) ?? fluentApiCall;
        }

        foreach (var annotation in annotations.Values)
        {
            var hasAnnotation = new FluentApiCodeFragment(nameof(ModelBuilder.HasAnnotation))
            {
                Arguments = { annotation.Name, annotation.Value }, IsHandledByDataAnnotations = isHandledByDataAnnotations
            };

            root = root?.Chain(hasAnnotation) ?? hasAnnotation;
        }

        return root;
    }
}
