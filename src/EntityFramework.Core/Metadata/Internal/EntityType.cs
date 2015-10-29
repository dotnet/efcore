// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class EntityType : ConventionalAnnotatable, IMutableEntityType, ICanGetNavigations
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
        
        /// <summary>
        ///     Creates a new metadata object representing an entity type that will participate in shadow-state
        ///     such that there is no underlying .NET type corresponding to this metadata object.
        /// </summary>
        /// <param name="name">The name of the shadow-state entity type.</param>
        /// <param name="model">The model associated with this entity type.</param>
        public EntityType([NotNull] string name, [NotNull] Model model)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(model, nameof(model));

            _typeOrName = name;
            Model = model;

            _properties = new SortedDictionary<string, Property>(new PropertyComparer(this));
#if DEBUG
            DebugName = DisplayName();
#endif
        }
#if DEBUG
        private string DebugName { get; set; }
#endif

        Type IEntityType.ClrType => ClrType;
        
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

                    if (BaseType != null
                        || GetDirectlyDerivedTypes().Any()
                        || GetProperties().Any())
                    {
                        throw new InvalidOperationException(CoreStrings.EntityTypeInUse(DisplayName()));
                    }

                    _typeOrName = value;
                    _useEagerSnapshots = !this.HasPropertyChangingNotifications();
                }
            }
        }

        public virtual Model Model { get; }

        public virtual EntityType BaseType
        {
            get { return _baseType; }
            [param: CanBeNull]
            set
            {
                if (_baseType == value)
                {
                    return;
                }

                _baseType?._directlyDerivedTypes.Remove(this);
                _baseType = null;
                if (value != null)
                {
                    if (this.HasClrType())
                    {
                        if (!value.HasClrType())
                        {
                            throw new InvalidOperationException(CoreStrings.NonClrBaseType(this, value));
                        }

                        if (!value.ClrType.GetTypeInfo().IsAssignableFrom(ClrType.GetTypeInfo()))
                        {
                            throw new InvalidOperationException(CoreStrings.NotAssignableClrBaseType(this, value, ClrType.Name, value.ClrType.Name));
                        }
                    }

                    if (!this.HasClrType()
                        && value.HasClrType())
                    {
                        throw new InvalidOperationException(CoreStrings.NonShadowBaseType(this, value));
                    }

                    if (value.InheritsFrom(this))
                    {
                        throw new InvalidOperationException(CoreStrings.CircularInheritance(this, value));
                    }

                    if (_keys.Any())
                    {
                        throw new InvalidOperationException(CoreStrings.DerivedEntityCannotHaveKeys(Name));
                    }

                    var propertyCollisions = value.GetProperties().Select(p => p.Name)
                        .SelectMany(FindPropertiesInHierarchy);
                    if (propertyCollisions.Any())
                    {
                        throw new InvalidOperationException(
                            CoreStrings.DuplicatePropertiesOnBase(
                                Name,
                                value.Name,
                                string.Join(", ", propertyCollisions.Select(p => p.Name))));
                    }

                    var navigationCollisions = value.GetNavigations().Select(p => p.Name)
                        .SelectMany(FindNavigationsInHierarchy);
                    if (navigationCollisions.Any())
                    {
                        throw new InvalidOperationException(
                            CoreStrings.DuplicateNavigationsOnBase(
                                Name,
                                value.Name,
                                string.Join(", ", navigationCollisions.Select(p => p.Name))));
                    }

                    _baseType = value;
                    _baseType._directlyDerivedTypes.Add(this);
                }

                PropertyMetadataChanged(null);
            }
        }

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
            while ((et = et.BaseType) != null);

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

        public virtual string DisplayName()
        {
            if (ClrType != null)
            {
                return ClrType.DisplayName(false) ?? ParseSimpleName();
            }
            return ParseSimpleName();
        }

        private string ParseSimpleName()
        {
            var fullName = (string)_typeOrName;
            var lastDot = fullName.LastIndexOfAny(_simpleNameChars);

            return lastDot > 0 ? fullName.Substring(lastDot + 1) : fullName;
        }

        public override string ToString() => Name;

        public virtual int PropertyCount => (BaseType?.PropertyCount ?? 0) + _properties.Count;

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

                PropertyMetadataChanged(null);
            }
        }

        #region Primary and Candidate Keys

        public virtual Key SetPrimaryKey([CanBeNull] Property property)
            => SetPrimaryKey(property == null ? null : new[] { property });

        public virtual Key SetPrimaryKey([CanBeNull] IReadOnlyList<Property> properties)
        {
            if (BaseType != null)
            {
                throw new InvalidOperationException(CoreStrings.DerivedEntityTypeKey(Name));
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

            if (properties != null
                && properties.Count != 0)
            {
                var key = GetOrAddKey(properties);

                foreach (var property in key.Properties)
                {
                    _properties.Remove(property.Name);
                }

                _primaryKey = key;

                foreach (var property in key.Properties)
                {
                    property.IsNullable = null;
                    _properties.Add(property.Name, property);
                }
            }

            PropertyMetadataChanged(null);

            return _primaryKey;
        }

        public virtual Key GetOrSetPrimaryKey([NotNull] Property property)
            => GetOrSetPrimaryKey(new[] { property });

        public virtual Key GetOrSetPrimaryKey([NotNull] IReadOnlyList<Property> properties)
            => FindPrimaryKey(properties) ?? SetPrimaryKey(properties);

        public virtual Key FindPrimaryKey()
            => BaseType?.FindPrimaryKey() ?? FindDeclaredPrimaryKey();

        public virtual Key FindDeclaredPrimaryKey() => _primaryKey;

        public virtual Key FindPrimaryKey([CanBeNull] IReadOnlyList<Property> properties)
        {
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotEmpty(properties, nameof(properties));

            if (BaseType != null)
            {
                return BaseType.FindPrimaryKey(properties);
            }

            if (_primaryKey != null
                && PropertyListComparer.Instance.Compare(_primaryKey.Properties, properties) == 0)
            {
                return _primaryKey;
            }

            return null;
        }

        public virtual Key AddKey([NotNull] Property property)
            => AddKey(new[] { property });

        public virtual Key AddKey([NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));

            if (BaseType != null)
            {
                throw new InvalidOperationException(CoreStrings.DerivedEntityTypeKey(Name));
            }

            foreach (var property in properties)
            {
                if (FindProperty(property.Name) != property)
                {
                    throw new ArgumentException(CoreStrings.KeyPropertiesWrongEntity(Property.Format(properties), Name));
                }
            }

            var key = FindKey(properties);
            if (key != null)
            {
                throw new InvalidOperationException(CoreStrings.DuplicateKey(Property.Format(properties), DisplayName(), key.DeclaringEntityType.DisplayName()));
            }

            key = new Key(properties);
            _keys.Add(properties, key);

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

            return FindDeclaredKey(properties) ?? BaseType?.FindKey(properties);
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
            }
            _keys.Remove(key.Properties);
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

        public virtual IEnumerable<Key> GetKeys() => BaseType?.GetKeys().Concat(_keys.Values) ?? _keys.Values;

        #endregion

        #region Foreign Keys

        public virtual ForeignKey AddForeignKey(
            [NotNull] Property property,
            [NotNull] Key principalKey,
            [NotNull] EntityType principalEntityType)
            => AddForeignKey(new[] { property }, principalKey, principalEntityType);

        public virtual ForeignKey AddForeignKey(
            [NotNull] IReadOnlyList<Property> properties,
            [NotNull] Key principalKey,
            [NotNull] EntityType principalEntityType)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotNull(principalKey, nameof(principalKey));
            Check.NotNull(principalEntityType, nameof(principalEntityType));

            foreach (var property in properties)
            {
                var actualProperty = FindProperty(property.Name);
                if (actualProperty == null
                    || actualProperty.DeclaringEntityType != property.DeclaringEntityType)
                {
                    throw new ArgumentException(CoreStrings.ForeignKeyPropertiesWrongEntity(Property.Format(properties), Name));
                }
            }

            var duplicateForeignKey = FindForeignKeysInHierarchy(properties, principalKey, principalEntityType).FirstOrDefault();
            if (duplicateForeignKey != null)
            {
                throw new InvalidOperationException(CoreStrings.DuplicateForeignKey(Property.Format(properties), DisplayName(), duplicateForeignKey.DeclaringEntityType.DisplayName()));
            }

            var foreignKey = new ForeignKey(properties, principalKey, this, principalEntityType);

            if (principalEntityType.Model != Model)
            {
                throw new ArgumentException(CoreStrings.EntityTypeModelMismatch(this, principalEntityType));
            }

            _foreignKeys.Add(properties, foreignKey);

            PropertyMetadataChanged(null);

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
            return BaseType == null
                ? declaredForeignKeys
                : declaredForeignKeys.Concat(BaseType.FindForeignKeys(properties));
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

        public virtual ForeignKey RemoveForeignKey([NotNull] ForeignKey foreignKey)
        {
            foreignKey.HasDependentToPrincipal(null);
            foreignKey.HasPrincipalToDependent(null);

            return _foreignKeys.Remove(foreignKey.Properties) ? foreignKey : null;
        }

        public virtual IEnumerable<ForeignKey> GetReferencingForeignKeys()
            => ((IEntityType)this).GetReferencingForeignKeys().Cast<ForeignKey>();

        public virtual IEnumerable<ForeignKey> GetDeclaredReferencingForeignKeys()
            => ((IEntityType)this).GetDeclaredReferencingForeignKeys().Cast<ForeignKey>();

        public virtual IEnumerable<ForeignKey> GetForeignKeys()
            => BaseType?.GetForeignKeys().Concat(_foreignKeys.Values) ?? _foreignKeys.Values;

        #endregion

        #region Navigations

        public virtual Navigation AddNavigation([NotNull] string name, [NotNull] ForeignKey foreignKey, bool pointsToPrincipal)
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
                else
                {
                    throw new InvalidOperationException(CoreStrings.DuplicateNavigation(name, DisplayName(), duplicateNavigation.DeclaringEntityType.DisplayName()));
                }
            }

            var duplicateProperty = FindPropertiesInHierarchy(name).FirstOrDefault();
            if (duplicateProperty != null)
            {
                throw new InvalidOperationException(CoreStrings.ConflictingProperty(name, DisplayName(),
                    duplicateProperty.DeclaringEntityType.DisplayName()));
            }

            Debug.Assert(!GetNavigations().Any(n => n.ForeignKey == foreignKey && n.IsDependentToPrincipal() == pointsToPrincipal),
                "There is another navigation corresponding to the same foreign key and pointing in the same direction.");

            Debug.Assert((pointsToPrincipal ? foreignKey.DeclaringEntityType : foreignKey.PrincipalEntityType) == this,
                "EntityType mismatch");

            Navigation.IsCompatible(
                name,
                this,
                pointsToPrincipal ? foreignKey.PrincipalEntityType : foreignKey.DeclaringEntityType,
                !pointsToPrincipal && !((IForeignKey)foreignKey).IsUnique,
                shouldThrow: true);

            var navigation = new Navigation(name, foreignKey);
            _navigations.Add(name, navigation);

            return navigation;
        }

        public virtual Navigation FindNavigation([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            return FindDeclaredNavigation(name) ?? BaseType?.FindNavigation(name);
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
            else
            {
                _navigations.Remove(name);
                return navigation;
            }
        }

        public virtual IEnumerable<Navigation> GetNavigations()
            => BaseType?.GetNavigations().Concat(_navigations.Values) ?? _navigations.Values;

        #endregion

        #region Indexes

        public virtual Index AddIndex([NotNull] Property property) => AddIndex(new[] { property });

        public virtual Index AddIndex([NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));

            foreach (var property in properties)
            {
                if (FindProperty(property.Name) != property)
                {
                    throw new ArgumentException(CoreStrings.IndexPropertiesWrongEntity(Property.Format(properties), Name));
                }
            }

            var duplicateIndex = FindIndexesInHierarchy(properties).FirstOrDefault();
            if (duplicateIndex != null)
            {
                throw new InvalidOperationException(CoreStrings.DuplicateIndex(Property.Format(properties), DisplayName(), duplicateIndex.DeclaringEntityType.DisplayName()));
            }

            var index = new Index(properties);
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

            return FindDeclaredIndex(properties) ?? BaseType?.FindIndex(properties);
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
            return index;
        }

        public virtual IEnumerable<Index> GetIndexes() => BaseType?.GetIndexes().Concat(_indexes.Values) ?? _indexes.Values;

        #endregion

        #region Properties

        public virtual Property AddProperty([NotNull] PropertyInfo propertyInfo)
            => (Property)((IMutableEntityType)this).AddProperty(propertyInfo);

        public virtual Property AddProperty([NotNull] string name, [NotNull] Type propertyType)
            => (Property)((IMutableEntityType)this).AddProperty(name, propertyType);

        public virtual Property AddProperty([NotNull] string name)
        {
            Check.NotNull(name, nameof(name));

            var duplicateProperty = FindPropertiesInHierarchy(name).FirstOrDefault();
            if (duplicateProperty != null)
            {
                throw new InvalidOperationException(CoreStrings.DuplicateProperty(
                    name, DisplayName(), duplicateProperty.DeclaringEntityType.DisplayName()));
            }

            var duplicateNavigation = FindNavigationsInHierarchy(name).FirstOrDefault();
            if (duplicateNavigation != null)
            {
                throw new InvalidOperationException(CoreStrings.ConflictingNavigation(name, DisplayName(),
                    duplicateNavigation.DeclaringEntityType.DisplayName()));
            }

            var property = new Property(name, this);

            _properties.Add(name, property);

            PropertyMetadataChanged(property);

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

            return FindDeclaredProperty(name) ?? BaseType?.FindProperty(name);
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

            PropertyMetadataChanged(property);

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
            => BaseType?.GetProperties().Concat(_properties.Values) ?? _properties.Values;

        public virtual void PropertyMetadataChanged([CanBeNull] Property property)
        {
            var index = BaseType?.PropertyCount ?? 0;
            var shadowIndex = BaseType?.ShadowPropertyCount() ?? 0;
            var originalValueIndex = BaseType?.OriginalValueCount() ?? 0;

            foreach (var indexedProperty in _properties.Values)
            {
                indexedProperty.SetIndex(index++);

                if (((IProperty)indexedProperty).IsShadowProperty)
                {
                    indexedProperty.SetShadowIndex(shadowIndex++);
                }

                indexedProperty.SetOriginalValueIndex(
                    RequiresOriginalValue(indexedProperty) ? originalValueIndex++ : -1);
            }

            foreach (var derivedType in GetDirectlyDerivedTypes())
            {
                derivedType.PropertyMetadataChanged(property);
            }
        }

        private bool RequiresOriginalValue(Property addedOrRemovedProperty)
        {
            return _useEagerSnapshots
                   || ((IProperty)addedOrRemovedProperty).IsConcurrencyToken
                   || GetForeignKeys().SelectMany(k => k.Properties).Contains(addedOrRemovedProperty);
        }

        #endregion

        #region Explicit interface implementations

        IEntityType IEntityType.BaseType => BaseType;

        IModel IEntityType.Model => Model;

        IMutableModel IMutableEntityType.Model => Model;

        IMutableEntityType IMutableEntityType.BaseType
        {
            get { return BaseType; }
            set { BaseType = (EntityType)value; }
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

        private IEnumerable<T> ToEnumerable<T>(T element)
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
                if (xIndex == -1
                    && yIndex == -1)
                {
                    return StringComparer.Ordinal.Compare(x, y);
                }

                // Both properties are part of the Primary Key
                // Compare the indices
                if (xIndex > -1
                    && yIndex > -1)
                {
                    return xIndex - yIndex;
                }

                // One property is part of the Primary Key
                // The primary key property is first
                return (xIndex > yIndex)
                    ? -1
                    : 1;
            }
        }
    }
}
