// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EntityType : ConventionalAnnotatable, IMutableEntityType
    {
        private readonly SortedSet<ForeignKey> _foreignKeys
            = new SortedSet<ForeignKey>(ForeignKeyComparer.Instance);

        private readonly SortedDictionary<string, Navigation> _navigations
            = new SortedDictionary<string, Navigation>(StringComparer.Ordinal);

        private readonly SortedDictionary<IReadOnlyList<IProperty>, Index> _indexes
            = new SortedDictionary<IReadOnlyList<IProperty>, Index>(PropertyListComparer.Instance);

        private readonly SortedDictionary<string, Property> _properties;

        private readonly SortedDictionary<IReadOnlyList<IProperty>, Key> _keys
            = new SortedDictionary<IReadOnlyList<IProperty>, Key>(PropertyListComparer.Instance);

        private readonly object _typeOrName;
        private Key _primaryKey;
        private EntityType _baseType;

        private ChangeTrackingStrategy? _changeTrackingStrategy;

        private ConfigurationSource _configurationSource;
        private ConfigurationSource? _baseTypeConfigurationSource;
        private ConfigurationSource? _primaryKeyConfigurationSource;
        private readonly Dictionary<string, ConfigurationSource> _ignoredMembers = new Dictionary<string, ConfigurationSource>();

        // Warning: Never access these fields directly as access needs to be thread-safe
        private PropertyCounts _counts;
        private Func<InternalEntityEntry, ISnapshot> _relationshipSnapshotFactory;
        private Func<InternalEntityEntry, ISnapshot> _originalValuesFactory;
        private Func<ValueBuffer, ISnapshot> _shadowValuesFactory;
        private Func<ISnapshot> _emptyShadowValuesFactory;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityType([NotNull] string name, [NotNull] Model model, ConfigurationSource configurationSource)
            : this(model, configurationSource)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(model, nameof(model));

            _typeOrName = name;
#if DEBUG
            DebugName = this.DisplayName();
#endif
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityType([NotNull] Type clrType, [NotNull] Model model, ConfigurationSource configurationSource)
            : this(model, configurationSource)
        {
            Check.ValidEntityType(clrType, nameof(clrType));
            Check.NotNull(model, nameof(model));

            _typeOrName = clrType;
#if DEBUG
            DebugName = this.DisplayName();
#endif
        }

        private EntityType([NotNull] Model model, ConfigurationSource configurationSource)
        {
            Model = model;
            _configurationSource = configurationSource;
            _properties = new SortedDictionary<string, Property>(new PropertyComparer(this));
            Builder = new InternalEntityTypeBuilder(this, model.Builder);
        }

#if DEBUG
        [UsedImplicitly]
        private string DebugName { get; set; }
#endif

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Builder { [DebuggerStepThrough] get; [DebuggerStepThrough] [param: CanBeNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Type ClrType => _typeOrName as Type;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Model Model { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType BaseType => _baseType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void HasBaseType(
            [CanBeNull] EntityType entityType,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            if (_baseType == entityType)
            {
                UpdateBaseTypeConfigurationSource(configurationSource);
                entityType?.UpdateConfigurationSource(configurationSource);
                return;
            }

            var originalBaseType = _baseType;
            _baseType?._directlyDerivedTypes.Remove(this);
            _baseType = null;
            if (entityType != null)
            {
                if (this.HasClrType())
                {
                    if (!entityType.HasClrType())
                    {
                        throw new InvalidOperationException(CoreStrings.NonClrBaseType(this.DisplayName(), entityType.DisplayName()));
                    }

                    if (!entityType.ClrType.GetTypeInfo().IsAssignableFrom(ClrType.GetTypeInfo()))
                    {
                        throw new InvalidOperationException(CoreStrings.NotAssignableClrBaseType(this.DisplayName(), entityType.DisplayName(), ClrType.ShortDisplayName(), entityType.ClrType.ShortDisplayName()));
                    }
                }

                if (!this.HasClrType()
                    && entityType.HasClrType())
                {
                    throw new InvalidOperationException(CoreStrings.NonShadowBaseType(this.DisplayName(), entityType.DisplayName()));
                }

                if (entityType.InheritsFrom(this))
                {
                    throw new InvalidOperationException(CoreStrings.CircularInheritance(this.DisplayName(), entityType.DisplayName()));
                }

                if (_keys.Any())
                {
                    throw new InvalidOperationException(CoreStrings.DerivedEntityCannotHaveKeys(this.DisplayName()));
                }

                var propertyCollisions = entityType.GetProperties()
                    .Select(p => p.Name)
                    .SelectMany(FindPropertiesInHierarchy)
                    .ToList();
                if (propertyCollisions.Any())
                {
                    throw new InvalidOperationException(
                        CoreStrings.DuplicatePropertiesOnBase(
                            this.DisplayName(),
                            entityType.DisplayName(),
                            string.Join(", ", propertyCollisions.Select(p => p.Name))));
                }

                var navigationCollisions = entityType.GetNavigations()
                    .Select(p => p.Name)
                    .SelectMany(FindNavigationsInHierarchy)
                    .ToList();
                if (navigationCollisions.Any())
                {
                    throw new InvalidOperationException(
                        CoreStrings.DuplicateNavigationsOnBase(
                            this.DisplayName(),
                            entityType.DisplayName(),
                            string.Join(", ", navigationCollisions.Select(p => p.Name))));
                }

                _baseType = entityType;
                _baseType._directlyDerivedTypes.Add(this);
            }

            PropertyMetadataChanged();
            UpdateBaseTypeConfigurationSource(configurationSource);
            entityType?.UpdateConfigurationSource(configurationSource);

            if (runConventions)
            {
                Model.ConventionDispatcher.OnBaseEntityTypeSet(Builder, originalBaseType);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetBaseTypeConfigurationSource() => _baseTypeConfigurationSource;

        private void UpdateBaseTypeConfigurationSource(ConfigurationSource configurationSource)
            => _baseTypeConfigurationSource = configurationSource.Max(_baseTypeConfigurationSource);

        private readonly List<EntityType> _directlyDerivedTypes = new List<EntityType>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<EntityType> GetDirectlyDerivedTypes() => _directlyDerivedTypes;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<EntityType> GetDerivedTypes()
        {
            var derivedTypes = new List<EntityType>();
            var type = this;
            var currentTypeIndex = 0;
            while (type != null)
            {
                derivedTypes.AddRange(type.GetDirectlyDerivedTypes());
                type = derivedTypes.Count > currentTypeIndex
                    ? derivedTypes[currentTypeIndex]
                    : null;
                currentTypeIndex++;
            }
            return derivedTypes;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<EntityType> GetDerivedTypesInclusive()
            => new[] { this }.Concat(GetDerivedTypes());

        private bool InheritsFrom(EntityType entityType)
        {
            var et = this;

            do
            {
                if (entityType == et)
                {
                    return true;
                }
            }
            while ((et = et._baseType) != null);

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType RootType() => (EntityType)((IEntityType)this).RootType();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Name
        {
            get
            {
                if (ClrType != null)
                {
                    return ClrType.DisplayName() ?? (string)_typeOrName;
                }
                return (string)_typeOrName;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string ToString() => Name;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource GetConfigurationSource() => _configurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
            => _configurationSource = _configurationSource.Max(configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ChangeTrackingStrategy ChangeTrackingStrategy
        {
            get { return _changeTrackingStrategy ?? Model.ChangeTrackingStrategy; }
            set
            {
                var errorMessage = this.CheckChangeTrackingStrategy(value);
                if (errorMessage != null)
                {
                    throw new InvalidOperationException(errorMessage);
                }

                _changeTrackingStrategy = value;

                PropertyMetadataChanged();
            }
        }

        #region Primary and Candidate Keys

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key SetPrimaryKey([CanBeNull] Property property)
            => SetPrimaryKey(property == null ? null : new[] { property });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key SetPrimaryKey(
            [CanBeNull] IReadOnlyList<Property> properties,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            if (_baseType != null)
            {
                throw new InvalidOperationException(CoreStrings.DerivedEntityTypeKey(this.DisplayName(), _baseType.DisplayName()));
            }

            var oldPrimaryKey = _primaryKey;
            if (oldPrimaryKey != null)
            {
                foreach (var property in _primaryKey.Properties)
                {
                    _properties.Remove(property.Name);
                    property.PrimaryKey = null;
                }

                _primaryKey = null;

                foreach (var property in oldPrimaryKey.Properties)
                {
                    _properties.Add(property.Name, property);
                }
            }

            if ((properties != null)
                && (properties.Count != 0))
            {
                var key = GetOrAddKey(properties);

                foreach (var property in key.Properties)
                {
                    _properties.Remove(property.Name);
                    property.PrimaryKey = key;
                }

                _primaryKey = key;

                foreach (var property in key.Properties)
                {
                    _properties.Add(property.Name, property);
                }
            }

            PropertyMetadataChanged();
            UpdatePrimaryKeyConfigurationSource(configurationSource);

            if (runConventions
                && _primaryKey != null)
            {
                Model.ConventionDispatcher.OnPrimaryKeySet(_primaryKey.Builder, oldPrimaryKey);
            }

            return _primaryKey;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key GetOrSetPrimaryKey([NotNull] Property property)
            => GetOrSetPrimaryKey(new[] { property });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key GetOrSetPrimaryKey([NotNull] IReadOnlyList<Property> properties)
            => FindPrimaryKey(properties) ?? SetPrimaryKey(properties);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key FindPrimaryKey()
            => _baseType?.FindPrimaryKey() ?? FindDeclaredPrimaryKey();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key FindDeclaredPrimaryKey() => _primaryKey;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key FindPrimaryKey([CanBeNull] IReadOnlyList<Property> properties)
        {
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotEmpty(properties, nameof(properties));

            if (_baseType != null)
            {
                return _baseType.FindPrimaryKey(properties);
            }

            if ((_primaryKey != null)
                && (PropertyListComparer.Instance.Compare(_primaryKey.Properties, properties) == 0))
            {
                return _primaryKey;
            }

            return null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetPrimaryKeyConfigurationSource() => _primaryKeyConfigurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        private void UpdatePrimaryKeyConfigurationSource(ConfigurationSource configurationSource)
            => _primaryKeyConfigurationSource = configurationSource.Max(_primaryKeyConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key AddKey([NotNull] Property property, ConfigurationSource configurationSource = ConfigurationSource.Explicit)
            => AddKey(new[] { property }, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key AddKey([NotNull] IReadOnlyList<Property> properties,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));

            if (_baseType != null)
            {
                throw new InvalidOperationException(CoreStrings.DerivedEntityTypeKey(this.DisplayName(), _baseType.DisplayName()));
            }

            foreach (var property in properties)
            {
                if (FindProperty(property.Name) != property)
                {
                    throw new InvalidOperationException(CoreStrings.KeyPropertiesWrongEntity(Property.Format(properties), this.DisplayName()));
                }

                if (property.GetContainingForeignKeys().Any(k => k.DeclaringEntityType != this))
                {
                    throw new InvalidOperationException(CoreStrings.KeyPropertyInForeignKey(property.Name, this.DisplayName()));
                }

                if (property.IsNullable)
                {
                    throw new InvalidOperationException(CoreStrings.NullableKey(this.DisplayName(), property.Name));
                }
            }

            var key = FindKey(properties);
            if (key != null)
            {
                throw new InvalidOperationException(CoreStrings.DuplicateKey(Property.Format(properties), this.DisplayName(), key.DeclaringEntityType.DisplayName()));
            }

            key = new Key(properties, configurationSource);
            _keys.Add(properties, key);

            foreach (var property in properties)
            {
                var currentKeys = property.Keys;
                if (currentKeys == null)
                {
                    property.Keys = new IKey[] { key };
                }
                else
                {
                    var newKeys = currentKeys.ToList();
                    newKeys.Add(key);
                    property.Keys = newKeys;
                }
            }

            PropertyMetadataChanged();

            return Model.ConventionDispatcher.OnKeyAdded(key.Builder)?.Metadata;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key GetOrAddKey([NotNull] Property property)
            => GetOrAddKey(new[] { property });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key GetOrAddKey([NotNull] IReadOnlyList<Property> properties)
            => FindKey(properties)
               ?? AddKey(properties);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key FindKey([NotNull] IProperty property) => FindKey(new[] { property });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key FindKey([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotEmpty(properties, nameof(properties));

            return FindDeclaredKey(properties) ?? _baseType?.FindKey(properties);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Key> GetDeclaredKeys() => _keys.Values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key FindDeclaredKey([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.NotEmpty(properties, nameof(properties));

            Key key;
            return _keys.TryGetValue(properties, out key)
                ? key
                : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // ReSharper disable once MethodOverloadWithOptionalParameter
        public virtual Key RemoveKey([NotNull] IReadOnlyList<IProperty> properties, bool runConventions = true)
        {
            Check.NotEmpty(properties, nameof(properties));

            var key = FindDeclaredKey(properties);
            return key == null
                ? null
                : RemoveKey(key, runConventions);
        }

        private Key RemoveKey([NotNull] Key key, bool runConventions)
        {
            CheckKeyNotInUse(key);

            if (_primaryKey == key)
            {
                SetPrimaryKey((IReadOnlyList<Property>)null);
                _primaryKeyConfigurationSource = null;
            }

            _keys.Remove(key.Properties);
            key.Builder = null;

            foreach (var property in key.Properties)
            {
                property.Keys =
                    property.Keys != null
                    && property.Keys.Count > 1
                        ? property.Keys.Where(k => k != key).ToList()
                        : null;
            }

            PropertyMetadataChanged();

            if (runConventions)
            {
                Model.ConventionDispatcher.OnKeyRemoved(Builder, key);
            }
            return key;
        }

        private void CheckKeyNotInUse(Key key)
        {
            var foreignKey = key.GetReferencingForeignKeys().FirstOrDefault();
            if (foreignKey != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.KeyInUse(Property.Format(key.Properties), this.DisplayName(), foreignKey.DeclaringEntityType.DisplayName()));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Key> GetKeys() => _baseType?.GetKeys().Concat(_keys.Values) ?? _keys.Values;

        #endregion

        #region Foreign Keys

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ForeignKey AddForeignKey(
            [NotNull] Property property,
            [NotNull] Key principalKey,
            [NotNull] EntityType principalEntityType,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
            => AddForeignKey(new[] { property }, principalKey, principalEntityType, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ForeignKey AddForeignKey(
            [NotNull] IReadOnlyList<Property> properties,
            [NotNull] Key principalKey,
            [NotNull] EntityType principalEntityType,
            ConfigurationSource? configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotNull(principalKey, nameof(principalKey));
            Check.NotNull(principalEntityType, nameof(principalEntityType));

            foreach (var property in properties)
            {
                var actualProperty = FindProperty(property.Name);
                if (actualProperty == null
                    || !actualProperty.DeclaringEntityType.IsAssignableFrom(property.DeclaringEntityType))
                {
                    throw new InvalidOperationException(CoreStrings.ForeignKeyPropertiesWrongEntity(Property.Format(properties), this.DisplayName()));
                }

                if (actualProperty.GetContainingKeys().Any(k => k.DeclaringEntityType != this))
                {
                    throw new InvalidOperationException(CoreStrings.ForeignKeyPropertyInKey(actualProperty.Name, this.DisplayName()));
                }
            }

            var duplicateForeignKey = FindForeignKeysInHierarchy(properties, principalKey, principalEntityType).FirstOrDefault();
            if (duplicateForeignKey != null)
            {
                throw new InvalidOperationException(CoreStrings.DuplicateForeignKey(
                    Property.Format(properties),
                    this.DisplayName(),
                    duplicateForeignKey.DeclaringEntityType.DisplayName(),
                    Property.Format(principalKey.Properties),
                    principalEntityType.DisplayName()));
            }

            var foreignKey = new ForeignKey(properties, principalKey, this, principalEntityType, configurationSource ?? ConfigurationSource.Convention);
            if (configurationSource.HasValue)
            {
                principalEntityType.UpdateConfigurationSource(configurationSource.Value);
                foreignKey.UpdateForeignKeyPropertiesConfigurationSource(configurationSource.Value);
                foreignKey.UpdatePrincipalKeyConfigurationSource(configurationSource.Value);
                foreignKey.UpdatePrincipalEndConfigurationSource(configurationSource.Value);
            }

            if (principalEntityType.Model != Model)
            {
                throw new InvalidOperationException(CoreStrings.EntityTypeModelMismatch(this, principalEntityType));
            }

            _foreignKeys.Add(foreignKey);

            foreach (var property in properties)
            {
                var currentKeys = property.ForeignKeys;
                if (currentKeys == null)
                {
                    property.ForeignKeys = new IForeignKey[] { foreignKey };
                }
                else
                {
                    var newKeys = currentKeys.ToList();
                    newKeys.Add(foreignKey);
                    property.ForeignKeys = newKeys;
                }
            }

            if (principalKey.ReferencingForeignKeys == null)
            {
                principalKey.ReferencingForeignKeys = new List<ForeignKey> { foreignKey };
            }
            else
            {
                principalKey.ReferencingForeignKeys.Add(foreignKey);
            }

            if (principalEntityType.DeclaredReferencingForeignKeys == null)
            {
                principalEntityType.DeclaredReferencingForeignKeys = new List<ForeignKey> { foreignKey };
            }
            else
            {
                principalEntityType.DeclaredReferencingForeignKeys.Add(foreignKey);
            }

            PropertyMetadataChanged();

            if (runConventions)
            {
                var builder = Model.ConventionDispatcher.OnForeignKeyAdded(foreignKey.Builder);
                if (builder != null
                    && configurationSource.HasValue)
                {
                    builder = Model.ConventionDispatcher.OnPrincipalEndSet(builder);
                }
                foreignKey = builder?.Metadata;
            }

            return foreignKey;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ForeignKey GetOrAddForeignKey(
            [NotNull] Property property, [NotNull] Key principalKey, [NotNull] EntityType principalEntityType)
            => GetOrAddForeignKey(new[] { property }, principalKey, principalEntityType);

        // Note: this will return an existing foreign key even if it doesn't have the same referenced key
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ForeignKey GetOrAddForeignKey(
            [NotNull] IReadOnlyList<Property> properties, [NotNull] Key principalKey, [NotNull] EntityType principalEntityType)
            => FindForeignKey(properties, principalKey, principalEntityType)
               ?? AddForeignKey(properties, principalKey, principalEntityType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindForeignKeys([NotNull] IProperty property)
            => FindForeignKeys(new[] { property });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindForeignKeys([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotEmpty(properties, nameof(properties));

            return _baseType?.FindForeignKeys(properties)?.Concat(FindDeclaredForeignKeys(properties))
                   ?? FindDeclaredForeignKeys(properties);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ForeignKey FindForeignKey(
            [NotNull] IProperty property,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType)
            => FindForeignKey(new[] { property }, principalKey, principalEntityType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ForeignKey FindForeignKey(
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType)
        {
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotEmpty(properties, nameof(properties));
            Check.NotNull(principalKey, nameof(principalKey));
            Check.NotNull(principalEntityType, nameof(principalEntityType));

            return FindDeclaredForeignKey(properties, principalKey, principalEntityType)
                   ?? _baseType?.FindForeignKey(properties, principalKey, principalEntityType);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetDeclaredForeignKeys() => _foreignKeys;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetDerivedForeignKeys()
            => GetDerivedTypes().SelectMany(et => et.GetDeclaredForeignKeys());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetDerivedForeignKeysInclusive()
            => GetDeclaredForeignKeys().Concat(GetDerivedTypes().SelectMany(et => et.GetDeclaredForeignKeys()));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindDeclaredForeignKeys([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.NotEmpty(properties, nameof(properties));

            return _foreignKeys.Where(fk => PropertyListComparer.Instance.Equals(fk.Properties, properties));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ForeignKey FindDeclaredForeignKey(
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.NotNull(principalKey, nameof(principalKey));
            Check.NotNull(principalEntityType, nameof(principalEntityType));

            return FindDeclaredForeignKeys(properties).SingleOrDefault(fk =>
                PropertyListComparer.Instance.Equals(fk.PrincipalKey.Properties, principalKey.Properties) &&
                StringComparer.Ordinal.Equals(fk.PrincipalEntityType.Name, principalEntityType.Name));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindDerivedForeignKeys(
            [NotNull] IReadOnlyList<IProperty> properties)
            => GetDerivedTypes().SelectMany(et => et.FindDeclaredForeignKeys(properties));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindDerivedForeignKeys(
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType)
            => GetDerivedTypes().Select(et => et.FindDeclaredForeignKey(properties, principalKey, principalEntityType))
                .Where(fk => fk != null);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindForeignKeysInHierarchy(
            [NotNull] IReadOnlyList<IProperty> properties)
            => FindForeignKeys(properties).Concat(FindDerivedForeignKeys(properties));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindForeignKeysInHierarchy(
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType)
            => ToEnumerable(FindForeignKey(properties, principalKey, principalEntityType))
                .Concat(FindDerivedForeignKeys(properties, principalKey, principalEntityType));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ForeignKey RemoveForeignKey(
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            bool runConventions = true)
        {
            Check.NotEmpty(properties, nameof(properties));

            var foreignKey = FindDeclaredForeignKey(properties, principalKey, principalEntityType);
            return foreignKey == null
                ? null
                : RemoveForeignKey(foreignKey, runConventions);
        }

        private ForeignKey RemoveForeignKey([NotNull] ForeignKey foreignKey, bool runConventions)
        {
            if (foreignKey.DependentToPrincipal != null)
            {
                foreignKey.DeclaringEntityType.RemoveNavigation(foreignKey.DependentToPrincipal.Name);
            }

            if (foreignKey.PrincipalToDependent != null)
            {
                foreignKey.PrincipalEntityType.RemoveNavigation(foreignKey.PrincipalToDependent.Name);
            }

            var removed = _foreignKeys.Remove(foreignKey);
            foreignKey.Builder = null;

            foreach (var property in foreignKey.Properties)
            {
                property.ForeignKeys =
                    property.ForeignKeys != null
                    && property.ForeignKeys.Count > 1
                        ? property.ForeignKeys.Where(k => k != foreignKey).ToList()
                        : null;
            }

            foreignKey.PrincipalKey.ReferencingForeignKeys.Remove(foreignKey);
            foreignKey.PrincipalEntityType.DeclaredReferencingForeignKeys.Remove(foreignKey);

            PropertyMetadataChanged();

            if (removed)
            {
                if (runConventions)
                {
                    if (foreignKey.DependentToPrincipal != null)
                    {
                        Model.ConventionDispatcher.OnNavigationRemoved(
                            Builder,
                            foreignKey.PrincipalEntityType.Builder,
                            foreignKey.DependentToPrincipal.Name,
                            foreignKey.DependentToPrincipal.PropertyInfo);
                    }

                    if (foreignKey.PrincipalToDependent != null)
                    {
                        Model.ConventionDispatcher.OnNavigationRemoved(
                            foreignKey.PrincipalEntityType.Builder,
                            Builder,
                            foreignKey.PrincipalToDependent.Name,
                            foreignKey.PrincipalToDependent.PropertyInfo);
                    }

                    Model.ConventionDispatcher.OnForeignKeyRemoved(Builder, foreignKey);
                }
                return foreignKey;
            }

            return null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetReferencingForeignKeys()
            => _baseType?.GetReferencingForeignKeys().Concat(GetDeclaredReferencingForeignKeys())
               ?? GetDeclaredReferencingForeignKeys();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetDeclaredReferencingForeignKeys()
            => DeclaredReferencingForeignKeys ?? Enumerable.Empty<ForeignKey>();

        private List<ForeignKey> DeclaredReferencingForeignKeys { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetForeignKeys()
            => _baseType?.GetForeignKeys().Concat(_foreignKeys) ?? _foreignKeys;

        #endregion

        #region Navigations

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Navigation AddNavigation(
            [NotNull] string name,
            [NotNull] ForeignKey foreignKey,
            bool pointsToPrincipal)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(foreignKey, nameof(foreignKey));

            return AddNavigation(new PropertyIdentity(name), foreignKey, pointsToPrincipal);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Navigation AddNavigation(
            [NotNull] PropertyInfo navigationProperty,
            [NotNull] ForeignKey foreignKey,
            bool pointsToPrincipal)
        {
            Check.NotNull(navigationProperty, nameof(navigationProperty));
            Check.NotNull(foreignKey, nameof(foreignKey));

            return AddNavigation(new PropertyIdentity(navigationProperty), foreignKey, pointsToPrincipal);
        }

        private Navigation AddNavigation(PropertyIdentity propertyIdentity, ForeignKey foreignKey, bool pointsToPrincipal)
        {
            var name = propertyIdentity.Name;
            var duplicateNavigation = FindNavigationsInHierarchy(name).FirstOrDefault();
            if (duplicateNavigation != null)
            {
                if (duplicateNavigation.ForeignKey != foreignKey)
                {
                    throw new InvalidOperationException(
                        CoreStrings.NavigationForWrongForeignKey(
                            duplicateNavigation.Name,
                            duplicateNavigation.DeclaringEntityType.DisplayName(),
                            Property.Format(foreignKey.Properties),
                            Property.Format(duplicateNavigation.ForeignKey.Properties)));
                }

                throw new InvalidOperationException(
                    CoreStrings.DuplicateNavigation(name, this.DisplayName(), duplicateNavigation.DeclaringEntityType.DisplayName()));
            }

            var duplicateProperty = FindPropertiesInHierarchy(name).FirstOrDefault();
            if (duplicateProperty != null)
            {
                throw new InvalidOperationException(CoreStrings.ConflictingProperty(name, this.DisplayName(),
                    duplicateProperty.DeclaringEntityType.DisplayName()));
            }

            Debug.Assert(!GetNavigations().Any(n => (n.ForeignKey == foreignKey) && (n.IsDependentToPrincipal() == pointsToPrincipal)),
                "There is another navigation corresponding to the same foreign key and pointing in the same direction.");

            Debug.Assert((pointsToPrincipal ? foreignKey.DeclaringEntityType : foreignKey.PrincipalEntityType) == this,
                "EntityType mismatch");

            Navigation navigation = null;
            var navigationProperty = propertyIdentity.Property;
            if (ClrType != null)
            {
                Navigation.IsCompatible(
                    propertyIdentity.Name,
                    navigationProperty,
                    this,
                    pointsToPrincipal ? foreignKey.PrincipalEntityType : foreignKey.DeclaringEntityType,
                    !pointsToPrincipal && !foreignKey.IsUnique,
                    shouldThrow: true);
                navigation = new Navigation(navigationProperty, foreignKey);
            }
            else
            {
                navigation = new Navigation(name, foreignKey);
            }

            _navigations.Add(name, navigation);

            PropertyMetadataChanged();

            return navigation;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Navigation FindNavigation([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            return FindDeclaredNavigation(name) ?? _baseType?.FindNavigation(name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Navigation FindNavigation([NotNull] PropertyInfo propertyInfo)
            => FindNavigation(Check.NotNull(propertyInfo, nameof(propertyInfo)).Name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Navigation FindDeclaredNavigation([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            Navigation navigation;
            return _navigations.TryGetValue(name, out navigation)
                ? navigation
                : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Navigation> GetDeclaredNavigations() => _navigations.Values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Navigation> FindDerivedNavigations([NotNull] string navigationName)
        {
            Check.NotNull(navigationName, nameof(navigationName));

            return GetDerivedTypes().Select(et => et.FindDeclaredNavigation(navigationName)).Where(n => n != null);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Navigation> FindNavigationsInHierarchy([NotNull] string propertyName)
            => ToEnumerable(FindNavigation(propertyName)).Concat(FindDerivedNavigations(propertyName));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Navigation RemoveNavigation([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            var navigation = FindDeclaredNavigation(name);
            if (navigation == null)
            {
                return null;
            }

            _navigations.Remove(name);

            PropertyMetadataChanged();

            return navigation;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Navigation> GetNavigations()
            => _baseType?.GetNavigations().Concat(_navigations.Values) ?? _navigations.Values;

        #endregion

        #region Indexes

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Index AddIndex([NotNull] Property property,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
            => AddIndex(new[] { property }, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Index AddIndex([NotNull] IReadOnlyList<Property> properties,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));

            foreach (var property in properties)
            {
                if (FindProperty(property.Name) != property)
                {
                    throw new InvalidOperationException(CoreStrings.IndexPropertiesWrongEntity(Property.Format(properties), this.DisplayName()));
                }
            }

            var duplicateIndex = FindIndexesInHierarchy(properties).FirstOrDefault();
            if (duplicateIndex != null)
            {
                throw new InvalidOperationException(CoreStrings.DuplicateIndex(Property.Format(properties), this.DisplayName(), duplicateIndex.DeclaringEntityType.DisplayName()));
            }

            var index = new Index(properties, this, configurationSource);
            _indexes.Add(properties, index);

            foreach (var property in properties)
            {
                var currentIndexes = property.Indexes;
                if (currentIndexes == null)
                {
                    property.Indexes = new IIndex[] { index };
                }
                else
                {
                    var newIndex = currentIndexes.ToList();
                    newIndex.Add(index);
                    property.Indexes = newIndex;
                }
            }

            return Model.ConventionDispatcher.OnIndexAdded(index.Builder)?.Metadata;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Index GetOrAddIndex([NotNull] Property property)
            => GetOrAddIndex(new[] { property });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Index GetOrAddIndex([NotNull] IReadOnlyList<Property> properties)
            => FindIndex(properties) ?? AddIndex(properties);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Index FindIndex([NotNull] IProperty property)
            => FindIndex(new[] { property });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Index FindIndex([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotEmpty(properties, nameof(properties));

            return FindDeclaredIndex(properties) ?? _baseType?.FindIndex(properties);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Index> GetDeclaredIndexes() => _indexes.Values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Index> GetDerivedIndexes()
            => GetDerivedTypes().SelectMany(et => et.GetDeclaredIndexes());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Index> GetDerivedIndexesInclusive()
            => GetDeclaredIndexes().Concat(GetDerivedTypes().SelectMany(et => et.GetDeclaredIndexes()));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Index FindDeclaredIndex([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.NotEmpty(properties, nameof(properties));

            Index index;
            return _indexes.TryGetValue(properties, out index)
                ? index
                : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Index> FindDerivedIndexes([NotNull] IReadOnlyList<IProperty> properties)
            => GetDerivedTypes().Select(et => et.FindDeclaredIndex(properties)).Where(i => i != null);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Index> FindIndexesInHierarchy([NotNull] IReadOnlyList<IProperty> properties)
            => ToEnumerable(FindIndex(properties)).Concat(FindDerivedIndexes(properties));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Index RemoveIndex([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.NotEmpty(properties, nameof(properties));

            var index = FindDeclaredIndex(properties);
            return index == null
                ? null
                : RemoveIndex(index);
        }

        private Index RemoveIndex(Index index)
        {
            _indexes.Remove(index.Properties);
            index.Builder = null;

            foreach (var property in index.Properties)
            {
                property.Indexes =
                    property.Indexes != null
                    && property.Indexes.Count > 1
                        ? property.Indexes.Where(k => k != index).ToList()
                        : null;
            }

            Model.ConventionDispatcher.OnIndexRemoved(Builder, index);

            return index;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Index> GetIndexes() => _baseType?.GetIndexes().Concat(_indexes.Values) ?? _indexes.Values;

        #endregion

        #region Properties

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Property AddProperty(
            [NotNull] string name,
            [CanBeNull] Type propertyType = null,
            bool? shadow = null,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            Check.NotNull(name, nameof(name));

            ValidateCanAddProperty(name);

            if (shadow != true)
            {
                var clrProperty = ClrType?.GetPropertiesInHierarchy(name).FirstOrDefault();
                if (clrProperty != null)
                {
                    if (propertyType != null
                        && propertyType != clrProperty.PropertyType)
                    {
                        throw new InvalidOperationException(CoreStrings.PropertyWrongClrType(
                            name,
                            this.DisplayName(),
                            clrProperty.PropertyType.ShortDisplayName(),
                            propertyType.ShortDisplayName()));
                    }

                    return AddProperty(clrProperty, configurationSource, runConventions);
                }

                if (shadow == false)
                {
                    if (ClrType == null)
                    {
                        throw new InvalidOperationException(CoreStrings.ClrPropertyOnShadowEntity(name, this.DisplayName()));
                    }

                    throw new InvalidOperationException(CoreStrings.NoClrProperty(name, this.DisplayName()));
                }
            }

            if (propertyType == null)
            {
                throw new InvalidOperationException(CoreStrings.NoPropertyType(name, this.DisplayName()));
            }
            return AddProperty(new Property(name, propertyType, this, configurationSource), runConventions);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Property AddProperty(
            [NotNull] PropertyInfo propertyInfo,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            ValidateCanAddProperty(propertyInfo.Name);

            if (ClrType == null)
            {
                throw new InvalidOperationException(CoreStrings.ClrPropertyOnShadowEntity(propertyInfo.Name, this.DisplayName()));
            }

            if (propertyInfo.DeclaringType == null
                || !propertyInfo.DeclaringType.GetTypeInfo().IsAssignableFrom(ClrType.GetTypeInfo()))
            {
                throw new ArgumentException(CoreStrings.PropertyWrongEntityClrType(
                    propertyInfo.Name, this.DisplayName(), propertyInfo.DeclaringType?.ShortDisplayName()));
            }

            return AddProperty(new Property(propertyInfo, this, configurationSource), runConventions);
        }

        private void ValidateCanAddProperty(string name)
        {
            var duplicateProperty = FindPropertiesInHierarchy(name).FirstOrDefault();
            if (duplicateProperty != null)
            {
                throw new InvalidOperationException(CoreStrings.DuplicateProperty(
                    name, this.DisplayName(), duplicateProperty.DeclaringEntityType.DisplayName()));
            }

            var duplicateNavigation = FindNavigationsInHierarchy(name).FirstOrDefault();
            if (duplicateNavigation != null)
            {
                throw new InvalidOperationException(CoreStrings.ConflictingNavigation(name, this.DisplayName(),
                    duplicateNavigation.DeclaringEntityType.DisplayName()));
            }
        }

        private Property AddProperty(Property property, bool runConventions)
        {
            _properties.Add(property.Name, property);

            PropertyMetadataChanged();

            if (runConventions)
            {
                property = Model.ConventionDispatcher.OnPropertyAdded(property.Builder)?.Metadata;
            }

            return property;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Property GetOrAddProperty([NotNull] PropertyInfo propertyInfo)
            => FindProperty(propertyInfo) ?? AddProperty(propertyInfo);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Property GetOrAddProperty([NotNull] string name, [NotNull] Type propertyType, bool shadow)
            => FindProperty(name) ?? AddProperty(name, propertyType, shadow);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Property FindProperty([NotNull] PropertyInfo propertyInfo)
            => FindProperty(propertyInfo.Name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Property FindProperty([NotNull] string name)
            => FindDeclaredProperty(Check.NotEmpty(name, nameof(name))) ?? _baseType?.FindProperty(name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Property FindDeclaredProperty([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, nameof(propertyName));

            Property property;
            return _properties.TryGetValue(propertyName, out property)
                ? property
                : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Property> GetDeclaredProperties() => _properties.Values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Property> FindDerivedProperties([NotNull] string propertyName)
        {
            Check.NotNull(propertyName, nameof(propertyName));

            return GetDerivedTypes().Select(et => et.FindDeclaredProperty(propertyName)).Where(p => p != null);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Property> FindPropertiesInHierarchy([NotNull] string propertyName)
            => ToEnumerable(FindProperty(propertyName)).Concat(FindDerivedProperties(propertyName));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Property RemoveProperty([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            var property = FindDeclaredProperty(name);
            return property == null
                ? null
                : RemoveProperty(property);
        }

        private Property RemoveProperty(Property property)
        {
            CheckPropertyNotInUse(property);

            _properties.Remove(property.Name);
            property.Builder = null;

            PropertyMetadataChanged();

            return property;
        }

        private void CheckPropertyNotInUse(Property property)
        {
            CheckPropertyNotInUse(property, this);

            foreach (var entityType in GetDerivedTypes())
            {
                CheckPropertyNotInUse(property, entityType);
            }
        }

        private void CheckPropertyNotInUse(Property property, EntityType entityType)
        {
            if (entityType.GetDeclaredKeys().Any(k => k.Properties.Contains(property))
                || entityType.GetDeclaredForeignKeys().Any(k => k.Properties.Contains(property))
                || entityType.GetDeclaredIndexes().Any(i => i.Properties.Contains(property)))
            {
                throw new InvalidOperationException(CoreStrings.PropertyInUse(property.Name, this.DisplayName()));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Property> GetProperties()
            => _baseType?.GetProperties().Concat(_properties.Values) ?? _properties.Values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void PropertyMetadataChanged()
        {
            foreach (var indexedProperty in this.GetPropertiesAndNavigations().OfType<PropertyBase>())
            {
                indexedProperty.PropertyIndexes = null;
            }

            // This path should only kick in when the model is still mutable and therefore access does not need
            // to be thread-safe.
            _counts = null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual PropertyCounts Counts
            => NonCapturingLazyInitializer.EnsureInitialized(ref _counts, this, entityType => entityType.CalculateCounts());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<InternalEntityEntry, ISnapshot> RelationshipSnapshotFactory
            => NonCapturingLazyInitializer.EnsureInitialized(ref _relationshipSnapshotFactory, this,
                entityType => new RelationshipSnapshotFactoryFactory().Create(entityType));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<InternalEntityEntry, ISnapshot> OriginalValuesFactory
            => NonCapturingLazyInitializer.EnsureInitialized(ref _originalValuesFactory, this,
                entityType => new OriginalValuesFactoryFactory().Create(entityType));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<ValueBuffer, ISnapshot> ShadowValuesFactory
            => NonCapturingLazyInitializer.EnsureInitialized(ref _shadowValuesFactory, this,
                entityType => new ShadowValuesFactoryFactory().Create(entityType));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<ISnapshot> EmptyShadowValuesFactory
            => NonCapturingLazyInitializer.EnsureInitialized(ref _emptyShadowValuesFactory, this,
                entityType => new EmptyShadowValuesFactoryFactory().CreateEmpty(entityType));

        #endregion

        #region Ignore

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Ignore([NotNull] string name, ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            Check.NotNull(name, nameof(name));

            ConfigurationSource existingIgnoredConfigurationSource;
            if (_ignoredMembers.TryGetValue(name, out existingIgnoredConfigurationSource))
            {
                configurationSource = configurationSource.Max(existingIgnoredConfigurationSource);
            }

            _ignoredMembers[name] = configurationSource;

            if (runConventions)
            {
                Model.ConventionDispatcher.OnEntityTypeMemberIgnored(Builder, name);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<string> GetIgnoredMembers()
            => _ignoredMembers.Keys.ToList();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? FindDeclaredIgnoredMemberConfigurationSource([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            ConfigurationSource ignoredConfigurationSource;
            if (_ignoredMembers.TryGetValue(name, out ignoredConfigurationSource))
            {
                return ignoredConfigurationSource;
            }

            return null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? FindIgnoredMemberConfigurationSource([NotNull] string name)
        {
            var ignoredSource = FindDeclaredIgnoredMemberConfigurationSource(name);

            return BaseType == null ? ignoredSource : BaseType.FindIgnoredMemberConfigurationSource(name).Max(ignoredSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Unignore([NotNull] string name)
        {
            Check.NotNull(name, nameof(name));
            _ignoredMembers.Remove(name);
        }

        #endregion

        #region Explicit interface implementations

        IModel IEntityType.Model => Model;
        IMutableModel IMutableEntityType.Model => Model;
        Type IEntityType.ClrType => ClrType;
        IEntityType IEntityType.BaseType => _baseType;

        IMutableEntityType IMutableEntityType.BaseType
        {
            get { return _baseType; }
            set { HasBaseType((EntityType)value); }
        }

        IMutableKey IMutableEntityType.SetPrimaryKey(IReadOnlyList<IMutableProperty> properties)
            => SetPrimaryKey(properties?.Cast<Property>().ToList());

        IKey IEntityType.FindPrimaryKey() => FindPrimaryKey();
        IMutableKey IMutableEntityType.FindPrimaryKey() => FindPrimaryKey();

        IMutableKey IMutableEntityType.AddKey(IReadOnlyList<IMutableProperty> properties)
            => AddKey(properties.Cast<Property>().ToList());

        IKey IEntityType.FindKey(IReadOnlyList<IProperty> properties) => FindKey(properties);
        IMutableKey IMutableEntityType.FindKey(IReadOnlyList<IProperty> properties) => FindKey(properties);
        IEnumerable<IKey> IEntityType.GetKeys() => GetKeys();
        IEnumerable<IMutableKey> IMutableEntityType.GetKeys() => GetKeys();
        IMutableKey IMutableEntityType.RemoveKey(IReadOnlyList<IProperty> properties) => RemoveKey(properties);

        IMutableForeignKey IMutableEntityType.AddForeignKey(
            IReadOnlyList<IMutableProperty> properties, IMutableKey principalKey, IMutableEntityType principalEntityType)
            => AddForeignKey(properties.Cast<Property>().ToList(), (Key)principalKey, (EntityType)principalEntityType);

        IMutableForeignKey IMutableEntityType.FindForeignKey(
            IReadOnlyList<IProperty> properties, IKey principalKey, IEntityType principalEntityType)
            => FindForeignKey(properties, principalKey, principalEntityType);

        IForeignKey IEntityType.FindForeignKey(IReadOnlyList<IProperty> properties, IKey principalKey, IEntityType principalEntityType)
            => FindForeignKey(properties, principalKey, principalEntityType);

        IEnumerable<IForeignKey> IEntityType.GetForeignKeys() => GetForeignKeys();
        IEnumerable<IMutableForeignKey> IMutableEntityType.GetForeignKeys() => GetForeignKeys();

        IMutableForeignKey IMutableEntityType.RemoveForeignKey(
            IReadOnlyList<IProperty> properties, IKey principalKey, IEntityType principalEntityType)
            => RemoveForeignKey(properties, principalKey, principalEntityType);

        IMutableIndex IMutableEntityType.AddIndex(IReadOnlyList<IMutableProperty> properties)
            => AddIndex(properties.Cast<Property>().ToList());

        IIndex IEntityType.FindIndex(IReadOnlyList<IProperty> properties) => FindIndex(properties);
        IMutableIndex IMutableEntityType.FindIndex(IReadOnlyList<IProperty> properties) => FindIndex(properties);
        IEnumerable<IIndex> IEntityType.GetIndexes() => GetIndexes();
        IEnumerable<IMutableIndex> IMutableEntityType.GetIndexes() => GetIndexes();

        IMutableIndex IMutableEntityType.RemoveIndex(IReadOnlyList<IProperty> properties)
            => RemoveIndex(properties);

        IMutableProperty IMutableEntityType.AddProperty(string name, Type propertyType, bool shadow) => AddProperty(name, propertyType, shadow);
        IProperty IEntityType.FindProperty(string name) => FindProperty(name);
        IMutableProperty IMutableEntityType.FindProperty(string name) => FindProperty(name);
        IEnumerable<IProperty> IEntityType.GetProperties() => GetProperties();
        IEnumerable<IMutableProperty> IMutableEntityType.GetProperties() => GetProperties();
        IMutableProperty IMutableEntityType.RemoveProperty(string name) => RemoveProperty(name);

        #endregion

        private static IEnumerable<T> ToEnumerable<T>(T element)
            where T : class
            => element == null ? Enumerable.Empty<T>() : new[] { element };

        private class PropertyComparer : IComparer<string>
        {
            private readonly EntityType _entityType;

            public PropertyComparer(EntityType entityType)
            {
                _entityType = entityType;
            }

            public int Compare(string x, string y)
            {
                var properties = _entityType.FindPrimaryKey()?.Properties.Select(p => p.Name).ToList();

                var xIndex = -1;
                var yIndex = -1;

                if (properties != null)
                {
                    xIndex = properties.IndexOf(x);
                    yIndex = properties.IndexOf(y);
                }

                // Neither property is part of the Primary Key
                // Compare the property names
                if ((xIndex == -1)
                    && (yIndex == -1))
                {
                    return StringComparer.Ordinal.Compare(x, y);
                }

                // Both properties are part of the Primary Key
                // Compare the indices
                if ((xIndex > -1)
                    && (yIndex > -1))
                {
                    return xIndex - yIndex;
                }

                // One property is part of the Primary Key
                // The primary key property is first
                return xIndex > yIndex
                    ? -1
                    : 1;
            }
        }
    }
}
