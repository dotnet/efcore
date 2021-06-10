// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         The validator that enforces core rules common for all providers.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class ModelValidator : IModelValidator
    {
        private static readonly IEnumerable<string> _dictionaryProperties =
            typeof(IDictionary<string, object>).GetRuntimeProperties().Select(e => e.Name);

        /// <summary>
        ///     Creates a new instance of <see cref="ModelValidator" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public ModelValidator(ModelValidatorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies used to create a <see cref="ModelValidator" />
        /// </summary>
        protected virtual ModelValidatorDependencies Dependencies { get; }

        /// <inheritdoc />
        public virtual void Validate(IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            ValidateNoShadowEntities(model, logger);
#pragma warning restore CS0618 // Type or member is obsolete
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
            LogShadowProperties(model, logger);
        }

        /// <summary>
        ///     Validates relationships.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateRelationships(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            Check.NotNull(model, nameof(model));

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
        /// <param name="model"> The model. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidatePropertyMapping(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            Check.NotNull(model, nameof(model));

            if (model is not IConventionModel conventionModel)
            {
                return;
            }

            foreach (var entityType in conventionModel.GetEntityTypes())
            {
                var unmappedProperty = entityType.GetDeclaredProperties().FirstOrDefault(
                    p => (!ConfigurationSource.Convention.Overrides(p.GetConfigurationSource())
                            // Use a better condition for non-persisted properties when issue #14121 is implemented
                            || !p.IsImplicitlyCreated())
                        && p.FindTypeMapping() == null);

                if (unmappedProperty != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.PropertyNotMapped(
                            entityType.DisplayName(), unmappedProperty.Name,
                            (unmappedProperty.GetValueConverter()?.ProviderClrType ?? unmappedProperty.ClrType).ShortDisplayName()));
                }

                if (entityType.ClrType == Model.DefaultPropertyBagType)
                {
                    continue;
                }

                var runtimeProperties = entityType.GetRuntimeProperties();
                var clrProperties = new HashSet<string>(StringComparer.Ordinal);
                clrProperties.UnionWith(
                    runtimeProperties.Values
                        .Where(pi => pi.IsCandidateProperty(needsWrite: false))
                        .Select(pi => pi.GetSimpleMemberName()));

                clrProperties.ExceptWith(
                    ((IEnumerable<IConventionPropertyBase>)entityType.GetProperties())
                    .Concat(entityType.GetNavigations())
                    .Concat(entityType.GetSkipNavigations())
                    .Concat(entityType.GetServiceProperties()).Select(p => p.Name));

                if (entityType.IsPropertyBag)
                {
                    clrProperties.ExceptWith(_dictionaryProperties);
                }

                if (clrProperties.Count <= 0)
                {
                    continue;
                }

                foreach (var clrPropertyName in clrProperties)
                {
                    if (entityType.FindIgnoredConfigurationSource(clrPropertyName) != null)
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
                        clrProperty, conventionModel, out var targetOwned);
                    if (targetType == null
                        && clrProperty.FindSetterProperty() == null)
                    {
                        continue;
                    }

                    if (targetType != null)
                    {
                        var targetShared = conventionModel.IsShared(targetType);
                        targetOwned ??= conventionModel.IsOwned(targetType);
                        // ReSharper disable CheckForReferenceEqualityInstead.1
                        // ReSharper disable CheckForReferenceEqualityInstead.3
                        if ((!entityType.IsKeyless
                                || targetSequenceType == null)
                            && entityType.GetDerivedTypes().All(
                                dt => dt.GetDeclaredNavigations().FirstOrDefault(n => n.Name == clrProperty.GetSimpleMemberName())
                                    == null)
                            && (!(targetShared || targetOwned.Value)
                                || (!targetType.Equals(entityType.ClrType)
                                    && (!entityType.IsInOwnershipPath(targetType)
                                        || (entityType.FindOwnership()!.PrincipalEntityType.ClrType.Equals(targetType)
                                            && targetSequenceType == null)))))
                        {
                            if (entityType.IsOwned()
                                && targetOwned.Value)
                            {
                                throw new InvalidOperationException(
                                    CoreStrings.AmbiguousOwnedNavigation(
                                        entityType.DisplayName() + "." + clrProperty.Name, targetType.ShortDisplayName()));
                            }

                            if (targetShared)
                            {
                                throw new InvalidOperationException(
                                    CoreStrings.NonConfiguredNavigationToSharedType(clrProperty.Name, entityType.DisplayName()));
                            }

                            throw new InvalidOperationException(
                                CoreStrings.NavigationNotAdded(
                                    entityType.DisplayName(), clrProperty.Name, propertyType.ShortDisplayName()));
                        }

                        // ReSharper restore CheckForReferenceEqualityInstead.3
                        // ReSharper restore CheckForReferenceEqualityInstead.1
                    }
                    else if (targetSequenceType == null && propertyType.IsInterface
                        || targetSequenceType?.IsInterface == true)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.InterfacePropertyNotAdded(
                                entityType.DisplayName(), clrProperty.Name, propertyType.ShortDisplayName()));
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            CoreStrings.PropertyNotAdded(
                                entityType.DisplayName(), clrProperty.Name, propertyType.ShortDisplayName()));
                    }
                }
            }
        }

        /// <summary>
        ///     Validates that no attempt is made to ignore inherited properties.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateIgnoredMembers(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            Check.NotNull(model, nameof(model));

            if (!(model is IConventionModel conventionModel))
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
                        if (property.DeclaringEntityType != entityType)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.InheritedPropertyCannotBeIgnored(
                                    ignoredMember, entityType.DisplayName(), property.DeclaringEntityType.DisplayName()));
                        }

                        Check.DebugAssert(false, "Should never get here...");
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

                        Check.DebugAssert(false, "Should never get here...");
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

                        Check.DebugAssert(false, "Should never get here...");
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

                        Check.DebugAssert(false, "Should never get here...");
                    }
                }
            }
        }

        /// <summary>
        ///     Validates that the model does not contain any entity types without a corresponding CLR type.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        [Obsolete("Shadow entity types cannot be created anymore")]
        protected virtual void ValidateNoShadowEntities(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
        }

        /// <summary>
        ///     Validates the mapping/configuration of shadow keys in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateNoShadowKeys(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            Check.NotNull(model, nameof(model));

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
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateNoMutableKeys(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            Check.NotNull(model, nameof(model));

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
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateNoCycles(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            Check.NotNull(model, nameof(model));

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
                c => CoreStrings.IdentifyingRelationshipCycle(c));
        }

        /// <summary>
        ///     Validates the mapping/configuration of primary key nullability in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateNonNullPrimaryKeys(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            Check.NotNull(model, nameof(model));

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
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateClrInheritance(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            Check.NotNull(model, nameof(model));

            var validEntityTypes = new HashSet<IEntityType>();
            foreach (var entityType in model.GetEntityTypes())
            {
                ValidateClrInheritance(model, entityType, validEntityTypes);
            }
        }

        private void ValidateClrInheritance(
            IModel model,
            IEntityType entityType,
            HashSet<IEntityType> validEntityTypes)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(validEntityTypes, nameof(validEntityTypes));

            if (validEntityTypes.Contains(entityType))
            {
                return;
            }

            if (entityType.HasSharedClrType
                && entityType.BaseType != null)
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
        ///     Validates the mapping/configuration of inheritance in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        [Obsolete("Use ValidateInheritanceMapping")]
        protected virtual void ValidateDiscriminatorValues(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var rootEntityType in model.GetRootEntityTypes())
            {
                ValidateDiscriminatorValues(rootEntityType);
            }
        }

        /// <summary>
        ///     Validates the mapping of inheritance in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateInheritanceMapping(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            ValidateDiscriminatorValues(model, logger);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        ///     Validates the discriminator and values for all entity types derived from the given one.
        /// </summary>
        /// <param name="rootEntityType"> The entity type to validate. </param>
        protected virtual void ValidateDiscriminatorValues(IEntityType rootEntityType)
        {
            var derivedTypes = rootEntityType.GetDerivedTypesInclusive().ToList();
            if (derivedTypes.Count == 1)
            {
                return;
            }

            var discriminatorProperty = rootEntityType.FindDiscriminatorProperty();
            if (discriminatorProperty == null)
            {
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

                var discriminatorValue = derivedType.GetDiscriminatorValue();
                if (discriminatorValue == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.NoDiscriminatorValue(derivedType.DisplayName()));
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
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateChangeTrackingStrategy(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            Check.NotNull(model, nameof(model));

            var requireFullNotifications = (bool?)model[CoreAnnotationNames.FullChangeTrackingNotificationsRequiredAnnotation] == true;
            foreach (var entityType in model.GetEntityTypes())
            {
                var errorMessage = EntityType.CheckChangeTrackingStrategy(
                    entityType, entityType.GetChangeTrackingStrategy(), requireFullNotifications);

                if (errorMessage != null)
                {
                    throw new InvalidOperationException(errorMessage);
                }
            }
        }

        /// <summary>
        ///     Validates the mapping/configuration of ownership in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateOwnership(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            Check.NotNull(model, nameof(model));

            foreach (var entityType in model.GetEntityTypes())
            {
                var ownerships = entityType.GetForeignKeys().Where(fk => fk.IsOwnership).ToList();
                if (ownerships.Count > 1)
                {
                    throw new InvalidOperationException(CoreStrings.MultipleOwnerships(
                        entityType.DisplayName(),
                        string.Join(", ",
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

                    if (ownership.PrincipalToDependent == null)
                    {
                        throw new InvalidOperationException(CoreStrings.NavigationlessOwnership(
                            ownership.PrincipalEntityType.DisplayName(), entityType.DisplayName()));
                    }

                    foreach (var referencingFk in entityType.GetReferencingForeignKeys().Where(
                        fk => !fk.IsOwnership
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
                        fk => !fk.IsOwnership
                            && fk.PrincipalToDependent != null
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

        private bool Contains(IForeignKey? inheritedFk, IForeignKey derivedFk)
            => inheritedFk != null
                && inheritedFk.PrincipalEntityType.IsAssignableFrom(derivedFk.PrincipalEntityType)
                && PropertyListComparer.Instance.Equals(inheritedFk.Properties, derivedFk.Properties);

        /// <summary>
        ///     Validates the mapping/configuration of foreign keys in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateForeignKeys(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            Check.NotNull(model, nameof(model));

            foreach (var entityType in model.GetEntityTypes())
            {
                foreach (var declaredForeignKey in entityType.GetDeclaredForeignKeys())
                {
                    if (declaredForeignKey.PrincipalEntityType == declaredForeignKey.DeclaringEntityType
                        && declaredForeignKey.PrincipalKey.Properties.SequenceEqual(declaredForeignKey.Properties))
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
                        .All(d => d.GetForeignKeys()
                            .Any(fk => fk.Properties.Contains(property)));
        }

        /// <summary>
        ///     Validates the mapping/configuration of defining navigations in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        [Obsolete]
        protected virtual void ValidateDefiningNavigations(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
        }

        /// <summary>
        ///     Validates the mapping/configuration of properties mapped to fields in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateFieldMapping(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            Check.NotNull(model, nameof(model));

            foreach (var entityType in model.GetEntityTypes())
            {
                var properties = new HashSet<IPropertyBase>(
                    entityType
                        .GetDeclaredProperties()
                        .Cast<IPropertyBase>()
                        .Concat(entityType.GetDeclaredNavigations())
                        .Where(p => !p.IsShadowProperty() && !p.IsIndexerProperty()));

                var constructorBinding = entityType.ConstructorBinding;
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
                        forConstruction: true,
                        forSet: true,
                        memberInfo: out _,
                        errorMessage: out var errorMessage))
                    {
                        throw new InvalidOperationException(errorMessage);
                    }

                    if (!propertyBase.TryGetMemberInfo(
                        forConstruction: false,
                        forSet: true,
                        memberInfo: out _,
                        errorMessage: out errorMessage))
                    {
                        throw new InvalidOperationException(errorMessage);
                    }

                    if (!propertyBase.TryGetMemberInfo(
                        forConstruction: false,
                        forSet: false,
                        memberInfo: out _,
                        errorMessage: out errorMessage))
                    {
                        throw new InvalidOperationException(errorMessage);
                    }
                }
            }
        }

        /// <summary>
        ///     Validates the type mapping of properties the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateTypeMappings(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(logger, nameof(logger));

            foreach (var entityType in model.GetEntityTypes())
            {
                foreach (var property in entityType.GetDeclaredProperties())
                {
                    var converter = property.GetValueConverter();
                    if (converter != null
                        && property[CoreAnnotationNames.ValueComparer] == null)
                    {
                        var type = converter.ModelClrType;
                        if (type != typeof(string)
                            && !(type == typeof(byte[]) && property.IsKey()) // Already special-cased elsewhere
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
                }
            }
        }

        /// <summary>
        ///     Validates the mapping/configuration of query filters in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateQueryFilters(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            Check.NotNull(model, nameof(model));

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

                var requiredNavigationWithQueryFilter = entityType.GetNavigations()
                    .Where(
                        n => !n.IsCollection
                            && n.ForeignKey.IsRequired
                            && n.IsOnDependent
                            && n.ForeignKey.PrincipalEntityType.GetQueryFilter() != null
                            && n.ForeignKey.DeclaringEntityType.GetQueryFilter() == null).FirstOrDefault();

                if (requiredNavigationWithQueryFilter != null)
                {
                    logger.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning(
                        requiredNavigationWithQueryFilter.ForeignKey);
                }
            }
        }

        /// <summary>
        ///     Validates the mapping/configuration of data (e.g. seed data) in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateData(IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            Check.NotNull(model, nameof(model));

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
        ///     Logs all shadow properties that were created because there was no matching CLR member.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void LogShadowProperties(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            Check.NotNull(model, nameof(model));

            foreach (IConventionEntityType entityType in model.GetEntityTypes())
            {
                foreach (var property in entityType.GetDeclaredProperties())
                {
                    if (property.IsImplicitlyCreated())
                    {
                        logger.ShadowPropertyCreated((IProperty)property);
                    }
                }
            }
        }
    }
}
