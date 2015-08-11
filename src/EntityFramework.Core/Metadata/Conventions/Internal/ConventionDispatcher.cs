// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class ConventionDispatcher
    {
        private readonly ConventionSet _conventionSet;

        public ConventionDispatcher([NotNull] ConventionSet conventionSet)
        {
            Check.NotNull(conventionSet, nameof(conventionSet));

            _conventionSet = conventionSet;
        }

        public virtual InternalEntityTypeBuilder OnEntityTypeAdded([NotNull] InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            foreach (var entityTypeConvention in _conventionSet.EntityTypeAddedConventions)
            {
                entityTypeBuilder = entityTypeConvention.Apply(entityTypeBuilder);
                if (entityTypeBuilder == null)
                {
                    break;
                }
            }

            return entityTypeBuilder;
        }

        public virtual InternalEntityTypeBuilder OnBaseEntityTypeSet(
            [NotNull] InternalEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] EntityType oldBaseType)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            foreach (var entityTypeConvention in _conventionSet.BaseEntityTypeSetConventions)
            {
                if (!entityTypeConvention.Apply(entityTypeBuilder, oldBaseType))
                {
                    break;
                }
            }

            return entityTypeBuilder;
        }

        public virtual InternalRelationshipBuilder OnForeignKeyAdded([NotNull] InternalRelationshipBuilder relationshipBuilder)
        {
            Check.NotNull(relationshipBuilder, nameof(relationshipBuilder));

            foreach (var relationshipConvention in _conventionSet.ForeignKeyAddedConventions)
            {
                relationshipBuilder = relationshipConvention.Apply(relationshipBuilder);
                if (relationshipBuilder == null)
                {
                    break;
                }
            }

            return relationshipBuilder;
        }

        public virtual void OnForeignKeyRemoved([NotNull] InternalEntityTypeBuilder entityTypeBuilder, [NotNull] ForeignKey foreignKey)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(foreignKey, nameof(foreignKey));

            foreach (var foreignKeyConvention in _conventionSet.ForeignKeyRemovedConventions)
            {
                foreignKeyConvention.Apply(entityTypeBuilder, foreignKey);
            }
        }

        public virtual InternalKeyBuilder OnKeyAdded([NotNull] InternalKeyBuilder keyBuilder)
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));

            foreach (var keyConvention in _conventionSet.KeyAddedConventions)
            {
                keyBuilder = keyConvention.Apply(keyBuilder);
                if (keyBuilder == null)
                {
                    break;
                }
            }

            return keyBuilder;
        }

        public virtual InternalModelBuilder OnModelBuilt([NotNull] InternalModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            foreach (var modelConvention in _conventionSet.ModelBuiltConventions)
            {
                modelBuilder = modelConvention.Apply(modelBuilder);
                if (modelBuilder == null)
                {
                    break;
                }
            }

            return modelBuilder;
        }

        public virtual InternalModelBuilder OnModelInitialized([NotNull] InternalModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            foreach (var modelConvention in _conventionSet.ModelInitializedConventions)
            {
                modelBuilder = modelConvention.Apply(modelBuilder);
                if (modelBuilder == null)
                {
                    break;
                }
            }

            return modelBuilder;
        }

        public virtual InternalRelationshipBuilder OnNavigationAdded([NotNull] InternalRelationshipBuilder relationshipBuilder, [NotNull] Navigation navigation)
        {
            Check.NotNull(relationshipBuilder, nameof(relationshipBuilder));
            Check.NotNull(navigation, nameof(navigation));

            foreach (var navigationConvention in _conventionSet.NavigationAddedConventions)
            {
                relationshipBuilder = navigationConvention.Apply(relationshipBuilder, navigation);
                if (relationshipBuilder == null)
                {
                    break;
                }
            }

            return relationshipBuilder;
        }

        public virtual InternalPropertyBuilder OnPropertyAdded([NotNull] InternalPropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            foreach (var propertyConvention in _conventionSet.PropertyAddedConventions)
            {
                propertyBuilder = propertyConvention.Apply(propertyBuilder);
                if (propertyBuilder == null)
                {
                    break;
                }
            }

            return propertyBuilder;
        }
    }
}
