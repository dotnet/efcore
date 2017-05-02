// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     The validator that enforces core rules common for all providers.
    /// </summary>
    public class CoreModelValidator : ModelValidator
    {
        /// <summary>
        ///     Creates a new instance of <see cref="CoreModelValidator" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public CoreModelValidator([NotNull] ModelValidatorDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     Validates a model, throwing an exception if any errors are found.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        public override void Validate(IModel model)
        {
            EnsureNoShadowEntities(model);
            EnsureNonNullPrimaryKeys(model);
            EnsureNoShadowKeys(model);
            EnsureClrInheritance(model);
            EnsureChangeTrackingStrategy(model);
            ValidateOwnership(model);
            ValidateDelegatedIdentityNavigations(model);
            EnsureFieldMapping(model);
            ValidateQueryFilters(model);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ValidateQueryFilters([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            var filterValidatingExppressionVisitor = new FilterValidatingExppressionVisitor();

            foreach (var entityType in model.GetEntityTypes())
            {
                if (entityType.Filter != null)
                {
                    if (entityType.BaseType != null)
                    {
                        ShowError(CoreStrings.BadFilterDerivedType(entityType.Filter, entityType.DisplayName()));
                    }

                    if (!filterValidatingExppressionVisitor.IsValid(entityType))
                    {
                        ShowError(CoreStrings.BadFilterExpression(entityType.Filter, entityType.DisplayName(), entityType.ClrType));
                    }
                }
            }
        }

        private sealed class FilterValidatingExppressionVisitor : ExpressionVisitor
        {
            private IEntityType _entityType;

            private bool _valid = true;

            public bool IsValid(IEntityType entityType)
            {
                _entityType = entityType;

                Visit(entityType.Filter.Body);

                return _valid;
            }

            protected override Expression VisitMember(MemberExpression memberExpression)
            {
                if (memberExpression.Expression == _entityType.Filter.Parameters[0]
                    && memberExpression.Member is PropertyInfo propertyInfo
                    && _entityType.FindNavigation(propertyInfo) != null)
                {
                    _valid = false;

                    return memberExpression;
                }

                return base.VisitMember(memberExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.IsEFProperty()
                    && methodCallExpression.Arguments[0] == _entityType.Filter.Parameters[0]
                    && (!(methodCallExpression.Arguments[1] is ConstantExpression constantExpression)
                        || !(constantExpression.Value is string propertyName)
                        || _entityType.FindNavigation(propertyName) != null))
                {
                    _valid = false;

                    return methodCallExpression;
                }

                return base.VisitMethodCall(methodCallExpression);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void EnsureNoShadowEntities([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            var firstShadowEntity = model.GetEntityTypes().FirstOrDefault(entityType => !entityType.HasClrType());
            if (firstShadowEntity != null)
            {
                ShowError(CoreStrings.ShadowEntity(firstShadowEntity.DisplayName()));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void EnsureNoShadowKeys([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            foreach (var entityType in model.GetEntityTypes().Where(t => t.ClrType != null))
            {
                foreach (var key in entityType.GetDeclaredKeys())
                {
                    if (key.Properties.Any(p => p.IsShadowProperty)
                        && key is Key concreteKey
                        && ConfigurationSource.Convention.Overrides(concreteKey.GetConfigurationSource())
                        && !key.IsPrimaryKey())
                    {
                        var referencingFk = key.GetReferencingForeignKeys().FirstOrDefault();

                        if (referencingFk != null)
                        {
                            ShowError(
                                CoreStrings.ReferencedShadowKey(
                                    referencingFk.DeclaringEntityType.DisplayName() +
                                    (referencingFk.DependentToPrincipal == null
                                        ? ""
                                        : "." + referencingFk.DependentToPrincipal.Name),
                                    entityType.DisplayName() +
                                    (referencingFk.PrincipalToDependent == null
                                        ? ""
                                        : "." + referencingFk.PrincipalToDependent.Name),
                                    Property.Format(referencingFk.Properties, includeTypes: true),
                                    Property.Format(entityType.FindPrimaryKey().Properties, includeTypes: true)));
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void EnsureNonNullPrimaryKeys([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            var entityTypeWithNullPk = model.GetEntityTypes().FirstOrDefault(et => et.FindPrimaryKey() == null);
            if (entityTypeWithNullPk != null)
            {
                ShowError(CoreStrings.EntityRequiresKey(entityTypeWithNullPk.DisplayName()));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void EnsureClrInheritance([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            var validEntityTypes = new HashSet<IEntityType>();
            foreach (var entityType in model.GetEntityTypes())
            {
                EnsureClrInheritance(model, entityType, validEntityTypes);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void EnsureClrInheritance(
            [NotNull] IModel model, [NotNull] IEntityType entityType, [NotNull] HashSet<IEntityType> validEntityTypes)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(validEntityTypes, nameof(validEntityTypes));

            if (validEntityTypes.Contains(entityType))
            {
                return;
            }

            var baseClrType = entityType.ClrType?.GetTypeInfo().BaseType;
            while (baseClrType != null)
            {
                var baseEntityType = model.FindEntityType(baseClrType);
                if (baseEntityType != null)
                {
                    if (!baseEntityType.IsAssignableFrom(entityType))
                    {
                        ShowError(CoreStrings.InconsistentInheritance(entityType.DisplayName(), baseEntityType.DisplayName()));
                    }
                    EnsureClrInheritance(model, baseEntityType, validEntityTypes);
                    break;
                }
                baseClrType = baseClrType.GetTypeInfo().BaseType;
            }

            if (entityType.ClrType?.IsInstantiable() == false
                && !entityType.GetDerivedTypes().Any())
            {
                ShowError(CoreStrings.AbstractLeafEntityType(entityType.DisplayName()));
            }

            validEntityTypes.Add(entityType);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void EnsureChangeTrackingStrategy([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            var detectChangesNeeded = false;
            foreach (var entityType in model.GetEntityTypes())
            {
                var changeTrackingStrategy = entityType.GetChangeTrackingStrategy();
                if (changeTrackingStrategy == ChangeTrackingStrategy.Snapshot)
                {
                    detectChangesNeeded = true;
                }

                var errorMessage = entityType.CheckChangeTrackingStrategy(changeTrackingStrategy);
                if (errorMessage != null)
                {
                    ShowError(errorMessage);
                }
            }

            if (!detectChangesNeeded)
            {
                (model as IMutableModel)?.GetOrAddAnnotation(ChangeDetector.SkipDetectChangesAnnotation, "true");
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ValidateOwnership([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            foreach (var entityType in model.GetEntityTypes())
            {
                var ownerships = entityType.GetForeignKeys().Where(fk => fk.IsOwnership).ToList();
                if (ownerships.Count > 1)
                {
                    throw new InvalidOperationException(CoreStrings.MultipleOwnerships(entityType.DisplayName()));
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ValidateDelegatedIdentityNavigations([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            foreach (var entityType in model.GetEntityTypes())
            {
                if (entityType.DefiningEntityType != null)
                {
                    if (entityType.FindDefiningNavigation() == null
                        || (entityType.DefiningEntityType as EntityType)?.Builder == null)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.NoDefiningNavigation(
                                entityType.DefiningNavigationName, entityType.DefiningEntityType.DisplayName(), entityType.DisplayName()));
                    }

                    var ownership = entityType.GetForeignKeys().SingleOrDefault(fk => fk.IsOwnership);
                    if (ownership != null)
                    {
                        if (ownership.PrincipalToDependent?.Name != entityType.DefiningNavigationName)
                        {
                            var ownershipNavigation = ownership.PrincipalToDependent == null
                                ? ""
                                : "." + ownership.PrincipalToDependent.Name;
                            throw new InvalidOperationException(
                                CoreStrings.NonDefiningOwnership(
                                    ownership.PrincipalEntityType.DisplayName() + ownershipNavigation,
                                    entityType.DefiningNavigationName,
                                    entityType.DisplayName()));
                        }

                        foreach (var otherEntityType in model.GetEntityTypes().Where(et => et.ClrType == entityType.ClrType && et != entityType))
                        {
                            if (!otherEntityType.GetForeignKeys().Any(fk => fk.IsOwnership))
                            {
                                throw new InvalidOperationException(
                                    CoreStrings.InconsistentOwnership(entityType.DisplayName(), otherEntityType.DisplayName()));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void EnsureFieldMapping([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            foreach (var entityType in model.GetEntityTypes())
            {
                foreach (var propertyBase in entityType
                    .GetDeclaredProperties()
                    .Where(e => !e.IsShadowProperty)
                    .Cast<IPropertyBase>()
                    .Concat(entityType.GetDeclaredNavigations()))
                {
                    if (!propertyBase.TryGetMemberInfo(
                        forConstruction: true,
                        forSet: true,
                        memberInfo: out var _,
                        errorMessage: out var errorMessage))
                    {
                        ShowError(errorMessage);
                    }

                    if (!propertyBase.TryGetMemberInfo(
                        forConstruction: false,
                        forSet: true,
                        memberInfo: out _,
                        errorMessage: out errorMessage))
                    {
                        ShowError(errorMessage);
                    }

                    if (!propertyBase.TryGetMemberInfo(
                        forConstruction: false,
                        forSet: false,
                        memberInfo: out _,
                        errorMessage: out errorMessage))
                    {
                        ShowError(errorMessage);
                    }
                }
            }
        }
    }
}
