// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Redis.Utilities;

namespace Microsoft.Data.Entity.Redis.Query
{
    public class RedisQuery
    {
        private readonly IEntityType _entityType;
        private readonly List<IProperty> _selectedProperties = new List<IProperty>();

        public RedisQuery([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");
            _entityType = entityType;
        }

        public virtual IEntityType EntityType
        {
            get { return _entityType; }
        }

        public virtual List<IProperty> SelectedProperties
        {
            get { return _selectedProperties; }
        }

        public virtual void AddProperty([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            _selectedProperties.Add(property);
        }

        public virtual int GetProjectionIndex([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            // TODO: if this causes perf issues consider pre-calculating a Dictionary of IProperty to index
            return _selectedProperties.IndexOf(property);
        }
    }
}
