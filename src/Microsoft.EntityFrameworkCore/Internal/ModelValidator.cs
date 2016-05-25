// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public abstract class ModelValidator : IModelValidator
    {
        public virtual void Validate(IModel model)
        {
            EnsureNoShadowEntities(model);
            EnsureNoShadowKeys(model);
            EnsureNonNullPrimaryKeys(model);
            EnsureClrInheritance(model);
            EnsureChangeTrackingStrategy(model);
        }

        protected virtual void EnsureNoShadowEntities([NotNull] IModel model)
        {
            var firstShadowEntity = model.GetEntityTypes().FirstOrDefault(entityType => !entityType.HasClrType());
            if (firstShadowEntity != null)
            {
                ShowError(CoreStrings.ShadowEntity(firstShadowEntity.Name));
            }
        }

        protected virtual void EnsureNoShadowKeys([NotNull] IModel model)
        {
            var messages = KeyConvention.GetShadowKeyExceptionMessage(model, key => key.Properties.Any(p => p.IsShadowProperty));
            if (messages == null)
            {
                return;
            }

            foreach (var message in messages)
            {
                ShowWarning(message);
            }
        }

        protected virtual void EnsureNonNullPrimaryKeys([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            var entityTypeWithNullPk = model.GetEntityTypes().FirstOrDefault(et => et.FindPrimaryKey() == null);
            if (entityTypeWithNullPk != null)
            {
                ShowError(CoreStrings.EntityRequiresKey(entityTypeWithNullPk.Name));
            }
        }

        protected virtual void EnsureClrInheritance([NotNull] IModel model)
        {
            var validEntityTypes = new HashSet<IEntityType>();
            foreach (var entityType in model.GetEntityTypes())
            {
                EnsureClrInheritance(model, entityType, validEntityTypes);
            }
        }

        private void EnsureClrInheritance(IModel model, IEntityType entityType, HashSet<IEntityType> validEntityTypes)
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

        protected virtual void ShowError([NotNull] string message)
        {
            throw new InvalidOperationException(message);
        }

        protected abstract void ShowWarning([NotNull] string message);
    }
}
