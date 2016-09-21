// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class Model : ConventionalAnnotatable, IMutableModel
    {
        private readonly SortedDictionary<string, TypeBase> _mappedTypes
            = new SortedDictionary<string, TypeBase>();

        private readonly IDictionary<Type, TypeBase> _clrTypeMap
            = new Dictionary<Type, TypeBase>();

        private readonly Dictionary<string, ConfigurationSource> _ignoredTypeNames
            = new Dictionary<string, ConfigurationSource>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Model()
            : this(new ConventionSet())
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Model([NotNull] ConventionSet conventions)
        {
            ConventionDispatcher = new ConventionDispatcher(conventions);
            Builder = new InternalModelBuilder(this);
            ConventionDispatcher.OnModelInitialized(Builder);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ChangeTrackingStrategy ChangeTrackingStrategy { get; set; }
            = ChangeTrackingStrategy.Snapshot;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConventionDispatcher ConventionDispatcher { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Builder { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<EntityType> GetEntityTypes()
            => _mappedTypes.Values.OfType<EntityType>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ComplexTypeDefinition> GetComplexTypeDefinitions()
            => _mappedTypes.Values.OfType<ComplexTypeDefinition>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<TypeBase> GetMappedTypes() => _mappedTypes.Values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType AddEntityType(
            [NotNull] string name,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            Check.NotEmpty(name, nameof(name));

            return AddEntityType(new EntityType(name, this, configurationSource), runConventions);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType AddEntityType(
            [NotNull] Type type,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            Check.NotNull(type, nameof(type));

            var entityType = new EntityType(type, this, configurationSource);

            _clrTypeMap[type] = entityType;

            return AddEntityType(entityType, runConventions);
        }

        private EntityType AddEntityType(EntityType entityType, bool runConventions)
        {
            AddType(entityType);

            return runConventions ? ConventionDispatcher.OnEntityTypeAdded(entityType.Builder)?.Metadata : entityType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeDefinition AddComplexTypeDefinition(
            [NotNull] string name,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            Check.NotEmpty(name, nameof(name));

            return AddComplexTypeDefinition(new ComplexTypeDefinition(name, this, configurationSource), runConventions);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeDefinition AddComplexTypeDefinition(
            [NotNull] Type type,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            Check.NotNull(type, nameof(type));

            var complexType = new ComplexTypeDefinition(type, this, configurationSource);

            _clrTypeMap[type] = complexType;

            return AddComplexTypeDefinition(complexType, runConventions);
        }

        private ComplexTypeDefinition AddComplexTypeDefinition(ComplexTypeDefinition complexType, bool runConventions)
        {
            AddType(complexType);

            // TOSO: Builders
            //return runConventions ? ConventionDispatcher.OnEntityTypeAdded(complexType.Builder)?.Metadata : complexType;
            return complexType;
        }

        private void AddType(TypeBase type)
        {
            var existing = FindMappedType(type.Name);
            if (existing != null)
            {
                if (existing is EntityType)
                {
                    if (type is EntityType)
                    {
                        throw new InvalidOperationException(CoreStrings.DuplicateEntityType(type.DisplayName()));
                    }
                    throw new InvalidOperationException(CoreStrings.EntityTypeAlreadyExists(type.DisplayName()));
                }
                if (type is ComplexTypeDefinition)
                {
                    throw new InvalidOperationException(CoreStrings.DuplicateComplexType(type.DisplayName()));
                }
                throw new InvalidOperationException(CoreStrings.ComplexTypeAlreadyExists(type.DisplayName()));
            }

            _mappedTypes[type.Name] = type;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType GetOrAddEntityType([NotNull] Type type)
            => FindEntityType(type) ?? AddEntityType(type);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType GetOrAddEntityType([NotNull] string name)
            => FindEntityType(name) ?? AddEntityType(name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeDefinition GetOrAddComplexTypeDefinition([NotNull] Type type)
            => FindComplexTypeDefinition(type) ?? AddComplexTypeDefinition(type);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeDefinition GetOrAddComplexTypeDefinition([NotNull] string name)
            => FindComplexTypeDefinition(name) ?? AddComplexTypeDefinition(name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType FindEntityType([NotNull] Type type)
            => FindMappedType(type) as EntityType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType FindEntityType([NotNull] string name)
            => FindMappedType(name) as EntityType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeDefinition FindComplexTypeDefinition([NotNull] Type type)
            => FindMappedType(type) as ComplexTypeDefinition;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeDefinition FindComplexTypeDefinition([NotNull] string name)
            => FindMappedType(name) as ComplexTypeDefinition;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TypeBase FindMappedType([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            TypeBase entityType;
            return _clrTypeMap.TryGetValue(type, out entityType)
                ? entityType
                : FindEntityType(type.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TypeBase FindMappedType([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            TypeBase entityType;
            return _mappedTypes.TryGetValue(name, out entityType)
                ? entityType
                : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType RemoveEntityType([NotNull] Type type)
            => (EntityType)RemoveMappedType(FindEntityType(type));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType RemoveEntityType([NotNull] string name)
            => (EntityType)RemoveMappedType(FindEntityType(name));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeDefinition RemoveComplexTypeDefinition([NotNull] Type type)
            => (ComplexTypeDefinition)RemoveMappedType(FindComplexTypeDefinition(type));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeDefinition RemoveComplexTypeDefinition([NotNull] string name)
            => (ComplexTypeDefinition)RemoveMappedType(FindComplexTypeDefinition(name));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TypeBase RemoveMappedType([NotNull] Type type)
            => RemoveMappedType(FindMappedType(type));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TypeBase RemoveMappedType([NotNull] string name)
            => RemoveMappedType(FindMappedType(name));

        private TypeBase RemoveMappedType(TypeBase type)
        {
            if (type == null)
            {
                return null;
            }

            // TODO: FKs that use Complex Type
            var entityType = type as EntityType;

            var referencingForeignKey = entityType?.GetDeclaredReferencingForeignKeys().FirstOrDefault();
            if (referencingForeignKey != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypeInUseByForeignKey(
                        type.DisplayName(),
                        Property.Format(referencingForeignKey.Properties),
                        referencingForeignKey.DeclaringEntityType.DisplayName()));
            }

            var derivedEntityType = entityType?.GetDirectlyDerivedTypes().FirstOrDefault();
            if (derivedEntityType != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypeInUseByDerived(
                        type.DisplayName(),
                        derivedEntityType.DisplayName()));
            }

            if (type.ClrType != null)
            {
                _clrTypeMap.Remove(type.ClrType);
            }

            var removed = _mappedTypes.Remove(type.Name);
            Debug.Assert(removed);

            if (entityType != null)
            {
                // TODO: Builders
                entityType.Builder = null;
            }

            return type;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Ignore([NotNull] Type type,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
            => Ignore(Check.NotNull(type, nameof(type)).DisplayName(), type, configurationSource, runConventions);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Ignore([NotNull] string name,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
            => Ignore(Check.NotNull(name, nameof(name)), null, configurationSource, runConventions);

        private void Ignore([NotNull] string name,
            [CanBeNull] Type type,
            ConfigurationSource configurationSource,
            bool runConventions)
        {
            ConfigurationSource existingIgnoredConfigurationSource;
            if (_ignoredTypeNames.TryGetValue(name, out existingIgnoredConfigurationSource))
            {
                configurationSource = configurationSource.Max(existingIgnoredConfigurationSource);
                runConventions = false;
            }

            _ignoredTypeNames[name] = configurationSource;

            if (runConventions)
            {
                ConventionDispatcher.OnEntityTypeIgnored(Builder, name, type);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? FindIgnoredTypeConfigurationSource([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            return FindIgnoredTypeConfigurationSource(type.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? FindIgnoredTypeConfigurationSource([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            ConfigurationSource ignoredConfigurationSource;
            return _ignoredTypeNames.TryGetValue(name, out ignoredConfigurationSource)
                ? (ConfigurationSource?)ignoredConfigurationSource
                : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Unignore([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));
            Unignore(type.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Unignore([NotNull] string name)
        {
            Check.NotNull(name, nameof(name));
            _ignoredTypeNames.Remove(name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Validate() => ConventionDispatcher.OnModelBuilt(Builder);

        IEntityType IModel.FindEntityType(string name) => FindEntityType(name);
        IEnumerable<IEntityType> IModel.GetEntityTypes() => GetEntityTypes();

        IMutableEntityType IMutableModel.AddEntityType(string name) => AddEntityType(name);
        IMutableEntityType IMutableModel.AddEntityType(Type type) => AddEntityType(type);
        IEnumerable<IMutableEntityType> IMutableModel.GetEntityTypes() => GetEntityTypes();
        IMutableEntityType IMutableModel.FindEntityType(string name) => FindEntityType(name);
        IMutableEntityType IMutableModel.RemoveEntityType(string name) => RemoveEntityType(name);

        IComplexTypeDefinition IModel.FindComplexTypeDefinition(string name) => FindComplexTypeDefinition(name);
        IEnumerable<IComplexTypeDefinition> IModel.GetComplexTypeDefinitions() => GetComplexTypeDefinitions();

        IMutableComplexTypeDefinition IMutableModel.AddComplexTypeDefinition(string name) => AddComplexTypeDefinition(name);
        IMutableComplexTypeDefinition IMutableModel.AddComplexTypeDefinition(Type type) => AddComplexTypeDefinition(type);
        IEnumerable<IMutableComplexTypeDefinition> IMutableModel.GetComplexTypeDefinitions() => GetComplexTypeDefinitions();
        IMutableComplexTypeDefinition IMutableModel.FindComplexTypeDefinition(string name) => FindComplexTypeDefinition(name);
        IMutableComplexTypeDefinition IMutableModel.RemoveComplexTypeDefinition(string name) => RemoveComplexTypeDefinition(name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DebugView<Model> DebugView
            => new DebugView<Model>(this, m => m.ToDebugString());
    }
}
