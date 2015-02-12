// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Infrastructure
{
    public abstract class ModelValidatorBase : ModelValidator
    {
        public override void Validate(IModel model)
        {
            EnsureNoShadowKeys(model);
        }

        protected void EnsureNoShadowKeys(IModel model)
        {
            foreach (var entityType in model.EntityTypes)
            {
                foreach (var key in entityType.Keys)
                {
                    if (key.Properties.Any(p => p.IsShadowProperty))
                    {
                        string message;
                        var referencingFk = model.GetReferencingForeignKeys(key).FirstOrDefault();
                        if (referencingFk != null)
                        {
                            message = Strings.ReferencedShadowKey(
                                Property.Format(key.Properties),
                                entityType.Name,
                                Property.Format(key.Properties.Where(p => p.IsShadowProperty)),
                                Property.Format(referencingFk.Properties),
                                referencingFk.EntityType.Name);
                        }
                        else
                        {
                            message = Strings.ShadowKey(
                                Property.Format(key.Properties),
                                entityType.Name,
                                Property.Format(key.Properties.Where(p => p.IsShadowProperty)));
                        }

                        ShowWarning(message);
                    }
                }
            }
        }

        protected virtual void ShowError(string message)
        {
            throw new InvalidOperationException(message);
        }

        protected abstract void ShowWarning(string message);
    }
}
