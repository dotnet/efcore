// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class EntityQuery
    {
        private readonly SqlSelect _sqlSelect = new SqlSelect();
        private readonly Dictionary<IProperty, int> _propertyIndexes = new Dictionary<IProperty, int>();

        public EntityQuery([NotNull] string tableName)
        {
            Check.NotEmpty(tableName, "tableName");

            _sqlSelect.Table = tableName;
        }

        public virtual void SetTopN(int n)
        {
            _sqlSelect.TopN = n;
        }

        public virtual void AddToProjection([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            if (!_propertyIndexes.ContainsKey(property))
            {
                _propertyIndexes.Add(property, _sqlSelect.AddToSelectList(property.StorageName));
            }
        }

        public virtual void AddToOrderBy([NotNull] IProperty property, OrderingDirection orderingDirection)
        {
            Check.NotNull(property, "property");

            _sqlSelect.AddToOrderByList(property.StorageName, orderingDirection);
        }

        public virtual int GetProjectionIndex([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            return _propertyIndexes[property];
        }

        public override string ToString()
        {
            return _sqlSelect.ToString();
        }
    }
}
