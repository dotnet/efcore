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

        public virtual Type Type => _typeOrName as Type;

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
                        || _keys.Value.Any())
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

        public virtual bool HasDerivedTypes => GetDerivedTypes().Any();

        public virtual IEnumerable<EntityType> GetDerivedTypes()
        {
            return GetDerivedTypes(Model, this);
        }

        public virtual IEnumerable<EntityType> GetConcreteTypesInHierarchy()
        {
            return new[] { this }
                .Concat(GetDerivedTypes())
                .Where(et => !et.IsAbstract);
        }

        private static IEnumerable<EntityType> GetDerivedTypes(Model model, EntityType entityType)
        {
            foreach (var et1 in model.EntityTypes
                .Where(et1 => et1.BaseType == entityType))
            {
                yield return et1;

                foreach (var et2 in GetDerivedTypes(model, et1))
                {
                    yield return et2;
                }
            }
        }

        public virtual bool IsAbstract => Type?.GetTypeInfo().IsAbstract ?? false;

        public virtual EntityType RootType => BaseType?.RootType ?? this;

        public virtual string Name => Type?.FullName ?? (string)_typeOrName;

        public virtual string SimpleName => Type?.Name ?? ParseSimpleName();

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
                && (primaryKey = TryGetPrimaryKey(properties)) != null)
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
        public virtual Key TryGetPrimaryKey()
        {
            return BaseType?.TryGetPrimaryKey() ?? _primaryKey;
        }

        [CanBeNull]
        public virtual Key TryGetPrimaryKey([CanBeNull] IReadOnlyList<Property> properties)
        {
            Check.NotNull(properties, nameof(properties));

            if (BaseType != null)
            {
                return BaseType.TryGetPrimaryKey(properties);
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
            Check.NotEmpty(properties, nameof(properties));

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
            Check.NotNull(key, nameof(key));
            ThrowIfDerivedEntity();

            Key removedKey;
            if (_keys.HasValue
                && _keys.Value.TryGetValue(key.Properties, out removedKey))
            {
                CheckKeyNotInUse(removedKey);

                if (_primaryKey == removedKey)
                {
                    SetPrimaryKey((IReadOnlyList<Property>)null);
                }
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
                if (BaseType != null)
                {
                    return BaseType.Keys;
                }

                return _keys.Value.Values.ToList();
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
            [NotNull] Key referencedKey,
            [CanBeNull] EntityType referencedEntityType = null)
        {
            return AddForeignKey(new[] { property }, referencedKey, referencedEntityType);
        }

        public virtual ForeignKey AddForeignKey(
            [NotNull] IReadOnlyList<Property> properties,
            [NotNull] Key referencedKey,
            [CanBeNull] EntityType referencedEntityType = null)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.NotNull(referencedKey, nameof(referencedKey));

            if (_foreignKeys.Value.ContainsKey(properties))
            {
                throw new InvalidOperationException(Strings.DuplicateForeignKey(Property.Format(properties), Name));
            }

            var foreignKey = new ForeignKey(properties, referencedKey, referencedEntityType);

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
            Check.NotEmpty(properties, nameof(properties));

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
            Check.NotNull(principalType, nameof(principalType));

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
            Check.NotNull(foreignKey, nameof(foreignKey));

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
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(foreignKey, nameof(foreignKey));

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
            Check.NotEmpty(name, nameof(name));

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
            Check.NotNull(navigation, nameof(navigation));

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
            Check.NotEmpty(properties, nameof(properties));

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
            Check.NotEmpty(properties, nameof(properties));

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
            Check.NotNull(index, nameof(index));

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
            return TryGetProperty(name) ?? AddProperty(name, propertyType, shadowProperty);
        }

        [CanBeNull]
        public virtual Property TryGetProperty([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return TryGetProperty(propertyInfo.Name);
        }

        [CanBeNull]
        public virtual Property TryGetProperty([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, nameof(propertyName));

            Property property;

            return _properties.TryGetValue(propertyName, out property)
                ? property
                : BaseType?.TryGetProperty(propertyName);
        }

        public virtual Property GetProperty([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return GetProperty(propertyInfo.Name);
        }

        public virtual Property GetProperty([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, nameof(propertyName));

            var property = TryGetProperty(propertyName);

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
            var shadowIndex = BaseType?.ShadowPropertyCount ?? 0;

            foreach (var property in _properties.Values.Where(p => p.IsShadowProperty))
            {
                property.ShadowIndex = shadowIndex++;
            }

            _shadowPropertyCount = shadowIndex;
        }

        private void UpdateOriginalValueIndexes()
        {
            var originalValueIndex = BaseType?.OriginalValueCount() ?? 0;

            foreach (var property in _properties.Values)
            {
                property.OriginalValueIndex
                    = RequiresOriginalValue(property) ? originalValueIndex++ : -1;
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

        IEntityType IEntityType.RootType => RootType;

        IModel IEntityType.Model => Model;

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

        IEnumerable<IEntityType> IEntityType.GetDerivedTypes()
        {
            return GetDerivedTypes();
        }

        IEnumerable<IEntityType> IEntityType.GetConcreteTypesInHierarchy()
        {
            return GetConcreteTypesInHierarchy();
        }

        IEnumerable<IProperty> IEntityType.Properties => Properties;

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
