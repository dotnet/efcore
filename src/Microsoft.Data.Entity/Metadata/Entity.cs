// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Entity : MetadataBase
    {
        private readonly Type _type;

        private readonly LazyRef<ImmutableDictionary<PropertyInfo, Property>> _properties
            = new LazyRef<ImmutableDictionary<PropertyInfo, Property>>(() => ImmutableDictionary<PropertyInfo, Property>.Empty);

        private readonly LazyRef<ImmutableList<Property>> _keyProperties
            = new LazyRef<ImmutableList<Property>>(() => ImmutableList<Property>.Empty);

        public Entity(Type type)
            : base(Check.NotNull(type, "type", t => t.Name))
        {
            _type = type;
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
            
            set
            {
                Check.NotNull(value, "value");

                _keyProperties.ExchangeValue(l => l.Clear().AddRange(value));
                _properties.ExchangeValue(ps => ps.SetItems(value.ToDictionary(p => p.PropertyInfo)));
            }
        }

        public virtual void AddProperty(Property property)
        {
            Check.NotNull(property, "property");

            _properties.ExchangeValue(l => l.Add(property.PropertyInfo, property));
        }

        public virtual void RemoveProperty(Property property)
        {
            Check.NotNull(property, "property");

            if (_properties.HasValue)
            {
                _properties.ExchangeValue(l => l.Remove(property.PropertyInfo));

                if (_keyProperties.HasValue)
                {
                    _keyProperties.ExchangeValue(l => l.Remove(property));
                }
            }
        }

        public virtual Property Property(PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, "propertyInfo");

            Property property;
            return _properties.HasValue
                   && _properties.Value.TryGetValue(propertyInfo, out property)
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
    }
}
