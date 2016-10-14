// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

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
        /// <param name="logger"> The logger. </param>
        public CoreModelValidator([NotNull] ILogger<ModelValidator> logger)
            : base(logger)
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
            EnsureFieldMapping(model);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void EnsureNoShadowEntities([NotNull] IModel model)
        {
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
            foreach (var entityType in model.GetEntityTypes().Where(t => t.ClrType != null))
            {
                foreach (var key in entityType.GetDeclaredKeys())
                {
                    if (key.Properties.Any(p => p.IsShadowProperty))
                    {
                        var referencingFk = key.GetReferencingForeignKeys().FirstOrDefault();
                        var conventionalKey = key as Key;
                        if (referencingFk != null
                            && conventionalKey != null
                            && ConfigurationSource.Convention.Overrides(conventionalKey.GetConfigurationSource()))
                        {
                            ShowError(CoreStrings.ReferencedShadowKey(
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
                            continue;
                        }

                        ShowWarning(CoreEventId.ModelValidationShadowKeyWarning, CoreStrings.ShadowKey(
                            Property.Format(key.Properties),
                            entityType.DisplayName(),
                            Property.Format(key.Properties)));
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
        protected virtual void EnsureClrInheritance([NotNull] IModel model, [NotNull] IEntityType entityType, [NotNull] HashSet<IEntityType> validEntityTypes)
        {
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
        protected virtual void EnsureFieldMapping([NotNull] IModel model)
        {
            foreach (var entityType in model.GetEntityTypes())
            {
                foreach (var propertyBase in entityType
                    .GetDeclaredProperties().Where(e => !e.IsShadowProperty).Cast<IPropertyBase>()
                    .Concat(entityType.GetDeclaredNavigations()))
                {
                    MemberInfo _;
                    string errorMessage;

                    if (!propertyBase.TryGetMemberInfo(
                        forConstruction: true,
                        forSet: true,
                        memberInfo: out _,
                        errorMessage: out errorMessage))
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ShowWarning(CoreEventId eventId, [NotNull] string message)
            => Logger.LogWarning(eventId, () => message);
    }
}
