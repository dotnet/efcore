// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class EntityType : Annotatable, IEntityType
    {
        private static readonly char[] _simpleNameChars = { '.', '+' };

        private readonly SortedDictionary<IReadOnlyList<Property>, ForeignKey> _foreignKeys
            = new SortedDictionary<IReadOnlyList<Property>, ForeignKey>(PropertyListComparer.Instance);

        private readonly SortedDictionary<string, Navigation> _navigations
            = new SortedDictionary<string, Navigation>(StringComparer.Ordinal);

        private readonly SortedDictionary<IReadOnlyList<Property>, Index> _indexes
            = new SortedDictionary<IReadOnlyList<Property>, Index>(PropertyListComparer.Instance);

        private readonly SortedDictionary<string, Property> _properties
            = new SortedDictionary<string, Property>(StringComparer.Ordinal);

        private readonly SortedDictionary<IReadOnlyList<Property>, Key> _keys
            = new SortedDictionary<IReadOnlyList<Property>, Key>(PropertyListComparer.Instance);

        private readonly object _typeOrName;

        private Key _primaryKey;
        private EntityType _baseType;

        public event EventHandler<Property> PropertyMetadataChanged;

        private int _shadowPropertyCount;

        private bool _useEagerSnapshots;

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
            : this(
                (object)Check.NotNull(type, nameof(type)),
                Check.NotNull(model, nameof(model)))
        {
            Check.ValidEntityType(type, nameof(type));

            _useEagerSnapshots = !this.HasPropertyChangingNotifications();
        }

        /// <summary>
        ///     Creates a new metadata object representing an entity type that will participate in shadow-state
        ///     such that there is no underlying .NET type corresponding to this metadata object.
        /// </summary>
        /// <param name="name">The name of the shadow-state entity type.</param>
        /// <param name="model">The model associated with this entity type.</param>
        public EntityType([NotNull] string name, [NotNull] Model model)
            : this(
                (object)Check.NotEmpty(name, nameof(name)),
                Check.NotNull(model, nameof(model)))
        {
        }

        private EntityType(object typeOrName, Model model)
        {
            _typeOrName = typeOrName;

            Model = model;
        }

        public virtual Type ClrType => _typeOrName as Type;

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

                if (value != null)
                {
                    if (value.InheritsFrom(this))
                    {
                        throw new InvalidOperationException(Strings.CircularInheritance(this, value));
                    }

                    if (_primaryKey != null
                        || _keys.Any())
                    {
                        throw new InvalidOperationException(Strings.DerivedEntityCannotHaveKeys(this));
                    }

                    ValidateNoNameCollision(value);

                    value.PropertyMetadataChanged += OnPropertyMetadataChanged;
                }
                else
                {
                    _baseType.PropertyMetadataChanged -= OnPropertyMetadataChanged;
                }

                _baseType = value;

                UpdateIndexes();
                UpdateShadowIndexes();
                UpdateOriginalValueIndexes();
            }
        }

        private void ValidateNoNameCollision(EntityType entityType)
        {
            foreach (var property in entityType.Properties
                .Where(property => _properties.ContainsKey(property.Name)))
            {
                throw new InvalidOperationException(Strings.DuplicateProperty(property.Name, Name));
            }
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

        public virtual bool IsAbstract => ClrType?.GetTypeInfo().IsAbstract ?? false;

        public virtual string Name => ClrType?.FullName ?? (string)_typeOrName;

        public virtual string DisplayName() => ClrType?.Name ?? ParseSimpleName();

        private string ParseSimpleName()
        {
            var fullName = (string)_typeOrName;
            var lastDot = fullName.LastIndexOfAny(_simpleNameChars);

            return lastDot > 0 ? fullName.Substring(lastDot + 1) : fullName;
        }

        public override string ToString()
        {
            return Name;
        }

        public virtual int ShadowPropertyCount => _shadowPropertyCount;

        public virtual int PropertyCount => (BaseType?.PropertyCount ?? 0) + _properties.Count;

        public virtual bool HasClrType => ClrType != null;

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

        #region Primary and Candidate Keys

        [ContractAnnotation("null => null; notnull => notnull")]
        public virtual Key SetPrimaryKey([CanBeNull] Property property)
        {
            return SetPrimaryKey(property == null ? null : new[] { property });
        }

        [ContractAnnotation("null => null; notnull => notnull")]
        public virtual Key SetPrimaryKey([CanBeNull] IReadOnlyList<Property> properties)
        {
            ThrowIfDerivedEntity();

            Key key = null;
            if (properties != null
                && properties.Count != 0)
            {
                key = GetOrAddKey(properties);
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
            Key primaryKey;
            if (properties != null
                && (primaryKey = FindPrimaryKey(properties)) != null)
            {
                return primaryKey;
            }

            return SetPrimaryKey(properties);
        }

        public virtual Key GetPrimaryKey()
        {
            if (BaseType != null)
            {
                return BaseType.GetPrimaryKey();
            }

            if (_primaryKey == null)
            {
                throw new ModelItemNotFoundException(Strings.EntityRequiresKey(Name));
            }

            return _primaryKey;
        }

        [CanBeNull]
        public virtual Key FindPrimaryKey()
        {
            return BaseType?.FindPrimaryKey() ?? _primaryKey;
        }

        [CanBeNull]
        public virtual Key FindPrimaryKey([CanBeNull] IReadOnlyList<Property> properties)
        {
            Check.NotNull(properties, nameof(properties));

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
        {
            return AddKey(new[] { property });
        }

        public virtual Key AddKey([NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(properties, nameof(properties));
            ThrowIfDerivedEntity();

            var key = FindKey(properties);
            if (key != null)
            {
                throw new InvalidOperationException(Strings.DuplicateKey(Property.Format(properties), Name));
            }

            key = new Key(properties);
            if (key.EntityType != this)
            {
                throw new ArgumentException(Strings.KeyPropertiesWrongEntity(Property.Format(properties), Name));
            }

            _keys.Add(properties, key);

            return key;
        }

        public virtual Key GetOrAddKey([NotNull] Property property)
        {
            return GetOrAddKey(new[] { property });
        }

        public virtual Key GetOrAddKey([NotNull] IReadOnlyList<Property> properties)
        {
            return FindKey(properties)
                   ?? AddKey(properties);
        }

        [CanBeNull]
        public virtual Key FindKey([NotNull] Property property)
        {
            return FindKey(new[] { property });
        }

        [CanBeNull]
        public virtual Key FindKey([NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(properties, nameof(properties));

            var key = FindPrimaryKey(properties);
            if (key != null)
            {
                return key;
            }

            if (_keys.TryGetValue(properties, out key))
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
            var key = FindKey(properties);
            if (key == null)
            {
                throw new ModelItemNotFoundException(Strings.KeyNotFound(Property.Format(properties), Name));
            }

            return key;
        }

        public virtual Key RemoveKey([NotNull] Key key)
        {
            Check.NotNull(key, nameof(key));
            ThrowIfDerivedEntity();

            Key removedKey;
            if (_keys.TryGetValue(key.Properties, out removedKey))
            {
                CheckKeyNotInUse(removedKey);

                if (_primaryKey == removedKey)
                {
                    SetPrimaryKey((IReadOnlyList<Property>)null);
                }
                _keys.Remove(key.Properties);
                return removedKey;
            }

            return null;
        }

        private void CheckKeyNotInUse(Key key)
        {
            var foreignKey = Model?.EntityTypes.SelectMany(e => e.ForeignKeys).FirstOrDefault(k => k.PrincipalKey == key);

            if (foreignKey != null)
            {
                throw new InvalidOperationException(Strings.KeyInUse(Property.Format(key.Properties), Name, foreignKey.EntityType.Name));
            }
        }

        public virtual IReadOnlyList<Key> Keys
        {
            get
            {
                if (BaseType != null)
                {
                    return BaseType.Keys;
                }

                return _keys.Values.ToList();
            }
        }

        private void ThrowIfDerivedEntity([CallerMemberName] string caller = null)
        {
            if (BaseType != null)
            {
                throw new InvalidOperationException(Strings.InvalidForDerivedEntity(caller, Name));
            }
        }

        #endregion

        #region Foreign Keys

        public virtual ForeignKey AddForeignKey(
            [NotNull] Property property,
            [NotNull] Key principalKey,
            [CanBeNull] EntityType principalEntityType = null)
        {
            return AddForeignKey(new[] { property }, principalKey, principalEntityType);
        }

        public virtual ForeignKey AddForeignKey(
            [NotNull] IReadOnlyList<Property> properties,
            [NotNull] Key principalKey,
            [CanBeNull] EntityType principalEntityType = null)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.NotNull(principalKey, nameof(principalKey));

            if (_foreignKeys.ContainsKey(properties))
            {
                throw new InvalidOperationException(Strings.DuplicateForeignKey(Property.Format(properties), Name));
            }

            var foreignKey = new ForeignKey(properties, principalKey, principalEntityType);

            if (foreignKey.EntityType != this)
            {
                throw new ArgumentException(Strings.ForeignKeyPropertiesWrongEntity(Property.Format(properties), Name));
            }

            _foreignKeys.Add(properties, foreignKey);

            UpdateOriginalValueIndexes();

            return foreignKey;
        }

        public virtual ForeignKey GetOrAddForeignKey(
            [NotNull] Property property, [NotNull] Key principalKey)
        {
            return GetOrAddForeignKey(new[] { property }, principalKey);
        }

        public virtual ForeignKey GetOrAddForeignKey(
            [NotNull] IReadOnlyList<Property> properties, [NotNull] Key principalKey)
        {
            // Note: this will return an existing foreign key even if it doesn't have the same referenced key
            return FindForeignKey(properties)
                   ?? AddForeignKey(properties, principalKey);
        }

        [CanBeNull]
        public virtual ForeignKey FindForeignKey([NotNull] Property property)
        {
            return FindForeignKey(new[] { property });
        }

        [CanBeNull]
        public virtual ForeignKey FindForeignKey([NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(properties, nameof(properties));

            ForeignKey foreignKey;
            if (_foreignKeys.TryGetValue(properties, out foreignKey))
            {
                return foreignKey;
            }

            return null;
        }

        [CanBeNull]
        public virtual ForeignKey FindForeignKey(
            [NotNull] EntityType principalType,
            [CanBeNull] string navigationToPrincipal,
            [CanBeNull] string navigationToDependent,
            [CanBeNull] IReadOnlyList<Property> foreignKeyProperties,
            [CanBeNull] IReadOnlyList<Property> principalProperties,
            bool? isUnique)
        {
            Check.NotNull(principalType, nameof(principalType));

            return ForeignKeys.FirstOrDefault(fk =>
                fk.IsCompatible(
                    principalType,
                    this,
                    navigationToPrincipal,
                    navigationToDependent,
                    foreignKeyProperties,
                    principalProperties,
                    isUnique));
        }

        public virtual ForeignKey GetForeignKey([NotNull] Property property)
        {
            return GetForeignKey(new[] { property });
        }

        public virtual ForeignKey GetForeignKey([NotNull] IReadOnlyList<Property> properties)
        {
            var foreignKey = FindForeignKey(properties);
            if (foreignKey == null)
            {
                throw new ModelItemNotFoundException(Strings.ForeignKeyNotFound(Property.Format(properties), Name));
            }

            return foreignKey;
        }

        public virtual ForeignKey RemoveForeignKey([NotNull] ForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            ForeignKey removedFk;
            if (_foreignKeys.TryGetValue(foreignKey.Properties, out removedFk))
            {
                CheckForeignKeyNotInUse(removedFk);

                _foreignKeys.Remove(removedFk.Properties);
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

        public virtual IReadOnlyList<ForeignKey> ForeignKeys => _foreignKeys.Values.ToList();

        #endregion

        #region Navigations

        public virtual Navigation AddNavigation([NotNull] string name, [NotNull] ForeignKey foreignKey, bool pointsToPrincipal)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(foreignKey, nameof(foreignKey));

            if (_navigations.ContainsKey(name))
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

            var clrProperty = ClrType.GetPropertiesInHierarchy(navigation.Name).FirstOrDefault();
            if (clrProperty == null)
            {
                throw new InvalidOperationException(Strings.NoClrNavigation(navigation.Name, Name));
            }

            var targetType = navigation.GetTargetType();
            if (!targetType.HasClrType)
            {
                throw new InvalidOperationException(Strings.NavigationToShadowEntity(navigation.Name, Name, targetType.Name));
            }

            var targetClrType = targetType.ClrType;
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

            var otherNavigation = _navigations.Values.FirstOrDefault(
                n =>
                    n.ForeignKey == navigation.ForeignKey
                    && n.PointsToPrincipal == navigation.PointsToPrincipal);

            if (otherNavigation != null)
            {
                throw new InvalidOperationException(Strings.MultipleNavigations(navigation.Name, otherNavigation.Name, Name));
            }

            _navigations.Add(name, navigation);

            return navigation;
        }

        public virtual Navigation GetOrAddNavigation([NotNull] string name, [NotNull] ForeignKey foreignKey, bool pointsToPrincipal)
        {
            return FindNavigation(name) ?? AddNavigation(name, foreignKey, pointsToPrincipal);
        }

        [CanBeNull]
        public virtual Navigation FindNavigation([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            Navigation navigation;
            if (_navigations.TryGetValue(name, out navigation))
            {
                return navigation;
            }
            return null;
        }

        public virtual Navigation GetNavigation([NotNull] string name)
        {
            var navigation = FindNavigation(name);
            if (navigation == null)
            {
                throw new ModelItemNotFoundException(Strings.NavigationNotFound(name, Name));
            }
            return navigation;
        }

        public virtual Navigation RemoveNavigation([NotNull] Navigation navigation)
        {
            Check.NotNull(navigation, nameof(navigation));

            Navigation removedNavigation;
            if (_navigations.TryGetValue(navigation.Name, out removedNavigation))
            {
                _navigations.Remove(navigation.Name);
                return removedNavigation;
            }

            return null;
        }

        public virtual IReadOnlyList<Navigation> Navigations => _navigations.Values.ToList();

        #endregion

        #region Indexes

        public virtual Index AddIndex([NotNull] Property property)
        {
            return AddIndex(new[] { property });
        }

        public virtual Index AddIndex([NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(properties, nameof(properties));

            if (_indexes.ContainsKey(properties))
            {
                throw new InvalidOperationException(Strings.DuplicateIndex(Property.Format(properties), Name));
            }

            var index = new Index(properties);

            if (index.EntityType != this)
            {
                throw new ArgumentException(Strings.IndexPropertiesWrongEntity(Property.Format(properties), Name));
            }

            _indexes.Add(properties, index);

            return index;
        }

        public virtual Index GetOrAddIndex([NotNull] Property property)
        {
            return GetOrAddIndex(new[] { property });
        }

        public virtual Index GetOrAddIndex([NotNull] IReadOnlyList<Property> properties)
        {
            return FindIndex(properties) ?? AddIndex(properties);
        }

        [CanBeNull]
        public virtual Index FindIndex([NotNull] Property property)
        {
            return FindIndex(new[] { property });
        }

        [CanBeNull]
        public virtual Index FindIndex([NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotEmpty(properties, nameof(properties));

            Index index;
            if (_indexes.TryGetValue(properties, out index))
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
            var index = FindIndex(properties);
            if (index == null)
            {
                throw new ModelItemNotFoundException(Strings.IndexNotFound(Property.Format(properties), Name));
            }
            return index;
        }

        public virtual Index RemoveIndex([NotNull] Index index)
        {
            Check.NotNull(index, nameof(index));

            Index removedIndex;
            if (_indexes.TryGetValue(index.Properties, out removedIndex))
            {
                _indexes.Remove(index.Properties);
                return removedIndex;
            }

            return null;
        }

        public virtual IReadOnlyList<Index> Indexes => _indexes.Values.ToList();

        #endregion

        #region Properties

        [NotNull]
        public virtual Property AddProperty([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return AddProperty(propertyInfo.Name, propertyInfo.PropertyType);
        }

        [NotNull]
        public virtual Property AddProperty([NotNull] string name, [NotNull] Type propertyType, bool shadowProperty = false)
        {
            Check.NotNull(name, nameof(name));
            Check.NotNull(propertyType, nameof(propertyType));

            if (_properties.ContainsKey(name))
            {
                throw new InvalidOperationException(Strings.DuplicateProperty(name, Name));
            }

            var property = new Property(name, propertyType, this, shadowProperty);

            ValidateAgainstClrProperty(property);

            _properties.Add(name, property);

            OnPropertyMetadataChanged(this, property);

            return property;
        }

        public virtual Property GetOrAddProperty([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return GetOrAddProperty(propertyInfo.Name, propertyInfo.PropertyType);
        }

        public virtual Property GetOrAddProperty([NotNull] string name, [NotNull] Type propertyType, bool shadowProperty = false)
        {
            // Note: If the property already exists, then whether or not it is a shadow property is not changed.
            // It is useful in many places to get an existing property if it exists, but then create it either in
            // or out of shadow state if it doesn't.
            return FindProperty(name) ?? AddProperty(name, propertyType, shadowProperty);
        }

        [CanBeNull]
        public virtual Property FindProperty([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return FindProperty(propertyInfo.Name);
        }

        [CanBeNull]
        public virtual Property FindProperty([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, nameof(propertyName));

            Property property;

            return _properties.TryGetValue(propertyName, out property)
                ? property
                : BaseType?.FindProperty(propertyName);
        }

        public virtual Property GetProperty([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return GetProperty(propertyInfo.Name);
        }

        public virtual Property GetProperty([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, nameof(propertyName));

            var property = FindProperty(propertyName);

            if (property == null)
            {
                throw new ModelItemNotFoundException(Strings.PropertyNotFound(propertyName, Name));
            }

            return property;
        }

        public virtual Property RemoveProperty([NotNull] Property property)
        {
            Check.NotNull(property, nameof(property));

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

                OnPropertyMetadataChanged(this, property);

                return removedProperty;
            }

            return null;
        }

        public virtual IEnumerable<Property> Properties
            => BaseType != null
                ? BaseType.Properties.Concat(_properties.Values)
                : _properties.Values;

        private void ValidateAgainstClrProperty(IProperty property)
        {
            if (!property.IsShadowProperty)
            {
                if (HasClrType)
                {
                    var clrProperty = ClrType.GetPropertiesInHierarchy(property.Name).FirstOrDefault();

                    if (clrProperty == null)
                    {
                        throw new InvalidOperationException(Strings.NoClrProperty(property.Name, Name));
                    }

                    if (property.ClrType != clrProperty.PropertyType)
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

        internal void OnPropertyMetadataChanged(object sender, Property property)
        {
            ValidateAgainstClrProperty(property);

            if (BaseType != null)
            {
                ValidateNoNameCollision(BaseType);
            }

            UpdateIndexes();
            UpdateShadowIndexes();
            UpdateOriginalValueIndexes();

            PropertyMetadataChanged?.Invoke(this, property);
        }

        private void UpdateIndexes()
        {
            var index = BaseType?.PropertyCount ?? 0;

            foreach (var property in _properties.Values)
            {
                property.Index = index++;
            }
        }

        private void UpdateShadowIndexes()
        {
            var shadowIndex = BaseType?.ShadowPropertyCount() ?? 0;

            foreach (var property in _properties.Values.Where(p => p.IsShadowProperty))
            {
                property.SetShadowIndex(shadowIndex++);
            }

            _shadowPropertyCount = shadowIndex;
        }

        private void UpdateOriginalValueIndexes()
        {
            var originalValueIndex = BaseType?.OriginalValueCount() ?? 0;

            foreach (var property in _properties.Values)
            {
                property.SetOriginalValueIndex(
                    RequiresOriginalValue(property) ? originalValueIndex++ : -1);
            }
        }

        private bool RequiresOriginalValue(Property addedOrRemovedProperty)
        {
            return _useEagerSnapshots
                   || ((IProperty)addedOrRemovedProperty).IsConcurrencyToken
                   || ForeignKeys.SelectMany(k => k.Properties).Contains(addedOrRemovedProperty);
        }

        #endregion

        #region Explicit interface implementations

        IEntityType IEntityType.BaseType => BaseType;

        IModel IEntityType.Model => Model;

        IKey IEntityType.GetPrimaryKey()
        {
            return GetPrimaryKey();
        }

        IProperty IEntityType.FindProperty(string propertyName)
        {
            return FindProperty(propertyName);
        }

        IProperty IEntityType.GetProperty(string propertyName)
        {
            return GetProperty(propertyName);
        }

        INavigation IEntityType.FindNavigation(string name)
        {
            return FindNavigation(name);
        }

        INavigation IEntityType.GetNavigation(string name)
        {
            return GetNavigation(name);
        }

        IEnumerable<IProperty> IEntityType.GetProperties() => Properties;

        IEnumerable<IForeignKey> IEntityType.GetForeignKeys() => ForeignKeys;

        IEnumerable<INavigation> IEntityType.GetNavigations() => Navigations;

        IEnumerable<IIndex> IEntityType.GetIndexes() => Indexes;

        IEnumerable<IKey> IEntityType.GetKeys() => Keys;

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
