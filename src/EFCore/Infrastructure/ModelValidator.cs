// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     The validator that enforces core rules common for all providers.
/// </summary>
/// <param name="dependencies">Parameter object containing dependencies for this service.</param>
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
public class ModelValidator(ModelValidatorDependencies dependencies) : IModelValidator
{
    private static readonly IEnumerable<string> DictionaryProperties =
        typeof(IDictionary<string, object>).GetRuntimeProperties().Select(e => e.Name);

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ModelValidatorDependencies Dependencies { get; } = dependencies;

    /// <inheritdoc />
    public virtual void Validate(IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var validEntityTypes = new HashSet<IEntityType>();
        var identityMaps = new Dictionary<IKey, IIdentityMap>();
        var identifyingFkGraph = new Multigraph<IEntityType, IForeignKey>();
        var sensitiveDataLogged = logger.ShouldLogSensitiveData();

        foreach (var entityType in model.GetEntityTypes())
        {
            ValidateEntityType(entityType, logger);
            ValidateClrInheritance(entityType, validEntityTypes);
            ValidateData(entityType, identityMaps, sensitiveDataLogged, logger);
            
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

                identifyingFkGraph.AddVertex(entityType);
                identifyingFkGraph.AddVertex(principalType);
                identifyingFkGraph.AddEdge(entityType, principalType, foreignKey);
            }
        }

