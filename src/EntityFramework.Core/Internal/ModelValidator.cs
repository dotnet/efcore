// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Internal
{
    public abstract class ModelValidator : IModelValidator
    {
        public virtual void Validate(IModel model)
        {
            EnsureNoShadowEntities(model);
            EnsureNoShadowKeys(model);
            EnsureNonNullPrimaryKeys(model);
        }

        protected virtual void EnsureNoShadowEntities([NotNull] IModel model)
        {
            var firstShadowEntity = model.EntityTypes.FirstOrDefault(entityType => !entityType.HasClrType());
            if (firstShadowEntity != null)
            {
                ShowError(CoreStrings.ShadowEntity(firstShadowEntity.Name));
            }
        }

        protected virtual void EnsureNoShadowKeys([NotNull] IModel model)
        {
            foreach (var entityType in model.EntityTypes)
            {
                foreach (var key in entityType.GetKeys())
                {
                    if (key.Properties.Any(p => p.IsShadowProperty))
                    {
                        string message;
                        var referencingFk = model.FindReferencingForeignKeys(key).FirstOrDefault();
                        if (referencingFk != null)
                        {
                            message = CoreStrings.ReferencedShadowKey(
                                Property.Format(key.Properties),
                                entityType.Name,
                                Property.Format(key.Properties.Where(p => p.IsShadowProperty)),
                                Property.Format(referencingFk.Properties),
                                referencingFk.DeclaringEntityType.Name);
                        }
                        else
                        {
                            message = CoreStrings.ShadowKey(
                                Property.Format(key.Properties),
                                entityType.Name,
                                Property.Format(key.Properties.Where(p => p.IsShadowProperty)));
                        }

                        ShowWarning(message);
                    }
                }
            }
        }

        protected virtual void EnsureNonNullPrimaryKeys([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            var entityTypeWithNullPk = model.EntityTypes.FirstOrDefault(et => et.FindPrimaryKey() == null);
            if (entityTypeWithNullPk != null)
            {
                ShowError(CoreStrings.EntityRequiresKey(entityTypeWithNullPk.Name));
            }
        }

        protected virtual void ShowError([NotNull] string message)
        {
            throw new InvalidOperationException(message);
        }

        protected abstract void ShowWarning([NotNull] string message);
    }
}
