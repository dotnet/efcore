// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Redis.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Redis.Query
{
    public class RedisQuery
    {
        private readonly IEntityType _entityType;
        private readonly List<IProperty> _selectedProperties = new List<IProperty>();
        private readonly Dictionary<IProperty, int> _propertyIndexes = new Dictionary<IProperty, int>();

        public RedisQuery([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");
            _entityType = entityType;
        }

        public IEntityType EntityType
        {
            get { return _entityType; }
        }

        public List<IProperty> SelectedProperties
        {
            get { return _selectedProperties; }
        }

        public void AddProperty([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");
            if (property.EntityType != _entityType)
            {
                throw new Exception("Attempting to add invalid property of name " + property.Name
                    + " from IEntityType " + property.EntityType.Type.FullName
                    + " to RedisQuery for IEntityType " + _entityType.Type.FullName);
            }

            _selectedProperties.Add(property);
            if (!_propertyIndexes.ContainsKey(property))
            {
                _propertyIndexes[property] = _selectedProperties.Count - 1;
            }
        }

        public IEnumerable<IValueReader> GetValueReaders([NotNull] RedisQueryContext redisQueryContext)
        {
            Check.NotNull(redisQueryContext, "redisQueryContext");
            return redisQueryContext.GetResultsFromRedis(this).Select(array => new ObjectArrayValueReader(array));
        }

        public virtual int GetProjectionIndex([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");
            return _propertyIndexes[property];
        }
    }
}
