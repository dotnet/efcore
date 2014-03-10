// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    [DebuggerDisplay("{Name},nq")]
    public class EntityType : MetadataBase, IEntityType
    {
        private readonly string _name;
        private readonly Type _type;
        private readonly LazyRef<List<ForeignKey>> _foreignKeys = new LazyRef<List<ForeignKey>>(() => new List<ForeignKey>());
        private readonly LazyRef<List<Navigation>> _navigations = new LazyRef<List<Navigation>>(() => new List<Navigation>());
        private readonly List<Property> _properties = new List<Property>();

        private IReadOnlyList<Property> _keyProperties;
        private string _storageName;
        public Func<object[], object> _activator;
        private int _shadowPropertyCount;

        /// <summary>
        /// This constructor is intended only for use when creating test doubles that will override members
        /// with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        /// behavior including but not limited to throwing <see cref="NullReferenceException"/>.
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
        }

        /// <summary>
        ///     Creates a new metadata object representing an entity type that will participate in shadow-state
        ///     such that there is no underlying .NET type corresponding to this metadata object.
        /// </summary>
        /// <param name="name">The name of the shadow-state entity type.</param>
        public EntityType([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            _name = name;
        }

        public virtual string Name
        {
            get { return _name; }
        }

        public virtual string StorageName
        {
            get { return _storageName ?? _name; }
            [param: NotNull]
            set
            {
                Check.NotEmpty(value, "value");

                _storageName = value;
            }
        }

        public virtual Type Type
        {
            get { return _type; }
        }

        public object CreateInstance(object[] values)
        {
            Check.NotNull(values, "values");

            if (_activator == null)
            {
                var ctor = Type.GetDeclaredConstructor(new[] { typeof(object[]) });

                if (ctor == null)
                {
                    // TODO: Fallback to slow path
                    throw new InvalidOperationException();
                }

                _activator = vs => ctor.Invoke(new object[] { vs });
            }

            return _activator(values);
        }

        public virtual IReadOnlyList<Property> Key
        {
            get { return _keyProperties ?? ImmutableList<Property>.Empty; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _keyProperties = value;
                foreach (var property in value)
                {
                    // TODO: Consider if this should be replace/throw/no-op when prop with this name exists
                    AddProperty(property);
                }
            }
        }

        public virtual ForeignKey AddForeignKey([NotNull] ForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            // TODO: Consider ordering of FKs
            _foreignKeys.Value.Add(foreignKey);
            foreach (var property in foreignKey.DependentProperties)
            {
                // TODO: Consider if this should be replace/throw/no-op when prop with this name exists
                AddProperty(property);
            }

            var currentOwner = foreignKey.DependentType;
            if (currentOwner != null
                && !ReferenceEquals(currentOwner, this))
            {
                currentOwner.RemoveForeignKey(foreignKey);
            }
            foreignKey.DependentType = this;

            return foreignKey;
        }

        public virtual void RemoveForeignKey([NotNull] ForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            if (_foreignKeys.HasValue
                && _foreignKeys.Value.Remove(foreignKey))
            {
                foreignKey.DependentType = null;

                // TODO: Consider--should the property also be removed?
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

        public virtual Property AddProperty([NotNull] Property property)
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
                }
            }
            else
            {
                var newIndex = ~currentIndex;
                _properties.Insert(newIndex, property);
                UpdateIndexes(property, newIndex);
            }

            var currentOwner = property.EntityType;
            if (currentOwner != null
                && !ReferenceEquals(currentOwner, this))
            {
                currentOwner.RemoveProperty(property);
            }
            property.EntityType = this;

            return property;
        }

        private void UpdateIndexes(Property addedOrRemovedProperty, int startingIndex)
        {
            for (var i = startingIndex; i < _properties.Count; i++)
            {
                _properties[i].Index = i;
            }
            UpdateShadowIndexes(addedOrRemovedProperty);
        }

        private void UpdateShadowIndexes(Property addedOrRemovedProperty)
        {
            if (!addedOrRemovedProperty.HasClrProperty)
            {
                var shadowIndex = 0;
                foreach (var property in _properties.Where(p => !p.HasClrProperty))
                {
                    property.ShadowIndex = shadowIndex++;
                }
                _shadowPropertyCount = shadowIndex;
            }
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
            if (_keyProperties != null
                && _keyProperties.Contains(property))
            {
                var newList = _keyProperties.ToList();
                newList.Remove(property);
                _keyProperties = newList;
            }
        }

        [CanBeNull]
        public virtual Property TryGetProperty([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            // TODO: This should be O(n) but an additional index could be created
            // TODO: if this is too slow or if creating the surrogate Property object is too expensive
            var surrogate = new Property(name, typeof(object), false);
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

        public virtual bool HasClrType
        {
            get { return _type != null; }
        }

        public virtual IReadOnlyList<Property> Properties
        {
            get { return _properties; }
        }

        IReadOnlyList<IProperty> IEntityType.Key
        {
            get { return Key; }
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
