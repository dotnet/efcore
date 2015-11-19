// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class EntityType :
        ConventionalAnnotatable,
        IMutableEntityType,
        ICanGetNavigations,
        IPropertyCountsAccessor,
        ISnapshotFactorySource
    {
        private static readonly char[] _simpleNameChars = { '.', '+' };

        private readonly SortedDictionary<IReadOnlyList<IProperty>, ForeignKey> _foreignKeys
            = new SortedDictionary<IReadOnlyList<IProperty>, ForeignKey>(PropertyListComparer.Instance);

        private readonly SortedDictionary<string, Navigation> _navigations
            = new SortedDictionary<string, Navigation>(StringComparer.Ordinal);

        private readonly SortedDictionary<IReadOnlyList<IProperty>, Index> _indexes
            = new SortedDictionary<IReadOnlyList<IProperty>, Index>(PropertyListComparer.Instance);

        private readonly SortedDictionary<string, Property> _properties;

        private readonly SortedDictionary<IReadOnlyList<IProperty>, Key> _keys
            = new SortedDictionary<IReadOnlyList<IProperty>, Key>(PropertyListComparer.Instance);

        private object _typeOrName;
        private Key _primaryKey;
        private EntityType _baseType;

        private bool _useEagerSnapshots;

        private ConfigurationSource _configurationSource;
        private ConfigurationSource? _baseTypeConfigurationSource;
        private ConfigurationSource? _primaryKeyConfigurationSource;
        private readonly Dictionary<string, ConfigurationSource> _ignoredMembers = new Dictionary<string, ConfigurationSource>();

        // Warning: Never access this field directly as access needs to be thread-safe
        private PropertyCounts _counts;

        // Warning: Never access this field directly as access needs to be thread-safe
        private Func<InternalEntityEntry, ISnapshot> _relationshipSnapshotFactory;

        // Warning: Never access this field directly as access needs to be thread-safe
        private Func<InternalEntityEntry, ISnapshot> _originalValuesFactory;

        /// <summary>
        ///     Creates a new metadata object representing an entity type that will participate in shadow-state
        ///     such that there is no underlying .NET type corresponding to this metadata object.
        /// </summary>
        /// <param name="name">The name of the shadow-state entity type.</param>
        /// <param name="model">The model associated with this entity type.</param>
        /// <param name="configurationSource">The configuration source that added this entity type.</param>
        public EntityType([NotNull] string name, [NotNull] Model model, ConfigurationSource configurationSource)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(model, nameof(model));

            _typeOrName = name;
            Model = model;
            _configurationSource = configurationSource;
            Builder = new InternalEntityTypeBuilder(this, model.Builder);

            _properties = new SortedDictionary<string, Property>(new PropertyComparer(this));
#if DEBUG
            DebugName = EntityTypeExtensions.DisplayName(this);
#endif
        }

#if DEBUG
        private string DebugName { get; set; }
