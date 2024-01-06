// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the table name based on the <see cref="DbSet{TEntity}" /> property name.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class TableNameFromDbSetConvention :
    IEntityTypeAddedConvention,
    IEntityTypeBaseTypeChangedConvention,
    IEntityTypeAnnotationChangedConvention,
    IModelFinalizingConvention
{
    private readonly IDictionary<Type, string> _sets;

    /// <summary>
    ///     Creates a new instance of <see cref="TableNameFromDbSetConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public TableNameFromDbSetConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
    {
        _sets = new Dictionary<Type, string>();
        List<Type>? ambiguousTypes = null;
        foreach (var set in dependencies.SetFinder.FindSets(dependencies.ContextType))
        {
            if (!_sets.ContainsKey(set.Type))
            {
                _sets.Add(set.Type, set.Name);
            }
            else
            {
                ambiguousTypes ??= [];

                ambiguousTypes.Add(set.Type);
            }
        }

        if (ambiguousTypes != null)
        {
            foreach (var type in ambiguousTypes)
            {
                _sets.Remove(type);
            }
        }

        Dependencies = dependencies;
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessEntityTypeBaseTypeChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? oldBaseType,
        IConventionContext<IConventionEntityType> context)
    {
        var entityType = entityTypeBuilder.Metadata;

        if (oldBaseType == null
            && newBaseType != null
            && (entityType.GetMappingStrategy() ?? RelationalAnnotationNames.TphMappingStrategy)
            == RelationalAnnotationNames.TphMappingStrategy)
        {
            entityTypeBuilder.HasNoAnnotation(RelationalAnnotationNames.TableName);
        }
        else if (oldBaseType != null
                 && newBaseType == null
                 && !entityType.HasSharedClrType
                 && _sets.TryGetValue(entityType.ClrType, out var setName))
        {
            entityTypeBuilder.ToTable(setName);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
    {
        var entityType = entityTypeBuilder.Metadata;
        if (!entityType.HasSharedClrType
            && (entityType.BaseType == null
                || (entityType.GetMappingStrategy() ?? RelationalAnnotationNames.TphMappingStrategy)
                != RelationalAnnotationNames.TphMappingStrategy)
            && _sets.TryGetValue(entityType.ClrType, out var setName))
        {
            entityTypeBuilder.ToTable(setName);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessEntityTypeAnnotationChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (name == RelationalAnnotationNames.MappingStrategy
            && annotation != null
            && (entityTypeBuilder.Metadata.GetMappingStrategy() ?? RelationalAnnotationNames.TphMappingStrategy)
            != RelationalAnnotationNames.TphMappingStrategy)
        {
            foreach (var derivedEntityType in entityTypeBuilder.Metadata.GetDerivedTypesInclusive())
            {
                if (!derivedEntityType.HasSharedClrType
                    && _sets.TryGetValue(derivedEntityType.ClrType, out var setName))
                {
                    derivedEntityType.Builder.ToTable(setName);
                }
            }
        }
    }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            if (entityType.GetTableName() != null
                && _sets.ContainsKey(entityType.ClrType))
            {
                if (entityType.GetViewNameConfigurationSource() != null)
                {
                    // Undo the convention change if the entity type is mapped to a view
                    entityType.Builder.HasNoAnnotation(RelationalAnnotationNames.TableName);
                }

                var mappingStrategy = entityType.GetMappingStrategy();

                if (mappingStrategy == RelationalAnnotationNames.TpcMappingStrategy
                    && entityType.IsAbstract())
                {
                    // Undo the convention change if the entity type is mapped using TPC
                    entityType.Builder.HasNoAnnotation(RelationalAnnotationNames.TableName);
                }

                if (mappingStrategy == RelationalAnnotationNames.TphMappingStrategy
                    && entityType.BaseType != null)
                {
                    // Undo the convention change if the hierarchy ultimately ends up TPH
                    entityType.Builder.HasNoAnnotation(RelationalAnnotationNames.TableName);
                }
            }
        }
    }
}
