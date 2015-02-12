// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class EntityType : MetadataBase, IEntityType
    {
        private readonly object _typeOrName;

        private readonly LazyRef<SortedDictionary<IReadOnlyList<Property>, ForeignKey>> _foreignKeys
            = new LazyRef<SortedDictionary<IReadOnlyList<Property>, ForeignKey>>(()
                => new SortedDictionary<IReadOnlyList<Property>, ForeignKey>(PropertyListComparer.Instance));

        private readonly LazyRef<SortedDictionary<string, Navigation>> _navigations
            = new LazyRef<SortedDictionary<string, Navigation>>(() =>
                new SortedDictionary<string, Navigation>(StringComparer.Ordinal));

        private readonly LazyRef<SortedDictionary<IReadOnlyList<Property>, Index>> _indexes
            = new LazyRef<SortedDictionary<IReadOnlyList<Property>, Index>>(() =>
                new SortedDictionary<IReadOnlyList<Property>, Index>(PropertyListComparer.Instance));

        private readonly SortedDictionary<string, Property> _properties
            = new SortedDictionary<string, Property>(StringComparer.Ordinal);

        private readonly LazyRef<SortedDictionary<IReadOnlyList<Property>, Key>> _keys
            = new LazyRef<SortedDictionary<IReadOnlyList<Property>, Key>>(() =>
                new SortedDictionary<IReadOnlyList<Property>, Key>(PropertyListComparer.Instance));

        private Key _primaryKey;

        private int _shadowPropertyCount;
        private int _originalValueCount;
        private bool _useEagerSnapshots;
        private static readonly char[] _simpleNameChars = { '.', '+' };

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected EntityType()
        {
        }

        /// <summary>
        ///     Creates a new metadata object representing an entity type associated with the given .NET type.
        /// </summary>
        /// <param name="type">The .NET entity type that this metadata object represents.</param>
        /// <param name="model">The model associated with this entity type.</param>
        public EntityType([NotNull] Type type, [NotNull] Model model)
        {
            Check.NotNull(type, "type");
            Check.ValidEntityType(type, "type");

            Model = model;
            _typeOrName = type;
            _useEagerSnapshots = !this.HasPropertyChangingNotifications();
        }

        /// <summary>
        ///     Creates a new metadata object representing an entity type that will participate in shadow-state
        ///     such that there is no underlying .NET type corresponding to this metadata object.
        /// </summary>
        /// <param name="name">The name of the shadow-state entity type.</param>
        /// <param name="model">The model associated with this entity type.</param>
        public EntityType([NotNull] string name, [NotNull] Model model)
        {
            Check.NotEmpty(name, "name");

            Model = model;
            _typeOrName = name;
        }

        public virtual Model Model { get; }

        public virtual Type Type => _typeOrName as Type;

        public virtual string Name
        {
            get
            {
                var type = _typeOrName as Type;
                return type != null ? type.FullName : (string)_typeOrName;
            }
        }

        public virtual string SimpleName
        {
            get
            {
                var type = _typeOrName as Type;
                if (type != null)
                {
                    return type.Name;
                }

                var fullName = (string)_typeOrName;
                var lastDot = fullName.LastIndexOfAny(_simpleNameChars);

                return lastDot > 0 ? fullName.Substring(lastDot + 1) : fullName;
            }
        }

        #region Primary and Candidate Keys

        [ContractAnnotation("null => null; notnull => notnull")]
        public virtual Key SetPrimaryKey([CanBeNull] Property property)
        {
            return SetPrimaryKey(property == null ? null : new[] { property });
        }

        [ContractAnnotation("null => null; notnull => notnull")]
        public virtual Key SetPrimaryKey([CanBeNull] IReadOnlyList<Property> properties)
        {
            Key key = null;
            if (properties != null
                && properties.Count != 0)
            {
                key = TryGetKey(properties);

                if (key != null)
                {
                    _keys.Value.Remove(properties);
                }
                else
                {
                    key = new Key(properties);
                    if (key.EntityType != this)
                    {
                        throw new ArgumentException(
                            Strings.KeyPropertiesWrongEntity(Property.Format(key.Properties), Name));
                    }
                }
            }

            if (_primaryKey != null
                && _primaryKey != key)
            {
                CheckKeyNotInUse(_primaryKey);
            }

            _primaryKey = key;
            return _primaryKey;
        }

        [ContractAnnotation("null => null; notnull => notnull")]
        public virtual Key GetOrSetPrimaryKey([CanBeNull] Property property)
        {
            return GetOrSetPrimaryKey(property == null ? null : new[] { property });
        }

        [ContractAnnotation("null => null; notnull => notnull")]
        public virtual Key GetOrSetPrimaryKey([CanBeNull] IReadOnlyList<Property> properties)
        {
            if (properties != null
                && TryGetPrimaryKey(properties) != null)
            {
                return _primaryKey;
            }

            return SetPrimaryKey(properties);
        }

        public virtual Key GetPrimaryKey()
        {
            if (_primaryKey == null)
            {
                throw new ModelItemNotFoundException(Strings.EntityRequiresKey(Name));
            }

            return _primaryKey;
        }

        [CanBeNull]
        public virtual Key TryGetPrimaryKey()
        {
            return _primaryKey;
        }

        [CanBeNull]
        public virtual Key TryGetPrimaryKey([CanBeNull] IReadOnlyList<Property> properties)
        {
            Check.NotNull(properties, "properties");

            if (_primaryKey != null
                && PropertyListComparer.Instance.Compare(_primaryKey.Properties, properties) == 0)
            {
                return _primaryKey;
            }

            return null;
        }

        public virtual Key AddKey([NotNull] Property property)
        {
            return AddKey(new[] { property });
        }

        public virtual Key AddKey([NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(properties, "properties");

            var key = TryGetKey(properties);
            if (key != null)
            {
                throw new InvalidOperationException(Strings.DuplicateKey(Property.Format(properties), Name));
            }

            key = new Key(properties);
            if (key.EntityType != this)
            {
                throw new ArgumentException(Strings.KeyPropertiesWrongEntity(Property.Format(properties), Name));
            }

            _keys.Value.Add(properties, key);

            return key;
        }

        public virtual Key GetOrAddKey([NotNull] Property property)
        {
            return GetOrAddKey(new[] { property });
        }

        public virtual Key GetOrAddKey([NotNull] IReadOnlyList<Property> properties)
        {
            return TryGetKey(properties)
                   ?? AddKey(properties);
        }

        [CanBeNull]
        public virtual Key TryGetKey([NotNull] Property property)
        {
            return TryGetKey(new[] { property });
        }

        [CanBeNull]
        public virtual Key TryGetKey([NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(properties, "properties");

            var key = TryGetPrimaryKey(properties);
            if (key != null)
            {
                return key;
            }

            if (_keys.HasValue
                && _keys.Value.TryGetValue(properties, out key))
            {
                return key;
            }

            return null;
        }

        public virtual Key GetKey([NotNull] Property property)
        {
            return GetKey(new[] { property });
        }

        public virtual Key GetKey([NotNull] IReadOnlyList<Property> properties)
        {
            var key = TryGetKey(properties);
            if (key == null)
            {
                throw new ModelItemNotFoundException(Strings.KeyNotFound(Property.Format(properties), Name));
            }

            return key;
        }

        public virtual Key RemoveKey([NotNull] Key key)
        {
            Check.NotNull(key, "key");

            var primaryKey = TryGetPrimaryKey(key.Properties);
            if (primaryKey != null)
            {
                SetPrimaryKey((IReadOnlyList<Property>)null);
                return primaryKey;
            }

            Key removedKey;
            if (_keys.HasValue
                && _keys.Value.TryGetValue(key.Properties, out removedKey))
            {
                CheckKeyNotInUse(removedKey);

                _keys.Value.Remove(key.Properties);
                return removedKey;
            }

            return null;
        }

        private void CheckKeyNotInUse(Key key)
        {
            var foreignKey = Model?.EntityTypes.SelectMany(e => e.ForeignKeys).FirstOrDefault(k => k.ReferencedKey == key);

            if (foreignKey != null)
            {
                throw new InvalidOperationException(Strings.KeyInUse(Property.Format(key.Properties), Name, foreignKey.EntityType.Name));
            }
        }

        public virtual IReadOnlyList<Key> Keys
        {
            get
            {
                var keys = _primaryKey != null ? new List<Key> { _primaryKey } : new List<Key>();

                if (_keys.HasValue)
                {
                    keys.AddRange(_keys.Value.Values);
                }

                return keys;
            }
        }

        #endregion

        #region Foreign Keys

        public virtual ForeignKey AddForeignKey(
            [NotNull] Property property, [NotNull] Key referencedKey)
        {
            return AddForeignKey(new[] { property }, referencedKey);
        }

        public virtual ForeignKey AddForeignKey(
            [NotNull] IReadOnlyList<Property> properties, [NotNull] Key referencedKey)
        {
            Check.NotEmpty(properties, "properties");
            Check.NotNull(referencedKey, "referencedKey");

            if (_foreignKeys.Value.ContainsKey(properties))
            {
                throw new InvalidOperationException(Strings.DuplicateForeignKey(Property.Format(properties), Name));
            }

            var foreignKey = new ForeignKey(properties, referencedKey);
            if (foreignKey.EntityType != this)
            {
                throw new ArgumentException(Strings.ForeignKeyPropertiesWrongEntity(Property.Format(properties), Name));
            }

            _foreignKeys.Value.Add(properties, foreignKey);

            UpdateOriginalValueIndexes();

            return foreignKey;
        }

        public virtual ForeignKey GetOrAddForeignKey(
            [NotNull] Property property, [NotNull] Key referencedKey)
        {
            return GetOrAddForeignKey(new[] { property }, referencedKey);
        }

        public virtual ForeignKey GetOrAddForeignKey(
            [NotNull] IReadOnlyList<Property> properties, [NotNull] Key referencedKey)
        {
            // Note: this will return an existing foreign key even if it doesn't have the same referenced key
            return TryGetForeignKey(properties)
                   ?? AddForeignKey(properties, referencedKey);
        }

        [CanBeNull]
        public virtual ForeignKey TryGetForeignKey([NotNull] Property property)
        {
            return TryGetForeignKey(new[] { property });
        }

        [CanBeNull]
        public virtual ForeignKey TryGetForeignKey([NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(properties, "properties");

            ForeignKey foreignKey;
            if (_foreignKeys.HasValue
                && _foreignKeys.Value.TryGetValue(properties, out foreignKey))
            {
                return foreignKey;
            }

            return null;
        }

        [CanBeNull]
        public virtual ForeignKey TryGetForeignKey(
            [NotNull] EntityType principalType,
            [CanBeNull] string navigationToPrincipal,
            [CanBeNull] string navigationToDependent,
            [CanBeNull] IReadOnlyList<Property> foreignKeyProperties,
            [CanBeNull] IReadOnlyList<Property> referencedProperties,
            bool? isUnique)
        {
            Check.NotNull(principalType, "principalType");

            return ForeignKeys.FirstOrDefault(fk =>
                fk.IsCompatible(
                    principalType,
                    this,
                    navigationToPrincipal,
                    navigationToDependent,
                    foreignKeyProperties,
                    referencedProperties,
                    isUnique));
        }

        public virtual ForeignKey GetForeignKey([NotNull] Property property)
        {
            return GetForeignKey(new[] { property });
        }

        public virtual ForeignKey GetForeignKey([NotNull] IReadOnlyList<Property> properties)
        {
            var foreignKey = TryGetForeignKey(properties);
            if (foreignKey == null)
            {
                throw new ModelItemNotFoundException(Strings.ForeignKeyNotFound(Property.Format(properties), Name));
            }

            return foreignKey;
        }

        public virtual ForeignKey RemoveForeignKey([NotNull] ForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            ForeignKey removedFk;
            if (_foreignKeys.HasValue
                && _foreignKeys.Value.TryGetValue(foreignKey.Properties, out removedFk))
            {
                CheckForeignKeyNotInUse(removedFk);

                _foreignKeys.Value.Remove(removedFk.Properties);
                return removedFk;
            }

            return null;
        }

        private void CheckForeignKeyNotInUse(ForeignKey foreignKey)
        {
            var navigation = foreignKey.GetNavigationToDependent() ?? foreignKey.GetNavigationToPrincipal();

            if (navigation != null)
            {
                throw new InvalidOperationException(Strings.ForeignKeyInUse(Property.Format(foreignKey.Properties), Name, navigation.Name, navigation.EntityType.Name));
            }
        }

        public virtual IReadOnlyList<ForeignKey> ForeignKeys => _foreignKeys.HasValue
            ? (IReadOnlyList<ForeignKey>)_foreignKeys.Value.Values.ToList()
            : ImmutableList<ForeignKey>.Empty;

        #endregion

        #region Navigations

        public virtual Navigation AddNavigation([NotNull] string name, [NotNull] ForeignKey foreignKey, bool pointsToPrincipal)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(foreignKey, "foreignKey");

            if (_navigations.HasValue
                && _navigations.Value.ContainsKey(name))
            {
                throw new InvalidOperationException(Strings.DuplicateNavigation(name, Name));
            }

            var navigation = new Navigation(name, foreignKey, pointsToPrincipal);

            if (navigation.EntityType != null
                && navigation.EntityType != this)
            {
                throw new InvalidOperationException(Strings.NavigationAlreadyOwned(navigation.Name, Name, navigation.EntityType.Name));
            }

            if (!HasClrType)
            {
                throw new InvalidOperationException(Strings.NavigationOnShadowEntity(navigation.Name, Name));
            }

            var clrProperty = Type.GetPropertiesInHierarchy(navigation.Name).FirstOrDefault();
            if (clrProperty == null)
            {
                throw new InvalidOperationException(Strings.NoClrNavigation(navigation.Name, Name));
            }

            var targetType = navigation.GetTargetType();
            if (!targetType.HasClrType)
            {
                throw new InvalidOperationException(Strings.NavigationToShadowEntity(navigation.Name, Name, targetType.Name));
            }

            var targetClrType = targetType.Type;
            Debug.Assert(targetClrType != null, "targetClrType != null");
            if (navigation.IsCollection())
            {
                var elementType = clrProperty.PropertyType.TryGetElementType(typeof(IEnumerable<>));

                if (elementType == null
                    || !elementType.GetTypeInfo().IsAssignableFrom(targetClrType.GetTypeInfo()))
                {
                    throw new InvalidOperationException(Strings.NavigationCollectionWrongClrType(
                        navigation.Name, Name, clrProperty.PropertyType.FullName, targetClrType.FullName));
                }
            }
            else if (!clrProperty.PropertyType.GetTypeInfo().IsAssignableFrom(targetClrType.GetTypeInfo()))
            {
                throw new InvalidOperationException(Strings.NavigationSingleWrongClrType(
                    navigation.Name, Name, clrProperty.PropertyType.FullName, targetClrType.FullName));
            }

            var otherNavigation = _navigations.Value.Values.FirstOrDefault(n => n.ForeignKey == navigation.ForeignKey
                                                                                && navigation.PointsToPrincipal == n.PointsToPrincipal);
            if (otherNavigation != null)
            {
                throw new InvalidOperationException(Strings.MultipleNavigations(navigation.Name, otherNavigation.Name, Name));
            }

            _navigations.Value.Add(name, navigation);

            return navigation;
        }

        public virtual Navigation GetOrAddNavigation([NotNull] string name, [NotNull] ForeignKey foreignKey, bool pointsToPrincipal)
        {
            return TryGetNavigation(name) ?? AddNavigation(name, foreignKey, pointsToPrincipal);
        }

        [CanBeNull]
        public virtual Navigation TryGetNavigation([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            Navigation navigation;
            if (_navigations.HasValue
                && _navigations.Value.TryGetValue(name, out navigation))
            {
                return navigation;
            }
            return null;
        }

        public virtual Navigation GetNavigation([NotNull] string name)
        {
            var navigation = TryGetNavigation(name);
            if (navigation == null)
            {
                throw new ModelItemNotFoundException(Strings.NavigationNotFound(name, Name));
            }
            return navigation;
        }

        public virtual Navigation RemoveNavigation([NotNull] Navigation navigation)
        {
            Check.NotNull(navigation, "navigation");

            Navigation removedNavigation;
            if (_navigations.HasValue & _navigations.Value.TryGetValue(navigation.Name, out removedNavigation))
            {
                _navigations.Value.Remove(navigation.Name);
                return removedNavigation;
            }

            return null;
        }

        public virtual IReadOnlyList<Navigation> Navigations => _navigations.HasValue
            ? (IReadOnlyList<Navigation>)_navigations.Value.Values.ToList()
            : ImmutableList<Navigation>.Empty;

        #endregion

        #region Indexes

        public virtual Index AddIndex([NotNull] Property property)
        {
            return AddIndex(new[] { property });
        }

        public virtual Index AddIndex([NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(properties, "properties");

            if (_indexes.Value.ContainsKey(properties))
            {
                throw new InvalidOperationException(Strings.DuplicateIndex(Property.Format(properties), Name));
            }

            var index = new Index(properties);

            if (index.EntityType != this)
            {
                throw new ArgumentException(Strings.IndexPropertiesWrongEntity(Property.Format(properties), Name));
            }

            _indexes.Value.Add(properties, index);

            return index;
        }

        public virtual Index GetOrAddIndex([NotNull] Property property)
        {
            return GetOrAddIndex(new[] { property });
        }

        public virtual Index GetOrAddIndex([NotNull] IReadOnlyList<Property> properties)
        {
            return TryGetIndex(properties) ?? AddIndex(properties);
        }

        [CanBeNull]
        public virtual Index TryGetIndex([NotNull] Property property)
        {
            return TryGetIndex(new[] { property });
        }

        [CanBeNull]
        public virtual Index TryGetIndex([NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(properties, "properties");

            Index index;
            if (_indexes.HasValue
                && _indexes.Value.TryGetValue(properties, out index))
            {
                return index;
            }
            return null;
        }

        public virtual Index GetIndex([NotNull] Property property)
        {
            return GetIndex(new[] { property });
        }

        public virtual Index GetIndex([NotNull] IReadOnlyList<Property> properties)
        {
            var index = TryGetIndex(properties);
            if (index == null)
            {
                throw new ModelItemNotFoundException(Strings.IndexNotFound(Property.Format(properties), Name));
            }
            return index;
        }

        public virtual Index RemoveIndex([NotNull] Index index)
        {
            Check.NotNull(index, "index");

            Index removedIndex;
            if (_indexes.HasValue
                && _indexes.Value.TryGetValue(index.Properties, out removedIndex))
            {
                _indexes.Value.Remove(index.Properties);
                return removedIndex;
            }

            return null;
        }

        public virtual IReadOnlyList<Index> Indexes => _indexes.HasValue
            ? (IReadOnlyList<Index>)_indexes.Value.Values.ToList()
            : ImmutableList<Index>.Empty;

        #endregion

        #region Properties

        [NotNull]
        public virtual Property AddProperty([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, "propertyInfo");

            // ReSharper disable once RedundantArgumentDefaultValue
            return AddProperty(propertyInfo.Name, propertyInfo.PropertyType, shadowProperty: false);
        }

        [NotNull]
        public virtual Property AddProperty([NotNull] string name, [NotNull] Type propertyType, bool shadowProperty = false)
        {
            Check.NotNull(name, "name");
            Check.NotNull(propertyType, "propertyType");

            if (_properties.ContainsKey(name))
            {
                throw new InvalidOperationException(Strings.DuplicateProperty(name, Name));
            }

            var property = new Property(name, propertyType, this, shadowProperty);

            ValidateAgainstClrProperty(property);

            _properties.Add(name, property);
            UpdateIndexes(property);

            return property;
        }

        private void ValidateAgainstClrProperty(Property property)
        {
            if (!property.IsShadowProperty)
            {
                if (HasClrType)
                {
                    var clrProperty = Type.GetPropertiesInHierarchy(property.Name).FirstOrDefault();

                    if (clrProperty == null)
                    {
                        throw new InvalidOperationException(Strings.NoClrProperty(property.Name, Name));
                    }

                    if (property.PropertyType != clrProperty.PropertyType)
                    {
                        throw new InvalidOperationException(Strings.PropertyWrongClrType(property.Name, Name));
                    }
                }
                else
                {
                    throw new InvalidOperationException(Strings.ClrPropertyOnShadowEntity(property.Name, Name));
                }
            }
        }

        private void UpdateIndexes(Property addedOrRemovedProperty)
        {
            var index = 0;
            foreach (var property in _properties.Values)
            {
                property.Index = index++;
            }

            PropertyMetadataChanged(addedOrRemovedProperty);
        }

        internal void PropertyMetadataChanged(Property property)
        {
            ValidateAgainstClrProperty(property);

            UpdateShadowIndexes();
            UpdateOriginalValueIndexes(property);
        }

        private void UpdateShadowIndexes()
        {
            var shadowIndex = 0;
            foreach (var property in _properties.Values.Where(p => p.IsShadowProperty))
            {
                property.ShadowIndex = shadowIndex++;
            }
            _shadowPropertyCount = shadowIndex;
        }

        private void UpdateOriginalValueIndexes(Property addedOrRemovedProperty)
        {
            if (RequiresOriginalValue(addedOrRemovedProperty))
            {
                UpdateOriginalValueIndexes();
            }
        }

        private void UpdateOriginalValueIndexes()
        {
            var originalValueIndex = 0;
            foreach (var property in _properties.Values)
            {
                property.OriginalValueIndex = RequiresOriginalValue(property) ? originalValueIndex++ : -1;
            }
            _originalValueCount = originalValueIndex;
        }

        private bool RequiresOriginalValue(Property addedOrRemovedProperty)
        {
            return _useEagerSnapshots
                   || ((IProperty)addedOrRemovedProperty).IsConcurrencyToken
                   || ForeignKeys.SelectMany(k => k.Properties).Contains(addedOrRemovedProperty);
        }

        public virtual Property GetOrAddProperty([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, "propertyInfo");

            return GetOrAddProperty(propertyInfo.Name, propertyInfo.PropertyType);
        }

        public virtual Property GetOrAddProperty([NotNull] string name, [NotNull] Type propertyType, bool shadowProperty = false)
        {
            // Note: If the property already exists, then whether or not it is a shadow property is not changed.
            // It is useful in many places to get an existing property if it exists, but then create it either in
            // or out of shadow state if it doesn't.
            return TryGetProperty(name) ?? AddProperty(name, propertyType, shadowProperty);
        }

        [CanBeNull]
        public virtual Property TryGetProperty([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, "propertyInfo");

            return TryGetProperty(propertyInfo.Name);
        }

        [CanBeNull]
        public virtual Property TryGetProperty([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, "propertyName");

            Property property;
            if (_properties.TryGetValue(propertyName, out property))
            {
                return property;
            }

            return null;
        }

        public virtual Property GetProperty([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, "propertyInfo");

            return GetProperty(propertyInfo.Name);
        }

        public virtual Property GetProperty([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, "propertyName");

            var property = TryGetProperty(propertyName);
            if (property == null)
            {
                throw new ModelItemNotFoundException(Strings.PropertyNotFound(propertyName, Name));
            }
            return property;
        }

        public virtual Property RemoveProperty([NotNull] Property property)
        {
            Check.NotNull(property, "property");

            Property removedProperty;
            if (_properties.TryGetValue(property.Name, out removedProperty))
            {
                if (Keys.Any(k => k.Properties.Contains(property))
                    || ForeignKeys.Any(k => k.Properties.Contains(property))
                    || Indexes.Any(i => i.Properties.Contains(property)))
                {
                    throw new InvalidOperationException(Strings.PropertyInUse(property.Name, Name));
                }

                _properties.Remove(property.Name);
                UpdateIndexes(property);

                return removedProperty;
            }

            return null;
        }

        public virtual IReadOnlyList<Property> Properties => _properties.Values.ToList();

        public virtual int ShadowPropertyCount => _shadowPropertyCount;

        public virtual int OriginalValueCount => _originalValueCount;

        public virtual bool HasClrType => Type != null;

        public virtual bool UseEagerSnapshots
        {
            get { return _useEagerSnapshots; }
            set
            {
                if (!value
                    && !this.HasPropertyChangingNotifications())
                {
                    throw new InvalidOperationException(Strings.EagerOriginalValuesRequired(Name));
                }

                _useEagerSnapshots = value;
                UpdateOriginalValueIndexes();
            }
        }

        #endregion

        public override string ToString()
        {
            return Name;
        }

        #region Explicit interface implementations

        IModel IEntityType.Model
        {
            get { return Model; }
        }

        IKey IEntityType.TryGetPrimaryKey()
        {
            return TryGetPrimaryKey();
        }

        IKey IEntityType.GetPrimaryKey()
        {
            return GetPrimaryKey();
        }

        IProperty IEntityType.TryGetProperty(string propertyName)
        {
            return TryGetProperty(propertyName);
        }

        IProperty IEntityType.GetProperty(string propertyName)
        {
            return GetProperty(propertyName);
        }

        INavigation IEntityType.TryGetNavigation(string name)
        {
            return TryGetNavigation(name);
        }

        INavigation IEntityType.GetNavigation(string name)
        {
            return GetNavigation(name);
        }

        IReadOnlyList<IProperty> IEntityType.Properties => Properties;

        IReadOnlyList<IForeignKey> IEntityType.ForeignKeys => ForeignKeys;

        IReadOnlyList<INavigation> IEntityType.Navigations => Navigations;

        IReadOnlyList<IIndex> IEntityType.Indexes => Indexes;

        IReadOnlyList<IKey> IEntityType.Keys => Keys;

        #endregion

        private class PropertyListComparer : IComparer<IReadOnlyList<Property>>
        {
            public static readonly PropertyListComparer Instance = new PropertyListComparer();

            private PropertyListComparer()
            {
            }

            public int Compare(IReadOnlyList<Property> x, IReadOnlyList<Property> y)
            {
                var result = x.Count - y.Count;

                if (result != 0)
                {
                    return result;
                }

                var index = 0;
                while (result == 0
                       && index < x.Count)
                {
                    result = StringComparer.Ordinal.Compare(x[index].Name, y[index].Name);
                    index++;
                }
                return result;
            }
        }
    }
}
