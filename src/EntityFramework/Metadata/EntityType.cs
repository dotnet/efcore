// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    [DebuggerDisplay("{Name,nq}")]
    public class EntityType : MetadataBase, IEntityType
    {
        private readonly object _typeOrName;
        private readonly LazyRef<List<ForeignKey>> _foreignKeys = new LazyRef<List<ForeignKey>>(() => new List<ForeignKey>());
        private readonly LazyRef<List<Navigation>> _navigations = new LazyRef<List<Navigation>>(() => new List<Navigation>());
        private readonly LazyRef<List<Index>> _indexes = new LazyRef<List<Index>>(() => new List<Index>());
        private readonly List<Property> _properties = new List<Property>();
        private readonly LazyRef<List<Key>> _keys = new LazyRef<List<Key>>(() => new List<Key>());

        private Key _primaryKey;

        private int _shadowPropertyCount;
        private int _originalValueCount;
        private bool _useLazyOriginalValues = true;
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
        public EntityType([NotNull] Type type)
        {
            Check.NotNull(type, "type");

            _typeOrName = type;
            _useLazyOriginalValues = CanUseLazyOriginalValues();
        }

        /// <summary>
        ///     Creates a new metadata object representing an entity type that will participate in shadow-state
        ///     such that there is no underlying .NET type corresponding to this metadata object.
        /// </summary>
        /// <param name="name">The name of the shadow-state entity type.</param>
        public EntityType([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            _typeOrName = name;
        }

        public virtual Model Model { get; internal set; }

        public virtual Type Type
        {
            get { return _typeOrName as Type; }
        }

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

        [CanBeNull]
        public virtual Key SetPrimaryKey([CanBeNull] Property property)
        {
            return SetPrimaryKey(property == null ? null : new[] { property });
        }

        [CanBeNull]
        public virtual Key SetPrimaryKey([CanBeNull] IReadOnlyList<Property> properties)
        {
            Key key = null;
            if (properties != null
                && properties.Count != 0)
            {
                key = new Key(properties);

                if (key.EntityType != this)
                {
                    throw new ArgumentException(
                        Strings.FormatKeyPropertiesWrongEntity(key, Name));
                }
            }

            if (_primaryKey != null)
            {
                CheckKeyNotInUse(_primaryKey);
            }

            _primaryKey = key;
            return _primaryKey;
        }

        [CanBeNull]
        public virtual Key GetOrSetPrimaryKey([CanBeNull] Property property)
        {
            return GetOrSetPrimaryKey(property == null ? null : new[] { property });
        }

        [CanBeNull]
        public virtual Key GetOrSetPrimaryKey([CanBeNull] IReadOnlyList<Property> properties)
        {
            if (properties != null
                && properties.Count != 0
                && TryGetPrimaryKey(properties) != null)
            {
                return _primaryKey;
            }

            return SetPrimaryKey(properties);
        }

        [NotNull]
        public virtual Key GetPrimaryKey()
        {
            if (_primaryKey == null)
            {
                throw new ModelItemNotFoundException(Strings.FormatEntityRequiresKey(Name));
            }

            return _primaryKey;
        }

        [CanBeNull]
        public virtual Key TryGetPrimaryKey()
        {
            return _primaryKey;
        }

        [CanBeNull]
        public virtual Key TryGetPrimaryKey([CanBeNull] Property property)
        {
            return TryGetPrimaryKey(property == null ? null : new[] { property });
        }

        [CanBeNull]
        public virtual Key TryGetPrimaryKey([CanBeNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(properties, "properties");

            if (_primaryKey == null
                || !_primaryKey.Properties.SequenceEqual(properties))
            {
                return null;
            }

            return _primaryKey;
        }

        [NotNull]
        public virtual Key AddKey([NotNull] Property property)
        {
            return AddKey(new[] { property });
        }

        [NotNull]
        public virtual Key AddKey([NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(properties, "properties");

            var key = TryGetKey(properties);
            if (key != null)
            {
                throw new InvalidOperationException(Strings.FormatDuplicateKey(FormatProperties(properties), Name));
            }

            key = new Key(properties);
            if (key.EntityType != this)
            {
                throw new ArgumentException(
                    Strings.FormatKeyPropertiesWrongEntity(FormatProperties(properties), Name));
            }

            _keys.Value.Add(key);

            return key;
        }

        [NotNull]
        public virtual Key GetOrAddKey([NotNull] Property property)
        {
            return GetOrAddKey(new[] { property });
        }

        [NotNull]
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

            if (_primaryKey != null
                && _primaryKey.Properties.SequenceEqual(properties))
            {
                return _primaryKey;
            }

            return Keys.FirstOrDefault(k => k.Properties.SequenceEqual(properties));
        }

        [NotNull]
        public virtual Key GetKey([NotNull] Property property)
        {
            return GetKey(new[] { property });
        }

        [NotNull]
        public virtual Key GetKey([NotNull] IReadOnlyList<Property> properties)
        {
            var key = TryGetKey(properties);
            if (key == null)
            {
                throw new ModelItemNotFoundException(Strings.FormatKeyNotFound(new Key(properties), Name));
            }
            return key;
        }

        public virtual void RemoveKey([NotNull] Key key)
        {
            Check.NotNull(key, "key");

            if (key == _primaryKey)
            {
                SetPrimaryKey((Property)null);
                return;
            }
            if (_keys.HasValue)
            {
                CheckKeyNotInUse(key);

                if (_keys.Value.Remove(key))
                {
                    return;
                }
            }

            throw new ModelItemNotFoundException(Strings.FormatKeyNotFound(key, Name));
        }

        private void CheckKeyNotInUse(Key key)
        {
            if (Model != null)
            {
                var foreignKey = Model.EntityTypes.SelectMany(e => e.ForeignKeys).FirstOrDefault(k => k.ReferencedKey == key);

                if (foreignKey != null)
                {
                    throw new InvalidOperationException(Strings.FormatKeyInUse(key, Name, foreignKey.EntityType.Name));
                }
            }
        }

        public virtual IReadOnlyList<Key> Keys
        {
            get
            {
                var keys = _primaryKey != null ? new List<Key> { _primaryKey } : new List<Key>();

                if (_keys.HasValue)
                {
                    keys.AddRange(_keys.Value);
                }

                return keys;
            }
        }

        #endregion

        #region Foreign Keys

        [NotNull]
        public virtual ForeignKey AddForeignKey(
            [NotNull] Property property, [NotNull] Key referencedKey)
        {
            return AddForeignKey(new[] { property }, referencedKey);
        }

        [NotNull]
        public virtual ForeignKey AddForeignKey(
            [NotNull] IReadOnlyList<Property> properties, [NotNull] Key referencedKey)
        {
            Check.NotEmpty(properties, "properties");
            Check.NotNull(referencedKey, "referencedKey");

            var foreignKey = TryGetForeignKey(properties);
            if (foreignKey != null)
            {
                throw new InvalidOperationException(Strings.FormatDuplicateForeignKey(FormatProperties(foreignKey.Properties), Name));
            }

            foreignKey = new ForeignKey(properties, referencedKey);
            if (foreignKey.EntityType != this)
            {
                throw new ArgumentException(Strings.FormatForeignKeyPropertiesWrongEntity(FormatProperties(properties), Name));
            }

            _foreignKeys.Value.Add(foreignKey);

            UpdateOriginalValueIndexes();

            return foreignKey;
        }

        [NotNull]
        public virtual ForeignKey GetOrAddForeignKey(
            [NotNull] Property property, [NotNull] Key referencedKey)
        {
            return GetOrAddForeignKey(new[] { property }, referencedKey);
        }

        [NotNull]
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

            return _foreignKeys.HasValue
                ? _foreignKeys.Value.FirstOrDefault(k => k.Properties.SequenceEqual(properties))
                : null;
        }

        [NotNull]
        public virtual ForeignKey GetForeignKey([NotNull] Property property)
        {
            return GetForeignKey(new[] { property });
        }

        [NotNull]
        public virtual ForeignKey GetForeignKey([NotNull] IReadOnlyList<Property> properties)
        {
            var foreignKey = TryGetForeignKey(properties);
            if (foreignKey == null)
            {
                throw new ModelItemNotFoundException(Strings.FormatForeignKeyNotFound(FormatProperties(properties), Name));
            }

            return foreignKey;
        }

        public virtual void RemoveForeignKey([NotNull] ForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            if (_foreignKeys.HasValue)
            {
                CheckForeignKeyNotInUse(foreignKey);

                if (_foreignKeys.Value.Remove(foreignKey))
                {
                    return;
                }
            }

            throw new ModelItemNotFoundException(Strings.FormatForeignKeyNotFound(FormatProperties(foreignKey.Properties), Name));
        }

        private void CheckForeignKeyNotInUse(ForeignKey foreignKey)
        {
            var navigation = (Model == null
                ? new[] { this }
                : Model.EntityTypes).SelectMany(e => e.Navigations).FirstOrDefault(k => k.ForeignKey == foreignKey);

            if (navigation != null)
            {
                throw new InvalidOperationException(Strings.FormatForeignKeyInUse(FormatProperties(foreignKey.Properties), Name, navigation.Name, navigation.EntityType.Name));
            }
        }

        public virtual IReadOnlyList<ForeignKey> ForeignKeys
        {
            get
            {
                return _foreignKeys.HasValue
                    ? (IReadOnlyList<ForeignKey>)_foreignKeys.Value
                    : ImmutableList<ForeignKey>.Empty;
            }
        }

        #endregion

        #region Navigations

        [NotNull]
        public virtual Navigation AddNavigation([NotNull] string name, [NotNull] ForeignKey foreignKey, bool pointsToPrincipal)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(foreignKey, "foreignKey");

            var navigation = new Navigation(name, foreignKey, pointsToPrincipal);

            if (navigation.EntityType != null
                && navigation.EntityType != this)
            {
                throw new InvalidOperationException(Strings.FormatNavigationAlreadyOwned(navigation.Name, Name, navigation.EntityType.Name));
            }

            if (TryGetNavigation(name) != null)
            {
                throw new InvalidOperationException(Strings.FormatDuplicateNavigation(navigation.Name, Name));
            }

            if (!HasClrType)
            {
                throw new InvalidOperationException(Strings.FormatNavigationOnShadowEntity(navigation.Name, Name));
            }

            var clrProperty = Type.GetPropertiesInHierarchy(navigation.Name).FirstOrDefault();
            if (clrProperty == null)
            {
                throw new InvalidOperationException(Strings.FormatNoClrNavigation(navigation.Name, Name));
            }

            var targetType = navigation.GetTargetType();
            if (!targetType.HasClrType)
            {
                throw new InvalidOperationException(Strings.FormatNavigationToShadowEntity(navigation.Name, Name, targetType.Name));
            }

            var targetClrType = targetType.Type;
            Debug.Assert(targetClrType != null, "targetClrType != null");
            if (navigation.IsCollection())
            {
                var elementType = clrProperty.PropertyType.TryGetElementType(typeof(IEnumerable<>));

                if (elementType == null
                    || !elementType.GetTypeInfo().IsAssignableFrom(targetClrType.GetTypeInfo()))
                {
                    throw new InvalidOperationException(Strings.FormatWrongClrCollectionNavigationType(
                        navigation.Name, Name, clrProperty.PropertyType.FullName, targetClrType.FullName));
                }
            }
            else if (!clrProperty.PropertyType.GetTypeInfo().IsAssignableFrom(targetClrType.GetTypeInfo()))
            {
                throw new InvalidOperationException(Strings.FormatWrongClrSingleNavigationType(
                    navigation.Name, Name, clrProperty.PropertyType.FullName, targetClrType.FullName));
            }

            var otherNavigation = _navigations.Value.FirstOrDefault(n => n.ForeignKey == navigation.ForeignKey
                                                                         && navigation.PointsToPrincipal == n.PointsToPrincipal);
            if (otherNavigation != null)
            {
                throw new InvalidOperationException(Strings.FormatMultipleNavigations(navigation.Name, otherNavigation.Name, Name));
            }

            _navigations.Value.Add(navigation);

            return navigation;
        }

        [NotNull]
        public virtual Navigation GetOrAddNavigation([NotNull] string name, [NotNull] ForeignKey foreignKey, bool pointsToPrincipal)
        {
            return TryGetNavigation(name) ?? AddNavigation(name, foreignKey, pointsToPrincipal);
        }

        [CanBeNull]
        public virtual Navigation TryGetNavigation([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            return Navigations.FirstOrDefault(n => n.Name.Equals(name, StringComparison.Ordinal));
        }

        [NotNull]
        public virtual Navigation GetNavigation([NotNull] string name)
        {
            var navigation = TryGetNavigation(name);
            if (navigation == null)
            {
                throw new ModelItemNotFoundException(Strings.FormatNavigationNotFound(name, Name));
            }
            return navigation;
        }

        public virtual void RemoveNavigation([NotNull] Navigation navigation)
        {
            Check.NotNull(navigation, "navigation");

            if (!_navigations.HasValue
                || !_navigations.Value.Remove(navigation))
            {
                throw new ModelItemNotFoundException(Strings.FormatNavigationNotFound(navigation.Name, Name));
            }
        }

        public virtual IReadOnlyList<Navigation> Navigations
        {
            get
            {
                return _navigations.HasValue
                    ? (IReadOnlyList<Navigation>)_navigations.Value
                    : ImmutableList<Navigation>.Empty;
            }
        }

        #endregion

        #region Indexes

        [NotNull]
        public virtual Index AddIndex([NotNull] Property property)
        {
            return AddIndex(new[] { property });
        }

        [NotNull]
        public virtual Index AddIndex([NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(properties, "properties");

            var index = TryGetIndex(properties);
            if (index != null)
            {
                throw new InvalidOperationException(Strings.FormatDuplicateIndex(FormatProperties(index.Properties), Name));
            }

            index = new Index(properties);
            if (index.EntityType != this)
            {
                throw new ArgumentException(
                    Strings.FormatIndexPropertiesWrongEntity(FormatProperties(properties), Name));
            }

            _indexes.Value.Add(index);

            return index;
        }

        [NotNull]
        public virtual Index GetOrAddIndex([NotNull] Property property)
        {
            return GetOrAddIndex(new[] { property });
        }

        [NotNull]
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

            return Indexes.FirstOrDefault(i => i.Properties.SequenceEqual(properties));
        }

        [NotNull]
        public virtual Index GetIndex([NotNull] Property property)
        {
            return GetIndex(new[] { property });
        }

        [NotNull]
        public virtual Index GetIndex([NotNull] IReadOnlyList<Property> properties)
        {
            var index = TryGetIndex(properties);
            if (index == null)
            {
                throw new ModelItemNotFoundException(Strings.FormatIndexNotFound(FormatProperties(properties), Name));
            }
            return index;
        }

        public virtual void RemoveIndex([NotNull] Index index)
        {
            Check.NotNull(index, "index");

            if (!_indexes.HasValue
                || !_indexes.Value.Remove(index))
            {
                throw new ModelItemNotFoundException(Strings.FormatIndexNotFound(FormatProperties(index.Properties), Name));
            }
        }

        public virtual IReadOnlyList<Index> Indexes
        {
            get
            {
                return _indexes.HasValue
                    ? (IReadOnlyList<Index>)_indexes.Value
                    : ImmutableList<Index>.Empty;
            }
        }

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

            var property = new Property(name, propertyType, this, shadowProperty);

            var currentIndex = _properties.BinarySearch(property, PropertyComparer.Instance);
            if (currentIndex >= 0)
            {
                throw new InvalidOperationException(Strings.FormatDuplicateProperty(property.Name, Name));
            }

            ValidateAgainstClrProperty(property);

            var newIndex = ~currentIndex;
            _properties.Insert(newIndex, property);

            UpdateIndexes(property, newIndex);

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
                        throw new InvalidOperationException(Strings.FormatNoClrProperty(property.Name, Name));
                    }

                    if (property.PropertyType != clrProperty.PropertyType)
                    {
                        throw new InvalidOperationException(Strings.FormatWrongClrPropertyType(property.Name, Name));
                    }
                }
                else
                {
                    throw new InvalidOperationException(Strings.FormatClrPropertyOnShadowEntity(property.Name, Name));
                }
            }
        }

        private void UpdateIndexes(Property addedOrRemovedProperty, int startingIndex)
        {
            for (var i = startingIndex; i < _properties.Count; i++)
            {
                _properties[i].Index = i;
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
            foreach (var property in _properties.Where(p => p.IsShadowProperty))
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
            foreach (var property in _properties)
            {
                property.OriginalValueIndex = RequiresOriginalValue(property) ? originalValueIndex++ : -1;
            }
            _originalValueCount = originalValueIndex;
        }

        private bool RequiresOriginalValue(Property addedOrRemovedProperty)
        {
            return !_useLazyOriginalValues
                   || addedOrRemovedProperty.IsConcurrencyToken
                   || ForeignKeys.SelectMany(k => k.Properties).Contains(addedOrRemovedProperty);
        }

        [NotNull]
        public virtual Property GetOrAddProperty([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, "propertyInfo");

            return GetOrAddProperty(propertyInfo.Name, propertyInfo.PropertyType);
        }

        [NotNull]
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
        public virtual Property TryGetProperty([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            // TODO: This should be O(log(n)) but an additional index could be created
            // TODO: if this is too slow or if creating the surrogate Property object is too expensive
            var surrogate = new Property(name, typeof(object), new EntityType(typeof(object)));
            var index = _properties.BinarySearch(surrogate, PropertyComparer.Instance);
            return index >= 0 ? _properties[index] : null;
        }

        [NotNull]
        public virtual Property GetProperty([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, "propertyInfo");

            return GetProperty(propertyInfo.Name);
        }

        [NotNull]
        public virtual Property GetProperty([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            var property = TryGetProperty(name);
            if (property == null)
            {
                throw new ModelItemNotFoundException(Strings.FormatPropertyNotFound(name, Name));
            }
            return property;
        }

        public virtual void RemoveProperty([NotNull] Property property)
        {
            Check.NotNull(property, "property");

            var currentIndex = _properties.BinarySearch(property, PropertyComparer.Instance);
            if (currentIndex >= 0
                && _properties[currentIndex] == property)
            {
                if (Keys.Any(k => k.Properties.Contains(property))
                    || ForeignKeys.Any(k => k.Properties.Contains(property))
                    || Indexes.Any(i => i.Properties.Contains(property)))
                {
                    throw new InvalidOperationException(Strings.FormatPropertyInUse(property.Name, Name));
                }

                _properties.RemoveAt(currentIndex);
                UpdateIndexes(property, currentIndex);
            }
            else
            {
                throw new ModelItemNotFoundException(Strings.FormatPropertyNotFound(property.Name, Name));
            }
        }

        public virtual IReadOnlyList<Property> Properties
        {
            get { return _properties; }
        }

        public virtual int ShadowPropertyCount
        {
            get { return _shadowPropertyCount; }
        }

        public virtual int OriginalValueCount
        {
            get { return _originalValueCount; }
        }

        public virtual bool HasClrType
        {
            get { return Type != null; }
        }

        public virtual bool UseLazyOriginalValues
        {
            get { return _useLazyOriginalValues; }
            set
            {
                if (value && !CanUseLazyOriginalValues())
                {
                    throw new InvalidOperationException(Strings.FormatEagerOriginalValuesRequired(Name));
                }

                _useLazyOriginalValues = value;
                UpdateOriginalValueIndexes();
            }
        }

        private bool CanUseLazyOriginalValues()
        {
            return Type == null
                   || (typeof(INotifyPropertyChanging).GetTypeInfo().IsAssignableFrom(Type.GetTypeInfo())
                       && typeof(INotifyPropertyChanged).GetTypeInfo().IsAssignableFrom(Type.GetTypeInfo()));
        }

        #endregion

        #region Explicit interface implementations

        IModel IEntityType.Model
        {
            get { return Model; }
        }

        IKey IEntityType.GetPrimaryKey()
        {
            return GetPrimaryKey();
        }

        IProperty IEntityType.TryGetProperty(string name)
        {
            return TryGetProperty(name);
        }

        IProperty IEntityType.GetProperty(string name)
        {
            return GetProperty(name);
        }

        INavigation IEntityType.TryGetNavigation(string name)
        {
            return TryGetNavigation(name);
        }

        INavigation IEntityType.GetNavigation(string name)
        {
            return GetNavigation(name);
        }

        IReadOnlyList<IProperty> IEntityType.Properties
        {
            get { return Properties; }
        }

        IReadOnlyList<IForeignKey> IEntityType.ForeignKeys
        {
            get { return ForeignKeys; }
        }

        IReadOnlyList<INavigation> IEntityType.Navigations
        {
            get { return Navigations; }
        }

        IReadOnlyList<IIndex> IEntityType.Indexes
        {
            get { return Indexes; }
        }

        IReadOnlyList<IKey> IEntityType.Keys
        {
            get { return Keys; }
        }

        #endregion

        private string FormatProperties(IEnumerable<Property> properties)
        {
            return string.Join(", ", properties.Select(p => p.Name));
        }

        private class PropertyComparer : IComparer<Property>
        {
            public static readonly PropertyComparer Instance = new PropertyComparer();

            private PropertyComparer()
            {
            }

            public int Compare(Property x, Property y)
            {
                return StringComparer.Ordinal.Compare(x.Name, y.Name);
            }
        }
    }
}
