// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RelationalScaffoldingModelFactory : IScaffoldingModelFactory
{
    internal const string NavigationNameUniquifyingPattern = "{0}Navigation";
    internal const string SelfReferencingPrincipalEndNavigationNamePattern = "Inverse{0}";

    private readonly IOperationReporter _reporter;
    private readonly ICandidateNamingService _candidateNamingService;
    private Dictionary<DatabaseTable, CSharpUniqueNamer<DatabaseColumn>> _columnNamers = null!;
    private ModelReverseEngineerOptions _options = null!;
    private readonly DatabaseTable _nullTable = new();
    private CSharpUniqueNamer<DatabaseTable> _tableNamer = null!;
    private CSharpUniqueNamer<DatabaseTable> _dbSetNamer = null!;
    private readonly HashSet<DatabaseColumn> _unmappedColumns = [];
    private readonly IPluralizer _pluralizer;
    private readonly ICSharpUtilities _cSharpUtilities;
    private readonly IScaffoldingTypeMapper _scaffoldingTypeMapper;
    private readonly IModelRuntimeInitializer _modelRuntimeInitializer;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RelationalScaffoldingModelFactory(
        IOperationReporter reporter,
        ICandidateNamingService candidateNamingService,
        IPluralizer pluralizer,
        ICSharpUtilities cSharpUtilities,
        IScaffoldingTypeMapper scaffoldingTypeMapper,
        IModelRuntimeInitializer modelRuntimeInitializer)
    {
        _reporter = reporter;
        _candidateNamingService = candidateNamingService;
        _pluralizer = pluralizer;
        _cSharpUtilities = cSharpUtilities;
        _scaffoldingTypeMapper = scaffoldingTypeMapper;
        _modelRuntimeInitializer = modelRuntimeInitializer;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IModel Create(DatabaseModel databaseModel, ModelReverseEngineerOptions options)
    {
        var modelBuilder = new ModelBuilder();

        _tableNamer = new CSharpUniqueNamer<DatabaseTable>(
            options.UseDatabaseNames
                ? (t => t.Name)
                : t => _candidateNamingService.GenerateCandidateIdentifier(t),
            _cSharpUtilities,
            options.NoPluralize
                ? null
                : _pluralizer.Singularize,
            caseSensitive: false);
        _dbSetNamer = new CSharpUniqueNamer<DatabaseTable>(
            options.UseDatabaseNames
                ? (t => t.Name)
                : t => _candidateNamingService.GenerateCandidateIdentifier(t),
            _cSharpUtilities,
            options.NoPluralize
                ? null
                : _pluralizer.Pluralize,
            caseSensitive: true);
        _columnNamers = new Dictionary<DatabaseTable, CSharpUniqueNamer<DatabaseColumn>>();
        _options = options;

        VisitDatabaseModel(modelBuilder, databaseModel);

        return _modelRuntimeInitializer.Initialize((IModel)modelBuilder.Model, designTime: true);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual string GetEntityTypeName(DatabaseTable table)
        => _tableNamer.GetName(table);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual string GetDbSetName(DatabaseTable table)
        => _dbSetNamer.GetName(table);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual string GetPropertyName(DatabaseColumn column)
    {
        var table = column.Table ?? _nullTable;
        var usedNames = new List<string>();
        if (column.Table != null)
        {
            usedNames.Add(GetEntityTypeName(table));
        }

        if (!_columnNamers.ContainsKey(table))
        {
            if (_options.UseDatabaseNames)
            {
                _columnNamers.Add(
                    table,
                    new CSharpUniqueNamer<DatabaseColumn>(
                        c => c.Name,
                        usedNames,
                        _cSharpUtilities,
                        singularizePluralizer: null,
                        caseSensitive: true));
            }
            else
            {
                _columnNamers.Add(
                    table,
                    new CSharpUniqueNamer<DatabaseColumn>(
                        c => _candidateNamingService.GenerateCandidateIdentifier(c),
                        usedNames,
                        _cSharpUtilities,
                        singularizePluralizer: null,
                        caseSensitive: true));
            }
        }

        return _columnNamers[table].GetName(column);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual ModelBuilder VisitDatabaseModel(ModelBuilder modelBuilder, DatabaseModel databaseModel)
    {
        if (!string.IsNullOrEmpty(databaseModel.DefaultSchema))
        {
            modelBuilder.HasDefaultSchema(databaseModel.DefaultSchema);
        }

        if (!string.IsNullOrEmpty(databaseModel.DatabaseName))
        {
            modelBuilder.Model.SetDatabaseName(
                !_options.UseDatabaseNames && !string.IsNullOrEmpty(databaseModel.DatabaseName)
                    ? _candidateNamingService.GenerateCandidateIdentifier(databaseModel.DatabaseName)
                    : databaseModel.DatabaseName);
        }

        if (!string.IsNullOrEmpty(databaseModel.Collation))
        {
            modelBuilder.UseCollation(databaseModel.Collation);
        }

        VisitSequences(modelBuilder, databaseModel.Sequences);
        VisitTables(modelBuilder, databaseModel.Tables);
        VisitForeignKeys(modelBuilder, databaseModel.Tables.SelectMany(table => table.ForeignKeys).ToList());

        modelBuilder.Model.AddAnnotations(
            databaseModel.GetAnnotations().Where(
                a => a.Name != ScaffoldingAnnotationNames.ConnectionString));

        return modelBuilder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual ModelBuilder VisitSequences(
        ModelBuilder modelBuilder,
        ICollection<DatabaseSequence> sequences)
    {
        foreach (var sequence in sequences)
        {
            VisitSequence(modelBuilder, sequence);
        }

        return modelBuilder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SequenceBuilder? VisitSequence(ModelBuilder modelBuilder, DatabaseSequence sequence)
    {
        if (string.IsNullOrEmpty(sequence.Name))
        {
            _reporter.WriteWarning(DesignStrings.SequencesRequireName);
            return null;
        }

        Type? sequenceType = null;
        if (sequence.StoreType != null)
        {
            sequenceType = _scaffoldingTypeMapper.FindMapping(
                    sequence.StoreType,
                    keyOrIndex: false,
                    rowVersion: false)
                ?.ClrType;
        }

        if (sequenceType != null
            && !Sequence.SupportedTypes.Contains(sequenceType))
        {
            _reporter.WriteWarning(DesignStrings.BadSequenceType(sequence.Name, sequence.StoreType));
            return null;
        }

        var builder = sequenceType != null
            ? modelBuilder.HasSequence(sequenceType, sequence.Name, sequence.Schema)
            : modelBuilder.HasSequence(sequence.Name, sequence.Schema);

        if (sequence.IncrementBy.HasValue)
        {
            builder.IncrementsBy(sequence.IncrementBy.Value);
        }

        if (sequence.MaxValue.HasValue)
        {
            builder.HasMax(sequence.MaxValue.Value);
        }

        if (sequence.MinValue.HasValue)
        {
            builder.HasMin(sequence.MinValue.Value);
        }

        if (sequence.StartValue.HasValue)
        {
            builder.StartsAt(sequence.StartValue.Value);
        }

        if (sequence.IsCyclic.HasValue)
        {
            builder.IsCyclic(sequence.IsCyclic.Value);
        }

        if (sequence.IsCached.HasValue && !sequence.IsCached.Value)
        {
            builder.UseNoCache();
        }
        else if (sequence.IsCached.HasValue && sequence.CacheSize.HasValue)
        {
            builder.UseCache(sequence.CacheSize);
        }
        else
        {
            builder.UseCache();
        }

        return builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual ModelBuilder VisitTables(ModelBuilder modelBuilder, ICollection<DatabaseTable> tables)
    {
        foreach (var table in tables)
        {
            VisitTable(modelBuilder, table);
        }

        return modelBuilder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual EntityTypeBuilder? VisitTable(ModelBuilder modelBuilder, DatabaseTable table)
    {
        var entityTypeName = GetEntityTypeName(table);

        var builder = modelBuilder.Entity(entityTypeName);

        var dbSetName = GetDbSetName(table);
        builder.Metadata.SetDbSetName(dbSetName);

        if (table is DatabaseView)
        {
            builder.ToView(table.Name, table.Schema);
        }
        else
        {
            builder.ToTable(
                table.Name, table.Schema, tb =>
                {
                    if (table.Comment != null)
                    {
                        tb.HasComment(table.Comment);
                    }
                });
        }

        VisitColumns(builder, table.Columns);

        if (table.PrimaryKey != null)
        {
            var keyBuilder = VisitPrimaryKey(builder, table);

            if (keyBuilder == null)
            {
                _reporter.WriteWarning(DesignStrings.UnableToGenerateEntityType(table.DisplayName()));

                modelBuilder.Model.RemoveEntityType(entityTypeName);
                return null;
            }
        }
        else
        {
            builder.HasNoKey();
        }

        VisitUniqueConstraints(builder, table.UniqueConstraints);
        VisitIndexes(builder, table.Indexes);

        foreach (var trigger in table.Triggers)
        {
            builder.ToTable(
                table.Name, table.Schema, tb => tb
                    .HasTrigger(trigger.Name)
                    .Metadata.AddAnnotations(trigger.GetAnnotations()));
        }

        builder.Metadata.AddAnnotations(table.GetAnnotations());

        return builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual EntityTypeBuilder VisitColumns(EntityTypeBuilder builder, ICollection<DatabaseColumn> columns)
    {
        foreach (var column in columns)
        {
            VisitColumn(builder, column);
        }

        return builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual PropertyBuilder? VisitColumn(EntityTypeBuilder builder, DatabaseColumn column)
    {
        var typeScaffoldingInfo = GetTypeScaffoldingInfo(column);

        if (typeScaffoldingInfo == null)
        {
            _unmappedColumns.Add(column);
            _reporter.WriteWarning(
                DesignStrings.CannotFindTypeMappingForColumn(column.DisplayName(), column.StoreType));
            return null;
        }

        var clrType = typeScaffoldingInfo.ClrType;
        if (column.IsNullable)
        {
            clrType = clrType.MakeNullable();
        }

        if (clrType == typeof(bool)
            && column.DefaultValueSql != null
            && column.DefaultValue == null)
        {
            _reporter.WriteWarning(
                DesignStrings.NonNullableBoooleanColumnHasDefaultConstraint(column.DisplayName()));

            clrType = clrType.MakeNullable();
        }

        var property = builder.Property(clrType, GetPropertyName(column));

        property.HasColumnName(column.Name);

        if (!typeScaffoldingInfo.IsInferred
            && !string.IsNullOrWhiteSpace(column.StoreType))
        {
            property.HasColumnType(column.StoreType);
        }

        if (typeScaffoldingInfo.ScaffoldUnicode.HasValue)
        {
            property.IsUnicode(typeScaffoldingInfo.ScaffoldUnicode.Value);
        }

        if (typeScaffoldingInfo.ScaffoldFixedLength == true)
        {
            property.IsFixedLength();
        }

        if (typeScaffoldingInfo.ScaffoldMaxLength.HasValue)
        {
            property.HasMaxLength(typeScaffoldingInfo.ScaffoldMaxLength.Value);
        }

        if (typeScaffoldingInfo.ScaffoldPrecision.HasValue)
        {
            if (typeScaffoldingInfo.ScaffoldScale.HasValue)
            {
                property.HasPrecision(
                    typeScaffoldingInfo.ScaffoldPrecision.Value,
                    typeScaffoldingInfo.ScaffoldScale.Value);
            }
            else
            {
                property.HasPrecision(typeScaffoldingInfo.ScaffoldPrecision.Value);
            }
        }

        if (column.ValueGenerated == ValueGenerated.OnAdd)
        {
            property.ValueGeneratedOnAdd();
        }

        if (column.ValueGenerated == ValueGenerated.OnUpdate)
        {
            property.ValueGeneratedOnUpdate();
        }

        if (column.ValueGenerated == ValueGenerated.OnAddOrUpdate)
        {
            property.ValueGeneratedOnAddOrUpdate();
        }

        if (column.DefaultValue != null)
        {
            property.HasDefaultValue(column.DefaultValue);
        }

        if (column.DefaultValueSql != null)
        {
            property.HasDefaultValueSql(column.DefaultValueSql);
        }

        if (column.ComputedColumnSql != null)
        {
            if (column.ComputedColumnSql.Length == 0)
            {
                property.HasComputedColumnSql();
            }
            else
            {
                property.HasComputedColumnSql(column.ComputedColumnSql, column.IsStored);
            }
        }

        if (column.Comment != null)
        {
            property.HasComment(column.Comment);
        }

        if (column.Collation != null)
        {
            property.UseCollation(column.Collation);
        }

        if (!(column.Table.PrimaryKey?.Columns.Contains(column) ?? false))
        {
            property.IsRequired(!column.IsNullable);
        }

        if ((bool?)column[ScaffoldingAnnotationNames.ConcurrencyToken] == true)
        {
            property.IsConcurrencyToken();
        }

        property.Metadata.SetColumnOrder(column.Table.Columns.IndexOf(column));

        property.Metadata.AddAnnotations(
            column.GetAnnotations().Where(
                a => a.Name != ScaffoldingAnnotationNames.ConcurrencyToken
                    && a.Name != ScaffoldingAnnotationNames.ClrType));

        return property;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual KeyBuilder? VisitPrimaryKey(EntityTypeBuilder builder, DatabaseTable table)
    {
        var primaryKey = table.PrimaryKey!;

        var unmappedColumns = primaryKey.Columns
            .Where(c => _unmappedColumns.Contains(c))
            .Select(c => c.Name)
            .ToList();
        if (unmappedColumns.Count > 0)
        {
            _reporter.WriteWarning(
                DesignStrings.PrimaryKeyErrorPropertyNotFound(
                    table.DisplayName(),
                    string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, unmappedColumns)));
            return null;
        }

        var keyBuilder = builder.HasKey(primaryKey.Columns.Select(GetPropertyName).ToArray());

        if (primaryKey.Columns is [{ ValueGenerated: null, DefaultValueSql: null }])
        {
            var property = builder.Metadata.FindProperty(GetPropertyName(primaryKey.Columns[0]));
            if (property != null)
            {
                var conventionalValueGenerated = ValueGenerationConvention.GetValueGenerated(property);
                if (conventionalValueGenerated == ValueGenerated.OnAdd)
                {
                    property.ValueGenerated = ValueGenerated.Never;
                }
            }
        }

        if (!string.IsNullOrEmpty(primaryKey.Name)
            && primaryKey.Name != keyBuilder.Metadata.GetDefaultName())
        {
            keyBuilder.HasName(primaryKey.Name);
        }

        keyBuilder.Metadata.AddAnnotations(primaryKey.GetAnnotations());

        return keyBuilder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual EntityTypeBuilder VisitUniqueConstraints(
        EntityTypeBuilder builder,
        ICollection<DatabaseUniqueConstraint> uniqueConstraints)
    {
        foreach (var uniqueConstraint in uniqueConstraints)
        {
            VisitUniqueConstraint(builder, uniqueConstraint);
        }

        return builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IndexBuilder? VisitUniqueConstraint(
        EntityTypeBuilder builder,
        DatabaseUniqueConstraint uniqueConstraint)
    {
        var unmappedColumns = uniqueConstraint.Columns
            .Where(c => _unmappedColumns.Contains(c))
            .Select(c => c.Name)
            .ToList();
        if (unmappedColumns.Count > 0)
        {
            _reporter.WriteWarning(
                DesignStrings.UnableToScaffoldIndexMissingProperty(
                    uniqueConstraint.Name,
                    string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, unmappedColumns)));
            return null;
        }

        var propertyNames = uniqueConstraint.Columns.Select(GetPropertyName).ToArray();
        var indexBuilder = string.IsNullOrEmpty(uniqueConstraint.Name)
            ? builder.HasIndex(propertyNames)
            : builder.HasIndex(propertyNames, uniqueConstraint.Name);
        indexBuilder = indexBuilder.IsUnique();
        indexBuilder.Metadata.AddAnnotations(uniqueConstraint.GetAnnotations());

        return indexBuilder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual EntityTypeBuilder VisitIndexes(EntityTypeBuilder builder, ICollection<DatabaseIndex> indexes)
    {
        foreach (var index in indexes)
        {
            VisitIndex(builder, index);
        }

        return builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IndexBuilder? VisitIndex(EntityTypeBuilder builder, DatabaseIndex index)
    {
        var unmappedColumns = index.Columns
            .Where(c => _unmappedColumns.Contains(c))
            .Select(c => c.Name)
            .ToList();
        if (unmappedColumns.Count > 0)
        {
            _reporter.WriteWarning(
                DesignStrings.UnableToScaffoldIndexMissingProperty(
                    index.Name,
                    string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, unmappedColumns)));
            return null;
        }

        var propertyNames = index.Columns.Select(GetPropertyName).ToArray();
        var indexBuilder = string.IsNullOrEmpty(index.Name)
            ? builder.HasIndex(propertyNames)
            : builder.HasIndex(propertyNames, index.Name);

        indexBuilder = indexBuilder.IsUnique(index.IsUnique);

        if (index.IsDescending.Any(desc => desc))
        {
            indexBuilder = indexBuilder.IsDescending(index.IsDescending.ToArray());
        }

        if (index.Filter != null)
        {
            indexBuilder = indexBuilder.HasFilter(index.Filter);
        }

        indexBuilder.Metadata.AddAnnotations(index.GetAnnotations());

        return indexBuilder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual ModelBuilder VisitForeignKeys(
        ModelBuilder modelBuilder,
        IList<DatabaseForeignKey> foreignKeys)
    {
        foreach (var fk in foreignKeys)
        {
            VisitForeignKey(modelBuilder, fk);
        }

        // Note: must completely assign all foreign keys before assigning
        // navigation properties otherwise naming of navigation properties
        // when there are multiple foreign keys does not work.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (((IEntityType)entityType).IsSimpleManyToManyJoinEntityType())
            {
                var fks = entityType.GetForeignKeys().ToArray();
                var leftEntityType = fks[0].PrincipalEntityType;
                var rightEntityType = fks[1].PrincipalEntityType;

                var leftExistingIdentifiers = ExistingIdentifiers(leftEntityType);
                var leftNavigationPropertyCandidateName =
                    _candidateNamingService.GetDependentEndCandidateNavigationPropertyName(fks[1]);
                if (!_options.NoPluralize)
                {
                    leftNavigationPropertyCandidateName = _pluralizer.Pluralize(leftNavigationPropertyCandidateName);
                }

                var leftNavigationPropertyName =
                    _cSharpUtilities.GenerateCSharpIdentifier(
                        leftNavigationPropertyCandidateName,
                        leftExistingIdentifiers,
                        singularizePluralizer: null,
                        uniquifier: NavigationUniquifier);

                var rightExistingIdentifiers = ExistingIdentifiers(rightEntityType);
                var rightNavigationPropertyCandidateName =
                    _candidateNamingService.GetDependentEndCandidateNavigationPropertyName(fks[0]);
                if (!_options.NoPluralize)
                {
                    rightNavigationPropertyCandidateName = _pluralizer.Pluralize(rightNavigationPropertyCandidateName);
                }

                var rightNavigationPropertyName =
                    _cSharpUtilities.GenerateCSharpIdentifier(
                        rightNavigationPropertyCandidateName,
                        rightExistingIdentifiers,
                        singularizePluralizer: null,
                        uniquifier: NavigationUniquifier);

                var leftSkipNavigation = leftEntityType.AddSkipNavigation(
                    leftNavigationPropertyName, memberInfo: null, targetEntityType: rightEntityType, collection: true, onDependent: false);
                leftSkipNavigation.SetForeignKey(fks[0]);
                var rightSkipNavigation = rightEntityType.AddSkipNavigation(
                    rightNavigationPropertyName, memberInfo: null, targetEntityType: leftEntityType, collection: true, onDependent: false);
                rightSkipNavigation.SetForeignKey(fks[1]);
                leftSkipNavigation.SetInverse(rightSkipNavigation);
                rightSkipNavigation.SetInverse(leftSkipNavigation);
                continue;
            }

            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                AddNavigationProperties(foreignKey);
            }
        }

        return modelBuilder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IMutableForeignKey? VisitForeignKey(ModelBuilder modelBuilder, DatabaseForeignKey foreignKey)
    {
        if (foreignKey.PrincipalTable == null)
        {
            _reporter.WriteWarning(
                DesignStrings.ForeignKeyScaffoldErrorPrincipalTableNotFound(foreignKey.DisplayName()));
            return null;
        }

        if (foreignKey.Table == null)
        {
            return null;
        }

        var dependentEntityType = modelBuilder.Model.FindEntityType(GetEntityTypeName(foreignKey.Table));
        if (dependentEntityType == null)
        {
            return null;
        }

        var unmappedDependentColumns = foreignKey.Columns
            .Where(c => _unmappedColumns.Contains(c))
            .Select(c => c.Name)
            .ToList();
        if (unmappedDependentColumns.Count > 0)
        {
            _reporter.WriteWarning(
                DesignStrings.ForeignKeyScaffoldErrorPropertyNotFound(
                    foreignKey.DisplayName(),
                    string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, unmappedDependentColumns)));
            return null;
        }

        var dependentProperties = foreignKey.Columns
            .Select(GetPropertyName)
            .Select(name => dependentEntityType.FindProperty(name)!)
            .ToList()
            .AsReadOnly();

        var principalEntityType = modelBuilder.Model.FindEntityType(GetEntityTypeName(foreignKey.PrincipalTable));
        if (principalEntityType == null)
        {
            _reporter.WriteWarning(
                DesignStrings.ForeignKeyScaffoldErrorPrincipalTableScaffoldingError(
                    foreignKey.DisplayName(),
                    foreignKey.PrincipalTable.DisplayName()));
            return null;
        }

        var unmappedPrincipalColumns = foreignKey.PrincipalColumns
            .Where(pc => principalEntityType.FindProperty(GetPropertyName(pc)) == null)
            .Select(pc => pc.Name)
            .ToList();
        if (unmappedPrincipalColumns.Count > 0)
        {
            _reporter.WriteWarning(
                DesignStrings.ForeignKeyScaffoldErrorPropertyNotFound(
                    foreignKey.DisplayName(),
                    string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, unmappedPrincipalColumns)));
            return null;
        }

        var principalPropertiesMap = foreignKey.PrincipalColumns
            .Select(
                fc => (property: principalEntityType.FindProperty(GetPropertyName(fc))!, column: fc)).ToList();
        var principalProperties = principalPropertiesMap
            .Select(tuple => tuple.property)
            .ToList();

        var principalKey = principalEntityType.FindKey(principalProperties);
        if (principalKey == null)
        {
            var index = principalEntityType
                .GetIndexes()
                .FirstOrDefault(i => i.Properties.SequenceEqual(principalProperties) && i.IsUnique);
            if (index != null)
            {
                // ensure all principal properties are non-nullable even if the columns
                // are nullable on the database. EF's concept of a key requires this.
                var nullablePrincipalProperties =
                    principalPropertiesMap.Where(tuple => tuple.property.IsNullable).ToList();
                if (nullablePrincipalProperties.Count > 0)
                {
                    _reporter.WriteWarning(
                        DesignStrings.ForeignKeyPrincipalEndContainsNullableColumns(
                            foreignKey.DisplayName(),
                            index.GetDatabaseName(),
                            nullablePrincipalProperties.Select(tuple => tuple.column.DisplayName()).ToList()
                                .Aggregate((a, b) => a + "," + b)));

                    nullablePrincipalProperties.ForEach(tuple => tuple.property.IsNullable = false);
                }

                principalKey = principalEntityType.AddKey(principalProperties);
            }
            else
            {
                var principalColumns = foreignKey.PrincipalColumns.Select(c => c.Name).ToList();

                _reporter.WriteWarning(
                    DesignStrings.ForeignKeyScaffoldErrorPrincipalKeyNotFound(
                        foreignKey.DisplayName(),
                        string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, principalColumns),
                        principalEntityType.Name));

                return null;
            }
        }

        var existingForeignKey = dependentEntityType.FindForeignKey(dependentProperties, principalKey, principalEntityType);
        if (existingForeignKey is not null)
        {
            _reporter.WriteWarning(
                DesignStrings.ForeignKeyWithSameFacetsExists(foreignKey.DisplayName(), existingForeignKey.GetConstraintName()));

            return null;
        }

        var newForeignKey = dependentEntityType.AddForeignKey(
            dependentProperties, principalKey, principalEntityType);

        var dependentKey = dependentEntityType.FindKey(dependentProperties);
        var dependentIndexes = dependentEntityType.GetIndexes()
            .Where(i => i.Properties.SequenceEqual(dependentProperties));
        newForeignKey.IsUnique = dependentKey != null
            || dependentIndexes.Any(i => i.IsUnique);

        if (!string.IsNullOrEmpty(foreignKey.Name)
            && foreignKey.Name != newForeignKey.GetDefaultName())
        {
            newForeignKey.SetConstraintName(foreignKey.Name);
        }

        AssignOnDeleteAction(foreignKey, newForeignKey);

        newForeignKey.AddAnnotations(foreignKey.GetAnnotations());

        return newForeignKey;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void AddNavigationProperties(IMutableForeignKey foreignKey)
    {
        var dependentEndExistingIdentifiers = ExistingIdentifiers(foreignKey.DeclaringEntityType);
        var dependentEndNavigationPropertyCandidateName =
            _candidateNamingService.GetDependentEndCandidateNavigationPropertyName(foreignKey);
        var dependentEndNavigationPropertyName =
            _cSharpUtilities.GenerateCSharpIdentifier(
                dependentEndNavigationPropertyCandidateName,
                dependentEndExistingIdentifiers,
                singularizePluralizer: null,
                uniquifier: NavigationUniquifier);

        foreignKey.SetDependentToPrincipal(dependentEndNavigationPropertyName);

        if (foreignKey.DeclaringEntityType.IsKeyless)
        {
            return;
        }

        var principalEndExistingIdentifiers = ExistingIdentifiers(foreignKey.PrincipalEntityType);
        var principalEndNavigationPropertyCandidateName = foreignKey.IsSelfReferencing()
            ? string.Format(
                CultureInfo.CurrentCulture,
                SelfReferencingPrincipalEndNavigationNamePattern,
                dependentEndNavigationPropertyName)
            : _candidateNamingService.GetPrincipalEndCandidateNavigationPropertyName(
                foreignKey, dependentEndNavigationPropertyName);

        if (!foreignKey.IsUnique
            && !foreignKey.IsSelfReferencing())
        {
            principalEndNavigationPropertyCandidateName = _options.NoPluralize
                ? principalEndNavigationPropertyCandidateName
                : _pluralizer.Pluralize(principalEndNavigationPropertyCandidateName);
        }

        var principalEndNavigationPropertyName =
            _cSharpUtilities.GenerateCSharpIdentifier(
                principalEndNavigationPropertyCandidateName,
                principalEndExistingIdentifiers,
                singularizePluralizer: null,
                uniquifier: NavigationUniquifier);

        foreignKey.SetPrincipalToDependent(principalEndNavigationPropertyName);
    }

    // Stores the names of the EntityType itself and its Properties, but does not include any Navigation Properties
    private readonly Dictionary<IReadOnlyEntityType, List<string>> _entityTypeAndPropertyIdentifiers =
        new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual List<string> ExistingIdentifiers(IReadOnlyEntityType entityType)
    {
        if (!_entityTypeAndPropertyIdentifiers.TryGetValue(entityType, out var existingIdentifiers))
        {
            existingIdentifiers = [entityType.Name];
            existingIdentifiers.AddRange(entityType.GetProperties().Select(p => p.Name));
            _entityTypeAndPropertyIdentifiers[entityType] = existingIdentifiers;
        }

        existingIdentifiers.AddRange(entityType.GetNavigations().Select(p => p.Name));
        existingIdentifiers.AddRange(entityType.GetSkipNavigations().Select(p => p.Name));
        return existingIdentifiers;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual TypeScaffoldingInfo? GetTypeScaffoldingInfo(DatabaseColumn column)
    {
        if (column.StoreType == null)
        {
            return null;
        }

        return _scaffoldingTypeMapper.FindMapping(
            column.StoreType,
            column.IsKeyOrIndex(),
            column.IsRowVersion(),
            (Type?)column[ScaffoldingAnnotationNames.ClrType]);
    }

    private static void AssignOnDeleteAction(
        DatabaseForeignKey databaseForeignKey,
        IMutableForeignKey foreignKey)
    {
        switch (databaseForeignKey.OnDelete)
        {
            case ReferentialAction.Cascade:
                foreignKey.DeleteBehavior = DeleteBehavior.Cascade;
                break;

            case ReferentialAction.SetNull:
                foreignKey.DeleteBehavior = DeleteBehavior.SetNull;
                break;

            case ReferentialAction.Restrict:
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
                break;

            default:
                foreignKey.DeleteBehavior = DeleteBehavior.ClientSetNull;
                break;
        }
    }

    // TODO use CSharpUniqueNamer
    private static string NavigationUniquifier(string proposedIdentifier, ICollection<string>? existingIdentifiers)
    {
        if (existingIdentifiers?.Contains(proposedIdentifier) != true)
        {
            return proposedIdentifier;
        }

        var finalIdentifier =
            string.Format(CultureInfo.CurrentCulture, NavigationNameUniquifyingPattern, proposedIdentifier);
        var suffix = 1;
        while (existingIdentifiers.Contains(finalIdentifier))
        {
            finalIdentifier = proposedIdentifier + suffix;
            suffix++;
        }

        return finalIdentifier;
    }
}
