// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ConventionDispatcher
    {
        private readonly ConventionSet _conventionSet;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ConventionDispatcher([NotNull] ConventionSet conventionSet)
        {
            Check.NotNull(conventionSet, nameof(conventionSet));

            _conventionSet = conventionSet;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool OnEntityTypeIgnored(
            [NotNull] InternalModelBuilder modelBuilder, [NotNull] string name, [CanBeNull] Type type)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(name, nameof(name));

            foreach (var entityTypeIgnoredConvention in _conventionSet.EntityTypeIgnoredConventions)
            {
                if (!entityTypeIgnoredConvention.Apply(modelBuilder, name, type))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder OnEntityTypeMemberIgnored(
            [NotNull] InternalEntityTypeBuilder entityTypeBuilder,
            [NotNull] string ignoredMemberName)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotEmpty(ignoredMemberName, nameof(ignoredMemberName));

            foreach (var entityType in entityTypeBuilder.Metadata.GetDerivedTypesInclusive())
            {
                foreach (var entityTypeMemberIgnoredConvention in _conventionSet.EntityTypeMemberIgnoredConventions)
                {
                    if (!entityTypeMemberIgnoredConvention.Apply(entityType.Builder, ignoredMemberName))
                    {
                        break;
                    }
                }
            }

            return entityTypeBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder OnBaseEntityTypeSet(
            [NotNull] InternalEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] EntityType previousBaseType)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            foreach (var entityTypeConvention in _conventionSet.BaseEntityTypeSetConventions)
            {
                if (!entityTypeConvention.Apply(entityTypeBuilder, previousBaseType))
                {
                    break;
                }
            }

            return entityTypeBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
                Debug.Assert(relationshipBuilder.Metadata.Builder == relationshipBuilder);
            }

            return relationshipBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void OnForeignKeyRemoved([NotNull] InternalEntityTypeBuilder entityTypeBuilder, [NotNull] ForeignKey foreignKey)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(foreignKey, nameof(foreignKey));

            foreach (var foreignKeyConvention in _conventionSet.ForeignKeyRemovedConventions)
            {
                foreignKeyConvention.Apply(entityTypeBuilder, foreignKey);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void OnKeyRemoved([NotNull] InternalEntityTypeBuilder entityTypeBuilder, [NotNull] Key key)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(key, nameof(key));

            foreach (var keyConvention in _conventionSet.KeyRemovedConventions)
            {
                keyConvention.Apply(entityTypeBuilder, key);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalKeyBuilder OnPrimaryKeySet([NotNull] InternalKeyBuilder keyBuilder, [CanBeNull] Key previousPrimaryKey)
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));

            foreach (var keyConvention in _conventionSet.PrimaryKeySetConventions)
            {
                if (!keyConvention.Apply(keyBuilder, previousPrimaryKey))
                {
                    break;
                }
            }

            return keyBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalIndexBuilder OnIndexAdded([NotNull] InternalIndexBuilder indexBuilder)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            foreach (var indexConvention in _conventionSet.IndexAddedConventions)
            {
                indexBuilder = indexConvention.Apply(indexBuilder);
                if (indexBuilder == null)
                {
                    break;
                }
            }

            return indexBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void OnIndexRemoved([NotNull] InternalEntityTypeBuilder entityTypeBuilder, [NotNull] Index index)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(index, nameof(index));

            foreach (var indexConvention in _conventionSet.IndexRemovedConventions)
            {
                indexConvention.Apply(entityTypeBuilder, index);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool OnIndexUniquenessChanged([NotNull] InternalIndexBuilder indexBuilder)
        {
            foreach (var indexUniquenessConvention in _conventionSet.IndexUniquenessConventions)
            {
                if (!indexUniquenessConvention.Apply(indexBuilder))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void OnNavigationRemoved(
            [NotNull] InternalEntityTypeBuilder sourceEntityTypeBuilder,
            [NotNull] InternalEntityTypeBuilder targetEntityTypeBuilder,
            [NotNull] string navigationName,
            [CanBeNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(sourceEntityTypeBuilder, nameof(sourceEntityTypeBuilder));
            Check.NotNull(targetEntityTypeBuilder, nameof(targetEntityTypeBuilder));
            Check.NotNull(navigationName, nameof(navigationName));

            foreach (var navigationConvention in _conventionSet.NavigationRemovedConventions)
            {
                if (!navigationConvention.Apply(sourceEntityTypeBuilder, targetEntityTypeBuilder, navigationName, propertyInfo))
                {
                    break;
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool OnForeignKeyUniquenessChanged([NotNull] InternalRelationshipBuilder relationshipBuilder)
        {
            foreach (var uniquenessConvention in _conventionSet.ForeignKeyUniquenessConventions)
            {
                if (!uniquenessConvention.Apply(relationshipBuilder))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder OnPrincipalEndSet([NotNull] InternalRelationshipBuilder relationshipBuilder)
        {
            Check.NotNull(relationshipBuilder, nameof(relationshipBuilder));

            foreach (var relationshipConvention in _conventionSet.PrincipalEndSetConventions)
            {
                relationshipBuilder = relationshipConvention.Apply(relationshipBuilder);
                if (relationshipBuilder == null)
                {
                    break;
                }
            }

            return relationshipBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool OnPropertyNullableChanged([NotNull] InternalPropertyBuilder propertyBuilder)
        {
            foreach (var propertyConvention in _conventionSet.PropertyNullableChangedConventions)
            {
                if (!propertyConvention.Apply(propertyBuilder))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool OnPropertyFieldChanged(
            [NotNull] InternalPropertyBuilder propertyBuilder, [CanBeNull] FieldInfo oldFieldInfo)
        {
            foreach (var propertyConvention in _conventionSet.PropertyFieldChangedConventions)
            {
                if (!propertyConvention.Apply(propertyBuilder, oldFieldInfo))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
