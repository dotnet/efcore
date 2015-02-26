// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.ModelConventions
{
    public class ConventionDispatcher
    {
        private readonly ConventionSet _conventionSet;

        public ConventionDispatcher([NotNull] ConventionSet conventionSet)
        {
            Check.NotNull(conventionSet, nameof(conventionSet));

            _conventionSet = conventionSet;
        }

        public virtual InternalEntityBuilder OnEntityTypeAdded([NotNull] InternalEntityBuilder entityBuilder)
        {
            Check.NotNull(entityBuilder, nameof(entityBuilder));

            foreach (var entityTypeConvention in _conventionSet.EntityTypeAddedConventions)
            {
                entityBuilder = entityTypeConvention.Apply(entityBuilder);
                if (entityBuilder == null)
                {
                    break;
                }
            }

            return entityBuilder;
        }

        public virtual InternalKeyBuilder OnKeyAdded([NotNull] InternalKeyBuilder keyBuilder)
        {
            Check.NotNull(keyBuilder, "keyBuilder");

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

        public virtual void OnForeignKeyRemoved([NotNull] InternalEntityBuilder entityBuilder, [NotNull] ForeignKey foreignKey)
        {
            Check.NotNull(entityBuilder, "entityBuilder");
            Check.NotNull(foreignKey, "foreignKey");

            foreach (var keyConvention in _conventionSet.ForeignKeyRemovedConventions)
            {
                keyConvention.Apply(entityBuilder, foreignKey);
            }
        }

        public virtual InternalRelationshipBuilder OnRelationshipAdded([NotNull] InternalRelationshipBuilder relationshipBuilder)
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
    }
}
