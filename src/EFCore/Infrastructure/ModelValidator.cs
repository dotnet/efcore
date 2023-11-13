// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     The validator that enforces core rules common for all providers.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public class ModelValidator : IModelValidator
{
    private static readonly IEnumerable<string> DictionaryProperties =
        typeof(IDictionary<string, object>).GetRuntimeProperties().Select(e => e.Name);

    /// <summary>
    ///     Creates a new instance of <see cref="ModelValidator" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public ModelValidator(ModelValidatorDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ModelValidatorDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual void Validate(IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        ValidateIgnoredMembers(model, logger);
        ValidatePropertyMapping(model, logger);
        ValidateRelationships(model, logger);
        ValidateOwnership(model, logger);
        ValidateNonNullPrimaryKeys(model, logger);
        ValidateNoShadowKeys(model, logger);
        ValidateNoMutableKeys(model, logger);
        ValidateNoCycles(model, logger);
        ValidateClrInheritance(model, logger);
        ValidateInheritanceMapping(model, logger);
        ValidateChangeTrackingStrategy(model, logger);
        ValidateForeignKeys(model, logger);
        ValidateFieldMapping(model, logger);
        ValidateQueryFilters(model, logger);
        ValidateData(model, logger);
        ValidateTypeMappings(model, logger);
        ValidatePrimitiveCollections(model, logger);
        ValidateTriggers(model, logger);
        LogShadowProperties(model, logger);
    }

    /// <summary>
    ///     Validates relationships.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateRelationships(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            foreach (var foreignKey in entityType.GetDeclaredForeignKeys())
            {
                if (foreignKey.IsUnique
                    && foreignKey is IConventionForeignKey concreteFk
                    && concreteFk.GetPrincipalEndConfigurationSource() == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.AmbiguousOneToOneRelationship(
                            foreignKey.DeclaringEntityType.DisplayName()
                            + (foreignKey.DependentToPrincipal == null
                                ? ""
                                : "." + foreignKey.DependentToPrincipal.Name),
                            foreignKey.PrincipalEntityType.DisplayName()
                            + (foreignKey.PrincipalToDependent == null
                                ? ""
                                : "." + foreignKey.PrincipalToDependent.Name)));
                }
            }

            foreach (var skipNavigation in entityType.GetDeclaredSkipNavigations())
            {
                if (!skipNavigation.IsCollection)
                {
                    throw new InvalidOperationException(
                        CoreStrings.SkipNavigationNonCollection(
                            skipNavigation.Name, skipNavigation.DeclaringEntityType.DisplayName()));
                }

                if (skipNavigation.ForeignKey == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.SkipNavigationNoForeignKey(
                            skipNavigation.Name, skipNavigation.DeclaringEntityType.DisplayName()));
                }

                if (skipNavigation.Inverse == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.SkipNavigationNoInverse(
                            skipNavigation.Name, skipNavigation.DeclaringEntityType.DisplayName()));
                }
            }
        }
    }

    /// <summary>
    ///     Validates property mappings.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidatePropertyMapping(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (model is not IConventionModel conventionModel)
        {
            return;
        }

        foreach (var entityType in conventionModel.GetEntityTypes())
        {
            Validate(entityType);
        }

        void Validate(IConventionTypeBase typeBase)
        {
            var unmappedProperty = typeBase.GetDeclaredProperties().FirstOrDefault(
                p => (!ConfigurationSource.Convention.Overrides(p.GetConfigurationSource())
                        // Use a better condition for non-persisted properties when issue #14121 is implemented
                        || !p.IsImplicitlyCreated())
                    && p.FindTypeMapping() == null);

            if (unmappedProperty != null)
            {
                ThrowPropertyNotMappedException(
                    (unmappedProperty.GetValueConverter()?.ProviderClrType ?? unmappedProperty.ClrType).ShortDisplayName(),
                    typeBase,
                    unmappedProperty);
            }

            foreach (var complexProperty in typeBase.GetDeclaredComplexProperties())
            {
                if (complexProperty.IsShadowProperty())
                {
                    throw new InvalidOperationException(
                        CoreStrings.ComplexPropertyShadow(typeBase.DisplayName(), complexProperty.Name));
                }

                if (complexProperty.IsIndexerProperty())
                {
                    throw new InvalidOperationException(
                        CoreStrings.ComplexPropertyIndexer(typeBase.DisplayName(), complexProperty.Name));
                }

                if (complexProperty.IsCollection)
                {
                    throw new InvalidOperationException(
                        CoreStrings.ComplexPropertyCollection(typeBase.DisplayName(), complexProperty.Name));
                }

                if (complexProperty.IsNullable)
                {
                    throw new InvalidOperationException(
                        CoreStrings.ComplexPropertyOptional(typeBase.DisplayName(), complexProperty.Name));
                }

                if (!complexProperty.ComplexType.GetMembers().Any())
                {
                    throw new InvalidOperationException(
                        CoreStrings.EmptyComplexType(complexProperty.ComplexType.DisplayName()));
                }

                Validate(complexProperty.ComplexType);
            }

            if (typeBase.ClrType == Model.DefaultPropertyBagType)
            {
                return;
            }

            var runtimeProperties = typeBase.GetRuntimeProperties();
            var clrProperties = new HashSet<string>(StringComparer.Ordinal);
            clrProperties.UnionWith(
                runtimeProperties.Values
                    .Where(pi => pi.IsCandidateProperty(needsWrite: false))
                    .Select(pi => pi.GetSimpleMemberName()));

            clrProperties.ExceptWith(typeBase.GetMembers().Select(p => p.Name));

            if (typeBase.IsPropertyBag)
            {
                clrProperties.ExceptWith(DictionaryProperties);
            }

            if (clrProperties.Count <= 0)
            {
                return;
            }

            foreach (var clrPropertyName in clrProperties)
            {
                if (typeBase.FindIgnoredConfigurationSource(clrPropertyName) != null)
                {
                    continue;
                }

                var clrProperty = runtimeProperties[clrPropertyName];
                var propertyType = clrProperty.PropertyType;
                var targetSequenceType = propertyType.TryGetSequenceType();

                if (conventionModel.FindIgnoredConfigurationSource(propertyType) != null
                    || conventionModel.IsIgnoredType(propertyType)
                    || (targetSequenceType != null
                        && (conventionModel.FindIgnoredConfigurationSource(targetSequenceType) != null
                            || conventionModel.IsIgnoredType(targetSequenceType))))
                {
                    continue;
                }

                var targetType = Dependencies.MemberClassifier.FindCandidateNavigationPropertyType(
                    clrProperty, conventionModel, useAttributes: true, out var targetOwned);
                if (targetType == null
                    && clrProperty.FindSetterProperty() == null)
                {
                    continue;
                }

                var isAdHoc = Equals(model.FindAnnotation(CoreAnnotationNames.AdHocModel)?.Value, true);
                if (targetType != null)
                {
                    var targetShared = conventionModel.IsShared(targetType);
                    targetOwned ??= IsOwned(targetType, conventionModel);

                    if (typeBase is not IConventionEntityType entityType)
                    {
                        if (!((IReadOnlyComplexType)typeBase).IsContainedBy(targetType))
                        {
                            throw new InvalidOperationException(
                                CoreStrings.NavigationNotAddedComplexType(
                                    typeBase.DisplayName(), clrProperty.Name, propertyType.ShortDisplayName()));
                        }

                        continue;
                    }

                    // ReSharper disable CheckForReferenceEqualityInstead.1
                    // ReSharper disable CheckForReferenceEqualityInstead.3
                    if ((isAdHoc
                            || !entityType.IsKeyless
                            || targetSequenceType == null)
                        && entityType.GetDerivedTypes().All(
                            dt => dt.GetDeclaredNavigations().FirstOrDefault(n => n.Name == clrProperty.GetSimpleMemberName())
                                == null)
                        && (!(targetShared || targetOwned.Value)
                            || !targetType.Equals(entityType.ClrType))
                        && (!entityType.IsInOwnershipPath(targetType)
                            || targetSequenceType == null))
                    {
                        if (entityType.IsOwned()
                            && targetOwned.Value)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.AmbiguousOwnedNavigation(
                                    typeBase.DisplayName() + "." + clrProperty.Name, targetType.ShortDisplayName()));
                        }

                        if (targetShared)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.NonConfiguredNavigationToSharedType(clrProperty.Name, typeBase.DisplayName()));
                        }

                        throw new InvalidOperationException(
                            isAdHoc
                                ? CoreStrings.NavigationNotAddedAdHoc(
                                    typeBase.DisplayName(), clrProperty.Name, propertyType.ShortDisplayName())
                                : CoreStrings.NavigationNotAdded(
                                    typeBase.DisplayName(), clrProperty.Name, propertyType.ShortDisplayName()));
                    }

                    // ReSharper restore CheckForReferenceEqualityInstead.3
                    // ReSharper restore CheckForReferenceEqualityInstead.1
                }
                else if (targetSequenceType == null && propertyType.IsInterface
                         || targetSequenceType?.IsInterface == true)
                {
                    throw new InvalidOperationException(
                        CoreStrings.InterfacePropertyNotAdded(
                            typeBase.DisplayName(), clrProperty.Name, propertyType.ShortDisplayName()));
                }
                else
                {
                    throw new InvalidOperationException(
                        isAdHoc
                            ? CoreStrings.PropertyNotAddedAdHoc(
                                typeBase.DisplayName(), clrProperty.Name, propertyType.ShortDisplayName())
                            : CoreStrings.PropertyNotAdded(
                                typeBase.DisplayName(), clrProperty.Name, propertyType.ShortDisplayName()));
                }
            }
        }
    }

    /// <summary>
    ///     Throws an <see cref="InvalidOperationException" /> with a message containing provider-specific information, when
    ///     available, indicating possible reasons why the property cannot be mapped.
    /// </summary>
    /// <param name="propertyType">The property CLR type.</param>
    /// <param name="typeBase">The structural type.</param>
    /// <param name="unmappedProperty">The property.</param>
    protected virtual void ThrowPropertyNotMappedException(
        string propertyType,
        IConventionTypeBase typeBase,
        IConventionProperty unmappedProperty)
        => throw new InvalidOperationException(
            CoreStrings.PropertyNotMapped(
                propertyType,
                typeBase.DisplayName(),
                unmappedProperty.Name));

    /// <summary>
    ///     Returns a value indicating whether that target CLR type would correspond to an owned entity type.
    /// </summary>
    /// <param name="targetType">The target CLR type.</param>
    /// <param name="conventionModel">The model.</param>
    /// <returns><see langword="true" /> if the given CLR type corresponds to an owned entity type.</returns>
    protected virtual bool IsOwned(Type targetType, IConventionModel conventionModel)
        => conventionModel.FindIsOwnedConfigurationSource(targetType) != null
            || conventionModel.FindEntityTypes(targetType).Any(t => t.IsOwned());

    /// <summary>
    ///     Validates that no attempt is made to ignore inherited properties.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateIgnoredMembers(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (model is not IConventionModel conventionModel)
        {
            return;
        }

        foreach (var entityType in conventionModel.GetEntityTypes())
        {
            foreach (var ignoredMember in entityType.GetIgnoredMembers())
            {
                if (entityType.FindIgnoredConfigurationSource(ignoredMember) != ConfigurationSource.Explicit)
                {
                    continue;
                }

                var property = entityType.FindProperty(ignoredMember);
                if (property != null)
                {
                    if (property.DeclaringType != entityType)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.InheritedPropertyCannotBeIgnored(
                                ignoredMember, entityType.DisplayName(), property.DeclaringType.DisplayName()));
                    }

                    Check.DebugFail("Should never get here...");
                }

                var navigation = entityType.FindNavigation(ignoredMember);
                if (navigation != null)
                {
                    if (navigation.DeclaringEntityType != entityType)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.InheritedPropertyCannotBeIgnored(
                                ignoredMember, entityType.DisplayName(), navigation.DeclaringEntityType.DisplayName()));
                    }

                    Check.DebugFail("Should never get here...");
                }

                var skipNavigation = entityType.FindSkipNavigation(ignoredMember);
                if (skipNavigation != null)
                {
                    if (skipNavigation.DeclaringEntityType != entityType)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.InheritedPropertyCannotBeIgnored(
                                ignoredMember, entityType.DisplayName(), skipNavigation.DeclaringEntityType.DisplayName()));
                    }

                    Check.DebugFail("Should never get here...");
                }

                var serviceProperty = entityType.FindServiceProperty(ignoredMember);
                if (serviceProperty != null)
                {
                    if (serviceProperty.DeclaringEntityType != entityType)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.InheritedPropertyCannotBeIgnored(
                                ignoredMember, entityType.DisplayName(), serviceProperty.DeclaringEntityType.DisplayName()));
                    }

                    Check.DebugFail("Should never get here...");
                }
            }
        }
    }

    /// <summary>
    ///     Validates the mapping/configuration of shadow keys in the model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateNoShadowKeys(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (IConventionEntityType entityType in model.GetEntityTypes())
        {
            foreach (var key in entityType.GetDeclaredKeys())
            {
                if (key.Properties.Any(p => p.IsImplicitlyCreated())
                    && ConfigurationSource.Convention.Overrides(key.GetConfigurationSource())
                    && !key.IsPrimaryKey())
                {
                    var referencingFk = key.GetReferencingForeignKeys().FirstOrDefault();

                    if (referencingFk != null)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.ReferencedShadowKey(
                                referencingFk.DeclaringEntityType.DisplayName()
                                + (referencingFk.DependentToPrincipal == null
                                    ? ""
                                    : "." + referencingFk.DependentToPrincipal.Name),
                                entityType.DisplayName()
                                + (referencingFk.PrincipalToDependent == null
                                    ? ""
                                    : "." + referencingFk.PrincipalToDependent.Name),
                                referencingFk.Properties.Format(includeTypes: true),
                                entityType.FindPrimaryKey()!.Properties.Format(includeTypes: true)));
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Validates the mapping/configuration of mutable in the model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateNoMutableKeys(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            foreach (var key in entityType.GetDeclaredKeys())
            {
                var mutableProperty = key.Properties.FirstOrDefault(p => p.ValueGenerated.HasFlag(ValueGenerated.OnUpdate));
                if (mutableProperty != null)
                {
                    throw new InvalidOperationException(CoreStrings.MutableKeyProperty(mutableProperty.Name));
                }
            }
        }
    }

    /// <summary>
    ///     Validates the mapping/configuration of the model for cycles.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateNoCycles(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var graph = new Multigraph<IEntityType, IForeignKey>();
        foreach (var entityType in model.GetEntityTypes())
        {
            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey == null)
            {
                continue;
            }

            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                var principalType = foreignKey.PrincipalEntityType;
                if (!foreignKey.PrincipalKey.IsPrimaryKey()
                    || !PropertyListComparer.Instance.Equals(foreignKey.Properties, primaryKey.Properties)
                    || foreignKey.PrincipalEntityType.IsAssignableFrom(entityType))
                {
                    continue;
                }

                graph.AddVertex(entityType);
                graph.AddVertex(principalType);
                graph.AddEdge(entityType, principalType, foreignKey);
            }
        }

        graph.TopologicalSort(
            tryBreakEdge: null,
            formatCycle: c => c.Select(d => d.Item1.DisplayName()).Join(" -> "),
            CoreStrings.IdentifyingRelationshipCycle);
    }

    /// <summary>
    ///     Validates that all trackable entity types have a primary key.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateNonNullPrimaryKeys(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var entityTypeWithNullPk
            = model.GetEntityTypes()
                .FirstOrDefault(et => !((IConventionEntityType)et).IsKeyless && et.BaseType == null && et.FindPrimaryKey() == null);

        if (entityTypeWithNullPk != null)
        {
            throw new InvalidOperationException(
                CoreStrings.EntityRequiresKey(entityTypeWithNullPk.DisplayName()));
        }
    }

    /// <summary>
    ///     Validates the mapping/configuration of inheritance in the model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateClrInheritance(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var validEntityTypes = new HashSet<IEntityType>();
        foreach (var entityType in model.GetEntityTypes())
        {
            ValidateClrInheritance(model, entityType, validEntityTypes);
        }
    }

    private static void ValidateClrInheritance(
        IModel model,
        IEntityType entityType,
        HashSet<IEntityType> validEntityTypes)
    {
        if (validEntityTypes.Contains(entityType))
        {
            return;
        }

        if (entityType is { HasSharedClrType: true, BaseType: not null })
        {
            throw new InvalidOperationException(CoreStrings.SharedTypeDerivedType(entityType.DisplayName()));
        }

        if (entityType.FindDeclaredOwnership() == null
            && entityType.BaseType != null)
        {
            var baseClrType = entityType.ClrType.BaseType;
            while (baseClrType != null)
            {
                var baseEntityType = model.FindEntityType(baseClrType);
                if (baseEntityType != null)
                {
                    if (!baseEntityType.IsAssignableFrom(entityType))
                    {
                        throw new InvalidOperationException(
                            CoreStrings.InconsistentInheritance(
                                entityType.DisplayName(), entityType.BaseType.DisplayName(), baseEntityType.DisplayName()));
                    }

                    break;
                }

                baseClrType = baseClrType.BaseType;
            }
        }

        if (!entityType.ClrType.IsInstantiable()
            && !entityType.GetDerivedTypes().Any())
        {
            throw new InvalidOperationException(
                CoreStrings.AbstractLeafEntityType(entityType.DisplayName()));
        }

        validEntityTypes.Add(entityType);
    }

    /// <summary>
    ///     Validates the mapping of inheritance in the model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateInheritanceMapping(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var rootEntityType in model.GetRootEntityTypes())
        {
            ValidateDiscriminatorValues(rootEntityType);
        }
    }

    /// <summary>
    ///     Validates the discriminator and values for all entity types derived from the given one.
    /// </summary>
    /// <param name="rootEntityType">The entity type to validate.</param>
    protected virtual void ValidateDiscriminatorValues(IEntityType rootEntityType)
    {
        var derivedTypes = rootEntityType.GetDerivedTypesInclusive().ToList();
        var discriminatorProperty = rootEntityType.FindDiscriminatorProperty();
        if (discriminatorProperty == null)
        {
            if (derivedTypes.Count == 1)
            {
                return;
            }

            throw new InvalidOperationException(
                CoreStrings.NoDiscriminatorProperty(rootEntityType.DisplayName()));
        }

        var discriminatorValues = new Dictionary<object, IEntityType>(discriminatorProperty.GetKeyValueComparer());

        foreach (var derivedType in derivedTypes)
        {
            if (!derivedType.ClrType.IsInstantiable())
            {
                continue;
            }

            var discriminatorValue = derivedType[CoreAnnotationNames.DiscriminatorValue];
            if (discriminatorValue == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.NoDiscriminatorValue(derivedType.DisplayName()));
            }

            if (!discriminatorProperty.ClrType.IsInstanceOfType(discriminatorValue))
            {
                throw new InvalidOperationException(
                    CoreStrings.DiscriminatorValueIncompatible(
                        discriminatorValue, derivedType.DisplayName(), discriminatorProperty.ClrType.DisplayName()));
            }

            if (discriminatorValues.TryGetValue(discriminatorValue, out var duplicateEntityType))
            {
                throw new InvalidOperationException(
                    CoreStrings.DuplicateDiscriminatorValue(
                        derivedType.DisplayName(), discriminatorValue, duplicateEntityType.DisplayName()));
            }

            discriminatorValues[discriminatorValue] = derivedType;
        }
    }

    /// <summary>
    ///     Validates the mapping/configuration of change tracking in the model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateChangeTrackingStrategy(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var requireFullNotifications = (bool?)model[CoreAnnotationNames.FullChangeTrackingNotificationsRequired] == true;
        foreach (var entityType in model.GetEntityTypes())
        {
            Validate(entityType, requireFullNotifications);
        }

        static void Validate(ITypeBase typeBase, bool requireFullNotifications)
        {
            var errorMessage = TypeBase.CheckChangeTrackingStrategy(
                typeBase, typeBase.GetChangeTrackingStrategy(), requireFullNotifications);

            if (errorMessage != null)
            {
                throw new InvalidOperationException(errorMessage);
            }

            foreach (var complexProperty in typeBase.GetComplexProperties())
            {
                Validate(complexProperty.ComplexType, requireFullNotifications);
            }
        }
    }

    /// <summary>
    ///     Validates the mapping/configuration of ownership in the model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateOwnership(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            var ownerships = entityType.GetForeignKeys().Where(fk => fk.IsOwnership).ToList();
            if (ownerships.Count > 1)
            {
                throw new InvalidOperationException(
                    CoreStrings.MultipleOwnerships(
                        entityType.DisplayName(),
                        string.Join(
                            ", ",
                            ownerships.Select(o => $"'{o.PrincipalEntityType.DisplayName()}.{o.PrincipalToDependent?.Name}'"))));
            }

            if (ownerships.Count == 1)
            {
                Check.DebugAssert(entityType.IsOwned(), $"Expected the entity type {entityType.DisplayName()} to be marked as owned");

                var ownership = ownerships[0];
                if (entityType.BaseType != null)
                {
                    throw new InvalidOperationException(CoreStrings.OwnedDerivedType(entityType.DisplayName()));
                }

                foreach (var referencingFk in entityType.GetReferencingForeignKeys().Where(
                             fk => !fk.IsOwnership
                                 && (fk.PrincipalEntityType != fk.DeclaringEntityType
                                     || !fk.Properties.SequenceEqual(entityType.FindPrimaryKey()!.Properties))
                                 && !Contains(fk.DeclaringEntityType.FindOwnership(), fk)))
                {
                    throw new InvalidOperationException(
                        CoreStrings.PrincipalOwnedType(
                            referencingFk.DeclaringEntityType.DisplayName()
                            + (referencingFk.DependentToPrincipal == null
                                ? ""
                                : "." + referencingFk.DependentToPrincipal.Name),
                            referencingFk.PrincipalEntityType.DisplayName()
                            + (referencingFk.PrincipalToDependent == null
                                ? ""
                                : "." + referencingFk.PrincipalToDependent.Name),
                            entityType.DisplayName()));
                }

                foreach (var fk in entityType.GetDeclaredForeignKeys().Where(
                             fk => fk is { IsOwnership: false, PrincipalToDependent: not null }
                                 && !Contains(fk.DeclaringEntityType.FindOwnership(), fk)))
                {
                    throw new InvalidOperationException(
                        CoreStrings.InverseToOwnedType(
                            fk.PrincipalEntityType.DisplayName(),
                            fk.PrincipalToDependent!.Name,
                            entityType.DisplayName(),
                            ownership.PrincipalEntityType.DisplayName()));
                }
            }
            else if (((IConventionModel)model).IsOwned(entityType.ClrType)
                     || entityType.IsOwned())
            {
                throw new InvalidOperationException(CoreStrings.OwnerlessOwnedType(entityType.DisplayName()));
            }
        }
    }

    private static bool Contains(IForeignKey? inheritedFk, IForeignKey derivedFk)
        => inheritedFk != null
            && inheritedFk.PrincipalEntityType.IsAssignableFrom(derivedFk.PrincipalEntityType)
            && PropertyListComparer.Instance.Equals(inheritedFk.Properties, derivedFk.Properties);

    /// <summary>
    ///     Validates the mapping/configuration of foreign keys in the model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateForeignKeys(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            foreach (var declaredForeignKey in entityType.GetDeclaredForeignKeys())
            {
                if (IsRedundant(declaredForeignKey))
                {
                    logger.RedundantForeignKeyWarning(declaredForeignKey);
                }

                if (entityType.BaseType == null
                    || declaredForeignKey.IsBaseLinking())
                {
                    continue;
                }

                foreach (var generatedProperty in declaredForeignKey.Properties)
                {
                    if (!generatedProperty.ValueGenerated.ForAdd())
                    {
                        continue;
                    }

                    foreach (var inheritedKey in generatedProperty.GetContainingKeys())
                    {
                        if (inheritedKey.DeclaringEntityType != entityType
                            && inheritedKey.Properties.All(p => declaredForeignKey.Properties.Contains(p))
                            && !ContainedInForeignKeyForAllConcreteTypes(inheritedKey.DeclaringEntityType, generatedProperty))
                        {
                            throw new InvalidOperationException(
                                CoreStrings.ForeignKeyPropertyInKey(
                                    generatedProperty.Name,
                                    entityType.DisplayName(),
                                    inheritedKey.Properties.Format(),
                                    inheritedKey.DeclaringEntityType.DisplayName()));
                        }
                    }
                }
            }
        }

        static bool ContainedInForeignKeyForAllConcreteTypes(IEntityType entityType, IProperty property)
            => entityType.ClrType.IsAbstract
                && entityType.GetDerivedTypes().Where(t => !t.ClrType.IsAbstract)
                    .All(
                        d => d.GetForeignKeys()
                            .Any(fk => fk.Properties.Contains(property)));
    }

    /// <summary>
    ///     Returns a value indicating whether the given foreign key is redundant.
    /// </summary>
    /// <param name="foreignKey">A foreign key.</param>
    /// <returns>A value indicating whether the given foreign key is redundant.</returns>
    protected virtual bool IsRedundant(IForeignKey foreignKey)
        => foreignKey.PrincipalEntityType == foreignKey.DeclaringEntityType
            && foreignKey.PrincipalKey.Properties.SequenceEqual(foreignKey.Properties);

    /// <summary>
    ///     Validates the mapping/configuration of properties mapped to fields in the model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateFieldMapping(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            Validate(entityType);
        }

        static void Validate(ITypeBase typeBase)
        {
            var properties = new HashSet<IPropertyBase>(
                typeBase
                    .GetDeclaredMembers()
                    .Where(p => !p.IsShadowProperty() && !p.IsIndexerProperty()));

            var fieldProperties = new Dictionary<FieldInfo, IPropertyBase>();
            foreach (var propertyBase in properties)
            {
                var field = propertyBase.FieldInfo;
                if (field == null)
                {
                    continue;
                }

                if (fieldProperties.TryGetValue(field, out var conflictingProperty))
                {
                    throw new InvalidOperationException(
                        CoreStrings.ConflictingFieldProperty(
                            propertyBase.DeclaringType.DisplayName(),
                            propertyBase.Name,
                            field.Name,
                            conflictingProperty.DeclaringType.DisplayName(),
                            conflictingProperty.Name));
                }

                fieldProperties.Add(field, propertyBase);
            }

            var constructorBinding = typeBase.ConstructorBinding;
            if (constructorBinding != null)
            {
                foreach (var consumedProperty in constructorBinding.ParameterBindings.SelectMany(p => p.ConsumedProperties))
                {
                    properties.Remove(consumedProperty);
                }
            }

            foreach (var propertyBase in properties)
            {
                if (!propertyBase.TryGetMemberInfo(
                        forMaterialization: true,
                        forSet: true,
                        memberInfo: out _,
                        errorMessage: out var errorMessage))
                {
                    throw new InvalidOperationException(errorMessage);
                }

                if (!propertyBase.TryGetMemberInfo(
                        forMaterialization: false,
                        forSet: true,
                        memberInfo: out _,
                        errorMessage: out errorMessage))
                {
                    throw new InvalidOperationException(errorMessage);
                }

                if (!propertyBase.TryGetMemberInfo(
                        forMaterialization: false,
                        forSet: false,
                        memberInfo: out _,
                        errorMessage: out errorMessage))
                {
                    throw new InvalidOperationException(errorMessage);
                }
            }

            foreach (var complexProperty in typeBase.GetDeclaredComplexProperties())
            {
                Validate(complexProperty.ComplexType);
            }
        }
    }

    /// <summary>
    ///     Validates the type mapping of properties the model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateTypeMappings(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            Validate(entityType, logger);
        }

        static void Validate(ITypeBase typeBase, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var property in typeBase.GetDeclaredProperties())
            {
                var converter = property.GetValueConverter();
                if (converter != null
                    && property[CoreAnnotationNames.ValueComparer] == null)
                {
                    var type = converter.ModelClrType;
                    if (type != typeof(string)
                        && !(type == typeof(byte[]) && property.IsKey()) // Already special-cased elsewhere
                        && !property.IsForeignKey()
                        && type.TryGetSequenceType() != null)
                    {
                        logger.CollectionWithoutComparer(property);
                    }
                }

                if (property.IsKey()
                    || property.IsForeignKey()
                    || property.IsUniqueIndex())
                {
                    _ = property.GetCurrentValueComparer(); // Will throw if there is no way to compare
                }

                var providerComparer = property.GetProviderValueComparer();
                if (providerComparer == null)
                {
                    continue;
                }

                var typeMapping = property.GetTypeMapping();
                var actualProviderClrType = (typeMapping.Converter?.ProviderClrType ?? typeMapping.ClrType).UnwrapNullableType();

                if (providerComparer.Type.UnwrapNullableType() != actualProviderClrType)
                {
                    throw new InvalidOperationException(
                        CoreStrings.ComparerPropertyMismatch(
                            providerComparer.Type.ShortDisplayName(),
                            property.DeclaringType.DisplayName(),
                            property.Name,
                            actualProviderClrType.ShortDisplayName()));
                }
            }

            foreach (var complexProperty in typeBase.GetDeclaredComplexProperties())
            {
                Validate(complexProperty.ComplexType, logger);
            }
        }
    }

    /// <summary>
    ///     Validates the mapping of primitive collection properties the model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidatePrimitiveCollections(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            Validate(entityType, logger);
        }

        static void Validate(ITypeBase typeBase, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var property in typeBase.GetDeclaredProperties())
            {
                var elementClrType = property.GetElementType()?.ClrType;
                if (property is { IsPrimitiveCollection: true, ClrType.IsArray: false })
                {
                    if (property.ClrType.IsSealed && property.ClrType.TryGetElementType(typeof(IList<>)) == null)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.BadListType(
                                property.ClrType.ShortDisplayName(),
                                typeof(IList<>).MakeGenericType(elementClrType!).ShortDisplayName()));
                    }

                    if (property.ClrType.IsGenericType
                        && (property.ClrType.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>)
                            || property.ClrType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)))
                    {
                        throw new InvalidOperationException(CoreStrings.ReadOnlyListType(property.ClrType.ShortDisplayName()));
                    }
                }
            }

            foreach (var complexProperty in typeBase.GetDeclaredComplexProperties())
            {
                Validate(complexProperty.ComplexType, logger);
            }
        }
    }

    /// <summary>
    ///     Validates the mapping/configuration of query filters in the model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateQueryFilters(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            if (entityType.GetQueryFilter() != null)
            {
                if (entityType.BaseType != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.BadFilterDerivedType(
                            entityType.GetQueryFilter(),
                            entityType.DisplayName(),
                            entityType.GetRootType().DisplayName()));
                }

                if (entityType.IsOwned())
                {
                    throw new InvalidOperationException(
                        CoreStrings.BadFilterOwnedType(entityType.GetQueryFilter(), entityType.DisplayName()));
                }
            }

            if (!entityType.IsOwned())
            {
                // Owned type doesn't allow to define query filter
                // So we don't check navigations there. We assume the owner will propagate filtering
                var requiredNavigationWithQueryFilter = entityType
                    .GetNavigations()
                    .FirstOrDefault(
                        n => n is { IsCollection: false, ForeignKey.IsRequired: true, IsOnDependent: true }
                            && n.ForeignKey.PrincipalEntityType.GetRootType().GetQueryFilter() != null
                            && n.ForeignKey.DeclaringEntityType.GetRootType().GetQueryFilter() == null);

                if (requiredNavigationWithQueryFilter != null)
                {
                    logger.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning(
                        requiredNavigationWithQueryFilter.ForeignKey);
                }
            }
        }
    }

    /// <summary>
    ///     Validates the mapping/configuration of data (e.g. seed data) in the model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateData(IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var identityMaps = new Dictionary<IKey, IIdentityMap>();
        var sensitiveDataLogged = logger.ShouldLogSensitiveData();

        foreach (var entityType in model.GetEntityTypes())
        {
            var key = entityType.FindPrimaryKey();
            if (key == null)
            {
                if (entityType.GetSeedData().Any())
                {
                    throw new InvalidOperationException(CoreStrings.SeedKeylessEntity(entityType.DisplayName()));
                }

                continue;
            }

            IIdentityMap? identityMap = null;
            foreach (var seedDatum in entityType.GetSeedData())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (!seedDatum.TryGetValue(property.Name, out var value)
                        || value == null)
                    {
                        if (!property.IsNullable
                            && ((!property.RequiresValueGenerator()
                                    && !property.ValueGenerated.ForAdd())
                                || property.IsPrimaryKey()))
                        {
                            throw new InvalidOperationException(
                                CoreStrings.SeedDatumMissingValue(entityType.DisplayName(), property.Name));
                        }
                    }
                    else if (property.RequiresValueGenerator()
                             && property.IsPrimaryKey()
                             && property.ClrType.IsDefaultValue(value))
                    {
                        if (property.ClrType.IsSignedInteger())
                        {
                            throw new InvalidOperationException(
                                CoreStrings.SeedDatumSignedNumericValue(entityType.DisplayName(), property.Name));
                        }

                        throw new InvalidOperationException(
                            CoreStrings.SeedDatumDefaultValue(
                                entityType.DisplayName(), property.Name, property.ClrType.GetDefaultValue()));
                    }
                    else if (!property.ClrType.IsAssignableFrom(value.GetType().GetTypeInfo()))
                    {
                        if (sensitiveDataLogged)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.SeedDatumIncompatibleValueSensitive(
                                    entityType.DisplayName(), value, property.Name, property.ClrType.DisplayName()));
                        }

                        throw new InvalidOperationException(
                            CoreStrings.SeedDatumIncompatibleValue(
                                entityType.DisplayName(), property.Name, property.ClrType.DisplayName()));
                    }
                }

                var keyValues = new object[key.Properties.Count];
                for (var i = 0; i < key.Properties.Count; i++)
                {
                    keyValues[i] = seedDatum[key.Properties[i].Name]!;
                }

                foreach (var navigation in entityType.GetNavigations().Concat<INavigationBase>(entityType.GetSkipNavigations()))
                {
                    if (seedDatum.TryGetValue(navigation.Name, out var value)
                        && ((navigation.IsCollection && value is IEnumerable collection && collection.Any())
                            || (!navigation.IsCollection && value != null)))
                    {
                        var foreignKey = navigation is INavigation nav
                            ? nav.ForeignKey
                            : ((ISkipNavigation)navigation).ForeignKey;
                        if (sensitiveDataLogged)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.SeedDatumNavigationSensitive(
                                    entityType.DisplayName(),
                                    string.Join(", ", key.Properties.Select((p, i) => p.Name + ":" + keyValues[i])),
                                    navigation.Name,
                                    foreignKey.DeclaringEntityType.DisplayName(),
                                    foreignKey.Properties.Format()));
                        }

                        throw new InvalidOperationException(
                            CoreStrings.SeedDatumNavigation(
                                entityType.DisplayName(),
                                navigation.Name,
                                foreignKey.DeclaringEntityType.DisplayName(),
                                foreignKey.Properties.Format()));
                    }
                }

                foreach (var complexProperty in entityType.GetComplexProperties())
                {
                    if (seedDatum.TryGetValue(complexProperty.Name, out var value)
                        && ((complexProperty.IsCollection && value is IEnumerable collection && collection.Any())
                            || (!complexProperty.IsCollection && value != complexProperty.Sentinel)))
                    {
                        if (sensitiveDataLogged)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.SeedDatumComplexPropertySensitive(
                                    entityType.DisplayName(),
                                    string.Join(", ", key.Properties.Select((p, i) => p.Name + ":" + keyValues[i])),
                                    complexProperty.Name));
                        }

                        throw new InvalidOperationException(
                            CoreStrings.SeedDatumComplexProperty(
                                entityType.DisplayName(),
                                complexProperty.Name));
                    }
                }

                if (identityMap == null)
                {
                    if (!identityMaps.TryGetValue(key, out identityMap))
                    {
                        identityMap = ((IRuntimeKey)key).GetIdentityMapFactory()(sensitiveDataLogged);
                        identityMaps[key] = identityMap;
                    }
                }

                var entry = identityMap.TryGetEntry(keyValues);
                if (entry != null)
                {
                    if (sensitiveDataLogged)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.SeedDatumDuplicateSensitive(
                                entityType.DisplayName(),
                                string.Join(", ", key.Properties.Select((p, i) => p.Name + ":" + keyValues[i]))));
                    }

                    throw new InvalidOperationException(
                        CoreStrings.SeedDatumDuplicate(
                            entityType.DisplayName(), key.Properties.Format()));
                }

                entry = new InternalEntityEntry(null!, entityType, seedDatum);
                identityMap.Add(keyValues, entry);
            }
        }
    }

    /// <summary>
    ///     Validates triggers.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateTriggers(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
    }

    /// <summary>
    ///     Logs all shadow properties that were created because there was no matching CLR member.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void LogShadowProperties(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (IConventionEntityType entityType in model.GetEntityTypes())
        {
            foreach (var property in entityType.GetDeclaredProperties())
            {
                if (property.IsImplicitlyCreated())
                {
                    var uniquifiedAnnotation = property.FindAnnotation(CoreAnnotationNames.PreUniquificationName);
                    if (uniquifiedAnnotation != null
                        && property.IsForeignKey())
                    {
                        logger.ShadowForeignKeyPropertyCreated((IProperty)property, (string)uniquifiedAnnotation.Value!);
                    }
                    else
                    {
                        logger.ShadowPropertyCreated((IProperty)property);
                    }
                }
            }
        }
    }
}
