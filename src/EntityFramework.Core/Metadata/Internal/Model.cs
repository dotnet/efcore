// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class Model : ConventionalAnnotatable, IMutableModel
    {
        private readonly SortedDictionary<string, EntityType> _entityTypes = new SortedDictionary<string, EntityType>();

        private readonly Dictionary<string, ConfigurationSource> _ignoredEntityTypeNames
            = new Dictionary<string, ConfigurationSource>();

        public Model()
            : this(new ConventionSet())
        {
        }

        public Model([NotNull] ConventionSet conventions)
        {
            ConventionDispatcher = new ConventionDispatcher(conventions);
            Builder = new InternalModelBuilder(this);
        }

        public virtual ConventionDispatcher ConventionDispatcher { get; }
        public virtual InternalModelBuilder Builder { get; }

        public virtual IEnumerable<EntityType> GetEntityTypes() => _entityTypes.Values;

        public virtual EntityType AddEntityType(
            [NotNull] string name, ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            Check.NotEmpty(name, nameof(name));

            var entityType = new EntityType(name, this, configurationSource);
            var previousLength = _entityTypes.Count;
            _entityTypes[name] = entityType;

            if (previousLength == _entityTypes.Count)
            {
                throw new InvalidOperationException(CoreStrings.DuplicateEntityType(entityType.Name));
            }

            return entityType;
        }

        public virtual EntityType AddEntityType(
            [NotNull] Type type, ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            Check.NotNull(type, nameof(type));

            var entityType = AddEntityType(type.DisplayName(), configurationSource);
            entityType.ClrType = type;
            return entityType;
        }

        public virtual EntityType GetOrAddEntityType([NotNull] Type type)
            => FindEntityType(type) ?? AddEntityType(type);

        public virtual EntityType GetOrAddEntityType([NotNull] string name)
            => FindEntityType(name) ?? AddEntityType(name);

        public virtual EntityType FindEntityType([NotNull] Type type)
            => (EntityType)((IMutableModel)this).FindEntityType(type);

        public virtual EntityType FindEntityType([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            EntityType entityType;
            return _entityTypes.TryGetValue(name, out entityType)
                ? entityType
                : null;
        }

        public virtual EntityType RemoveEntityType([NotNull] Type type)
        {
            var entityType = FindEntityType(type);
            return entityType == null
                ? null
                : RemoveEntityType(entityType);
        }

        public virtual EntityType RemoveEntityType([NotNull] string name)
        {
            var entityType = FindEntityType(name);
            return entityType == null
                ? null
                : RemoveEntityType(entityType);
        }

        private EntityType RemoveEntityType([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            var referencingForeignKey = entityType.GetDeclaredReferencingForeignKeys().FirstOrDefault();
            if (referencingForeignKey != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypeInUseByForeignKey(
                        entityType.DisplayName(),
                        Property.Format(referencingForeignKey.Properties),
                        referencingForeignKey.DeclaringEntityType.DisplayName()));
            }

            var derivedEntityType = entityType.GetDirectlyDerivedTypes().FirstOrDefault();
            if (derivedEntityType != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypeInUseByDerived(
                        entityType.DisplayName(),
                        derivedEntityType.DisplayName()));
            }

            var removed = _entityTypes.Remove(entityType.Name);
            Debug.Assert(removed);

            return entityType;
        }

        public virtual void Ignore([NotNull] Type type, ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            Check.NotNull(type, nameof(type));
            Ignore(type.DisplayName(), configurationSource);
        }

        public virtual void Ignore([NotNull] string name, ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            Check.NotNull(name, nameof(name));

            ConfigurationSource existingIgnoredConfigurationSource;
            if (_ignoredEntityTypeNames.TryGetValue(name, out existingIgnoredConfigurationSource))
            {
                configurationSource = configurationSource.Max(existingIgnoredConfigurationSource);
            }

            _ignoredEntityTypeNames[name] = configurationSource;
        }

        public virtual ConfigurationSource? FindIgnoredEntityTypeConfigurationSource([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            return FindIgnoredEntityTypeConfigurationSource(type.DisplayName());
        }

        public virtual ConfigurationSource? FindIgnoredEntityTypeConfigurationSource([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            ConfigurationSource ignoredConfigurationSource;
            if (_ignoredEntityTypeNames.TryGetValue(name, out ignoredConfigurationSource))
            {
                return ignoredConfigurationSource;
            }

            return null;
        }

        public virtual void Unignore([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));
           Unignore(type.DisplayName());
        }

        public virtual void Unignore([NotNull] string name)
        {
            Check.NotNull(name, nameof(name));
             _ignoredEntityTypeNames.Remove(name);
        }

        IEntityType IModel.FindEntityType(string name) => FindEntityType(name);
        IEnumerable<IEntityType> IModel.GetEntityTypes() => GetEntityTypes();

        IMutableEntityType IMutableModel.AddEntityType(string name) => AddEntityType(name);
        IEnumerable<IMutableEntityType> IMutableModel.GetEntityTypes() => GetEntityTypes();
        IMutableEntityType IMutableModel.FindEntityType(string name) => FindEntityType(name);
        IMutableEntityType IMutableModel.RemoveEntityType(string name) => RemoveEntityType(name);
    }
}