#endif

        public virtual InternalEntityTypeBuilder Builder { get; [param: CanBeNull] set; }

        /// <summary>
        ///     Gets or sets the associated .NET type.
        /// </summary>
        public virtual Type ClrType
        {
            get { return _typeOrName as Type; }
            set
            {
                if (value == null)
                {
                    _typeOrName = Name;
                    _useEagerSnapshots = false;
                }
                else
                {
                    Check.ValidEntityType(value, nameof(value));

                    if (Name != value.DisplayName())
                    {
                        // Don't use DisplayName for the second argument as it could be ambiguous
                        throw new InvalidOperationException(CoreStrings.ClrTypeWrongName(value.DisplayName(), Name));
                    }

                    if ((_baseType != null)
                        || GetDirectlyDerivedTypes().Any()
                        || GetProperties().Any())
                    {
                        throw new InvalidOperationException(CoreStrings.EntityTypeInUse(EntityTypeExtensions.DisplayName(this)));
                    }

                    _typeOrName = value;
                    _useEagerSnapshots = !this.HasPropertyChangingNotifications();
                }
            }
        }

        public virtual Model Model { get; }

        public virtual EntityType BaseType => _baseType;

        public virtual void HasBaseType([CanBeNull] EntityType entityType, ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            if (_baseType == entityType)
            {
                UpdateBaseTypeConfigurationSource(configurationSource);
                entityType?.UpdateConfigurationSource(configurationSource);
                return;
            }

            _baseType?._directlyDerivedTypes.Remove(this);
            _baseType = null;
            if (entityType != null)
            {
                if (this.HasClrType())
                {
                    if (!entityType.HasClrType())
                    {
                        throw new InvalidOperationException(CoreStrings.NonClrBaseType(this, entityType));
                    }

                    if (!entityType.ClrType.GetTypeInfo().IsAssignableFrom(ClrType.GetTypeInfo()))
                    {
                        throw new InvalidOperationException(CoreStrings.NotAssignableClrBaseType(this, entityType, ClrType.Name, entityType.ClrType.Name));
                    }
                }

                if (!this.HasClrType()
                    && entityType.HasClrType())
                {
                    throw new InvalidOperationException(CoreStrings.NonShadowBaseType(this, entityType));
                }

                if (entityType.InheritsFrom(this))
                {
                    throw new InvalidOperationException(CoreStrings.CircularInheritance(this, entityType));
                }

                if (_keys.Any())
                {
                    throw new InvalidOperationException(CoreStrings.DerivedEntityCannotHaveKeys(Name));
                }

                var propertyCollisions = entityType.GetProperties().Select(p => p.Name)
                    .SelectMany(FindPropertiesInHierarchy);
                if (propertyCollisions.Any())
                {
                    throw new InvalidOperationException(
                        CoreStrings.DuplicatePropertiesOnBase(
                            Name,
                            entityType.Name,
                            string.Join(", ", propertyCollisions.Select(p => p.Name))));
                }

                var navigationCollisions = entityType.GetNavigations().Select(p => p.Name)
                    .SelectMany(FindNavigationsInHierarchy);
                if (navigationCollisions.Any())
                {
                    throw new InvalidOperationException(
                        CoreStrings.DuplicateNavigationsOnBase(
                            Name,
                            entityType.Name,
                            string.Join(", ", navigationCollisions.Select(p => p.Name))));
                }

                _baseType = entityType;
                _baseType._directlyDerivedTypes.Add(this);
            }

            PropertyMetadataChanged();
            UpdateBaseTypeConfigurationSource(configurationSource);
            entityType?.UpdateConfigurationSource(configurationSource);
        }

        public virtual ConfigurationSource? GetBaseTypeConfigurationSource() => _baseTypeConfigurationSource;

        private void UpdateBaseTypeConfigurationSource(ConfigurationSource configurationSource)
            => _baseTypeConfigurationSource = configurationSource.Max(_baseTypeConfigurationSource);

        private readonly List<EntityType> _directlyDerivedTypes = new List<EntityType>();
        public virtual IReadOnlyList<EntityType> GetDirectlyDerivedTypes() => _directlyDerivedTypes;

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

        public virtual EntityType RootType() => (EntityType)((IEntityType)this).RootType();

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

        public override string ToString() => Name;

        public virtual ConfigurationSource GetConfigurationSource() => _configurationSource;

        public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
            => _configurationSource = _configurationSource.Max(configurationSource);

        public virtual bool UseEagerSnapshots
        {
            get { return _useEagerSnapshots; }
            set
            {
                if (!value
                    && !this.HasPropertyChangingNotifications())
                {
                    throw new InvalidOperationException(CoreStrings.EagerOriginalValuesRequired(Name));
                }

                _useEagerSnapshots = value;

                PropertyMetadataChanged();
            }
        }

        #region Primary and Candidate Keys

        public virtual Key SetPrimaryKey([CanBeNull] Property property)
            => SetPrimaryKey(property == null ? null : new[] { property });

        public virtual Key SetPrimaryKey(
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            if (_baseType != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.DerivedEntityTypeKey(EntityTypeExtensions.DisplayName(this), _baseType.DisplayName()));
            }

            if (_primaryKey != null)
            {
                foreach (var property in _primaryKey.Properties)
                {
                    _properties.Remove(property.Name);
                }

                var oldPrimaryKey = _primaryKey;
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
                }

                _primaryKey = key;

                foreach (var property in key.Properties)
                {
                    property.IsNullable = false;
                    _properties.Add(property.Name, property);
                }
            }

            PropertyMetadataChanged();
            UpdatePrimaryKeyConfigurationSource(configurationSource);

            return _primaryKey;
        }

        public virtual Key GetOrSetPrimaryKey([NotNull] Property property)
            => GetOrSetPrimaryKey(new[] { property });

        public virtual Key GetOrSetPrimaryKey([NotNull] IReadOnlyList<Property> properties)
            => FindPrimaryKey(properties) ?? SetPrimaryKey(properties);

        public virtual Key FindPrimaryKey()
            => _baseType?.FindPrimaryKey() ?? FindDeclaredPrimaryKey();

        public virtual Key FindDeclaredPrimaryKey() => _primaryKey;

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

        public virtual ConfigurationSource? GetPrimaryKeyConfigurationSource() => _primaryKeyConfigurationSource;

        private void UpdatePrimaryKeyConfigurationSource(ConfigurationSource configurationSource)
            => _primaryKeyConfigurationSource = configurationSource.Max(_primaryKeyConfigurationSource);

        public virtual Key AddKey([NotNull] Property property, ConfigurationSource configurationSource = ConfigurationSource.Explicit)
            => AddKey(new[] { property }, configurationSource);

        public virtual Key AddKey([NotNull] IReadOnlyList<Property> properties,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));

            if (_baseType != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.DerivedEntityTypeKey(EntityTypeExtensions.DisplayName(this), _baseType.DisplayName()));
            }

            foreach (var property in properties)
            {
                if (FindProperty(property.Name) != property)
                {
                    throw new ArgumentException(
                        CoreStrings.KeyPropertiesWrongEntity(
                            Property.Format(properties),
                            EntityTypeExtensions.DisplayName(this)));
                }
            }

            var key = FindKey(properties);
            if (key != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.DuplicateKey(
                        Property.Format(properties),
                        EntityTypeExtensions.DisplayName(this),
                        key.DeclaringEntityType.DisplayName()));
            }

            key = new Key(properties, configurationSource);
            _keys.Add(properties, key);

            PropertyMetadataChanged();

            return key;
        }

        public virtual Key GetOrAddKey([NotNull] Property property)
            => GetOrAddKey(new[] { property });

        public virtual Key GetOrAddKey([NotNull] IReadOnlyList<Property> properties)
            => FindKey(properties)
               ?? AddKey(properties);

        public virtual Key FindKey([NotNull] IProperty property) => FindKey(new[] { property });

        public virtual Key FindKey([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotEmpty(properties, nameof(properties));

            return FindDeclaredKey(properties) ?? _baseType?.FindKey(properties);
        }

        public virtual IEnumerable<Key> GetDeclaredKeys() => _keys.Values;

        public virtual Key FindDeclaredKey([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.NotEmpty(properties, nameof(properties));

            Key key;
            return _keys.TryGetValue(properties, out key)
                ? key
                : null;
        }

        public virtual Key RemoveKey([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.NotEmpty(properties, nameof(properties));

            var key = FindDeclaredKey(properties);
            return key == null
                ? null
                : RemoveKey(key);
        }

        private Key RemoveKey([NotNull] Key key)
        {
            CheckKeyNotInUse(key);

            if (_primaryKey == key)
            {
                SetPrimaryKey((IReadOnlyList<Property>)null);
                _primaryKeyConfigurationSource = null;
            }

            _keys.Remove(key.Properties);
            key.Builder = null;

            PropertyMetadataChanged();

            return key;
        }

        private void CheckKeyNotInUse(Key key)
        {
            var foreignKey = key.FindReferencingForeignKeys().FirstOrDefault();
            if (foreignKey != null)
            {
                throw new InvalidOperationException(CoreStrings.KeyInUse(Property.Format(key.Properties), Name, foreignKey.DeclaringEntityType.Name));
            }
        }

        public virtual IEnumerable<Key> GetKeys() => _baseType?.GetKeys().Concat(_keys.Values) ?? _keys.Values;

        #endregion

        #region Foreign Keys

        public virtual ForeignKey AddForeignKey(
            [NotNull] Property property,
            [NotNull] Key principalKey,
            [NotNull] EntityType principalEntityType,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
            => AddForeignKey(new[] { property }, principalKey, principalEntityType, configurationSource);

        public virtual ForeignKey AddForeignKey(
            [NotNull] IReadOnlyList<Property> properties,
            [NotNull] Key principalKey,
            [NotNull] EntityType principalEntityType,
            ConfigurationSource? configurationSource = ConfigurationSource.Explicit)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotNull(principalKey, nameof(principalKey));
            Check.NotNull(principalEntityType, nameof(principalEntityType));

            foreach (var property in properties)
            {
                var actualProperty = FindProperty(property.Name);
                if ((actualProperty == null)
                    || (actualProperty.DeclaringEntityType != property.DeclaringEntityType))
                {
                    throw new ArgumentException(CoreStrings.ForeignKeyPropertiesWrongEntity(Property.Format(properties), Name));
                }
            }

            var duplicateForeignKey = FindForeignKeysInHierarchy(properties, principalKey, principalEntityType).FirstOrDefault();
            if (duplicateForeignKey != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.DuplicateForeignKey(
                        Property.Format(properties),
                        EntityTypeExtensions.DisplayName(this),
                        duplicateForeignKey.DeclaringEntityType.DisplayName()));
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
                throw new ArgumentException(CoreStrings.EntityTypeModelMismatch(this, principalEntityType));
            }

            _foreignKeys.Add(properties, foreignKey);

            PropertyMetadataChanged();

            return foreignKey;
        }

        public virtual ForeignKey GetOrAddForeignKey(
            [NotNull] Property property, [NotNull] Key principalKey, [NotNull] EntityType principalEntityType)
            => GetOrAddForeignKey(new[] { property }, principalKey, principalEntityType);

        // Note: this will return an existing foreign key even if it doesn't have the same referenced key
        public virtual ForeignKey GetOrAddForeignKey(
            [NotNull] IReadOnlyList<Property> properties, [NotNull] Key principalKey, [NotNull] EntityType principalEntityType)
            => FindForeignKey(properties, principalKey, principalEntityType)
               ?? AddForeignKey(properties, principalKey, principalEntityType);

        public virtual IEnumerable<ForeignKey> FindForeignKeys([NotNull] IProperty property)
            => FindForeignKeys(new[] { property });

        public virtual IEnumerable<ForeignKey> FindForeignKeys([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotEmpty(properties, nameof(properties));

            var declaredForeignKeys = FindDeclaredForeignKeys(properties);
            return _baseType == null
                ? declaredForeignKeys
                : declaredForeignKeys.Concat(_baseType.FindForeignKeys(properties));
        }

        public virtual ForeignKey FindForeignKey(
            [NotNull] IProperty property, [NotNull] IKey principalKey, [NotNull] IEntityType principalEntityType)
            => FindForeignKey(new[] { property }, principalKey, principalEntityType);

        public virtual ForeignKey FindForeignKey(
            [NotNull] IReadOnlyList<IProperty> properties, [NotNull] IKey principalKey, [NotNull] IEntityType principalEntityType)
        {
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotEmpty(properties, nameof(properties));

            return FindForeignKeys(properties).SingleOrDefault();
        }

        public virtual IEnumerable<ForeignKey> GetDeclaredForeignKeys() => _foreignKeys.Values;

        public virtual IEnumerable<ForeignKey> GetDerivedForeignKeys()
            => GetDerivedTypes().SelectMany(et => et.GetDeclaredForeignKeys());

        public virtual IEnumerable<ForeignKey> FindDeclaredForeignKeys([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.NotEmpty(properties, nameof(properties));

            ForeignKey foreignKey;
            return _foreignKeys.TryGetValue(properties, out foreignKey)
                ? new[] { foreignKey }
                : new ForeignKey[0];
        }

        public virtual ForeignKey FindDeclaredForeignKey(
            [NotNull] IReadOnlyList<IProperty> properties, [NotNull] IKey principalKey, [NotNull] IEntityType principalEntityType)
        {
            Check.NotEmpty(properties, nameof(properties));

            return FindDeclaredForeignKeys(properties).SingleOrDefault();
        }

        public virtual IEnumerable<ForeignKey> FindDerivedForeignKeys(
            [NotNull] IReadOnlyList<IProperty> properties)
            => GetDerivedTypes().SelectMany(et => et.FindDeclaredForeignKeys(properties));

        public virtual IEnumerable<ForeignKey> FindDerivedForeignKeys(
            [NotNull] IReadOnlyList<IProperty> properties, [NotNull] IKey principalKey, [NotNull] IEntityType principalEntityType)
            => GetDerivedTypes().Select(et => et.FindDeclaredForeignKey(properties, principalKey, principalEntityType))
                .Where(fk => fk != null);

        public virtual IEnumerable<ForeignKey> FindForeignKeysInHierarchy(
            [NotNull] IReadOnlyList<IProperty> properties)
            => FindForeignKeys(properties).Concat(FindDerivedForeignKeys(properties));

        public virtual IEnumerable<ForeignKey> FindForeignKeysInHierarchy(
            [NotNull] IReadOnlyList<IProperty> properties, [NotNull] IKey principalKey, [NotNull] IEntityType principalEntityType)
            => ToEnumerable(FindForeignKey(properties, principalKey, principalEntityType))
                .Concat(FindDerivedForeignKeys(properties, principalKey, principalEntityType));

        public virtual ForeignKey RemoveForeignKey(
            [NotNull] IReadOnlyList<IProperty> properties, [NotNull] IKey principalKey, [NotNull] IEntityType principalEntityType)
        {
            Check.NotEmpty(properties, nameof(properties));

            var foreignKey = FindDeclaredForeignKey(properties, principalKey, principalEntityType);
            return foreignKey == null
                ? null
                : RemoveForeignKey(foreignKey);
        }

        private ForeignKey RemoveForeignKey([NotNull] ForeignKey foreignKey)
        {
            if (foreignKey.DependentToPrincipal != null)
            {
                foreignKey.DeclaringEntityType.RemoveNavigation(foreignKey.DependentToPrincipal.Name);
            }

            if (foreignKey.PrincipalToDependent != null)
            {
                foreignKey.PrincipalEntityType.RemoveNavigation(foreignKey.PrincipalToDependent.Name);
            }

            var removed = _foreignKeys.Remove(foreignKey.Properties);
            foreignKey.Builder = null;

            PropertyMetadataChanged();

            return removed ? foreignKey : null;
        }

        public virtual IEnumerable<ForeignKey> GetReferencingForeignKeys()
            => ((IEntityType)this).GetReferencingForeignKeys().Cast<ForeignKey>();

        public virtual IEnumerable<ForeignKey> GetDeclaredReferencingForeignKeys()
            => ((IEntityType)this).GetDeclaredReferencingForeignKeys().Cast<ForeignKey>();

        public virtual IEnumerable<ForeignKey> GetForeignKeys()
            => _baseType?.GetForeignKeys().Concat(_foreignKeys.Values) ?? _foreignKeys.Values;

        #endregion

        #region Navigations

        public virtual Navigation AddNavigation(
            [NotNull] string name,
            [NotNull] ForeignKey foreignKey,
            bool pointsToPrincipal)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(foreignKey, nameof(foreignKey));

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
                    CoreStrings.DuplicateNavigation(
                        name,
                        EntityTypeExtensions.DisplayName(this),
                        duplicateNavigation.DeclaringEntityType.DisplayName()));
            }

            var duplicateProperty = FindPropertiesInHierarchy(name).FirstOrDefault();
            if (duplicateProperty != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.ConflictingProperty(
                        name,
                        EntityTypeExtensions.DisplayName(this),
                        duplicateProperty.DeclaringEntityType.DisplayName()));
            }

            Debug.Assert(!GetNavigations().Any(n => (n.ForeignKey == foreignKey) && (n.IsDependentToPrincipal() == pointsToPrincipal)),
                "There is another navigation corresponding to the same foreign key and pointing in the same direction.");

            Debug.Assert((pointsToPrincipal ? foreignKey.DeclaringEntityType : foreignKey.PrincipalEntityType) == this,
                "EntityType mismatch");

            Navigation.IsCompatible(
                name,
                this,
                pointsToPrincipal ? foreignKey.PrincipalEntityType : foreignKey.DeclaringEntityType,
                !pointsToPrincipal && !foreignKey.IsUnique,
                shouldThrow: true);

            var navigation = new Navigation(name, foreignKey);
            _navigations.Add(name, navigation);

            PropertyMetadataChanged();

            return navigation;
        }

        public virtual Navigation FindNavigation([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            return FindDeclaredNavigation(name) ?? _baseType?.FindNavigation(name);
        }

        public virtual Navigation FindDeclaredNavigation([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            Navigation navigation;
            return _navigations.TryGetValue(name, out navigation)
                ? navigation
                : null;
        }

        public virtual IEnumerable<Navigation> GetDeclaredNavigations() => _navigations.Values;

        public virtual IEnumerable<Navigation> FindDerivedNavigations([NotNull] string navigationName)
        {
            Check.NotNull(navigationName, nameof(navigationName));

            return GetDerivedTypes().Select(et => et.FindDeclaredNavigation(navigationName)).Where(n => n != null);
        }

        public virtual IEnumerable<Navigation> FindNavigationsInHierarchy([NotNull] string propertyName)
            => ToEnumerable(FindNavigation(propertyName)).Concat(FindDerivedNavigations(propertyName));

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

        public virtual IEnumerable<Navigation> GetNavigations()
            => _baseType?.GetNavigations().Concat(_navigations.Values) ?? _navigations.Values;

        #endregion

        #region Indexes

        public virtual Index AddIndex([NotNull] Property property,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
            => AddIndex(new[] { property }, configurationSource);

        public virtual Index AddIndex([NotNull] IReadOnlyList<Property> properties,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));

            foreach (var property in properties)
            {
                if (FindProperty(property.Name) != property)
                {
                    throw new ArgumentException(
                        CoreStrings.IndexPropertiesWrongEntity(
                            Property.Format(properties),
                            EntityTypeExtensions.DisplayName(this)));
                }
            }

            var duplicateIndex = FindIndexesInHierarchy(properties).FirstOrDefault();
            if (duplicateIndex != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.DuplicateIndex(
                        Property.Format(properties),
                        EntityTypeExtensions.DisplayName(this),
                        duplicateIndex.DeclaringEntityType.DisplayName()));
            }

            var index = new Index(properties, this, configurationSource);
            _indexes.Add(properties, index);

            return index;
        }

        public virtual Index GetOrAddIndex([NotNull] Property property)
            => GetOrAddIndex(new[] { property });

        public virtual Index GetOrAddIndex([NotNull] IReadOnlyList<Property> properties)
            => FindIndex(properties) ?? AddIndex(properties);

        public virtual Index FindIndex([NotNull] IProperty property)
            => FindIndex(new[] { property });

        public virtual Index FindIndex([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotEmpty(properties, nameof(properties));

            return FindDeclaredIndex(properties) ?? _baseType?.FindIndex(properties);
        }

        public virtual IEnumerable<Index> GetDeclaredIndexes() => _indexes.Values;

        public virtual Index FindDeclaredIndex([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.NotEmpty(properties, nameof(properties));

            Index index;
            return _indexes.TryGetValue(properties, out index)
                ? index
                : null;
        }

        public virtual IEnumerable<Index> FindDerivedIndexes([NotNull] IReadOnlyList<IProperty> properties)
            => GetDerivedTypes().Select(et => et.FindDeclaredIndex(properties)).Where(i => i != null);

        public virtual IEnumerable<Index> FindIndexesInHierarchy([NotNull] IReadOnlyList<IProperty> properties)
            => ToEnumerable(FindIndex(properties)).Concat(FindDerivedIndexes(properties));

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
            return index;
        }

        public virtual IEnumerable<Index> GetIndexes() => _baseType?.GetIndexes().Concat(_indexes.Values) ?? _indexes.Values;

        #endregion

        #region Properties

        public virtual Property AddProperty([NotNull] PropertyInfo propertyInfo)
            => (Property)((IMutableEntityType)this).AddProperty(propertyInfo);

        public virtual Property AddProperty([NotNull] string name, [NotNull] Type propertyType)
            => (Property)((IMutableEntityType)this).AddProperty(name, propertyType);

        public virtual Property AddProperty([NotNull] string name,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            Check.NotNull(name, nameof(name));

            var duplicateProperty = FindPropertiesInHierarchy(name).FirstOrDefault();
            if (duplicateProperty != null)
            {
                throw new InvalidOperationException(CoreStrings.DuplicateProperty(
                    name, EntityTypeExtensions.DisplayName(this), duplicateProperty.DeclaringEntityType.DisplayName()));
            }

            var duplicateNavigation = FindNavigationsInHierarchy(name).FirstOrDefault();
            if (duplicateNavigation != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.ConflictingNavigation(
                        name,
                        EntityTypeExtensions.DisplayName(this),
                        duplicateNavigation.DeclaringEntityType.DisplayName()));
            }

            var property = new Property(name, this, configurationSource);

            _properties.Add(name, property);

            PropertyMetadataChanged();

            return property;
        }

        public virtual Property GetOrAddProperty([NotNull] PropertyInfo propertyInfo)
            => (Property)((IMutableEntityType)this).GetOrAddProperty(propertyInfo);

        public virtual Property GetOrAddProperty([NotNull] string name)
            => FindProperty(name) ?? AddProperty(name);

        public virtual Property FindProperty([NotNull] PropertyInfo propertyInfo)
            => (Property)((IMutableEntityType)this).FindProperty(propertyInfo);

        public virtual Property FindProperty([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            return FindDeclaredProperty(name) ?? _baseType?.FindProperty(name);
        }

        public virtual Property FindDeclaredProperty([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, nameof(propertyName));

            Property property;
            return _properties.TryGetValue(propertyName, out property)
                ? property
                : null;
        }

        public virtual IEnumerable<Property> GetDeclaredProperties() => _properties.Values;

        public virtual IEnumerable<Property> FindDerivedProperties([NotNull] string propertyName)
        {
            Check.NotNull(propertyName, nameof(propertyName));

            return GetDerivedTypes().Select(et => et.FindDeclaredProperty(propertyName)).Where(p => p != null);
        }

        public virtual IEnumerable<Property> FindPropertiesInHierarchy([NotNull] string propertyName)
            => ToEnumerable(FindProperty(propertyName)).Concat(FindDerivedProperties(propertyName));

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
                throw new InvalidOperationException(CoreStrings.PropertyInUse(property.Name, Name));
            }
        }

        public virtual IEnumerable<Property> GetProperties()
            => _baseType?.GetProperties().Concat(_properties.Values) ?? _properties.Values;

        public virtual void PropertyMetadataChanged()
        {
            foreach (var indexedProperty in this.GetPropertiesAndNavigations().OfType<IPropertyIndexesAccessor>())
            {
                indexedProperty.Indexes = null;
            }

            // This path should only kick in when the model is still mutable and therefore access does not need
            // to be thread-safe.
            _counts = null;
        }

        public virtual PropertyCounts Counts => LazyInitializer.EnsureInitialized(ref _counts, CalculateCounts);

        private PropertyCounts CalculateCounts() => EntityTypeExtensions.CalculateCounts(this);

        public virtual Func<InternalEntityEntry, ISnapshot> RelationshipSnapshotFactory
            => LazyInitializer.EnsureInitialized(ref _relationshipSnapshotFactory, CreateRelationshipSnapshotFactory);

        private Func<InternalEntityEntry, ISnapshot> CreateRelationshipSnapshotFactory()
            => new RelationshipSnapshotFactoryFactory().Create(this);

        public virtual Func<InternalEntityEntry, ISnapshot> OriginalValuesFactory
            => LazyInitializer.EnsureInitialized(ref _originalValuesFactory, CreateOriginalValuesFactory);

        private Func<InternalEntityEntry, ISnapshot> CreateOriginalValuesFactory()
            => new OriginalValuesFactoryFactory().Create(this);

        #endregion

        #region Ignore

        public virtual void Ignore([NotNull] string name, ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            Check.NotNull(name, nameof(name));

            ConfigurationSource existingIgnoredConfigurationSource;
            if (_ignoredMembers.TryGetValue(name, out existingIgnoredConfigurationSource))
            {
                configurationSource = configurationSource.Max(existingIgnoredConfigurationSource);
            }

            _ignoredMembers[name] = configurationSource;
        }

        public virtual ConfigurationSource? FindIgnoredMemberConfigurationSource([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            ConfigurationSource ignoredConfigurationSource;
            if (_ignoredMembers.TryGetValue(name, out ignoredConfigurationSource))
            {
                return ignoredConfigurationSource;
            }

            return null;
        }

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

        IEnumerable<INavigation> ICanGetNavigations.GetNavigations() => GetNavigations();

        IMutableIndex IMutableEntityType.AddIndex(IReadOnlyList<IMutableProperty> properties)
            => AddIndex(properties.Cast<Property>().ToList());

        IIndex IEntityType.FindIndex(IReadOnlyList<IProperty> properties) => FindIndex(properties);
        IMutableIndex IMutableEntityType.FindIndex(IReadOnlyList<IProperty> properties) => FindIndex(properties);
        IEnumerable<IIndex> IEntityType.GetIndexes() => GetIndexes();
        IEnumerable<IMutableIndex> IMutableEntityType.GetIndexes() => GetIndexes();

        IMutableIndex IMutableEntityType.RemoveIndex(IReadOnlyList<IProperty> properties)
            => RemoveIndex(properties);

        IMutableProperty IMutableEntityType.AddProperty(string name) => AddProperty(name);
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
