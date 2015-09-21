// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
            EnsureClrPropertyTypesMatch(model);
            EnsureValidForeignKeyChains(model);
        }


        protected virtual void EnsureNoShadowEntities([NotNull] IModel model)
        {
            var firstShadowEntity = model.EntityTypes.FirstOrDefault(entityType => !entityType.HasClrType());
            if (firstShadowEntity != null)
            {
                ShowError(Strings.ShadowEntity(firstShadowEntity.Name));
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
                            message = Strings.ReferencedShadowKey(
                                Property.Format(key.Properties),
                                entityType.Name,
                                Property.Format(key.Properties.Where(p => p.IsShadowProperty)),
                                Property.Format(referencingFk.Properties),
                                referencingFk.DeclaringEntityType.Name);
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

        protected virtual void EnsureNonNullPrimaryKeys([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            var entityTypeWithNullPk = model.EntityTypes.FirstOrDefault(et => et.FindPrimaryKey() == null);
            if (entityTypeWithNullPk != null)
            {
                ShowError(Strings.EntityRequiresKey(entityTypeWithNullPk.Name));
            }
        }

        protected virtual void EnsureClrPropertyTypesMatch([NotNull] IModel model)
        {
            foreach (var entityType in model.EntityTypes)
            {
                foreach (var property in entityType.GetDeclaredProperties())
                {
                    if (property.IsShadowProperty
                        || !entityType.HasClrType())
                    {
                        continue;
                    }

                    var clrProperty = entityType.ClrType.GetPropertiesInHierarchy(property.Name).FirstOrDefault();
                    if (clrProperty == null)
                    {
                        ShowError(Strings.NoClrProperty(property.Name, entityType.Name));
                        continue;
                    }

                    if (property.ClrType != clrProperty.PropertyType)
                    {
                        ShowError(Strings.PropertyWrongClrType(property.Name, entityType.Name));
                    }
                }
            }
        }

        protected virtual void EnsureValidForeignKeyChains([NotNull] IModel model)
        {
            var verifiedProperties = new Dictionary<IProperty, IProperty>();
            foreach (var entityType in model.EntityTypes)
            {
                foreach (var foreignKey in entityType.GetForeignKeys())
                {
                    foreach (var referencedProperty in foreignKey.Properties)
                    {
                        string errorMessage;
                        VerifyRootPrincipal(referencedProperty, verifiedProperties, ImmutableList<IForeignKey>.Empty, out errorMessage);
                        if (errorMessage != null)
                        {
                            ShowError(errorMessage);
                        }
                    }
                }
            }
        }

        private IProperty VerifyRootPrincipal(
            IProperty principalProperty,
            Dictionary<IProperty, IProperty> verifiedProperties,
            ImmutableList<IForeignKey> visitedForeignKeys,
            out string errorMessage)
        {
            errorMessage = null;
            IProperty rootPrincipal;
            if (verifiedProperties.TryGetValue(principalProperty, out rootPrincipal))
            {
                return rootPrincipal;
            }

            var rootPrincipals = new Dictionary<IProperty, IForeignKey>();
            foreach (var foreignKey in principalProperty.DeclaringEntityType.GetForeignKeys())
            {
                for (var index = 0; index < foreignKey.Properties.Count; index++)
                {
                    if (principalProperty == foreignKey.Properties[index])
                    {
                        var nextPrincipalProperty = foreignKey.PrincipalKey.Properties[index];
                        if (visitedForeignKeys.Contains(foreignKey))
                        {
                            var cycleStart = visitedForeignKeys.IndexOf(foreignKey);
                            var cycle = visitedForeignKeys.GetRange(cycleStart, visitedForeignKeys.Count - cycleStart);
                            errorMessage = Strings.CircularDependency(cycle.Select(fk => fk.ToString()).Join());
                            continue;
                        }
                        rootPrincipal = VerifyRootPrincipal(nextPrincipalProperty, verifiedProperties, visitedForeignKeys.Add(foreignKey), out errorMessage);
                        if (rootPrincipal == null)
                        {
                            if (principalProperty.RequiresValueGenerator)
                            {
                                rootPrincipals[principalProperty] = foreignKey;
                            }
                            continue;
                        }

                        if (principalProperty.RequiresValueGenerator)
                        {
                            ShowError(Strings.ForeignKeyValueGenerationOnAdd(
                                principalProperty.Name,
                                principalProperty.DeclaringEntityType.DisplayName(),
                                Property.Format(foreignKey.Properties)));
                            return principalProperty;
                        }

                        rootPrincipals[rootPrincipal] = foreignKey;
                    }
                }
            }

            if (rootPrincipals.Count == 0)
            {
                if (errorMessage != null)
                {
                    return null;
                }

                if (!principalProperty.RequiresValueGenerator)
                {
                    ShowError(Strings.PrincipalKeyNoValueGenerationOnAdd(principalProperty.Name, principalProperty.DeclaringEntityType.DisplayName()));
                    return null;
                }

                return principalProperty;
            }

            if (rootPrincipals.Count > 1)
            {
                var firstRoot = rootPrincipals.Keys.ElementAt(0);
                var secondRoot = rootPrincipals.Keys.ElementAt(1);
                ShowWarning(Strings.MultipleRootPrincipals(
                    rootPrincipals[firstRoot].DeclaringEntityType.DisplayName(),
                    Property.Format(rootPrincipals[firstRoot].Properties),
                    firstRoot.DeclaringEntityType.DisplayName(),
                    firstRoot.Name,
                    Property.Format(rootPrincipals[secondRoot].Properties),
                    secondRoot.DeclaringEntityType.DisplayName(),
                    secondRoot.Name));

                return firstRoot;
            }

            errorMessage = null;
            rootPrincipal = rootPrincipals.Keys.Single();
            verifiedProperties[principalProperty] = rootPrincipal;
            return rootPrincipal;
        }

        protected virtual void ShowError([NotNull] string message)
        {
            throw new InvalidOperationException(message);
        }

        protected abstract void ShowWarning([NotNull] string message);
    }
}
