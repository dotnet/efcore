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

        private ImmutableSortedSet<Property> _properties = ImmutableSortedSet<Property>.Empty.WithComparer(new PropertyComparer());
        private IDictionary<string, int> _propertyIndexes;
        private IList<Property> _keyProperties;
        private IList<ForeignKey> _foreignKeys;
        private string _storageName;

        /// <summary>
        ///     Creates a new metadata object representing an entity type associated with the given .NET type.
        /// </summary>
        /// <param name="type">The .NET entity type that this metadata object represents.</param>
        public EntityType([NotNull] Type type)
        {
            Check.NotNull(type, "type");

            _name = type.Name;
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

        public virtual IEnumerable<Property> Key
        {
            get { return _keyProperties ?? Enumerable.Empty<Property>(); }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _keyProperties = value.ToList();
                foreach (var property in value)
                {
                    // TODO: Consider if this should be replace/throw/no-op when prop with this name exists
                    AddProperty(property);
                }
            }
        }

        public virtual void AddForeignKey([NotNull] ForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            if (_foreignKeys == null)
            {
                _foreignKeys = new List<ForeignKey>();
            }

            // TODO: Consider ordering of FKs
            _foreignKeys.Add(foreignKey);
            foreach (var property in foreignKey.Properties)
            {
                // TODO: Consider if this should be replace/throw/no-op when prop with this name exists
                AddProperty(property);
            }
        }

        public virtual IEnumerable<ForeignKey> ForeignKeys
        {
            get { return _foreignKeys ?? Enumerable.Empty<ForeignKey>(); }
        }

        public virtual void AddProperty([NotNull] Property property)
        {
            Check.NotNull(property, "property");

            // TODO: Consider if replace as opposed to throw/no-op is correct when prop with this name exists

            Property currentProperty;
            if (_properties.TryGetValue(property, out currentProperty))
            {
                if (!ReferenceEquals(currentProperty, property))
                {
                    _properties = _properties.Remove(currentProperty).Add(property);
                    _propertyIndexes = null;
                }
            }
            else
            {
                _properties = _properties.Add(property);
                _propertyIndexes = null;
            }
        }

        public virtual void RemoveProperty([NotNull] Property property)
        {
            Check.NotNull(property, "property");

            var updatedProperties = _properties.Remove(property);
            if (updatedProperties != _properties)
            {
                _properties = updatedProperties;
                _propertyIndexes = null;
            }

            // TODO: Consider if it is okay to take properties out of the key, which may not be empty
            // TODO: Consider what to do with FKs that contain this property
            if (_keyProperties != null && _keyProperties.Contains(property))
            {
                _keyProperties.Remove(property);
            }
        }

        public virtual Property Property([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            EnsureIndexesCreated();

            int index;
            return _propertyIndexes.TryGetValue(name, out index) ? _properties[index] : null;
        }

        public virtual int PropertyIndex(string name)
        {
            EnsureIndexesCreated();

            int index;
            return _propertyIndexes.TryGetValue(name, out index) ? index : -1;
        }

        private void EnsureIndexesCreated()
        {
            if (_propertyIndexes == null)
            {
                _propertyIndexes = new Dictionary<string, int>(_properties.Count, StringComparer.Ordinal);
                for (var i = 0; i < _properties.Count; i++)
                {
                    _propertyIndexes[_properties[i].Name] = i;
                }
            }
        }

        public virtual IEnumerable<Property> Properties
        {
            get { return _properties; }
        }

        IEnumerable<IProperty> IEntityType.Key
        {
            get { return Key; }
        }

        IProperty IEntityType.Property(string name)
        {
            return Property(name);
        }

        IEnumerable<IProperty> IEntityType.Properties
        {
            get { return Properties; }
        }

        IEnumerable<IForeignKey> IEntityType.ForeignKeys
        {
            get { return ForeignKeys; }
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
