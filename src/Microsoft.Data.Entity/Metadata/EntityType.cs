// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class EntityType : MetadataBase, IEntityType
    {
        private readonly string _name;
        private string _storageName;
        private readonly Type _type;

        private readonly LazyRef<ImmutableDictionary<string, Property>> _properties
            = new LazyRef<ImmutableDictionary<string, Property>>(() => ImmutableDictionary<string, Property>.Empty);

        private readonly LazyRef<ImmutableList<Property>> _keyProperties
            = new LazyRef<ImmutableList<Property>>(() => ImmutableList<Property>.Empty);

        private readonly LazyRef<EntityKeyFactory> _keyFactory;

        /// <summary>
        ///     Creates a new metadata object representing an entity type associated with the given .NET type.
        /// </summary>
        /// <param name="type">The .NET entity type that this metadata object represents.</param>
        public EntityType([NotNull] Type type)
        {
            Check.NotNull(type, "type");

            _name = type.Name;
            _type = type;
            _keyFactory = new LazyRef<EntityKeyFactory>(() => new EntityKeyFactoryFactory().Create(this));
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
            get
            {
                return _keyProperties.HasValue
                    ? _keyProperties.Value
                    : Enumerable.Empty<Property>();
            }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _keyProperties.ExchangeValue(l => l.Clear().AddRange(value));
                _properties.ExchangeValue(ps => ps.SetItems(value.ToDictionary(p => p.Name)));
            }
        }

        public virtual void AddProperty([NotNull] Property property)
        {
            Check.NotNull(property, "property");

            _properties.ExchangeValue(l => l.Add(property.Name, property));
        }

        public virtual void RemoveProperty([NotNull] Property property)
        {
            Check.NotNull(property, "property");

            if (_properties.HasValue)
            {
                _properties.ExchangeValue(l => l.Remove(property.Name));

                if (_keyProperties.HasValue)
                {
                    _keyProperties.ExchangeValue(l => l.Remove(property));
                }
            }
        }

        public virtual Property Property([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            Property property;
            return _properties.HasValue
                   && _properties.Value.TryGetValue(name, out property)
                ? property
                : null;
        }

        public virtual IEnumerable<Property> Properties
        {
            get
            {
                return _properties.HasValue
                    ? _properties.Value.Values.OrderByOrdinal(e => e.Name)
                    : Enumerable.Empty<Property>();
            }
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

        public EntityKey CreateKey(object entity)
        {
            Check.NotNull(entity, "entity");

            return _keyFactory.Value.Create(entity);
        }
    }
}
