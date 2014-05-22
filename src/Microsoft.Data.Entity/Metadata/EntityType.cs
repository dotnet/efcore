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
    public class EntityType : NamedMetadataBase, IEntityType
    {
        private readonly Type _type;
        private readonly LazyRef<List<ForeignKey>> _foreignKeys = new LazyRef<List<ForeignKey>>(() => new List<ForeignKey>());
        private readonly LazyRef<List<Navigation>> _navigations = new LazyRef<List<Navigation>>(() => new List<Navigation>());
        private readonly List<Property> _properties = new List<Property>();

        private Key _key;

        public Func<object[], object> _activator;
        private int _shadowPropertyCount;
        private int _originalValueCount;
        private bool _useLazyOriginalValues = true;

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
            : this(Check.NotNull(type, "type").Name)
        {
            _type = type;
            _useLazyOriginalValues = CanUseLazyOriginalValues();
        }

        /// <summary>
        ///     Creates a new metadata object representing an entity type that will participate in shadow-state
        ///     such that there is no underlying .NET type corresponding to this metadata object.
        /// </summary>
        /// <param name="name">The name of the shadow-state entity type.</param>
        public EntityType([NotNull] string name)
            : base(Check.NotEmpty(name, "name"))
        {
        }

        public virtual Type Type
        {
            get { return _type; }
        }

        public virtual Key GetKey()
        {
            if (_key == null)
            {
                throw new ModelItemNotFoundException(Strings.FormatEntityRequiresKey(Name));
            }

            return _key;
        }

        public virtual Key TryGetKey()
        {
            return _key;
        }

        public virtual void SetKey([CanBeNull] params Property[] properties)
        {
            _key = null;

            if (properties != null)
            {
                Check.NotEmpty(properties, "properties");

                var key = new Key(properties);

                if (key.EntityType != this)
                {
                    throw new ArgumentException(
                        Strings.FormatKeyPropertiesWrongEntity(Name));
                }

                _key = key;
            }
        }

        public virtual ForeignKey AddForeignKey(
            [NotNull] Key referencedKey, [NotNull] params Property[] properties)
        {
            Check.NotNull(referencedKey, "referencedKey");
            Check.NotNull(properties, "properties");
            Check.NotEmpty(properties, "properties");

            var foreignKey = new ForeignKey(referencedKey, properties);

            if (foreignKey.EntityType != this)
            {
                throw new ArgumentException(
                    Strings.FormatForeignKeyPropertiesWrongEntity(Name));
            }

            _foreignKeys.Value.Add(foreignKey);

            UpdateOriginalValueIndexes();

            return foreignKey;
        }

        public virtual void RemoveForeignKey([NotNull] ForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            if (_foreignKeys.HasValue)
            {
                _foreignKeys.Value.Remove(foreignKey);
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

        public virtual Navigation AddNavigation([NotNull] Navigation navigation)
        {
            Check.NotNull(navigation, "navigation");

            _navigations.Value.Add(navigation);

            var currentOwner = navigation.EntityType;
            if (currentOwner != null
                && !ReferenceEquals(currentOwner, this))
            {
                currentOwner.RemoveNavigation(navigation);
            }
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

        public virtual IReadOnlyList<Navigation> Navigations
        {
            get
            {
                return _navigations.HasValue
                    ? (IReadOnlyList<Navigation>)_navigations.Value
                    : ImmutableList<Navigation>.Empty;
            }
        }

        public virtual Property AddProperty([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, "propertyInfo");

            return AddProperty(propertyInfo.Name, propertyInfo.PropertyType, shadowProperty: false, concurrencyToken: false);
        }

        public virtual Property AddProperty([NotNull] string name, [NotNull] Type propertyType)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(propertyType, "propertyType");

            var property = new Property(name, propertyType, shadowProperty: false, concurrencyToken: false);

            AddProperty(property);

            return property;
        }

        public virtual Property AddProperty([NotNull] string name, [NotNull] Type propertyType, bool shadowProperty, bool concurrencyToken)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(propertyType, "propertyType");

            var property = new Property(name, propertyType, shadowProperty, concurrencyToken);

            AddProperty(property);

            return property;
        }

        private void AddProperty([NotNull] Property property)
        {
            Check.NotNull(property, "property");

            // TODO: Consider if replace as opposed to throw/no-op is correct when prop with this name exists

            var currentIndex = _properties.BinarySearch(property, PropertyComparer.Instance);

            if (currentIndex >= 0)
            {
                var currentProperty = _properties[currentIndex];

                if (!ReferenceEquals(currentProperty, property))
                {
                    _properties[currentIndex] = property;

                    property.Index = currentIndex;

                    UpdateShadowIndexes(property);
                    UpdateOriginalValueIndexes(property);
                }
            }
            else
            {
                var newIndex = ~currentIndex;

                _properties.Insert(newIndex, property);

                UpdateIndexes(property, newIndex);
            }

            // TODO: Remove this
            var currentOwner = property.EntityType;
            if (currentOwner != null
                && !ReferenceEquals(currentOwner, this))
            {
                currentOwner.RemoveProperty(property);
            }

            property.EntityType = this;
        }

        private void UpdateIndexes(Property addedOrRemovedProperty, int startingIndex)
        {
            for (var i = startingIndex; i < _properties.Count; i++)
            {
                _properties[i].Index = i;
            }
            UpdateShadowIndexes(addedOrRemovedProperty);
            UpdateOriginalValueIndexes(addedOrRemovedProperty);
        }

        private void UpdateShadowIndexes(Property addedOrRemovedProperty)
        {
            if (!addedOrRemovedProperty.IsClrProperty)
            {
                var shadowIndex = 0;
                foreach (var property in _properties.Where(p => !p.IsClrProperty))
                {
                    property.ShadowIndex = shadowIndex++;
                }
                _shadowPropertyCount = shadowIndex;
            }
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
                _properties.RemoveAt(currentIndex);
                UpdateIndexes(property, currentIndex);
                property.EntityType = null;
            }

            // TODO: Consider if it is okay to take properties out of the key, which may not be empty
            // TODO: Consider what to do with FKs that contain this property
            if (_key != null
                && _key.Properties.Contains(property))
            {
                _key = new Key(_key.Properties.Except(new[] { property }).ToArray())
                    {
                        StorageName = _key.StorageName
                    };
            }
        }

        [CanBeNull]
        public virtual Property TryGetProperty([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            // TODO: This should be O(log(n)) but an additional index could be created
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
            get { return _type != null; }
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
            return _type == null
                   || (typeof(INotifyPropertyChanging).GetTypeInfo().IsAssignableFrom(_type.GetTypeInfo())
                       && typeof(INotifyPropertyChanged).GetTypeInfo().IsAssignableFrom(_type.GetTypeInfo()));
        }

        public virtual IReadOnlyList<Property> Properties
        {
            get { return _properties; }
        }

        IKey IEntityType.GetKey()
        {
            return GetKey();
        }

        IProperty IEntityType.TryGetProperty(string name)
        {
            return TryGetProperty(name);
        }

        IProperty IEntityType.GetProperty(string name)
        {
            return GetProperty(name);
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
