// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class EntityType : MetadataBase, IEntityType
    {
        private readonly MetadataName _name;
        private readonly Type _type;

        private readonly LazyRef<ImmutableDictionary<string, IProperty>> _properties
            = new LazyRef<ImmutableDictionary<string, IProperty>>(() => ImmutableDictionary<string, IProperty>.Empty);

        private readonly LazyRef<ImmutableList<IProperty>> _keyProperties
            = new LazyRef<ImmutableList<IProperty>>(() => ImmutableList<IProperty>.Empty);

        /// <summary>
        /// Creates a new metadata object representing an entity type associated with the given .NET type.
        /// </summary>
        /// <param name="type">The .NET entity type that this metadata object represents.</param>
        public EntityType([NotNull] Type type)
        {
            Check.NotNull(type, "type");

            _name = new MetadataName(type.Name);
            _type = type;
        }

        /// <summary>
        /// Creates a new metadata object representing an entity type that will participate in shadow-state
        /// such that there is no underlying .NET type corresponding to this metadata object.
        /// </summary>
        /// <param name="name">The name of the shadow-state entity type.</param>
        public EntityType([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            _name = new MetadataName(name);
        }

        public virtual string Name
        {
            get { return _name.Name; }
        }

        public virtual string StorageName
        {
            get { return _name.StorageName; }
            [param: NotNull] set
            {
                Check.NotEmpty(value, "value");

                _name.StorageName = value;
            }
        }

        public virtual Type Type
        {
            get { return _type; }
        }

        public virtual IEnumerable<IProperty> Key
        {
            get
            {
                return _keyProperties.HasValue
                    ? _keyProperties.Value
                    : Enumerable.Empty<IProperty>();
            }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _keyProperties.ExchangeValue(l => l.Clear().AddRange(value));
                _properties.ExchangeValue(ps => ps.SetItems(value.ToDictionary(p => p.Name)));
            }
        }

        public virtual void AddProperty([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            _properties.ExchangeValue(l => l.Add(property.Name, property));
        }

        public virtual void RemoveProperty([NotNull] IProperty property)
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

        public virtual IProperty Property(string name)
        {
            Check.NotEmpty(name, "name");

            IProperty property;
            return _properties.HasValue
                   && _properties.Value.TryGetValue(name, out property)
                ? property
                : null;
        }

        public virtual IEnumerable<IProperty> Properties
        {
            get
            {
                return _properties.HasValue
                    ? _properties.Value.Values.OrderByOrdinal(e => e.Name)
                    : Enumerable.Empty<IProperty>();
            }
        }
    }
}
