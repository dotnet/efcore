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

        public virtual Key GetPrimaryKey()
        {
            if (_primaryKey == null)
            {
                throw new ModelItemNotFoundException(Strings.FormatEntityRequiresKey(Name));
            }

            return _primaryKey;
        }

        public virtual Key TryGetPrimaryKey()
        {
            return _primaryKey;
        }

        public virtual Key SetPrimaryKey([CanBeNull] Key key)
        {
            if (key != _primaryKey)
            {
                if (key != null
                    && key.EntityType != this)
                {
                    throw new ArgumentException(
                        Strings.FormatKeyPropertiesWrongEntity(Name));
                }

                if (_primaryKey != null)
                {
                    CheckKeyNotInUse(_primaryKey);
                }

                _primaryKey = key;
            }

            return _primaryKey;
        }

        public virtual Key GetOrSetPrimaryKey([CanBeNull] params Property[] properties)
        {
            if (properties != null)
            {
                if (_primaryKey == null
                    || !_primaryKey.Properties.SequenceEqual(properties))
                {
                    SetPrimaryKey(new Key(properties));
                }
            }
            else
            {
                SetPrimaryKey(null);
            }

            return _primaryKey;
        }

        public virtual Key AddKey([NotNull] Key key)
        {
            Check.NotNull(key, "key");

            if (!Keys.Contains(key))
            {
                if (key.EntityType != this)
                {
                    throw new ArgumentException(
                        Strings.FormatKeyPropertiesWrongEntity(Name));
                }

                _keys.Value.Add(key);
            }

            return key;
        }

        public virtual Key GetOrAddKey([NotNull] params Property[] properties)
        {
            Check.NotEmpty(properties, "properties");

            return Keys.FirstOrDefault(k => k.Properties.SequenceEqual(properties))
                   ?? AddKey(new Key(properties));
        }

        public virtual void RemoveKey([NotNull] Key key)
        {
            Check.NotNull(key, "key");

            if (key == _primaryKey)
            {
                SetPrimaryKey(null);
            }
            else if (_keys.HasValue)
            {
                CheckKeyNotInUse(key);

                _keys.Value.Remove(key);
            }
        }

        private void CheckKeyNotInUse(Key key)
        {
            if (Model != null)
            {
                var foreignKey = Model.EntityTypes.SelectMany(e => e.ForeignKeys).FirstOrDefault(k => k.ReferencedKey == key);

                if (foreignKey != null)
                {
                    throw new InvalidOperationException(Strings.FormatKeyInUse(Name, foreignKey.EntityType.Name));
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

        public virtual ForeignKey AddForeignKey([NotNull] ForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            if (foreignKey.EntityType != this)
            {
                throw new ArgumentException(
                    Strings.FormatForeignKeyPropertiesWrongEntity(Name));
            }

            if (!_foreignKeys.Value.Contains(foreignKey))
            {
                _foreignKeys.Value.Add(foreignKey);

                UpdateOriginalValueIndexes();
            }

            return foreignKey;
        }

        public virtual ForeignKey GetOrAddForeignKey(
            [NotNull] Key referencedKey, [NotNull] params Property[] properties)
        {
            Check.NotNull(referencedKey, "referencedKey");
            Check.NotEmpty(properties, "properties");

            var foreignKey = _foreignKeys.Value.FirstOrDefault(k => k.ReferencedKey == referencedKey
                                                                    && k.Properties.SequenceEqual(properties))
                             ?? AddForeignKey(new ForeignKey(referencedKey, properties));

            return foreignKey;
        }

        public virtual void RemoveForeignKey([NotNull] ForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            if (_foreignKeys.HasValue)
            {
                CheckForeignKeyNotInUse(foreignKey);

                _foreignKeys.Value.Remove(foreignKey);
            }
        }

        private void CheckForeignKeyNotInUse(ForeignKey foreignKey)
        {
            var navigation = (Model == null
                ? new[] { this }
                : Model.EntityTypes).SelectMany(e => e.Navigations).FirstOrDefault(k => k.ForeignKey == foreignKey);

            if (navigation != null)
            {
                throw new InvalidOperationException(Strings.FormatForeignKeyInUse(Name, navigation.Name, navigation.EntityType.Name));
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

        public virtual Navigation GetOrAddNavigation([NotNull] string name, [NotNull] ForeignKey foreignKey, bool pointsToPrincipal)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(foreignKey, "foreignKey");

            return _navigations.Value.FirstOrDefault(n => n.Name == name)
                   ?? AddNavigation(new Navigation(foreignKey, name, pointsToPrincipal));
        }

        public virtual Navigation AddNavigation([NotNull] Navigation navigation)
        {
            Check.NotNull(navigation, "navigation");

            if (navigation.EntityType != null
                && navigation.EntityType != this)
            {
                throw new InvalidOperationException(Strings.FormatNavigationAlreadyOwned(navigation.Name, Name, navigation.EntityType.Name));
            }

            if (_navigations.Value.Any(n => n.Name == navigation.Name))
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

            var targetClrType = navigation.GetTargetType().Type;

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

            navigation.EntityType = this;

            return navigation;
        }

        public virtual void RemoveNavigation([NotNull] Navigation navigation)
        {
            Check.NotNull(navigation, "navigation");

            if (_navigations.HasValue
                && _navigations.Value.Remove(navigation))
            {
                navigation.EntityType = null;
            }
        }

        public virtual Navigation TryGetNavigation([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            return Navigations.FirstOrDefault(n => n.Name == name);
        }

        public virtual Navigation GetNavigation([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            var navigation = TryGetNavigation(name);
            if (navigation == null)
            {
                throw new ModelItemNotFoundException(Strings.FormatNavigationNotFound(name, Name));
            }
            return navigation;
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

        public virtual Index GetOrAddIndex([NotNull] params Property[] properties)
        {
            Check.NotNull(properties, "properties");
            Check.NotEmpty(properties, "properties");

            return _indexes.Value.FirstOrDefault(i => i.Properties.SequenceEqual(properties))
                   ?? AddIndex(new Index(properties));
        }

        public virtual Index AddIndex([NotNull] Index index)
        {
            Check.NotNull(index, "index");

            if (index.EntityType != this)
            {
                throw new ArgumentException(
                    Strings.FormatIndexPropertiesWrongEntity(Name));
            }

            _indexes.Value.Add(index);

            return index;
        }

        public virtual void RemoveIndex([NotNull] Index index)
        {
            Check.NotNull(index, "index");

            if (_indexes.HasValue)
            {
                _indexes.Value.Remove(index);
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

        public virtual Property GetOrAddProperty([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, "propertyInfo");

            return GetOrAddProperty(propertyInfo.Name, propertyInfo.PropertyType);
        }

        public virtual Property GetOrAddProperty([NotNull] string name, [NotNull] Type propertyType, bool shadowProperty = false)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(propertyType, "propertyType");

            // Note: If the property already exists, then whether or not it is a shadow property is not changed.
            // It is useful in many places to get an existing property if it exists, but then create it either in
            // or out of shadow state if it doesn't.

            return _properties.FirstOrDefault(p => p.Name == name)
                   ?? AddProperty(new Property(name, propertyType, shadowProperty));
        }

        public virtual Property AddProperty([NotNull] Property property)
        {
            Check.NotNull(property, "property");

            if (property.EntityType != null
                && property.EntityType != this)
            {
                throw new InvalidOperationException(Strings.FormatPropertyAlreadyOwned(property.Name, Name, property.EntityType.Name));
            }

            var currentIndex = _properties.BinarySearch(property, PropertyComparer.Instance);
            if (currentIndex >= 0)
            {
                throw new InvalidOperationException(Strings.FormatDuplicateProperty(property.Name, Name));
            }

            ValidateAgainstClrProperty(property);

            property.EntityType = this;

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

        public virtual void RemoveProperty([NotNull] Property property)
        {
            Check.NotNull(property, "property");

            var currentIndex = _properties.BinarySearch(property, PropertyComparer.Instance);
            if (currentIndex >= 0)
            {
                if (Keys.Any(k => k.Properties.Contains(property))
                    || ForeignKeys.Any(k => k.Properties.Contains(property))
                    || Indexes.Any(i => i.Properties.Contains(property)))
                {
                    throw new InvalidOperationException(Strings.FormatPropertyInUse(property.Name, Name));
                }

                _properties.RemoveAt(currentIndex);
                UpdateIndexes(property, currentIndex);

                property.EntityType = null;
            }
        }

        [CanBeNull]
        public virtual Property TryGetProperty([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            // TODO: Perf: This should be O(log(n)) but an additional index could be created
            // TODO: if this is too slow or if creating the surrogate Property object is too expensive
            var surrogate = new Property(name, typeof(object));
            var index = _properties.BinarySearch(surrogate, PropertyComparer.Instance);
            return index >= 0 ? _properties[index] : null;
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

        public virtual IReadOnlyList<Property> Properties
        {
            get { return _properties; }
        }

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
