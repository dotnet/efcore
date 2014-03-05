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
        private readonly LazyRef<IDictionary<string, int>> _propertyIndexes;
        private readonly LazyRef<List<ForeignKey>> _foreignKeys = new LazyRef<List<ForeignKey>>(() => new List<ForeignKey>());
        private readonly LazyRef<List<Navigation>> _navigations = new LazyRef<List<Navigation>>(() => new List<Navigation>());

        private ImmutableSortedSet<Property> _properties = ImmutableSortedSet<Property>.Empty.WithComparer(new PropertyComparer());
        private IReadOnlyList<Property> _keyProperties;
        private string _storageName;
        public Func<object[], object> _activator;

        // Intended only for creation of test doubles
        internal EntityType()
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
            _propertyIndexes = new LazyRef<IDictionary<string, int>>(CreateIndexes);
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

            Property currentProperty;
            if (_properties.TryGetValue(property, out currentProperty))
            {
                if (!ReferenceEquals(currentProperty, property))
                {
                    _properties = _properties.Remove(currentProperty).Add(property);
                    _propertyIndexes.Reset(CreateIndexes);
                }
            }
            else
            {
                _properties = _properties.Add(property);
                _propertyIndexes.Reset(CreateIndexes);
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

        public virtual void RemoveProperty([NotNull] Property property)
        {
            Check.NotNull(property, "property");

            var updatedProperties = _properties.Remove(property);
            if (updatedProperties != _properties)
            {
                _properties = updatedProperties;
                _propertyIndexes.Reset(CreateIndexes);
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

        public virtual Property Property([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            int index;
            return _propertyIndexes.Value.TryGetValue(name, out index) ? _properties[index] : null;
        }

        public virtual int PropertyIndex(string name)
        {
            int index;
            return _propertyIndexes.Value.TryGetValue(name, out index) ? index : -1;
        }

        private Dictionary<string, int> CreateIndexes()
        {
            var propertyIndexes = new Dictionary<string, int>(_properties.Count, StringComparer.Ordinal);
            for (var i = 0; i < _properties.Count; i++)
            {
                propertyIndexes[_properties[i].Name] = i;
            }
            return propertyIndexes;
        }

        public virtual IReadOnlyList<Property> Properties
        {
            get { return _properties; }
        }

        IReadOnlyList<IProperty> IEntityType.Key
        {
            get { return Key; }
        }

        IProperty IEntityType.Property(string name)
        {
            return Property(name);
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
            public int Compare(Property x, Property y)
            {
                return StringComparer.Ordinal.Compare(x.Name, y.Name);
            }
        }
    }
}