        ValidateNoIdentifyingRelationshipCycles(identifyingFkGraph);
    }

    private static void ValidateNoIdentifyingRelationshipCycles(Multigraph<IEntityType, IForeignKey> graph)
    {
        graph.TopologicalSort(
            tryBreakEdge: null,
            formatCycle: c => c.Select(d => d.Item1.DisplayName()).Join(" -> "),
            CoreStrings.IdentifyingRelationshipCycle);
    }

    /// <summary>
    ///     Validates a single entity type.
    /// </summary>
    /// <param name="entityType">The entity type to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateEntityType(
        IEntityType entityType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        ValidateEntityClrType(entityType, logger);
        ValidateChangeTrackingStrategy(entityType, logger);
        ValidateIgnoredMembers(entityType, logger);
        ValidatePropertyMapping(entityType, logger);
        ValidateOwnership(entityType, logger);
        ValidateNonNullPrimaryKey(entityType, logger);
        ValidateInheritanceMapping(entityType, logger);
        ValidateFieldMapping(entityType, logger);
        ValidateQueryFilters(entityType, logger);

        foreach (var property in entityType.GetDeclaredProperties())
        {
            ValidateProperty(property, entityType, logger);
        }

        foreach (var skipNavigation in entityType.GetDeclaredSkipNavigations())
        {
            ValidateSkipNavigation(skipNavigation, logger);
        }

        foreach (var foreignKey in entityType.GetDeclaredForeignKeys())
        {
            ValidateForeignKey(foreignKey, logger);
        }

        foreach (var key in entityType.GetDeclaredKeys())
        {
            ValidateKey(key, logger);
        }

        foreach (var index in entityType.GetDeclaredIndexes())
        {
            ValidateIndex(index, logger);
        }

        foreach (var trigger in entityType.GetDeclaredTriggers())
        {
            ValidateTrigger(trigger, entityType, logger);
        }

        foreach (var complexProperty in entityType.GetDeclaredComplexProperties())
        {
            ValidateComplexProperty(complexProperty, logger);
        }

        LogShadowProperties(entityType, logger);
    }

    /// <summary>
    ///     Validates inheritance mapping for an entity type.
    /// </summary>
    /// <param name="entityType">The entity type to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateInheritanceMapping(
        IEntityType entityType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        // For root entity types, validate discriminator values
        if (entityType.BaseType == null)
        {
            ValidateDiscriminatorValues(entityType);
        }
    }

    /// <summary>
    ///     Validates a single property.
    /// </summary>
    /// <param name="property">The property to validate.</param>
    /// <param name="structuralType">The structural type containing the property.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateProperty(
        IProperty property,
        ITypeBase structuralType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        ValidateTypeMapping(property, logger);
        ValidatePrimitiveCollection(property, logger);
    }

    /// <summary>
    ///     Validates a single complex property and its nested members.
    /// </summary>
    /// <param name="complexProperty">The complex property to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateComplexProperty(
        IComplexProperty complexProperty,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var complexType = complexProperty.ComplexType;

        ValidateChangeTrackingStrategy(complexType, logger);

        foreach (var property in complexType.GetDeclaredProperties())
        {
            ValidateProperty(property, complexType, logger);
        }

        foreach (var nestedComplexProperty in complexType.GetDeclaredComplexProperties())
        {
            ValidateComplexProperty(nestedComplexProperty, logger);
        }
    }

    /// <summary>
    ///     Validates a single index.
    /// </summary>
    /// <param name="index">The index to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateIndex(
        IIndex index,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
    }

    /// <summary>
    ///     Validates a single key.
    /// </summary>
    /// <param name="key">The key to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateKey(
        IKey key,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        ValidateShadowKey(key, logger);
        ValidateMutableKey(key, logger);
    }

    /// <summary>
    ///     Validates that a one-to-one relationship has an unambiguous principal end.
    /// </summary>
    /// <param name="foreignKey">The foreign key to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateAmbiguousOneToOneRelationship(
        IForeignKey foreignKey,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
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

    /// <summary>
    ///     Validates a skip navigation.
    /// </summary>
    /// <param name="skipNavigation">The skip navigation to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSkipNavigation(
        ISkipNavigation skipNavigation,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        ValidateSkipNavigationIsCollection(skipNavigation, logger);
        ValidateSkipNavigationForeignKey(skipNavigation, logger);
        ValidateSkipNavigationInverse(skipNavigation, logger);
    }

    /// <summary>
    ///     Validates that a skip navigation is a collection navigation.
    /// </summary>
    /// <param name="skipNavigation">The skip navigation to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSkipNavigationIsCollection(
        ISkipNavigation skipNavigation,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (!skipNavigation.IsCollection)
        {
            throw new InvalidOperationException(
                CoreStrings.SkipNavigationNonCollection(
                    skipNavigation.Name, skipNavigation.DeclaringEntityType.DisplayName()));
        }
    }

    /// <summary>
    ///     Validates that a skip navigation has a foreign key configured.
    /// </summary>
    /// <param name="skipNavigation">The skip navigation to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSkipNavigationForeignKey(
        ISkipNavigation skipNavigation,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (skipNavigation.ForeignKey == null)
        {
            throw new InvalidOperationException(
                CoreStrings.SkipNavigationNoForeignKey(
                    skipNavigation.Name, skipNavigation.DeclaringEntityType.DisplayName()));
        }
    }

    /// <summary>
    ///     Validates that a skip navigation has an inverse navigation configured.
    /// </summary>
    /// <param name="skipNavigation">The skip navigation to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSkipNavigationInverse(
        ISkipNavigation skipNavigation,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (skipNavigation.Inverse == null)
        {
            throw new InvalidOperationException(
                CoreStrings.SkipNavigationNoInverse(
                    skipNavigation.Name, skipNavigation.DeclaringEntityType.DisplayName()));
        }
    }

    /// <summary>
    ///     Validates property mappings for a given type.
    /// </summary>
    /// <param name="structuralType">The type base to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidatePropertyMapping(
        ITypeBase structuralType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var conventionTypeBase = (IConventionTypeBase)structuralType;
        var conventionModel = (IConventionModel)structuralType.Model;

        var unmappedProperty = conventionTypeBase.GetDeclaredProperties().FirstOrDefault(p
            => (!ConfigurationSource.Convention.Overrides(p.GetConfigurationSource())
                // Use a better condition for non-persisted properties when issue #14121 is implemented
                || !p.IsImplicitlyCreated())
            && p.FindTypeMapping() == null);

        if (unmappedProperty != null)
        {
            ThrowPropertyNotMappedException(
                (unmappedProperty.GetValueConverter()?.ProviderClrType ?? unmappedProperty.ClrType).ShortDisplayName(),
                structuralType,
                (IProperty)unmappedProperty);
        }

        foreach (var complexProperty in conventionTypeBase.GetDeclaredComplexProperties())
        {
            ValidatePropertyMapping((IComplexProperty)complexProperty, logger);
            ValidatePropertyMapping((ITypeBase)complexProperty.ComplexType, logger);
        }

        if (structuralType.ClrType == Model.DefaultPropertyBagType)
        {
            return;
        }

        var runtimeProperties = structuralType.GetRuntimeProperties();
        var clrProperties = new HashSet<string>(StringComparer.Ordinal);
        clrProperties.UnionWith(
            runtimeProperties.Values
                .Where(pi => pi.IsCandidateProperty(needsWrite: false))
                .Select(pi => pi.GetSimpleMemberName()));

        clrProperties.ExceptWith(structuralType.GetMembers().Select(p => p.Name));

        if (structuralType.IsPropertyBag)
        {
            clrProperties.ExceptWith(DictionaryProperties);
        }

        if (clrProperties.Count <= 0)
        {
            return;
        }

        foreach (var clrPropertyName in clrProperties)
        {
            if (conventionTypeBase.FindIgnoredConfigurationSource(clrPropertyName) != null)
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

            var isAdHoc = Equals(conventionModel.FindAnnotation(CoreAnnotationNames.AdHocModel)?.Value, true);
            if (targetType != null)
            {
                var targetShared = conventionModel.IsShared(targetType);
                targetOwned ??= IsOwned(targetType, structuralType.Model);

                if (conventionTypeBase is not IEntityType entityType)
                {
                    if (!((IReadOnlyComplexType)structuralType).IsContainedBy(targetType))
                    {
                        throw new InvalidOperationException(
                            CoreStrings.NavigationNotAddedComplexType(
                                structuralType.DisplayName(), clrProperty.Name, propertyType.ShortDisplayName()));
                    }

                    continue;
                }

                // ReSharper disable CheckForReferenceEqualityInstead.1
                // ReSharper disable CheckForReferenceEqualityInstead.3
                if ((isAdHoc
                        || !((IConventionEntityType)entityType).IsKeyless
                        || targetSequenceType == null)
                    && entityType.GetDerivedTypes().All(dt
                        => dt.GetDeclaredNavigations().FirstOrDefault(n => n.Name == clrProperty.GetSimpleMemberName())
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
                                structuralType.DisplayName() + "." + clrProperty.Name, targetType.ShortDisplayName()));
                    }

                    if (targetShared)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.NonConfiguredNavigationToSharedType(clrProperty.Name, structuralType.DisplayName()));
                    }

                    throw new InvalidOperationException(
                        isAdHoc
                            ? CoreStrings.NavigationNotAddedAdHoc(
                                structuralType.DisplayName(), clrProperty.Name, propertyType.ShortDisplayName())
                            : CoreStrings.NavigationNotAdded(
                                structuralType.DisplayName(), clrProperty.Name, propertyType.ShortDisplayName()));
                }

                // ReSharper restore CheckForReferenceEqualityInstead.3
                // ReSharper restore CheckForReferenceEqualityInstead.1
            }
            else if (targetSequenceType == null && propertyType.IsInterface
                     || targetSequenceType?.IsInterface == true)
            {
                throw new InvalidOperationException(
                    CoreStrings.InterfacePropertyNotAdded(
                        structuralType.DisplayName(), clrProperty.Name, propertyType.ShortDisplayName()));
            }
            else
            {
                throw new InvalidOperationException(
                    isAdHoc
                        ? CoreStrings.PropertyNotAddedAdHoc(
                            structuralType.DisplayName(), clrProperty.Name, propertyType.ShortDisplayName())
                        : CoreStrings.PropertyNotAdded(
                            structuralType.DisplayName(), clrProperty.Name, propertyType.ShortDisplayName()));
            }
        }
    }

    /// <summary>
    ///     Validates property mappings for a given complex property.
    /// </summary>
    /// <param name="complexProperty">The complex property to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidatePropertyMapping(
        IComplexProperty complexProperty,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var structuralType = complexProperty.DeclaringType;
        var targetType = complexProperty.ComplexType;

        // Issue #31243: Shadow complex properties are not supported
        if (complexProperty.IsShadowProperty())
        {
            throw new InvalidOperationException(
                CoreStrings.ComplexPropertyShadow(structuralType.DisplayName(), complexProperty.Name));
        }

        if (complexProperty.IsIndexerProperty())
        {
            throw new InvalidOperationException(
                CoreStrings.ComplexPropertyIndexer(structuralType.DisplayName(), complexProperty.Name));
        }

        if (!targetType.GetMembers().Any())
        {
            throw new InvalidOperationException(
                CoreStrings.EmptyComplexType(targetType.DisplayName()));
        }

        if (!complexProperty.IsCollection && complexProperty.ClrType.IsGenericType)
        {
            var genericTypeDefinition = complexProperty.ClrType.GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(List<>)
                || genericTypeDefinition == typeof(HashSet<>)
                || genericTypeDefinition == typeof(Collection<>)
                || genericTypeDefinition == typeof(ObservableCollection<>))
            {
                logger.AccidentalComplexPropertyCollection((IComplexProperty)complexProperty);
            }
        }

        // Issue #31411: Complex value type collections are not supported
        if (complexProperty.IsCollection && targetType.ClrType.IsValueType)
        {
            throw new InvalidOperationException(
                CoreStrings.ComplexValueTypeCollection(structuralType.DisplayName(), complexProperty.Name));
        }

        var nonDiscriminatorShadowProperty = targetType.GetDeclaredProperties()
            .FirstOrDefault(p => p.IsShadowProperty() && p != targetType.FindDiscriminatorProperty());
        if (nonDiscriminatorShadowProperty is not null)
        {
            throw targetType.ClrType.IsValueType
                // Issue #35337: Shadow properties on value type complex types are not supported
                ? new InvalidOperationException(
                    CoreStrings.ComplexValueTypeShadowProperty(targetType.DisplayName(), nonDiscriminatorShadowProperty.Name))
                // Issue #35613: Shadow properties on all complex types are not supported
                : new InvalidOperationException(
                    CoreStrings.ComplexTypeShadowProperty(targetType.DisplayName(), nonDiscriminatorShadowProperty.Name));
        }
    }

    /// <summary>
    ///     Throws an <see cref="InvalidOperationException" /> with a message containing provider-specific information, when
    ///     available, indicating possible reasons why the property cannot be mapped.
    /// </summary>
    /// <param name="propertyType">The property CLR type.</param>
    /// <param name="structuralType">The structural type.</param>
    /// <param name="unmappedProperty">The property.</param>
    protected virtual void ThrowPropertyNotMappedException(
        string propertyType,
        ITypeBase structuralType,
        IProperty unmappedProperty)
        => throw new InvalidOperationException(
            CoreStrings.PropertyNotMapped(
                propertyType,
                structuralType.DisplayName(),
                unmappedProperty.Name));

    /// <summary>
    ///     Returns a value indicating whether that target CLR type would correspond to an owned entity type.
    /// </summary>
    /// <param name="targetType">The target CLR type.</param>
    /// <param name="model">The model.</param>
    /// <returns><see langword="true" /> if the given CLR type corresponds to an owned entity type.</returns>
    protected virtual bool IsOwned(Type targetType, IModel model)
    {
        var conventionModel = (IConventionModel)model;
        return conventionModel.FindIsOwnedConfigurationSource(targetType) != null
            || conventionModel.FindEntityTypes(targetType).Any(t => t.IsOwned());
    }

    /// <summary>
    ///     Validates that no attempt is made to ignore inherited properties on an entity type.
    /// </summary>
    /// <param name="entityType">The entity type to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateIgnoredMembers(
        IEntityType entityType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (entityType is not IConventionEntityType conventionEntityType)
        {
            return;
        }

        foreach (var ignoredMember in conventionEntityType.GetIgnoredMembers())
        {
            if (conventionEntityType.FindIgnoredConfigurationSource(ignoredMember) != ConfigurationSource.Explicit)
            {
                continue;
            }

            var property = entityType.FindProperty(ignoredMember);
            if (property != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.InheritedPropertyCannotBeIgnored(
                        ignoredMember, entityType.DisplayName(), property.DeclaringType.DisplayName()));
            }

            var navigation = entityType.FindNavigation(ignoredMember);
            if (navigation != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.InheritedPropertyCannotBeIgnored(
                        ignoredMember, entityType.DisplayName(), navigation.DeclaringEntityType.DisplayName()));
            }

            var skipNavigation = entityType.FindSkipNavigation(ignoredMember);
            if (skipNavigation != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.InheritedPropertyCannotBeIgnored(
                        ignoredMember, entityType.DisplayName(), skipNavigation.DeclaringEntityType.DisplayName()));
            }

            var serviceProperty = entityType.FindServiceProperty(ignoredMember);
            if (serviceProperty != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.InheritedPropertyCannotBeIgnored(
                        ignoredMember, entityType.DisplayName(), serviceProperty.DeclaringEntityType.DisplayName()));
            }
        }
    }

    /// <summary>
    ///     Validates that a key doesn't have shadow properties inappropriately.
    /// </summary>
    /// <param name="key">The key to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateShadowKey(
        IKey key,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (key is IConventionKey conventionKey
            && key.Properties.Any(p => p.IsShadowProperty())
            && ConfigurationSource.Convention.Overrides(conventionKey.GetConfigurationSource())
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
                        key.DeclaringEntityType.DisplayName()
                        + (referencingFk.PrincipalToDependent == null
                            ? ""
                            : "." + referencingFk.PrincipalToDependent.Name),
                        referencingFk.Properties.Format(includeTypes: true),
                        key.DeclaringEntityType.FindPrimaryKey()!.Properties.Format(includeTypes: true)));
            }
        }
    }

    /// <summary>
    ///     Validates that a key doesn't have mutable properties.
    /// </summary>
    /// <param name="key">The key to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateMutableKey(
        IKey key,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var mutableProperty = key.Properties.FirstOrDefault(p => p.ValueGenerated.HasFlag(ValueGenerated.OnUpdate));
        if (mutableProperty != null)
        {
            throw new InvalidOperationException(CoreStrings.MutableKeyProperty(mutableProperty.Name));
        }
    }

    /// <summary>
    ///     Validates that a trackable entity type has a primary key.
    /// </summary>
    /// <param name="entityType">The entity type to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateNonNullPrimaryKey(
        IEntityType entityType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (!((IConventionEntityType)entityType).IsKeyless
            && entityType.BaseType == null
            && entityType.FindPrimaryKey() == null)
        {
            throw new InvalidOperationException(
                CoreStrings.EntityRequiresKey(entityType.DisplayName()));
        }
    }

    private static void ValidateClrInheritance(
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
                var baseEntityType = entityType.Model.FindEntityType(baseClrType);
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
    ///     Validates the discriminator and values for all entity types derived from the given one.
    /// </summary>
    /// <param name="rootEntityType">The entity type to validate.</param>
    protected virtual void ValidateDiscriminatorValues(IEntityType rootEntityType)
    {
        var derivedTypes = rootEntityType.GetDerivedTypesInclusive();
        var discriminatorProperty = rootEntityType.FindDiscriminatorProperty();
        if (discriminatorProperty == null)
        {
            if (!derivedTypes.Skip(1).Any())
            {
                foreach (var complexProperty in rootEntityType.GetDeclaredComplexProperties())
                {
                    ValidateDiscriminatorValues(complexProperty.ComplexType);
                }

                return;
            }

            throw new InvalidOperationException(
                CoreStrings.NoDiscriminatorProperty(rootEntityType.DisplayName()));
        }

        var discriminatorValues = new Dictionary<object, IEntityType>(discriminatorProperty.GetKeyValueComparer());
        foreach (var derivedType in derivedTypes)
        {
            foreach (var complexProperty in derivedType.GetDeclaredComplexProperties())
            {
                ValidateDiscriminatorValues(complexProperty.ComplexType);
            }

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
    ///     Validates the discriminator and values for the given complex type and nested ones.
    /// </summary>
    /// <param name="complexType">The entity type to validate.</param>
    protected virtual void ValidateDiscriminatorValues(IComplexType complexType)
    {
        foreach (var complexProperty in complexType.GetComplexProperties())
        {
            if (complexProperty.IsCollection)
            {
                continue;
            }

            ValidateDiscriminatorValues(complexProperty.ComplexType);
        }

        var derivedTypes = complexType.GetDerivedTypesInclusive();
        var discriminatorProperty = complexType.FindDiscriminatorProperty();

        if (discriminatorProperty != null
            && (complexType.ComplexProperty.IsCollection
                || complexType is IRuntimeTypeBase { ContainingEntryType: IComplexType }))
        {
            var containingComplexType = complexType is IRuntimeTypeBase { ContainingEntryType: IComplexType ct } ? ct : complexType;
            throw new InvalidOperationException(
                CoreStrings.DiscriminatorPropertyNotAllowedOnComplexCollection(
                    complexType.DisplayName(), containingComplexType.DisplayName()));
        }

        if (discriminatorProperty == null)
        {
            if (!derivedTypes.Skip(1).Any())
            {
                return;
            }

            throw new InvalidOperationException(
                CoreStrings.NoDiscriminatorProperty(complexType.DisplayName()));
        }

        var discriminatorValues = new Dictionary<object, IComplexType>(discriminatorProperty.GetKeyValueComparer());

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
    private void ValidateChangeTrackingStrategy(
        ITypeBase structuralType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var requireFullNotifications =
            (bool?)structuralType.Model[CoreAnnotationNames.FullChangeTrackingNotificationsRequired] == true;
        var errorMessage = TypeBase.CheckChangeTrackingStrategy(
            structuralType, structuralType.GetChangeTrackingStrategy(), requireFullNotifications);

        if (errorMessage != null)
        {
            throw new InvalidOperationException(errorMessage);
        }
    }

    /// <summary>
    ///     Validates ownership configuration for an entity type.
    /// </summary>
    /// <param name="entityType">The entity type to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateOwnership(
        IEntityType entityType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
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

            foreach (var referencingFk in entityType.GetReferencingForeignKeys().Where(fk => !fk.IsOwnership
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

            foreach (var fk in entityType.GetDeclaredForeignKeys().Where(fk
                         => fk is { IsOwnership: false, PrincipalToDependent: not null }
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
        else if (((IConventionModel)entityType.Model).IsOwned(entityType.ClrType)
                 || entityType.IsOwned())
        {
            throw new InvalidOperationException(CoreStrings.OwnerlessOwnedType(entityType.DisplayName()));
        }
    }

    private static bool Contains(IForeignKey? inheritedFk, IForeignKey derivedFk)
        => inheritedFk != null
            && inheritedFk.PrincipalEntityType.IsAssignableFrom(derivedFk.PrincipalEntityType)
            && PropertyListComparer.Instance.Equals(inheritedFk.Properties, derivedFk.Properties);

    /// <summary>
    ///     Validates a foreign key.
    /// </summary>
    /// <param name="foreignKey">The foreign key to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateForeignKey(
        IForeignKey foreignKey,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        ValidateAmbiguousOneToOneRelationship(foreignKey, logger);
        ValidateRedundantForeignKey(foreignKey, logger);
        ValidateForeignKeyPropertyInKey(foreignKey, logger);
    }

    /// <summary>
    ///     Validates that a foreign key is not redundant and logs a warning if it is.
    /// </summary>
    /// <param name="foreignKey">The foreign key to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateRedundantForeignKey(
        IForeignKey foreignKey,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (IsRedundant(foreignKey))
        {
            logger.RedundantForeignKeyWarning(foreignKey);
        }
    }

    /// <summary>
    ///     Validates that foreign key properties are not part of an inherited key that would cause issues.
    /// </summary>
    /// <param name="foreignKey">The foreign key to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateForeignKeyPropertyInKey(
        IForeignKey foreignKey,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (foreignKey.DeclaringEntityType.BaseType == null
            || foreignKey.IsBaseLinking())
        {
            return;
        }

        foreach (var generatedProperty in foreignKey.Properties)
        {
            if (!generatedProperty.ValueGenerated.ForAdd())
            {
                continue;
            }

            foreach (var inheritedKey in generatedProperty.GetContainingKeys())
            {
                if (inheritedKey.DeclaringEntityType != foreignKey.DeclaringEntityType
                    && inheritedKey.Properties.All(p => foreignKey.Properties.Contains(p))
                    && !ContainedInForeignKeyForAllConcreteTypes(inheritedKey.DeclaringEntityType, generatedProperty))
                {
                    throw new InvalidOperationException(
                        CoreStrings.ForeignKeyPropertyInKey(
                            generatedProperty.Name,
                            foreignKey.DeclaringEntityType.DisplayName(),
                            inheritedKey.Properties.Format(),
                            inheritedKey.DeclaringEntityType.DisplayName()));
                }
            }
        }

        static bool ContainedInForeignKeyForAllConcreteTypes(IEntityType entityType, IProperty property)
            => entityType.ClrType.IsAbstract
                && entityType.GetDerivedTypes().Where(t => !t.ClrType.IsAbstract)
                    .All(d => d.GetForeignKeys()
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
    ///     Validates field mapping configuration for a structural type.
    /// </summary>
    /// <param name="structuralType">The structural type to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateFieldMapping(
        ITypeBase structuralType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var properties = new HashSet<IPropertyBase>(
            structuralType
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

        var constructorBinding = structuralType.ConstructorBinding;
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

        foreach (var complexProperty in structuralType.GetDeclaredComplexProperties())
        {
            if (complexProperty.IsCollection
                && !complexProperty.ClrType.GetGenericTypeImplementations(typeof(IList<>)).Any())
            {
                throw new InvalidOperationException(
                    CoreStrings.NonListCollection(
                        complexProperty.DeclaringType.DisplayName(),
                        complexProperty.Name,
                        complexProperty.ClrType.ShortDisplayName(),
                        $"IList<{complexProperty.ComplexType.ClrType.ShortDisplayName()}>"));
            }

            ValidateFieldMapping(complexProperty.ComplexType, logger);
        }
    }

    /// <summary>
    ///     Validates type mapping for a property.
    /// </summary>
    /// <param name="property">The property to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateTypeMapping(
        IProperty property,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
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
            return;
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

    /// <summary>
    ///     Validates that an entity type is not accidentally mapped from common CLR types.
    /// </summary>
    /// <param name="entityType">The entity type to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateEntityClrType(
        IEntityType entityType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (entityType.ClrType.IsGenericType)
        {
            var genericTypeDefinition = entityType.ClrType.GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(List<>)
                || genericTypeDefinition == typeof(HashSet<>)
                || genericTypeDefinition == typeof(Collection<>)
                || genericTypeDefinition == typeof(ObservableCollection<>))
            {
                logger.AccidentalEntityType(entityType);
            }
        }
    }

    /// <summary>
    ///     Validates a primitive collection property.
    /// </summary>
    /// <param name="property">The property to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidatePrimitiveCollection(
        IProperty property,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
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
        }
    }

    /// <summary>
    ///     Validates query filter configuration for an entity type.
    /// </summary>
    /// <param name="entityType">The entity type to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateQueryFilters(
        IEntityType entityType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var queryFilters = entityType.GetDeclaredQueryFilters();
        if (queryFilters.Count > 0)
        {
            if (entityType.BaseType != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.BadFilterDerivedType(
                        queryFilters.First().Expression,
                        entityType.DisplayName(),
                        entityType.GetRootType().DisplayName()));
            }

            if (entityType.IsOwned())
            {
                throw new InvalidOperationException(
                    CoreStrings.BadFilterOwnedType(queryFilters.First().Expression, entityType.DisplayName()));
            }
        }

        if (!entityType.IsOwned())
        {
            // Owned type doesn't allow to define query filter
            // So we don't check navigations there. We assume the owner will propagate filtering
            var requiredNavigationWithQueryFilter = entityType
                .GetNavigations()
                .FirstOrDefault(n => n is { IsCollection: false, ForeignKey.IsRequired: true, IsOnDependent: true }
                    && n.ForeignKey.PrincipalEntityType.GetRootType().GetDeclaredQueryFilters().Count > 0
                    && n.ForeignKey.DeclaringEntityType.GetRootType().GetDeclaredQueryFilters().Count == 0);

            if (requiredNavigationWithQueryFilter != null)
            {
                logger.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning(
                    requiredNavigationWithQueryFilter.ForeignKey);
            }
        }
    }

    /// <summary>
    ///     Validates seed data for an entity type.
    /// </summary>
    /// <param name="entityType">The entity type to validate.</param>
    /// <param name="identityMaps">Shared identity maps for detecting duplicates.</param>
    /// <param name="sensitiveDataLogged">Whether sensitive data should be logged.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateData(
        IEntityType entityType,
        Dictionary<IKey, IIdentityMap> identityMaps,
        bool sensitiveDataLogged,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var key = entityType.FindPrimaryKey();
        if (key == null)
        {
            if (entityType.GetSeedData().Any())
            {
                throw new InvalidOperationException(CoreStrings.SeedKeylessEntity(entityType.DisplayName()));
            }

            return;
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

    /// <summary>
    ///     Validates a single trigger.
    /// </summary>
    /// <param name="trigger">The trigger to validate.</param>
    /// <param name="entityType">The entity type that declares the trigger.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateTrigger(
        ITrigger trigger,
        IEntityType entityType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
    }

    /// <summary>
    ///     Logs all shadow properties for an entity type.
    /// </summary>
    /// <param name="entityType">The entity type to check.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void LogShadowProperties(
        IEntityType entityType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (entityType is not IConventionEntityType conventionEntityType)
        {
            return;
        }

        foreach (var property in conventionEntityType.GetDeclaredProperties())
        {
            if (property.IsShadowProperty()
                && property.GetConfigurationSource() == ConfigurationSource.Convention)
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
